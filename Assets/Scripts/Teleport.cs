using UnityEngine;
using vnc.Tools;

namespace Assets
{
    public class Teleport : MonoBehaviour
    {
        public TeleportType m_teleportType;

        public void OnTeleport(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                VncEventSystem.Trigger(new GameEvent { Event = GameEventType.TrackRestart });
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (m_teleportType == TeleportType.OnEnter)
                OnTeleport(other);
        }

        private void OnTriggerExit(Collider other)
        {
            if (m_teleportType == TeleportType.OnExit)
                OnTeleport(other);
        }
    }

    public enum TeleportType
    {
        OnEnter,
        OnExit
    }
}
