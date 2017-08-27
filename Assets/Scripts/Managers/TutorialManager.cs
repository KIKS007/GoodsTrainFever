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
	[Header ("Level 1")]
	public Tutorial[] TutorialList;
	[Header ("Level 2")]
	public Tutorial[] TutorialList2;
	[Header ("General Settings")]
	public Tutorial CurrentTutorial;
	public Canvas MainTutorialCanvas;
	public bool isActive;
	public float TextSpeed;

	public void LaunchTutorial (int id)
	{
		if (isActive) {
			ForceStop ();
		} 
		isActive = true;
		CurrentTutorial = TutorialList [id];
		CurrentTutorial.StartTutorial (id);
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
		this.transform.DOKill ();
		CurrentTutorial.ForceStopTutorial ();
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