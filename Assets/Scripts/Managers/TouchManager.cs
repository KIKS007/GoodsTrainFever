using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class TouchManager : Singleton<TouchManager>
{
    public Action OnTouchDown;
    public Action<Vector3> OnTouchHold;
    public Action OnTouchUp;
    public Action OnTouchUpNoTarget;
    public Action OnTouchUpNoContainerTarget;

    public bool isTouchingTouchable = false;
    public bool isTouchingUI = false;
    public LayerMask touchableLayer;
    public float boxCastHalfExtent = 0.2f;

    private bool _touchDown = false;
    private Vector3 _deltaPosition;
    private Vector3 _mousePosition;
    private Camera _camera;

    void Start()
    {
        _camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

        MenuManager.Instance.OnLevelStart += () =>
        {
            isTouchingTouchable = false;
            isTouchingUI = false;
        };
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.gameState != GameState.Playing)
            return;

        #if UNITY_EDITOR
        if (Application.isEditor && !UnityEditor.EditorApplication.isRemoteConnected)
            MouseHold();
        else
            TouchHold();
        #else
		TouchHold ();
        #endif
    }

    void TouchHold()
    {
        if (Input.touchCount == 0)
            return;

        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);
				
            Touchable touchable = null;
				
            _deltaPosition = touch.deltaPosition;
				
            switch (touch.phase)
            {
                case TouchPhase.Began:
					
                    _touchDown = true;
					
                    touchable = RaycastTouchable(touch.position);
                    if (touchable != null)
                        touchable.OnTouchDown();
					
					//Debug.Log ("Touchable: " + touchable);
                    if (OnTouchDown != null)
                        OnTouchDown();
					
                    StartCoroutine(TouchHoldCoroutine());
					
                    break;
						
                case TouchPhase.Ended:
					
                    _touchDown = false;
					
                    _deltaPosition = new Vector3();
					
                    if (!isTouchingUI)
                    {
                        touchable = RaycastTouchable(touch.position);

                        if (touchable != null)
                            touchable.OnTouchUpAsButton();
						
                        if (touchable && touchable.GetType() != typeof(Container) && touchable.GetType() != typeof(Spot) && OnTouchUpNoContainerTarget != null)
                            OnTouchUpNoContainerTarget();
                    }

					//Debug.Log ("END - isTouchingUI: " + isTouchingUI + " touchable: " + touchable);

                    if (OnTouchUpNoTarget != null && !isTouchingTouchable && !isTouchingUI)
                    {
                        OnTouchUpNoTarget();
                    }
					
                    if (OnTouchUp != null)
                        OnTouchUp();
					
                    isTouchingTouchable = false;
                    isTouchingUI = false;
					
                    break;
            }
        }
    }

    void MouseHold()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _touchDown = true;

            _mousePosition = Input.mousePosition;

            Touchable touchable = RaycastTouchable(_mousePosition);
            if (touchable != null)
                touchable.OnTouchDown();

            if (OnTouchDown != null)
                OnTouchDown();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            _touchDown = false;

            _deltaPosition = new Vector3();

            if (!isTouchingUI)
            {
                Touchable touchable = RaycastTouchable(_mousePosition);
                if (touchable != null)
                    touchable.OnTouchUpAsButton();

                //Debug.Log ("END touchable: " + touchable, touchable);

                if (touchable && touchable.GetType() != typeof(Container) && touchable.GetType() != typeof(Spot) && OnTouchUpNoContainerTarget != null)
                    OnTouchUpNoContainerTarget();
            }


            if (OnTouchUpNoTarget != null && !isTouchingTouchable && !isTouchingUI)
                OnTouchUpNoTarget();

            if (OnTouchUp != null)
                OnTouchUp();
			
            isTouchingTouchable = false;
            isTouchingUI = false;
        }
        else if (Input.GetMouseButton(0))
        {
            _deltaPosition = Input.mousePosition - _mousePosition; 
			
            if (OnTouchHold != null)
                OnTouchHold(_deltaPosition);
			
            _mousePosition = Input.mousePosition;
        }

    }

    Touchable RaycastTouchable(Vector3 position, LayerMask mask)
    {
        RaycastHit hit;
        Ray ray = _camera.ScreenPointToRay(position);

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, mask))
        {
            Touchable touchable = hit.collider.GetComponent<Touchable>();

            if (touchable == null && hit.rigidbody)
                touchable = hit.rigidbody.gameObject.GetComponent<Touchable>();
			
            if (touchable != null)
                return touchable;
            else
                return null;
        }
        else
            return null;
    }

    /*Touchable RaycastTouchable (Vector3 position)
	{
		RaycastHit hit;
		Ray ray = _camera.ScreenPointToRay (position);

		if (Physics.Raycast (ray, out hit, Mathf.Infinity)) {
			Touchable touchable = hit.collider.GetComponent<Touchable> ();

			if (touchable == null && hit.rigidbody)
				touchable = hit.rigidbody.gameObject.GetComponent<Touchable> ();

			if (touchable != null)
				return touchable;
			else
				return null;
		} else
			return null;
	}*/

    Touchable RaycastTouchable(Vector3 position)
    {
        Ray ray = _camera.ScreenPointToRay(position);

        var colliders = Physics.BoxCastAll(ray.origin, new Vector3(boxCastHalfExtent, boxCastHalfExtent, boxCastHalfExtent), ray.direction, Quaternion.identity, 50f, touchableLayer, QueryTriggerInteraction.Collide).ToList();

        if (colliders.Count == 0)
            return null;

        colliders = colliders.OrderBy(x => Vector3.Distance(x.point, ray.origin)).ToList();

        foreach (var c in colliders)
        {
            Touchable touchable = c.collider.GetComponent<Touchable>();

            if (touchable == null)
                continue;

            if (touchable == null && c.collider.attachedRigidbody != null)
                touchable = c.collider.attachedRigidbody.gameObject.GetComponent<Touchable>();

            if (touchable != null)
                return touchable;
            else
                continue;
        }

        return null;
    }

    IEnumerator TouchHoldCoroutine()
    {
        while (_touchDown)
        {
            if (OnTouchHold != null)
                OnTouchHold(_deltaPosition);
			
            yield return new WaitForEndOfFrame();
        }
    }

    public void IsTouchingUI()
    {
        isTouchingUI = true;
    }
}
