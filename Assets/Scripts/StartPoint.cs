using Assets.Controller;
using Assets.Managers;
using UnityEngine;
using vnc.Tools;

namespace Assets
{
    public class StartPoint : MonoBehaviour
    {
        public Camera ownCamera;
        public SkatingController m_playerPrefab;

        void Start()
        {
            GameManager.Instance.playerInstance = FindObjectOfType<SkatingController>();
            if (GameManager.Instance.playerInstance == null)
            {
                GameManager.Instance.playerInstance = Instantiate(m_playerPrefab, transform.position, transform.rotation);
            }
            else
            {
                GameManager.Instance.playerInstance.transform.position = transform.position;
                GameManager.Instance.playerInstance.transform.rotation = transform.rotation;
            }

            ownCamera.gameObject.SetActive(false);
            GameManager.Instance.currentStartPoint = this; 

            VncEventSystem.Trigger(new GameEvent { Event = GameEventType.TrackStart });
        }
    }
}
