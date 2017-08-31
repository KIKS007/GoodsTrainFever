using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour
{
    public Vector2 Speed;
    public float NoiseScale = 1;
    public float HeightScale = 1;

    private Transform _transform;
    private Vector3 _initialScale;
    private Vector3 _initialPosition;
    private Vector2 _noisePos;

    // Use this for initialization
    void Start()
    {
        _transform = GetComponent<Transform>();
        _initialScale = _transform.localScale;
        _initialPosition = _transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        _noisePos += Speed * Time.deltaTime;
        var height = Mathf.PerlinNoise((_transform.position.x + _noisePos.x) * NoiseScale, (_transform.position.z + _noisePos.y) * NoiseScale) * HeightScale;
        _transform.localScale = _initialScale + Vector3.up * height;
        _transform.position = _initialPosition + Vector3.up * height / 2f;
    }
}
