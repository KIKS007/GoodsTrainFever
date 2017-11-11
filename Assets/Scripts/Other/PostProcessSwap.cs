using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;

public class PostProcessSwap : MonoBehaviour
{
	[SerializeField]
	private PostProcessingBehaviour PPB;
	[SerializeField]
	private AmplifyColorEffect ACE;
	private bool DoOnce;

	// Use this for initialization
	void Start ()
	{
		if (!DoOnce) {
			DoOnce = true;
			if (SystemInfo.SupportsTextureFormat (TextureFormat.RGBAHalf)) {
				PPB.enabled = true;
				ACE.enabled = false;
			} else {
				PPB.enabled = false;
				ACE.enabled = true;
			}
		}

	}
}
