using UnityEngine;
using vnc.Tools;
using vnc.Utilities;

namespace Assets
{
    public class FinishPoint : MonoBehaviour
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
        }

        private void Update()
        {
            if (trackEnded)
            {
                Quaternion target = originalCameraRot * Quaternion.Euler(0, m_yaw * dir, 0);
                m_camera.transform.rotation = Quaternion.RotateTowards(m_camera.transform.rotation, target, Time.deltaTime * m_lookingSpeed);
                if (m_camera.transform.rotation == target)
                    dir *= -1;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareLayer(m_player))
            {
                m_camera.gameObject.SetActive(true);
                VncEventSystem.Trigger(new GameEvent { Event = GameEventType.TrackEnd });
                trackEnded = true;
            }

        }
    }
}
