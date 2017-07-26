using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;

public class MenuComponent : MonoBehaviour 
{
	[Header ("MainContent")]
	public RectTransform mainContent;

	[Header ("Contents")]
	public List<MenuContent> contents = new List<MenuContent> ();

	// Use this for initialization
	void Awake () 
	{
		if (mainContent == null && transform.Find ("MainContent"))
			mainContent = transform.Find ("MainContent").GetComponent<RectTransform> ();

		foreach (var c in contents)
			if (c.content)
				c.showPosition = c.content.anchoredPosition;
	}

	[ButtonGroup ("", -1)]
	public virtual void Show ()
	{
		MenuManager.Instance.ShowMenu (this);
	}

	[ButtonGroup ("", -1)]
	public virtual void Hide ()
	{
		MenuManager.Instance.HideMenu (this);
	}
}

[System.Serializable]
public class MenuContent
{
	public RectTransform content;

	[Header ("Positions")]
	public Vector2 hidePosition;
	[HideInInspector]
	public Vector2 showPosition;

	[Header ("Durations")]
	public float delay = 0;
	public bool overrideDuration = false;
	[ShowIfAttribute ("overrideDuration")]
	public float duration;

	[Header ("Ease")]
	public bool overrideEase = false;
	[ShowIfAttribute ("overrideEase")]
	public Ease ease;

	[Header ("Display")]
	public bool showOnShow = true;
	public bool hideOnHide = true;

}
