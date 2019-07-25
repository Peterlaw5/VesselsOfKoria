using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

namespace VoK
{
    public class DeathCamera : MonoBehaviour
    {
        
        public float speedCamera = 0.5f;
        public bool invertXAxis = false;
        public bool invertYAxis = false;                     
        public float maxDistance = 5f;
        public float startXpos = -5f;
        public float startYpos = 3f;
        public float startZpos = 0f;

        public Image killCamFadePanel;
        public float fadeInTime = 0.5f;
        public float fadeOutTime = 0.5f;
        public float collisionSpeedCamera = 1f;
        //public float targetPointYCamera = 1.5f;

        Camera myCamera;
        float distance;

        //public float angle = 20f;

        GameObject m_player;
        GamePlayer m_playerController;
        Transform targetTransform;
        public LayerMask walkableLayerMask;

        float invertX;
        float invertY;

        private void Awake()
        {
            m_playerController = null;
            targetTransform = null;
        }

        //private void Start()
        //{            
        //    m_playerController = m_player.GetComponent<GamePlayer>();

        //   if(!m_playerController.isLocalPlayer)
        //   {
        //        Destroy(gameObject);
        //        return;
        //   }


        //}

        private void OnEnable()
        {
            Debug.Log("death camera is enables");
            m_player = transform.parent.gameObject;     
            
            StopCoroutine("CoWaitLocalPlayer");
            StartCoroutine("CoWaitLocalPlayer");

        }

        private void OnDisable()
        {
            transform.position = transform.parent.position;
           
        }
/*
        public void Update()
        {          
            
            float distance = maxDistance;

            invertX = invertXAxis ? -1f : 1f;
            invertY = invertYAxis ? -1f : 1f;

            if (m_playerController == null) return;

          

            if(m_playerController.isLocalPlayer && m_playerController.IsDead)
            {               
                float verticalAngle = 0f;
                float horizontalAngle = 0f;

                Vector3 pos = transform.localPosition;
                
                if (Input.GetAxis("Mouse X") > 0f)
                {
                   horizontalAngle += Time.deltaTime * speedCamera * invertX;
                    
                }

                if (Input.GetAxis("Mouse X") < 0f)
                {
                    horizontalAngle -= Time.deltaTime * speedCamera * invertX;
                  
                }

                
                if (Input.GetAxis("Mouse Y") < 0f)
                {
                    verticalAngle += Time.deltaTime * speedCamera * invertY;


                }

                if (Input.GetAxis("Mouse Y") > 0f)
                {
                    verticalAngle -= Time.deltaTime * speedCamera * invertY;

                }
               


                //rotazione
                pos = Quaternion.Euler(0, horizontalAngle, verticalAngle) * pos;

                pos = pos.normalized * distance;
                transform.localPosition = pos;
                transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles.x + horizontalAngle, transform.localRotation.eulerAngles.y + verticalAngle, 5f);

                transform.LookAt(m_player.transform.position);

                //transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles.x, transform.localRotation.eulerAngles.y, 0f);
                //transform.Rotate(m_player.transform.position, angle);
                //transform.RotateAround(m_player.transform.position,new Vector3(0,1,0), angle* Time.deltaTime);
            }
            


        }*/



        public void SetTargetTransform(int indexMove = 0)
        {
            //find all players alive (same team)
            List<GamePlayer> teamPlayersAlive = new List<GamePlayer>();

            for(int i = 0; i < GamePlayer.allPlayers.Count; i++)
            {
                GamePlayer currentPlayer = GamePlayer.allPlayers[i];
                if (!currentPlayer.IsDead && currentPlayer.Team == m_playerController.Team)
                {
                    teamPlayersAlive.Add(currentPlayer);
                    Debug.Log("added alive player to list");
                }
            }

            Debug.Log("alive players are: " + teamPlayersAlive.Count);
            //move camera to titan
            if (teamPlayersAlive.Count == 0)
            {
                targetTransform = GameManager.instance.GetTitan(m_playerController.Team).cameraPosition;
                transform.parent = targetTransform;

                Debug.Log("no alive players founds, using titan");
            }
            else
            {
                if(indexMove == 0)
                {
                    targetTransform = teamPlayersAlive[0].cameraPosition;
                    //targetTransform.position += new Vector3(0, changeYPos, 0);
                    //transform.parent = targetTransform;

                    //test
                    transform.parent = targetTransform;
                    Debug.Log("usign first player");
                }
                else
                {
                    int index = 0;
                    for(int i = 0; i< teamPlayersAlive.Count; i++)
                    {
                        if(teamPlayersAlive[i].cameraPosition == targetTransform)
                        {
                            index = i;
                            break;
                        }
                    }

                    int targetIndex = index + indexMove;

                    //from first to last player alive
                    while(targetIndex < 0)
                    {
                        targetIndex = targetIndex + teamPlayersAlive.Count;
                    }

                    //from last to first player alive
                    while(targetIndex > teamPlayersAlive.Count - 1)
                    {
                        targetIndex = targetIndex - teamPlayersAlive.Count;
                    }
                    //if (targetIndex < 0)
                    //{
                    //    targetIndex = targetIndex + teamPlayersAlive.Count;
                    //}

                    ////from last to first player alive
                    //if (targetIndex > teamPlayersAlive.Count - 1)
                    //{
                    //    targetIndex = targetIndex - teamPlayersAlive.Count;
                    //}

                    //targetTransform = teamPlayersAlive[targetIndex].transform;
                    targetTransform = teamPlayersAlive[targetIndex].cameraPosition;
                    //targetTransform.position += new Vector3(0, changeYPos, 0);
                   //test1
                    //transform.parent = targetTransform;


                    //////////////////////
                    transform.parent = targetTransform;
                    Debug.Log("else changed transform.parent.position");
                    Debug.Log("cycling players");
                }
               
            }

           
            

            
        }

        public IEnumerator CoFade()
        {
            float timer = 0f;
            //Transform myTitanTransform = GameManager.instance.GetTitan(m_playerController.Team).cameraPosition;
            
            // First fade in (player -> titan)
            while (timer < fadeInTime)
            {
                yield return null;
                timer += Time.deltaTime;
                if(killCamFadePanel) killCamFadePanel.color = Color.Lerp(Color.clear, Color.black, timer / fadeInTime);
            }
            
            // Get titan
            if (killCamFadePanel) killCamFadePanel.color = Color.black;
            SetTargetTransform();
                      

            // Set starting local position
            if (transform.localPosition.sqrMagnitude > 0.001f)
            {
                transform.localPosition = new Vector3(startXpos, startYpos, startZpos);
            }
            transform.localRotation = Quaternion.identity;

            // Enable secondary camera
            if (myCamera)
            {
                m_playerController.mainCamera.gameObject.SetActive(false);
                myCamera.enabled = true;
            }

            // First fade out (player -> titan)
            timer = 0f;
            while (timer < fadeOutTime)
            {
                yield return null;
                timer += Time.deltaTime;
                if (killCamFadePanel) killCamFadePanel.color = Color.Lerp(Color.black, Color.clear, timer / fadeOutTime);
                MoveTitanCamera();
            }
            if (killCamFadePanel) killCamFadePanel.color = Color.clear;

            // Titan camera view
            timer = 0f;
            bool endPhase = GameManager.instance.GetTitan(m_playerController.Team).HasCompletedPath;            
            float respawnTime = endPhase ? m_playerController.m_endPhaseRespawnTime : m_playerController.m_respawnTime;

            while (timer < (respawnTime - 2f * fadeInTime - fadeOutTime))
            {
                yield return null;
                timer += Time.deltaTime;
                MoveTitanCamera();
            }

            // Second fade in (titan -> player)
            timer = 0f;
            while (timer < fadeInTime)
            {
                yield return null;
                timer += Time.deltaTime;
                if (killCamFadePanel) killCamFadePanel.color = Color.Lerp(Color.clear, Color.black, timer / fadeInTime);

            }

            // Reset camera on player
            transform.parent = m_player.transform;
            if (myCamera)
            {
                myCamera.enabled = false;
                m_playerController.mainCamera.gameObject.SetActive(true);
            }

            // Second fade out (titan -> player)
            timer = 0f;
            while (timer < fadeOutTime)
            {
                yield return null;
                timer += Time.deltaTime;
                if (killCamFadePanel) killCamFadePanel.color = Color.Lerp(Color.black, Color.clear, timer / fadeOutTime);

            }

            m_playerController.secondaryCamera.gameObject.SetActive(false);
        }

        IEnumerator CoWaitLocalPlayer()
        {
            yield return new WaitUntil(() => GamePlayer.local != null && GamePlayer.local.LobbyPlayer != null);

            if (m_playerController == null)
            {
                m_playerController = m_player.GetComponent<GamePlayer>();

                if (!m_playerController.isLocalPlayer)
                {
                    Destroy(gameObject);
                    yield break;
                }
            }

            myCamera = GetComponent<Camera>();
            if (myCamera) myCamera.enabled = false;

            killCamFadePanel = UIGame.instance.deathCamPanel;

            StartCoroutine("CoFade");

        }

        void MoveTitanCamera()
        {
            // distance = maxDistance;

            RaycastHit hit;
            if (Physics.Raycast(targetTransform.position, transform.position - targetTransform.position , out hit, maxDistance, walkableLayerMask))
            {   //distance = hit.distance;
                distance = Mathf.MoveTowards(distance, hit.distance, Time.deltaTime * collisionSpeedCamera);
            }
            else
            {
                distance = Mathf.MoveTowards(distance, maxDistance, Time.deltaTime * collisionSpeedCamera);
            }
            
            

            invertX = invertXAxis ? -1f : 1f;
            invertY = invertYAxis ? -1f : 1f;

            float verticalAngle = 0f;
            float horizontalAngle = 0f;

            Vector3 pos = transform.localPosition;

            if (Input.GetAxis("Mouse X") > 0f)
            {
                horizontalAngle += Time.deltaTime * speedCamera * invertX;
            }

            if (Input.GetAxis("Mouse X") < 0f)
            {
                horizontalAngle -= Time.deltaTime * speedCamera * invertX;
            }

            if (Input.GetAxis("Mouse Y") > 0f)
            {
                verticalAngle += Time.deltaTime * speedCamera * invertY;
            }

            if (Input.GetAxis("Mouse Y") < 0f)
            {
                verticalAngle -= Time.deltaTime * speedCamera * invertY;
            }

            if(Input.GetMouseButtonDown(0))
            {
                SetTargetTransform(1);
            }
            else if (Input.GetMouseButtonDown(1))
            {
                SetTargetTransform(-1);
            }


            //rotazione

            pos = Quaternion.Euler(0f, horizontalAngle, 0f) * pos;
            //pos = Quaternion.AngleAxis(verticalAngle,transform.parent.right) * pos;
            pos = pos.normalized * distance;
            transform.localPosition = pos;
            //transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles.x + horizontalAngle, transform.localRotation.eulerAngles.y + verticalAngle, 5f);

            transform.LookAt(transform.parent.position);
            //transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles.x + verticalAngle, transform.localRotation.eulerAngles.y , 0f);
        }

    }
}