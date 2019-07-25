using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace VoK
{
    public class TeamBase : NetBehaviour
    {
        [Header("Kor circle base settings")]
        public int numberOfKor;
        public GameObject korPrefab;
        public GameObject korCircle;
        public float diameter;
        public float korRotationXSpeed = 0f;
        public float korRotationYSpeed = 10f;
        public float korRotationZSpeed = 0f;


        List<KorFinalAttack> korModels;
        [Header("Base settings")]
        [Tooltip("Conquest speed")]
        public float m_conquestSpeed = 0.1f;

        [Header("Base state")]
        [Tooltip("Conquest progress")]
        public float m_conquestCompletition = 0f;
        public float Progress { get { return m_conquestCompletition / m_maxConquestCompletition; } }
        [Tooltip("Max conquest progress to reach")]
        public float m_maxConquestCompletition = 100f;

        [Header("Base references - to assign")]
        public Team m_team;
        public Team Team { get { return m_team; } }
        [ColorUsageAttribute(true, true)] public Color m_circle_allyColor;
        [ColorUsageAttribute(true, true)] public Color m_circle_enemyColor;

        public Renderer circleRenderer;
        GameTitan m_titan;
        private List<GamePlayer> playersInArea;
        bool m_rightTeamInside;
        [HideInInspector]
        public bool isRoundOver;
        public bool IsActivated { get { return (m_titan != null ? m_titan.HasCompletedPath : false); } }
        bool syncTitanCollider = false;
        float startColliderRadius;

        private void Awake()
        {
            playersInArea = new List<GamePlayer>();
        }

        void Start()
        {
            m_rightTeamInside = false;
            m_conquestCompletition = 0f;
            m_titan = GameManager.instance.GetTitan(m_team);
            isRoundOver = false;
            korModels = new List<KorFinalAttack>();
            syncTitanCollider = false;
            for (int i = 0; i < numberOfKor; i++)
            {
                var angle = i * Mathf.PI * 2 / numberOfKor;
                var pos = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * diameter/2;
                var k = Instantiate(korPrefab, korCircle.transform).GetComponent<KorFinalAttack>();
                //Debug.Log(angle + " " + i);
                k.transform.Rotate(0,270 - 360/numberOfKor * i, 0);
                k.transform.localPosition = pos;
                korModels.Add(k);
                k.mytitan = m_titan;
                k.kor.gameObject.SetActive(false);
                k.line.gameObject.SetActive(false);
            }
            startColliderRadius = m_titan.GetComponent<CapsuleCollider>().radius;
            if (isServer)
            {
                GameManager.instance.OnRoundStart += CleanUpBase;
            }
            
        }
        
        void Update()
        {           
            if(isServer && !isRoundOver)
            {
               if(m_titan != null && m_titan.HasCompletedPath)
                {
                    AddProgress();                    
                }
            }
            circleRenderer.material.SetFloat("_Level", m_conquestCompletition/m_maxConquestCompletition);
            
            if(m_titan.HasCompletedPath)
            {
                if(!syncTitanCollider)
                {
                    StartCoroutine(CoWaitLocalPlayer());
                    m_titan.GetComponent<CapsuleCollider>().radius = GetComponent<CapsuleCollider>().radius / m_titan.transform.localScale.x;
                    //GetComponentInChildren<MeshRenderer>().material.color = (m_team != GamePlayer.local.Team ? GameManager.instance.enemyColor : GameManager.instance.allyColor);
                    GameManager.instance.SetMapSoundtrack(2f);
                    syncTitanCollider = true;
                }
                for (int i = 0; i < korModels.Count; i++)
                {
                    if (m_conquestCompletition> m_maxConquestCompletition/numberOfKor*i)
                    {
                        if (Progress >= 1f || (i < Progress * (numberOfKor - 1) && !korModels[i].line.gameObject.activeSelf))
                        {
                            korModels[i].kor.gameObject.SetActive(true);
                            korModels[i].line.gameObject.SetActive(true);
                            korModels[i].activate();
                        }
                        korModels[i].kor.transform.Rotate(korRotationXSpeed * Time.deltaTime, korRotationYSpeed * Time.deltaTime, korRotationZSpeed * Time.deltaTime);
                    }
                }
            }      
        }

        public void AddProgress()
        {
            if (!isRoundOver)
            {
                m_conquestCompletition = m_titan.m_final_attack_energy;
                NetRpcSendMessage(NetMsg.UpdateBase, m_conquestCompletition.ToString("F2"));
                if (Progress >= 1f)
                {
                    foreach (TeamBase t in GameManager.instance.teamBases)
                    {
                        t.isRoundOver = true;
                    }
                    GameManager.instance.UpdateRoundScore(m_team);
                }
            }
        }

        public void CleanUpBase()
        {
            Debug.Log(name + " resetted for round");
            m_conquestCompletition = 0f;
            NetRpcSendMessage(NetMsg.UpdateBase, m_conquestCompletition.ToString("F2"));
            isRoundOver = false;
            syncTitanCollider = false;
            StopAllCoroutines();
            m_titan.GetComponent<CapsuleCollider>().radius = startColliderRadius;
            for (int i = 0; i < korModels.Count; i++)
            {
                if (korModels[i].kor.gameObject.activeSelf)
                {
                    korModels[i].kor.gameObject.SetActive(false);
                    korModels[i].line.gameObject.SetActive(false);
                }
            }
            NetRpcSendMessage(NetMsg.SetReady);
        }

        public bool CheckTeamInside()
        {
            if (playersInArea.Count == 0) return false;
            foreach (GamePlayer player in playersInArea)
            {
                if (player.Team != m_team)
                {
                    return false;
                }
            }
            return true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Player")
            {
                GamePlayer player = other.gameObject.GetComponent<GamePlayer>();
                if (player != null)
                {
                    if (!player.IsDead)
                    {
                        playersInArea.Add(player);
                        m_rightTeamInside = CheckTeamInside();
                        if (isServer)
                        {
                            player.OnPlayerDeath += RemovePlayer;
                        }
                    }
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.tag == "Player")
            {
                GamePlayer player = other.gameObject.GetComponent<GamePlayer>();
                if (player != null)
                {
                    RemovePlayer(player);
                }
            }
        }

        public void RemovePlayer(GamePlayer player)
        {
            if (playersInArea.Contains(player))
            {
                playersInArea.Remove(player);
                m_rightTeamInside = CheckTeamInside();
                if (isServer)
                {
                    player.OnPlayerDeath -= RemovePlayer;
                }
            }
        }

        public override bool CheckMessage(string msg, NetMsgType netMsgType)
        {
            string[] msgArray = DeserializeMessage(msg);
            NetMsg msgIndex = (NetMsg)System.Convert.ToInt32(msgArray[0]);
            switch (msgIndex)
            {
                case NetMsg.SetReady:
                case NetMsg.UpdateBase:
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
                case NetMsg.SetReady:
                {
                    if (netMsgType == NetMsgType.Rpc)
                    {
                        if (!isServer)
                        {
                            m_conquestCompletition = 0f;
                            isRoundOver = false;
                            syncTitanCollider = false;
                            StopAllCoroutines();
                            m_titan.GetComponent<CapsuleCollider>().radius = startColliderRadius;
                            for (int i = 0; i < korModels.Count; i++)
                            {
                                if (korModels[i].kor.gameObject.activeSelf)
                                {
                                    korModels[i].kor.gameObject.SetActive(false);
                                    korModels[i].line.gameObject.SetActive(false);
                                }
                            }
                        }
                    }
                    break;
                }
                case NetMsg.UpdateBase:
                {
                    if (netMsgType == NetMsgType.Rpc)
                    {
                        m_conquestCompletition = System.Convert.ToSingle(msgArray[1]);
                    }
                    break;
                }
            }
        }

        IEnumerator CoWaitLocalPlayer()
        {
            yield return new WaitUntil(() => GamePlayer.local != null && GamePlayer.local.LobbyPlayer != null);
            circleRenderer.material.SetColor("_Color", (m_team == GamePlayer.local.Team ? m_circle_allyColor : m_circle_enemyColor ));
        }

        public void OnDisable()
        {
            StopAllCoroutines();
        }
      
    }
}