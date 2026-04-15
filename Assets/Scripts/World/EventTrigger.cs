using UnityEngine;
using UnityEngine.Events;

namespace TheRealNews.World
{
    /// <summary>
    /// Collider-trigger that fires UnityEvents on enter/exit. Replaces the
    /// decompiled ActionEvent base class with a plain MonoBehaviour. Used to
    /// detect the player reaching the desk in Phase 1.
    /// </summary>
    [DisallowMultipleComponent]
    public class EventTrigger : MonoBehaviour
    {
        [Header("Trigger")]
        [SerializeField] private string m_TriggerTag = "Player";
        [SerializeField] private bool m_SingleUse = true;
        [SerializeField] private bool m_Active = true;

        [Header("Events")]
        public UnityEvent OnEnter;
        public UnityEvent OnExit;

        public bool HasFired { get; private set; }

        private void Awake()
        {
            ForceTriggers(transform);
        }

        private void ForceTriggers(Transform root)
        {
            var col = root.GetComponent<Collider>();
            if (col != null && !col.isTrigger) col.isTrigger = true;

            foreach (var c in root.GetComponentsInChildren<Collider>(true))
                if (!c.isTrigger) c.isTrigger = true;
        }

        public void SetActive(bool active) => m_Active = active;
        public void ResetTrigger()         => HasFired = false;

        private void OnTriggerEnter(Collider other)
        {
            if (!m_Active || (m_SingleUse && HasFired)) return;
            if (!string.IsNullOrEmpty(m_TriggerTag) && !other.CompareTag(m_TriggerTag)) return;

            HasFired = true;
            OnEnter?.Invoke();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!m_Active) return;
            if (!string.IsNullOrEmpty(m_TriggerTag) && !other.CompareTag(m_TriggerTag)) return;
            OnExit?.Invoke();
        }
    }
}
