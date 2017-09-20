using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class LevelSettings_Sync : MonoBehaviour
{


	public Transform lvlparent;
	[HideInInspector]
	public bool Wait = false;
	#if UNITY_EDITOR
	[Button]
	[ExecuteInEditMode]
	void SyncLDValues ()
	{
		if (SpreadsheetImporter.Singleton != null) {
			StartCoroutine (Fetching ());
		} else {
			this.gameObject.GetComponent<SpreadsheetImporter> ().MakeInstance ();
			StartCoroutine (Fetching ());
		}

	}

	[ExecuteInEditMode]
	IEnumerator Fetching ()
	{
		SpreadsheetImporter.Singleton.isFetched = false;
		SpreadsheetImporter.Singleton.sync = true;
		SpreadsheetImporter.Singleton.FetchLD (this);
		yield return new WaitUntil (() => Wait);
		Wait = false;

		foreach (Transform t in lvlparent) {
			if (t.gameObject.GetComponent<LevelSettings_LD> ()) {
				LevelSettings_LD lsld = t.gameObject.GetComponent<LevelSettings_LD> ();
				string row = System.Text.RegularExpressions.Regex.Match (lsld.name, @"\d+").Value + "";
				SpreadsheetImporter.Singleton.ChangeLanguage (row);
				
				
				int.TryParse (SpreadsheetImporter.Singleton.GetText ("Duration"), out lsld.levelDuration);
				int.TryParse (SpreadsheetImporter.Singleton.GetText ("Errors"), out lsld.errorsAllowed);
				

				string t1 = SpreadsheetImporter.Singleton.GetText ("Orders Count");
				string t2 = SpreadsheetImporter.Singleton.GetText ("Trains Count");
				t1 = t1.Replace ("[", "");
				t1 = t1.Replace ("]", "");
				string[] t1tmp = t1.Split (',');
				int.TryParse (t1tmp [0], out lsld.ordersCountMin);
				int.TryParse (t1tmp [1], out lsld.ordersCountMax);

				t2 = t2.Replace ("[", "");
				t2 = t2.Replace ("]", "");
				string[] t2tmp = t2.Split (',');
				int.TryParse (t2tmp [0], out lsld.trainsCountMin);
				int.TryParse (t2tmp [1], out lsld.trainsCountMax);

				

				
			}
		}
		SpreadsheetImporter.Singleton.isFetched = false;
		SpreadsheetImporter.Singleton.sync = false;
	}
	#endif
}
