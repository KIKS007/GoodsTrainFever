using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class OderNavigationTest : MonoBehaviour
{
    public OrderUI target;
    public void AddOrder()
    {
        Array colors = Enum.GetValues(typeof(ContainerColor));
        Array types = Enum.GetValues(typeof(ContainerType));

        Order_Level test = new Order_Level();
        for (int i = 0; i < 16; i++)
        {
            Container_Level cont = new Container_Level();
            cont.containerColor = (ContainerColor)colors.GetValue(UnityEngine.Random.Range(0, colors.Length));
            cont.containerType = (ContainerType)types.GetValue(UnityEngine.Random.Range(0, types.Length));
            cont.isDoubleSize = UnityEngine.Random.Range(0, 2) == 0 ? true : false;
            test.levelContainers.Add(cont);
        }
        target.AddOrder(test);
        Observable.Timer(TimeSpan.FromSeconds(UnityEngine.Random.Range(3f, 15f))).Subscribe(_ =>
             {
                 target.RemoveOrder(test);
             });
        test.isPrepared = false;

    }
}
