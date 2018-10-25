using UnityEngine;

namespace Assets.Managers
{
    public class GameManager : MonoBehaviour
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
            
        }

    }
}
