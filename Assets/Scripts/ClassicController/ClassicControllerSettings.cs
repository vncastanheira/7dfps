using System;
using UnityEngine;

namespace Assets.Controller
{
    [CreateAssetMenu(fileName = "Character CEM", menuName = "Game/Character Settings")]
    public class ClassicControllerSettings : ScriptableObject {

        //public float CPMScale { get { return cpmScale; } }

        //[SerializeField, Range(0.01f, 3f)]
        //private float cpmScale = 1.0f;

        [Space]
        [Header("Gravity")]
        [SerializeField] private float gravity = 4;
        [SerializeField] private float jumpVelocity = 2f;
        [SerializeField] private float waterGravityMultiplier = 0.4f;       // multiply by gravity when underwater
        [SerializeField, System.Obsolete] private float groundMaxDistance = 2f;
        [Tooltip("Ledge forgiveness")]
        public int JumpGraceTime = 5;   // let the character jump even after disconnecting from the group

        // Shortcut Properties
        public float Gravity { get { return gravity; } }
        public float JumpVelocity { get { return jumpVelocity; } }
        public float WaterGravityMultiplier { get { return waterGravityMultiplier; } }
        public float GroundMaxDistance { get { return groundMaxDistance; } }

        [Header("Speed")]
        [Tooltip("Only for ground")]
        public float MaxVelocity = 0.375f;          // Max ground velocity
        public float MaxSprintVelocity = 0.375f;    // Max ground sprint velocity

        public float MaxAirVelocity = 0.375f;           // Max airborne velocity
        public float MaxAirSprintVelocity = 0.375f;     // Max airbone velocity after jumping + sprint
        public float MaxWaterVelocity = 0.2f;           // Max velocity underwater
        public float StopSpeed = 1.0f;                
        public float VerticalVelocityLimit = 2f;        // Max velocity on the Y axis (negative and positive)
        public float RotateVelocity = 2f;

        [Header("Acceleration")]
        public float Accelerate = 10f;
        public float AccelerationFactor = 0.045f;
        public float AirAccelerate = 7f;
        public float WaterAccelerate = 6f;
        public bool AirboneControl = false;         // let the character alter the velocity when mid air

        [Header("Friction")]
        public float GroundFriction = 5f;
        public float WaterFriction = 6f;
        public float FlyFriction = 4f;
        [Range(0, 1)] public float WallFrictionTolerance = 0f;

        [Header("Character")]
        public bool FlyingCharacter = false;        // flying characters are not affected by gravity
        public bool CanWalljump = false;            // let the character perform a jump when near a SOLID
        [Range(0.01f, 1f)] public float SlopeLimit = 0.8f;              // tolerance dot product before sliding
        public float StepOffset = 5f;               // maximum step height
        public float SkinWidth = 0.08f;             
        public float MinMoveDistance = 0.001f;
        public Vector3 Center;
        public float Radius = 4.5f;
        public float Height = 14f;
        public float Depenetration = 0.001f; // depenetration magic number for ComputePenetration
    }
}

[Flags]
public enum CC_State
{
    None = 0,
    IsGrounded = 2,
    OnPlatform = 4
}

[Flags]
public enum CC_Collision
{
    None = 0,
    CollisionAbove = 2,
    CollisionBelow = 4,
    CollisionSides = 8,
}

public enum CC_Liquid
{
    Water,
    Hazard
}