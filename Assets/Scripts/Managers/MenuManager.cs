using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System;
using UniRx;
using DarkTonic.MasterAudio;

public class MenuManager : Singleton<MenuManager>
{
    public Action OnMenuTransitionStart;
    public Action OnMenuTransitionEnd;
    public Action OnLevelStart;
    public Action OnMainMenu;
    public Action OnPause;
    public Action OnResume;

    public bool inTransition = false;

    [Header("UI")]
    public CanvasGroup UICanvasGroup;

    [Header("Menu")]
    public CanvasGroup menuCanvasGroup;
    public bool disableMenusOnHide = true;
    public Transform menuParent;
    public MenuComponent menuOnStart;
    public MenuComponent currentMenu;

    [Header("Main Menu")]
    public MenuComponent mainMenu;
    public RectTransform title;
    public float titleHiddenYPos = 260f;
    public Image MainMenuBackground, MainMenuSun, MainMenuCloud1, MainMenuCloud2, MainMenuLand, MainMenuTrees, MainMenuLocomotive, MainMenuContainer1, MainMenuContainer2;

    [Header("End Level Menu")]
    public float endLevelDelay = 2f;
    public MenuComponent endLevelMenu;

    [Header("Quit Menu")]
    public MenuComponent quitMenu;

    [Header("Pause Menu")]
    public Button pauseButton;
    public MenuComponent pauseMenu;
    public Button fastforwardButton;

    [Header("Menu Trophies")]
    public MenuTrophies menuTrophies;

    [Header("Menu Panel")]
    public CanvasGroup menuPanel;

    [Header("Menu Levels")]
    public MenuLevels menulevels;
    public MenuAllTrophies menuAllTrophies;

    [Header("Back Button")]
    public RectTransform backButton;
    public Vector2 backButtonHiddenPos;

    [Header("Menu Animations")]
    public Ease menuEase = Ease.OutQuad;
    public float menuAnimationDuration = 0.5f;
    public Vector2 menuHidePosition;
    public Vector2 menuShowPosition;

    [Header("Unlocked Stages")]
    public List<Stage_Menu> unlockedStages = new List<Stage_Menu>();

    [Header("Camera Movement")]
    public Ease movementEase = Ease.OutQuad;
    public float movementDuration;

    [Header("Menu Position")]
    public Vector3 menuPosition;
    public Vector3 menuRotation;

    [Header("Game Position")]
    public Vector3 gamePosition;
    public Vector3 gameRotation;

    [Header("Projection Switch")]
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
    private float _timeScaleOnPause;

    private bool MUSIC_Ingame = false;
    private bool MUSIC_MainMenu = false;

    // Use this for initialization
    void Start()
    {
        _camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        _menuPanelShowAlpha = menuPanel.alpha;

        _titleShowPos = title.anchoredPosition.y;

        _backButtonShowPos = backButton.anchoredPosition;

        Stage_Menu.OnStageUnlock += (Stage_Menu obj) => unlockedStages.Add(obj);

        GameManager.Instance.OnPlaying += () =>
        {
            if (!TutorialManager.Instance.isActive)
            {
                pauseButton.gameObject.SetActive(true);
                pauseButton.interactable = true;
                fastforwardButton.gameObject.SetActive(true);
            }
            if (MUSIC_Ingame == false)
            {
                MasterAudio.ChangePlaylistByName("InGame");
                MUSIC_Ingame = true;
                MUSIC_MainMenu = false;
            }

            MasterAudio.PlaySoundAndForget("SFX_Water");
        };

        GameManager.Instance.OnLevelEnd += () =>
        {
            pauseButton.interactable = false;
        };

        GameManager.Instance.OnMenu += () =>
        {
            //MasterAudio.PlaySound ("SFX_Swap");

        };

        if (GameManager.Instance.gameState == GameState.Playing)
            return;

        menuParent.gameObject.SetActive(true);
        //menuPanel.gameObject.SetActive(true);
        title.gameObject.SetActive(true);
        backButton.gameObject.SetActive(true);

        foreach (Transform t in menuParent)
            if (t.gameObject.GetComponent<MenuComponent>() != null)
                ClearMenu(t.gameObject.GetComponent<MenuComponent>());


        UIFadeOut();

        //MainMenu Load
        MainMenuBackground.gameObject.SetActive(true);
        MainMenuSun.gameObject.SetActive(true);
        MainMenuSun.transform.DOScale(0f, 1f).From().SetEase(Ease.OutBack).SetDelay(0.5f);
        title.DOScale(0f, 1f).From().SetEase(Ease.OutBack).SetDelay(0.3f);
        Observable.EveryUpdate().Subscribe(_ =>
            {
                MainMenuSun.transform.Rotate(0, 0, 15f * Time.deltaTime, Space.Self);
            });
        MainMenuLand.transform.DOLocalMoveY(-1000f, 0.5f).From().SetDelay(0.6f).SetEase(Ease.OutBack).OnStart(() => MainMenuLand.gameObject.SetActive(true));
        MainMenuTrees.transform.DOLocalMoveY(-1000f, 0.5f).From().SetDelay(0.7f).SetEase(Ease.OutBack).OnStart(() => MainMenuTrees.gameObject.SetActive(true));
        MainMenuCloud1.transform.DOLocalMoveX(-400, 0.5f).From().SetDelay(0.7f).OnStart(() => MainMenuCloud1.gameObject.SetActive(true));
        MainMenuCloud1.DOFade(0, 1f).From().SetDelay(0.7f);
        MainMenuCloud2.transform.DOLocalMoveX(400, 0.5f).From().SetDelay(0.75f).OnStart(() => MainMenuCloud2.gameObject.SetActive(true));
        MainMenuCloud2.DOFade(0, 1f).From().SetDelay(0.75f);
        MainMenuLocomotive.transform.DOLocalRotate(new Vector3(0, 0, 40), 2f).From().SetDelay(1.2f).SetEase(Ease.OutExpo).OnStart(() =>
            {
                MainMenuLocomotive.gameObject.SetActive(true);
                MasterAudio.PlaySoundAndForget("SFX_TrainMenu");
            });
        MainMenuContainer1.transform.DOLocalRotate(new Vector3(0, 0, 40 + 38.557f), 2f).From().OnStart(() => MainMenuContainer1.gameObject.SetActive(true)).SetDelay(1.2f).SetEase(Ease.OutExpo);
        MainMenuContainer2.transform.DOLocalRotate(new Vector3(0, 0, 40 + 38.557f + 38.557f), 2f).From().OnStart(() => MainMenuContainer2.gameObject.SetActive(true)).SetDelay(1.2f).SetEase(Ease.OutExpo);
        MainMenuCloud1.transform.DOLocalMoveX(1000, 30).SetSpeedBased().SetRelative().SetDelay(1.5f).SetLoops(-1, LoopType.Yoyo);
        MainMenuCloud2.transform.DOLocalMoveX(-1000, 30).SetSpeedBased().SetRelative().SetDelay(1.5f).SetLoops(-1, LoopType.Yoyo);

        BackButton(mainMenu);

        menulevels.SetupLevels();
        menuAllTrophies.SetupLevels();

        DOVirtual.DelayedCall(1.5f, () =>
            {
                if (menuOnStart != null)
                    ShowMenu(menuOnStart);
                else
                    ShowMenu(mainMenu, false);
            });

        SetupMatrix();
    }

    void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.Space))
		{
			if (_orthoOn)
				GamePosition ();
			else
				MenuPosition ();
		}*/

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentMenu && GameManager.Instance.gameState != GameState.Playing)
                Back();

            if (GameManager.Instance.gameState == GameState.Playing)
                PauseAndShowMenu(pauseMenu);

            if (currentMenu == quitMenu)
                QuitGame();

            if (currentMenu == mainMenu)
                QuitMenu();
        }
    }

    void BackButton(MenuComponent menu)
    {
        if (menu && menu.backToMainMenu || menu && menu.backMenu != null)
        {
            DOTween.Kill(backButton);
            backButton.DOAnchorPos(_backButtonShowPos, menuAnimationDuration).SetEase(menuEase).SetUpdate(true);
        }
        else
        {
            DOTween.Kill(backButton);
            backButton.DOAnchorPos(backButtonHiddenPos, menuAnimationDuration).SetEase(menuEase).SetUpdate(true);
        }
    }

    public void Back()
    {
        if (currentMenu.backToMainMenu)
            MainMenu();
        else if (currentMenu.backButton != null)
            currentMenu.backButton.onClick.Invoke();
        else if (currentMenu.backMenu != null)
            ToMenu(currentMenu.backMenu);
        else if (currentMenu.backEvent != null)
            currentMenu.backEvent.Invoke();

        MasterAudio.PlaySound("SFX_PopBack");

    }

    void SetupMatrix()
    {
        _matrixBlender = _camera.GetComponent<MatrixBlender>();

        _fov = _camera.fieldOfView;
        _near = _camera.nearClipPlane;
        _far = _camera.farClipPlane;
        _orthographicSize = _camera.orthographicSize;

        _aspect = (float)Screen.width / (float)Screen.height;
        _ortho = Matrix4x4.Ortho(-_orthographicSize * _aspect, _orthographicSize * _aspect, -_orthographicSize, _orthographicSize, _near, _far);
        _perspective = Matrix4x4.Perspective(_fov, _aspect, 0.01f, _far);
    }

    public void MenuPosition()
    {
        _orthoOn = !_orthoOn;
        _matrixBlender.BlendToMatrix(_perspective, switchDuration);

        DOTween.Kill(transform);

        _camera.transform.DOMove(menuPosition, movementDuration).SetEase(movementEase).SetUpdate(true);
        _camera.transform.DORotate(menuRotation, movementDuration).SetEase(movementEase).SetUpdate(true);
    }

    public void GamePosition()
    {
        _orthoOn = !_orthoOn;
        _matrixBlender.BlendToMatrix(_ortho, switchDuration);

        DOTween.Kill(transform);

        _camera.transform.DOMove(gamePosition, movementDuration).SetEase(movementEase).SetUpdate(true);
        _camera.transform.DORotate(gameRotation, movementDuration).SetEase(movementEase).SetUpdate(true);
    }

    public void HideEndLevel()
    {
        HideMenu(endLevelMenu);
    }

    public void ToMenu(MenuComponent menu, bool showPanel = true)
    {
        DOTween.Kill("Menu");

        if (currentMenu != null)
            HideMenu(currentMenu, menu);

        DOVirtual.DelayedCall(menuAnimationDuration, () => ShowMenu(menu, showPanel)).SetId("Menu").SetUpdate(true);
    }

    public void ShowMenu(MenuComponent menu, bool showPanel = true)
    {
        menuParent.gameObject.SetActive(true);

        menu.gameObject.SetActive(true);

        EnableMenuCanvas(menu);

        if (showPanel)
            ShowPanel();

        StartTransition();

        BackButton(menu);

        currentMenu = menu;

        float animationDuration = menuAnimationDuration;

        if (menu.mainContent)
        {
            DOTween.Kill(menu.mainContent);

            menu.mainContent.gameObject.SetActive(true);

            HideContent(menu.mainContent, menuShowPosition, menuHidePosition, menu.disableOnHide);
            //menu.mainContent.anchoredPosition = menuHidePosition;

            ShowContent(menu.mainContent, menuShowPosition, menuHidePosition, menuAnimationDuration, menuEase, menu.disableOnHide);
            //menu.mainContent.DOAnchorPos (menuShowPosition, menuAnimationDuration).SetEase (menuEase);
        }

        foreach (var c in menu.contents)
        {
            if (c.content == null)
                continue;

            if (!c.showOnShow)
                continue;

            c.content.gameObject.SetActive(true);

            DOTween.Kill(c.content);

            HideContent(c.content, c.showPosition, c.hidePosition, menu.disableOnHide);
            //c.content.anchoredPosition = c.hidePosition;

            float duration = c.overrideDuration ? c.duration : menuAnimationDuration;
            Ease ease = c.overrideEase ? c.ease : menuEase;

            ShowContent(c.content, c.showPosition, c.hidePosition, duration, ease, menu.disableOnHide, c.delay);
            /*if(c.delay > 0)
				c.content.DOAnchorPos (c.showPosition, duration).SetEase (ease).SetDelay (c.delay);
			else
				c.content.DOAnchorPos (c.showPosition, duration).SetEase (ease);*/

            if (duration + c.delay > animationDuration)
                animationDuration = duration + c.delay;
        }

        menu.OnShow();

        DOVirtual.DelayedCall(animationDuration, () => EndTransition(menu, false)).SetId("Menu").SetUpdate(true);
    }

    public void HideMenu(MenuComponent menu, MenuComponent toMenu = null)
    {
        StartTransition();

        float animationDuration = menuAnimationDuration;

        currentMenu = null;

        BackButton(toMenu);

        if (menu.mainContent)
        {
            DOTween.Kill(menu.mainContent);

            HideContent(menu.mainContent, menuShowPosition, menuHidePosition, menuAnimationDuration, menuEase, menu.disableOnHide, 0);
            //menu.mainContent.DOAnchorPos (menuHidePosition, menuAnimationDuration).SetEase (menuEase).OnComplete (()=> menu.mainContent.gameObject.SetActive (false));
        }

        foreach (var c in menu.contents)
        {
            if (c.content == null)
                continue;

            if (!c.hideOnHide)
                continue;

            DOTween.Kill(c.content);

            float duration = c.overrideDuration ? c.duration : menuAnimationDuration;
            Ease ease = c.overrideEase ? c.ease : menuEase;

            HideContent(c.content, c.showPosition, c.hidePosition, duration, ease, menu.disableOnHide, c.delay);

            //HideContent (c.content, c.showPosition, c.hidePosition, duration, ease, c.delay, ()=> c.content.gameObject.SetActive (false));
            /*if(c.delay > 0)
				c.content.DOAnchorPos (c.hidePosition, duration).SetEase (ease).SetDelay (c.delay).OnComplete (()=> c.content.gameObject.SetActive (false));
			else
				c.content.DOAnchorPos (c.hidePosition, duration).SetEase (ease).OnComplete (()=> c.content.gameObject.SetActive (false));*/

            if (duration + c.delay > animationDuration)
                animationDuration = duration + c.delay;
        }

        menu.OnHide();

        DOVirtual.DelayedCall(animationDuration, () => EndTransition(menu, true)).SetId("Menu").SetUpdate(true);
    }

    public void HideCurrentMenu()
    {
        if (currentMenu != null)
            HideMenu(currentMenu);
    }


    void HideContent(RectTransform content, Vector2 showPosition, Vector2 hidePosition, bool disable)
    {
        if (showPosition.x != hidePosition.x && showPosition.y != hidePosition.y)
            content.anchoredPosition = hidePosition;
        else if (showPosition.x != hidePosition.x && showPosition.y == hidePosition.y)
            content.anchoredPosition = new Vector2(hidePosition.x, content.anchoredPosition.y);
        else if (showPosition.x == hidePosition.x && showPosition.y != hidePosition.y)
            content.anchoredPosition = new Vector2(content.anchoredPosition.x, hidePosition.y);

        if (disable && disableMenusOnHide)
            content.gameObject.SetActive(false);
    }

    void HideContent(RectTransform content, Vector2 showPosition, Vector2 hidePosition, float duration, Ease ease, bool disable, float delay = 0, Action action = null)
    {
        if (showPosition.x != hidePosition.x && showPosition.y != hidePosition.y)
            content.DOAnchorPos(hidePosition, duration).SetEase(ease).SetDelay(delay).SetUpdate(true).OnComplete(() =>
                {

                    if (action != null)
                        action();

                    if (disable && disableMenusOnHide)
                        content.gameObject.SetActive(false);

                });
        else if (showPosition.x != hidePosition.x && showPosition.y == hidePosition.y)
            content.DOAnchorPosX(hidePosition.x, duration).SetEase(ease).SetDelay(delay).SetUpdate(true).OnComplete(() =>
                {

                    if (action != null)
                        action();

                    if (disable && disableMenusOnHide)
                        content.gameObject.SetActive(false);
                });
        else if (showPosition.x == hidePosition.x && showPosition.y != hidePosition.y)
            content.DOAnchorPosY(hidePosition.y, duration).SetEase(ease).SetDelay(delay).SetUpdate(true).OnComplete(() =>
                {

                    if (action != null)
                        action();

                    if (disable && disableMenusOnHide)
                        content.gameObject.SetActive(false);
                });
    }

    void ShowContent(RectTransform content, Vector2 showPosition, Vector2 hidePosition, bool disable)
    {
        if (showPosition.x != hidePosition.x && showPosition.y != hidePosition.y)
            content.anchoredPosition = showPosition;
        else if (showPosition.x != hidePosition.x && showPosition.y == hidePosition.y)
            content.anchoredPosition = new Vector2(showPosition.x, content.anchoredPosition.y);
        else if (showPosition.x == hidePosition.x && showPosition.y != hidePosition.y)
            content.anchoredPosition = new Vector2(content.anchoredPosition.x, showPosition.y);

        if (disable && disableMenusOnHide)
            content.gameObject.SetActive(true);
    }

    void ShowContent(RectTransform content, Vector2 showPosition, Vector2 hidePosition, float duration, Ease ease, bool disable, float delay = 0, Action action = null)
    {
        if (disable && disableMenusOnHide)
            content.gameObject.SetActive(true);

        if (showPosition.x != hidePosition.x && showPosition.y != hidePosition.y)
            content.DOAnchorPos(showPosition, duration).SetEase(ease).SetDelay(delay).SetUpdate(true).OnComplete(() =>
                {

                    if (action != null)
                        action();
                });
        else if (showPosition.x != hidePosition.x && showPosition.y == hidePosition.y)
            content.DOAnchorPosX(showPosition.x, duration).SetEase(ease).SetDelay(delay).SetUpdate(true).OnComplete(() =>
                {

                    if (action != null)
                        action();
                });
        else if (showPosition.x == hidePosition.x && showPosition.y != hidePosition.y)
            content.DOAnchorPosY(showPosition.y, duration).SetEase(ease).SetDelay(delay).SetUpdate(true).OnComplete(() =>
                {

                    if (action != null)
                        action();
                });
    }

    public void Pause(float delay)
    {
        this.transform.DOMove(this.transform.position + new Vector3(UnityEngine.Random.Range(-5, 5), UnityEngine.Random.Range(-5, 5), UnityEngine.Random.Range(-5, 5)), delay).OnComplete(() =>
            {
                if (GameManager.Instance.gameState == GameState.Playing)
                {
                    _timeScaleOnPause = Time.timeScale;
                    Time.timeScale = 0;

                    GameManager.Instance.gameState = GameState.Pause;
                }
            });

    }

    public void UnPause(float delay)
    {
        this.transform.DOKill();
        DOVirtual.DelayedCall(delay, () =>
            {
                if (GameManager.Instance.gameState == GameState.Pause)
                {
                    Time.timeScale = _timeScaleOnPause;
                    GameManager.Instance.gameState = GameState.Playing;
                }
            });

    }

    public void PauseAndShowMenu(MenuComponent menu)
    {
        menuParent.gameObject.SetActive(true);

        if (GameManager.Instance.gameState == GameState.Playing)
        {
            _timeScaleOnPause = Time.timeScale;
            Time.timeScale = 0;

            GameManager.Instance.gameState = GameState.Pause;

            UIFadeOut();

            if (OnPause != null)
                OnPause();
        }

        ShowMenu(menu, false);
    }

    public void ResumeAndHideMenu(MenuComponent menu)
    {
        HideMenu(menu);

        DOVirtual.DelayedCall(menuAnimationDuration, () =>
            {

                if (GameManager.Instance.gameState == GameState.Pause)
                {
                    Time.timeScale = _timeScaleOnPause;

                    UIFadeIn();
                    GameManager.Instance.gameState = GameState.Playing;

                    if (OnResume != null)
                        OnResume();

                    menuParent.gameObject.SetActive(false);
                }

            }).SetUpdate(true);
    }

    void ClearMenu(MenuComponent menu)
    {
        menu.gameObject.SetActive(true);

        DisableMenuCanvas(menu);

        if (menu.mainContent)
        {
            DOTween.Kill(menu.mainContent);
            menu.mainContent.anchoredPosition = menuHidePosition;
        }

        foreach (var c in menu.contents)
        {
            if (c.content == null)
                continue;

            DOTween.Kill(c.content);

            c.content.anchoredPosition = c.hidePosition;
        }

        /*if(menu != mainMenu)
			DOVirtual.DelayedCall (0.1f, ()=> menu.gameObject.SetActive (false));*/
    }

    void EnableMenuCanvas(MenuComponent menu)
    {
        menu.menuCanvasGroup.blocksRaycasts = true;
        menu.menuCanvasGroup.interactable = true;
    }

    void DisableMenuCanvas(MenuComponent menu)
    {
        if (menu.disableOnHide)
            menu.gameObject.SetActive(false);

        menu.menuCanvasGroup.blocksRaycasts = false;
        menu.menuCanvasGroup.interactable = false;
    }

    void ShowPanel()
    {
        UIFadeOut();

        menuPanel.gameObject.SetActive(true);

        if (menuPanel.alpha != _menuPanelShowAlpha)
        {
            DOTween.Kill(menuPanel);
            menuPanel.DOFade(_menuPanelShowAlpha, menuAnimationDuration).SetEase(menuEase).SetUpdate(true);
        }
    }

    void HidePanel()
    {
        if (menuPanel.alpha != 0)
        {
            DOTween.Kill(menuPanel);
            menuPanel.DOFade(0, menuAnimationDuration).SetEase(menuEase).SetUpdate(true).OnComplete(() => menuPanel.gameObject.SetActive(false));
        }
    }

    void StartTransition()
    {
        inTransition = true;

        if (OnMenuTransitionStart != null)
            OnMenuTransitionStart();
    }

    void EndTransition(MenuComponent menu, bool disableMenu)
    {
        inTransition = false;

        if (OnMenuTransitionEnd != null)
            OnMenuTransitionEnd();

        if (disableMenu)
        {
            DisableMenuCanvas(menu);

            //menu.gameObject.SetActive (false);
        }
    }

    public void EndLevel()
    {
        DOVirtual.DelayedCall(endLevelDelay, () =>
            {
                UIFadeOut();

                if (unlockedStages.Count > 0)
                    ShowTrophyMenu();
                else
                    ToMenu(endLevelMenu, false);

            }).SetUpdate(true);
    }

    void ShowTrophyMenu()
    {
        menuTrophies.endLevel = true;

        var stage = unlockedStages[0];

        MenuManager.Instance.menuTrophies.stageMenu = stage;
        MenuManager.Instance.ToMenu(MenuManager.Instance.menuTrophies);

        unlockedStages.RemoveAt(0);
    }

    public void HideTrophyMenu()
    {
        if (unlockedStages.Count > 0)
            ShowTrophyMenu();
        else
        {
            menuTrophies.endLevel = false;
            ToMenu(endLevelMenu, false);
        }
    }

    public void MainMenu()
    {
        Time.timeScale = 1;
        if (MUSIC_MainMenu == false)
        {
            MasterAudio.ChangePlaylistByName("MainMenu");
            MUSIC_Ingame = false;
            MUSIC_MainMenu = true;
        }

        if (TutorialManager.Instance.isActive)
        {
            TutorialManager.Instance.ForceStop();
        }

        //ShowPanel();

        ShowTitle();

        ToMenu(mainMenu);

        DOVirtual.DelayedCall(menuAnimationDuration, () =>
            {

                if (OnMainMenu != null)
                    OnMainMenu();
            }).SetUpdate(true);
    }

    public void RetryLevel()
    {
        StartCoroutine(LoadLevel(() =>
                {
                    LevelsManager.Instance.LoadLevel(LevelsManager.Instance.levelIndex);
                }));
    }

    public void NextLevel()
    {
        StartCoroutine(LoadLevel(() =>
                {
                    LevelsManager.Instance.NextLevel();
                }));
    }

    public void StartLevel()
    {
        StartCoroutine(LoadLevel());
    }

    IEnumerator LoadLevel(Action action = null)
    {
        Time.timeScale = 1;

        ShowPanel();

        ContainersMovementManager.Instance.DeselectContainer();

        HideTitle();

        HideCurrentMenu();

        yield return new WaitForSeconds(menuAnimationDuration);

        if (OnLevelStart != null)
            OnLevelStart();

        if (action != null)
            action();

        yield return new WaitWhile(() => LevelsGenerationManager.Instance.isGeneratingLevel);

        yield return new WaitForSeconds(0.1f);

        HidePanel();
        UIFadeIn();
        GameManager.Instance.StartLevel();

        yield return new WaitForSeconds(menuAnimationDuration);

        menuParent.gameObject.SetActive(false);
    }

    public void UIFadeOut()
    {
        UICanvasGroup.DOFade(0, menuAnimationDuration).SetEase(menuEase).SetUpdate(true).OnComplete(() =>
            {
                UICanvasGroup.blocksRaycasts = false;
                UICanvasGroup.interactable = false;
            });
    }

    public void UIFadeIn()
    {
        UICanvasGroup.gameObject.SetActive(true);

        UICanvasGroup.blocksRaycasts = true;
        UICanvasGroup.interactable = true;

        UICanvasGroup.DOFade(1, menuAnimationDuration).SetEase(menuEase).SetUpdate(true);
    }

    void ShowTitle()
    {
        title.gameObject.SetActive(true);
        title.DOAnchorPosY(_titleShowPos, menuAnimationDuration).SetEase(menuEase).SetUpdate(true);
    }

    void HideTitle()
    {
        title.DOAnchorPosY(titleHiddenYPos, menuAnimationDuration).SetEase(menuEase).OnComplete(() => title.gameObject.SetActive(false)).SetUpdate(true);
    }

    public void QuitMenu()
    {
        ToMenu(quitMenu);
    }

    public void HideQuitMenu()
    {
        ToMenu(mainMenu);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void Reset()
    {
        unlockedStages.Clear();
    }
}
