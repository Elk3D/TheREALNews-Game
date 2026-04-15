using System;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class LightFlicker : JMonoBehaviour
{
	private enum WaveType
	{
		SIN,
		TRI,
		SQR,
		SAW,
		INV,
		NOISE
	}

	[SerializeField]
	private bool m_IsOff;

	[Header("Flicker Settings")]
	[SerializeField]
	private WaveType m_WaveType;

	[SerializeField]
	private float m_Base;

	[SerializeField]
	private float m_Amplitude;

	[SerializeField]
	private float m_Phase;

	[SerializeField]
	private float m_Frequency;

	private Color m_OriginalColor;

	private float m_OrigintalIntensity;

	private bool m_IsConnected;

	public Color OriginalColor => m_OriginalColor;

	public Light Light { get; private set; }

	public bool IsOff => m_IsOff;

	public float WaveValue { get; private set; }

	public override void Awake()
	{
		Light = GetComponent<Light>();
		m_OriginalColor = Light.color;
		m_OrigintalIntensity = Light.intensity;
	}

	private void Update()
	{
		if (!IsOff && !base.IsDisposed && !GameManager.Instance.IsPaused && !(Light == null) && !m_IsConnected && Light.enabled && Light.gameObject.activeInHierarchy)
		{
			UpdateLight(EvalWave());
		}
	}

	public void SetConnected(bool isConnected)
	{
		m_IsConnected = isConnected;
	}

	public void TurnOn(bool light = false)
	{
		if (light)
		{
			m_IsOff = true;
		}
		else
		{
			m_IsOff = false;
		}
		if (Light != null)
		{
			Light.color = m_OriginalColor;
			Light.intensity = m_OrigintalIntensity;
		}
	}

	public void TurnOff(bool light = false)
	{
		m_IsOff = true;
		if (light)
		{
			Light.color = m_OriginalColor;
			Light.intensity = 0f;
		}
		else if (Light != null)
		{
			Light.color = m_OriginalColor;
			Light.intensity = m_OrigintalIntensity;
		}
	}

	public void UpdateLight(float value)
	{
		if (value < 0f)
		{
			Light.color = m_OriginalColor * 0f;
		}
		else
		{
			Light.color = m_OriginalColor * value;
		}
	}

	private float EvalWave()
	{
		float num = (Time.time + m_Phase) * m_Frequency;
		float num2 = 0f;
		num -= Mathf.Floor(num);
		return WaveValue = m_WaveType switch
		{
			WaveType.SIN => Mathf.Sin(num * 2f * MathF.PI), 
			WaveType.TRI => (!(num < 0.5f)) ? (-4f * num + 3f) : (4f * num - 1f), 
			WaveType.SQR => (!(num < 0.5f)) ? (-1f) : 1f, 
			WaveType.SAW => num, 
			WaveType.INV => 1f - num, 
			WaveType.NOISE => 1f - UnityEngine.Random.value * 2f, 
			_ => 1f, 
		} * m_Amplitude + m_Base;
	}

	public void SetColor(Color color)
	{
		m_OriginalColor = color;
	}

	protected override void OnDisposed()
	{
		Light = null;
		base.OnDisposed();
	}
}
