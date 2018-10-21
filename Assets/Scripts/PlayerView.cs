using Assets.Controller;
using System;
using UnityEngine;
using vnc.Tools;
using vnc.Utilities;

namespace Assets.Player
{
    [RequireComponent(typeof(ClassicController))]
    public class PlayerView : MonoBehaviour
    {
        public float leanRotation { get; set; }

        [Header("View")]
        public Camera View;
        public Vector3 ViewBobbing = new Vector3(0, 1, 0);
        public Animator m_ViewAnimator;
        [Range(0.01f, 100.0f)] public float m_delayStep = 0.5f;
        Vector3 viewDelay = Vector3.zero;
        public float m_fallTween = 0.5f;    // tween effect speed when falling
        float defaultFOV;

        [Header("Gun")]
        public Camera GunCamera;
        public Vector3 GunOffset;
        public float BobbingSpeed = 200f;
        public float m_RecoilDecay = 5f;
        private Vector3 gunStartPos;
        private float gunbobCycle = 0;

        [Header("Landing")]
        public float m_LandingForce = 2;            // lower on Y axis by this amount, maximum
        public float m_LandingEase = 2;             // speed of camera movement
        public float m_LadingVelocityMax = 0.4f;    // target player Y velocity to reach maximum landing force
        private float landing = 0;                  // helper variable, current unit value
        private float landingTarget = 0;            // helper variable, set by landing force

        [Header("Stairs")]
        public Easings.Functions m_stairEasingFunction;
        public float m_stairSpeed = 5f;
        public float m_cameraOffset;
        public float m_stairEasingThreshold = 0.01f;
        float viewUpPosition;

        [Space]
        public float fallPunchForce = 5.0f;
        public float fallTolerance = 2.0f;
        private float fallOffset = 0;

        public float viewRecoil = 0;

        private ClassicController classicController;

        private Vector3 viewStartPos;
        private float viewbobCycle = 0;

        private Vector3 viewVelocity;
        private float strafe;

        void Awake()
        {
            classicController = GetComponent<ClassicController>();

            gunStartPos = GunCamera.transform.localPosition;
            viewStartPos = View.transform.localPosition;
        }

        void Start()
        {
            classicController.OnLanding.AddListener(KnockScreenOnFalling);
            classicController.OnLanding.AddListener(OnLanding);
            viewDelay = transform.position;
            defaultFOV = View.fieldOfView;
        }

        RaycastHit[] aimhit = new RaycastHit[2];
        void Update()
        {
            if (true) // Game.Player.Health.IsAlive
            {

                MoveGun();
                LeanView();

                m_ViewAnimator.SetFloat("Recoil_Intensity", viewRecoil);
                viewRecoil = Mathf.Lerp(viewRecoil, 0, Time.deltaTime * m_RecoilDecay);
                //viewDelay = Vector3.Slerp(viewDelay, transform.position, m_delayStep * Time.deltaTime);
                //float delayOffset = (viewDelay.y - transform.position.y);
                //View.transform.localPosition += delayOffset * Vector3.up;
            }
            else
            {
                Vector3 damp = Vector3.zero;
                View.transform.localPosition = Vector3.SmoothDamp(View.transform.localPosition, new Vector3(0, -1.2f, 0), ref damp, .1f);
            }

            Letterbox();


            //int hits = Physics.RaycastNonAlloc(View.transform.position, View.transform.forward, aimhit, Mathf.Infinity,
            //    Game.Settings.ENTITY_LAYER | Game.Settings.SOLID_LAYER, QueryTriggerInteraction.Ignore);
        }

        public void LateUpdate()
        {
            BobView();
            Stair();
        }

        private void Letterbox()
        {
            //if (classicController.Velocity.y < -0.9)
            //{
            //    float y = Mathf.MoveTowards(View.rect.y, .05f, Time.deltaTime * m_fallTween);
            //    float h = Mathf.MoveTowards(View.rect.height, .9f, Time.deltaTime * m_fallTween);
            //    View.rect = new Rect(0, y, 1, h);
            //    View.fieldOfView = Mathf.Lerp(View.fieldOfView, defaultFOV + 10, Time.deltaTime * 2);
            //}
            //else
            //{
            //    float y = Mathf.MoveTowards(View.rect.y, 0, Time.deltaTime * m_fallTween);
            //    float h = Mathf.MoveTowards(View.rect.height, 1, Time.deltaTime * m_fallTween);
            //    View.rect = new Rect(0, y, 1, h);
            //    View.fieldOfView = Mathf.Lerp(View.fieldOfView, defaultFOV, Time.deltaTime * 2);
            //}
            //GunCamera.rect = View.rect;
        }

        Vector3 InverseLerp(Vector3 a, Vector3 b, float t)
        {
            return a - (b - a) * t;
        }

        /// <summary>
        /// Move the gun as the player is moving
        /// </summary>
        void MoveGun()
        {
            var inputMagnitude = Mathf.Abs(classicController.WalkForward) + Mathf.Abs(classicController.Strafe);
            inputMagnitude = Mathf.Clamp01(inputMagnitude);

            float bobOscillate = Mathf.Sin(gunbobCycle * Mathf.Deg2Rad) / 2;
            gunbobCycle += (Time.deltaTime * BobbingSpeed);
            if (gunbobCycle >= 360) gunbobCycle = 0;
            bobOscillate *= inputMagnitude;

            var move = gunStartPos + new Vector3(bobOscillate * GunOffset.x, bobOscillate * +GunOffset.y, bobOscillate * GunOffset.z);
            GunCamera.transform.localPosition = Vector3.Slerp(GunCamera.transform.localPosition, move, Time.deltaTime * 5);
        }

        /// <summary>
        /// Move camera up and down on velocity
        /// </summary>
        void BobView()
        {
            //var velocity = classicController.Velocity;
            //velocity.y = 0f;
            //var velocityMag = Mathf.Clamp01(velocity.magnitude);

            //float bobOscillate = Mathf.Sin(viewbobCycle * Mathf.Deg2Rad) / 2;
            //viewbobCycle += (Time.deltaTime * BobbingSpeed);
            //if (viewbobCycle >= 360) viewbobCycle = 0;
            //bobOscillate *= velocityMag;

            //var move = viewStartPos + new Vector3(bobOscillate * ViewBobbing.x, bobOscillate * +ViewBobbing.y + landing, bobOscillate * ViewBobbing.z);

            //View.transform.position = View.transform.parent.position + move;
            //View.transform.position += Vector3.down * classicController.StepDelta;

            //landing = Mathf.Lerp(landing, landingTarget, Time.deltaTime * m_LandingEase);
            //landingTarget = Mathf.Clamp(landingTarget + Time.deltaTime * m_LandingEase, -10, 0);

        }

        void Stair()
        {
            float next = View.transform.parent.position.y;
            float difference = next - viewUpPosition;

            if (difference > m_stairEasingThreshold)
            {
                viewUpPosition += Easings.Interpolate(m_stairSpeed * Time.deltaTime, m_stairEasingFunction) * difference;
            }
            else
            {
                viewUpPosition = next;
            }

            View.transform.position = new Vector3(View.transform.position.x, viewUpPosition, View.transform.position.z);
        }

        public void OnLanding()
        {
            //float fallingVel = Mathf.Abs(classicController.Velocity.y);

            //if (fallingVel > .3f)
            //{
            //    // force multiplied by the falling speed
            //    landingTarget = -m_LandingForce * (Mathf.Clamp01(fallingVel / m_LadingVelocityMax));
            //}
        }

        /// <summary>
        /// Knock the screen a bit to the left when landing
        /// </summary>
        void KnockScreenOnFalling()
        {
            //fallOffset -= Time.deltaTime; fallOffset = fallOffset < 0 ? 0 : fallOffset;
            //var fallVel = Mathf.Abs(classicController.Velocity.y);

            //if (fallVel > fallTolerance)
            //{
            //    Vector3 safeAngle = Vector3.forward * fallPunchForce * fallVel;
            //    fallOffset = fallPunchForce;
            //}
        }

        /// <summary>
        /// Turn camera view left and right
        /// </summary>
        void LeanView()
        {
            strafe = Mathf.SmoothStep(strafe, classicController.Strafe, 0.15f);
            m_ViewAnimator.SetFloat("Leaning", strafe);
        }

        public void RecoilPunch(float intensity)
        {
            m_ViewAnimator.SetTrigger("Recoil");

            viewRecoil = intensity;
        }
    }

}
