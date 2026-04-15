using UnityEngine;

public class Player : JMonoBehaviour
{
	[Header("Containers")]
	[SerializeField]
	private Transform m_HeadContainer;

	[SerializeField]
	private Transform m_CameraContainer;

	[SerializeField]
	private Transform m_CameraHolder;

	[SerializeField]
	private Transform m_CameraTiltContainer;

	[Space]
	[SerializeField]
	private PlayerMovement m_PlayerMovement;

	[Space]
	[SerializeField]
	private PlayerLook m_PlayerLook;

	[Space]
	[SerializeField]
	private PlayerInteraction m_PlayerInteraction;

	[Space]
	[SerializeField]
	private CameraHeadBob m_CameraHeadBob;

	[Space]
	[SerializeField]
	private PlayerFootsteps m_PlayerFootsteps;

	[Space]
	[SerializeField]
	private CameraMovements m_CameraMovements;

	[Space]
	[SerializeField]
	private CameraTilt m_CameraTilt;

	private float defaultFOV = 60f;

	private float runFOV = 65f;

	private float fovLerpSpeed = 5f;

	private bool m_IsSecretCodeActive;

	private float m_SecretCodeTimer;

	private string m_SecretCode;

	public Transform HeadContainer => m_HeadContainer;

	public Transform CameraContainer => m_CameraContainer;

	public Transform CameraHolder => m_CameraHolder;

	public Transform CameraTiltContainer => m_CameraTiltContainer;

	public PlayerMovement PlayerMovement => m_PlayerMovement;

	public PlayerLook PlayerLook => m_PlayerLook;

	public PlayerInteraction PlayerInteraction => m_PlayerInteraction;

	public CameraHeadBob CameraHeadBob => m_CameraHeadBob;

	public PlayerFootsteps PlayerFootsteps => m_PlayerFootsteps;

	public CameraMovements CameraMovements => m_CameraMovements;

	public CameraTilt CameraTilt => m_CameraTilt;

	public CharacterController CharacterController { get; private set; }

	public GameCamera GameCamera { get; private set; }

	public bool IsInitialized { get; private set; }

	public bool IsLocked { get; private set; }

	public void Initialize()
	{
		if (!IsInitialized)
		{
			CharacterController = GetComponent<CharacterController>();
			m_PlayerMovement.Initialize(CharacterController);
			m_PlayerLook.Initialize(base.transform, m_HeadContainer);
			GameCamera = GetComponentInChildren<GameCamera>(includeInactive: true);
			if (GameCamera != null)
			{
				GameCamera.Initialize(m_CameraHolder, m_CameraContainer);
			}
			m_CameraHeadBob.Initialize(CharacterController, m_CameraHolder);
			m_CameraMovements.Initialize();
			m_CameraTilt.Initialize(m_CameraTiltContainer);
			IsInitialized = true;
		}
	}

	private void Update()
	{
		if (!GameManager.Instance.IsPaused && !base.IsDisposed)
		{
			SecretCodes();
			m_CameraMovements.Update();
			if (!IsLocked)
			{
				m_PlayerLook.GetInput();
				m_PlayerLook.UpdateCursorLock();
				m_PlayerMovement.UpdateMovementInput();
				m_PlayerLook.Rotation(base.transform, m_HeadContainer);
				m_PlayerMovement.UpdateMovement(base.transform);
				m_PlayerInteraction.Update(m_HeadContainer);
				m_CameraHeadBob.Update();
				m_PlayerFootsteps.Update(CharacterController.velocity.magnitude, PlayerMovement.CurrentSpeed, PlayerMovement.MovementInput.x, PlayerMovement.MovementInput.y, PlayerMovement.IsRunning);
				m_CameraTilt.Update();
				float b = (m_PlayerMovement.IsRunning ? runFOV : defaultFOV);
				GameCamera.Camera.fieldOfView = Mathf.Lerp(GameCamera.Camera.fieldOfView, b, Time.deltaTime * fovLerpSpeed);
			}
		}
	}

	public void Lock()
	{
		IsLocked = true;
	}

	public void Unlock()
	{
		IsLocked = false;
	}

	private void SecretCodes()
	{
		if (!m_IsSecretCodeActive)
		{
			if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.K))
			{
				m_IsSecretCodeActive = true;
			}
			return;
		}
		if (m_SecretCodeTimer >= 2f)
		{
			m_SecretCode = "";
			m_IsSecretCodeActive = false;
			m_SecretCodeTimer = 0f;
			return;
		}
		if (Input.GetKeyDown(KeyCode.Alpha4))
		{
			m_SecretCode += "4";
		}
		else if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			m_SecretCode += "1";
		}
		if (m_SecretCode == "414")
		{
			Interactable_Seasonal_InkDemonsEve_IcePumpkin.hasPumpkin = false;
			Interactable_Seasonal_InkDemonsEve_Skull.skullCount = 0;
			InkDemon inkDemon = GameManager.Instance.AssetManager.CreateAsset<InkDemon>("Prefab_InkDemon");
			inkDemon.ActivateMask(active: false);
			inkDemon.Execute();
			m_SecretCode = "";
			m_IsSecretCodeActive = false;
			m_SecretCodeTimer = 0f;
		}
		else
		{
			m_SecretCodeTimer += Time.deltaTime;
		}
	}
}
