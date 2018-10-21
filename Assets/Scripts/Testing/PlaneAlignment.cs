using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using vnc.Utilities;

namespace Assets.Testing
{
    public class PlaneAlignment : MonoBehaviour {

        public CapsuleCollider ownCollider;
	
	    void Update () {
            Ray floorRay = new Ray(transform.position, Vector3.down);
            RaycastHit hit;
            
            if (PhysicsExtensions.CapsuleCast(ownCollider, -transform.up, out hit, Mathf.Infinity, 11))
            {
                transform.rotation = Quaternion.LookRotation(transform.forward, hit.normal);
            }
        }
    }
}
