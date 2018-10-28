using Assets.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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
            /*
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                Scene s = SceneManager.GetSceneByBuildIndex(i);
                var trackBtn = Instantiate(m_trackTemplate, m_tracklistLayout.transform);
                var btnText = trackBtn.GetComponentInChildren<Text>();
                btnText.text = s.name;
            }*/
            for (int i = 0; i < GameManager.Instance.m_tracklistProfile.m_tracks.Length; i++)
            {
                string trackName = GameManager.Instance.m_tracklistProfile.m_tracks[i];
                var trackBtn = Instantiate(m_trackTemplate, m_tracklistLayout.transform);
                trackBtn.SetTrack(trackName);
            }

            m_trackTemplate.gameObject.SetActive(false);
        }

    }
}
