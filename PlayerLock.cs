public class PlayerLock : JMonoBehaviour
{
	public void Lock()
	{
		GameManager.Instance.Player.Lock();
	}

	public void Unlock()
	{
		GameManager.Instance.Player.Unlock();
	}

	public void UnlockPause()
	{
		GameManager.Instance.UnlockPause();
	}
}
