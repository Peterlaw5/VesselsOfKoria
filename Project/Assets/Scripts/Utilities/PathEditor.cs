 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoK
{
    public class PathEditor : MonoBehaviour
    {
        [Header("Path Editor visualization")]
        [Tooltip("Unity Editor Scene path lines color")]
        public Color lineColor = Color.white;
        [Tooltip("Unity Editor Scene path point sphere radius")]
        public float spherePointRadius;
        [Header("Path informations")]
        [Tooltip("Path points, automatically loaded by script")]
        [SerializeField]
        private List<Transform> m_pathPoints = new List<Transform>();
        public List<Transform> PathPoints { get { return m_pathPoints; } }
        //private List<float[]> m_pathPointsDistances; 
        private Transform[] tranformArray;
        //public float startSeparation;
        [SerializeField] private GameObject point = null;
        [Header("Speed Lanes settings")]
        [Tooltip("Area height")]
        public float speedLaneHeight = 4f;
        [Tooltip("Area width")]
        public float speedLaneWidth = 8f;
        [Tooltip("Area width")]
        public float speedLaneOffset = 4f;
        [Tooltip("Walkable Layermask")]
        public LayerMask m_walkableLayer;

        [HideInInspector]
        public bool speedLaneEnabled = false;
        List<BoxCollider> speedLaneColliders;
        public GameObject speedlanePrefab;
        private void Awake()
        {
            speedLaneEnabled = false;
            tranformArray = GetComponentsInChildren<Transform>();
            speedLaneColliders = new List<BoxCollider>();
            m_pathPoints = new List<Transform>();
            m_pathPoints.Clear();
            foreach (Transform pathPoint in tranformArray)
            {
                if (pathPoint != transform)
                {
                    m_pathPoints.Add(pathPoint);
                }
            }
        }

        public void Start()
        {
            // Spawn objects with speedlane colliders
            for(int i = 0; i < m_pathPoints.Count - 1; i++ )
            {
                // GameObject setup
                GameObject g = Instantiate(speedlanePrefab,m_pathPoints[i].transform );
                g.name = "SpeedLane" + i;
                g.tag = "SpeedLane";
                ParticleSystem part = g.GetComponentInChildren<ParticleSystem>();
                // Collider set up
                var shape = part.shape;
               BoxCollider area = g.AddComponent<BoxCollider>();
                area.isTrigger = true;
                g.transform.position = (m_pathPoints[i].position + m_pathPoints[i + 1].position) * 0.5f;
                g.transform.LookAt(m_pathPoints[i + 1]);
                area.size = new Vector3(speedLaneWidth,speedLaneHeight, Vector3.Distance(m_pathPoints[i].position, m_pathPoints[i + 1].position));

                RaycastHit hit;
                if (Physics.Raycast(g.transform.position, Vector3.down,out hit, 100f, m_walkableLayer))
                {
                   area.center = Vector3.down * hit.distance;
                   shape.position= Vector3.down * hit.distance;
                }
                area.enabled = false;
                speedLaneColliders.Add(area);
                part.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        private void Update()
        {
            
        }

        public void EnableSpeedLane()
        {
            if (!speedLaneEnabled)
            {
                /*
                foreach (BoxCollider b in speedLaneColliders)
                {
                    b.enabled = true;
                    b.GetComponentInChildren<ParticleSystem>().Play(true);
                }*/
                speedLaneEnabled = true;
            }
        }

        public void DisableSpeedLane()
        {
            if (speedLaneEnabled)
            {
                foreach (BoxCollider b in speedLaneColliders)
                {
                    b.enabled = false;
                    b.GetComponentInChildren<ParticleSystem>().Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
                }
                speedLaneEnabled = false;
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = lineColor;
            tranformArray = GetComponentsInChildren<Transform>();
            if (tranformArray == null) return;
            m_pathPoints.Clear();
            foreach (Transform pathPoint in tranformArray)
            {
                if (pathPoint != this.transform)
                {
                    m_pathPoints.Add(pathPoint);
                }
            }
            for (int i = 0; i < m_pathPoints.Count; i++)
            {
                Vector3 position = m_pathPoints[i].transform.position;
                Gizmos.DrawWireSphere(position, spherePointRadius);
                if (i > 0)
                {
                    Vector3 previusPosition = m_pathPoints[i - 1].transform.position;
                    Gizmos.DrawLine(position, previusPosition);
                }
            }
        }
        
        // To add points
        public void AddNode(Transform newNodeTransform)
        {
           //Transform lastPoint = pathPoints[pathPoints.Count - 1];
            //GameObject newNode = Instantiate(point, lastPoint.position + new Vector3(startSeparation, 0, 0), Quaternion.identity, transform);
            GameObject newNode = Instantiate(point, newNodeTransform.position, newNodeTransform.rotation, transform);            
            newNode.name = string.Format("Point ({0})", m_pathPoints.Count);
        }

        public float TotalLength(int startIndex = 1, int endIndex = -1)
        {
            float totalLength = 0f;
            if (endIndex == -1) endIndex = m_pathPoints.Count;
            startIndex = Mathf.Clamp(startIndex, 1, endIndex);
            for (int i = startIndex; i < endIndex; i++)
            {
                totalLength += Vector3.Distance(m_pathPoints[i - 1].position, m_pathPoints[i].position);
            }
            return totalLength;
        }

        public float TotalSqrLength(int startIndex = 0, int endIndex = -1)
        {
            float totalLength = 0f;
            if (endIndex == -1) endIndex = m_pathPoints.Count;
            startIndex = Mathf.Clamp(startIndex, 1, endIndex);
            for (int i = startIndex; i < endIndex; i++)
            {
                totalLength += (m_pathPoints[i - 1].position - m_pathPoints[i].position).sqrMagnitude;
            }
            return totalLength;
        }
    }
}
