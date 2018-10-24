using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Assets.UI
{
    public class PlayerUI : MonoBehaviour
    {
        [Header("Settings")]
        public TextMeshProUGUI m_SpeedText;
        public Rigidbody m_body;
        
        void Update()
        {
            m_SpeedText.text = m_body.velocity.sqrMagnitude.ToString("0");
        }
    }
}

