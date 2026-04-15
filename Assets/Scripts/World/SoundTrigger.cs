using UnityEngine;

namespace TheRealNews.World
{
    /// <summary>
    /// Plays a random clip from a list. Hook Play() to an EventTrigger.OnEnter
    /// or to an Animation Event.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class SoundTrigger : MonoBehaviour
    {
        [SerializeField] private AudioClip[] m_Clips;
        [SerializeField] private AudioSource m_AudioSource;

        private void Awake()
        {
            if (m_AudioSource == null) m_AudioSource = GetComponent<AudioSource>();
        }

        public void Play()
        {
            if (m_Clips == null || m_Clips.Length == 0 || m_AudioSource == null) return;
            m_AudioSource.PlayOneShot(m_Clips[Random.Range(0, m_Clips.Length)]);
        }
    }
}
