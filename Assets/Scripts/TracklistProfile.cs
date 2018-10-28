using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets
{
    [CreateAssetMenu(menuName = "Tracklist Profile", fileName = "Default Tracklist Profile")]
    public class TracklistProfile : ScriptableObject
    {
        public string[] m_tracks;
    }

}
