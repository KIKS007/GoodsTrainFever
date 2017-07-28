﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class MenuLevels : MenuComponent 
{
	[Header ("Level Prefab")]
	public GameObject levelPanelPrefab;

	[Header ("Level Parent")]
	public RectTransform levelsScrollView;

	[Header ("Levels Layout")]
	public Vector2 levelPosition = new Vector2 ();
	public float levelsSpacing;

	[Header ("Stages")]
	public GameObject levelStagePrefab;

	private float _levelsPanelWidth;
	[HideInInspector]
	public List<Level_Menu> _levelsMenu = new List<Level_Menu> ();

	// Use this for initialization
	void Start () 
	{
		_levelsPanelWidth = levelPanelPrefab.GetComponent<RectTransform> ().sizeDelta.x;

		SetupLevels ();
	}

	void OnEnable ()
	{
		UpdateLevels ();
	}

	public void UpdateLevels ()
	{
		UpdateLevelStages ();

		for(int i = 0; i < LevelsManager.Instance.levelsCount; i++)
		{
			if (i >= levelsScrollView.childCount)
				break;

			Level_Menu levelMenu = _levelsMenu [i];
			Level level = LevelsManager.Instance.transform.GetChild (i).GetComponent<Level> ();

			levelMenu.Setup (i, level);
		}
	}

	[ButtonAttribute]
	public void SetupLevels ()
	{
		foreach (Transform t in levelsScrollView.transform)
			Destroy (t.gameObject);

		int panelsCount = 0;
		Stage_Menu stageMenu = null;

		_levelsMenu.Clear ();

		for(int i = 0; i < LevelsManager.Instance.levelsCount; i++)
		{
			Level level = LevelsManager.Instance.transform.GetChild (i).GetComponent<Level> ();

			Vector2 panelPosition = levelPosition;
			panelPosition.x += (_levelsPanelWidth + levelsSpacing) * panelsCount;

			RectTransform panel = (Instantiate (levelPanelPrefab, Vector3.zero, Quaternion.identity, levelsScrollView).GetComponent<RectTransform> ());
			panel.localPosition = Vector3.zero;
			panel.localRotation = Quaternion.Euler (Vector3.zero);

			panel.anchoredPosition = panelPosition;

			Level_Menu levelMenu = panel.GetComponent<Level_Menu> ();

			_levelsMenu.Add (levelMenu);

			levelMenu.Setup (i, level, stageMenu);

			panelsCount++;

			Stage_Menu stageTemp = SetupLevelStage (i, panelsCount);
			if(stageTemp != null)
			{
				stageMenu = stageTemp;
				panelsCount++;
			}
		}

		float scrollViewWidth = (_levelsPanelWidth + levelsSpacing) * (panelsCount) + levelPosition.x - levelsSpacing;
		levelsScrollView.sizeDelta = new Vector2 (scrollViewWidth, levelsScrollView.sizeDelta.y);

		UpdateLevelStages ();
	}

	Stage_Menu SetupLevelStage (int index, int panelsCount)
	{
		foreach(var s in ScoreManager.Instance.levelStages)
		{
			if(s.index == index + 1)
			{
				Vector2 stagePanelPosition = levelPosition;
				stagePanelPosition.x += (_levelsPanelWidth + levelsSpacing) * panelsCount;

				RectTransform stagePanel = (Instantiate (levelStagePrefab, Vector3.zero, Quaternion.identity, levelsScrollView).GetComponent<RectTransform> ());
				stagePanel.localPosition = Vector3.zero;
				stagePanel.localRotation = Quaternion.Euler (Vector3.zero);

				stagePanel.anchoredPosition = stagePanelPosition;

				Stage_Menu stage = stagePanel.GetComponent<Stage_Menu> ();
				s.stage = stage;

				stage.Setup (false, s.starsRequired);

				return stage;
			}
		}

		return null;
	}

	void UpdateLevelStages ()
	{
		int starsRequired = 0;

		foreach(var s in ScoreManager.Instance.levelStages)
		{
			int stars = s.starsRequired;

			if((ScoreManager.Instance.starsEarned - starsRequired) > 0)
				stars = s.starsRequired - (ScoreManager.Instance.starsEarned - starsRequired);

			if(s.stage != null)
				s.stage.Setup (ScoreManager.Instance.starsEarned >= s.starsRequired + starsRequired, stars);

			starsRequired += s.starsRequired;
		}
	}
}
