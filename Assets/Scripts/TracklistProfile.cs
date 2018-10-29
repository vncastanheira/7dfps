using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets
{
    [CreateAssetMenu(menuName = "Tracklist Profile", fileName = "Default Tracklist Profile")]
    public class TracklistProfile : ScriptableObject
    {
        public TableRegister[] m_tracks;
    }

    [System.Serializable]
    public struct TableRegister
    {
        public string TrackName;    // uses active scene name 
        public int ID;              // registered score id in Game Jolt
    }
}
