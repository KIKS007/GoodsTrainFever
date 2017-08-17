using System;
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

    private Dictionary<Order_Level, GameObject> _orders = new Dictionary<Order_Level, GameObject>();
    private List<Order_Level> _orderQueue = new List<Order_Level>();
    private Dictionary<Container_Level, GameObject> _containerRepresentations = new Dictionary<Container_Level, GameObject>();
    private bool _showOrders = false;
    private VerticalLayoutGroup _layout;
    private CanvasScaler _scaler;
    private Transform _notification;
    // Use this for initialization
    void Start()
    {
        Array colors = Enum.GetValues(typeof(ContainerColor));
        Array types = Enum.GetValues(typeof(ContainerType));
        _layout = GetComponent<VerticalLayoutGroup>();
        _scaler = GetComponentInParent<CanvasScaler>();
        _notification = transform.GetChild(0);
        _notification.gameObject.SetActive(false);

        for (int x = 0; x < 5; x++)
        {
            Order_Level test = new Order_Level();
            for (int i = 0; i < 16; i++)
            {
                Container_Level cont = new Container_Level();
                cont.containerColor = (ContainerColor)colors.GetValue(UnityEngine.Random.Range(0, colors.Length));
                cont.containerType = (ContainerType)types.GetValue(UnityEngine.Random.Range(0, types.Length));
                cont.isDoubleSize = UnityEngine.Random.Range(0, 2) == 0 ? true : false;
                test.levelContainers.Add(cont);
            }
            Observable.Timer(TimeSpan.FromSeconds(1f + x * 0.5f)).Subscribe(_ =>
                 {
                     AddOrder(test);
                 });
            Observable.Timer(TimeSpan.FromSeconds(3 + x * 2)).Subscribe(_ =>
                 {
                     //    (_orders[test].transform as RectTransform).DOSizeDelta(new Vector2((_orders[test].transform as RectTransform).rect.width, 0), 2f).OnUpdate(() =>
                     //    {
                     //        _layout.CalculateLayoutInputHorizontal();
                     //        _layout.CalculateLayoutInputVertical();
                     //        _layout.SetLayoutHorizontal();
                     //        _layout.SetLayoutVertical();
                     //    });
                     RemoveOrder(test);
                 });
            test.isPrepared = false;
        }

    }

    public void AddOrder(Order_Level order)
    {
        var orderGO = Instantiate(OrderPrefab, transform.position, Quaternion.identity, transform);
        _orderQueue.Add(order);
        _notification.SetAsLastSibling();
        if (!_showOrders)
            orderGO.GetComponent<CanvasGroup>().alpha = 1f - orderGO.transform.GetSiblingIndex() / 3f;
        if (orderGO.GetComponent<CanvasGroup>().alpha <= 0)
            orderGO.SetActive(false);
        _orders.Add(order, orderGO);
        Transform[] holders = { orderGO.transform.GetChild(0).GetChild(1), orderGO.transform.GetChild(0).GetChild(2), orderGO.transform.GetChild(0).GetChild(3), orderGO.transform.GetChild(0).GetChild(4) };
        StartCoroutine(populateUi(order.levelContainers, holders));
        if (!_notification.gameObject.activeInHierarchy)
        {
            _notification.gameObject.SetActive(true);
            (_notification.GetChild(0) as RectTransform).DOAnchorPosX(100, 0.4f).SetEase(Ease.OutBack);
            (_notification.GetChild(0) as RectTransform).DOAnchorPosX(0, 0.3f).SetDelay(1.2f).SetEase(Ease.OutExpo).OnComplete(() => _notification.gameObject.SetActive(false));
        }
    }

    IEnumerator populateUi(List<Container_Level> containers, Transform[] holders)
    {
        var larges = containers.FindAll(x => x.isDoubleSize);
        var small = containers.FindAll(x => !x.isDoubleSize);
        containers = new List<Container_Level>();
        containers.AddRange(larges);
        containers.AddRange(small);
        foreach (var c in containers)
        {
            var prefab = c.isDoubleSize ? LargeContainerPrefab : SmallContainerPrefab;
            var layoutElement = prefab.GetComponent<LayoutElement>();

            var holder = holders[(int)c.containerType].GetChild(0) as RectTransform;
            yield return new WaitForEndOfFrame();
            yield return new WaitUntil(() => holder.gameObject.activeInHierarchy);
            if (holder.rect.width + layoutElement.minWidth + 10 >= 270)
                holder = holders[(int)c.containerType].GetChild(1) as RectTransform;

            var representation = Instantiate(prefab, Vector3.zero, Quaternion.identity, holder);

            yield return new WaitForEndOfFrame();
            yield return new WaitUntil(() => holder.gameObject.activeInHierarchy);
            if (holder.rect.width + layoutElement.minWidth + 10 >= 270)
                representation.transform.SetParent(holders[(int)c.containerType].GetChild(1));

            if (c.containerColor != ContainerColor.Random)
                representation.GetComponent<Image>().color = ColorUi[(int)c.containerColor - 1];

        }

    }

    public void RemoveOrder(Order_Level order)
    {
        if (_orders.ContainsKey(order))
        {

            var target = _orders[order];
            target.transform.GetChild(0).DOMoveX(Screen.width * 1.8f, 1f).SetRelative();
            (target.transform as RectTransform).DOSizeDelta(new Vector2((target.transform as RectTransform).rect.width, -5), 0.2f).SetDelay(0.5f).OnComplete(() =>
            {
                if (!_showOrders)
                    HideOrders();
                Destroy(target);
            }).OnUpdate(() =>
            {
                _layout.CalculateLayoutInputHorizontal();
                _layout.CalculateLayoutInputVertical();
                _layout.SetLayoutHorizontal();
                _layout.SetLayoutVertical();
            });
            _orders.Remove(order);
            _orderQueue.Remove(order);

        }
    }

    public void ShowOrders()
    {
        _showOrders = true;
        foreach (var order in _orders)
        {
            var canvasGrp = order.Value.GetComponent<CanvasGroup>();
            if (!order.Value.activeSelf)
                order.Value.SetActive(true);
            canvasGrp.DOFade(1, 0.2f);
        }
    }

    public void HideOrders()
    {
        _showOrders = false;
        int i = 0;
        foreach (var order in _orderQueue)
        {
            var go = _orders[order];
            var canvasGrp = go.GetComponent<CanvasGroup>();
            float alphaTarget = 1f - i / 3f;
            canvasGrp.DOFade(alphaTarget, 0.2f);
            if (alphaTarget <= 0)
                DOVirtual.DelayedCall(0.2f, () => go.SetActive(false));
            if (alphaTarget > 0 && !go.activeSelf)
                go.SetActive(true);
            i++;
        }
    }

}
