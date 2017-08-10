using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;

public enum FeedbackType { Default, StartHover, StopHover, StartTakeSpot, EndTakeSpot }

public class ScreenshakeManager : Singleton<ScreenshakeManager> 
{
	public List<SlowMotionSettings> screenshakesSettings = new List<SlowMotionSettings> ();

	[Header ("Common Settings")]
	public Transform shakeTarget;
	public float resetDuration = 1f;

	[Header ("Debug")]
	public FeedbackType shakeDebug = FeedbackType.Default;

	private Vector3 _initialPosition;

	// Use this for initialization
	void Start () 
	{
		_initialPosition = shakeTarget.position;
	}

	public void Shake (FeedbackType whichSlowMo = FeedbackType.Default)
	{
		float shakeDuration = 0;
		float shakeDelay = 0;
		int shakeVibrato = 0;
		Vector3 shakePunch = Vector3.zero;
		bool exactType = true;

		for(int i = 0; i < screenshakesSettings.Count; i++)
		{
			if(screenshakesSettings[i].feedbackType == whichSlowMo)
			{
				shakeDuration = screenshakesSettings [i].shakeDuration;
				shakeDelay = screenshakesSettings [i].shakeDelay;
				shakePunch = screenshakesSettings [i].shakePunch;
				shakeVibrato = screenshakesSettings [i].shakeVibrato;
				exactType = true;
				break;
			}
		}

		if(!exactType)
		{
			shakeDuration = screenshakesSettings [0].shakeDuration;
			shakeDelay = screenshakesSettings [0].shakeDelay;
			shakeVibrato = screenshakesSettings [0].shakeVibrato;
			shakePunch = screenshakesSettings [0].shakePunch;
		}

		shakeTarget.DOKill (true);

		shakeTarget.DOPunchPosition (shakePunch, shakeDuration, shakeVibrato).SetDelay (shakeDelay).OnComplete (ResetCameraRotation).SetUpdate (true);
	}

	void ResetCameraRotation ()
	{
		if (DOTween.IsTweening (shakeTarget))
			return;

		shakeTarget.DOMove (_initialPosition, resetDuration).SetUpdate (true);
	}

	[ButtonGroup ("A")]
	[ButtonAttribute ("Shake")]
	public void ShakeDebug ()
	{
		Shake(shakeDebug);
	}

	[ButtonGroup ("A")]
	[ButtonAttribute ("Reset")]
	public void ResetDebug ()
	{
		ResetCameraRotation();
	}
}

[System.Serializable]
public class SlowMotionSettings
{
	public FeedbackType feedbackType = FeedbackType.Default;

	public Vector3 shakePunch = new Vector3 (0, 1, 0);
	public float shakeDuration = 0.2f;
	public int shakeVibrato = 1;
	public float shakeDelay = 0f;
}
