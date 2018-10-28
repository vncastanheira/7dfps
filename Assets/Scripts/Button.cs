using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using vnc.Tools;

namespace Assets
{
    public class Button : MonoBehaviour,
        IVncEventListener<ButtonPressedEvent>,
        IVncEventListener<GameEvent>
    {
        public Vector3 m_pressedOffset;
        public Door m_door;

        Vector3 originalPosition;
        Vector3 pressedPosition;
        bool pressed = false;

        private void Start()
        {
            originalPosition = transform.position;
            pressedPosition = transform.position + m_pressedOffset;
            this.Listen<ButtonPressedEvent>();
            this.Listen<GameEvent>();
        }

        void Press()
        {
            transform.position = pressedPosition;
            m_door.Activate();
            pressed = true;
        }

        public void OnVncEvent(ButtonPressedEvent e)
        {
            if (e.target == gameObject && !pressed)
            {
                Press();
            }
        }


        public void OnVncEvent(GameEvent e)
        {
            switch (e.Event)
            {
                case GameEventType.TrackRestart:
                    transform.position = originalPosition;
                    pressed = false;
                    break;                
            }
        }

        private void OnDestroy()
        {
            this.Unlisten<ButtonPressedEvent>();
            this.Unlisten<GameEvent>();
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (m_door)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, m_door.transform.position);
            }
        }

#endif
    }
}
