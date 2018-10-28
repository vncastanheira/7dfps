using Assets.Managers;
using GameJolt.UI;
using UnityEngine;
using UIButton = UnityEngine.UI.Button;

namespace Assets.UI
{
    public class StartUI : MonoBehaviour
    {
        public UIButton m_startButton;
        public UIButton m_controlsButton;

        private void Start()
        {
            m_startButton.onClick.AddListener(() => StartCoroutine(GameManager.Instance.LoadLevel("Tracks Menu")));
            // TODO: controls scene
            //m_controlsButton.onClick.AddListener(() => StartCoroutine(GameManager.Instance.LoadLevel("")));
            m_startButton.gameObject.SetActive(false);

            GameJoltUI.Instance.ShowSignIn((signedIn) =>
            {
                m_startButton.gameObject.SetActive(true);
            });
        }
    }
}
