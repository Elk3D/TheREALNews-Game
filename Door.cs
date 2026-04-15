using System;
using DG.Tweening;
using UnityEngine;

public class Door : JMonoBehaviour
{
	[Header("Events")]
	[SerializeField]
	private ActionEvent m_ActionEvent;

	[Header("Door Positions")]
	[SerializeField]
	private Transform m_Hinge;

	[SerializeField]
	private Transform m_OpenLocation;

	[SerializeField]
	private Transform m_CloseLocation;

	[Header("Door Settings")]
	[SerializeField]
	private float m_SpeedOpen = 1f;

	[SerializeField]
	private Ease m_EaseOpen = Ease.InOutSine;

	[SerializeField]
	private float m_SpeedClose = 0.5f;

	[SerializeField]
	private Ease m_EaseClose = Ease.Linear;

	[SerializeField]
	private bool m_StartLocked;

	[Header("Audio - TEMP")]
	[SerializeField]
	private AudioClip[] m_OpenClips;

	[SerializeField]
	private AudioClip[] m_CloseClips;

	[SerializeField]
	private AudioClip[] m_LockedClips;

	[SerializeField]
	private AudioSource m_AudioSource;

	private Sequence m_Sequence;

	public bool IsOpen { get; private set; }

	public bool IsLocked { get; private set; }

	public void Lock()
	{
		IsLocked = true;
	}

	public void Unlock()
	{
		IsLocked = false;
	}

	public override void Start()
	{
		IsLocked = m_StartLocked;
		if (m_ActionEvent != null)
		{
			m_ActionEvent.OnInteract += HandleActionEventOnInteract;
		}
	}

	private void HandleActionEventOnInteract(object sender, EventArgs e)
	{
		if (IsOpen)
		{
			Close();
		}
		else
		{
			Open();
		}
	}

	public void Open()
	{
		if (!IsOpen)
		{
			DoAction(ref m_OpenLocation, ref m_OpenClips[UnityEngine.Random.Range(0, m_OpenClips.Length)]);
		}
	}

	public void Close()
	{
		if (IsOpen)
		{
			DoAction(ref m_CloseLocation, ref m_CloseClips[UnityEngine.Random.Range(0, m_CloseClips.Length)]);
		}
	}

	public void Locked()
	{
		if (!IsOpen)
		{
			m_AudioSource.PlayOneShot(m_LockedClips[UnityEngine.Random.Range(0, m_LockedClips.Length)]);
		}
	}

	private void DoAction(ref Transform toLocation, ref AudioClip audio)
	{
		if (IsLocked)
		{
			Locked();
			return;
		}
		if (m_ActionEvent != null)
		{
			m_ActionEvent.SetActive(active: false);
		}
		bool isOpen = toLocation == m_OpenLocation;
		float duration = (isOpen ? m_SpeedOpen : m_SpeedClose);
		Ease ease = (isOpen ? m_EaseOpen : m_EaseClose);
		ResetSequence();
		if (m_Hinge.position != toLocation.position)
		{
			m_Sequence.Insert(0f, m_Hinge.DOMove(toLocation.position, duration).SetEase(ease));
		}
		if (m_Hinge.rotation != toLocation.rotation)
		{
			m_Sequence.Insert(0f, m_Hinge.DORotateQuaternion(toLocation.rotation, duration).SetEase(ease));
		}
		m_Sequence.OnComplete(delegate
		{
			IsOpen = isOpen;
			if (!IsOpen && m_ActionEvent != null)
			{
				m_ActionEvent.SetActive(active: true);
				m_ActionEvent.ResetAction();
			}
		});
		if (audio != null)
		{
			m_AudioSource.PlayOneShot(audio);
		}
	}

	public void SetActive(bool active)
	{
		if (!(m_ActionEvent == null))
		{
			m_ActionEvent.SetActive(active);
		}
	}

	public void ResetAction()
	{
		if (!(m_ActionEvent == null))
		{
			m_ActionEvent.ResetAction();
		}
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

	protected override void OnDisposed()
	{
		if (m_ActionEvent != null)
		{
			m_ActionEvent.OnInteract -= HandleActionEventOnInteract;
		}
		KillSequence();
		base.OnDisposed();
	}
}
