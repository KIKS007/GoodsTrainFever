using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Level_Menu : MonoBehaviour
{
    public int levelIndex;
    public bool isUnlocked = true;
    public Stage_Menu levelStage = null;

    [Header("Elements")]
    public Text levelTitle;
    public Text errorsCount;
    public GameObject lockImage;
    public GameObject playButton;

    [Header("Stars")]
    public Image[] stars = new Image[3];

    [Header("Fade")]
    public float lockFade = 0.8f;

    public void Setup(int index, Level level, Stage_Menu stage = null)
    {
        levelIndex = index;

        if (stage != null)
            levelStage = stage;

        levelTitle.text = "Niveau " + (index + 1).ToString();

        for (int i = 0; i < 3; i++)
        {
            if (i < level.starsEarned)
                stars[i].gameObject.SetActive(true);
            else
                stars[i].gameObject.SetActive(false);
        }

        if (levelStage == null || levelStage.isUnlocked)
            isUnlocked = true;
        else
            isUnlocked = false;

        if (!isUnlocked)
        {
            playButton.SetActive(false);
            lockImage.SetActive(true);
            GetComponent<CanvasGroup>().alpha = lockFade;
        }
        else
        {
            playButton.SetActive(true);
            lockImage.SetActive(false);
            GetComponent<CanvasGroup>().alpha = 1;
        }

        errorsCount.text = level.errorsAllowed.ToString();
    }

    public void Play()
    {
        LevelsManager.Instance.LoadLevel(levelIndex);
        MenuManager.Instance.StartLevel();
    }

    public void ResetLevel()
    {
        ScoreManager.Instance.ResetLevelStars(levelIndex);
    }
}
