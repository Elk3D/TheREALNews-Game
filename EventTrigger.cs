using UnityEngine;

public class EventTrigger : ActionEvent
{
	[Header("Trigger Layer")]
	[SerializeField]
	[TagSelector]
	private string m_TriggerTag = "Player";

	public Collider Collider { get; protected set; }

	public Collider[] Colliders { get; protected set; }

	public override void Awake()
	{
		base.gameObject.layer = LayerMask.NameToLayer("EventTrigger");
		InternalGetCollider();
	}

	protected void InternalGetCollider()
	{
		Collider = GetComponent<Collider>();
		if (Collider != null && !Collider.isTrigger)
		{
			Collider.isTrigger = true;
		}
		Colliders = GetComponentsInChildren<Collider>();
		if (Colliders == null)
		{
			return;
		}
		for (int i = 0; i < Colliders.Length; i++)
		{
			if (!Colliders[i].isTrigger)
			{
				Colliders[i].isTrigger = true;
			}
		}
	}

	public override void SetActive(bool active)
	{
		if (Collider != null)
		{
			Collider.enabled = (m_IsActive = active);
		}
		if (Colliders != null)
		{
			for (int i = 0; i < Colliders.Length; i++)
			{
				Colliders[i].enabled = (m_IsActive = active);
			}
		}
		InternalSetActive(active);
	}

	protected virtual void InternalSetActive(bool active)
	{
	}

	private void OnTriggerEnter(Collider col)
	{
		if (CanTrigger(_isTriggered: false, col) && InternalEnterCheck(col))
		{
			ActivateAction(true, delegate
			{
				OnInternalEnter(col);
			}, delegate
			{
				SendOnEnter(this);
				SendOnInteract(this);
			});
			if (m_IsSingleAction)
			{
				Clear();
			}
		}
	}

	protected virtual bool InternalEnterCheck(Collider col)
	{
		return true;
	}

	protected virtual void OnInternalEnter(Collider col)
	{
	}

	private void OnTriggerExit(Collider col)
	{
		if (CanTrigger(_isTriggered: true, col) && InternalExitCheck(col) && !m_IsSingleAction)
		{
			ActivateAction(false, delegate
			{
				OnInternalExit(col);
			}, delegate
			{
				SendOnExit(this);
			});
		}
	}

	protected virtual bool InternalExitCheck(Collider col)
	{
		return true;
	}

	protected virtual void OnInternalExit(Collider col)
	{
	}

	protected virtual bool CanTrigger(bool _isTriggered, Collider col)
	{
		if (m_IsActive && base.IsActioned == _isTriggered && CheckTag(col.tag))
		{
			return InternalCanTrigger(col);
		}
		return false;
	}

	protected virtual bool CheckTag(string tag)
	{
		return tag == m_TriggerTag;
	}

	protected virtual bool InternalCanTrigger(Collider col)
	{
		return true;
	}

	protected override void OnDisposed()
	{
		Collider = null;
		Colliders = null;
		base.OnDisposed();
	}
}
