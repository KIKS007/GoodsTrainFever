using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Spreadsheet;
using Sirenix.OdinInspector;



[ExecuteInEditMode]
public class SpreadsheetImporter : MonoBehaviour {
    [HideInInspector]
    public string SheetID;

    public static SpreadsheetImporter Singleton;
    [Header ("Status")]
    public bool sync;
      [HideInInspector]
    public bool refresh;
      [HideInInspector]
    public bool fetch;
    [HideInInspector]
    public bool InEditorupdate;
    [HideInInspector]
    public LevelSettings_Sync lsfe;
    [HideInInspector]
    public bool isFetched = false;
    [HideInInspector]
    public string CurrentLang = "English";
  [HideInInspector]
    public List<string> LanguagesName = new List<string> ();
    public Dictionary<string, Language> Languages = new Dictionary<string, Language> ();

    public Dictionary<string, string> ColumnToLangName = new Dictionary<string, string> ();
    public Dictionary<string, string> RowToTextID = new Dictionary<string, string> ();
    // Use this for initialization

    [ExecuteInEditMode]
    void Awake () {
        if (SpreadsheetImporter.Singleton == null) {
            SpreadsheetImporter.Singleton = this;
        } else {
            Destroy (gameObject);
        }
    }


    [ExecuteInEditMode]
   public void MakeInstance () {
        if (SpreadsheetImporter.Singleton == null) {
            SpreadsheetImporter.Singleton = this;
        }else{
            Debug.Log("Instance Already Created");
        }
    }


    [ExecuteInEditMode]
    void DestroyInstance () {
        if (SpreadsheetImporter.Singleton != null) {
            SpreadsheetImporter.Singleton = null;
        }
    }

    [ExecuteInEditMode]
    public string GetText (string key) {

        if (LanguagesName.IndexOf (CurrentLang) == -1)
            return "BAD LANG";

        if (!Languages[CurrentLang].Texts.ContainsKey (key))
            return "BAD KEY";

        return Languages[CurrentLang].Texts[key];
    }

    [ExecuteInEditMode]
    void FetchLanguages () {
        UnityEngine.Object[] langs = Resources.LoadAll ("lang");
        if (langs != null) {
            for (int i = 0; i <= langs.Length - 1; i++) {
                if ((TextAsset) langs[i]) {
                    TextAsset ta = (TextAsset) langs[i];
                    LanguagesName.Add (ta.name);
                }
            }
        }
    }

    [ExecuteInEditMode]
    public event MyDel OnLangChange;

    [ExecuteInEditMode]
    public void ChangeLanguage (string lang) {
        if (LanguagesName.IndexOf (lang) == -1)
            return;

        CurrentLang = lang;
        Load (CurrentLang);

        if (OnLangChange != null)
            OnLangChange ();

    }

    [ExecuteInEditMode]
    void Load (string LangName) {
        TextAsset ta = Resources.Load<TextAsset> ("lang/" + LangName);
        if (ta != null) {
            Language lang = JsonUtility.FromJson<Language> (ta.text);
            for (int j = 0; j <= lang.textsDictionary.Count - 1; j++) {
                lang.Texts.Add (lang.textsDictionary[j].key, lang.textsDictionary[j].value);
            }

            if (Languages.ContainsKey (lang.name)) {
                Languages.Remove (lang.name);
            }

            if (LanguagesName.IndexOf (lang.name) == -1) {
                LanguagesName.Add (lang.name);
            }

            Languages.Add (lang.name, lang);
        }
    }

    [ExecuteInEditMode]
    public IEnumerator Fetch () {
#if UNITY_EDITOR
        if (sync) {
            isFetched =false;
           // Debug.Log("Fetching LD Datas");
            WWW www = new WWW ("https://spreadsheets.google.com/feeds/cells/" + SheetID + "/1/public/values?alt=json");
            yield return www;
            /*
             * Ensure url exsists (fetch all url)
             */
            if (www.responseHeaders["STATUS"] != "HTTP/1.1 400 Bad Request") {
                Languages.Clear ();
                RowToTextID.Clear ();
                ColumnToLangName.Clear ();
                LanguagesName.Clear ();

                Sheet t = JsonUtility.FromJson<Sheet> (www.text.Trim ().Replace ("$", "__"));

                // FENO'S MAGIC
                foreach (Entry e in t.feed.entry) {
                    string column = e.title.__t;
                    string row = System.Text.RegularExpressions.Regex.Match(column, @"\d+").Value+"";
                    column = column.Replace(row, "")+"";

                    // Debug.Log(column);
                    // Debug.Log(e.title.__t[1] +"|||"+ e.title.__t.Length);

                    // Debug.Log(e.title.__t[0]);
                    if (row == "1") {
                        Language l = new Language ();
                        l.name = (string) e.content.__t;
                        l.id = e.title.__t[0];
                        ColumnToLangName.Add (column, (string) e.content.__t);
                        Languages.Add ((string) e.content.__t, l);
                        LanguagesName.Add ((string) e.content.__t);
                    } else if (column == "A" ) {
                        RowToTextID.Add (row, (string) e.content.__t);

                        foreach (KeyValuePair<string, Language> entry in Languages) {
                            Languages[entry.Key].Texts.Add ((string) e.content.__t, "");
                        }
                    } else {
                        // Debug.Log(column);
                        // Debug.Log(ColumnToLangName[column]);

                        Languages[ColumnToLangName[column]].Texts[RowToTextID[row]] = e.content.__t;
                        TextsDictionary td = new TextsDictionary ();
                        td.key = RowToTextID[row];
                        td.value = e.content.__t;
                        Languages[ColumnToLangName[column]].textsDictionary.Add (td);
                    }
                }

                // SAVING FILE
                foreach (KeyValuePair<string, Language> entry in Languages) {
                    System.IO.File.WriteAllText ("Assets/Resources/lang/" + entry.Value.name + ".json", JsonUtility.ToJson (Languages[entry.Key]));
                }
                //Debug.Log ("Spreadsheet synced");
                AssetDatabase.Refresh ();
                isFetched = true;
                lsfe.Wait = true;
                InEditorupdate = false;
                ChangeLanguage (CurrentLang);
            }
        }
#endif
        yield return null;
    }

    [ExecuteInEditMode]
    void Start () {
        Languages.Clear ();
        RowToTextID.Clear ();
        ColumnToLangName.Clear ();
        LanguagesName.Clear ();

        FetchLanguages ();
        Load (CurrentLang);
        /*
		 * LOAD ALL
		UnityEngine.Object[] langs = Resources.LoadAll ("lang"); 
		if (langs != null) {
			for (int i = 0; i <= langs.Length - 1; i++) {
				if ((TextAsset) langs [i]) {
					TextAsset ta = (TextAsset) (langs [i]);
					Language lang = JsonUtility.FromJson<Language> (ta.text);
					for (int j = 0; j <= lang.textsDictionary.Count - 1; j++) {
						lang.Texts.Add (lang.textsDictionary [j].key, lang.textsDictionary [j].value);
					}
					Languages.Add (lang.name, lang);
					LanguagesName.Add (lang.name);
				}
			}
		}
*/

        StartCoroutine (Fetch ());

        //		yield return null;
    }

#if UNITY_EDITOR
    // Update is called once per frame
    [ExecuteInEditMode]
    void Update () {

        if (refresh) {
            ChangeLanguage (CurrentLang);
            refresh = false;
        }

        if (fetch) {
            StartCoroutine (Fetch ());
            fetch = false;
        }
    }


[ExecuteInEditMode]
public void FetchLD(LevelSettings_Sync lsfis){
    lsfe = lsfis;   
    InEditorupdate = true;
    refresh = true;
    fetch = true;
    StartCoroutine (Fetch ());
}

public void RefreshLD(){
ChangeLanguage (CurrentLang);
}
#endif

}

//{"appid":730,"name":"Counter-Strike: Global Offensive","developer":"Valve","publisher":"Valve","score_rank":84,"owners":22796584,"owners_variance":118099,"players_forever":22079017,"players_forever_variance":116367,"players_2weeks":8212915,"players_2weeks_variance":72623,"average_forever":15526,"average_2weeks":882,"median_forever":4300,"median_2weeks":398,"ccu":593940}