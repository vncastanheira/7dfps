using Assets.Controller;
using UnityEngine;
using vnc.Tools;

namespace Assets.Managers
{
    public class GameManager : MonoBehaviour,
        IVncEventListener<GameEvent>
    {
        public static GameManager Instance { get; private set; }

        public TracklistProfile m_tracklistProfile;
        [HideInInspector] public SkatingController playerInstance;
        [HideInInspector] public StartPoint currentStartPoint;
        public bool Paused { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Init()
        {
            Instance = FindObjectOfType<GameManager>();
            if (Instance == null)
            {
                var manager = Resources.Load<GameManager>("_GAME_MANAGER");
                Instance = Instantiate(manager);
            }
        }

        private void Awake()
        {
            this.Listen();
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            if (Input.GetButtonDown("Pause"))
            {
                if(Paused)
                {
                    VncEventSystem.Trigger(new GameEvent { Event = GameEventType.Resume });
                    Paused = false;
                }
                else
                {
                    VncEventSystem.Trigger(new GameEvent { Event = GameEventType.Pause });
                    Paused = true;
                }
            }
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
                case GameEventType.Pause:
                    Time.timeScale = 0f;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    break;
                case GameEventType.Resume:
                    Time.timeScale = 1f;
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
