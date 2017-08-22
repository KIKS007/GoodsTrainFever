using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPSCounter_UI : MonoBehaviour
{


    private Text _text;

    // Use this for initialization
    void Start()
    {
        _text = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        _text.text = (Mathf.RoundToInt(1.0f / Time.smoothDeltaTime)).ToString("##.00");
    }
}
