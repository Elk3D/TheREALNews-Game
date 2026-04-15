using UnityEngine;
using TheRealNews.Core;

namespace TheRealNews.Player
{
    /// <summary>
    /// Tiny adapter so Animation Events / UnityEvent wiring can lock/unlock
    /// the player without a direct GameManager reference in the inspector.
    /// </summary>
    public class PlayerLock : MonoBehaviour
    {
        public void Lock()
        {
            var p = GameManager.Instance != null ? GameManager.Instance.Player : null;
            if (p != null) p.Lock();
        }

        public void Unlock()
        {
            var p = GameManager.Instance != null ? GameManager.Instance.Player : null;
            if (p != null) p.Unlock();
        }

        public void UnlockPause()
        {
            if (GameManager.Instance != null) GameManager.Instance.UnlockPause();
        }
    }
}
