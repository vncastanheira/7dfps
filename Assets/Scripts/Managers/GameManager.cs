using Assets.Controller;
using UnityEngine;
using vnc.Tools;

namespace Assets.Managers
{
    public class GameManager : MonoBehaviour,
        IVncEventListener<GameEvent>
    {
        public static GameManager Instance { get; private set; }

        [HideInInspector] public SkatingController playerInstance;
        [HideInInspector] public StartPoint currentStartPoint;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Init()
        {
            Instance = FindObjectOfType<GameManager>();
            if (Instance == null)
            {
                //Instance = (new GameObject("_GAME_MANAGER")).AddComponent<GameManager>();
                Instance = Resources.Load<GameManager>("_GAME_MANAGER");
            }
        }

        private void Awake()
        {
            this.Listen();
            DontDestroyOnLoad(gameObject);
        }

        public void OnVncEvent(GameEvent e)
        {
            switch (e.Event)
            {
                case GameEventType.TrackStart:
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    break;
                case GameEventType.TrackRestart:
                    RestartTrack();
                    break;
                case GameEventType.TrackEnd:
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    break;
            }
        }

        public void RestartTrack()
        {
            playerInstance.transform.position = currentStartPoint.transform.position;
            playerInstance.transform.rotation = currentStartPoint.transform.rotation;
            var playerRb = playerInstance.GetComponent<Rigidbody>();
            playerRb.velocity = Vector3.zero;
        }

        private void OnDestroy()
        {
            this.Unlisten();
        }
    }
}
