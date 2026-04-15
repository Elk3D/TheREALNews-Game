using System;
using UnityEngine;

[Serializable]
public class PlayerLook : JDisposable
{
	[SerializeField]
	private float m_Sensitivity = 0.5f;

	[SerializeField]
	private bool m_ClampVerticalRotation = true;

	[SerializeField]
	private float m_VerticalMinClamp = -82f;

	[SerializeField]
	private float m_VerticalMaxClamp = 82f;

	private Quaternion m_CharacterTargetRotation;

	private Quaternion m_CameraTargetRotation;

	private bool m_IsRotationInitialized;

	private bool m_IsInitialVerticalClampInitialized;

	private float m_HorizontalClamp;

	private float m_InitialVerticalMaxClamp;

	private float m_InitialVerticalMinClamp;

	private float turnSpeedBoostTimer;

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

	public void ForceRotation(Quaternion rotation)
	{
		m_CharacterTargetRotation = rotation;
	}

	public void ForceCameraRotation(Quaternion rotation)
	{
		m_CameraTargetRotation = rotation;
	}

	public void GetInput()
	{
		float tSpeed = -0.5f;
		if (turnSpeedBoostTimer > 0.3f)
		{
			tSpeed = 0f;
		}
		float num = PlayerInput.LookX(tSpeed);
		if (Mathf.Abs(num) > 0.1f)
		{
			turnSpeedBoostTimer += Time.deltaTime;
		}
		else
		{
			turnSpeedBoostTimer = 0f;
		}
		m_InputX = num * m_Sensitivity;
		m_InputY = (0f - PlayerInput.LookY()) * m_Sensitivity;
	}

	public void Rotation(Transform character)
	{
		Rotation(character, null, hasGravity: true);
	}

	public void Rotation(Transform character, bool hasGravity)
	{
		Rotation(character, null, hasGravity);
	}

	public void Rotation(Transform character, params Transform[] cameras)
	{
		for (int i = 0; i < cameras.Length; i++)
		{
			if ((bool)cameras[i])
			{
				Rotation(character, cameras[i], hasGravity: true);
			}
		}
	}

	public void Rotation(Transform character, Transform camera)
	{
		Rotation(character, camera, hasGravity: true);
	}

	public void Rotation(Transform character, Transform camera, bool hasGravity)
	{
		if (!IsNullRotation(m_InputX, m_InputY))
		{
			if (hasGravity)
			{
				m_CharacterTargetRotation *= Quaternion.Euler(0f, m_InputX, 0f);
				if (hasHorizontalLock)
				{
					m_CharacterTargetRotation = ClampRotationYAxis(m_CharacterTargetRotation, 0f - m_HorizontalClamp, m_HorizontalClamp);
				}
				character.localRotation = m_CharacterTargetRotation;
				if ((bool)camera)
				{
					m_CameraTargetRotation *= Quaternion.Euler(m_InputY, 0f, 0f);
					if (m_ClampVerticalRotation)
					{
						m_CameraTargetRotation = ClampRotationXAxis(m_CameraTargetRotation, m_VerticalMinClamp, m_VerticalMaxClamp);
					}
					camera.localRotation = m_CameraTargetRotation;
					Vector3 localEulerAngles = camera.localEulerAngles;
					localEulerAngles.z = 0f;
					camera.localEulerAngles = localEulerAngles;
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
			if ((bool)camera)
			{
				camera.localRotation = m_CameraTargetRotation;
			}
		}
		UpdateCursorLock();
	}

	public void SmoothLook(Transform character, Transform camera, Vector3 newDirection)
	{
		Quaternion b = Quaternion.LookRotation(newDirection);
		character.localRotation = Quaternion.Lerp(character.rotation, b, 2f * Time.deltaTime);
		Vector3 localEulerAngles = new Vector3(0f, character.localEulerAngles.y, 0f);
		character.localEulerAngles = localEulerAngles;
		camera.localRotation = Quaternion.Lerp(camera.rotation, b, 2f * Time.deltaTime);
		Quaternion localRotation = camera.localRotation;
		localRotation = ClampRotationXAxis(localRotation, m_VerticalMinClamp, m_VerticalMaxClamp);
		localRotation = ClampRotationYAxis(localRotation, 0f - m_HorizontalClamp, m_HorizontalClamp);
		camera.localRotation = localRotation;
		Vector3 localEulerAngles2 = camera.localEulerAngles;
		localEulerAngles2.z = 0f;
		camera.localEulerAngles = localEulerAngles2;
	}

	public void ResetRotation(Transform character)
	{
		ResetRotation(character, null);
	}

	public void ResetRotation(Transform character, Transform camera)
	{
		m_CharacterTargetRotation.eulerAngles = new Vector3(0f, character.localRotation.eulerAngles.y, 0f);
		if ((bool)camera)
		{
			m_CameraTargetRotation.eulerAngles = new Vector3(camera.localRotation.eulerAngles.x, 0f, 0f);
		}
	}

	public void ResetVerticalClamp()
	{
		m_VerticalMinClamp = m_InitialVerticalMinClamp;
		m_VerticalMaxClamp = m_InitialVerticalMaxClamp;
	}

	public void SetVerticalClamp(float clamp)
	{
		m_VerticalMinClamp = 0f - clamp;
		m_VerticalMaxClamp = clamp;
	}

	public void SetHorizontalClamp(float clamp)
	{
		m_HorizontalClamp = clamp;
	}

	public void HorizontalClampSetActive(bool active)
	{
		hasHorizontalLock = active;
	}

	public void UpdateCursorLock()
	{
		InternalLockUpdate();
	}

	private void InternalLockUpdate()
	{
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	private bool IsNullRotation(float horizontal, float vertical)
	{
		if (horizontal == 0f && vertical == 0f)
		{
			return true;
		}
		if (!m_IsRotationInitialized)
		{
			return m_IsRotationInitialized = true;
		}
		return false;
	}

	private Quaternion ClampRotationXAxis(Quaternion _quaternion, float minimum, float maximum)
	{
		_quaternion.x /= _quaternion.w;
		_quaternion.y /= _quaternion.w;
		_quaternion.z /= _quaternion.w;
		_quaternion.w = 1f;
		float value = 114.59156f * Mathf.Atan(_quaternion.x);
		value = Mathf.Clamp(value, minimum, maximum);
		_quaternion.x = Mathf.Tan(MathF.PI / 360f * value);
		return _quaternion;
	}

	private Quaternion ClampRotationYAxis(Quaternion _quaternion, float minimum, float maximum)
	{
		_quaternion.x /= _quaternion.w;
		_quaternion.y /= _quaternion.w;
		_quaternion.z /= _quaternion.w;
		_quaternion.w = 1f;
		float value = 114.59156f * Mathf.Atan(_quaternion.y);
		value = Mathf.Clamp(value, minimum, maximum);
		_quaternion.y = Mathf.Tan(MathF.PI / 360f * value);
		return _quaternion;
	}

	protected override void OnDisposed()
	{
		base.OnDisposed();
	}
}
