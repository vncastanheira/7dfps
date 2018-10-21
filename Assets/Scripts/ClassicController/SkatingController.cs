#define CLASSIC

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using vnc.Utilities;

namespace Assets.Controller
{
    public class SkatingController : MonoBehaviour
    {
        [Header("Settings")]
        public SkatingControllerSettings m_Settings;
        [Space]
        public CapsuleCollider ownCollider;
        public LayerMask m_SolidLayer;
        public LayerMask m_EnemyLayer;

        // velocity
        Vector3 Velocity;
        Vector3 FacingDirection;
        float FowardForce;
        Vector3 wishDir;


        // etc
        private float FowardInput;
        [HideInInspector] public bool IsAlive = true;
        [HideInInspector] public CC_Collision Collisions;
        [HideInInspector] public CC_State State;
        private Collider[] overlapingColliders = new Collider[16];
        private const float Depenetration = 0.001f; // depenetration magic number for ComputePenetration

        void Start()
        {
            FacingDirection = transform.forward;
            FowardForce = 0f;
            wishDir = Vector3.zero;
        }

        void Update()
        {
            FacingDirection = transform.forward;

            // get input
            FowardInput = (Input.GetButton("Forward") ? 1 : 0) - (Input.GetButton("Backwards") ? 1 : 0);
            float yaw = (Input.GetButton("Right") ? 1 : 0) - (Input.GetButton("Left") ? 1 : 0);
            wishDir = FowardInput * transform.TransformDirection(Vector3.forward);
            wishDir = Quaternion.Euler(0, 90 * yaw, 0) * wishDir;  
        }

        private void FixedUpdate()
        {
            Velocity = MoveGround(wishDir, Velocity);
            // rotate to face velocity direction
            transform.rotation = Quaternion.LookRotation(FacingDirection, transform.up);

            CalculateGravity();
#if CLASSIC
            CharacterMove(Velocity);
#else
            CharacterMove((transform.forward * FowardForce) + Vector3.down * GravityMag);
#endif
        }

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
                    float curMagnitude = Mathf.Min(stepDistance, distance - curDist);
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

        private Vector3 MoveGround(Vector3 wishdir, Vector3 prevVelocity)
        {
#if CLASSIC

            prevVelocity = Friction(prevVelocity, m_Settings.m_friction);
            Turning(FowardInput);
            prevVelocity = Accelerate(wishdir, prevVelocity, m_Settings.m_acceleration, m_Settings.m_maxSpeed);
            return prevVelocity;
#else

            Friction(m_Settings.m_friction);
            Turning(fwd);
            Accelerate(m_Settings.m_acceleration, fwd);
#endif
        }

        float projVel;
        private Vector3 Accelerate(Vector3 wishdir, Vector3 prevVelocity, float accelerate, float max_velocity)
        {
            projVel = Vector3.Dot(prevVelocity, wishdir);
            float accelSpeed = accelerate * 0.05f;

            if (projVel + accelSpeed > max_velocity)
                accelSpeed = max_velocity - projVel;

            Vector3 newVel = prevVelocity + wishdir * accelSpeed;
            return newVel;
        }

        private Vector3 Friction(Vector3 prevVelocity, float friction)
        {
            var wishspeed = prevVelocity;

            float speed = wishspeed.magnitude;
            // Apply Friction
            if (speed != 0) // To avoid divide by zero errors
            {
                float control = speed < m_Settings.m_stopspeed ? m_Settings.m_stopspeed : speed;
                // Quake 3 code I guess
                float drop = control * friction * Time.fixedDeltaTime;

                // hack to make the speed jump from a smaller value
                //speed = speed < min_speed ? min_speed : speed;

                wishspeed *= Mathf.Max(speed - drop, 0) / speed; // Scale the velocity based on friction.
            }
            return wishspeed;
        }

        //void Accelerate(float accelerate, float foward)
        //{
        //    projVel = Vector3.Dot(FacingDirection, wishDir);
        //    FowardForce = Mathf.Clamp(FowardForce + accelerate * projVel, 0f, m_Settings.m_maxSpeed);
        //}

        //void Friction(float friction)
        //{
        //    float speed = FowardForce;
        //    if (speed != 0)
        //    {
        //        float control = speed < m_Settings.m_stopspeed ? m_Settings.m_stopspeed : speed;
        //        // Quake 3 code I guess
        //        float drop = control * friction * Time.fixedDeltaTime;
        //        FowardForce *= Mathf.Max(speed - drop, 0) / speed;
        //    }
        //}

        void Turning(float foward)
        {
            FacingDirection += wishDir * m_Settings.m_turningSpeed * Mathf.Abs(foward);
            FacingDirection.Normalize();
        }

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

        private void OnCCHit(Vector3 normal)
        {
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

#if UNITY_EDITOR
        #region Editor
        private void OnDrawGizmos()
        {
            Vector3 xzPlane = FacingDirection;
            xzPlane.y = 0;

            if (xzPlane != Vector3.zero)
            {
                Handles.color = Color.white;
                Handles.ArrowHandleCap(0, transform.position, Quaternion.LookRotation(xzPlane), 2, EventType.Repaint);
            }

            if (wishDir != Vector3.zero)
            {
                Handles.color = Color.yellow;
                Handles.ArrowHandleCap(0, transform.position, Quaternion.LookRotation(wishDir), 2, EventType.Repaint);
            }
        }

        private void OnGUI()
        {
            Rect rect = new Rect(0, 0, 300, 300);
            string gui = string.Format("Facing Direction: {0}\n", FacingDirection)
                + string.Format("Foward Force: {0}\n", FowardForce)
                 + string.Format("Velocity: {0}\n", Velocity)
                 +string.Format("Is Grounded: {0}\n", (Collisions & CC_Collision.CollisionBelow))
                + string.Format("WishDir: {0}\n", wishDir)
                + string.Format("projVel: {0}\n", projVel);


            GUI.Label(rect, gui);
        }
        #endregion
#endif
    }
}
