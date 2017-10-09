using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;
using DG.Tweening;

public class MenuLevels : MenuComponent
{
    [Header("Level Prefab")]
    public GameObject levelPanelPrefab;

    [Header("Level Parent")]
    public RectTransform levelsScrollView;

    [Header("Levels Layout")]
    public Vector2 levelPosition = new Vector2();
    public float levelsSpacing;

    [Header("Stages")]
    public GameObject levelStagePrefab;

    [Header("Reset")]
    public float resetDuration = 0.5f;

    [Header("Positions Bounds")]
    public float positionBound = 20f;

    private float _levelsPanelWidth;
    [HideInInspector]
    public List<Level_Menu> _levelsMenu = new List<Level_Menu>();

    // Use this for initialization
    void Start()
    {
        //SetupLevels ();
        GameManager.Instance.OnLevelEnd += SetLevelPosition;
        MenuManager.Instance.OnMainMenu += SetLevelPosition;
    }

    void OnEnable()
    {
        UpdateLevels();
    }

    public override void OnShow()
    {
        base.OnShow();

        MenuManager.Instance.menuTrophies.backMenu = this;

        if (PlayerPrefs.HasKey("LevelsScrollRect"))
        {
            //Debug.Log ("Bite");

            float x = PlayerPrefs.GetFloat("LevelsScrollRect");
            levelsScrollView.anchoredPosition = new Vector2(x, levelsScrollView.anchoredPosition.y);
            //_scrollRect.horizontalNormalizedPosition = x;
        }
    }

    void Update()
    {
        /*Debug.Log(levelsScrollView.GetChild(0).transform.position.x);
        Debug.Log(transform.position.x);*/

        foreach (Transform t in levelsScrollView)
        {
            if (t.position.x < transform.position.x - positionBound || t.position.x > transform.position.x + positionBound)
            {
                if (t.gameObject.activeSelf)
                    t.gameObject.SetActive(false);
            }
            else
            {
                if (!t.gameObject.activeSelf)
                    t.gameObject.SetActive(true);
            }
        }
    }

    void SetLevelPosition()
    {
        //LevelsManager.Instance.levelIndex

        int index = LevelsManager.Instance.levelIndex;
        int stages = 0;

        foreach (var s in ScoreManager.Instance.levelStages)
            if (s.index <= index)
                stages++;

        index += stages;

        if (LevelsManager.Instance.currentLevel != null)
        {
            if (LevelsManager.Instance.currentLevel.starsEarned > 0)
                index++;
			
        }

        levelsScrollView.anchoredPosition = new Vector2(-index * (levelPanelPrefab.GetComponent<RectTransform>().sizeDelta.x + levelsSpacing) - 6, levelsScrollView.anchoredPosition.y);
    }

    public void UpdateLevels()
    {
        UpdateLevelStages();

        for (int i = 0; i < LevelsManager.Instance.levelsCount; i++)
        {
            if (i >= levelsScrollView.childCount)
                break;

            Level_Menu levelMenu = _levelsMenu[i];
            Level level = LevelsManager.Instance.transform.GetChild(i).GetComponent<Level>();

            levelMenu.Setup(i, level);
        }
    }

    [ButtonAttribute]
    public void SetupLevels()
    {
        _levelsPanelWidth = levelPanelPrefab.GetComponent<RectTransform>().sizeDelta.x;

        foreach (Transform t in levelsScrollView.transform)
            Destroy(t.gameObject);

        int panelsCount = 0;
        Stage_Menu stageMenu = null;

        _levelsMenu.Clear();

        for (int i = 0; i < LevelsManager.Instance.levelsCount; i++)
        {
            Level level = LevelsManager.Instance.transform.GetChild(i).GetComponent<Level>();

            Vector2 panelPosition = levelPosition;
            panelPosition.x += (_levelsPanelWidth + levelsSpacing) * panelsCount;

            RectTransform panel = (Instantiate(levelPanelPrefab, Vector3.zero, Quaternion.identity, levelsScrollView).GetComponent<RectTransform>());
            panel.localPosition = Vector3.zero;
            panel.localRotation = Quaternion.Euler(Vector3.zero);

            panel.anchoredPosition = panelPosition;

            Level_Menu levelMenu = panel.GetComponent<Level_Menu>();

            _levelsMenu.Add(levelMenu);

            levelMenu.Setup(i, level, stageMenu);

            panelsCount++;

            Stage_Menu stageTemp = SetupLevelStage(i, panelsCount);

            if (stageTemp != null)
            {
                stageMenu = stageTemp;
                panelsCount++;
            }
        }

        float scrollViewWidth = (_levelsPanelWidth + levelsSpacing) * (panelsCount) + levelPosition.x - levelsSpacing;
        levelsScrollView.sizeDelta = new Vector2(scrollViewWidth, levelsScrollView.sizeDelta.y);

        if (PlayerPrefs.HasKey("LevelsScrollRect"))
        {
            //Debug.Log ("Bite");

            float x = PlayerPrefs.GetFloat("LevelsScrollRect");
            levelsScrollView.anchoredPosition = new Vector2(x, levelsScrollView.anchoredPosition.y);
            //_scrollRect.horizontalNormalizedPosition = x;
        }

        UpdateLevelStages();
    }

    Stage_Menu SetupLevelStage(int index, int panelsCount)
    {
        for (int i = 0; i < ScoreManager.Instance.levelStages.Count; i++)
        {
            if (ScoreManager.Instance.levelStages[i].index == index + 1)
            {
                Vector2 stagePanelPosition = levelPosition;
                stagePanelPosition.x += (_levelsPanelWidth + levelsSpacing) * panelsCount;

                RectTransform stagePanel = (Instantiate(levelStagePrefab, Vector3.zero, Quaternion.identity, levelsScrollView).GetComponent<RectTransform>());
                stagePanel.localPosition = Vector3.zero;
                stagePanel.localRotation = Quaternion.Euler(Vector3.zero);

                stagePanel.anchoredPosition = stagePanelPosition;

                Stage_Menu stage = stagePanel.GetComponent<Stage_Menu>();

                ScoreManager.Instance.levelStages[i].stage = stage;

                stage.trophyStageIndex = i;

                stage.Setup(false, ScoreManager.Instance.levelStages[i].starsRequired);

                return stage;
            }
        }

        return null;
    }

    void UpdateLevelStages()
    {
        int starsRequired = 0;

        foreach (var s in ScoreManager.Instance.levelStages)
        {
            int stars = s.starsRequired;

            if ((ScoreManager.Instance.starsEarned - starsRequired) > 0)
                stars = s.starsRequired - (ScoreManager.Instance.starsEarned - starsRequired);

            if (s.stage != null)
                s.stage.Setup(ScoreManager.Instance.starsEarned >= s.starsRequired + starsRequired, stars);

            starsRequired += s.starsRequired;
        }


    }

    void OnDestroy()
    {
        PlayerPrefs.SetFloat("LevelsScrollRect", levelsScrollView.anchoredPosition.x);
    }

    public override void OnHide()
    {
        base.OnHide();

        SaveMenuPos();
    }

    public void SaveMenuPos()
    {
        //Debug.Log ("Saving: " + levelsScrollView.anchoredPosition.x);
        PlayerPrefs.SetFloat("LevelsScrollRect", levelsScrollView.anchoredPosition.x);
    }

    public void Reset()
    {
        levelsScrollView.anchoredPosition = new Vector2(0, levelsScrollView.anchoredPosition.y);
    }
}
