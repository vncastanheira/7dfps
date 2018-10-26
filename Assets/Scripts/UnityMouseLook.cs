using Assets;
using System;
using UnityEngine;
using vnc.Tools;

[Serializable]
public class UnityMouseLook
{
    public float m_HorizontalSpeed = 2f;
    public float m_VerticalSpeed = 2f;
    public float MinimumX = -90F;
    public float MaximumX = 90F;
    public float MinimumY = -90F;
    public float MaximumY = 90F;
    public bool smooth;
    public float smoothTime = 5f;
    private bool lockCursor = true;

    [Space]
    public bool cameraKick = true;
    public float cameraKickOffset;
    public float cameraKickoffsetWindow;
    public float cameraKickSpeed = 10;

    private Quaternion m_CharacterTargetRot;
    private Quaternion m_CameraTargetRot;
    private float kick = 0;

    public void Init(Transform character, Transform camera)
    {
        m_CharacterTargetRot = character.localRotation;
        m_CameraTargetRot = camera.localRotation;
    }


    public void LookRotationCamera(Transform camera, bool autoAim, float autoAimSpeed)
    {
        kick -= (Time.deltaTime * cameraKickSpeed);
        kick = Mathf.Clamp(kick, 0, cameraKickOffset);

        float aimSpeed = autoAim ? autoAimSpeed : 1f;

        float xRot = Input.GetAxis("LookVertical") * m_VerticalSpeed * aimSpeed;

        m_CameraTargetRot *= Quaternion.Euler(-xRot, 0f, 0f);

        m_CameraTargetRot = ClampRotationAroundXAxis(m_CameraTargetRot);
        //m_CharacterTargetRot = ClampRotationAroundYAxis(m_CharacterTargetRot);

        if (smooth)
        {
            camera.localRotation = Quaternion.Slerp(camera.localRotation, m_CameraTargetRot,
                smoothTime * Time.deltaTime);
        }
        else
        {
            camera.localRotation = m_CameraTargetRot;

            if (cameraKick)
            {
                var x = Mathf.Clamp(kick, 0, cameraKickOffset - cameraKickoffsetWindow);
                camera.localRotation *= Quaternion.Euler(-x, 0, 0);
            }
        }
    }

    public void LookRotationCharacter(Transform character, bool autoAim, float autoAimSpeed)
    {
        float aimSpeed = autoAim ? autoAimSpeed : 1f;

        float yRot = Input.GetAxis("LookHorizontal") * m_HorizontalSpeed * aimSpeed;

        m_CharacterTargetRot *= Quaternion.Euler(0f, yRot, 0f);

        if (smooth)
        {
            character.localRotation = Quaternion.Slerp(character.localRotation, m_CharacterTargetRot,
                smoothTime * Time.deltaTime);
        }
        else
        {
            character.localRotation = m_CharacterTargetRot;
            
        }
    }

    public void SetCursorLock(bool value)
    {
        lockCursor = value;
    }

    public void UpdateCursorLock()
    {
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void Kick(float k) { kick = k; }

    Quaternion ClampRotationAroundXAxis(Quaternion q)
    {
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;

        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
        angleX = Mathf.Clamp(angleX, MinimumX, MaximumX);
        q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);        
        return q;
    }

    Quaternion ClampRotationAroundYAxis(Quaternion q)
    {
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;

        float angleY = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.y);
        angleY = Mathf.Clamp(angleY, MinimumY, MaximumY);
        q.y = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleY);
        return q;
    }
}

