using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UIButton = UnityEngine.UI.Button;

namespace Assets.UI
{
    [RequireComponent(typeof(UIButton))]
    public class LoadTrackUI : MonoBehaviour {

        UIButton button;
        string trackName;

        public void SetTrack(string name)
        {
            trackName = name;
            button = GetComponent<UIButton>();
            button.onClick.AddListener(() => StartCoroutine(SceneLoading(trackName)));
            var btnText = button.GetComponentInChildren<Text>();
            btnText.text = name;
        }

        IEnumerator SceneLoading(string sceneName)
        {
            yield return SceneManager.LoadSceneAsync(sceneName);
        }

    }
}
