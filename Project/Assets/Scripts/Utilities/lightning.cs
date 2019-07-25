using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lightning : MonoBehaviour
{
    // Start is called before the first frame update
    private LineRenderer line;
    public int nSegments;
    public float segmentsLenght;
    public float segmentslLengtOffset;
    public float XYoffset;
    public float maxArc;
    public float maxArcOffset;
    float newY;
    float y;
    float x;
    float newX;
    bool yIsInverted;
    bool xIsInverted;
    public int maxDepth;
    public int maxNumChildren;
    public int depth;
    public int childrenCount;
    public GameObject prefabToInstantiate;
    void Start()
    {
        
        if (transform.parent==null)
        {
            Debug.Log("generated");
         
            Generate();
        }
    }

    void Generate()
    {
        line = GetComponent<LineRenderer>();
        yIsInverted = false;
        line.positionCount = nSegments;
        float z = 0;
        y = 0;
        float offsetX = Random.Range(0, maxArc + Random.Range(-maxArcOffset, maxArcOffset));
        float offsetY = Random.Range(0, maxArc + Random.Range(-maxArcOffset, maxArcOffset));
        for (int i = 0; i < line.positionCount; i++)
        {
            newY = Random.Range(0f, XYoffset);
            newX = Random.Range(0f, XYoffset); ;
            z = z + segmentsLenght + Random.Range(-segmentslLengtOffset, segmentslLengtOffset);
            calc(yIsInverted, xIsInverted);
            if (y < offsetY && !yIsInverted || y > offsetY && yIsInverted)
            {
            }
            else
            {
                offsetY = Random.Range(0, maxArc + Random.Range(-maxArcOffset, maxArcOffset));
                yIsInverted = !yIsInverted;
            }
            if (x < offsetX && !xIsInverted || x > offsetX && xIsInverted)
            {
            }
            else
            {
                offsetX = Random.Range(0, maxArc + Random.Range(-maxArcOffset, maxArcOffset));
                xIsInverted = !xIsInverted;
            }
            line.SetPosition(i, new Vector3(x, y, z));

        }
        if (depth <=maxDepth)
        {
            for (int i = line.positionCount/4; i < line.positionCount*0.75; i++)
            {
                float rnd = Random.Range(0f, 1f);
                Debug.Log(rnd);
                if (childrenCount < maxNumChildren && maxNumChildren>=1)
                {
                  
                    if (rnd<=0.05f)
                    {
                        
                        lightning l = Instantiate(prefabToInstantiate,line.GetPosition(i),Quaternion.Euler(Random.Range(-90,90),0,0) , transform).GetComponent<lightning>();
                        l.nSegments = nSegments;
                        l.segmentsLenght = segmentsLenght *0.6f;
                        l.segmentslLengtOffset = segmentslLengtOffset  *0.6f;
                        l.XYoffset = XYoffset * 0.6f;
                        l.maxArc = maxArc * 0.6f;
                        l.maxArcOffset = maxArcOffset * 0.6f;
                        l.maxDepth = maxDepth;
                        l.maxNumChildren = maxNumChildren / 2;
                        l.depth = depth + 1;
                        //l.line.widthMultiplier = line.widthMultiplier / 2;
                        l.Generate();
                        childrenCount++;
                    }
                }
            }
        }

        void calc(bool invertY, bool invertX)
        {
            if (invertY)
            {
                y = y - newY;
            }
            else
            {
                y = y + newY;
            }
            if (invertX)
            {
                x = x - newY;
            }
            else
            {
                x = x + newY;
            }
        }
    }
}
