using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;
using DG.Tweening;

public class MenuTrophies : MenuComponent 
{
	public Text titleText;
	public Text factText;
	public Stage_Menu stageMenu;

	[Header ("Trophy Animations")]
	public Ease trophyEase;
	public Vector3 trophyRotation;
	public float trophyRotationDelay = 0.1f;

	[Header ("Trophy Movement")]
	public float deceleration = 0.9f;
	public float mouseMovementFactor = 1;
	public float touchMovementFactor = 1;

	[Header ("Trophy Zoom")]
	public float relativeMaxScale;
	public float relativeMinScale;

	private GameObject _trophy;
	private Vector3 _deltaPosition;
	private Vector3 _mousePosition;

	private Vector3 _movement;
	private Transform _camera;
	private float _deltaMagnitudeDiff;
	private float _initialXScale;

	void Start ()
	{
		_camera = GameObject.FindGameObjectWithTag ("MainCamera").transform;
	}

	void Update ()
	{
		if (MenuManager.Instance.currentMenu != this)
			return;

		//_movement += new Vector3 (0, 0, -_deltaPosition.x);
		_movement += new Vector3 (_deltaPosition.y, 0, -_deltaPosition.x);

		MoveTrophy ();

		#if UNITY_EDITOR
		if(Application.isEditor && !UnityEditor.EditorApplication.isRemoteConnected)
			MouseHold ();
		else
			TouchHold ();

		#else
		TouchHold ();
		ZoomTrophy ();
		#endif
	}

	void TouchHold ()
	{
		if (Input.touchCount > 0)
		{
			if (Input.touchCount == 1)
			{
				Touch touch = Input.GetTouch (0);
				
				if(touch.phase == TouchPhase.Moved)
					_deltaPosition = touch.deltaPosition;
				
				if(touch.phase == TouchPhase.Ended)
					_deltaPosition = Vector3.zero; 
			}


			//Pinch Zoom
			if (Input.touchCount > 1)
			{
				// Store both touches.
				Touch touchZero = Input.GetTouch(0);
				Touch touchOne = Input.GetTouch(1);
				
				if(touchZero.phase == TouchPhase.Moved || touchOne.phase == TouchPhase.Moved)
				{
					// Find the position in the previous frame of each touch.
					Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
					Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;
					
					// Find the magnitude of the vector (the distance) between the touches in each frame.
					float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
					float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;
					
					// Find the difference in the distances between each frame.
					_deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;
				}
				
				if (touchZero.phase == TouchPhase.Ended || touchOne.phase == TouchPhase.Ended)
					_deltaMagnitudeDiff = 0;
				
			}
		}
	}

	void MouseHold ()
	{
		if(Input.GetMouseButton (0))
			_deltaPosition = Input.mousePosition - _mousePosition; 

		if(Input.GetMouseButtonUp (0))
			_deltaPosition = Vector3.zero; 
		
		_mousePosition = Input.mousePosition;
	}

	void MoveTrophy ()
	{
		if (_trophy == null)
			return;

		float factor = Application.isEditor ? mouseMovementFactor : touchMovementFactor;

		_movement *= deceleration;

		_trophy.transform.RotateAround (_trophy.transform.position, _camera.transform.up, _movement.z * factor * Time.deltaTime);
		_trophy.transform.RotateAround (_trophy.transform.position, _camera.transform.right, _movement.x * factor * Time.deltaTime);
	}

	void ZoomTrophy ()
	{
		if (_trophy == null)
			return;

		Vector3 newScale = _trophy.transform.localScale;
		newScale.x -= _deltaMagnitudeDiff * 0.0001f;
		newScale.y -= _deltaMagnitudeDiff * 0.0001f;
		newScale.z -= _deltaMagnitudeDiff * 0.0001f;

		if (newScale.x < _initialXScale * relativeMinScale)
			newScale = Vector3.one * _initialXScale * relativeMinScale;

		else if(newScale.x > _initialXScale * relativeMaxScale)
			newScale = Vector3.one * _initialXScale * relativeMaxScale;

		_trophy.transform.localScale = newScale;
	}

	public override void OnShow ()
	{
		base.OnShow ();

		if (_trophy)
			Destroy (_trophy);

		Vector3 scale = new Vector3 ();

		foreach(var s in ScoreManager.Instance.levelStages)
		{
			if(s.stage == stageMenu)
			{
				_trophy = Instantiate (s.trophy, s.trophy.transform.position, s.trophy.transform.rotation, GlobalVariables.Instance.gameplayParent) as GameObject;
				Trophy_Menu trophyMenu = _trophy.GetComponent<Trophy_Menu> ();

				_trophy.transform.localPosition = s.trophy.transform.localPosition;
				_trophy.transform.localRotation = s.trophy.transform.localRotation;

				_initialXScale = _trophy.transform.localScale.x;

				scale = _trophy.transform.localScale;
				_trophy.transform.localScale = Vector3.zero;

				titleText.text = trophyMenu.meshTitle;
				factText.text = trophyMenu.funFact;
				break;
			}
		}

		if(_trophy != null)
			StartCoroutine (ShowTrophy (scale));
	}

	public override void OnHide ()
	{
		base.OnHide ();

		if(_trophy != null)
			_trophy.transform.DOScale (0, MenuManager.Instance.menuAnimationDuration * 0.5f).SetEase (trophyEase).OnComplete (()=> Destroy (_trophy));
	}

	IEnumerator ShowTrophy (Vector3 scale)
	{
		Vector3 rotation = _trophy.transform.localEulerAngles;
		rotation -= trophyRotation;
		_trophy.transform.DOLocalRotate (rotation, 0);

		yield return new WaitForSeconds (MenuManager.Instance.menuAnimationDuration);

		rotation += trophyRotation;
		_trophy.transform.DOLocalRotate (rotation, MenuManager.Instance.menuAnimationDuration).SetEase (trophyEase).SetDelay (trophyRotationDelay);
		_trophy.transform.DOScale (scale, MenuManager.Instance.menuAnimationDuration).SetEase (trophyEase);
	}
}
