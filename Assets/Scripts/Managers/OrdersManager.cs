using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;

public class OrdersManager : Singleton<OrdersManager> 
{
	public RectTransform removeOrderTest;

	[Header ("UI")]
	public Ease ordersLayoutEase = Ease.OutQuad;
	public Canvas UICanvas;
	public CanvasGroup ordersCanvasGroup;

	[Header ("Orders Layout")]
	public float ordersSpacing;
	public float ordersLayoutDuration = 0.2f;
	public float ordersLayoutDelay;
	public float topPadding = 15f;
	public float leftPadding = 20f;

	[Header ("Remove Order")]
	public Vector2 removeLocalPosition;
	public float removeDuration = 0.2f;
	public float removeLayoutDelay;

	[Header ("Fade")]
	public float fadeOutValue = 0.5f;
	public float fadeDuration = 0.2f;
	public float fadeOutDelay = 0.1f;
	public float fadeInDelay = 0.1f;

	private float _fadeInValue;

	// Use this for initialization
	void Start () 
	{
		_fadeInValue = ordersCanvasGroup.alpha;

		Container.OnContainerSelected += (c)=> FadeOutGroup ();
		Container.OnContainerDeselected += (c)=> FadeInGroup ();
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}

	[PropertyOrder (-1)]
	[Button ("Update Orders Layout")]
	void UpdateOrdersLayoutTest ()
	{
		UpdateOrdersLayout (Application.isPlaying);
	}

	void UpdateOrdersLayout (bool animated = true, RectTransform orderToIgnore = null)
	{
		Vector2 previousPosition = new Vector2 ();
		float previousWidth = 0f;

		for(int i = 0; i <  ordersCanvasGroup.transform.childCount; i++)
		{
			Vector2 position = new Vector2 ();
			RectTransform rect = ordersCanvasGroup.transform.GetChild (i).GetComponent<RectTransform> ();

			if (rect == orderToIgnore)
				continue;

			position.y = topPadding;
			position.x += rect.sizeDelta.x * 0.5f;

			if(i > 0)
			{
				position.x += previousPosition.x;
				position.x += previousWidth * 0.5f;
				position.x += ordersSpacing;
			}
			else
				position.x += leftPadding;

			DOTween.Kill (rect);

			if (animated)
				rect.DOAnchorPos (position, ordersLayoutDuration).SetEase (ordersLayoutEase).SetDelay (ordersLayoutDelay * i);
			else
				rect.anchoredPosition = position;

			
			previousWidth = rect.sizeDelta.x;
			previousPosition = position;
		}
	}

	[PropertyOrder (-1)]
	[Button ("Remove Order")]
	void RemoveOrderTest ()
	{
		RemoveOrder (removeOrderTest);
	}

	void RemoveOrder (RectTransform order)
	{
		if (order == null)
			return;

		order.DOAnchorPos (order.anchoredPosition + removeLocalPosition, removeDuration).OnComplete (()=> Destroy (order.gameObject));
		DOVirtual.DelayedCall (removeLayoutDelay, ()=> UpdateOrdersLayout (true, order));
	}

	void AddOrder (Level_Order levelOrder)
	{
		
	}

	void FadeOutGroup ()
	{
		DOTween.Kill (ordersCanvasGroup);

		ordersCanvasGroup.DOFade (fadeOutValue, fadeDuration).SetDelay (fadeOutDelay).SetEase (ordersLayoutEase);
	}

	void FadeInGroup ()
	{
		DOTween.Kill (ordersCanvasGroup);

		ordersCanvasGroup.DOFade (_fadeInValue, fadeDuration).SetDelay (fadeInDelay).SetEase (ordersLayoutEase);
	}
}
