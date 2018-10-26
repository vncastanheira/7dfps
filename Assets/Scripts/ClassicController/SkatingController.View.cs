using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Controller
{
    public partial class SkatingController : MonoBehaviour
    {

        [Header("View")]
        public Transform m_Head;    // generelly the camera parent
        public Camera m_View;       // the main camera view
        public UnityMouseLook m_MouseLook;
        bool lockCursor;

        public void UpdateView()
        {
            m_MouseLook.SetCursorLock(lockCursor);
            m_MouseLook.LookRotationCamera(m_View.transform, aimAssist, m_Settings.m_aimAssistSpeed);
            m_MouseLook.UpdateCursorLock();
        }
    }


}
