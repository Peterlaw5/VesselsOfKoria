using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    // Start is called before the first frame update
    public float maxSpeed;
    public float minSpeed;
    public bool x;
    public bool y;
    public bool z;
    float xSpeed;
    float ySpeed;
    float zSpeed;
    void Start()
    {
        if(x) xSpeed = Random.Range(minSpeed, maxSpeed);
        if (y) ySpeed = Random.Range(minSpeed, maxSpeed);
        if (z) zSpeed = Random.Range(minSpeed, maxSpeed);
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(xSpeed * Time.deltaTime, ySpeed * Time.deltaTime, zSpeed * Time.deltaTime);
    }
}
