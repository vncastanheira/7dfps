using GameJolt.API;
using GameJolt.API.Objects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.SceneManagement;
using vnc.Tools;

namespace Assets.Managers
{
    public class ScoreManager : MonoBehaviour,
        IVncEventListener<GameEvent>
    {
        public static ScoreManager Instance { get; private set; }

        [HideInInspector] public string GuestName;
        public TableRegister[] m_tableList;
        Dictionary<string, int> m_tableDic;
        
        public float Timer { get { return timeEnd - timeStart; } }
        public string TimerFormatted
        {
            get { return Timer.ToString("0.000", CultureInfo.InvariantCulture).Replace('.', '\"'); }
        }
        private float timeStart = 0f;
        private float timeEnd = 0f;
        private bool trackRunning = false;

        public event Action<bool> OnScoreAdded;
        public event Action<Score[]> OnScoreShow;

        private void Awake()
        {
            Instance = this;

            m_tableDic = new Dictionary<string, int>();
            for (int i = 0; i < m_tableList.Length; i++)
            {
                m_tableDic.Add(m_tableList[i].TrackName, m_tableList[i].ID);
            }
            int number = new System.Random().Next();
            GuestName = string.Format("Surfer#{0}", number);
        }

        private void Start()
        {
            this.Listen();
        }

        private void Update()
        {
            if (trackRunning)
            {
                timeEnd = Time.time;
            }
        }

        public void OnVncEvent(GameEvent e)
        {
            switch (e.Event)
            {
                case GameEventType.TrackStart:
                case GameEventType.TrackRestart:
                    timeStart = Time.time;
                    trackRunning = true;
                    break;
                case GameEventType.TrackEnd:
                    trackRunning = false;
                    RegisterAndShowScores();
                    break;
                case GameEventType.Pause:
                    trackRunning = false;
                    break;
                case GameEventType.Resume:
                    trackRunning = true;
                    break;
            }
        }

        public void RegisterAndShowScores()
        {
            Scene currentScene = SceneManager.GetActiveScene();
            if (m_tableDic.ContainsKey(currentScene.name))
            {
                int tableID = m_tableDic[currentScene.name];
                //int tableID = 381463; // Set it to 0 for main highscore table.
                string text = "Time (ms)"; // A string representing the score to be shown on the website.
                Action<bool> addScore = s =>
                {
                    OnScoreAdded?.Invoke(s);
                    Scores.Get(OnScoreShow, table: tableID, limit: 10);
                };

                int milliseconds = (int)(Timer * 1000);
                if (GameJoltAPI.Instance.HasSignedInUser)
                {
                    Scores.Add(milliseconds, text, tableID, "", addScore);
                }
                else
                {
                    // Guest
                    Scores.Add(milliseconds, text, GuestName, table: tableID, callback: addScore);
                }
            }
            else
            {
                Debug.LogError("Table " + currentScene.name + " not found.");
            }
        }


        private void OnDestroy()
        {
            this.Unlisten();
        }
    }

    [System.Serializable]
    public struct TableRegister
    {
        public string TrackName;    // uses active scene name 
        public int ID;              // registered score id in Game Jolt
    }
}
