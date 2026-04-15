using DG.Tweening;
using UnityEngine;

namespace TheRealNews.Utility
{
    /// <summary>
    /// Static helper for one-shot camera shakes via DOTween.
    /// </summary>
    public static class CameraEffects
    {
        public static void ShakeRotation(float duration, float strength = 10f, int vibrato = 10,
            float randomness = 90f, bool fadeOut = true)
        {
            var cam = Camera.main;
            if (cam == null) return;

            cam.transform.DOKill();
            cam.transform.localEulerAngles = Vector3.zero;
            cam.DOShakeRotation(duration, strength, vibrato, randomness, fadeOut)
                .OnComplete(() => cam.transform.localEulerAngles = Vector3.zero);
        }
    }
}
