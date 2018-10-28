using Assets.Managers;
using GameJolt.API;
using GameJolt.API.Objects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using vnc.Tools;

namespace Assets.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class ScoresUI : GroupUI,
        IVncEventListener<GameEvent>
    {
        [Header("Settings")]
        public bool m_startHidden;
        public string m_finalTimerPrefix;
        public TextMeshProUGUI m_finalTimerText;
        [Space]
        public VerticalLayoutGroup m_leaderboard;
        public TextMeshProUGUI m_scoreTemplate;
        public RectTransform m_loading;

        TextMeshProUGUI[] cachedInstances = new TextMeshProUGUI[11];

        void Start()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (m_startHidden)
                Hide();

            this.Listen();

            ScoreManager.Instance.OnScoreShow += OnScoreShow;
        }

        public void OnVncEvent(GameEvent e)
        {
            switch (e.Event)
            {
                case GameEventType.TrackStart:
                case GameEventType.TrackRestart:
                    Hide();
                    for (int i = 0; i < cachedInstances.Length; i++)
                        if (cachedInstances[i] != null)
                            cachedInstances[i].text = string.Empty;
                    break;
                case GameEventType.TrackEnd:
                    Show();
                    m_scoreTemplate.gameObject.SetActive(false);
                    m_loading.gameObject.SetActive(true);
                    ShowFinalScore();
                    break;
                default:
                    break;
            }
        }

        public void ShowFinalScore()
        {
            m_finalTimerText.text = string.Format("{0} {1}", m_finalTimerPrefix, ScoreManager.Instance.TimerFormatted);
            
        }

        #region Options
        public void NextLevel()
        {

        }

        public void Restart()
        {
            VncEventSystem.Trigger(new GameEvent { Event = GameEventType.TrackRestart });
        }

        public void MainMenu()
        {

        }
        #endregion

        #region Callbacks

        public void OnScoreShow(Score[] scores)
        {
            for (int i = 0; i < scores.Length; i++)
            {
                if(cachedInstances[i] == null)
                    cachedInstances[i] = Instantiate(m_scoreTemplate, m_leaderboard.transform, false);

                float t = scores[i].Value;
                string time = (t / 1000f).ToString("0.000", CultureInfo.InvariantCulture).Replace('.', '\"');

                cachedInstances[i].text = string.Format("{0}. {1} by {2}", i+1, time, scores[i].PlayerName);
                cachedInstances[i].gameObject.SetActive(true);
            }

            m_loading.gameObject.SetActive(false);
        }

        #endregion


        private void OnDestroy()
        {
            this.Unlisten();
        }       
    }

}
