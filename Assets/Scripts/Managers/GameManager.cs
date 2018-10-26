using UnityEngine;
using vnc.Tools;

namespace Assets.Managers
{
    public class GameManager : MonoBehaviour,
        IVncEventListener<GameEvent>
    {
        public static GameManager Instance { get; private set; }


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Init()
        {
            Instance = FindObjectOfType<GameManager>();
            if (Instance == null)
            {
                Instance = (new GameObject("_GAME_MANAGER")).AddComponent<GameManager>();
            }
        }

        private void Awake()
        {
            this.Listen();
            
        }

        public void OnVncEvent(GameEvent e)
        {
            switch (e.Event)
            {
                case GameEventType.TrackStart:
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    break;
                case GameEventType.TrackEnd:
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    break;
                default:
                    break;
            }
        }

        private void OnDestroy()
        {
            this.Unlisten();
        }
    }
}
