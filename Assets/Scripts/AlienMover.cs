using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlienMover : MonoBehaviour {

    public float speed = 1f;

    private Vector3 originalPosition;

    float offsetY, offsetX;

    // Use this for initialization
    void Start()
    {
        originalPosition = transform.localPosition;

        offsetX = Random.value * 1000f;
        offsetY = Random.value * 1000f;
    }

    // Update is called once per frame
    void Update()
    {
        float sinValY = Mathf.Sin(Time.time * speed + offsetY * Mathf.PI);
        float sinValX = Mathf.Sin(Time.time * speed * 0.7f + offsetX * Mathf.PI);

        transform.localPosition = originalPosition + Vector3.up * 0.2f * Mathf.Abs(sinValY) + Vector3.right * sinValX * 0.3f;
    }
}
