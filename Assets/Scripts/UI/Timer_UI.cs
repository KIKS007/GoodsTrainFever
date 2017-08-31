using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Timer_UI : MonoBehaviour
{
	public Color vibrateColor;

	private Transform _clock;
	private CanvasGroup _group;
	private Material _material;
	private Color _materialInitialColor;
	public RotationType rotationType;

	public enum RotationType
	{
		NoRotation,
		Deg30,
		Deg45,
	}

	private float rotationValue;
	// Use this for initialization
	void Start ()
	{
		switch (rotationType) {
		case RotationType.NoRotation:
			rotationValue = 180;
			break;
		case RotationType.Deg30:
			rotationValue = 150;
			break;
		case RotationType.Deg45:
			rotationValue = 135;
			break;
		default:
			rotationValue = 180;
			break;
		}
		_clock = transform.GetChild (0);
		_clock.DORotate (new Vector3 (0, 180, 90), 0);
		_group = _clock.GetComponentInChildren<CanvasGroup> ();
		_material = transform.GetChild (0).GetComponent<Renderer> ().materials [1];
		_materialInitialColor = _material.GetColor ("_Color0");

		_group.alpha = 0;
	}

	public void Show (float time)
	{
		_group.DOFade (1, time);
		_clock.DORotate (new Vector3 (0, rotationValue, 0), time).SetEase (Ease.OutBack, 3f);
	}

	public void Hide (float time)
	{
		_group.DOFade (0, time);
		_clock.DORotate (new Vector3 (0, 180, 90), time).SetEase (Ease.OutBack, 3f);
	}

	public void Vibrate (int second)
	{
		float time = 0.3f;

		_clock.DOPunchRotation (new Vector3 (0, 0, 15 - second * 2), time);

		_material.DOColor (vibrateColor, "_Color0", time * 0.5f);
		_material.DOColor (_materialInitialColor, "_Color0", time).SetDelay (time * 0.5f);
	}
}
