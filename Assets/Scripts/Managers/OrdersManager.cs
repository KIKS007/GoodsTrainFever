using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine.UI;

public class OrdersManager : Singleton<OrdersManager>
{
	public RectTransform removeOrderTest;
	public Order_Level levelOrderTest;

	[Header ("Orders")]
	public bool allOrdersSent = false;
	public int ordersSentCount = 0;
	public int ordersCount = 0;
	//public List<Order_UI> orders = new List<Order_UI> ();
	public List<Order_Level> orders = new List<Order_Level> ();
	public List<Container> containersFromNoOrder;

	[Header ("UI")]
	public bool ordersHidden = false;
	public Ease ordersLayoutEase = Ease.OutQuad;
	public Canvas UICanvas;
	public CanvasGroup ordersCanvasGroup;
	public RectTransform ordersScrollView;
	public ScrollRect ordersScrollRect;


	[Header ("NEW UI")]
	public OrderUI newOrderUI;


	[Header ("Orders Feedback")]
	public float containerFeedbackPunchScale = 0.3f;
	public float containerAddedDuration = 0.2f;
	public float containerRemovedHeight = 0.5f;
	public float containerRemovedDuration = 0.2f;
	public float containerSentAlpha = 0.3f;

	[Header ("Orders Prefabs")]
	public GameObject orderPanel;
	public GameObject container20;
	public GameObject container40;

	[Header ("Arrow")]
	public Image moreOrdersArrow;
	public float moreOrdersOutOfScreenSize;
	[Range (0, 100)]
	public int moreOrdersEndPercentage;

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
	public Vector2 removeOrderLocalPosition;
	public float removeOrderDuration = 0.2f;
	public float removeOrderLayoutDelay = 0.5f;

	[Header ("Fade")]
	public float fadeOutValue = 0.5f;
	public float fadeDuration = 0.2f;
	public float fadeOutDelay = 0.1f;
	public float fadeInDelay = 0.1f;

	//private float _fadeInValue;
	//private bool _arrowVisible = false;

	// Use this for initialization
	void Start ()
	{
		//_fadeInValue = ordersCanvasGroup.alpha;
		ordersCanvasGroup.alpha = 0;
		Container.OnContainerSelected += (c) => FadeOutGroup ();
		Container.OnContainerDeselected += (c) => FadeInGroup ();
		GameManager.Instance.OnPlaying += Appear;
		GameManager.Instance.OnMenu += Disappear;
		Container.OnContainerSelected += ContainerSelected;

		Train.OnContainerAdded += ContainerAdded;
		Train.OnContainerRemoved += ContainerRemoved;
		moreOrdersArrow.DOFade (0, MenuManager.Instance.menuAnimationDuration);

		UpdateOrdersLayout ();
	}

	public void TrainDeparture (List<Container> trainContainers)
	{
		var containers = new List<Container> (trainContainers);
		var currentOrders = new List<Order_UI> (newOrderUI.OrderThing);

		foreach (var c in containers) {
			if (c == null)
				continue;

			currentOrders.Clear ();
			currentOrders = new List<Order_UI> (newOrderUI.OrderThing);

			foreach (var o in currentOrders) {
				if (o.ContainerSent (c)) {
					//Debug.Log ("Container Valid");
				}
				if (o.isSent) {
					ordersSentCount++;
					//o.OrderSent ();
					LevelsManager.Instance.OrderSent (o.orderLevel);

					RemoveOrder (o.orderLevel);

					if (ordersSentCount == ordersCount || currentOrders.Count == 0) {
						allOrdersSent = true;
					}
				}
			}
		}

		if (ordersSentCount == ordersCount || currentOrders.Count == 0) {
			allOrdersSent = true;
		}

	}

	void ContainerAdded (Container container)
	{
		if (newOrderUI.OrderThing.Count == 0)
			return;

		foreach (var o in newOrderUI.OrderThing) {
			if (o.ContainerAdded (container)) {
				//Debug.Log ("Container Valid");
				return;
			}
		}

		containersFromNoOrder.Add (container);

		//Debug.Log ("No Container");
	}

	void ContainerRemoved (Container container)
	{
		if (newOrderUI.OrderThing.Count == 0)
			return;

		if (containersFromNoOrder.Contains (container))
			containersFromNoOrder.Remove (container);
		
		foreach (var o in newOrderUI.OrderThing) {
			if (o.ContainerRemoved (container)) {
				//Debug.Log ("Container Valid");
				return;
			}
		}

		//Debug.Log ("No Container");
	}

	void ContainerSelected (Container container)
	{
		if (orders.Count == 0)
			return;

		foreach (var o in newOrderUI.OrderThing) {
			if (o.ContainerSelected (container)) {
				
				//return;
			}
		}
	}




	[PropertyOrder (-1)]
	[Button ("Update Orders Layout")]
	void UpdateOrdersLayoutTest ()
	{
		UpdateOrdersLayout (Application.isPlaying);
	}

	public void UpdateOrdersLayout (bool animated = true, RectTransform orderToIgnore = null)
	{
		/*Vector2 previousPosition = new Vector2 ();
		float previousWidth = 0f;

		for (int i = 0; i < ordersScrollView.transform.childCount; i++) {
			Vector2 position = new Vector2 ();
			RectTransform rect = ordersScrollView.transform.GetChild (i).GetComponent<RectTransform> ();

			if (rect == orderToIgnore)
				continue;

			position.y = topPadding;
			position.x += rect.sizeDelta.x * 0.5f;

			if (i > 0) {
				position.x += previousPosition.x;
				position.x += previousWidth * 0.5f;
				position.x += ordersSpacing;
			} else
				position.x += leftPadding;

			DOTween.Kill (rect);

			if (animated)
				rect.DOAnchorPos (position, ordersLayoutDuration).SetEase (ordersLayoutEase).SetDelay (ordersLayoutDelay * i).SetUpdate (true);
			else
				rect.anchoredPosition = position;

			
			previousWidth = rect.sizeDelta.x;
			previousPosition = position;
		}

		ordersScrollView.sizeDelta = new Vector2 (previousPosition.x + previousWidth * 0.5f + ordersSpacing, ordersScrollView.sizeDelta.y);*/

		//MoreOrdersArrow ();
	}

	public void RemoveOrder (Order_Level order)
	{

		newOrderUI.RemoveOrder (order);
		//StartCoroutine (RemoveOrderCoroutine (order, delay, animated));
	}

	/*IEnumerator RemoveOrderCoroutine (Order_Level order, float delay = 0, bool animated = true)
	{
		

		if (order == null)
			yield break;
		
		orders.Remove (order);

		if (delay > 0)
			yield return new WaitForSeconds (delay);

		//RectTransform orderRect = order.GetComponent<RectTransform> ();

		/*if (animated) {
			orderRect.DOAnchorPos (orderRect.anchoredPosition + removeOrderLocalPosition, removeOrderDuration).SetUpdate (true).OnComplete (() => Destroy (order.gameObject));
			yield return new WaitForSeconds (removeOrderDuration);
			UpdateOrdersLayout (true, orderRect);
		} else {
			Destroy (order.gameObject);
			UpdateOrdersLayout (true, orderRect);
		}
	}*/

	[PropertyOrder (-1)]
	[Button ("Add Order")]
	void AddOrderTest ()
	{
		AddOrder (levelOrderTest);
	}

	public void AddOrder (Order_Level levelOrder)
	{
		orders.Add (levelOrder);
		newOrderUI.AddOrder (levelOrder);
		ordersCount++;
		allOrdersSent = false;
		/*if (levelOrder == null) {
			Debug.LogError ("Invalid LevelOrder!", this);
			return;
		}

		allOrdersSent = false;

		ordersCount++;

		//Create Order Elements
		Vector2 panelPosition = new Vector2 (1500f, topPadding);
		RectTransform panel = (Instantiate (orderPanel, orderPanel.transform.localPosition, orderPanel.transform.localRotation, ordersScrollView)).GetComponent<RectTransform> ();
		Order_UI orderUI = panel.GetComponent<Order_UI> ();

		orderUI.orderLevel = levelOrder;

		orders.Add (orderUI);

		ResetRectTransform (panel);
		panel.anchoredPosition = panelPosition;

		int linesCount = 0;
		int rowsCount = 1;

		foreach (var c in levelOrder.levelContainers) {
			if (linesCount == maxLinesCount) {
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

		UpdateOrdersLayout ();*/
	}

	void SetPanelSize (RectTransform panel, int rowsCount)
	{
		/*float width = ordersPanelWidth * rowsCount;
		width += containersRowSpacing * (rowsCount + 1);

		float height = containersLineSpacing * maxLinesCount + ordersPanelHeightOffset;

		panel.sizeDelta = new Vector2 (width, height);*/
	}

	void SetContainersPositions (RectTransform panel)
	{
		/*int linesCount = 0;
		int rowsCount = 1;

		for (int i = 1; i < panel.childCount; i++) {
			if (linesCount == maxLinesCount) {
				linesCount = 0;
				rowsCount++;
			}

			RectTransform rect = panel.GetChild (i).GetComponent<RectTransform> ();
			Vector2 position = rect.anchoredPosition;

			position.x = (ordersPanelWidth * 0.5f) + containersRowSpacing + ((rowsCount - 1) * (ordersPanelWidth + containersRowSpacing));

			rect.anchoredPosition = position;

			linesCount++;
		}*/
	}

	void ResetRectTransform (RectTransform rect)
	{
		/*rect.localRotation = Quaternion.Euler (Vector3.zero);
		rect.localPosition = Vector3.zero;*/
	}

	void FadeOutGroup ()
	{
		//DOTween.Kill (ordersCanvasGroup);

		ordersHidden = true;

		//ordersCanvasGroup.DOFade (fadeOutValue, fadeDuration).SetDelay (fadeOutDelay).SetEase (ordersLayoutEase).SetUpdate (true);
	}

	void FadeInGroup ()
	{
		//DOTween.Kill (ordersCanvasGroup);

		//ordersCanvasGroup.DOFade (_fadeInValue, fadeDuration).SetDelay (fadeInDelay).SetEase (ordersLayoutEase).SetUpdate (true).OnComplete (() => ordersHidden = false);
	}

	void Appear ()
	{
		DOTween.Kill (ordersCanvasGroup);

		ordersHidden = true;

		ordersCanvasGroup.DOFade (1, fadeDuration).SetDelay (fadeOutDelay).SetEase (ordersLayoutEase).SetUpdate (true);
	}

	public void Disappear ()
	{
		DOTween.Kill (ordersCanvasGroup);

		ordersHidden = true;

		ordersCanvasGroup.DOFade (0, fadeDuration).SetDelay (fadeOutDelay).SetEase (ordersLayoutEase).SetUpdate (true);
	}

	public void ClearOrders (bool animated)
	{
		//Use this to have a derparture animation when clearing all orders
		/*foreach (var o in orders)
			RemoveOrder (o);*/
		newOrderUI.ClearAllOrder ();
		orders.Clear ();
		containersFromNoOrder.Clear ();

		ordersSentCount = 0;
		ordersCount = 0;
		/*

		containersFromNoOrder.Clear ();

		List<Order_Level> ordersTemp = new List<Order_Level> (orders);

		foreach (var o in orders)
			RemoveOrder (o);*/

		/*foreach (Transform t in ordersScrollView)
			Destroy (t.gameObject);*/
	}

	public void MoreOrdersArrow ()
	{
		/*if (ordersScrollView.sizeDelta.x <= moreOrdersOutOfScreenSize) {
			if (_arrowVisible) {
				_arrowVisible = false;
				moreOrdersArrow.DOFade (0, MenuManager.Instance.menuAnimationDuration);
			}
		} else {
			if (!_arrowVisible) {
				_arrowVisible = true;
				moreOrdersArrow.DOFade (1, MenuManager.Instance.menuAnimationDuration);
			}

			return;
		}*/



		/*if (ordersScrollRect.normalizedPosition.x * 100 < moreOrdersEndPercentage) {
			if (!_arrowVisible) {
				_arrowVisible = true;
				moreOrdersArrow.DOFade (1, MenuManager.Instance.menuAnimationDuration);
				return;
			}
		} else {
			if (_arrowVisible) {
				_arrowVisible = false;
				moreOrdersArrow.DOFade (0, MenuManager.Instance.menuAnimationDuration);
			}
		}*/
	}
}
