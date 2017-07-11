using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ObjectToIntroduce : MonoBehaviour
{

	public enum IntroductionType
	{
		Fall,
		Pop
	}

	public IntroductionType type;
	public float delay;
	public bool playAudio = true;

	private float mainDelay = 3.2f;
	// Use this for initialization
	void Awake ()
	{
		Camera.main.GetComponent<CameraMotion> ().OnIntroduction += On;
	}


	void On ()
	{
		if (type == IntroductionType.Fall) {
			Vector3 scale = transform.localScale;
			float y = transform.position.y;
			transform.localScale = Vector3.zero;
			transform.position += Vector3.up * 10f;
			transform.DOScale (scale, 1f).SetEase (Ease.OutElastic).SetDelay (mainDelay + delay);
			transform.DOMoveY (y, Random.Range (1f, 1.2f)).SetEase (Ease.OutBounce).SetDelay (mainDelay + delay).OnStart (() => {
				if (playAudio) {
//					AudioManager.Singleton.Play ("Pop");
				}
			});


		} else if (type == IntroductionType.Pop) {
			Vector3 scale = transform.localScale;
			transform.localScale = Vector3.zero;
			transform.DOScale (scale, 1f).SetEase (Ease.OutElastic).SetDelay (mainDelay + delay);
			transform.DORotate (Vector3.up * 360f, 1f, RotateMode.FastBeyond360).SetEase (Ease.InOutCirc).SetDelay (mainDelay + delay).OnStart (() => {
			});
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}
}
