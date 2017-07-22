using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Container_UI : MonoBehaviour 
{
	public bool isPrepared = false;
	public Container_Level container;

	[Header ("UI")]
	public Image containerImage;
	public Text containerTypeText;
	public Text neededCountText;
	public Text preparedCountText;

	[Header ("Counts")]
	public int neededCount;
	public int preparedCount = 0;

	public void Awake ()
	{
		preparedCount = 0;
		preparedCountText.enabled = false;
	}

	public void Setup (Container_Level c)
	{
		container = c;

		SetColor (c);

		neededCount = c.containerCount > 0 ? c.containerCount : 1;
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

	public void ContainerAdded ()
	{
		neededCount--;
		preparedCount++;

		UpdateTexts ();
	}

	public void ContainerRemoved ()
	{
		neededCount++;
		preparedCount--;

		UpdateTexts ();
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
