using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Timer_UI : MonoBehaviour
{

    private Transform _clock;
    private CanvasGroup _group;
    // Use this for initialization
    void Start()
    {
        _clock = transform.GetChild(0);
        _clock.DORotate(new Vector3(0, 180, 90), 0);
        _group = _clock.GetComponentInChildren<CanvasGroup>();
        _group.alpha = 0;
    }

    public void Show(float time)
    {
        _group.DOFade(1, time);
        _clock.DORotate(new Vector3(0, 180, 0), time).SetEase(Ease.OutBack, 3f);
    }

    public void Hide(float time)
    {
        _group.DOFade(0, time);
        _clock.DORotate(new Vector3(0, 180, 90), time).SetEase(Ease.OutBack, 3f);
    }
}
