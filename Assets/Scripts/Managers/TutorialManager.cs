using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.Serialization;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine.Events;
using UnityEngine.UI;

public class TutorialManager : Singleton<TutorialManager>
{
	public Tutorial[] TutorialList;
	public Tutorial CurrentTutorial;
	public Canvas MainTutorialCanvas;

	public void ActivateTutorial ()
	{
		
	}

	public void LaunchTutorial (int id)
	{
		CurrentTutorial = TutorialList [id];
		CurrentTutorial.StartTutorial (id);
	}

	private void StopCurrentTutorial ()
	{
		CurrentTutorial.StopTutorial ();
	}
		
}

[System.Serializable]
public class Tutorial
{
	public UnityEvent OnStartTutorial;
	public UnityEvent OnStopTutorial;
	public UnityEvent OnNextTutorial;
	public int canvaID;

	public void StartTutorial (int id)
	{
		canvaID = id;
		OnStartTutorial.Invoke ();
	}

	public void StopTutorial ()
	{
		OnStopTutorial.Invoke ();
	}
}