using System;
using UnityEngine;

namespace TheRealNews.Player
{
    /// <summary>
    /// Random footstep clip player driven by movement velocity. Renamed Update
    /// to Tick because Player drives it manually.
    /// </summary>
    [Serializable]
    public class PlayerFootsteps
    {
        [SerializeField] private bool m_Enable = true;

        [Header("Settings")]
        [SerializeField] private Transform m_FootstepAudioPosition;
        [SerializeField] private float m_WalkStepInterval = 0.75f;
        [SerializeField] private float m_RunStepInterval = 0.5f;

        [Header("Audio")]
        [SerializeField] private AudioSource m_AudioSource;
        [SerializeField] private AudioClip[] m_FootstepClips;

        private float m_MoveX;
        private float m_MoveY;
        private float m_StepCycle;
        private float m_NextStep;
        private float m_CurrentStepInterval;
        private int   m_LastClipIndex = -1;

        public void Tick(float magnitude, float currentSpeed, float moveX, float moveY, bool isRunning)
        {
            m_MoveX = moveX;
            m_MoveY = moveY;
            m_CurrentStepInterval = isRunning ? m_RunStepInterval : m_WalkStepInterval;
            ProgressStepCycle(magnitude, currentSpeed);
        }

        public void ProgressStepCycle(float magnitude, float speed)
        {
            if (!m_Enable) return;

            if (magnitude > 0f && (m_MoveX != 0f || m_MoveY != 0f))
                m_StepCycle += (magnitude + speed) * Time.deltaTime;

            if (m_StepCycle <= m_NextStep) return;

            m_NextStep = m_StepCycle + m_CurrentStepInterval;

            if (m_FootstepClips == null || m_FootstepClips.Length == 0 || m_AudioSource == null) return;

            int idx;
            do
            {
                idx = UnityEngine.Random.Range(0, m_FootstepClips.Length);
            }
            while (idx == m_LastClipIndex && m_FootstepClips.Length > 1);

            m_LastClipIndex = idx;
            m_AudioSource.PlayOneShot(m_FootstepClips[idx]);
        }

        public void Enable()  => m_Enable = true;
        public void Disable() => m_Enable = false;
    }
}
