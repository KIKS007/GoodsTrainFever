using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trophy_Menu : MonoBehaviour
{
	public string meshTitle;
	public string key;
	[Multiline]
	public string funFact;

	void Awake ()
	{
		funFact = LocalizationManager.Singleton.GetText (key);
	}
}
