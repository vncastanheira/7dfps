using Assets.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.UI
{
    public class TracklistUI : GroupUI
    {
        public LayoutGroup m_tracklistLayout;
        public LoadTrackUI m_trackTemplate;

        void Start()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            for (int i = 0; i < GameManager.Instance.m_tracklistProfile.m_tracks.Length; i++)
            {
                string trackName = GameManager.Instance.m_tracklistProfile.m_tracks[i].TrackName;
                var trackBtn = Instantiate(m_trackTemplate, m_tracklistLayout.transform);
                trackBtn.SetTrack(trackName);
            }

            m_trackTemplate.gameObject.SetActive(false);
        }

        public void MainMenu()
        {
            StartCoroutine(GameManager.Instance.LoadLevel("Title"));
        }
    }
}
