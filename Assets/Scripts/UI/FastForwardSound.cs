using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkTonic.MasterAudio;

public class FastForwardSound : MonoBehaviour
{

    public float Pitch;
    public AudioSource Source;

    public void FastForward()
    {
        Source.pitch = Pitch;
    }

    public void StopFastForward()
    {
        Source.pitch = 1.0f;
    }
}
