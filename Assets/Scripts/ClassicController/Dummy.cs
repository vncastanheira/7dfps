using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Controller
{
    public class Dummy : MonoBehaviour
    {
        public CapsuleCollider ownCollider;
        public LayerMask m_SolidLayer;
        Rigidbody body;

        void Start()
        {
            body = GetComponent<Rigidbody>();
        }


        void Update()
        {
            Ray floorRay = new Ray(transform.position, Vector3.down);
            RaycastHit hit;
            if (Physics.Raycast(floorRay, out hit, ownCollider.height, m_SolidLayer))
            {
                body.AddForce(transform.forward * 10, ForceMode.Acceleration);
            }
        }
    }

}
