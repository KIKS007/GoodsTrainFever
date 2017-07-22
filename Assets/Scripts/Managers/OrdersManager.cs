using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;

public class OrdersManager : Singleton<OrdersManager> 
{
	public RectTransform removeOrderTest;
	public Order_Level levelOrderTest;

	[Header ("Orders")]
	public List<Order_UI> orders = new List<Order_UI> ();
	public List<Container> containersFromNoOrder;

	[Header ("UI")]
	public Ease ordersLayoutEase = Ease.OutQuad;
	public Canvas UICanvas;
	public CanvasGroup ordersCanvasGroup;
	public RectTransform ordersScrollView;

	[Header ("Orders Prefabs")]
	public GameObject orderPanel;
	public GameObject container20;
	public GameObject container40;

	[Header ("Orders Layout")]
	public float ordersPanelWidth = 165f;
	public float ordersPanelHeightOffset = 40f;
	public float containersTopPadding;
	public float containersLineSpacing;
	public float containersRowSpacing;
	public int maxLinesCount = 4;

	[Header ("Overall Layout")]
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

		Train.OnContainerAdded += ContainerAdded;
		Train.OnContainerRemoved += ContainerRemoved;

		UpdateOrdersLayout ();
	}
	
	void ContainerAdded (Container container)
	{
		foreach(var o in orders)
		{
			if (o.ContainerAdded (container))
			{
				//Debug.Log ("Container Valid");
				return;
			}
		}

		containersFromNoOrder.Add (container);

		//Debug.Log ("No Container");
	}

	void ContainerRemoved (Container container)
	{
		if (containersFromNoOrder.Contains (container))
			containersFromNoOrder.Remove (container);
		
		foreach(var o in orders)
		{
			if (o.ContainerRemoved (container))
			{
				//Debug.Log ("Container Valid");
				return;
			}
		}

		//Debug.Log ("No Container");
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

		for(int i = 0; i <  ordersScrollView.transform.childCount; i++)
		{
			Vector2 position = new Vector2 ();
			RectTransform rect = ordersScrollView.transform.GetChild (i).GetComponent<RectTransform> ();

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

		ordersScrollView.sizeDelta = new Vector2 (previousPosition.x + previousWidth * 0.5f + ordersSpacing, ordersScrollView.sizeDelta.y);
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

		orders.Remove (order.GetComponent<Order_UI> ());

		order.DOAnchorPos (order.anchoredPosition + removeLocalPosition, removeDuration).OnComplete (()=> Destroy (order.gameObject));
		DOVirtual.DelayedCall (removeLayoutDelay, ()=> UpdateOrdersLayout (true, order));
	}

	[PropertyOrder (-1)]
	[Button ("Add Order")]
	void AddOrderTest ()
	{
		AddOrder (levelOrderTest);
	}

	void AddOrder (Order_Level levelOrder)
	{
		if(levelOrder == null)
		{
			Debug.LogError ("Invalid LevelOrder!", this);
			return;
		}

		//Create Order Elements
		Vector2 panelPosition = new Vector2 (1500f, topPadding);
		RectTransform panel = (Instantiate (orderPanel, orderPanel.transform.localPosition, orderPanel.transform.localRotation, ordersScrollView)).GetComponent<RectTransform> ();
		Order_UI orderUI = panel.GetComponent<Order_UI> ();

		orders.Add (orderUI);

		ResetRectTransform (panel);
		panel.anchoredPosition = panelPosition;

		int linesCount = 0;
		int rowsCount = 1;

		foreach(var c in levelOrder.levelContainers)
		{
			if (linesCount == maxLinesCount)
			{
				linesCount = 0;
				rowsCount++;
			}

			GameObject containerPrefab = c.isDoubleSize ? container40 : container20;
			RectTransform container = (Instantiate (containerPrefab, Vector3.zero, Quaternion.identity, panel)).GetComponent<RectTransform> ();

			Container_UI containerUI = container.GetComponent<Container_UI> ();
			containerUI.Setup (c);
			container.position = Vector3.zero;

			orderUI.containers.Add (containerUI);

			Vector2 position = new Vector2 ();

			position.y = containersTopPadding;
			position.y -= containersLineSpacing * linesCount;

			linesCount++;

			ResetRectTransform (container);
			container.anchoredPosition = position;
		}

		//Debug.Log ("Row: " + rowsCount + " Lines: " + linesCount);

		orderUI.Setup ();

		SetPanelSize (panel, rowsCount);

		SetContainersPositions (panel);

		UpdateOrdersLayout ();
	}

	void SetPanelSize (RectTransform panel, int rowsCount)
	{
		float width = ordersPanelWidth * rowsCount;
		width += containersRowSpacing * (rowsCount + 1);

		float height = containersLineSpacing * maxLinesCount + ordersPanelHeightOffset;

		panel.sizeDelta = new Vector2 (width, height);
	}

	void SetContainersPositions (RectTransform panel)
	{
		int linesCount = 0;
		int rowsCount = 1;

		for(int i = 1; i < panel.childCount; i++)
		{
			if (linesCount == maxLinesCount)
			{
				linesCount = 0;
				rowsCount++;
			}

			RectTransform rect = panel.GetChild (i).GetComponent<RectTransform> ();
			Vector2 position = rect.anchoredPosition;

			position.x = (ordersPanelWidth * 0.5f) + containersRowSpacing + ( (rowsCount - 1) * (ordersPanelWidth + containersRowSpacing) );

			rect.anchoredPosition = position;

			linesCount++;
		}
	}

	void ResetRectTransform (RectTransform rect)
	{
		rect.localRotation = Quaternion.Euler (Vector3.zero);
		rect.localPosition = Vector3.zero;
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
