using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlesAutoDestroy : MonoBehaviour 
{
	private ParticleSystem _particles;

	// Use this for initialization
	void Start () 
	{
		_particles = GetComponent<ParticleSystem> ();

		StartCoroutine (Wait ());
	}
	
	IEnumerator Wait ()
	{
		yield return new WaitWhile (() => _particles.IsAlive ());

		Destroy (gameObject);
	}
}
