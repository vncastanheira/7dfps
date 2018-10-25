using UnityEngine;

namespace Assets.Controller
{
    public partial class SkatingController : MonoBehaviour
    {
        [Header("Gun")]
        public Bullet m_bulletPrefab;
        public float m_bulletSpeed = 10f;
        Vector3 gunHitpoint;
        bool aimAssist = false;

        public void UpdateGun()
        {
            RaycastHit hit;
            Ray ray = new Ray(m_View.transform.position, m_View.transform.forward);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, m_EnemyLayer, QueryTriggerInteraction.Ignore))
            {
                aimAssist = true;
                gunHitpoint = hit.transform.position;
                CheckFire();
            }
            else if (Physics.Raycast(ray, out hit, Mathf.Infinity, m_SolidLayer, QueryTriggerInteraction.Ignore))
            {
                aimAssist = false;
                gunHitpoint = hit.point;;
                CheckFire();
            }
            else
            {
                aimAssist = false;
                gunHitpoint = m_View.transform.position + m_View.transform.forward * 1000f;
                CheckFire();
            }            
        }

        public void CheckFire()
        {
            if (Input.GetButtonDown("Fire1"))
            {
                Bullet bullet = Instantiate(m_bulletPrefab, transform.position + transform.right, m_View.transform.rotation);
                bullet.SetTarget(gunHitpoint, m_bulletSpeed);
            }            
        }

#if UNITY_EDITOR
        public void DrawGizmosGun()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(gunHitpoint, 1f);
        }
#endif
    }

}
