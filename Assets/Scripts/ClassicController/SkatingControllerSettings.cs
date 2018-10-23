using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Controller
{
    [CreateAssetMenu(fileName = "SkatingSetting", menuName = "Game/Skating Settings")]
    public class SkatingControllerSettings : ScriptableObject
    {
        public float m_stopspeed = 1f;

        public float m_acceleration = 0.32f;
        public float m_airAccelerationFactor = 0.1f; // proportional to normal acceleration
        public float m_crouchingAccelerationFactor = 0.5f; // proportional to normal acceleration
        public float m_jumpImpulse = 5f;

        public float m_maxSpeed = 0.03f;
        public float m_turningSpeed = 0.1f;
        public float m_airFriction = 0.01f;
        public float m_gravity = 0.09f;
        [Range(0.01f, 1f)] public float SlopeLimit = 0.8f;               

        [Range(0.01f, 1f)] public float m_planesDotAngleThreshold = 0.7f;   // threshold for changing plane alignment
        public float m_rotateSpeed = 8f;         // speed to change rotations
        public float m_groundDistance = 0.2f;   // check if controller is touching ground
        public float m_wallBounce = 2f;

        [Header("Rigidbody Properties")]
        public float m_mass = 8f;
        public float m_drag = 0.1f; // Drag on Rigidbody
        public float m_crouchingDrag = 0.01f; // Drag on Rigidbody

    }
}
