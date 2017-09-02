﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;

public class Tutorial_UI_Setup : MonoBehaviour
{
	public Sprite Character;
	public Sprite Bubble;
	public Transform BubblePlace;
	public Transform BubbleTextPlace;
	public Font BubbleFont;
	public bool CharacterSetup;
	public bool BubbleSetup;

	[Button]
	public void ApplySettings ()
	{
		GameObject[] allObjects = UnityEngine.Resources.FindObjectsOfTypeAll<GameObject> ();

		foreach (GameObject go in allObjects) {
			if (go.name.StartsWith ("Tuto ")) {
				SettingUP (go);
			}
		}
	}

	private void SettingUP (GameObject go)
	{
		if (!BubbleSetup && !CharacterSetup) {
			Debug.Log ("Nothing to setup, please activate at least one Setup Setting");
		} else {
			Debug.Log ("Setting " + go.name);
		}
		if (CharacterSetup) {
			var tmpGO = go.transform.Find ("Character");
			tmpGO.GetComponent<Image> ().color = Color.white;
			tmpGO.GetComponent<Image> ().sprite = Character;
			foreach (Transform child in tmpGO.transform) {
				DestroyImmediate (child.gameObject);
			}
		}

		#if UNITY_EDITOR

		if (BubbleSetup) {
			var tmpGO = go.transform.Find ("Panel");
			tmpGO.GetComponent<Image> ().color = Color.white;
			tmpGO.GetComponent<Image> ().sprite = Bubble;
			tmpGO.GetComponent<Image> ().type = Image.Type.Simple;
			UnityEditorInternal.ComponentUtility.CopyComponent (BubblePlace);
			UnityEditorInternal.ComponentUtility.PasteComponentValues (tmpGO);
			var tmp2 = tmpGO.transform.Find ("Text");
			UnityEditorInternal.ComponentUtility.CopyComponent (BubbleTextPlace);
			UnityEditorInternal.ComponentUtility.PasteComponentValues (tmp2);
			tmp2.GetComponent<Text> ().alignment = TextAnchor.UpperLeft;
			tmp2.GetComponent<Text> ().font = BubbleFont;
		}

		#endif
	}
}
