using Assets.Controller;
using UnityEngine;

namespace Assets
{
    public class StartPoint : MonoBehaviour
    {
        public Camera ownCamera;
        public SkatingController m_playerPrefab;
        SkatingController instanceController;

        void Start()
        {
            instanceController = FindObjectOfType<SkatingController>();
            if (instanceController == null)
            {
                instanceController = Instantiate(m_playerPrefab, transform.position, transform.rotation);
            }
            else
            {
                instanceController.transform.position = transform.position;
                instanceController.transform.rotation = transform.rotation;
            }

            ownCamera.gameObject.SetActive(false);
        }
    }
}
