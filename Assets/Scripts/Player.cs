using Assets.Controller;
using UnityEngine;
using vnc.Effects;

namespace Assets.Player
{
    [RequireComponent(typeof(ClassicController))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class Player : MonoBehaviour
    {
        [Header("Settings")]
        public Transform m_PlayerView;
        public Transform m_PlayerHead;
        public UnityMouseLook m_MouseLook;
        
        CapsuleCollider capsule;
        ClassicController p_Controller;

        // debug
        Mesh sceneMesh;

        void Awake()
        {
            capsule = GetComponent<CapsuleCollider>();
            p_Controller = GetComponent<ClassicController>();
            m_MouseLook.Init(transform, m_PlayerView);

            DontDestroyOnLoad(gameObject);
        }
        
        void Update()
        {
            if (Application.isPlaying)
            {
                m_MouseLook.SetCursorLock(true);
                //m_MouseLook.LookRotation(m_PlayerHead, m_PlayerView);
                m_MouseLook.UpdateCursorLock();

                float fwd = (Input.GetButton("Forward") ? 1 : 0) - (Input.GetButton("Backwards") ? 1 : 0);
                float yaw = (Input.GetButton("Right") ? 1 : 0) - (Input.GetButton("Left") ? 1 : 0);
                p_Controller.SetInput(fwd, 0, yaw, 0, false);
            }
        }


        float fwd, strafe, swim;
        bool jump;
        void GetKeyInput()
        {
            //if (Game.Player.Health.IsAlive && !OnIntermission)
            //{
            //    fwd = (InputManager.GetButton("Forward") ? 1 : 0) - (InputManager.GetButton("Backwards") ? 1 : 0);
            //    strafe = (InputManager.GetButton("Strafe_Right") ? 1 : 0) - (InputManager.GetButton("Strafe_Left") ? 1 : 0);

            //    swim = InputManager.GetAxisRaw("Swim");
            //    jump = InputManager.GetButtonDown("Jump");
            //    p_Controller.SetInput(fwd, strafe, swim, jump);
            //    p_Controller.Sprint = InputManager.GetButton("Sprint");
            //}
            //else
            //{
            //    p_Controller.SetInput(0, 0, 0, false);
            //}
        }
        
    }
}
