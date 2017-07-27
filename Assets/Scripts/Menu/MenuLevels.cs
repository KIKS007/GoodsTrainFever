using System.Collections;
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

	[Header ("Levels Stars Fade")]
	public float starsFadeOutValue;

	private float _levelsPanelWidth;

	// Use this for initialization
	void Start () 
	{
		_levelsPanelWidth = levelPanelPrefab.GetComponent<RectTransform> ().sizeDelta.x;

		SetupLevels ();
	}

	void OnEnable ()
	{
		for(int i = 0; i < LevelsManager.Instance.levelsCount; i++)
		{
			if (i >= levelsScrollView.childCount)
				break;

			Level_Menu levelMenu = levelsScrollView.GetChild (i).GetComponent<Level_Menu> ();
			Level level = LevelsManager.Instance.transform.GetChild (i).GetComponent<Level> ();

			levelMenu.Setup (i, level);
		}
	}

	[ButtonAttribute]
	public void SetupLevels ()
	{
		foreach (Transform t in levelsScrollView.transform)
			Destroy (t.gameObject);

		for(int i = 0; i < LevelsManager.Instance.levelsCount; i++)
		{
			Level level = LevelsManager.Instance.transform.GetChild (i).GetComponent<Level> ();

			Vector2 panelPosition = levelPosition;
			panelPosition.x += (_levelsPanelWidth + levelsSpacing) * i;

			RectTransform panel = (Instantiate (levelPanelPrefab, Vector3.zero, Quaternion.identity, levelsScrollView).GetComponent<RectTransform> ());
			panel.localPosition = Vector3.zero;
			panel.localRotation = Quaternion.Euler (Vector3.zero);

			panel.anchoredPosition = panelPosition;

			Level_Menu levelMenu = panel.GetComponent<Level_Menu> ();

			levelMenu.Setup (i, level);
		}

		float scrollViewWidth = (_levelsPanelWidth + levelsSpacing) * (LevelsManager.Instance.levelsCount) + levelPosition.x - levelsSpacing;
		levelsScrollView.sizeDelta = new Vector2 (scrollViewWidth, levelsScrollView.sizeDelta.y);
	}
}
