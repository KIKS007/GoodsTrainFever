using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector;
using UnityEngine.UI;

public class MenuButtonComponent : MonoBehaviour, ISubmitHandler, IPointerClickHandler
{
	[Header ("To Menu")]
	public bool toMenu = true;
	[ShowIfAttribute ("toMenu")]
	public MenuComponent menu;

	private Button _button;

	// Use this for initialization
	void Awake () 
	{
		_button = GetComponent<Button> ();

		MenuManager.Instance.OnMenuTransitionStart += () => _button.interactable = false;
		MenuManager.Instance.OnMenuTransitionEnd += () => _button.interactable = true;
	}

	public void OnSubmit (BaseEventData eventData)
	{
		Submit ();
	}

	public void OnPointerClick (PointerEventData eventData)
	{
		Submit ();
	}

	void Submit ()
	{
		if (!_button.interactable)
			return;

		if (toMenu && menu)
			MenuManager.Instance.ToMenu (menu);
	}
}
