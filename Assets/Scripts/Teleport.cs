using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleport : MonoBehaviour
{
    public Transform m_teleportTarget;

    private void OnTriggerEnter(Collider other)
    {
        if (m_teleportTarget)
        {
            other.transform.position = m_teleportTarget.position;
            other.transform.rotation = m_teleportTarget.rotation;
            other.attachedRigidbody.velocity = Vector3.zero;
        }
        else
        {
            Debug.LogWarning("No target for teleport " + name);
        }
    }

    private void OnDrawGizmos()
    {
        if (m_teleportTarget)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, m_teleportTarget.position);
            Gizmos.DrawCube(m_teleportTarget.position, Vector3.one);
        }
    }
}
