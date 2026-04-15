using DG.Tweening;
using UnityEngine;

namespace TheRealNews.World
{
    /// <summary>
    /// DOTween-driven hinge door. Triggered by an attached EventTrigger
    /// (toggle on enter). Replaces JMonoBehaviour + ActionEvent with plain
    /// MonoBehaviour + UnityEvent hookup.
    /// </summary>
    public class Door : MonoBehaviour
    {
        [Header("Trigger (optional)")]
        [SerializeField] private EventTrigger m_Trigger;

        [Header("Door Positions")]
        [SerializeField] private Transform m_Hinge;
        [SerializeField] private Transform m_OpenLocation;
        [SerializeField] private Transform m_CloseLocation;

        [Header("Door Settings")]
        [SerializeField] private float m_SpeedOpen = 1f;
        [SerializeField] private Ease  m_EaseOpen  = Ease.InOutSine;
        [SerializeField] private float m_SpeedClose = 0.5f;
        [SerializeField] private Ease  m_EaseClose  = Ease.Linear;
        [SerializeField] private bool  m_StartLocked;

        [Header("Audio")]
        [SerializeField] private AudioSource m_AudioSource;
        [SerializeField] private AudioClip[] m_OpenClips;
        [SerializeField] private AudioClip[] m_CloseClips;
        [SerializeField] private AudioClip[] m_LockedClips;

        private Sequence m_Sequence;

        public bool IsOpen   { get; private set; }
        public bool IsLocked { get; private set; }

        public void Lock()   => IsLocked = true;
        public void Unlock() => IsLocked = false;

        private void Start()
        {
            IsLocked = m_StartLocked;
            if (m_Trigger != null) m_Trigger.OnEnter.AddListener(Toggle);
        }

        private void OnDestroy()
        {
            if (m_Trigger != null) m_Trigger.OnEnter.RemoveListener(Toggle);
            KillSequence();
        }

        public void Toggle()
        {
            if (IsOpen) Close(); else Open();
        }

        public void Open()
        {
            if (!IsOpen) DoAction(m_OpenLocation, PickClip(m_OpenClips));
        }

        public void Close()
        {
            if (IsOpen) DoAction(m_CloseLocation, PickClip(m_CloseClips));
        }

        public void Locked()
        {
            if (IsOpen) return;
            var clip = PickClip(m_LockedClips);
            if (clip != null && m_AudioSource != null) m_AudioSource.PlayOneShot(clip);
        }

        private void DoAction(Transform target, AudioClip audio)
        {
            if (target == null) return;

            if (IsLocked)
            {
                Locked();
                return;
            }

            bool opening = target == m_OpenLocation;
            float duration = opening ? m_SpeedOpen : m_SpeedClose;
            Ease ease      = opening ? m_EaseOpen  : m_EaseClose;

            ResetSequence();

            if (m_Hinge.position != target.position)
                m_Sequence.Insert(0f, m_Hinge.DOMove(target.position, duration).SetEase(ease));

            if (m_Hinge.rotation != target.rotation)
                m_Sequence.Insert(0f, m_Hinge.DORotateQuaternion(target.rotation, duration).SetEase(ease));

            m_Sequence.OnComplete(() => IsOpen = opening);

            if (audio != null && m_AudioSource != null) m_AudioSource.PlayOneShot(audio);
        }

        private static AudioClip PickClip(AudioClip[] clips)
        {
            if (clips == null || clips.Length == 0) return null;
            return clips[Random.Range(0, clips.Length)];
        }

        private void ResetSequence()
        {
            KillSequence();
            m_Sequence = DOTween.Sequence();
        }

        private void KillSequence()
        {
            if (m_Sequence != null)
            {
                m_Sequence.Kill();
                m_Sequence = null;
            }
        }
    }
}
