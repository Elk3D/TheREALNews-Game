using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheRealNews.Core
{
    /// <summary>
    /// Lightweight game-wide singleton. Holds the Player reference, the run-state
    /// metrics (approval %, docs processed, words redacted, quota) and the current
    /// phase. Adapted scripts (CameraHeadBob, LightFlicker, etc.) read IsPaused
    /// and Player from here instead of the original heavyweight GameManager.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class GameManager : MonoBehaviour
    {
        public enum Phase
        {
            Office,     // Phase 1 — 3D walk-in
            Documents,  // Phase 2 — 2D UI loop
            GameOver
        }

        public static GameManager Instance { get; private set; }

        [Header("Refs")]
        [SerializeField] private Player.Player m_Player;

        [Header("Run State")]
        [SerializeField] private float m_StartingApproval = 100f;
        [SerializeField] private float m_GameOverThreshold = 40f;
        [SerializeField] private int m_DailyQuota = 8;

        public Player.Player Player => m_Player;
        public bool IsPaused { get; private set; }
        public Phase CurrentPhase { get; private set; } = Phase.Office;

        // Metrics
        public float Approval { get; private set; }
        public int DocumentsProcessed { get; private set; }
        public int WordsRedacted { get; private set; }
        public int DailyQuota => m_DailyQuota;
        public float GameOverThreshold => m_GameOverThreshold;

        // Events for UI hookup
        public event Action<Phase> OnPhaseChanged;
        public event Action OnMetricsChanged;
        public event Action OnGameOver;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            Approval = m_StartingApproval;

            if (m_Player == null)
                m_Player = FindObjectOfType<Player.Player>();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void SetPaused(bool paused) => IsPaused = paused;

        public void LockPause()   => IsPaused = true;
        public void UnlockPause() => IsPaused = false;

        public void EnterPhase(Phase phase)
        {
            if (CurrentPhase == phase) return;
            CurrentPhase = phase;
            OnPhaseChanged?.Invoke(phase);
        }

        /// <summary>
        /// Called by DocumentManager after a submission. Spin score is the
        /// average "spin" of the chosen replacements (0 = neutral, 1 = on-message).
        /// We translate that into an approval delta.
        /// </summary>
        public void RecordSubmission(int wordsRedactedThisDoc, float spinScore01)
        {
            DocumentsProcessed++;
            WordsRedacted += wordsRedactedThisDoc;

            // Spin in [0,1] -> delta in roughly [-6, +2]. Bad spin hurts more
            // than good spin helps; that's the squeeze the satire lives in.
            float delta = Mathf.Lerp(-6f, 2f, spinScore01);
            Approval = Mathf.Clamp(Approval + delta, 0f, 100f);

            OnMetricsChanged?.Invoke();

            if (Approval < m_GameOverThreshold)
                TriggerGameOver();
        }

        public void TriggerGameOver()
        {
            EnterPhase(Phase.GameOver);
            OnGameOver?.Invoke();
        }

        public void RestartRun()
        {
            Approval = m_StartingApproval;
            DocumentsProcessed = 0;
            WordsRedacted = 0;
            CurrentPhase = Phase.Office;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
