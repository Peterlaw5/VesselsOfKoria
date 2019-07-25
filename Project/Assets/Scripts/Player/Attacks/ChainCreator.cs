using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChainCreator : MonoBehaviour
{
    public int numberOfChainLinks;
    public GameObject chainLinkPrefab;
    public GameObject startNode;
    public GameObject endNode;
    public GameObject father;
    public Vector3 startLocalPosition;
    public float lastNodeOffset;
    CharacterJoint lastChainLink;
    CharacterJoint newLink;
    public  GameObject target;
    [Range(0f,1f)]
    public float lerpSpeed;
    // Start is called before the first frame update
    void Start()
    {

        lastChainLink = Instantiate(chainLinkPrefab, startNode.transform).GetComponent<CharacterJoint>();
        lastChainLink.connectedBody = startNode.GetComponent<Rigidbody>();
        lastChainLink.autoConfigureConnectedAnchor = false;
        lastChainLink.connectedAnchor = new Vector3(0, 0, 0);
        for (int i = 1; i < numberOfChainLinks - 2; i++)
        {

            newLink = Instantiate(chainLinkPrefab, lastChainLink.transform).GetComponent<CharacterJoint>();
            newLink.connectedBody = lastChainLink.GetComponent<Rigidbody>();
            lastChainLink = newLink;
        }
        lastChainLink = endNode.GetComponent<CharacterJoint>();
        lastChainLink.connectedBody = newLink.GetComponent<Rigidbody>();
        endNode.transform.parent.transform.localPosition = new Vector3(startLocalPosition.x, startLocalPosition.y, startLocalPosition.z);
        lastChainLink.autoConfigureConnectedAnchor = false;
        lastChainLink.connectedAnchor = new Vector3(0, 0, 0);

    }

    public void StarCofollow()
    {
        StartCoroutine(CoFollow());
    }
    IEnumerator CoFollow()
    {
        GameObject start = startNode.transform.parent.gameObject;
        GameObject end = endNode.transform.parent.gameObject;
        while (target!=null)
        {
           end.transform.position = target.transform.position;
            yield return null;
        }
        Destroy(start,0.5f);
        Destroy(end,0.5f);
        if(start==null)
        {
            yield break;
        }
        yield return null;
        while(start != null & end != null && transform != null && Vector3.Distance(end.transform.position, start.transform.position)>0.2f)
        {
            end.transform.position = Vector3.Lerp(end.transform.position, start.transform.position, lerpSpeed);
            yield return null;
        }
        var p = GetComponentInParent<Empty>();
        Destroy(start);
        Destroy(p.gameObject);
        
     }
}
