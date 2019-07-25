
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChainCreatorUltimate : MonoBehaviour
{
    // Start is called before the first frame update
    public bool bonded;
    public List<AnimationCurve> xSearchAnimations;
    public List<AnimationCurve> ySearchAnimations;
    public List<AnimationCurve> zSearchAnimations;
    public GameObject lsatNodeTarget;
    public int numberOfChainLinks;
    public  GameObject chainLinkPrefab;
    public  GameObject startNode;
    public GameObject endNode;
    public Vector3 startLocalPosition;
    public float lastNodeOffset;
    CharacterJoint lastChainLink;
    CharacterJoint newLink;
    Coroutine follow;
    public float rotationX;
    public float rotationY;
    public float rotationZ;
    void Start()
    {
        
        lastChainLink = Instantiate(chainLinkPrefab, startNode.transform).GetComponent<CharacterJoint>();
        lastChainLink.connectedBody = startNode.GetComponent<Rigidbody>();
        lastChainLink.autoConfigureConnectedAnchor = false;
        lastChainLink.connectedAnchor = new Vector3(0, 0, 0);
        for (int i = 1; i < numberOfChainLinks-2; i++)
        {
            
            newLink = Instantiate(chainLinkPrefab, lastChainLink.transform).GetComponent<CharacterJoint>();
            newLink.connectedBody = lastChainLink.GetComponent<Rigidbody>();
            lastChainLink=newLink;
        }
        lastChainLink = endNode.GetComponent<CharacterJoint>();
        lastChainLink.connectedBody = newLink.GetComponent<Rigidbody>();
        endNode.transform.parent.transform.localPosition = new Vector3(startLocalPosition.x, startLocalPosition.y, startLocalPosition.z);
        lastChainLink.autoConfigureConnectedAnchor = false;
        lastChainLink.connectedAnchor = new Vector3(0, 0, 0);
        follow = StartCoroutine(CoSearchAnimation());
        
    }

    IEnumerator CoSearchAnimation()
    {
    
        AnimationCurve x = xSearchAnimations[(int)Random.Range(0,xSearchAnimations.Count)];
        AnimationCurve y= ySearchAnimations[(int)Random.Range(0, ySearchAnimations.Count)];
        AnimationCurve z = zSearchAnimations[(int)Random.Range(0, zSearchAnimations.Count)];
        float timer = 0;
        while(x.keys[x.keys.Length - 1].time>timer)
        {
            lastChainLink.transform.parent.localPosition= Vector3.Lerp(lastChainLink.transform.parent.localPosition, new Vector3(startLocalPosition.x + x.Evaluate(timer), startLocalPosition.y + y.Evaluate(timer), startLocalPosition.z+z.Evaluate(timer)),0.1f);
            timer +=0.1f*Time.deltaTime;
            yield return null;
        }
        while (Vector3.Distance(lastChainLink.transform.localPosition, new Vector3(startLocalPosition.x, startLocalPosition.y, startLocalPosition.z)) < 0.1)
        {
            lastChainLink.transform.parent.localPosition = Vector3.Lerp(lastChainLink.transform.parent.localPosition, new Vector3(startLocalPosition.x, startLocalPosition.y, startLocalPosition.z), 0.1f);
            yield return null;
        }
        follow =StartCoroutine(CoSearchAnimation());
    
    }
    IEnumerator CoFollowTarget()
    {
      
       while (lsatNodeTarget!=null)
       {
          
           lastChainLink.transform.parent.position = Vector3.Lerp(lastChainLink.transform.parent.position, lsatNodeTarget.transform.position, 0.5f);
            yield return null;
       }

       while (Vector3.Distance(lastChainLink.transform.localPosition,new Vector3(startLocalPosition.x, startLocalPosition.y, startLocalPosition.z)) < 0.1) 
       {
            lastChainLink.transform.parent.localPosition = Vector3.Lerp(lastChainLink.transform.parent.localPosition, new Vector3(startLocalPosition.x, startLocalPosition.y, startLocalPosition.z), 0.1f);
       }
       lastChainLink.transform.parent.localPosition = new Vector3(startLocalPosition.x, startLocalPosition.y, startLocalPosition.z);
       bonded = false;
       follow = null;
      
    }
    public void SartFollowTarget()
    {
        if(follow==null)
        {
            follow=StartCoroutine(CoFollowTarget());
            bonded = true;
        }
        else
        {
            StopCoroutine(follow);
            follow= StartCoroutine(CoFollowTarget());
            bonded = true;
        }
    }
    // Update is called once per frame
    void Update()
    {
        //Debug.Log(endNode.transform.parent.localPosition.x);
        if(!bonded)
        {
            transform.Rotate(rotationX * Time.deltaTime,rotationY * Time.deltaTime, rotationZ* Time.deltaTime);
        }
       /* if(endNode.transform.parent.localPosition.x>lastNodeOffset)
        {
            endNode.transform.parent.localPosition =new Vector3( lastNodeOffset,0,0);
        }
        if(bonded && lsatNodeTarget!=null&&follow!=null)
        {
            follow = StartCoroutine(CoFollowTarget());
        }
        */
    }
}
