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

	public void LaunchTutorial (int id)
	{
		if (!BlockAllTutorial) {
			if (isActive) {
				ForceStop ();
			} 
			isActive = true;
			if (id < CurrentList.Length) {
				CurrentTutorial = CurrentList [id];
				CurrentTutorial.StartTutorial (id);
			} else {
				Debug.LogError ("The current tutorial list does not contain tutorial id: " + id);
			}
		}
	}

	public void SwitchTutorialList (int id)
	{
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
		StopTextAnimation ();
		if (CurrentTutorial.StopTutorial () || force) {
			if (force) {
				CurrentTutorial.ForceStopTutorial ();
			}
			LaunchTutorial (CurrentTutorial.TutorialID + 1);
		} 


	}

	public void ForceStop ()
	{
		if (isActive) {
			this.transform.DOKill ();
			CurrentTutorial.ForceStopTutorial ();
			isActive = false;
		}

	}

	public void StopTutorial ()
	{
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
	}

	public void Selected ()
	{
		CurrentTutorial.Selected.Invoke ();
	}

	public void OnTrain ()
	{
		CurrentTutorial.OnTrain.Invoke ();
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


	private string TextOnUI;
	private Text UIText;
	public bool TextFinished;

	public void StartTutorial (int id)
	{
		TextFinished = false;
		TutorialID = id;
		UIText = TutorialUI.GetComponentInChildren<Text> ();
		TextOnUI = UIText.text;
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