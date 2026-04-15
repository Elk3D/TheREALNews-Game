using DG.Tweening;
using UnityEngine;

public static class CameraEffects
{
	public static void ShakeRotation(float duration, float strength = 10f, int vibrato = 10, float randomness = 90f, bool fadeOut = true, bool vibrate = true)
	{
		Camera.main.transform.DOKill();
		Camera.main.transform.localEulerAngles = Vector3.zero;
		Camera.main.DOShakeRotation(duration, strength, vibrato, randomness, fadeOut).OnComplete(delegate
		{
			Camera.main.transform.localEulerAngles = Vector3.zero;
		});
	}
}
