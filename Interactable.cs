using UnityEngine;

public class Interactable : ActionEvent
{
	[SerializeField]
	protected PlayerInputType m_PlayerInputType = PlayerInputType.InteractOnReleased;

	[SerializeField]
	private bool m_ResetOnInteract;

	[SerializeField]
	protected Vector2 m_CustomRumble;

	public PlayerInputType PlayerInputType => m_PlayerInputType;

	public Vector2 CustomRumble => m_CustomRumble;

	public bool Enter(Vector3 origin, RaycastHit hit, object sender = null)
	{
		if (!m_IsActive || base.IsActioned || !InternalEnterCheck(origin, hit, sender))
		{
			return false;
		}
		ActivateAction(base.IsActioned, delegate
		{
			OnInternalEnter(origin, hit, sender);
		}, delegate
		{
			SendOnEnter(this);
		});
		return true;
	}

	public bool EnterCheck(Vector3 origin, RaycastHit hit, object sender = null)
	{
		return InternalEnterCheck(origin, hit, sender);
	}

	protected virtual bool InternalEnterCheck(Vector3 origin, RaycastHit hit, object sender = null)
	{
		return true;
	}

	protected virtual void OnInternalEnter(Vector3 origin, RaycastHit hit, object sender = null)
	{
	}

	public bool Exit(Vector3 origin, RaycastHit hit, object sender = null)
	{
		if (!m_IsActive || base.IsActioned || !InternalExitCheck(origin, hit, sender))
		{
			return false;
		}
		ActivateAction(base.IsActioned, delegate
		{
			OnInternalExit(origin, hit, sender);
		}, delegate
		{
			SendOnExit(this);
		});
		return true;
	}

	protected virtual bool InternalExitCheck(Vector3 origin, RaycastHit hit, object sender = null)
	{
		return true;
	}

	protected virtual void OnInternalExit(Vector3 origin, RaycastHit hit, object sender = null)
	{
	}

	public bool Interact(Vector3 origin, RaycastHit hit, object sender = null)
	{
		if (!m_IsActive || base.IsActioned || !InternalInteractCheck(origin, hit, sender))
		{
			return false;
		}
		ActivateAction(true, delegate
		{
			OnInternalInteract(origin, hit, sender);
		}, delegate
		{
			SendOnInteract(this);
		});
		if (m_IsSingleAction)
		{
			Clear();
		}
		else if (m_ResetOnInteract)
		{
			ResetAction();
		}
		return true;
	}

	protected virtual bool InternalInteractCheck(Vector3 origin, RaycastHit hit, object sender = null)
	{
		return true;
	}

	protected virtual void OnInternalInteract(Vector3 origin, RaycastHit hit, object sender = null)
	{
	}
}
