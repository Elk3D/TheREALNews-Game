using System;
using UnityEngine;

[Serializable]
public class PlayerInteraction : JDisposable
{
	[SerializeField]
	private bool m_IsActive = true;

	[SerializeField]
	private LayerMask m_IgnoreLayers;

	[SerializeField]
	private float m_LookDistance = 2f;

	[SerializeField]
	private float m_SphereCastThickness = 0.1f;

	[SerializeField]
	private bool DebugLines = true;

	public bool IsActive => m_IsActive;

	public RaycastHit Hit { get; private set; }

	public Interactable Interactable { get; private set; }

	public event EventHandler OnInteract;

	public void ResetInteraction()
	{
		if (Interactable != null)
		{
			ExitInteraction(Interactable.transform.position);
		}
		Interactable = null;
	}

	public void Update(Transform sender)
	{
		if (m_IsActive && !base.IsDisposed && !GameManager.Instance.IsPaused)
		{
			InternalUpdate(sender.position, sender.forward, m_LookDistance);
		}
	}

	private void InternalUpdate(Vector3 origin, Vector3 direction, float distance)
	{
		if (Physics.SphereCast(origin, m_SphereCastThickness, direction, out var hitInfo, distance, ~(int)m_IgnoreLayers))
		{
			Hit = hitInfo;
			Interactable component = Hit.transform.root.GetComponent<Interactable>();
			Interactable interactable = Hit.transform.GetComponent<Interactable>();
			if (interactable == null)
			{
				interactable = ((!(component == null)) ? component : Hit.transform.GetComponentInParent<Interactable>());
			}
			if (interactable != null)
			{
				DrawDebugLine(origin, Hit.point, Color.yellow);
				if (Interactable != interactable)
				{
					ExitInteraction(origin);
					Interactable = interactable;
					EnterInteraction(origin);
				}
				else if ((Interactable.PlayerInputType == PlayerInputType.InteractOnPressed || Interactable.PlayerInputType == PlayerInputType.InteractOnReleased) && PlayerInput.InteractOnReleased())
				{
					Interact(origin);
				}
				else if (Interactable.PlayerInputType == PlayerInputType.SpecialAction && PlayerInput.SpecialAction())
				{
					Interact(origin);
				}
				else if (Interactable.PlayerInputType == PlayerInputType.SpecialActionReleased && PlayerInput.SpecialActionReleased())
				{
					Interact(origin);
				}
				else if (!interactable.EnterCheck(origin, Hit))
				{
					ExitInteraction(origin);
				}
				return;
			}
		}
		ExitInteraction(origin);
	}

	private void Interact(Vector3 origin)
	{
		if (Interactable != null)
		{
			Interactable.Interact(origin, Hit, this);
			this.OnInteract.Send(this);
		}
	}

	private void EnterInteraction(Vector3 origin)
	{
		if (!(Interactable == null) && !Interactable.Enter(origin, Hit, this))
		{
			ExitInteraction(origin);
		}
	}

	private void ExitInteraction(Vector3 origin)
	{
		Interactable?.Exit(origin, Hit, this);
		Interactable = null;
	}

	public void SetActive(bool active)
	{
		m_IsActive = active;
	}

	protected override void OnDisposed()
	{
		Interactable = null;
		base.OnDisposed();
	}

	private void DrawDebugLine(Vector3 start, Vector3 end, Color color)
	{
	}
}
