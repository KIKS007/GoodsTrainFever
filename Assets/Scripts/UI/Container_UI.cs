﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Container_UI : MonoBehaviour
{
	public bool isPrepared = false;
	public bool isSent = false;
	public Container_Level containerLevel;
	public Container container;
	public Container myContainer;

	[Header ("UI")]
	public Image containerImage;
	public Text containerTypeText;
	public Text neededCountText;
	public Text preparedCountText;

	[Header ("Counts")]
	public int neededCount;
	public int preparedCount = 0;

	public Order_UI myOrderUI;
	public Order_UI debugOrderUI;
	//private RectTransform _rectTransform;
	private CanvasGroup _canvasGroup;

	private Color concolor;

	private float _containerSentFade = 0.2f;
	private float _containerAddedFade = 0.6f;

	public void Start ()
	{
		/*_rectTransform = GetComponent<RectTransform> ();
		 */
		_canvasGroup = GetComponent<CanvasGroup> ();
		containerImage = GetComponent<Image> ();

		preparedCount = 0;
		/*
		preparedCountText.enabled = false;*/

		Container.OnContainerDeselected += ContainerDeselected;

		transform.GetChild (0).gameObject.SetActive (false);
	}

	public void Setup (Container_Level c)
	{
		containerLevel = c;

		SetColor (c);

		neededCount = 1;
		/*neededCountText.text = neededCount.ToString ();

		containerTypeText.text = c.containerType.ToString ();*/
	}

	private void ForceGetmyOrderUI ()
	{
		
		myOrderUI = GetComponentInParent<Order_UI> ();
		if (myOrderUI == null) {
			myOrderUI = debugOrderUI;
			myOrderUI.ContainerDeselected ();
		}
	}

	void SetColor (Container_Level c)
	{

		switch (c.containerColor) {
		case ContainerColor.Red:
			concolor = GlobalVariables.Instance.redColor;
			break;
		case ContainerColor.Blue:
			concolor = GlobalVariables.Instance.blueColor;
			break;
		case ContainerColor.Yellow:
			concolor = GlobalVariables.Instance.yellowColor;
			break;
		case ContainerColor.Violet:
			concolor = GlobalVariables.Instance.violetColor;
			break;
		}

		//containerImage.color = color;
	}

	public void ContainerSent ()
	{
		isSent = true;

		if (this.gameObject.activeInHierarchy) {
			StartCoroutine (ContainerSentFeedback ());
		}
	}

	IEnumerator ContainerSentFeedback ()
	{
		yield return new WaitForSeconds (0.2f);

		/*preparedCountText.enabled = false;
		neededCountText.enabled = false;*/
		/*Debug.Log ("WAT");
		this.GetComponent<Image> ().DOFade (0.2f, 0.2f);*/

		_canvasGroup.DOFade (OrdersManager.Instance.containerSentAlpha, OrdersManager.Instance.fadeDuration);
		this.GetComponent<Image> ().DOFade (_containerSentFade, 0.2f);
	}

	public void ContainerAdded (Container c)
	{
		container = c;

		neededCount--;
		preparedCount++;

		UpdateTexts ();

		//containerImage.color = Color.green;

		transform.GetChild (0).gameObject.SetActive (true);

		_canvasGroup.DOFade (_containerAddedFade, MenuManager.Instance.menuAnimationDuration);

		if (TutorialManager.Instance.isActive) {

			TutorialManager.Instance.OnTrain ();
		}
		//Debug.Log ("Container: " + c.containerType + " | " + c.containerColor);
		if (this.gameObject.activeInHierarchy)
		{
			StartCoroutine (ContainerAddedFeedback ());	
		}
	}

	public void ContainerSelected (bool isMe, Container c)
	{
		if (isMe && c.train == null || c == container) 
		{
			this.GetComponent<Image> ().DOFade (1, 0.2f);
		} else 
		{
			this.GetComponent<Image> ().DOFade (0.2f, 0.2f);
		}
	}

	public void ContainerDeselected (Container c)
	{
		myContainer = null;

		if (myOrderUI != null) {
			myOrderUI.ContainerDeselected ();
		} else {
			ForceGetmyOrderUI ();

		}

		/*DOVirtual.DelayedCall (OrdersManager.Instance.fadeDuration + OrdersManager.Instance.fadeInDelay, () => {
			
			_canvasGroup.ignoreParentGroups = false;
		});*/

	}

	IEnumerator ContainerAddedFeedback ()
	{
		yield return new WaitWhile (() => OrdersManager.Instance.ordersHidden);

		DOTween.Kill (transform);

		transform.DOPunchScale (Vector3.one * OrdersManager.Instance.containerFeedbackPunchScale * 2, OrdersManager.Instance.containerAddedDuration);


	}

	public void ContainerRemoved ()
	{
		container = null;

		neededCount++;
		preparedCount--;

		UpdateTexts ();

		//containerImage.color = concolor;

		transform.GetChild (0).gameObject.SetActive (false);
		_canvasGroup.DOFade (1, MenuManager.Instance.menuAnimationDuration);

		if (this.gameObject.activeInHierarchy)
			StartCoroutine (ContainerRemovedFeedback ());
	}

	IEnumerator ContainerRemovedFeedback ()
	{
		yield return new WaitWhile (() => OrdersManager.Instance.ordersHidden);

		/*DOTween.Kill (_rectTransform);

		_rectTransform.DOPunchAnchorPos (Vector2.down * OrdersManager.Instance.containerRemovedHeight, OrdersManager.Instance.containerRemovedDuration);*/

		transform.DOPunchScale (Vector3.one * -OrdersManager.Instance.containerFeedbackPunchScale * 2, OrdersManager.Instance.containerRemovedDuration);


	}

	void UpdateTexts ()
	{
		if (neededCount > 0) {
			isPrepared = false;
			/*neededCountText.enabled = true;
			neededCountText.text = neededCount.ToString ();*/
		} else {
			/*neededCountText.enabled = false;*/
			isPrepared = true;
		}

/*		if (preparedCount > 0) {
			/*	preparedCountText.enabled = true;
			preparedCountText.text = preparedCount.ToString ();
		}*/
	}

	void OnDestroy ()
	{
		Container.OnContainerDeselected -= ContainerDeselected;
	}
}
