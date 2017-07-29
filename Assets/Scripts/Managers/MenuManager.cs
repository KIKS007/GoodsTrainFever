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
	public Action OnLevelStart;
	public Action OnMainMenu;

	public bool inTransition = false;

	[Header ("UI")]
	public CanvasGroup UICanvasGroup;

	[Header ("Menu")]
	public Transform menuParent;
	public MenuComponent menuOnStart;
	public MenuComponent currentMenu;

	[Header ("Main Menu")]
	public MenuComponent mainMenu;
	public RectTransform title;
	public float titleHiddenYPos = 260f;

	[Header ("End Level Menu")]
	public float endLevelDelay = 2f;
	public MenuComponent endLevelMenu;

	[Header ("Pause Menu")]
	public MenuComponent pauseMenu;

	[Header ("Menu Panel")]
	public Image menuPanel;

	[Header ("Menu Levels")]
	public MenuLevels menulevels;

	[Header ("Back Button")]
	public RectTransform backButton;
	public Vector2 backButtonHiddenPos;

	[Header ("Info Button")]
	public Button infosButton;

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
	private float _titleShowPos;
	private Vector2 _backButtonShowPos;

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

		_titleShowPos = title.anchoredPosition.y;

		_backButtonShowPos = backButton.anchoredPosition;

		menuParent.gameObject.SetActive (true);
		menuPanel.gameObject.SetActive (true);
		title.gameObject.SetActive (true);
		backButton.gameObject.SetActive (true);

		menulevels.SetupLevels ();

		infosButton.interactable = false;
			
		Container.OnContainerSelected += (c) => infosButton.interactable = true;
		Container.OnContainerDeselected += (c) => infosButton.interactable = false;

		foreach (var m in FindObjectsOfType<MenuComponent> ())
			ClearMenu (m);

		UIFadeOut ();

		if (menuOnStart != null)
			ShowMenu (menuOnStart);
		else
			ShowMenu (mainMenu);

		SetupMatrix ();
	}

	void Update ()
	{
		/*if (Input.GetKeyDown(KeyCode.Space))
		{
			if (_orthoOn)
				GamePosition ();
			else
				MenuPosition ();
		}*/

		if(Input.GetKeyDown (KeyCode.Escape) && currentMenu)
			Back ();
	}

	void BackButton (MenuComponent menu)
	{
		if(menu && menu.backToMainMenu || menu && menu.backMenu != null)
		{
			DOTween.Kill (backButton);
			backButton.DOAnchorPos (_backButtonShowPos, menuAnimationDuration).SetEase (menuEase);
		}
		else
		{
			DOTween.Kill (backButton);
			backButton.DOAnchorPos (backButtonHiddenPos, menuAnimationDuration).SetEase (menuEase);
		}
	}

	public void Back ()
	{
		if (currentMenu.backToMainMenu)
			MainMenu ();
		
		else if (currentMenu.backButton != null)
			currentMenu.backButton.onClick.Invoke ();

		else if (currentMenu.backEvent != null)
			currentMenu.backEvent.Invoke ();

		else if (currentMenu.backMenu != null)
			ToMenu (currentMenu.backMenu);
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

	public void ToMenu (MenuComponent menu, bool showPanel = true)
	{
		DOTween.Kill ("Menu");

		if (currentMenu != null)
			HideMenu (currentMenu, menu);

		DOVirtual.DelayedCall (menuAnimationDuration, ()=> ShowMenu (menu, showPanel)).SetId ("Menu");
	}

	public void ShowMenu (MenuComponent menu, bool showPanel = true)
	{
		menu.gameObject.SetActive (true);

		if(showPanel)
			ShowPanel ();

		StartTransition ();

		BackButton (menu);

		currentMenu = menu;

		float animationDuration = menuAnimationDuration;

		if(menu.mainContent)
		{
			DOTween.Kill (menu.mainContent);

			menu.mainContent.gameObject.SetActive (true);

				menu.mainContent.anchoredPosition = menuHidePosition;
				
				menu.mainContent.DOAnchorPos (menuShowPosition, menuAnimationDuration).SetEase (menuEase);
		}

		foreach(var c in menu.contents)
		{
			if (c.content == null)
				continue;

			if(!c.showOnShow)
				continue;

			/*if (c.content.anchoredPosition == c.showPosition)
				continue;*/

			c.content.gameObject.SetActive (true);

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

		menu.Show ();

		DOVirtual.DelayedCall (animationDuration, ()=> EndTransition (menu, false)).SetId ("Menu");
	}

	public void HideMenu (MenuComponent menu, MenuComponent toMenu = null)
	{
		StartTransition ();

		float animationDuration = menuAnimationDuration;

		currentMenu = null;

		BackButton (toMenu);

		if(menu.mainContent)
		{
			DOTween.Kill (menu.mainContent);

			menu.mainContent.DOAnchorPos (menuHidePosition, menuAnimationDuration).SetEase (menuEase).OnComplete (()=> menu.mainContent.gameObject.SetActive (false));
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
				c.content.DOAnchorPos (c.hidePosition, duration).SetEase (ease).SetDelay (c.delay).OnComplete (()=> c.content.gameObject.SetActive (false));
			else
				c.content.DOAnchorPos (c.hidePosition, duration).SetEase (ease).OnComplete (()=> c.content.gameObject.SetActive (false));

			if (duration + c.delay > animationDuration)
				animationDuration = duration + c.delay;
		}

		menu.Hide ();

		DOVirtual.DelayedCall (animationDuration, ()=> EndTransition (menu, true)).SetId ("Menu");
	}

	public void HideCurrentMenu ()
	{
		if (currentMenu != null)
			HideMenu (currentMenu);
	}

	public void PauseAndShowMenu (MenuComponent menu)
	{
		if(GameManager.Instance.gameState == GameState.Playing)
		{
			GameManager.Instance.gameState = GameState.Pause;

			UIFadeOut ();
		}

		ShowMenu (menu, false);
	}

	public void ResumeAndHideMenu (MenuComponent menu)
	{
		HideMenu (menu);

		DOVirtual.DelayedCall (menuAnimationDuration, ()=> {

			if(GameManager.Instance.gameState == GameState.Pause)
			{
				UIFadeIn ();
				GameManager.Instance.gameState = GameState.Playing;
			}

		});
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
		UIFadeOut ();
	
		menuPanel.gameObject.SetActive (true);

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
			menuPanel.DOFade (0, menuAnimationDuration).SetEase (menuEase).OnComplete (()=> menuPanel.gameObject.SetActive (false));
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

	public void EndLevel ()
	{
		DOVirtual.DelayedCall (endLevelDelay, ()=>{
			
			UIFadeOut ();
			ToMenu (endLevelMenu, false);
		});
	}

	public void MainMenu ()
	{
		ShowPanel ();

		ShowTitle ();

		ToMenu (mainMenu);

		DOVirtual.DelayedCall (menuAnimationDuration, ()=> {
			
			if (OnMainMenu != null)
				OnMainMenu ();
		});
	}

	public void RetryLevel ()
	{
		ShowPanel ();

		DOVirtual.DelayedCall (menuAnimationDuration, ()=>
			{
				LevelsManager.Instance.LoadLevelSettings (LevelsManager.Instance.levelIndex);

				if (OnLevelStart != null)
					OnLevelStart ();
			});

		DOVirtual.DelayedCall (menuAnimationDuration * 2, ()=>
			{
				HidePanel ();
				UIFadeIn ();
				HideCurrentMenu ();
				GameManager.Instance.StartLevel ();
			});
	}

	public void NextLevel ()
	{
		ShowPanel ();
		
		DOVirtual.DelayedCall (menuAnimationDuration, ()=>
			{
				LevelsManager.Instance.NextLevel ();

				if (OnLevelStart != null)
					OnLevelStart ();
			});

		DOVirtual.DelayedCall (menuAnimationDuration * 2, ()=>
			{
				HidePanel ();
				UIFadeIn ();
				HideCurrentMenu ();
				GameManager.Instance.StartLevel ();
			});
	}

	public void StartLevel ()
	{
		ShowPanel ();

		if (OnLevelStart != null)
			OnLevelStart ();
		
		if(currentMenu)
			HideMenu (currentMenu);

		HideTitle ();

		HideCurrentMenu ();

		DOVirtual.DelayedCall (menuAnimationDuration * 2, ()=>
			{
				HidePanel ();
				UIFadeIn ();
				GameManager.Instance.StartLevel ();
			});
	}

	public void UIFadeOut ()
	{
		UICanvasGroup.DOFade (0, menuAnimationDuration).SetEase (menuEase).OnComplete (()=> UICanvasGroup.gameObject.SetActive (false));
	}

	public void UIFadeIn ()
	{
		UICanvasGroup.gameObject.SetActive (true);
		UICanvasGroup.DOFade (1, menuAnimationDuration).SetEase (menuEase);
	}

	void ShowTitle ()
	{
		title.gameObject.SetActive (true);
		title.DOAnchorPosY (_titleShowPos, menuAnimationDuration).SetEase (menuEase);
	}

	void HideTitle ()
	{
		title.DOAnchorPosY (titleHiddenYPos, menuAnimationDuration).SetEase (menuEase).OnComplete (()=> title.gameObject.SetActive (false));
	}
}
