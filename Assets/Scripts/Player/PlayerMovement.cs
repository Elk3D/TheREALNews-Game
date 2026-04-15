using System;
using UnityEngine;

namespace TheRealNews.Player
{
    /// <summary>
    /// CharacterController-based first-person locomotion. Originally extended
    /// JDisposable and called into a static PlayerInput; both are gone — input
    /// goes through UnityEngine.Input directly. Field layout preserved so any
    /// in-scene Inspector tuning still applies.
    /// </summary>
    [Serializable]
    public class PlayerMovement
    {
        [Header("Movement Options")]
        [SerializeField] private float m_MoveSpeed = 2f;

        [Space]
        [SerializeField] private bool m_CanRun = true;
        [SerializeField] private float m_RunSpeed = 2.5f;

        [Space]
        [SerializeField] private bool m_CanJump = true;
        [SerializeField] private float m_JumpSpeed = 2f;

        [Space]
        [SerializeField] private bool m_CanCrouch = true;
        [SerializeField] private float m_CrouchSpeed = 0.25f;

        private readonly float m_Gravity = 0.2f;
        private float m_ActiveGravity;
        private Vector3 m_ExternalForce = Vector3.zero;
        private CharacterController m_CharacterController;
        private Vector3 m_MoveDirection = Vector3.zero;
        private Vector3 m_InitialAirPosition;

        public float RunSpeed => m_RunSpeed;
        public Vector3 MoveDirection => m_MoveDirection;
        public Vector2 MovementInput { get; private set; }
        public Vector3 PreviousPosition { get; private set; }
        public Vector3 CurrentPosition { get; private set; }
        public bool PreviouslyGrounded { get; private set; }
        public bool CrouchInput { get; private set; }
        public bool JumpInput { get; private set; }
        public float CurrentSpeed { get; private set; }
        public bool CanRun { get; private set; }
        public bool IsRunning { get; private set; }
        public bool IsRunLocked { get; private set; }
        public bool CanCrouch { get; private set; }
        public bool IsCrouched { get; private set; }
        public bool CanJump { get; private set; }
        public Vector3 InitialAirPosition { get => m_InitialAirPosition; set => m_InitialAirPosition = value; }

        public void LockRun()        => IsRunLocked = true;
        public void UnlockRun()      => IsRunLocked = false;
        public void StopRun()        => IsRunning = false;
        public void ForceLockRun()   => CanRun = false;
        public void ForceUnlockRun() => CanRun = true;
        public void LockCrouch()     => CanCrouch = false;
        public void UnlockCrouch()   => CanCrouch = true;
        public void LockJump()       => CanJump = false;
        public void UnlockJump()     => CanJump = true;

        public void Initialize(CharacterController characterController)
        {
            m_CharacterController = characterController;
            CanJump = m_CanJump;
            CanRun = m_CanRun;
            CanCrouch = m_CanCrouch;
            PreviouslyGrounded = true;
            m_MoveDirection.y = -0.27f;
        }

        public void UpdateMovementInput()
        {
            GetJumpInput();
            GetCrouchInput();
            GetMovementInput();
        }

        private void GetJumpInput()
        {
            if (JumpInput || !m_CharacterController.isGrounded || !CanJump) return;

            int playerLayer = LayerMask.NameToLayer("Player");
            int mask = playerLayer >= 0 ? ~(1 << playerLayer) : ~0;

            if (!Physics.SphereCast(m_CharacterController.transform.position, m_CharacterController.radius,
                Vector3.up, out _, 1.15f - m_CharacterController.radius, mask, QueryTriggerInteraction.Ignore))
            {
                JumpInput = Input.GetButtonDown("Jump");
                if (JumpInput)
                {
                    float kick = (IsRunning && MovementInput.y > 0f) ? 0.11f : 0.1f;
                    m_ActiveGravity += m_JumpSpeed * kick;
                    IsCrouched = false;
                }
            }
            JumpInput = false;
        }

        private void GetCrouchInput()
        {
            if (!CrouchInput && m_CharacterController.isGrounded && CanCrouch)
            {
                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C))
                    IsCrouched = true;
                else if (Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.C))
                    IsCrouched = false;

                CrouchInput = false;
            }
        }

        private void GetMovementInput()
        {
            float x = MovementInput.x;
            float y = MovementInput.y;

            if (m_CharacterController.isGrounded)
            {
                x = Input.GetAxis("Horizontal");
                y = Input.GetAxis("Vertical");
            }
            else
            {
                if (MovementInput.x == 0f) x = Input.GetAxisRaw("Horizontal") / 2f;
                if (MovementInput.y == 0f) y = Input.GetAxisRaw("Vertical")   / 2f;
            }

            IsRunning = y > 0f && CanRun && !IsRunLocked && Input.GetKey(KeyCode.LeftShift);

            int playerLayer = LayerMask.NameToLayer("Player");
            int mask = playerLayer >= 0 ? ~(1 << playerLayer) : ~0;

            if (IsRunning && IsCrouched && !Physics.SphereCast(m_CharacterController.transform.position,
                m_CharacterController.radius, Vector3.up, out _,
                1.15f - m_CharacterController.radius, mask, QueryTriggerInteraction.Ignore))
            {
                IsCrouched = false;
            }

            if (IsCrouched)
            {
                IsRunning = false;
                CurrentSpeed = m_CrouchSpeed;
            }
            else
            {
                CurrentSpeed = IsRunning ? m_RunSpeed : m_MoveSpeed;
            }

            var input = new Vector2(x, y);
            if (input.sqrMagnitude > 1f) input.Normalize();
            MovementInput = input;

            CurrentSpeed *= Time.deltaTime;
        }

        public void UpdateMovement(Transform transform)
        {
            GetMovement(transform);
            GetPhysics();
            GetGrounding();
            GetCrouch();
        }

        private void GetMovement(Transform transform)
        {
            if (!m_CharacterController.enabled) return;

            Vector3 planar = transform.forward * MovementInput.y + transform.right * MovementInput.x;
            m_MoveDirection.x = Mathf.Clamp(planar.x, -1f, 1f);
            m_MoveDirection.z = Mathf.Clamp(planar.z, -1f, 1f);
            float y = m_MoveDirection.y;

            m_MoveDirection = m_MoveDirection.normalized * CurrentSpeed;
            m_MoveDirection.y = y;

            bool grounded = m_ActiveGravity <= 0f && m_CharacterController.isGrounded;
            if (grounded)
            {
                m_ActiveGravity = 0f;
                m_MoveDirection.y = -0.27f;
            }
            else
            {
                m_ActiveGravity -= m_Gravity * Time.deltaTime;
                m_MoveDirection.y = m_ActiveGravity;
            }

            PreviousPosition = m_CharacterController.transform.position;
            CollisionFlags flags = m_CharacterController.Move(m_MoveDirection);

            if (m_ActiveGravity < 0f)
                flags = m_CharacterController.Move(Vector3.up * -0.01f);

            if (flags == CollisionFlags.Above && m_ActiveGravity > 0f)
                m_ActiveGravity = 0f;

            CurrentPosition = m_CharacterController.transform.position;
        }

        private void GetPhysics()
        {
            if (m_ExternalForce.sqrMagnitude <= 0f) return;
            m_CharacterController.Move(m_ExternalForce * Time.deltaTime);
            m_ExternalForce = Vector3.MoveTowards(m_ExternalForce, Vector3.zero, 0.95f);
        }

        private void GetGrounding()
        {
            if (m_CharacterController.isGrounded && !PreviouslyGrounded && m_ActiveGravity <= 0f)
                m_ActiveGravity = 0f;

            if (PreviouslyGrounded)
                m_InitialAirPosition = m_CharacterController.transform.position;

            PreviouslyGrounded = m_CharacterController.isGrounded;
        }

        public void SetPreviouslyGrounded(bool active) => PreviouslyGrounded = active;

        private void GetCrouch()
        {
            if (m_CharacterController.isGrounded && CrouchInput)
                IsCrouched = !IsCrouched;
        }

        public void CancelMovement()
        {
            MovementInput = Vector2.zero;
            CurrentSpeed = 0f;
            m_ActiveGravity = 0f;
            m_ExternalForce = Vector3.zero;
            CrouchInput = false;
            JumpInput = false;
        }

        public void ForceStand()
        {
            IsCrouched = false;
            CrouchInput = false;
        }

        public void ForceCrouch()
        {
            UnlockCrouch();
            IsCrouched = true;
            CrouchInput = false;
        }

        public void AddForce(Vector3 force) => m_ExternalForce += force;

        public void SetCrouchInput(bool isCrouched) => IsCrouched = isCrouched;
    }
}
