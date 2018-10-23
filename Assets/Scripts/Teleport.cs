using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Teleport : MonoBehaviour {

    public Transform m_teleportTarget;

    private void OnTriggerEnter(Collider other)
    {
        if (m_teleportTarget)
        {
            other.transform.position = m_teleportTarget.position;
            other.attachedRigidbody.velocity = Vector3.zero;
        }
        else
        {
            Debug.LogWarning("No target for teleport " + name);
        }
    }
}
