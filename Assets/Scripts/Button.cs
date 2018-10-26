using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using vnc.Tools;

namespace Assets
{
    public class Button : MonoBehaviour,
        IVncEventListener<ButtonPressedEvent>
    {
        public Vector3 m_pressedOffset;
        public Door m_door;

        Vector3 pressedPosition;
        bool pressed = false;

        private void Start()
        {
            pressedPosition = transform.position + m_pressedOffset;
            this.Listen();
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
        
        private void OnDestroy()
        {
            this.Unlisten();
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
