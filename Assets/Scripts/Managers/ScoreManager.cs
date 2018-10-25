using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using vnc.Tools;

namespace Assets.Managers
{
    public class ScoreManager : MonoBehaviour,
        IVncEventListener<GameEvent>
    {
        public static ScoreManager Instance { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Init()
        {
            Instance = FindObjectOfType<ScoreManager>();
            if (Instance == null)
            {
                Instance = (new GameObject("_SCORE_MANAGER")).AddComponent<ScoreManager>();
            }
        }

        public float Timer { get { return timeEnd - timeStart; } }
        private float timeStart = 0f;
        private float timeEnd = 0f;
        private bool trackRunning = false;

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
                    timeStart = Time.time;
                    trackRunning = true;
                    break;
                case GameEventType.TrackEnd:
                    trackRunning = false;
                    break;
            }
        }

        private void OnDestroy()
        {
            this.Unlisten();
        }
    }
}
