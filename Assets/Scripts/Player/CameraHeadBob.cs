using System;
using UnityEngine;

namespace TheRealNews.Player
{
    /// <summary>
    /// Sin/cos camera bob driven by velocity + IsRunning. The original pulled
    /// IsRunning off GameManager.Instance.Player.PlayerMovement; we just take
    /// a direct ref at Initialize() time.
    /// </summary>
    [Serializable]
    public class CameraHeadBob
    {
        [SerializeField] private bool m_Enable = true;

        [Header("Settings")]
        [SerializeField] private float m_WalkBobSpeed = 14f;
        [SerializeField] private float m_RunBobSpeed = 18f;
        [SerializeField] private float m_WalkBobAmount = 0.05f;
        [SerializeField] private float m_RunBobAmount = 0.1f;
        [SerializeField] private float m_HorizontalBobScale = 0.5f;
        [SerializeField] private float m_MidpointY;

        private CharacterController m_CharacterController;
        private Transform m_CameraHolder;
        private PlayerMovement m_Movement;
        private float m_Timer = MathF.PI / 2f;
        private Vector3 m_OriginalLocalPos;

        public void Initialize(CharacterController characterController, Transform cameraHolder, PlayerMovement movement)
        {
            m_CharacterController = characterController;
            m_CameraHolder = cameraHolder;
            m_Movement = movement;
            m_OriginalLocalPos = m_CameraHolder.localPosition;
            m_MidpointY = m_OriginalLocalPos.y;
            if (PlayerPrefs.GetInt("SETTINGS_HEAD_BOB", 1) == 0) Disable();
        }

        public void Tick()
        {
            if (!m_Enable || m_CameraHolder == null) return;

            Vector3 offset = Vector3.zero;
            float magnitude = m_CharacterController.velocity.magnitude;

            if (m_CharacterController.isGrounded && magnitude > 0.1f)
            {
                bool isRunning = m_Movement != null && m_Movement.IsRunning;
                float speed  = isRunning ? m_RunBobSpeed  : m_WalkBobSpeed;
                float amount = isRunning ? m_RunBobAmount : m_WalkBobAmount;

                m_Timer += Time.deltaTime * speed;
                offset.y = Mathf.Cos(m_Timer) * amount;
                offset.x = Mathf.Sin(m_Timer / 2f) * amount * m_HorizontalBobScale;
                m_CameraHolder.localPosition = m_OriginalLocalPos + offset;
            }
            else
            {
                m_Timer = MathF.PI / 2f;
                offset.y = Mathf.Lerp(m_CameraHolder.localPosition.y, m_MidpointY, Time.deltaTime * 8f);
                offset.x = Mathf.Lerp(m_CameraHolder.localPosition.x, m_OriginalLocalPos.x, Time.deltaTime * 8f);
                m_CameraHolder.localPosition = new Vector3(offset.x, offset.y, m_OriginalLocalPos.z);
            }
        }

        public void Enable() => m_Enable = true;

        public void Disable()
        {
            m_Enable = false;
            if (m_CameraHolder != null) m_CameraHolder.localPosition = m_OriginalLocalPos;
        }
    }
}
