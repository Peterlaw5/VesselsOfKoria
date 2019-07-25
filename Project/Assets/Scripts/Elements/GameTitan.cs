using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

namespace VoK
{
    public class GameTitan : NetBehaviour
    {
        [Header("Titan Editor settings")]
        public PathEditor m_path;
        public Transform titanCentre;
        [Tooltip("The team who owns this titan - to set in Scene")]
        public Team m_team;
        public Team Team { get { return m_team; } }
        public GameObject ally_circlePrefab;
        public GameObject enemy_circlePrefab;
        public Transform circlePosition;
        [Header("Titan Stats settings")]
        [Tooltip("Titan movement speed")]
        public float m_speed = 1f;
        [Tooltip("Titan energy gained for each kill by same team players")]
        public float m_energyPerKill = 1f;
        [Tooltip("Titan energy loss timer")]
        public float m_energyTickTime = 1f;
        [Tooltip("Titan energy loss each Tick time(defined above)")]
        public float m_energyLossPerTick = 1f;
        [Tooltip("Titan energy drain from player time")]
        public float m_energyFromPlayerTickTime = 1f;
        [Tooltip("Titan energy drained from player each Tick time(defined above)")]
        public float m_energyFromPlayerPerTick = 1f;

        [Header("Titan general informations")]
        [Tooltip("Titan energy owned")]
        public float m_energy;
        [Tooltip("Titan final attack energy owned")]
        public float m_final_attack_energy;
        [Tooltip("Titan path completition percentage")]//[SyncVar]
        public float m_pathCompletition;
        public bool HasCompletedPath { get { return m_pathCompletition >= 1f; } }
        public float Progress { get { return m_pathCompletition; } }

        //public TextMeshPro completitionPercentage;
        [Header("Titan Flames References")]
        public List<GameObject> AllyIdleFlames;
        public List<GameObject> EnemyIdleFlames;
        public GameObject AllyWalkFlames;
        public GameObject EnemyWalkFlames;
        public Transform cameraPosition;
        private ParticleSystem[] activeWalkParticles;
        [SerializeField]
        private float m_totalDistance;
        [SerializeField]
        private float m_previousPointDistance = 0f;
        [SerializeField]
        private float m_actualDistance;
        
        [Tooltip("Layer mask to check where to cast the energy delivery circle")]
        public LayerMask m_walkableLayer;
        
        //public float minDistance;
        private int m_actualPointId;
        
        private List<GamePlayer> playerList;
        public bool enemyInTitanArea;
        bool finalPartStarted;
        private NetTransform nt;
        private Coroutine netTransformSync;
        public int ReachingPointID
        {
            get { return m_actualPointId; }
        }
        public Transform ReachingPoint
        {
            get
            {
                return m_path.PathPoints[Mathf.Clamp(m_actualPointId,0,m_path.PathPoints.Count - 1)];
            }
        }

        void Start()
        {
            // Setup titan checks
            AllyWalkFlames.SetActive(false);
            EnemyWalkFlames.SetActive(false);
            foreach(GameObject g in AllyIdleFlames)
            {
                g.SetActive(false);
                //Debug.Log("deactivated flames" + g.name);
            }
            foreach (GameObject g in EnemyIdleFlames)
            {
                g.SetActive(false);
               // Debug.Log("deactivated flames" + g.name);
            }
            
            nt = GetComponent<NetTransform>();
            if (m_team == Team.None)
            {
                Debug.LogError(name + " has no <b>TEAM</b> assigned!");
            }
            if (m_path == null)
            {
                Debug.LogError(name + " has no <b>PATH</b> assigned!");
            }
            finalPartStarted = false;
            // Server initialization
            if (isServer)
            {
                m_totalDistance = m_path.TotalLength();
                Debug.Log(m_team + " " +  name + " will travel " + m_totalDistance + " m");
                m_actualPointId = 0;
                m_energy = 0;
                transform.position = m_path.PathPoints[m_actualPointId].position;
                StartCoroutine(CoEnergyLoss());

                //Create a list of players
                playerList = new List<GamePlayer>();
                GameManager.instance.OnRoundStart += CleanUpTitan;
            }
            StartCoroutine(CoWaitPlayerLoaded());
          
        }

        public void CleanUpTitan()
        {
            Debug.Log(name + " resetted for round");
            m_actualPointId = 0;
            m_energy = 0f;
            m_pathCompletition = 0f;
            m_actualDistance = 0f;
            m_previousPointDistance = 0f;
            m_final_attack_energy = 0f;
            m_path.DisableSpeedLane();
            finalPartStarted = false;
            StopAllCoroutines();
            StartCoroutine(CoEnergyLoss());
            playerList = new List<GamePlayer>();
            NetRpcSendMessage(NetMsg.SetItem);//uso set item per resettare la posizione
            NetRpcSendMessage(NetMsg.Update, m_pathCompletition.ToString("F2"), m_energy.ToString("F2"));
        }

        void Update()
        {
#if UNITY_EDITOR
            if(isServer)
            {
                if(Input.GetKeyDown(KeyCode.F5))
                {
                    m_energy = Random.Range(100f,200f);
                    m_speed = Random.Range(12f, 24f);
                    m_final_attack_energy += 100f;
                }
            }
#endif
            if (activeWalkParticles != null && m_energy <= 0f && activeWalkParticles[0].isPlaying)
            {
                foreach (ParticleSystem p in activeWalkParticles)
                {
                   p.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                }
            }
            if (activeWalkParticles != null && m_energy > 0 && activeWalkParticles[0].isStopped)
            {
                foreach (ParticleSystem p in activeWalkParticles)
                {
                   p.Play(true);
                }
            }
            if (isServer)
            {
                // If titans has energy
                if (m_energy > 0)
                {
                    // When path it's not completed
                    if (m_actualPointId < m_path.PathPoints.Count)
                    {
                        // Calculate actual distance only from second point (index = 1)
                        if (m_actualPointId != 0)
                        {
                            m_actualDistance = Vector3.Distance(transform.position, m_path.PathPoints[m_actualPointId - 1].position) + m_previousPointDistance;
                        }

                        // Move the titan
                        transform.position = Vector3.MoveTowards(transform.position, m_path.PathPoints[m_actualPointId].position, Time.deltaTime * m_speed);

                        // When titan reaches point
                        if (DistanceFromReachingPoint() <= 0f)
                        {
                            // Calculate previous distance only from second point (index = 1)
                            if (m_actualPointId != 0)
                            {
                                m_previousPointDistance += Vector3.Distance(m_path.PathPoints[m_actualPointId - 1].position, ReachingPoint.position);
                                m_actualDistance = m_previousPointDistance;
                            }

                            // Set next point
                            m_actualPointId++;

                            // Rotate according to point reached 
                            StartCoroutine(CoRotate());

                        }
                        m_pathCompletition = Mathf.Clamp01(m_actualDistance / m_totalDistance);
                        NetRpcSendMessage(NetMsg.Update,m_pathCompletition.ToString("F2"), m_energy.ToString("F2"));
                    }
                    else
                    {
                        m_pathCompletition = 1f;
                        NetRpcSendMessage(NetMsg.Update, m_pathCompletition.ToString("F2"), m_energy.ToString("F2"));
                        transform.position = m_path.PathPoints[m_path.PathPoints.Count - 1].position;
                    }
                }
            }

            if (m_pathCompletition >= 1f)
            {
                m_path.EnableSpeedLane();
            }
            if (isServer)
            {
                if (!finalPartStarted && m_pathCompletition >= 1f)
                {
                    GameManager.instance.StartEndPhase(Team);
                    finalPartStarted = true;
                }
            }


            RaycastHit hit;
            if(Physics.Raycast(transform.position, Vector3.down,out hit, 100f, m_walkableLayer))
            {
                circlePosition.position = hit.point + Vector3.up * 0.01f;                
            }          
        }

        //Check players in area
        private void OnTriggerEnter(Collider other)
        {
            if (isServer)
            {
                if (other.tag == "Player")
                {
                    GamePlayer player = other.gameObject.GetComponent<GamePlayer>();
                    if (player != null)
                    {
                        if (!player.IsDead)
                        {
                            playerList.Add(player);
                            enemyInTitanArea = CheckTeamInside() == Team.None;

                            player.OnPlayerDeath += RemovePlayer;
                        }
                    }
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (isServer)
            {
                if (other.tag == "Player")
                {
                    GamePlayer player = other.gameObject.GetComponent<GamePlayer>();
                    if (player != null)
                    {
                        enemyInTitanArea = CheckTeamInside() == Team.None;
                        RemovePlayer(player);
                    }
                }
            }
        }

        public Team CheckTeamInside()
        {
            bool foundTeamA = false;
            bool foundTeamB = false;
            foreach (GamePlayer player in playerList)
            {
                if (player.LobbyPlayer.team == Team.A)
                {
                    foundTeamA = true;
                }
                else if (player.LobbyPlayer.team == Team.B)
                {
                    foundTeamB = true;
                }
                if (foundTeamA && foundTeamB)
                    return Team.None;
            }
            if (foundTeamA && !foundTeamB) return Team.A;
            if (foundTeamB && !foundTeamA) return Team.B;
            return Team.None;
        }

        public void RemovePlayer(GamePlayer player)
        {
            if (playerList.Contains(player))
            {
                playerList.Remove(player);
                enemyInTitanArea = CheckTeamInside() == Team.None;
                player.OnPlayerDeath -= RemovePlayer;                
            }
        }

        public void GetEnergyFromPlayer(GamePlayer player)
        {
            GetEnergyFromPlayer(player, m_energyFromPlayerPerTick);
        }

        public void GetEnergyFromPlayer(GamePlayer player, float amount)
        {
            if (GameManager.instance.isRoundOver) return;
            amount = Mathf.Clamp(amount , 0f, player.Energy);
            AddEnergy(amount);
            player.LobbyPlayer.EnergyDelivered += amount;//(uint)player.Energy;
            player.SetEnergy(player.Energy - amount);
            player.SyncPlayerStats();
        }

        public override bool CheckMessage(string msg, NetMsgType netMsgType)
        {
            string[] msgArray = DeserializeMessage(msg);
            NetMsg msgIndex = (NetMsg)System.Convert.ToInt32(msgArray[0]);
            switch (msgIndex)
            {
                case NetMsg.SetItem:
                case NetMsg.Update:
                {
                    return true;
                }
            }
            return false;
        }

        public override void ExecuteMessage(string msg, NetMsgType netMsgType)
        {
            string[] msgArray = DeserializeMessage(msg);
            NetMsg msgIndex = (NetMsg)System.Convert.ToInt32(msgArray[0]);
            switch (msgIndex)
            {
                case NetMsg.Update:
                {
                    if (netMsgType == NetMsgType.Rpc)
                    {

                        if (!isServer)
                        {
                            m_pathCompletition = System.Convert.ToSingle(msgArray[1]);
                            m_energy = System.Convert.ToSingle(msgArray[2]);
                        }
                    }
                    break;
                }
                case NetMsg.SetItem:
                {
                        transform.position = m_path.PathPoints[0].position;
                        transform.rotation = m_path.PathPoints[0].rotation;
                        nt.forceTeleport = true;
                        if(netTransformSync!=null)
                        {
                            StopCoroutine(netTransformSync);
                            
                        }
                        //GetComponent<EndGameCamera>().titanAnimator.SetTrigger("reset");
                        netTransformSync = StartCoroutine(CoResetTeleport());
                        
                    break;
                }
            }
        }

        IEnumerator CoResetTeleport()
        {
            yield return new WaitForSeconds(2f);
            nt.forceTeleport = false;
            netTransformSync = null;
        }
        public void SetPath(PathEditor path)
        {
            this.m_path = path;
        }

        public float DistanceFromReachingPoint()
        {
            return Vector3.Distance(ReachingPoint.position, transform.position);
        }


        public void AddEnergy(float quantity)
        {
            if(!HasCompletedPath)
            {
                m_energy += quantity;
            }
            else
            {
                m_final_attack_energy += quantity;
            }
        }

        public void AddEnergyKill()
        {
            if(!HasCompletedPath)
            {
                m_energy += m_energyPerKill;
            }
            else
            {
                m_final_attack_energy += m_energyPerKill;
            }
            
        }

        private IEnumerator CoRotate()
        {
            Vector3 directionToGo = ReachingPoint.position - transform.position;
            Quaternion rotationToGo = (directionToGo == Vector3.zero ? ReachingPoint.rotation : Quaternion.LookRotation(directionToGo));
                
            while (Vector3.Distance(transform.rotation.eulerAngles, rotationToGo.eulerAngles) > 0.1)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, rotationToGo, 0.1f);
                yield return null;
                directionToGo = ReachingPoint.position - transform.position;
                rotationToGo = (directionToGo == Vector3.zero ? ReachingPoint.rotation : Quaternion.LookRotation(directionToGo));
            }
            transform.rotation = rotationToGo;
        }

        private IEnumerator CoEnergyLoss()
        {
            while (m_actualPointId != m_path.PathPoints.Count)
            {
                if (m_energy > 0f)
                {
                    yield return new WaitForSeconds(m_energyTickTime);
                    if (GameManager.instance.isRoundOver) m_energy = 0f;
                    m_energy -= m_energyLossPerTick;
                    if(m_energy < 0f) m_energy = 0f;
                   
                }
                else
                {
                    m_energy = 0f;
                   
                    yield return null;
                }
            }

        }
        IEnumerator CoWaitPlayerLoaded()
        {
            yield return new WaitUntil(() => GamePlayer.local != null && GamePlayer.local.LobbyPlayer != null);
            EndGameCamera end = GetComponent<EndGameCamera>();
            if (m_team == GamePlayer.local.LobbyPlayer.team)
            {
                Instantiate(ally_circlePrefab, circlePosition);
                foreach (GameObject g in AllyIdleFlames)
                {
                    g.SetActive(true);
                }
                AllyWalkFlames.SetActive(true);
                end.realBeam = end.allyBeam;
                end.realGlow = end.allyGlow;
                activeWalkParticles = AllyWalkFlames.GetComponentsInChildren<ParticleSystem>(); 
                
            }
            else
            {
                Instantiate(enemy_circlePrefab, circlePosition);
                foreach (GameObject g in EnemyIdleFlames)
                {
                    g.SetActive(true);
                }
                EnemyWalkFlames.SetActive(true);
                end.realBeam = end.enemyBeam;
                end.realGlow = end.enemyGlow;
                activeWalkParticles = EnemyWalkFlames.GetComponentsInChildren<ParticleSystem>();               
            }
            foreach (ParticleSystem p in activeWalkParticles)
            {
                p.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        public void OnDisable()
        {
            StopAllCoroutines();
        }
    }



}
