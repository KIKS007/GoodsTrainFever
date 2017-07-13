using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ContainersMovementManager : Singleton<ContainersMovementManager> 
{
	[Header ("Selected Container")]
	public Container selectedContainer = null;

	[Header ("Start Hover")]
	public float startHoverHeight = 2.5f;
	public float startHoverDuration = 0.3f;

	[Header ("Hover")]
	public Ease hoverEase = Ease.OutQuad;
	public float hoverHeight = 1f;
	public float hoverDuration = 1f;

	[Header ("Stop Hover")]
	public float stopHoverDuration = 0.2f;

	private float _startHeight;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void StartHover (Container container)
	{
		container.transform.DOKill (true);
		_startHeight = container.transform.localPosition.y;

		//ScreenshakeManager.Singleton.Shake (Vector3.up * .3f, 1, 0, .8f);
		container.transform.DOLocalMoveY (container.transform.localPosition.y + startHoverHeight, startHoverDuration).SetEase (hoverEase).OnComplete (()=> Hover (container));
	}

	void Hover (Container container)
	{
		container.transform.DOLocalMoveY (container.transform.localPosition.y - hoverHeight, hoverDuration).SetEase (hoverEase).OnComplete (() => 
			{
				container.transform.DOLocalMoveY (container.transform.localPosition.y + hoverHeight, hoverDuration).SetEase (hoverEase).OnComplete (()=> Hover (container));
			});
	}

	public void StopHover (Container container)
	{
		container.transform.DOKill ();
		container.transform.DOLocalMoveY (_startHeight, stopHoverDuration).SetEase (Ease.OutBounce, 40, 1);

		//ScreenshakeManager.Singleton.Shake (Vector3.left * Mathf.Max (0, (transform.position.y / startHeight) - 1.1f), 5, .1f, .4f);
	}
}
