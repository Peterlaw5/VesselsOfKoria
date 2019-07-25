using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoK;

public class LookAtCamera : MonoBehaviour
{
    [Tooltip("Assign to the transform which will be rotated")]
    public Transform transformToRotate;
    [Tooltip("If true, the object won't scale based on distance")]
    public bool fixedScale = false;
    [Tooltip("Scale value when the distance is equal or lower than minDistance")]
    public float minScale;
    [Tooltip("Minimun distance for dynamic scale calculus")]
    public float minDistance;
    [Tooltip("Scale value when the distance is equal or higher than maxDistance")]
    public float maxScale;
    [Tooltip("Maximum distance for dynamic scale calculus")]
    public float maxDistance;
    float scaleMult = 0.2f;
    [Tooltip("If true, the object will look away instead of toward to the transform")]
    public bool flipped = false;

    Camera myCamera;

    private void Awake()
    {
        myCamera = null;
        scaleMult = (maxScale - minScale) / (maxDistance - minDistance);
        if (GamePlayer.local != null)
        {
            myCamera = GamePlayer.local.mainCamera;
        }
        else
        {
            StartCoroutine(CoWaitLocalPlayer());
        }
    }

    void Update()
    {
        if(!GamePlayer.local.IsDead)
        {
            myCamera = GamePlayer.local.mainCamera;
        }
        else
        {
            myCamera = GamePlayer.local.secondaryCamera;
        }

        if (myCamera != null)
        {
            transformToRotate.forward = -myCamera.transform.forward;
            if(flipped) transformToRotate.forward = myCamera.transform.forward;
            if (!fixedScale)
            {
                scaleMult = (maxScale - minScale) / (maxDistance - minDistance);
                float distance = Vector3.Distance(myCamera.transform.position, transformToRotate.position);
                transformToRotate.localScale = Vector3.one * Mathf.Clamp((distance - minDistance) * scaleMult + minScale, minScale,maxScale);
            }
        }
    }

    IEnumerator CoWaitLocalPlayer()
    {
        yield return new WaitUntil(() => GamePlayer.local != null);
        myCamera = GamePlayer.local.mainCamera;
    }
}
