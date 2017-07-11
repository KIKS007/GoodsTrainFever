using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Feno
{
public class CameraMotion : MonoBehaviour
{
	Vector3 startPos;
	Vector3 startRot;
	float startOrth;
	Camera cam;

	public	AnimationCurve Unzoom;
	public GameObject[] ObjectsToIntroduceVertically;
	public GameObject[] ObjectsToIntroduceHorizontally;

	public Action OnIntroduction;

	private List<Vector3> ObjectsTo = new List<Vector3> ();
	// Use this for initialization
	void Start ()
	{
		cam = GetComponent<Camera> ();
		startOrth = cam.orthographicSize;
		startPos = transform.parent.position;
		startRot = transform.parent.eulerAngles;

		foreach (GameObject obj in ObjectsToIntroduceVertically) {
//			ObjectsTo.Add (obj.transform.position);
		}
		foreach (GameObject obj in ObjectsToIntroduceHorizontally) {
			ObjectsTo.Add (obj.transform.position);
		}




		DOVirtual.DelayedCall (1f, () => Introduction ());
		//	DOVirtual.DelayedCall (12f, () => Outro ()); 
	
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}

	void Outro ()
	{
		Transform point = GameObject.Find ("OUTRO_POINT").transform;

		transform.parent.DORotate (point.transform.eulerAngles, 3f).SetEase (Ease.InOutExpo);
		transform.parent.DOMove (point.transform.position, 3f).SetEase (Ease.InOutExpo);
		cam.DOOrthoSize (point.localScale.x, 3f).SetEase (Ease.InOutExpo);

		Debug.Log ("<size=30><color=green>In Outro</color></size>");
	}

	public void Introduction ()
	{
		if (OnIntroduction != null) {
			OnIntroduction ();
		}

		/*for (int i = 0; i <= ObjectsToIntroduceHorizontally.Length - 1; i++) {
			GameObject obj = ObjectsToIntroduceHorizontally [i];
//			obj.transform.position -= new Vector3 (0, 70, 0);
//			obj.transform.DOMoveY (ObjectsTo [ObjectsToIntroduceVertically.Length + i].y, 1f).SetEase (Ease.OutBack).SetDelay (3f + i * .5f);
		}

		for (int i = 0; i <= ObjectsToIntroduceVertically.Length - 1; i++) {
			GameObject obj = ObjectsToIntroduceVertically [i];
//			obj.transform.position -= new Vector3 (0, -30, 0);
//			obj.transform.DOMoveY (ObjectsTo [i].y, 1f).SetEase (Ease.OutBounce).SetDelay (3f + i * .05f);
		}*/


		Transform point = GameObject.Find ("INTRO_POINT").transform;
		transform.parent.position = point.transform.position;
		transform.parent.eulerAngles = point.transform.eulerAngles;
		cam.orthographicSize = point.localScale.x;

		transform.parent.DORotate (startRot, 2f).SetEase (Ease.InOutExpo).SetDelay (2.5f);
		transform.parent.DOMove (startPos, 2f).SetEase (Ease.InOutExpo).SetDelay (2.5f);
		cam.DOOrthoSize (startOrth, 2f).SetEase (Ease.InOutExpo).SetDelay (2.5f);
//		cam.DOOrthoSize (startOrth, 15f).SetEase (Unzoom).SetDelay (2f);


	}
}
}