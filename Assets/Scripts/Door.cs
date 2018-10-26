using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets
{
    public class Door : MonoBehaviour
    {
        public Vector3 m_offset;
        public float m_speed = 10f;
        Vector3 targetPosition;
        bool activated = false;
        MeshFilter meshFilter;

        private void Awake()
        {
            targetPosition = transform.position + m_offset;
        }

        private void Update()
        {
            if (activated && Application.isPlaying)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, m_speed * Time.deltaTime);
            }
        }

        public void Activate() { activated = true; }

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
