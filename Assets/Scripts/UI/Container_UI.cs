using System.Collections;
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

	[Header ("UI")]
	public Image containerImage;
	public Text containerTypeText;
	public Text neededCountText;
	public Text preparedCountText;

	[Header ("Counts")]
	public int neededCount;
	public int preparedCount = 0;

	private RectTransform _rectTransform;
	private CanvasGroup _canvasGroup;

	public void Awake ()
	{
		_rectTransform = GetComponent<RectTransform> ();
		_canvasGroup = GetComponent<CanvasGroup> ();

		preparedCount = 0;
		preparedCountText.enabled = false;
	}

	public void Setup (Container_Level c)
	{
		containerLevel = c;

		SetColor (c);

		neededCount = 1;
		neededCountText.text = neededCount.ToString ();

		containerTypeText.text = c.containerType.ToString ();
	}

	void SetColor (Container_Level c)
	{
		Color color = new Color ();

		switch (c.containerColor)
		{
		case ContainerColor.Red:
			color = GlobalVariables.Instance.redColor;
			break;
		case ContainerColor.Blue:
			color = GlobalVariables.Instance.blueColor;
			break;
		case ContainerColor.Yellow:
			color = GlobalVariables.Instance.yellowColor;
			break;
		case ContainerColor.Violet:
			color = GlobalVariables.Instance.violetColor;
			break;
		}

		containerImage.color = color;
	}

	public void ContainerSent ()
	{
		isSent = true;

		StartCoroutine (ContainerSentFeedback ());
	}

	IEnumerator ContainerSentFeedback ()
	{
		yield return new WaitWhile (()=> OrdersManager.Instance.ordersHidden);

		preparedCountText.enabled = false;
		neededCountText.enabled = false;

		_canvasGroup.DOFade (OrdersManager.Instance.containerSentAlpha, OrdersManager.Instance.fadeDuration);
	}

	public void ContainerAdded (Container c)
	{
		container = c;

		neededCount--;
		preparedCount++;

		UpdateTexts ();

		StartCoroutine (ContainerAddedFeedback ());
	}

	IEnumerator ContainerAddedFeedback ()
	{
		yield return new WaitWhile (()=> OrdersManager.Instance.ordersHidden);

		DOTween.Kill (transform);

		transform.DOPunchScale (Vector3.one * OrdersManager.Instance.containerFeedbackPunchScale, OrdersManager.Instance.containerAddedDuration);
	}

	public void ContainerRemoved ()
	{
		container = null;

		neededCount++;
		preparedCount--;

		UpdateTexts ();

		StartCoroutine (ContainerRemovedFeedback ());
	}

	IEnumerator ContainerRemovedFeedback ()
	{
		yield return new WaitWhile (()=> OrdersManager.Instance.ordersHidden);

		DOTween.Kill (_rectTransform);

		_rectTransform.DOPunchAnchorPos (Vector2.down * OrdersManager.Instance.containerRemovedHeight, OrdersManager.Instance.containerRemovedDuration);

		//transform.DOPunchScale (Vector3.one * -OrdersManager.Instance.containerFeedbackPunchScale, OrdersManager.Instance.containerRemovedDuration);
	}

	void UpdateTexts ()
	{
		if(neededCount > 0)
		{
			isPrepared = false;
			neededCountText.enabled = true;
			neededCountText.text = neededCount.ToString ();
		}
		else
		{
			neededCountText.enabled = false;
			isPrepared = true;
		}

		if(preparedCount > 0)
		{
			preparedCountText.enabled = true;
			preparedCountText.text = preparedCount.ToString ();
		}
		else
			preparedCountText.enabled = false;
	}
}
