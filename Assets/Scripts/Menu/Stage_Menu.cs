using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Stage_Menu : MonoBehaviour 
{
	public int starsRequired;
	public bool isUnlocked = false;

	[Header ("Elements")]
	public GameObject innerStar;
	public Text starsCount;
	public GameObject lockImage;
	public Button trophyButton;

	void Start ()
	{
		trophyButton.onClick.AddListener (()=> 
			{
				MenuManager.Instance.menuTrophies.stageMenu = this;
				MenuManager.Instance.ToMenu (MenuManager.Instance.menuTrophies);
			});
	}

	public void Setup (bool unlock, int stars)
	{
		starsRequired = stars;
		starsCount.text = starsRequired.ToString ();

		isUnlocked = unlock;

		innerStar.SetActive (!unlock);

		lockImage.SetActive (!unlock);

		trophyButton.gameObject.SetActive (unlock);

		starsCount.transform.parent.gameObject.SetActive (!unlock);
	}
}
