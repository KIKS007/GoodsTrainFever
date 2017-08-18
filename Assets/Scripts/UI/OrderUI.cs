﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Sirenix.OdinInspector;
using UniRx;

public class OrderUI : MonoBehaviour
{
    public GameObject OrderPrefab;
    public GameObject SmallContainerPrefab;
    public GameObject LargeContainerPrefab;
    public List<Color> ColorUi;

    private Dictionary<Order_Level, GameObject> _orders = new Dictionary<Order_Level, GameObject>(); // stock order gameObject representation
    private List<Order_Level> _orderList = new List<Order_Level>();// stock orders following their arrival
    private Dictionary<Container_Level, GameObject> _containerRepresentations = new Dictionary<Container_Level, GameObject>();//stock container representation
    private bool _showOrders = false;//if true, all the orders are shown, when false only the first three
    private VerticalLayoutGroup _layout;//the vertical layout holding all the orders
    private Transform _notification;//the notification transform
    private Image _notificationImg;//the notification image
    private int _notificationPos = 0;//the position target of the notification (0 when there are less than 3 orders, 100 otherwise)
    // Use this for initialization
    void Start()
    {
        _layout = GetComponent<VerticalLayoutGroup>();
        _notification = transform.GetChild(0);
        _notification.gameObject.SetActive(false);
        _notificationImg = _notification.GetComponentInChildren<Image>();
    }

    /// <summary>
    /// Add an order to the UI panel
    /// </summary>
    /// <param name="order">the order to add</param>
    public void AddOrder(Order_Level order)
    {
        //we instantiate the new order GameObject and we move the notification to the bottom
        var orderGO = Instantiate(OrderPrefab, transform.position, Quaternion.identity, transform);
        _orderList.Add(order);
        _notification.SetAsLastSibling();

        //according to what we have to show we set the alpha of the new order
        //if alpha is 0 then we desactivate the GO to save perf
        if (!_showOrders)
            orderGO.GetComponent<CanvasGroup>().alpha = 1f - orderGO.transform.GetSiblingIndex() / 3f;
        if (orderGO.GetComponent<CanvasGroup>().alpha <= 0)
            orderGO.SetActive(false);

        //we add the order to the dictionnary
        //we set the container holder
        //we then populate the container Holder
        _orders.Add(order, orderGO);
        Transform[] holders = { orderGO.transform.GetChild(0).GetChild(1), orderGO.transform.GetChild(0).GetChild(2), orderGO.transform.GetChild(0).GetChild(3), orderGO.transform.GetChild(0).GetChild(4) };
        StartCoroutine(populateUi(order.levelContainers, holders));


        //if we have more than 3 orders we set the notification pos to change accordingly
        if (_orderList.Count > 3)
            _notificationPos = 100;


        //notification animation
        if (!_notification.gameObject.activeInHierarchy)
        {
            _notification.gameObject.SetActive(true);
            (_notification.GetChild(0) as RectTransform).DOAnchorPosX(300, 0.4f).SetEase(Ease.OutBack).OnComplete(() =>
            {
                DOVirtual.DelayedCall(0.7f, () =>
                {
                    (_notification.GetChild(0) as RectTransform).DOAnchorPosX(_notificationPos, 0.3f).SetEase(Ease.OutExpo).OnComplete(() =>
                    {
                        if (_notificationPos == 0)//we only need to desactivate the notification GO if don't see it anymore
                            _notification.gameObject.SetActive(false);
                    });
                });
            });
        }
    }

    /// <summary>
    /// Populate the Holders with the following containers list
    /// </summary>
    /// <param name="containers">the containers list</param>
    /// <param name="holders">the holders transforms</param>
    /// <returns>it's a coroutine</returns>
    IEnumerator populateUi(List<Container_Level> containers, Transform[] holders)
    {
        //we want all the big container first
        var larges = containers.FindAll(x => x.isDoubleSize);
        var small = containers.FindAll(x => !x.isDoubleSize);
        containers = new List<Container_Level>();
        containers.AddRange(larges);
        containers.AddRange(small);


        foreach (var c in containers)
        {
            var prefab = c.isDoubleSize ? LargeContainerPrefab : SmallContainerPrefab;//we select the right prefab according to its type
            var layoutElement = prefab.GetComponent<LayoutElement>();

            var holder = holders[(int)c.containerType].GetChild(0) as RectTransform;

            yield return new WaitForEndOfFrame();// we need to wait one frame so the content size fitter of the row has been updated
            yield return new WaitUntil(() => holder != null && holder.gameObject.activeInHierarchy);//if the row isn't active it can't be updated

            if (holder.rect.width + layoutElement.minWidth + 10 >= 270)//if the row isn't big enough we take the second one
                holder = holders[(int)c.containerType].GetChild(1) as RectTransform;

            var representation = Instantiate(prefab, Vector3.zero, Quaternion.identity, holder);// we add the container GO to to holder

            //second verification to be sure that we're correct
            yield return new WaitForEndOfFrame();
            yield return new WaitUntil(() => holder != null && holder.gameObject.activeInHierarchy);
            if (holder.rect.width + layoutElement.minWidth + 10 >= 270)
                representation.transform.SetParent(holders[(int)c.containerType].GetChild(1));

            //we set the color of container
            if (c.containerColor != ContainerColor.Random)
                representation.GetComponent<Image>().color = ColorUi[(int)c.containerColor - 1];

        }

    }

    /// <summary>
    /// Remove an holder UI
    /// </summary>
    /// <param name="order">the order to remove, if null nothing append</param>
    public void RemoveOrder(Order_Level order)
    {
        if (_orders.ContainsKey(order))
        {

            var target = _orders[order];
            target.transform.GetChild(0).DOMoveX(Screen.width * 1.8f, 1f).SetRelative();// we move the order to the right

            //we collapse it to the top
            (target.transform as RectTransform).DOSizeDelta(new Vector2((target.transform as RectTransform).rect.width, -5), 0.2f).SetDelay(0.5f).OnComplete(() =>
            {
                if (!_showOrders)
                    HideOrders();
                Destroy(target);
            }).OnUpdate(() =>
            {   //this is needed to update the vertical layout
                _layout.CalculateLayoutInputHorizontal();
                _layout.CalculateLayoutInputVertical();
                _layout.SetLayoutHorizontal();
                _layout.SetLayoutVertical();
            });

            //we remove the order from the dictionnary and the from the list
            _orders.Remove(order);
            _orderList.Remove(order);

            //if there is less than 3 orders we need to collapse the notification
            if (_orderList.Count == 3)
            {
                _notificationPos = 0;
                (_notification.GetChild(0) as RectTransform).DOAnchorPosX(_notificationPos, 0.5f).OnComplete(() => _notification.gameObject.SetActive(false));
            }
        }
    }

    /// <summary>
    /// clear all orders without transition
    /// /// </summary>
    public void ClearAllOrder()
    {
        foreach (var order in _orderList)
        {
            Destroy(_orders[order]);
            _orders.Remove(order);
        }
        _orderList.Clear();

        _notificationPos = 0;
        (_notification.GetChild(0) as RectTransform).DOAnchorPosX(_notificationPos, 0.5f).OnComplete(() => _notification.gameObject.SetActive(false));
    }

    /// <summary>
    /// Show all orders
    /// </summary>
    public void ShowOrders()
    {
        _showOrders = true;
        foreach (var order in _orders)
        {
            var canvasGrp = order.Value.GetComponent<CanvasGroup>();
            if (!order.Value.activeSelf)
                order.Value.SetActive(true);
            canvasGrp.DOKill();
            canvasGrp.DOFade(1, 0.2f);
        }
        _notificationImg.DOKill();
        _notificationImg.DOFade(0, 0f);
    }

    /// <summary>
    /// Show only the first three orders
    /// can also be used to recalculate the alpha of the orders
    /// </summary>
    public void HideOrders()
    {
        _showOrders = false;
        int i = 0;
        _notificationImg.DOKill();
        _notificationImg.DOFade(1, 0f).SetDelay(0.4f);
        foreach (var order in _orderList)
        {
            var go = _orders[order];
            var canvasGrp = go.GetComponent<CanvasGroup>();
            float alphaTarget = 1f - i / 3f;
            canvasGrp.DOKill();
            canvasGrp.DOFade(alphaTarget, 0.2f);
            if (alphaTarget <= 0)
                DOVirtual.DelayedCall(0.2f, () => go.SetActive(false));
            if (alphaTarget > 0 && !go.activeSelf)
                go.SetActive(true);
            i++;
        }
    }

}
