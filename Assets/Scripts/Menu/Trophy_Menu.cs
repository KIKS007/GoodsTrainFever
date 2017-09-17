using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trophy_Menu : MonoBehaviour
{
	[Multiline]
	public string meshTitle;
	public string key;



	public string funFact (int id)
	{
		if (id < 10) {
			return LocalizationManager.Singleton.GetText ("FUNFACT-0" + id.ToString ());
		} else {
			return LocalizationManager.Singleton.GetText ("FUNFACT-" + id.ToString ());
		}
	}
}
