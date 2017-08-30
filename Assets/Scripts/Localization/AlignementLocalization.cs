using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

public class AlignementLocalization : MonoBehaviour
{
	public bool Inverse = false;

	// Use this for initialization

	void Start ()
	{
		Refresh ();
		LocalizationManager.Singleton.OnLangChange += new MyDel (() => {
			Refresh (.5f);
		});
	}

	void Refresh (float duration = 0)
	{
		float value = float.Parse (LocalizationManager.Singleton.GetText ("_SIDE"));
		if (Inverse)
			value *= -1;

		if (value > 0)
			GetComponent<Text> ().alignment = TextAnchor.MiddleLeft;
		else if (value < 0)
			GetComponent<Text> ().alignment = TextAnchor.MiddleRight;
		
	}
}
