using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FenalyticsScripts;
using System.Runtime.CompilerServices;
using System;

public class Fenalytics : MonoBehaviour
{
	public static void To (string name)
	{
		//	Core._instance.StartCoroutine (Core.Session.GetId (name));
	}

	public static void Ev (string name, object data)
	{
		/*Ev (name, new Dictionary<string, object> () {
			{ name, data }
		});*/
	}

	public static void Ev (string name, Dictionary<string, object> data)
	{
		//Core._instance.StartCoroutine (Core.Event.Send (name, data));
	}
}

