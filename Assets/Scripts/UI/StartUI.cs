using Assets.Managers;
using GameJolt.UI;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UIButton = UnityEngine.UI.Button;

namespace Assets.UI
{
    public class StartUI : GroupUI
    {
        [Header("Settings")]
        public InputField m_guestInput;
        public UIButton m_guestStartBtn;
        public UIButton m_loginBtn;
        public TextMeshProUGUI m_errorText;

        private void Start()
        {
            m_errorText.text = string.Empty;
            m_guestStartBtn.onClick.AddListener(() =>
            {
                if (Validate())
                {
                    ScoreManager.Instance.GuestName = m_guestInput.text;
                    StartCoroutine(GameManager.Instance.LoadLevel("Tracks Menu"));
                }                
            });

            m_loginBtn.onClick.AddListener(() =>
            {
                Hide();
                GameJoltUI.Instance.ShowSignIn((signedIn) =>
                {
                    if (signedIn)
                    {
                        StartCoroutine(GameManager.Instance.LoadLevel("Tracks Menu"));
                    }
                    else
                    {
                        Show();
                    }
                });
            });
        }

        private bool Validate()
        {
            string guestName = m_guestInput.text;
            Regex RgxUrl = new Regex("[^a-z0-9]");

            if (string.IsNullOrEmpty(guestName))
            {
                m_errorText.text = "Guest name cannot be empty.";
                return false;
            }

            if (guestName.Contains(" "))
            {
                m_errorText.text = "Guest name cannot contain spaces.";
                return false;
            }

            if (RgxUrl.IsMatch(guestName))
            {
                m_errorText.text = "Guest name cannot contain special characters.";
                return false;
            }

            return true;
        }
    }
}
