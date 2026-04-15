using System;
using UnityEngine;

[Serializable]
public class PlayerMovement : JDisposable
{
	[Header("Movement Options")]
	[SerializeField]
	private float m_MoveSpeed = 2f;

	[Space]
	[SerializeField]
	private bool m_CanRun = true;

	[SerializeField]
	private float m_RunSpeed = 2.5f;

	[Space]
	[SerializeField]
	private bool m_CanJump = true;

	[SerializeField]
	private float m_JumpSpeed = 2f;

	[Space]
	[SerializeField]
	private bool m_CanCrouch = true;

	[SerializeField]
	private float m_CrouchSpeed = 0.25f;

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

	public Vector3 InitialAirPosition
	{
		get
		{
			return m_InitialAirPosition;
		}
		set
		{
			m_InitialAirPosition = value;
		}
	}

	public void LockRun()
	{
		IsRunLocked = true;
	}

	public void UnlockRun()
	{
		IsRunLocked = false;
	}

	public void StopRun()
	{
		IsRunning = false;
	}

	public void ForceLockRun()
	{
		CanRun = false;
	}

	public void ForceUnlockRun()
	{
		CanRun = true;
	}

	public void LockCrouch()
	{
		CanCrouch = false;
	}

	public void UnlockCrouch()
	{
		CanCrouch = true;
	}

	public void LockJump()
	{
		CanJump = false;
	}

	public void UnlockJump()
	{
		CanJump = true;
	}

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
		if (JumpInput || !m_CharacterController.isGrounded || !CanJump)
		{
			return;
		}
		if (!Physics.SphereCast(m_CharacterController.transform.position, m_CharacterController.radius, Vector3.up, out var _, 1.15f - m_CharacterController.radius, ~(1 << LayerMask.NameToLayer("Player")), QueryTriggerInteraction.Ignore))
		{
			JumpInput = PlayerInput.Jump();
			if (JumpInput)
			{
				if (IsRunning && MovementInput.y > 0f)
				{
					m_ActiveGravity += m_JumpSpeed * 0.11f;
				}
				else
				{
					m_ActiveGravity += m_JumpSpeed * 0.1f;
				}
				IsCrouched = false;
			}
		}
		JumpInput = false;
	}

	private void GetCrouchInput()
	{
		if (!CrouchInput && m_CharacterController.isGrounded && CanCrouch)
		{
			if (PlayerInput.Crouch())
			{
				IsCrouched = true;
			}
			else if (PlayerInput.CrouchButtonUp())
			{
				IsCrouched = false;
			}
			CrouchInput = false;
		}
	}

	private void GetMovementInput()
	{
		float x = MovementInput.x;
		float num = MovementInput.y;
		if (m_CharacterController.isGrounded)
		{
			x = PlayerInput.MoveX();
			num = PlayerInput.MoveY();
		}
		else
		{
			if (MovementInput.x == 0f)
			{
				x = PlayerInput.MoveXRaw() / 2f;
			}
			if (MovementInput.y == 0f)
			{
				num = PlayerInput.MoveYRaw() / 2f;
			}
		}
		IsRunning = num > 0f && CanRun && !IsRunLocked && PlayerInput.Run();
		if (IsRunning && IsCrouched && !Physics.SphereCast(m_CharacterController.transform.position, m_CharacterController.radius, Vector3.up, out var _, 1.15f - m_CharacterController.radius, ~(1 << LayerMask.NameToLayer("Player")), QueryTriggerInteraction.Ignore))
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
			CurrentSpeed = (IsRunning ? m_RunSpeed : m_MoveSpeed);
		}
		MovementInput = new Vector2(x, num);
		if (MovementInput.magnitude > 1f || MovementInput.magnitude < -1f)
		{
			MovementInput.Normalize();
		}
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
		if (m_CharacterController.enabled)
		{
			Vector3 vector = transform.forward * MovementInput.y + transform.right * MovementInput.x;
			m_MoveDirection.x = vector.x;
			m_MoveDirection.z = vector.z;
			float y = MoveDirection.y;
			if (m_MoveDirection.z > 1f)
			{
				m_MoveDirection.z = 1f;
			}
			else if (m_MoveDirection.z < -1f)
			{
				m_MoveDirection.z = -1f;
			}
			if (m_MoveDirection.x > 1f)
			{
				m_MoveDirection.x = 1f;
			}
			else if (m_MoveDirection.x < -1f)
			{
				m_MoveDirection.x = -1f;
			}
			m_MoveDirection = m_MoveDirection.normalized * CurrentSpeed;
			m_MoveDirection.y = y;
			bool flag = false;
			if (m_ActiveGravity <= 0f && m_CharacterController.isGrounded)
			{
				flag = true;
			}
			if (flag)
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
			CollisionFlags collisionFlags = m_CharacterController.Move(MoveDirection);
			if (m_ActiveGravity < 0f)
			{
				collisionFlags = m_CharacterController.Move(Vector3.up * -0.01f);
			}
			if (collisionFlags == CollisionFlags.Above && m_ActiveGravity > 0f)
			{
				m_ActiveGravity = 0f;
			}
			CurrentPosition = m_CharacterController.transform.position;
		}
	}

	private void GetPhysics()
	{
		if (!(m_ExternalForce.magnitude <= 0f))
		{
			m_CharacterController.Move(m_ExternalForce * Time.deltaTime);
			m_ExternalForce = Vector3.MoveTowards(m_ExternalForce, Vector3.zero, 0.95f);
		}
	}

	private void GetGrounding()
	{
		if (m_CharacterController.isGrounded && !PreviouslyGrounded && m_ActiveGravity <= 0f)
		{
			m_ActiveGravity = 0f;
		}
		if (PreviouslyGrounded)
		{
			m_InitialAirPosition = m_CharacterController.transform.position;
		}
		PreviouslyGrounded = m_CharacterController.isGrounded;
	}

	public void SetPreviouslyGrounded(bool active)
	{
		PreviouslyGrounded = active;
	}

	private void GetCrouch()
	{
		if (m_CharacterController.isGrounded && CrouchInput)
		{
			IsCrouched = !IsCrouched;
		}
	}

	public void CancelMovement()
	{
		MovementInput = Vector3.zero;
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

	public void AddForce(Vector3 force)
	{
		m_ExternalForce += force;
	}

	public void SetCrouchInput(bool isCrouched)
	{
		IsCrouched = isCrouched;
	}

	protected override void OnDisposed()
	{
		m_CharacterController = null;
		base.OnDisposed();
	}
}
