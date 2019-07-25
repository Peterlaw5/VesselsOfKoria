using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBaseCollider : MonoBehaviour
{
    ContactPoint[] contactPoints;
    ContactPoint lowestPoint;
    [Tooltip("Layer mask to check where jumping is allowed")]
    public LayerMask walkableLayer;
    
    public bool isGrounded;

    [Range(0f, 90f)]
    public float angleLimit = 60f;
    public float currentAngle;

    public int collisionCounter = 0;
    bool firstCollisionCheck;
    bool resetLowest = false;

    private void Awake()
    {
        resetLowest = false;
        collisionCounter = 0;
        currentAngle = 0f;
    }

    private void Start()
    {
        isGrounded = false;
        firstCollisionCheck = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        collisionCounter++;
        resetLowest = true;
    }

    private void OnCollisionStay(Collision collision)
    {
        if(firstCollisionCheck)
        {
            resetLowest = true;
            firstCollisionCheck = false;
        }
        GetLowestContact(collision);
        currentAngle = Vector3.Angle(Vector3.up, lowestPoint.normal);//Vector3.SignedAngle(Vector3.up, lowestPoint.normal, Vector3.Cross(Vector3.up, lowestPoint.normal));
        isGrounded = (currentAngle < angleLimit) && (((1 << lowestPoint.otherCollider.gameObject.layer) & walkableLayer) != 0);// lowestPoint.otherCollider.gameObject.layer == walkableLayer.value;
    }

    private void OnCollisionExit(Collision collision)
    {
        collisionCounter--;
        resetLowest = true;
    }

    private void Update()
    {
        if(collisionCounter > 0)
        {
            if (lowestPoint.otherCollider != null)
            {
                currentAngle = Vector3.SignedAngle(Vector3.up, lowestPoint.normal, Vector3.Cross(Vector3.up, lowestPoint.normal));
                isGrounded = (currentAngle < angleLimit) && (((1 << lowestPoint.otherCollider.gameObject.layer) & walkableLayer) != 0);// lowestPoint.otherCollider.gameObject.layer == walkableLayer.value;
            }
        }
        else
        {
            if (isGrounded)
            {
                isGrounded = false;
                collisionCounter = 0;
            }
        }
    }

    private void LateUpdate()
    {
        firstCollisionCheck = true;
    }

    void GetLowestContact(Collision col)
    {
        if(resetLowest)
        {
            lowestPoint = col.GetContact(0);
            resetLowest = false;
        }
        for (int i = 0; i < col.contactCount; i++)
        {
            if(lowestPoint.point.y > col.GetContact(i).point.y)
            {
                lowestPoint = col.GetContact(i);
            }
        }
    }

}
