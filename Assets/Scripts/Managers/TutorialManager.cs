using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.Serialization;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine.Events;
using UnityEngine.UI;
using DG.Tweening;

public class TutorialManager : Singleton<TutorialManager>
{
	[TabGroup ("Tutorial Level", "Level 1")]
	public Tutorial[] TutorialList;
	[TabGroup ("Tutorial Level", "Level 2")]
	public Tutorial[] TutorialList2;
	[TabGroup ("Tutorial Level", "Level 3")]
	public Tutorial[] TutorialList3;
	[TabGroup ("Tutorial Level", "Level 4")]
	public Tutorial[] TutorialList4;
	[TabGroup ("Tutorial Level", "Level 5")]
	public Tutorial[] TutorialList5;
	[TabGroup ("Tutorial Level", "Level 6")]
	public Tutorial[] TutorialList6;
	[TabGroup ("Tutorial Level", "Level 8")]
	public Tutorial[] TutorialList8;
	[TabGroup ("Tutorial Level", "Level 9")]
	public Tutorial[] TutorialList9;
	[TabGroup ("Tutorial Level", "Level 10")]
	public Tutorial[] TutorialList10;
	[TabGroup ("Tutorial Level", "Level 13")]
	public Tutorial[] TutorialList13;
	[TabGroup ("Tutorial Level", "Level 26")]
	public Tutorial[] TutorialList26;
	private  Tutorial[] CurrentList;
	[Header ("General Settings")]
	public Tutorial CurrentTutorial;
	public Canvas MainTutorialCanvas;
	public bool isActive;
	public float TextSpeed;
	public bool BlockAllTutorial;
	private int CurrentTutorialListID;
	public GameObject TouchImage;

	[Header ("Objects to Hide during Tutorials")]
	public GameObject[] ObjectsToHide;

	public void LaunchTutorial (int id)
	{
		if (!BlockAllTutorial && !CheckTutorialDone (CurrentTutorialListID)) {
			HideObjects (false);
			if (isActive) {
				ForceStop ();
			} 
			isActive = true;
			if (id < CurrentList.Length) {
				CurrentTutorial = CurrentList [id];
				CurrentTutorial.StartTutorial (id, CurrentTutorialListID);
			} else {
				Debug.LogError ("The current tutorial list does not contain tutorial id: " + id);
			}
		}

	}

	//WIP HERE
	private void HideObjects (bool type)
	{
		foreach (GameObject go in ObjectsToHide) {
			go.SetActive (type);
		}
	}

	public void SwitchTutorialList (int id)
	{

		HideVisualFeedback ();
		CurrentTutorialListID = id;

		switch (id) {
		case 1:
			CurrentList = TutorialList;
			break;
		case 2:
			CurrentList = TutorialList2;
			break;
		case 3:
			CurrentList = TutorialList3;
			break;
		case 4:
			CurrentList = TutorialList4;
			break;
		case 5:
			CurrentList = TutorialList5;
			break;
		case 6:
			CurrentList = TutorialList6;
			break;
		case 8:
			CurrentList = TutorialList8;
			break;
		case 9:
			CurrentList = TutorialList9;
			break;
		case 10:
			CurrentList = TutorialList10;
			break;
		case 13:
			CurrentList = TutorialList13;
			break;
		case 26:
			CurrentList = TutorialList26;
			break;
		default:
			CurrentList = TutorialList;
			Debug.Log ("Tutorial List Switching to an undifined Tutorial ID! Please set it. Setting it to 1 to avoid ERRORS");
			break;
		}
	}

	public void NextTutorial (bool force)
	{
		HideObjects (false);
		HideVisualFeedback ();
		StopTextAnimation ();
		if (CurrentTutorial.StopTutorial () || force) {
			if (force) {
				CurrentTutorial.ForceStopTutorial ();
			}
			if (!CheckTutorialDone (CurrentTutorialListID)) {
				LaunchTutorial (CurrentTutorial.TutorialID + 1);
			}
		} 


	}

	public void ForceStop ()
	{
		HideVisualFeedback ();
		if (isActive) {
			
			this.transform.DOKill ();
			CurrentTutorial.ForceStopTutorial ();
			isActive = false;
		}

	}

	public void ForceStopandSave ()
	{
		if (isActive) {
			HideVisualFeedback ();
			SaveTutorialProgression (CurrentTutorialListID);
			this.transform.DOKill ();
			CurrentTutorial.ForceStopTutorial ();
			isActive = false;
		}

	}

	public void StopTutorial ()
	{
		HideVisualFeedback ();
		SaveTutorialProgression (CurrentTutorialListID);
		CurrentTutorial.StopTutorial ();
		isActive = false;
	}


	public void StopTextAnimation ()
	{
		StopAllCoroutines ();
	}

	public IEnumerator AnimateText (Tutorial TargetTutorial, Text targetUIText, string strComplete)
	{
		int i = 0;
		string str = "";
		while (i < strComplete.Length) {
			str += strComplete [i++];
			targetUIText.text = str;
			yield return new WaitForSecondsRealtime (TextSpeed);
		}
		TargetTutorial.TextFinished = true;
		ShowVisualFeedback ();
	}

	public void Selected ()
	{
		CurrentTutorial.Selected.Invoke ();
	}

	public void OnTrain ()
	{
		CurrentTutorial.OnTrain.Invoke ();
	}

	public void ToggleBlockAllTutorial (bool value)
	{
		BlockAllTutorial = !value;
	}

	public void OrderCompleted ()
	{
		CurrentTutorial.OrderCompleted.Invoke ();
	}

	public void OrderSent ()
	{
		CurrentTutorial.OrderSent.Invoke ();
	}

	public void WaitandNext (float delay)
	{
		this.transform.DOMove (this.transform.position + new Vector3 (Random.Range (-5, 5), Random.Range (-5, 5), Random.Range (-5, 5)), delay).OnComplete (() => {
			NextTutorial (true);
		});
	}

	public void SaveTutorialProgression (int id)
	{
		HideObjects (true);
		PlayerPrefs.SetInt ("Tutorial-" + id, 1);
		PlayerPrefs.Save ();
		//Debug.Log ("Tutorial-" + id + " saved completed");
	}

	private bool CheckTutorialDone (int id)
	{
		if (PlayerPrefs.GetInt ("Tutorial-" + id, 0) == 1) {
			//Debug.Log ("Tutorial-" + id + " already completed");
			return true;
		} else {
			//Debug.Log ("Tutorial-" + id + " not completed");
			return false;
		}
	}

	public void ResetTutorial (int id)
	{
		PlayerPrefs.DeleteKey ("Tutorial-" + id);
	}

	[Button]
	public void ResetAllTutorials ()
	{
		for (int i = 0; i < 100; i++) {
			PlayerPrefs.DeleteKey ("Tutorial-" + i);
		}
	}

	public void ShowVisualFeedback ()
	{
		if (isActive) {
			
			TouchImage.SetActive (true);
			TouchImage.transform.DOScale (1.2f, 0.8f).SetLoops (-1, LoopType.Yoyo).SetUpdate (true);
		}
	}


	public void HideVisualFeedback ()
	{
		TouchImage.transform.DOKill ();
		TouchImage.transform.localScale = Vector3.one;
		TouchImage.SetActive (false);
	}
}

[System.Serializable]
public class Tutorial
{
	public UnityEvent OnStartTutorial;
	public UnityEvent OnStopTutorial;
	public UnityEvent OnNextTutorial;
	public UnityEvent Selected;
	public UnityEvent OnTrain;
	public UnityEvent OrderCompleted;
	public UnityEvent OrderSent;
	public GameObject TutorialUI;
	public int TutorialID;
	public int TutorialListID;

	private string TextOnUI;
	private Text UIText;
	public bool TextFinished;

	public void StartTutorial (int id, int listID)
	{
		TextFinished = false;
		TutorialID = id;
		TutorialListID = listID;
		UIText = TutorialUI.GetComponentInChildren<Text> ();
		//TextOnUI = UIText.text;
		TextOnUI = LocalizationManager.Singleton.GetText ("TUTO-" + TutorialListID + "-" + TutorialID);
		//Debug.Log ("TUTO-" + TutorialListID + "-" + TutorialID);
		UIText.text = "";
		TutorialManager.Instance.StopTextAnimation ();
		TutorialManager.Instance.StartCoroutine (TutorialManager.Instance.AnimateText (this, UIText, TextOnUI));
		TutorialUI.SetActive (true);
		OnStartTutorial.Invoke ();
	}

	public bool StopTutorial ()
	{
		if (TextFinished) {
			TutorialUI.SetActive (false);
			OnStopTutorial.Invoke ();
			return true;
		} else {
			TutorialManager.Instance.StopTextAnimation ();
			UIText.text = TextOnUI;
			TextFinished = true;
			return false;
		}

	}

	public void ForceStopTutorial ()
	{
		
		OnStopTutorial.Invoke ();
		TextFinished = true;
		TutorialUI.SetActive (false);
	}




}