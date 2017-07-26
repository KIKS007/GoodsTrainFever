using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System;

public class MenuManager : Singleton<MenuManager>
{
	public Action OnMenuTransitionStart;
	public Action OnMenuTransitionEnd;

	public bool inTransition = false;

	[Header ("Menu")]
	public MenuComponent currentMenu;
	public MenuComponent mainMenu;
	public MenuComponent menuOnStart;
	public Transform menuParent;

	[Header ("Menu Panel")]
	public Image menuPanel;

	[Header ("Menu Animations")]
	public Ease menuEase = Ease.OutQuad;
	public float menuAnimationDuration = 0.5f;
	public Vector2 menuHidePosition;
	public Vector2 menuShowPosition;

	[Header ("Camera Movement")]
	public Ease movementEase = Ease.OutQuad;
	public float movementDuration;

	[Header ("Menu Position")]
	public Vector3 menuPosition;
	public Vector3 menuRotation;

	[Header ("Game Position")]
	public Vector3 gamePosition;
	public Vector3 gameRotation;

	[Header ("Projection Switch")]
	public Ease switchEase = Ease.OutQuad;
	public float switchDuration = 0.5f;

	private float _menuPanelShowAlpha;

	private Camera _camera;
	private Matrix4x4 _ortho, _perspective;
	private float _fov, _near, _far, _orthographicSize, _aspect;
	private MatrixBlender _matrixBlender;
	private bool _orthoOn = true;

	// Use this for initialization
	void Start () 
	{
		GameManager.Instance.gameState = GameState.Menu;

		_camera = GameObject.FindGameObjectWithTag ("MainCamera").GetComponent<Camera> ();
		_menuPanelShowAlpha = menuPanel.color.a;

		foreach (var m in menuParent.GetComponentsInChildren<MenuComponent> ())
			ClearMenu (m);

		if (menuOnStart != null)
			ShowMenu (menuOnStart);
		else
			ShowMenu (mainMenu);

		SetupMatrix ();
	}

	void SetupMatrix ()
	{
		_matrixBlender = _camera.GetComponent<MatrixBlender> ();

		_fov = _camera.fieldOfView;
		_near =  _camera.nearClipPlane;
		_far = _camera.farClipPlane;
		_orthographicSize = _camera.orthographicSize;

		_aspect = (float) Screen.width / (float) Screen.height;
		_ortho = Matrix4x4.Ortho(-_orthographicSize * _aspect, _orthographicSize * _aspect, -_orthographicSize, _orthographicSize, _near, _far);
		_perspective = Matrix4x4.Perspective(_fov, _aspect, 0.01f, _far);
	}

	public void MenuPosition ()
	{
		_orthoOn = !_orthoOn;
		_matrixBlender.BlendToMatrix(_perspective, switchDuration);

		DOTween.Kill (transform);

		_camera.transform.DOMove (menuPosition, movementDuration).SetEase (movementEase);
		_camera.transform.DORotate (menuRotation, movementDuration).SetEase (movementEase);
	}

	public void GamePosition ()
	{
		_orthoOn = !_orthoOn;
		_matrixBlender.BlendToMatrix(_ortho, switchDuration);

		DOTween.Kill (transform);

		_camera.transform.DOMove (gamePosition, movementDuration).SetEase (movementEase);
		_camera.transform.DORotate (gameRotation, movementDuration).SetEase (movementEase);
	}

	public void ToMenu (MenuComponent menu)
	{
		DOTween.Kill ("Menu");

		if (currentMenu != null)
			HideMenu (currentMenu);

		DOVirtual.DelayedCall (menuAnimationDuration, ()=> ShowMenu (menu)).SetId ("Menu");
	}

	public void ShowMenu (MenuComponent menu)
	{
		menu.gameObject.SetActive (true);

		ShowPanel ();

		StartTransition ();

		currentMenu = menu;

		float animationDuration = menuAnimationDuration;

		if(menu.mainContent)
		{
			DOTween.Kill (menu.mainContent);

			if(menu.mainContent.anchoredPosition != menuShowPosition)
			{
				menu.mainContent.anchoredPosition = menuHidePosition;
				
				menu.mainContent.DOAnchorPos (menuShowPosition, menuAnimationDuration).SetEase (menuEase);
			}
		}

		foreach(var c in menu.contents)
		{
			if (c.content == null)
				continue;

			if(!c.showOnShow)
				continue;

			if (c.content.anchoredPosition == c.showPosition)
				continue;

			DOTween.Kill (c.content);

			c.content.anchoredPosition = c.hidePosition;

			float duration = c.overrideDuration ? c.duration : menuAnimationDuration;
			Ease ease = c.overrideEase ? c.ease : menuEase;

			if(c.delay > 0)
				c.content.DOAnchorPos (c.showPosition, duration).SetEase (ease).SetDelay (c.delay);
			else
				c.content.DOAnchorPos (c.showPosition, duration).SetEase (ease);

			if (duration + c.delay > animationDuration)
				animationDuration = duration + c.delay;
		}

		DOVirtual.DelayedCall (animationDuration, ()=> EndTransition (menu, false)).SetId ("Menu");
	}

	public void HideMenu (MenuComponent menu)
	{
		StartTransition ();

		float animationDuration = menuAnimationDuration;

		if(menu.mainContent)
		{
			DOTween.Kill (menu.mainContent);

			menu.mainContent.DOAnchorPos (menuHidePosition, menuAnimationDuration).SetEase (menuEase).OnComplete (()=> currentMenu = null);
		}

		foreach(var c in menu.contents)
		{
			if (c.content == null)
				continue;

			if(!c.hideOnHide)
				continue;

			DOTween.Kill (c.content);

			float duration = c.overrideDuration ? c.duration : menuAnimationDuration;
			Ease ease = c.overrideEase ? c.ease : menuEase;

			if(c.delay > 0)
				c.content.DOAnchorPos (c.hidePosition, duration).SetEase (ease).SetDelay (c.delay);
			else
				c.content.DOAnchorPos (c.hidePosition, duration).SetEase (ease);

			if (duration + c.delay > animationDuration)
				animationDuration = duration + c.delay;
		}

		DOVirtual.DelayedCall (animationDuration, ()=> EndTransition (menu, true)).SetId ("Menu");
	}

	void ClearMenu (MenuComponent menu)
	{
		if(menu.mainContent)
		{
			DOTween.Kill (menu.mainContent);
			menu.mainContent.anchoredPosition = menuHidePosition;
		}

		foreach(var c in menu.contents)
		{
			if (c.content == null)
				continue;

			DOTween.Kill (c.content);

			c.content.anchoredPosition = c.hidePosition;
		}

		menu.gameObject.SetActive (false);
	}

	void ShowPanel ()
	{
		if (menuPanel.color.a != _menuPanelShowAlpha)
		{
			DOTween.Kill (menuPanel);
			menuPanel.DOFade (_menuPanelShowAlpha, menuAnimationDuration).SetEase (menuEase);
		}
	}

	void HidePanel ()
	{
		if (menuPanel.color.a != 0)
		{
			DOTween.Kill (menuPanel);
			menuPanel.DOFade (0, menuAnimationDuration).SetEase (menuEase);
		}
	}

	void StartTransition ()
	{
		inTransition = true;

		if (OnMenuTransitionStart != null)
			OnMenuTransitionStart ();
	}

	void EndTransition (MenuComponent menu, bool disableMenu)
	{
		inTransition = false;

		if (OnMenuTransitionEnd != null)
			OnMenuTransitionEnd ();

		if(disableMenu)
			menu.gameObject.SetActive (false);
	}

	// Update is called once per frame
	void Update () 
	{
		/*if (Input.GetKeyDown(KeyCode.Space))
		{
			if (_orthoOn)
				GamePosition ();
			else
				MenuPosition ();
		}*/
	}
}
