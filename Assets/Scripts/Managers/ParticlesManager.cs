using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ParticlesManager : Singleton<ParticlesManager> 
{
	public List<ParticlesType> particlesType = new List<ParticlesType> ();

	public void CreateParticles (FeedbackType feedback, Vector3 position, Quaternion rotation, float delay = 0)
	{
		GameObject particles = particlesType [0].particles;

		foreach(var p in particlesType)
			if(p.feedbackType == feedback)
			{
				particles = p.particles;
				break;
			}

		if(delay == 0)
			Instantiate (particles, position, rotation, transform);
		else
			DOVirtual.DelayedCall (delay, ()=> Instantiate (particles, position, rotation, transform));
	}

	public void CreateParticles (FeedbackType feedback, Vector3 position, float delay = 0)
	{
		GameObject particles = particlesType [0].particles;

		foreach(var p in particlesType)
			if(p.feedbackType == feedback)
			{
				particles = p.particles;
				break;
			}

		if(delay == 0)
			Instantiate (particles, position, particles.transform.rotation, transform);
		else
			DOVirtual.DelayedCall (delay, ()=> Instantiate (particles, position, particles.transform.rotation, transform));
	}
}

[System.Serializable]
public class ParticlesType 
{
	public FeedbackType feedbackType = FeedbackType.Default;
	public GameObject particles;
}
