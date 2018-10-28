using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using vnc.Tools;

namespace Assets.UI
{
    public class PauseUI : MonoBehaviour,
        IVncEventListener<GameEvent>
    {
        private void Start()
        {
            gameObject.SetActive(false);
            this.Listen();
        }

        public void OnVncEvent(GameEvent e)
        {
            if (e.Event == GameEventType.Pause)
            {
                gameObject.SetActive(true);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        public void Restart()
        {
            VncEventSystem.Trigger(new GameEvent { Event = GameEventType.TrackRestart });
        }

        public void Menu()
        {

        }

        private void OnDestroy()
        {
            this.Unlisten();
        }

    }
}
