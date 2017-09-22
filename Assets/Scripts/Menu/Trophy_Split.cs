using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trophy_Split : MonoBehaviour
{
	public Transform Locomotive;

	public Stage_Menu sm;

	public GameObject LastTrophy;

	void Start ()
	{
		if (Locomotive.childCount != 16) {
			Debug.LogError ("Trophy Locomotive should have 16 parts, not " + Locomotive.childCount);
		}
		if (sm == null) {
			sm = MenuManager.Instance.menuTrophies.stageMenu;
		}



	}

	void OnEnable ()
	{
		if (sm == null) {
			sm = MenuManager.Instance.menuTrophies.stageMenu;
		}

		if (sm.trophyStageIndex == 16) {
			LastTrophy.SetActive (true);
			for (int i = 0; i < Locomotive.childCount; i++) {
				Locomotive.GetChild (i).gameObject.SetActive (false);
			}
		} else {
			LastTrophy.SetActive (false);
			for (int i = 0; i < sm.trophyStageIndex + 1; i++) {
				Locomotive.GetChild (i).gameObject.SetActive (true);
			}
			
			for (int i = sm.trophyStageIndex + 1; i < Locomotive.childCount; i++) {
				Locomotive.GetChild (i).gameObject.SetActive (false);
			}
		}

	}
	

}
