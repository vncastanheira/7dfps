using UnityEngine;

namespace Assets
{
    public class Bullet : MonoBehaviour
    {
        public GameObject m_decal;
        public LayerMask hitMask;
        Vector3 target;
        float speed;

        void Update()
        {
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
            RaycastHit hit;
            if (Physics.Raycast(transform.position- transform.forward, transform.forward, 
                out hit, 2f, hitMask, QueryTriggerInteraction.Ignore))
            {
                Quaternion rotation = Quaternion.LookRotation(hit.normal) * Quaternion.Euler(0, 0, Random.Range(0, 356));
                var decal = Instantiate(m_decal, hit.point + hit.normal * 0.01f, rotation);
                Destroy(decal.gameObject, 50f);
                Destroy(gameObject);
            }
            else if (Vector3.Distance(transform.position, target) <= 0)
            {
                Destroy(gameObject);
            }
        }

        public void SetTarget(Vector3 target, float speed)
        {
            this.target = target;
            this.speed = speed;
            transform.LookAt(target);
        }
    }
}
