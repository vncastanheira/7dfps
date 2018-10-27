using Assets.Managers;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using vnc.Tools;

namespace Assets.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class PlayerUI : MonoBehaviour,
        IVncEventListener<GameEvent>
    {
        [Header("Settings")]
        public TextMeshProUGUI m_SpeedText;
        public TextMeshProUGUI m_TimerText;
        public Rigidbody m_body;
        CanvasGroup canvasGroup;

        private void Awake()
        {
            this.Listen();
            canvasGroup = GetComponent<CanvasGroup>();
        }

        void Update()
        {
            m_SpeedText.text = m_body.velocity.sqrMagnitude.ToString("0");
            //string timer = ScoreManager.Instance.Timer.ToString("0.000", CultureInfo.InvariantCulture).Replace('.', '\"');
            m_TimerText.text = string.Format("Time: {0}", ScoreManager.Instance.TimerFormatted);
        }


        public void OnVncEvent(GameEvent e)
        {
            switch (e.Event)
            {
                case GameEventType.TrackStart:
                case GameEventType.TrackRestart:
                    Show();
                    break;
                case GameEventType.TrackEnd:
                    Hide();
                    break;
            }
        }


        #region Canvas Group

        public void Hide()
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        public void Show()
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        #endregion


        private void OnDestroy()
        {
            this.Unlisten();
        }
    }
}

