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
        public float m_maxSpeed = 0.03f;
        public float m_turningSpeed = 0.1f;
        public float m_friction = 0.2f;
        public float m_gravity = 0.09f;
        [Range(0.01f, 1f)] public float SlopeLimit = 0.8f;              // tolerance dot product before sliding

        public float m_wallBounce = 2f; 
    }
}
