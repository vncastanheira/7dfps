using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using vnc.Tools;

namespace Assets
{
    public class Door : MonoBehaviour,
        IVncEventListener<GameEvent>
    {
        public Vector3 m_offset;
        public float m_speed = 10f;

        Vector3 originalPosition;
        Vector3 targetPosition;

        bool activated = false;
        MeshFilter meshFilter;

        private void Awake()
        {
            originalPosition = transform.position;
            targetPosition = transform.position + m_offset;
            this.Listen();
        }

        private void Update()
        {
            if (activated && Application.isPlaying)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, m_speed * Time.deltaTime);
            }
        }

        public void Activate() { activated = true; }

        public void OnVncEvent(GameEvent e)
        {
            switch (e.Event)
            {
                case GameEventType.TrackRestart:
                    transform.position = originalPosition;
                    activated = false;
                    break;
            }
        }

        private void OnDestroy()
        {
            this.Unlisten();
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (meshFilter == null)
                meshFilter = GetComponent<MeshFilter>();

            Gizmos.color = Color.red;
            Gizmos.DrawWireMesh(meshFilter.mesh, transform.position + m_offset);
        }


#endif
    }
}
