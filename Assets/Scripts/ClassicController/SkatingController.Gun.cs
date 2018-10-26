using UnityEngine;
using vnc.Tools;

namespace Assets.Controller
{
    public partial class SkatingController : MonoBehaviour
    {
        [Header("Gun")]
        public Transform m_Gun;
        public Bullet m_bulletPrefab;
        public float m_bulletSpeed = 10f;
        Vector3 gunHitpoint;
        bool aimAssist = false;

        public void UpdateGun()
        {
            RaycastHit hit;
            Ray ray = new Ray(m_View.transform.position, m_View.transform.forward);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, m_ButtonLayer, QueryTriggerInteraction.Ignore))
            {
                aimAssist = true;
                gunHitpoint = hit.point;
                CheckFire(hit.transform.gameObject);
            }
            else if (Physics.Raycast(ray, out hit, Mathf.Infinity, m_SolidLayer, QueryTriggerInteraction.Ignore))
            {
                aimAssist = false;
                gunHitpoint = hit.point;
                CheckFire();
            }
            else
            {
                aimAssist = false;
                gunHitpoint = m_View.transform.position + m_View.transform.forward * 1000f;
                CheckFire();
            }            
        }

        public void CheckFire(GameObject target = null)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                m_MouseLook.Kick(10);
                //m_Gun.LookAt(gunHitpoint);

                Bullet bullet = Instantiate(m_bulletPrefab, m_Gun.position, m_Gun.rotation);
                bullet.SetTarget(gunHitpoint, m_bulletSpeed);

                if (target)
                {
                    VncEventSystem.Trigger(new ButtonPressedEvent { target = target });
                }
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
