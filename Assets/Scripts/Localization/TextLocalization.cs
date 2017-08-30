using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

[ExecuteInEditMode]
public class TextLocalization : MonoBehaviour
{
	public bool UpperCase = true;

	public string key;
	// Use this for initialization

	[ExecuteInEditMode]
	void Start ()
	{
		Refresh ();
		LocalizationManager.Singleton.OnLangChange += new MyDel (() => {
			Refresh (.5f);
		});
	}

	[ExecuteInEditMode]
	void Refresh (float duration = 0)
	{
		string value = LocalizationManager.Singleton.GetText (key);
		if (UpperCase)
			value = value.ToUpper ();

		if (Application.isPlaying) {
			GetComponent<Text> ().text = "";
			GetComponent<Text> ().DOText (value, duration).SetEase (Ease.OutExpo);
		} else
			GetComponent<Text> ().text = value;
	}
}
