using UnityEngine;
using vnc.Tools;

namespace Assets
{
    public class FinishPoint : MonoBehaviour,
        IVncEventListener<GameEvent>
    {
        public LayerMask m_player;
        public Camera m_camera;
        public float m_lookingSpeed = 3f;
        [Range(0f, 90f)] public float m_yaw = 20f;
        Quaternion originalCameraRot;

        bool trackEnded = false;
        short dir = 1;

        private void Awake()
        {
            m_camera.gameObject.SetActive(false);
            originalCameraRot = m_camera.transform.rotation;
            this.Listen();
        }

        private void Update()
        {
            m_camera.gameObject.SetActive(trackEnded);

            if (trackEnded)
            {
                Quaternion target = originalCameraRot * Quaternion.Euler(0, m_yaw * dir, 0);
                m_camera.transform.rotation = Quaternion.RotateTowards(m_camera.transform.rotation, target, Time.deltaTime * m_lookingSpeed);
                if (m_camera.transform.rotation == target)
                    dir *= -1;
            }
        }

        public void OnVncEvent(GameEvent e)
        {
            switch (e.Event)
            {
                case GameEventType.TrackStart:
                case GameEventType.TrackRestart:
                    trackEnded = false;
                    break;
                case GameEventType.TrackEnd:
                    trackEnded = true;
                    break;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                VncEventSystem.Trigger(new GameEvent { Event = GameEventType.TrackEnd });
            }
        }

        private void OnDestroy()
        {
            this.Unlisten();
        }
    }
}
