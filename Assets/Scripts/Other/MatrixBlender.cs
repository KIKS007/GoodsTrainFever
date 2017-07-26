using UnityEngine;
using System.Collections;
using DG.Tweening;

[RequireComponent (typeof(Camera))]
public class MatrixBlender : MonoBehaviour
{
	private Camera _camera;

	void Awake ()
	{
		_camera = GetComponent<Camera> ();
	}

	public Coroutine BlendToMatrix(Matrix4x4 targetMatrix, float duration)
	{
		StopAllCoroutines();

		return StartCoroutine(LerpFromTo(_camera.projectionMatrix, targetMatrix, duration));
	}

	private IEnumerator LerpFromTo(Matrix4x4 src, Matrix4x4 dest, float duration)
	{
		float startTime = Time.time;

		while (Time.time - startTime < duration)
		{
			_camera.projectionMatrix = MatrixLerp(src, dest, (Time.time - startTime) / duration);
			yield return 1;
		}
		_camera.projectionMatrix = dest;
	}

	public static Matrix4x4 MatrixLerp(Matrix4x4 from, Matrix4x4 to, float duration)
	{
		Matrix4x4 ret = new Matrix4x4();

		for (int i = 0; i < 16; i++)
			ret[i] = Mathf.Lerp(from[i], to[i], duration);

		return ret;
	}
}