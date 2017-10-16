using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector;
using UnityEngine.UI;

public class MenuButtonComponent : MonoBehaviour, ISubmitHandler, IPointerClickHandler
{
    [Header("To Menu")]
    public bool toMenu = true;
    [ShowIfAttribute("toMenu")]
    public MenuComponent menu;

    private Button _button;

    // Use this for initialization
    void Awake()
    {
        _button = GetComponent<Button>();

        MenuManager.Instance.OnMenuTransitionStart += OnTransitionStart;
        MenuManager.Instance.OnMenuTransitionEnd += OnTransitionEnd;
    }

    public void OnSubmit(BaseEventData eventData)
    {
        Submit();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Submit();
    }

    void Submit()
    {
        if (!_button.interactable)
            return;

        if (toMenu && menu)
            MenuManager.Instance.ToMenu(menu);
    }

    void OnTransitionStart()
    {
        _button.interactable = false;
    }

    void OnTransitionEnd()
    {
        _button.interactable = true;
    }

    void OnDestroy()
    {
        if (!MenuManager.applicationIsQuitting)
        {
            MenuManager.Instance.OnMenuTransitionStart -= OnTransitionStart;
            MenuManager.Instance.OnMenuTransitionEnd -= OnTransitionEnd; 
        }
    }
}
