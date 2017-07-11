using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Feno
{
public class ParticlesManager : MonoBehaviour
{

	public GameObject FXDropFog;

	public static ParticlesManager Singleton;

	void Awake ()
	{
		if (ParticlesManager.Singleton == null) {
			ParticlesManager.Singleton = this;
		} else {
			Destroy (gameObject);
		}
	}

	public GameObject Create (GameObject prefab, Vector3 position)
	{
		GameObject i = Instantiate (prefab, position, Quaternion.identity) as GameObject;
		return i;
	}
}
}
