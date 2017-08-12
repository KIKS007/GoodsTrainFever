using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using System;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Reflection;

public class GlobalVariables : Singleton<GlobalVariables>
{
	[Header ("FPS")]
	public Text fpsText;

	[Header ("Parents")]
	public Transform gameplayParent;
	public Transform extraContainersParent;

	[Header ("Container Color")]
	public Color redColor;
	public Color blueColor;
	public Color yellowColor;
	public Color violetColor;

	[Header ("Weight")]
	public Color wagonNormalWeightColor;
	public Color wagonOverweightColor;

	[Header ("Info Button Colors")]
	public Color infoButtonRespectedColor;
	public Color infoButtonNotRespectedColor;

	[Header ("Stars Colors")]
	public Color normalStarColor;
	public Color errorLockedStarColor;

	[Header ("Spawn Spots Prefabs")]
	public GameObject spot40SpawnedPrefab;

	[Header ("Scriptable Object")]
	public UnityEngine.Object objectToCreate;

	// Use this for initialization
	void Start ()
	{
	}
	
	// Update is called once per frame
	void Update ()
	{
		FPS ();
	}

	void FPS ()
	{
		fpsText.text = (Mathf.RoundToInt (1.0f / Time.smoothDeltaTime)).ToString ("##.00");
	}

	#if UNITY_EDITOR
	public void ClearLog ()
	{ 
		var assembly = Assembly.GetAssembly (typeof(UnityEditor.ActiveEditorTracker)); 
		var type = assembly.GetType ("UnityEditorInternal.LogEntries"); 
		var method = type.GetMethod ("Clear"); 
		method.Invoke (new object (), null); 
	}
	#endif

	#if UNITY_EDITOR
	[Button]
	public void CreateAsset ()
	{
		var asset = ScriptableObject.CreateInstance (objectToCreate.name);

		string path = AssetDatabase.GetAssetPath (Selection.activeObject);
		if (path == "") {
			path = "Assets";
		} else if (Path.GetExtension (path) != "") {
			path = path.Replace (Path.GetFileName (AssetDatabase.GetAssetPath (objectToCreate)), "");
		}

		string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath (path + "/New " + objectToCreate.name + ".asset");

		AssetDatabase.CreateAsset (asset, assetPathAndName);

		AssetDatabase.SaveAssets ();
		AssetDatabase.Refresh ();
		EditorUtility.FocusProjectWindow ();
		Selection.activeObject = asset;
	}
	#endif
}
