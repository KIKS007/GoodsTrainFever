using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine.Events;
using UnityEngine.UI;

public class MenuComponent : MonoBehaviour 
{
	[Header ("MainContent")]
	public bool disableOnHide = true;
	public RectTransform mainContent;

	[Header ("Contents")]
	public List<MenuContent> contents = new List<MenuContent> ();

	[Header ("Canvas Group")]
	public CanvasGroup menuCanvasGroup;

	[Header ("Back")]
	public bool backToMainMenu = false;
	public MenuComponent backMenu;
	public Button backButton;
	public UnityEvent backEvent;

	// Use this for initialization
	void Awake () 
	{
		menuCanvasGroup = GetComponent<CanvasGroup> ();

		if (mainContent == null && transform.Find ("MainContent"))
			mainContent = transform.Find ("MainContent").GetComponent<RectTransform> ();

		foreach (var c in contents)
			if (c.content)
				c.showPosition = c.content.anchoredPosition;
	}

	public virtual void OnShow ()
	{
		
	}

	public virtual void OnHide ()
	{
		
	}

	[ButtonGroup ("", -1)]
	public void ShowEditor ()
	{
		MenuManager.Instance.ShowMenu (this);
	}

	[ButtonGroup ("", -1)]
	public void HideEditor ()
	{
		MenuManager.Instance.HideMenu (this);
	}
}

[System.Serializable]
public class MenuContent
{
	public RectTransform content;
	public bool disableOnHide = true;

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
