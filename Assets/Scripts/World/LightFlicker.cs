using System;
using UnityEngine;
using TheRealNews.Core;

namespace TheRealNews.World
{
    /// <summary>
    /// 6 wave types for a Light's intensity multiplier. Use NOISE for the
    /// classic fluorescent flicker. Adapted from JMonoBehaviour to plain
    /// MonoBehaviour and made GameManager-optional.
    /// </summary>
    [RequireComponent(typeof(Light))]
    public class LightFlicker : MonoBehaviour
    {
        public enum WaveType { SIN, TRI, SQR, SAW, INV, NOISE }

        [SerializeField] private bool m_IsOff;

        [Header("Flicker Settings")]
        [SerializeField] private WaveType m_WaveType;
        [SerializeField] private float m_Base;
        [SerializeField] private float m_Amplitude;
        [SerializeField] private float m_Phase;
        [SerializeField] private float m_Frequency;

        private Color m_OriginalColor;
        private float m_OriginalIntensity;
        private bool  m_IsConnected;

        public Color OriginalColor => m_OriginalColor;
        public Light Light { get; private set; }
        public bool  IsOff => m_IsOff;
        public float WaveValue { get; private set; }

        private void Awake()
        {
            Light = GetComponent<Light>();
            m_OriginalColor     = Light.color;
            m_OriginalIntensity = Light.intensity;
        }

        private void Update()
        {
            if (m_IsOff || Light == null || m_IsConnected) return;
            if (!Light.enabled || !Light.gameObject.activeInHierarchy) return;

            var gm = GameManager.Instance;
            if (gm != null && gm.IsPaused) return;

            UpdateLight(EvalWave());
        }

        public void SetConnected(bool isConnected) => m_IsConnected = isConnected;

        public void TurnOn(bool light = false)
        {
            m_IsOff = light;
            if (Light != null)
            {
                Light.color     = m_OriginalColor;
                Light.intensity = m_OriginalIntensity;
            }
        }

        public void TurnOff(bool light = false)
        {
            m_IsOff = true;
            if (Light == null) return;
            Light.color     = m_OriginalColor;
            Light.intensity = light ? 0f : m_OriginalIntensity;
        }

        public void UpdateLight(float value)
        {
            Light.color = value < 0f ? m_OriginalColor * 0f : m_OriginalColor * value;
        }

        private float EvalWave()
        {
            float t = (Time.time + m_Phase) * m_Frequency;
            t -= Mathf.Floor(t);

            float wave = m_WaveType switch
            {
                WaveType.SIN   => Mathf.Sin(t * 2f * MathF.PI),
                WaveType.TRI   => t < 0.5f ? (4f * t - 1f) : (-4f * t + 3f),
                WaveType.SQR   => t < 0.5f ? 1f : -1f,
                WaveType.SAW   => t,
                WaveType.INV   => 1f - t,
                WaveType.NOISE => 1f - UnityEngine.Random.value * 2f,
                _              => 1f,
            };

            return WaveValue = wave * m_Amplitude + m_Base;
        }

        public void SetColor(Color color) => m_OriginalColor = color;
    }
}
