using System;
using UnityEngine;

[Serializable]
public class CameraHeadBob
{
	[SerializeField]
	private bool m_Enable = true;

	[Header("Settings")]
	[SerializeField]
	private float m_WalkBobSpeed = 14f;

	[SerializeField]
	private float m_RunBobSpeed = 18f;

	[SerializeField]
	private float m_WalkBobAmount = 0.05f;

	[SerializeField]
	private float m_RunBobAmount = 0.1f;

	[SerializeField]
	private float m_HorizontalBobScale = 0.5f;

	[SerializeField]
	private float m_MidpointY;

	private CharacterController m_CharacterController;

	private Transform m_CameraHolder;

	private float m_Timer = MathF.PI / 2f;

	private Vector3 m_OriginalLocalPos;

	public void Initialize(CharacterController characterController, Transform cameraHolder)
	{
		m_CharacterController = characterController;
		m_CameraHolder = cameraHolder;
		m_OriginalLocalPos = m_CameraHolder.localPosition;
		m_MidpointY = m_OriginalLocalPos.y;
		if (PlayerPrefs.GetInt("SETTINGS_HEAD_BOB", 1) == 0)
		{
			Disable();
		}
	}

	public void Update()
	{
		if (m_Enable)
		{
			Vector3 zero = Vector3.zero;
			float magnitude = m_CharacterController.velocity.magnitude;
			if (m_CharacterController.isGrounded && magnitude > 0.1f)
			{
				bool isRunning = GameManager.Instance.Player.PlayerMovement.IsRunning;
				float num = (isRunning ? m_RunBobSpeed : m_WalkBobSpeed);
				float num2 = (isRunning ? m_RunBobAmount : m_WalkBobAmount);
				m_Timer += Time.deltaTime * num;
				zero.y = Mathf.Cos(m_Timer) * num2;
				zero.x = Mathf.Sin(m_Timer / 2f) * num2 * m_HorizontalBobScale;
				m_CameraHolder.localPosition = m_OriginalLocalPos + zero;
			}
			else
			{
				m_Timer = MathF.PI / 2f;
				zero.y = Mathf.Lerp(m_CameraHolder.localPosition.y, m_MidpointY, Time.deltaTime * 8f);
				zero.x = Mathf.Lerp(m_CameraHolder.localPosition.x, m_OriginalLocalPos.x, Time.deltaTime * 8f);
				m_CameraHolder.localPosition = new Vector3(zero.x, zero.y, m_OriginalLocalPos.z);
			}
		}
	}

	public void Enable()
	{
		m_Enable = true;
	}

	public void Disable()
	{
		m_Enable = false;
		m_CameraHolder.localPosition = m_OriginalLocalPos;
	}
}
