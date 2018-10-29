#define CLASSIC
#define RIGIDBODY

using Assets.Managers;
using UnityEditor;
using UnityEngine;
using vnc.Tools;
using vnc.Utilities;

namespace Assets.Controller
{
    public partial class SkatingController : MonoBehaviour,
        IVncEventListener<GameEvent>
    {
        [Header("Settings")]
#if RIGIDBODY
        public Rigidbody body;
#endif
        public SkatingControllerSettings m_Settings;
        [Space]
        public CapsuleCollider ownCollider;
        //public Transform m_arrow;
        public LayerMask m_SolidLayer;
        public LayerMask m_ButtonLayer;
        public TrailRenderer m_trail;

        // velocity
        Vector3 Velocity;
        Vector3 FacingDirection;
        Vector3 wishDir;

        // etc
        private float FowardInput;
        private float Yaw;
        private bool isCrouching;
        private bool JumpInput;
        private bool RecoverInput;  // adjust the player rotation when it falls sideway on the floor 
        [HideInInspector] public bool OnGround;
        [HideInInspector] public bool IsAlive = true;
        [HideInInspector] public CC_Collision Collisions;
        [HideInInspector] public CC_State State;
        private Collider[] overlapingColliders = new Collider[16];
        private const float Depenetration = 0.001f; // depenetration magic number for ComputePenetration

        //debug
        public GUISkin guiSkin;
        private Vector3 debugPlane;

        void Start()
        {
            FacingDirection = transform.forward;
            wishDir = Vector3.zero;
            m_MouseLook.Init(m_Head.transform, m_View.transform);

            this.Listen();
        }

        void Update()
        {
            if (IsAlive && !GameManager.Instance.Paused)
            {
                m_View.gameObject.SetActive(true);
                body.useGravity = true;

                UpdateView();
                UpdateGun();

                // get input
                FacingDirection = transform.forward;
                FowardInput = (Input.GetButton("Forward") ? 1 : 0) - (Input.GetButton("Backwards") ? 1 : 0);
                float strafe = (Input.GetButton("Right") ? 1 : 0) - (Input.GetButton("Left") ? 1 : 0);

                // body rotation
                float aimSpeed = aimAssist ? m_Settings.m_aimAssistSpeed : 1f;
                Yaw = Input.GetAxis("LookHorizontal") * m_MouseLook.m_HorizontalSpeed * aimSpeed;

                if (Input.GetButtonDown("Crouch"))
                    isCrouching = !isCrouching;

                if (Input.GetButtonDown("Jump") && OnGround)
                    JumpInput = true;

                RecoverInput = Input.GetButtonDown("Recover");

                wishDir = transform.TransformDirection(Vector3.forward) * FowardInput;
                wishDir += transform.TransformDirection(Vector3.right) * strafe;
                wishDir.Normalize();

                //if (FowardInput >= 0)
                //{
                //    wishDir = Quaternion.Euler(0, 90 * yaw, 0) * wishDir;
                //    //wishDir = Quaternion.Euler(0, m_Head.localEulerAngles.y, 0) * wishDir;
                //}

                //DisplayArrow();
            }
        }

        private void FixedUpdate()
        {
            if (IsAlive && !GameManager.Instance.Paused)
            {
                body.mass = m_Settings.m_mass;
                if (isCrouching)
                    body.drag = m_Settings.m_crouchingDrag; // less air resistance when crouching
                else
                    body.drag = m_Settings.m_drag;


#if RIGIDBODY
                //Turning();

                RaycastHit hit;
                OnGround = GetGround(out hit);
                if (OnGround)
                {

                    wishDir = Vector3.ProjectOnPlane(wishDir, hit.normal);
                    MoveGround();

                    // align the controller with the plane if the angle isn't too high
                    debugFloorDot = Vector3.Dot(transform.up, hit.normal);
                    if (debugFloorDot > m_Settings.m_planesDotAngleThreshold)
                        ControllerAlign(hit.normal);
                    else
                        ControllerAlign(Vector3.up);

                    if (JumpInput)
                    {
                        if (isCrouching && CanStand())
                            isCrouching = false;

                        body.AddForce(hit.normal * m_Settings.m_jumpImpulse, ForceMode.Impulse);
                        JumpInput = false;
                    }

                    debugPlane = hit.normal;
                }
                else // in mid air
                {
                    MoveAir();

                    ControllerAlign(Vector3.up);
                }

                Crouch();
                Recover();
            }

#else
            if (EnumExtensions.HasFlag(Collisions, CC_Collision.CollisionBelow))
            {
                Ray floorRay = new Ray(transform.position, Vector3.down * ownCollider.height);
                RaycastHit hit;
                if (Physics.Raycast(floorRay, out hit, Mathf.Infinity, m_SolidLayer))
                {
                    wishDir = Vector3.ProjectOnPlane(wishDir, hit.normal);
                }
                Velocity = MoveGround(wishDir, Velocity);
            }
            else
            {
                Velocity = MoveAir(wishDir, Velocity);
            }

            // rotate to face velocity direction
            transform.rotation = Quaternion.LookRotation(FacingDirection, transform.up);
            CalculateGravity();
#if CLASSIC
            CharacterMove(Velocity);
#else
            CharacterMove((transform.forward * FowardForce) + Vector3.down * GravityMag);   
#endif
#endif
        }

        bool GetGround(out RaycastHit hit)
        {
            Vector3 point0, point1, offset;
            float radius, distance;

            offset = transform.up;

            ownCollider.ToWorldSpaceCapsule(out point0, out point1, out radius);
            point0 += offset;
            point1 += offset;


            radius -= 0.1f;
            distance = ownCollider.height + m_Settings.m_groundDistance;

            bool onGround = Physics.CapsuleCast(point0, point1, radius, -transform.up, out hit, distance, m_SolidLayer);
            //if (Vector3.Dot(transform.up, hit.normal) < m_Settings.m_planesDotAngleThreshold)
            //    return false;

            return onGround;
        }

        float debugFloorDot;
        void ControllerAlign(Vector3 up)
        {
            Quaternion targetRot;
            // align the rotation
            //targetRot = Quaternion.LookRotation(transform.forward, up);
            targetRot = Quaternion.FromToRotation(transform.up, up) * transform.rotation;
            // smooth interpolation
            targetRot = Quaternion.Slerp(transform.rotation, targetRot, Time.fixedDeltaTime * m_Settings.m_rotateSpeed);

            targetRot *= Quaternion.Euler(0f, Yaw, 0f);

            //transform.rotation = targetRot;
            body.MoveRotation(targetRot);
        }

        void Turning()
        {
            Vector3 copy = m_Head.forward;
            copy.y = 0;
            FacingDirection += copy * m_Settings.m_turningSpeed;
            FacingDirection.Normalize();

            Quaternion targetRot = Quaternion.LookRotation(FacingDirection, transform.up);
            // rotate sideways
            transform.rotation = targetRot;
        }

        void MoveGround()
        {
            float factor = 1f;

            if (isCrouching)
                factor = m_Settings.m_crouchingAccelerationFactor;

            body.AddForce(wishDir * m_Settings.m_acceleration * factor, ForceMode.Acceleration);
        }

        void MoveAir()
        {
            if (isCrouching)
                return;

            body.AddForce(wishDir * m_Settings.m_acceleration * m_Settings.m_airAccelerationFactor, ForceMode.Acceleration);
        }

        /// <summary>
        /// Makes the controller crouch
        /// </summary>
        void Crouch()
        {
            // detect when the player is under something and don't let it stand
            if (!CanStand() && OnGround && isCrouching)
            {
                isCrouching = true;
            }

            // adjust the collider size

            float step = Time.deltaTime * 10;
            Vector3 cameraPos = m_View.transform.localPosition;
            if (isCrouching)
            {
                ownCollider.center = Vector3.Lerp(ownCollider.center, new Vector3(0f, -.5f, 0f), step);
                ownCollider.height = Mathf.Lerp(ownCollider.height, 1, step);

                cameraPos.y = -0.8f;
            }
            else
            {
                ownCollider.center = Vector3.Lerp(ownCollider.center, Vector3.zero, step);
                ownCollider.height = Mathf.Lerp(ownCollider.height, 2, step);

                cameraPos.y = 0f;
            }

            // ajdust the camera position
            m_View.transform.localPosition = Vector3.MoveTowards(m_View.transform.localPosition, cameraPos, step);
        }

        /// <summary>
        /// Check if there is nothing blocking the player while he is crouching
        /// </summary>
        /// <returns>If can stand</returns>
        bool CanStand()
        {
            RaycastHit hit;
            Vector3 point0, point1;
            float radius;
            ownCollider.ToWorldSpaceCapsule(out point0, out point1, out radius);
            radius -= 0.1f;
            bool isBlocking = Physics.CapsuleCast(point0, point1, radius, transform.up, out hit, ownCollider.height, m_SolidLayer);

            return !isBlocking;
        }

        void Recover()
        {
            if (RecoverInput)
            {
                ControllerAlign(Vector3.up);
                RecoverInput = false;
            }
        }

        //void DisplayArrow()
        //{
        //    var vel = body.velocity;
        //    vel.y = 0;
        //    if (vel != Vector3.zero)
        //    {
        //        m_arrow.rotation = Quaternion.LookRotation(vel);
        //        m_arrow.gameObject.SetActive(true);
        //    }
        //    else
        //    {
        //        m_arrow.gameObject.SetActive(false);
        //    }
        //}


        public void OnVncEvent(GameEvent e)
        {
            switch (e.Event)
            {
                case GameEventType.TrackStart:
                    IsAlive = true;
                    break;
                case GameEventType.Resume:
                    break;
                case GameEventType.TrackRestart:
                    isCrouching = false;
                    IsAlive = true;
                    m_trail.Clear();
                    break;
                case GameEventType.TrackEnd:
                    IsAlive = false;
                    m_View.gameObject.SetActive(false);
                    body.velocity = Vector3.zero;
                    body.useGravity = false;
                    break;
                case GameEventType.Pause:
                    break;


            }
        }

        private void OnDestroy()
        {
            this.Unlisten();
        }

#if !RIGIDBODY
        void CalculateGravity()
        {
#if CLASSIC
            Velocity += (Vector3.down * m_Settings.m_gravity) * Time.fixedDeltaTime;
#else

            if (EnumExtensions.HasFlag(Collisions, CC_Collision.CollisionBelow))
                GravityMag = m_Settings.m_gravity;
            else
            {
                GravityMag += m_Settings.m_gravity * Time.fixedDeltaTime;
            }
#endif
        }
#endif


#if !RIGIDBODY

        /// <summary>
        /// Move the character without going through things
        /// </summary>
        /// <param name="movement">offset to move the character</param>
        void CharacterMove(Vector3 movement)
        {
            Vector3 nTotal = Vector3.zero;

            // reset all collision flags
            Collisions = CC_Collision.None;

            Vector3 totalMovNorm = movement.normalized;
            float distance = movement.magnitude;

#if STAIRS
            StepDelta = Mathf.Clamp(StepDelta - Time.fixedDeltaTime * 1.5f, 0, Mathf.Infinity);
            if (IsGrounded && !CPMSettings.FlyingCharacter)
                MoveOnSteps1(movNormalized);
#endif
            //const float minimumStepDistance = 0.1f; // Greater than 0 to prevent infinite loop
            //float stepDistance = Math.Min((ownCollider as CapsuleCollider).radius, minimumStepDistance);
            float stepDistance = 0.05f;

            int nHits = 0;

            for (int i = 0; i < 3; i++)
            {
                Vector3 movNormalized = Vector3.zero;
                switch (i)
                {
                    case 0:
                        movNormalized = Vector3.right;
                        break;
                    case 1:
                        movNormalized = Vector3.up;
                        break;
                    case 2:
                        movNormalized = Vector3.forward;
                        break;
                }
                movNormalized *= totalMovNorm[i];

                Vector3 normal;
                if (distance > 0)
                {
                    for (float curDist = 0; curDist < distance; curDist += stepDistance)
                    {
                        float curMagnitude = Mathf.Min(stepDistance, distance - curDist);
                        Vector3 start = transform.position;
                        Vector3 end = start + movNormalized * curMagnitude;
                        transform.position = FixOverlaps(end, movNormalized * curMagnitude, out normal);

                        if (nHits < hitNormals.Length)
                        {
                            hitNormals[nHits] = normal;
                            nHits++;
                        }
                    }
                }
                else
                {
                    // when character doesn't move
                    transform.position = FixOverlaps(transform.position, Vector3.zero, out normal);

                    if (nHits < hitNormals.Length)
                    {
                        hitNormals[nHits] = normal;
                        nHits++;
                    }
                }


            }

            Vector3 resultNormal = Vector3.zero;
            for (int n = 0; n < nHits; n++)
            {
                // handles collision
                OnCCHit(hitNormals[n]);
                resultNormal += hitNormals[n];
            }

            // align with planes
            var copyVelocity = Velocity;
            var newDir = Vector3.ProjectOnPlane(copyVelocity.normalized, resultNormal.normalized);
            Velocity = (newDir * copyVelocity.magnitude);

            //FloorAlign();
        }


        Vector3[] hitNormals = new Vector3[16];
        private void OnCCHit(Vector3 normal)
        {
            if ((Collisions & CC_Collision.CollisionAbove) != 0 && Velocity.y > 0)
            {
                Velocity.y = 0;
            }

            if ((Collisions & CC_Collision.CollisionSides) != 0)
            {
                //var copyVelocity = Velocity;
                //copyVelocity.y = 0;
                //var newDir = Vector3.ProjectOnPlane(copyVelocity.normalized, normal);
                //Velocity.x = (newDir * copyVelocity.magnitude).x;
                //Velocity.z = (newDir * copyVelocity.magnitude).z;
            }
        }

        #region Physics
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
            LayerMask overlapMask;
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
                    dist += Depenetration;

                    dot = Vector3.Dot(normal, Vector3.up);
                    if (dot > m_Settings.SlopeLimit && dot <= 1)
                    {
                        Collisions = Collisions | CC_Collision.CollisionBelow;
                        position += normal * dist;

                        State &= ~CC_State.OnPlatform;
                    }

                    if (dot >= 0 && dot < m_Settings.SlopeLimit)
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

        #endregion

#endif

#if UNITY_EDITOR
        #region Editor
        private void OnDrawGizmos()
        {
            Vector3 xzPlane = FacingDirection;
            xzPlane.y = 0;

            Vector3 vel;
#if RIGIDBODY
            vel = body.velocity;
#else
            vel = Velocity;
#endif

            if (body.velocity != Vector3.zero)
            {
                Handles.color = Color.blue;
                Handles.ArrowHandleCap(0, transform.position, Quaternion.LookRotation(body.velocity), 2, EventType.Repaint);
            }

            if (wishDir != Vector3.zero)
            {
                Handles.color = Color.yellow;
                Handles.ArrowHandleCap(0, transform.position, Quaternion.LookRotation(wishDir), 2, EventType.Repaint);
            }

            if (debugPlane != Vector3.zero)
            {
                Handles.color = Color.red;
                Handles.ArrowHandleCap(0, transform.position, Quaternion.LookRotation(debugPlane), 2, EventType.Repaint);
            }
            DrawGizmosGun();
        }

        //private void OnGUI()
        //{
        //    Rect rect = new Rect(Screen.width - guiSkin.label.fixedWidth, 0,
        //        guiSkin.label.fixedWidth, guiSkin.label.fixedHeight);
        //    string gui = string.Format("Facing Direction: {0}\n", FacingDirection)
        //        + string.Format("Foward Force: {0}\n", FowardForce)
        //         + string.Format("Velocity: {0}\n", Velocity)
        //         + string.Format("Is Grounded: {0}\n", OnGround)
        //         + string.Format("Is Crouching: {0}\n", isCrouching)
        //        + string.Format("WishDir: {0}\n", wishDir)
        //        + string.Format("Time Scale: {0}\n", Time.timeScale)
        //        + string.Format("Is Alive: {0}\n", IsAlive);

        //    GUI.Label(rect, gui, guiSkin.label);
        //}

        #endregion
#endif
    }
}
