using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;
using DG.Tweening;

public class MenuAllTrophies : MenuComponent
{
	[Header ("Stages Parent")]
	public RectTransform levelsScrollView;

	[Header ("Stages Layout")]
	public Vector2 levelPosition = new Vector2 ();
	public float levelsSpacing;

	[Header ("Stages")]
	public GameObject levelStagePrefab;

	[Header ("Reset")]
	public float resetDuration = 0.5f;

	private float _levelsPanelWidth;

	private List<Stage_Menu> _stagesMenu = new List<Stage_Menu> ();

	// Use this for initialization
	void Start ()
	{
		//SetupLevels ();
	}

	void OnEnable ()
	{
		UpdateLevels ();
	}

	public override void OnShow ()
	{
		base.OnShow ();

		MenuManager.Instance.menuTrophies.backMenu = this;
	}

	public void UpdateLevels ()
	{
		UpdateLevelStages ();
	}

	[ButtonAttribute]
	public void SetupLevels ()
	{
		_levelsPanelWidth = levelStagePrefab.GetComponent<RectTransform> ().sizeDelta.x;

		foreach (Transform t in levelsScrollView.transform)
			Destroy (t.gameObject);

		int panelsCount = 0;

		foreach (var s in ScoreManager.Instance.levelStages) {
			_stagesMenu.Add (SetupLevelStage (s, panelsCount));
			panelsCount++;
		}

		float scrollViewWidth = (_levelsPanelWidth + levelsSpacing) * (panelsCount) + levelPosition.x - levelsSpacing;
		levelsScrollView.sizeDelta = new Vector2 (scrollViewWidth, levelsScrollView.sizeDelta.y);

		if (PlayerPrefs.HasKey ("TrophiesScrollRect")) {
			float x = PlayerPrefs.GetFloat ("TrophiesScrollRect");
			levelsScrollView.anchoredPosition = new Vector2 (x, levelsScrollView.anchoredPosition.y);
		}

		UpdateLevelStages ();
	}

	Stage_Menu SetupLevelStage (Stage s, int panelsCount)
	{
		Vector2 stagePanelPosition = levelPosition;
		stagePanelPosition.x += (_levelsPanelWidth + levelsSpacing) * panelsCount;
		
		RectTransform stagePanel = (Instantiate (levelStagePrefab, Vector3.zero, Quaternion.identity, levelsScrollView).GetComponent<RectTransform> ());
		stagePanel.localPosition = Vector3.zero;
		stagePanel.localRotation = Quaternion.Euler (Vector3.zero);
		
		stagePanel.anchoredPosition = stagePanelPosition;
		
		Stage_Menu stage = stagePanel.GetComponent<Stage_Menu> ();
		stage._allTrophiesMenu = true;
		
		stage.trophyStageIndex = panelsCount;

		stage.Setup (false, s.starsRequired);

		return stage;
	}

	void UpdateLevelStages ()
	{
		if (_stagesMenu.Count == 0)
			return;

		int starsRequired = 0;

		for (int i = 0; i < ScoreManager.Instance.levelStages.Count; i++) {
			int stars = ScoreManager.Instance.levelStages [i].starsRequired;

			if ((ScoreManager.Instance.starsEarned - starsRequired) > 0)
				stars = ScoreManager.Instance.levelStages [i].starsRequired - (ScoreManager.Instance.starsEarned - starsRequired);

			_stagesMenu [i].Setup (ScoreManager.Instance.starsEarned >= ScoreManager.Instance.levelStages [i].starsRequired + starsRequired, stars);

			starsRequired += ScoreManager.Instance.levelStages [i].starsRequired;
		}
	}

	void OnDestroy ()
	{
		PlayerPrefs.SetFloat ("TrophiesScrollRect", levelsScrollView.anchoredPosition.x);
	}



}
