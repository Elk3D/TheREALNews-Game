using System;
using UnityEngine;

namespace TheRealNews.Player
{
    /// <summary>
    /// Mouse look with vertical clamp. Originally extended JDisposable and
    /// pulled input from a static PlayerInput; now uses UnityEngine.Input
    /// directly. Sensitivity is multiplied by an internal scale to keep the
    /// "feels right" range similar to the decompiled defaults.
    /// </summary>
    [Serializable]
    public class PlayerLook
    {
        [SerializeField] private float m_Sensitivity = 0.5f;
        [SerializeField] private bool  m_ClampVerticalRotation = true;
        [SerializeField] private float m_VerticalMinClamp = -82f;
        [SerializeField] private float m_VerticalMaxClamp =  82f;

        // Mouse axes are already framerate-independent; this scale matches
        // the feel of the original PlayerInput.LookX / LookY values.
        private const float MouseScale = 2f;

        private Quaternion m_CharacterTargetRotation;
        private Quaternion m_CameraTargetRotation;
        private bool m_IsRotationInitialized;
        private bool m_IsInitialVerticalClampInitialized;
        private float m_HorizontalClamp;
        private float m_InitialVerticalMaxClamp;
        private float m_InitialVerticalMinClamp;

        private float m_InputX;
        private float m_InputY;

        public bool clampvert => m_ClampVerticalRotation;
        public bool hasHorizontalLock { get; private set; }
        public float LookInputX => m_InputX;

        public void Initialize(Transform character, Transform camera)
        {
            ResetRotation(character, camera);
            if (!m_IsInitialVerticalClampInitialized)
            {
                m_IsInitialVerticalClampInitialized = true;
                m_InitialVerticalMinClamp = m_VerticalMinClamp;
                m_InitialVerticalMaxClamp = m_VerticalMaxClamp;
            }
        }

        public void ForceRotation(Quaternion rotation)       => m_CharacterTargetRotation = rotation;
        public void ForceCameraRotation(Quaternion rotation) => m_CameraTargetRotation = rotation;

        public void GetInput()
        {
            m_InputX =  Input.GetAxis("Mouse X") * m_Sensitivity * MouseScale;
            m_InputY = -Input.GetAxis("Mouse Y") * m_Sensitivity * MouseScale;
        }

        public void Rotation(Transform character) => Rotation(character, null, true);
        public void Rotation(Transform character, bool hasGravity) => Rotation(character, null, hasGravity);

        public void Rotation(Transform character, params Transform[] cameras)
        {
            for (int i = 0; i < cameras.Length; i++)
                if (cameras[i]) Rotation(character, cameras[i], true);
        }

        public void Rotation(Transform character, Transform camera) => Rotation(character, camera, true);

        public void Rotation(Transform character, Transform camera, bool hasGravity)
        {
            if (!IsNullRotation(m_InputX, m_InputY))
            {
                if (hasGravity)
                {
                    m_CharacterTargetRotation *= Quaternion.Euler(0f, m_InputX, 0f);
                    if (hasHorizontalLock)
                        m_CharacterTargetRotation = ClampRotationYAxis(m_CharacterTargetRotation, -m_HorizontalClamp, m_HorizontalClamp);
                    character.localRotation = m_CharacterTargetRotation;

                    if (camera)
                    {
                        m_CameraTargetRotation *= Quaternion.Euler(m_InputY, 0f, 0f);
                        if (m_ClampVerticalRotation)
                            m_CameraTargetRotation = ClampRotationXAxis(m_CameraTargetRotation, m_VerticalMinClamp, m_VerticalMaxClamp);
                        camera.localRotation = m_CameraTargetRotation;

                        Vector3 e = camera.localEulerAngles;
                        e.z = 0f;
                        camera.localEulerAngles = e;
                    }
                }
                else
                {
                    m_CharacterTargetRotation *= Quaternion.Euler(m_InputY, m_InputX, 0f);
                    character.localRotation = m_CharacterTargetRotation;
                }
            }
            else
            {
                character.localRotation = m_CharacterTargetRotation;
                if (camera) camera.localRotation = m_CameraTargetRotation;
            }
            UpdateCursorLock();
        }

        public void SmoothLook(Transform character, Transform camera, Vector3 newDirection)
        {
            Quaternion target = Quaternion.LookRotation(newDirection);
            character.localRotation = Quaternion.Lerp(character.rotation, target, 2f * Time.deltaTime);
            character.localEulerAngles = new Vector3(0f, character.localEulerAngles.y, 0f);

            camera.localRotation = Quaternion.Lerp(camera.rotation, target, 2f * Time.deltaTime);
            Quaternion lr = camera.localRotation;
            lr = ClampRotationXAxis(lr, m_VerticalMinClamp, m_VerticalMaxClamp);
            lr = ClampRotationYAxis(lr, -m_HorizontalClamp, m_HorizontalClamp);
            camera.localRotation = lr;

            Vector3 e = camera.localEulerAngles;
            e.z = 0f;
            camera.localEulerAngles = e;
        }

        public void ResetRotation(Transform character) => ResetRotation(character, null);

        public void ResetRotation(Transform character, Transform camera)
        {
            m_CharacterTargetRotation.eulerAngles = new Vector3(0f, character.localRotation.eulerAngles.y, 0f);
            if (camera)
                m_CameraTargetRotation.eulerAngles = new Vector3(camera.localRotation.eulerAngles.x, 0f, 0f);
        }

        public void ResetVerticalClamp()
        {
            m_VerticalMinClamp = m_InitialVerticalMinClamp;
            m_VerticalMaxClamp = m_InitialVerticalMaxClamp;
        }

        public void SetVerticalClamp(float clamp)
        {
            m_VerticalMinClamp = -clamp;
            m_VerticalMaxClamp =  clamp;
        }

        public void SetHorizontalClamp(float clamp) => m_HorizontalClamp = clamp;
        public void HorizontalClampSetActive(bool active) => hasHorizontalLock = active;

        public void UpdateCursorLock()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private bool IsNullRotation(float horizontal, float vertical)
        {
            if (horizontal == 0f && vertical == 0f) return true;
            if (!m_IsRotationInitialized) return m_IsRotationInitialized = true;
            return false;
        }

        private static Quaternion ClampRotationXAxis(Quaternion q, float min, float max)
        {
            q.x /= q.w; q.y /= q.w; q.z /= q.w; q.w = 1f;
            float angle = 114.59156f * Mathf.Atan(q.x);
            angle = Mathf.Clamp(angle, min, max);
            q.x = Mathf.Tan(MathF.PI / 360f * angle);
            return q;
        }

        private static Quaternion ClampRotationYAxis(Quaternion q, float min, float max)
        {
            q.x /= q.w; q.y /= q.w; q.z /= q.w; q.w = 1f;
            float angle = 114.59156f * Mathf.Atan(q.y);
            angle = Mathf.Clamp(angle, min, max);
            q.y = Mathf.Tan(MathF.PI / 360f * angle);
            return q;
        }
    }
}
