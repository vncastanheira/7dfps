using Assets.Managers;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;

namespace Assets.UI
{
    public class PlayerUI : MonoBehaviour
    {
        [Header("Settings")]
        public TextMeshProUGUI m_SpeedText;
        public TextMeshProUGUI m_TimerText;
        public Rigidbody m_body;
        
        void Update()
        {
            m_SpeedText.text = m_body.velocity.sqrMagnitude.ToString("0");
            string timer = ScoreManager.Instance.Timer.ToString("0.000", CultureInfo.InvariantCulture).Replace('.', '\"');
            m_TimerText.text = string.Format("Time: {0}", timer);
        }
    }
}

