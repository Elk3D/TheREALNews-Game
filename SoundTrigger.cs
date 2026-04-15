using UnityEngine;

public class SoundTrigger : MonoBehaviour
{
	[SerializeField]
	private AudioClip[] Clips;

	[SerializeField]
	private AudioSource audioSource;

	private void Start()
	{
	}

	private void Update()
	{
	}

	private void Awake()
	{
		audioSource = GetComponent<AudioSource>();
	}

	private void SFX()
	{
		AudioClip randomClip = GetRandomClip();
		audioSource.PlayOneShot(randomClip);
	}

	private AudioClip GetRandomClip()
	{
		return Clips[Random.Range(0, Clips.Length)];
	}
}
