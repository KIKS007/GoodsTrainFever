using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnEnableEnable : MonoBehaviour
{
    public GameObject gameObjectToEnable;

    // Use this for initialization
    void Start()
    {
        gameObjectToEnable.SetActive(true);
    }

    void OnDisable()
    {
        if (!GlobalVariables.applicationIsQuitting)
            gameObjectToEnable.SetActive(false);
    }
}
