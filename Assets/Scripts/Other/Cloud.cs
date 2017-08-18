using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cloud : MonoBehaviour
{


    public Vector3 velocity;
    public float Xlimit;
    public float XBirth;
    public Vector2 ZBirthRange;
    public Vector2 XBirthScaleRange;
    public Vector2 ZBirthScaleRange;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.position += velocity * Time.deltaTime;
        if (transform.position.x > Xlimit)
        {
            Vector3 birth = new Vector3(XBirth, transform.position.y, Random.Range(ZBirthRange.x, ZBirthRange.y));
            Vector3 birthScale = new Vector3(Random.Range(XBirthScaleRange.x, XBirthScaleRange.y), transform.localScale.y, Random.Range(ZBirthScaleRange.x, ZBirthScaleRange.y));
            transform.position = birth;
            transform.localScale = birthScale;
        }
    }
}
