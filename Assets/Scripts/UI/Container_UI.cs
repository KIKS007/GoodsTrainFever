using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Container_UI : MonoBehaviour 
{
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

	public void Setup (int n, ContainerType type)
	{
		neededCount = n;
		neededCountText.text = neededCount.ToString ();

		containerTypeText.text = type.ToString ();
	}

	public void ContainerPrepared ()
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
			neededCountText.enabled = true;
			neededCountText.text = neededCount.ToString ();
		}
		else
			neededCountText.enabled = false;

		if(preparedCount > 0)
		{
			preparedCountText.enabled = true;
			preparedCountText.text = preparedCount.ToString ();
		}
		else
			preparedCountText.enabled = false;
	}
}
