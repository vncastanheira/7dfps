//#define DEFAULT_FEET
#define STAIRS

#if UNITY_EDITOR
using UnityEditor;
#endif

using System;
using UnityEngine;
using UnityEngine.Events;
using vnc.Utilities;

namespace Assets.Controller
{
    // CLASSIC CONTROLLER - BASE
    public partial class ClassicController : MonoBehaviour
    {
        [Header("Settings")]
        public ClassicControllerSettings CPMSettings;
        public CapsuleCollider ownCollider;

        public LayerMask m_SolidLayer;
        public LayerMask m_EnemyLayer;

        private Collider[] overlapingColliders = new Collider[16];

        [Header("Jump")]
        public int jumpInputWindow = 2;

        public float WalkForward { get; set; }
        public float Strafe { get; set; }
        public float Swim { get; set; }
        public bool JumpInput { get; set; }
        public bool Sprint { get; set; }
        public float YawRot { get; set; }

        private bool isAlive = true;
        public bool IsAlive
        {
            get { return isAlive; }
            set
            {
                if (!value)
                    gameObject.layer = LayerMask.NameToLayer("Dead");

                isAlive = value;
            }
        }

        public float pm_stopspeed { get { return CPMSettings.StopSpeed; } }
        public float pm_accelerate { get { return CPMSettings.Accelerate; } }
        public float pm_airaccelerate { get { return CPMSettings.AirAccelerate; } }
        public float pm_wateraccelerate { get { return CPMSettings.WaterAccelerate; } }
        public float pm_groundfriction { get { return CPMSettings.GroundFriction; } }
        public float pm_waterfriction { get { return CPMSettings.WaterFriction; } }

        // STATES
        private CC_State State;
        public bool IsGrounded { get { return (State & CC_State.IsGrounded) != 0; } }
        public bool OnPlatform { get { return (State & CC_State.OnPlatform) != 0; } }
        public bool IsSwimming { get; private set; }
        /// <summary> Helps camera smoothing on step </summary>
        public float StepDelta { get; private set; }

        [Header("Events"), Space]
        public UnityEvent OnJump;
        public UnityEvent OnLanding;

        //private Collider p_Collider;
        //private PlayerHealth p_Health;

        // VELOCITY, JUMP, COLLISIONS
        public Vector3 VelocityNormal { get; private set; }
        public float VelocityMagnitude { get; private set; }
        //private Vector3 VelocityFinal { get { return VelocityNormal * VelocityMagnitude} };
        private Vector3 Velocity;

        [HideInInspector] public Vector2 inputDir;
        private Vector3 wishdir;
        private float wishspeed;
        private float jumpGraceTimer;
        private bool sprintJump;    // jumped while sprinting
        [HideInInspector] public CC_Collision Collisions;
        [HideInInspector] public CC_Liquid OnLiquid;

        private int triedJumping = 0;
        private bool wasGrounded = false;
        private LayerMask overlapMask;

        private float _waterSurfacePosY;

        // Platforms
        private Collider platformCollider;
        private bool wasOnPlatform;

        void Awake()
        {
            State = CC_State.None;
            jumpGraceTimer = CPMSettings.JumpGraceTime;

            VelocityNormal = transform.forward;
            VelocityMagnitude = 0f;
        }

        void FixedUpdate()
        {
            if (Application.isPlaying)
            {
                // normal loop
                if (CPMSettings.FlyingCharacter)
                {
                    FlyMovementUpdate();
                }
                else
                {
                    GroundMovementUpdate();
                }
            }
        }

        #region General Movement

        void GroundMovementUpdate()
        {
            //check if is on ground
            //State = p_Controller.isGrounded ? State | CCState.IsGrounded : State & ~CCState.IsGrounded;
            State = (Collisions & CC_Collision.CollisionBelow) != 0 ? State | CC_State.IsGrounded : State & ~CC_State.IsGrounded;
            if (!IsGrounded)
                State &= ~CC_State.OnPlatform;  // remove platform if on air

            jumpGraceTimer = Mathf.Clamp(jumpGraceTimer - 1, 0, CPMSettings.JumpGraceTime);

            // check if above a platform
            //if (PhysicsExtensions.CapsuleCast(ownCollider, -transform.up, out platformHit, ownCollider.height, PlatformLayer))
            //    State |= CC_State.OnPlatform;
            //else
            //    State &= ~CC_State.OnPlatform;

            //OnPlatform = Physics.Raycast(transform.position, -transform.up, out platformHit, p_Controller.height, PlatformLayer);

            // player moved the character
            var walk = inputDir.y * transform.TransformDirection(Vector3.forward);
            var strafe = inputDir.x * transform.TransformDirection(Vector3.right);
            wishdir = (walk + strafe);

            float step = YawRot * CPMSettings.RotateVelocity * Time.deltaTime;
            wishdir = Quaternion.Euler(0, 10 * YawRot, 0) * wishdir;

            wishdir.Normalize();

            // only apply friction on groud for the last frame it was grounded
            if (wasGrounded)
            {
                Velocity = MoveGround(wishdir, Velocity);
            }
            else
            {
                // apply air movement
                Velocity = MoveAir(wishdir, Velocity);
            }

            // jumping window for bunnyhopping
            if (triedJumping > 0)
            {
                // normal jump, it's on the ground
                if (IsGrounded || jumpGraceTimer > 0)
                {
                    //p_velocity += Vector3.up * CPMSettings.JumpVelocity;
                    Velocity.y = CPMSettings.JumpVelocity;
                    triedJumping = 0;
                    jumpGraceTimer = 0;
                    sprintJump = Sprint;
                    OnJump.Invoke();
                }
            }


            CalculateGravity();

            CharacterMove(Velocity);
            
            // player just got off ground
            if (wasGrounded && !IsGrounded)
            {
                // falling
                if (Velocity.normalized.y < 0)
                    jumpGraceTimer = CPMSettings.JumpGraceTime;
            }

            triedJumping = Mathf.Clamp(triedJumping - 1, 0, 100);

            // notify when player reaches the gorund
            if (!wasGrounded && IsGrounded)
            {
                //if (OnPlatform)
                //{
                //    p_velocity = Friction(p_velocity, 20);
                //}

                Vector3 friction = Friction(Velocity, 20);
                Velocity.x = friction.x;
                Velocity.z = friction.z;

                jumpGraceTimer = 0;
                sprintJump = false;
                OnLanding.Invoke();
            }

            wasGrounded = IsGrounded;
            wasOnPlatform = OnPlatform;
        }

        void FlyMovementUpdate()
        {
            State &= ~CC_State.IsGrounded; // never on ground

            // player moved the character
            var walk = inputDir.y * m_View.transform.forward;
            var strafe = inputDir.x * transform.TransformDirection(Vector3.right);
            wishdir = (walk + strafe) + (Vector3.up * Swim);
            wishdir.Normalize();

            wishspeed = wishdir.magnitude;

            // fall when dead
            Velocity = MoveFly(wishdir, Velocity);

            //LastCollision = p_Controller.Move(p_velocity);
            CharacterMove(Velocity);
        }

        #region Move
        // move with ground friction
        private Vector3 MoveGround(Vector3 wishdir, Vector3 prevVelocity)
        {
            prevVelocity = Friction(prevVelocity, pm_groundfriction); // ground friction
            float maxVelocity = Sprint ? CPMSettings.MaxSprintVelocity : CPMSettings.MaxVelocity;
            prevVelocity = Accelerate(wishdir, prevVelocity, pm_accelerate, maxVelocity);
            return prevVelocity;
        }

        // move with air friction
        private Vector3 MoveAir(Vector3 accelDir, Vector3 prevVelocity)
        {
            float maxVelocity = sprintJump ? CPMSettings.MaxAirSprintVelocity : CPMSettings.MaxAirVelocity;
            prevVelocity = AccelerateAir(accelDir, prevVelocity, pm_airaccelerate, maxVelocity);
            return prevVelocity;
        }

        // move with water friction
        private Vector3 MoveWater(Vector3 accelDir, Vector3 prevVelocity)
        {
            prevVelocity = WaterFriction(prevVelocity);
            return Accelerate(accelDir, prevVelocity, pm_wateraccelerate, CPMSettings.MaxWaterVelocity);
        }

        private Vector3 MoveFly(Vector3 accelDir, Vector3 prevVelocity)
        {
            prevVelocity = Friction(prevVelocity, CPMSettings.FlyFriction); // ground friction
            prevVelocity = AccelerateAir(accelDir, prevVelocity, pm_airaccelerate, CPMSettings.MaxAirVelocity);
            return prevVelocity;
        }

        /// <summary>
        /// Push the character down the Y axis based on gravity value on settings
        /// </summary>
        /// <param name="gravityMultiplier">
        /// Multiplier used when dealing with different environments, like water. 
        /// </param>
        private void CalculateGravity(float gravityMultiplier = 1f)
        {
            // calculate gravity but on water
            Velocity += (Vector3.down * CPMSettings.Gravity * gravityMultiplier) * Time.fixedDeltaTime;

            // limit the Y velocity so the player doesn't speed
            // too much when falling or being propelled
            Velocity.y = Mathf.Clamp(Velocity.y, -CPMSettings.VerticalVelocityLimit, CPMSettings.VerticalVelocityLimit);
        }

        #endregion

        #region Acceleration
        float projVel;

        // acceleration
        private Vector3 Accelerate(Vector3 wishdir, Vector3 prevVelocity, float accelerate, float max_velocity)
        {
            projVel = Vector3.Dot(prevVelocity, wishdir);
            float accelSpeed = accelerate * CPMSettings.AccelerationFactor;

            if (projVel + accelSpeed > max_velocity)
                accelSpeed = max_velocity - projVel;

            Vector3 newVel = prevVelocity + wishdir * accelSpeed;
            return newVel;
        }

        private Vector3 AccelerateAir(Vector3 accelDir, Vector3 prevVelocity, float accelerate, float max_velocity)
        {
            projVel = Vector3.Dot(prevVelocity, accelDir);
            if (CPMSettings.AirboneControl)
            {
                float accelVel = accelerate * CPMSettings.AccelerationFactor;

                if (projVel + accelVel > max_velocity)
                    accelVel = max_velocity - projVel;

                Vector3 newVel = prevVelocity + accelDir * accelVel;
                return newVel;
            }
            else if (projVel < 0)
            {
                float accelVel = accelerate;

                if (projVel + accelVel > max_velocity)
                    accelVel = max_velocity - projVel;

                Vector3 newVel = prevVelocity + accelDir * accelVel;
                return newVel;
            }
            return prevVelocity;
        }

        #endregion

        #region Friction

        private Vector3 Friction(Vector3 prevVelocity, float friction)
        {
            var wishspeed = prevVelocity;

            float speed = wishspeed.magnitude;
            // Apply Friction
            if (speed != 0) // To avoid divide by zero errors
            {
                float control = speed < pm_stopspeed ? pm_stopspeed : speed;
                // Quake 3 code I guess
                float drop = control * friction * Time.fixedDeltaTime;

                // hack to make the speed jump from a smaller value
                //speed = speed < min_speed ? min_speed : speed;

                wishspeed *= Mathf.Max(speed - drop, 0) / speed; // Scale the velocity based on friction.
            }
            return wishspeed;
        }

        private Vector3 WaterFriction(Vector3 prevVelocity)
        {
            var wishspeed = prevVelocity;

            float speed = wishspeed.magnitude;
            // Apply Friction
            if (speed != 0) // To avoid divide by zero errors
            {
                // Quake 3 code I guess
                float drop = speed * pm_waterfriction * Time.fixedDeltaTime;

                wishspeed *= Mathf.Max(speed - drop, 0) / speed; // Scale the velocity based on friction.
            }
            return wishspeed;
        }

        //private void WallFriction(Vector3 normal)
        //{
        //    Vector3 movement = p_velocity;
        //    movement.y = 0;
        //    var d = Vector3.Dot(normal, transform.forward);

        //    if (d > -CPMSettings.WallFrictionTolerance)
        //        return;

        //    var i = Vector3.Dot(normal, movement);
        //    var into = (normal * i);
        //    var side = movement - into;

        //    p_velocity.x = side.x * (1 + d);
        //    p_velocity.z = side.z * (1 + d);
        //}

        #endregion

        #region Physics
        RaycastHit belowHit;

        /// <summary>
        /// Move the character without going through things
        /// </summary>
        /// <param name="movement">offset to move the character</param>
        void CharacterMove(Vector3 movement)
        {
            Vector3 nTotal = Vector3.zero;

            // reset all collision flags
            Collisions = CC_Collision.None;

            Vector3 movNormalized = movement.normalized;
            float distance = movement.magnitude;

#if STAIRS
            StepDelta = Mathf.Clamp(StepDelta - Time.fixedDeltaTime * 1.5f, 0, Mathf.Infinity);
            if (IsGrounded && !CPMSettings.FlyingCharacter)
                MoveOnSteps1(movNormalized);
#endif
            //const float minimumStepDistance = 0.1f; // Greater than 0 to prevent infinite loop
            //float stepDistance = Math.Min((ownCollider as CapsuleCollider).radius, minimumStepDistance);
            float stepDistance = 0.05f;

            Vector3 nResult;
            if (distance > 0)
            {
                for (float curDist = 0; curDist < distance; curDist += stepDistance)
                {
                    float curMagnitude = Math.Min(stepDistance, distance - curDist);
                    Vector3 start = transform.position;
                    Vector3 end = start + movNormalized * curMagnitude;
                    transform.position = FixOverlaps(end, movNormalized * curMagnitude, out nResult);
                    nTotal += nResult;
                }
            }
            else
            {
                // when character doesn't move
                transform.position = FixOverlaps(transform.position, Vector3.zero, out nResult);
                nTotal += nResult;
            }
            
            // handles collision
            OnCCHit(nTotal.normalized);
        }

        /// <summary>
        /// Move the transform trying to stop being overlaping other colliders
        /// </summary>
        /// <param name="position">start position. Bottom of the collider</param>
        /// <returns>Final position</returns>
        Vector3 FixOverlaps(Vector3 position, Vector3 offset, out Vector3 nResult)
        {
            Vector3 nTemp = Vector3.zero;

            // what if you sum all the penetration directions to give the final result
            // and with each collision, add a CollisionFlag accordingly with the axis collided?

            CapsuleCollider coll = ownCollider as CapsuleCollider;
            //Vector3 p = position - (position - coll.transform.position) + (coll.bounds.center - new Vector3(0, coll.bounds.extents.y, 0));
            //int nColls = Physics.OverlapCapsuleNonAlloc(p, new Vector3(p.x, p.y + coll.height, p.z), coll.radius, overlapingColliders, GroundLayer, QueryTriggerInteraction.Ignore);

            // when dead, collide onlly with the world
            if (IsAlive) overlapMask = m_SolidLayer | m_EnemyLayer;
            else overlapMask = m_SolidLayer;

            int nColls = PhysicsExtensions.OverlapCapsuleNonAlloc(coll, offset, overlapingColliders, overlapMask, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < nColls; i++)
            {
                Collider c = overlapingColliders[i];
                if (c == ownCollider) continue; // ignore itself

                Vector3 normal;
                float dist;
                float dot;
                if (Physics.ComputePenetration(ownCollider, position, transform.rotation,
                    c, c.transform.position, c.transform.rotation, out normal, out dist))
                {
                    // TODO: report bug
                    if (float.IsNaN(normal.x) || float.IsNaN(normal.y) || float.IsNaN(normal.y))
                        continue;

                    // depenetration value for preventing doors 
                    // from overlapping with player when it shouldn't
                    dist += CPMSettings.Depenetration;

                    dot = Vector3.Dot(normal, Vector3.up);
                    if (dot > CPMSettings.SlopeLimit && dot <= 1)
                    {
                        Collisions = Collisions | CC_Collision.CollisionBelow;
                        position += Vector3.up * dist;

                        State &= ~CC_State.OnPlatform;
                    }
                    else
                    {
                        position += normal * dist;
                    }

                    if (dot >= 0 && dot < CPMSettings.SlopeLimit)
                    {
                        Collisions = Collisions | CC_Collision.CollisionSides;
                    }

                    if (dot < 0)
                    {
                        Collisions = Collisions | CC_Collision.CollisionAbove;
                    }

                    nTemp += normal;
                }
            }
            nResult = nTemp;
            return position;
        }

#if STAIRS
        RaycastHit stepHit;
        RaycastHit cornerHit;
        Vector3 stepDir;
        float stepDist;
        Vector3 stepCenter;
        void MoveOnSteps1(Vector3 movNormalized)
        {
            Vector3 p0, p1; // capsule point 0 and 1
            float radius;   // capsule radius

            // ignore gravity pull
            stepDir = movNormalized;
            stepDir.y = 0;
            stepDist = stepDir.magnitude; // for debug purposes


            ownCollider.ToWorldSpaceCapsule(out p0, out p1, out radius);
            p0 += (stepDir * stepDist) + (Vector3.up * CPMSettings.StepOffset);
            p1 += (stepDir * stepDist) + (Vector3.up * CPMSettings.StepOffset);

            if (stepDist <= Mathf.Epsilon)
                return;

            // check if collides in the next step
            if (Physics.CheckCapsule(p0, p1, radius, m_SolidLayer, QueryTriggerInteraction.Ignore))
            {
                // collided with a solid object, probably a wall
                return; // doesn't do anything
            }
            else
            {
                // didn't found a collision, so there is a step
                // try to find the step point
                stepCenter = ((p0 + p1) / 2) + stepDir * radius;
                Vector3 size = new Vector3(ownCollider.radius * 2, ownCollider.height, ownCollider.radius * 2);

                //if (Physics.Raycast(stepCenter, Vector3.down, out stepHit, Mathf.Infinity, Game.Settings.SOLID_LAYER, QueryTriggerInteraction.Ignore))
                //{
                //    var bottom = ownCollider.center + transform.position + (Vector3.down * ownCollider.height / 2);
                //    var dot = Vector3.Dot(stepHit.normal, Vector3.up);
                //    if (stepHit.point.y > bottom.y && dot > 0.95f)
                //    {
                //        float upDist = Mathf.Abs(stepHit.point.y - bottom.y);
                //        transform.position += Vector3.up * upDist; //raise the player on the step size
                //    }
                //}

                if (Physics.CapsuleCast(p0 + stepDir * radius, p1 + stepDir * radius, radius, Vector3.down, out stepHit, Mathf.Infinity, m_SolidLayer, QueryTriggerInteraction.Ignore))
                {
                    var bottom = ownCollider.center + transform.position + (Vector3.down * ownCollider.height / 2);

                    if (Physics.Raycast(stepHit.point + Vector3.up * CPMSettings.StepOffset,
                        Vector3.down, out cornerHit, Mathf.Infinity, m_SolidLayer, QueryTriggerInteraction.Ignore))
                    {
                        var dot = Vector3.Dot(cornerHit.normal, Vector3.up);
                        if (stepHit.point.y > bottom.y && dot >= 0.98999999f)
                        {
                            float upDist = Mathf.Abs(stepHit.point.y - bottom.y);
                            transform.position += Vector3.up * upDist; //raise the player on the step size

                            if (upDist > StepDelta)
                                StepDelta = upDist;

                            Collisions |= CC_Collision.CollisionBelow;
                        }
                    }

                }
            }
        }

        //void MoveOnSteps2(Vector3 movNormalized)
        //{
        //    Vector3 p0, p1; // capsule point 0 and 1
        //    float radius;   // capsule radius
        //    Vector3 playerPosition;

        //    // ignore gravity pull
        //    stepDir = movNormalized;
        //    stepDir.y = 0;
        //    stepDist = stepDir.magnitude;
        //    playerPosition = transform.position;

        //    const float minimumStepDistance = 0.1f; // Greater than 0 to prevent infinite loop
        //    float stepDistance = Math.Min((ownCollider as CapsuleCollider).radius, minimumStepDistance);

        //    float pushUp = 0;
        //    for (float curDist = 0; curDist < CPMSettings.StepOffset; curDist += stepDistance)
        //    {
        //        ownCollider.ToWorldSpaceCapsule(out p0, out p1, out radius);
        //        p0 += (stepDir * stepDist) + Vector3.up * curDist;
        //        p1 += (stepDir * stepDist) + Vector3.up * curDist;
        //        if (Physics.CheckCapsule(p0, p1, radius, Game.Settings.SOLID_LAYER, QueryTriggerInteraction.Ignore))
        //            continue;
        //        else
        //        {
        //            pushUp = curDist;
        //            break;
        //        }

        //    }
        //    transform.position += Vector3.up * pushUp;
        //}
#endif

        /// <summary>
        /// Push this character
        /// </summary>
        /// <param name="force">Force push</param>
        public void PushThis(Vector3 direction, float force)
        {
            Velocity += direction * force * Time.deltaTime;
            //p_velocity = AccelerateAir(direction, p_velocity, force, CPMSettings.MaxAirVelocity);
        }

        private void OnCCHit(Vector3 normal)
        {
            //if ((Collisions & CC_Collision.CollisionBelow) != 0)
            //{
            //    groundHit = normal;
            //}

            if ((Collisions & CC_Collision.CollisionAbove) != 0 && Velocity.y > 0)
            {
                Velocity.y = 0;
            }

            if ((Collisions & CC_Collision.CollisionSides) != 0)
            {
                //WallFriction(normal);
                var copyVelocity = Velocity;
                copyVelocity.y = 0;
                var newDir = Vector3.ProjectOnPlane(copyVelocity.normalized, normal);
                Velocity.x = (newDir * copyVelocity.magnitude).x;
                Velocity.z = (newDir * copyVelocity.magnitude).z;
            }
        }

        #endregion Physics

        #endregion Movement

        public void SetInput(float fwd, float strafe, float yaw, float swim, bool jump)
        {
            WalkForward = fwd;
            Strafe = strafe;
            Swim = swim;
            JumpInput = jump;
            YawRot = yaw;

            if (JumpInput && triedJumping == 0)
                triedJumping = jumpInputWindow;

            inputDir = new Vector2(strafe, fwd);
        }


        //Vector2 GetKeyInput()
        //{
        //    WalkForward = (InputManager.GetButton("Forward") ? 1 : 0) - (InputManager.GetButton("Backwards") ? 1 : 0);
        //    Strafe = (InputManager.GetButton("Strafe_Right") ? 1 : 0) - (InputManager.GetButton("Strafe_Left") ? 1 : 0);

        //    SwimUp = InputManager.GetAxisRaw("SwimUp");
        //    JumpInput = InputManager.GetButtonDown("Jump");

        //    if (JumpInput && triedJumping == 0)
        //        triedJumping = jumpInputWindow;

        //    return new Vector2(Strafe, WalkForward);
        //}

        /// <summary> Check if the player view is under water layer </summary>
        private bool HasCollisionFlag(CC_Collision flag)
        {
            return (Collisions & flag) != 0;
        }

        //private void OnTriggerEnter(Collider other)
        //{
        //    if (other.gameObject.CompareLayer(WaterLayer))
        //    {
        //        IsSwimming = true;
        //        _waterSurfacePosY = other.transform.position.y;
        //    }
        //}

        //private void OnTriggerExit(Collider other)
        //{
        //    if (other.gameObject.CompareLayer(WaterLayer))
        //    {
        //        _waterSurfacePosY = other.transform.position.y;
        //        float fpsPosY = this.transform.position.y;
        //        if (fpsPosY > _waterSurfacePosY)
        //        {
        //            // ok we really left the water
        //            IsSwimming = false;
        //        }
        //    }
        //}

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Handles.color = Color.white;
            Vector3 xzPlane = Velocity;
            xzPlane.y = 0;

            if (xzPlane != Vector3.zero)
            {
                Handles.ArrowHandleCap(0, transform.position, Quaternion.LookRotation(xzPlane), 2, EventType.Repaint);
            }
        }

        private void OnGUI()
        {
            Rect rect = new Rect(0, 0, 300, 300);
            string gui = string.Format("Velocity: {0}\n", Velocity)
                + string.Format("WishDir: {0}\n", wishdir)
                + string.Format("YawRot: {0}", YawRot);


            GUI.Label(rect, gui);
        }
#endif

    }
}
