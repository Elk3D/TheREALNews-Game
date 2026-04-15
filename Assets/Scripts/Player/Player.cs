using UnityEngine;
using TheRealNews.Core;

namespace TheRealNews.Player
{
    /// <summary>
    /// Hub component for the first-person controller used in Phase 1 (Office).
    /// Drives the serialized sub-systems each frame. Stripped of the original
    /// decompiled GameCamera / CameraMovements / CameraTilt / SecretCode / InkDemon
    /// dependencies — this is a plain MonoBehaviour that talks to GameManager
    /// only for the global IsPaused flag.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class Player : MonoBehaviour
    {
        [Header("Containers")]
        [SerializeField] private Transform m_HeadContainer;
        [SerializeField] private Transform m_CameraHolder;

        [Header("Camera")]
        [SerializeField] private Camera m_Camera;
        [SerializeField] private float m_DefaultFOV = 60f;
        [SerializeField] private float m_RunFOV = 65f;
        [SerializeField] private float m_FovLerpSpeed = 5f;

        [Header("Sub-systems")]
        [SerializeField] private PlayerMovement m_PlayerMovement;
        [SerializeField] private PlayerLook m_PlayerLook;
        [SerializeField] private CameraHeadBob m_CameraHeadBob;
        [SerializeField] private PlayerFootsteps m_PlayerFootsteps;

        public Transform HeadContainer => m_HeadContainer;
        public Transform CameraHolder => m_CameraHolder;
        public PlayerMovement PlayerMovement => m_PlayerMovement;
        public PlayerLook PlayerLook => m_PlayerLook;
        public CameraHeadBob CameraHeadBob => m_CameraHeadBob;
        public PlayerFootsteps PlayerFootsteps => m_PlayerFootsteps;

        public CharacterController CharacterController { get; private set; }
        public Camera Camera => m_Camera;
        public bool IsLocked { get; private set; }
        public bool IsInitialized { get; private set; }

        private void Awake()
        {
            Initialize();
        }

        public void Initialize()
        {
            if (IsInitialized) return;

            CharacterController = GetComponent<CharacterController>();

            if (m_Camera == null) m_Camera = GetComponentInChildren<Camera>(true);

            m_PlayerMovement.Initialize(CharacterController);
            m_PlayerLook.Initialize(transform, m_HeadContainer);
            m_CameraHeadBob.Initialize(CharacterController, m_CameraHolder, m_PlayerMovement);

            IsInitialized = true;
        }

        private void Update()
        {
            var gm = GameManager.Instance;
            if (gm != null && gm.IsPaused) return;
            if (IsLocked) return;

            m_PlayerLook.GetInput();
            m_PlayerLook.UpdateCursorLock();
            m_PlayerMovement.UpdateMovementInput();
            m_PlayerLook.Rotation(transform, m_HeadContainer);
            m_PlayerMovement.UpdateMovement(transform);
            m_CameraHeadBob.Tick();
            m_PlayerFootsteps.Tick(
                CharacterController.velocity.magnitude,
                m_PlayerMovement.CurrentSpeed,
                m_PlayerMovement.MovementInput.x,
                m_PlayerMovement.MovementInput.y,
                m_PlayerMovement.IsRunning);

            if (m_Camera != null)
            {
                float target = m_PlayerMovement.IsRunning ? m_RunFOV : m_DefaultFOV;
                m_Camera.fieldOfView = Mathf.Lerp(m_Camera.fieldOfView, target, Time.deltaTime * m_FovLerpSpeed);
            }
        }

        public void Lock()
        {
            IsLocked = true;
            m_PlayerMovement.CancelMovement();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void Unlock()
        {
            IsLocked = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
