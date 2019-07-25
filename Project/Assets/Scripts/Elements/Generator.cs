using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using FMODUnity;

namespace VoK
{
    public class Generator : NetBehaviour
    {
        [Header("Generator settings")]
        [Tooltip("Time interval between energy sending to titan")]
        public float m_deliverTime = 1f;
        [Tooltip("Amount of energy delivered to titan each tick")]
        public float m_deliveredEnergy = 1f;
        [Tooltip("Conquest speed")]
        public float m_conquestSpeed = 1f;
        [Tooltip("If true, generator will start sending energy to titan at successfull conquest. If false, Deliver Time will pass before first energy sending to titan.")]
        public bool m_startDeliveringOnConquest = false;
        public Color m_baseColor = Color.grey;
        [ColorUsageAttribute(true, true)] public Color m_circle_allyColor;
        [ColorUsageAttribute(true, true)] public Color m_circle_enemyColor;
        public GameObject neutral_glow_prefab;
        public GameObject enemy_glow_prefab;
        public GameObject ally_glow_prefab;
        public GameObject explosion_ally_prefab;
        public GameObject explosion_enemy_prefab;
        public Transform explosionPosition;
        [Header("Generator state")]
        [Tooltip("Conquest progression value")]
        public float m_conquestValue;
        public float Progress { get { return m_conquestValue; } }
        [Tooltip("Team owner of this generator")]
        public Team m_teamOwner;
        public Team Team { get { return m_teamOwner; } }
        [Tooltip("Team inside the generator - None if both teams are inside")]
        public Team m_teamInside;

        public Renderer circleRenderer;
        public MeshRenderer generatorKorEye;
        private GameTitan m_titan;
        public List<GamePlayer> playersInArea;
        private Coroutine deliveringCo;
        public Transform actualGlowTransform;
        private GameObject actualGlow;

        private Coroutine resetGenerator;
        [Tooltip("The generator returns to the last owner conquest value after this time, if abandoned")]
        public float m_resetTime = 5f;
        public float m_soundConqueringParam = 0f;
        public float m_soundNeutralizeParam = 1f;
        public float m_soundConqueredParam = 2f;

        StudioEventEmitter generatorEmitter;

        private void Start()
        {
            actualGlow = Instantiate(neutral_glow_prefab, actualGlowTransform);
            actualGlow.name = neutral_glow_prefab.name;
            m_teamOwner = Team.None;
            m_teamInside = Team.None;
            m_conquestValue = 0f;
            deliveringCo = null;
            resetGenerator = null;
            playersInArea = new List<GamePlayer>();
            circleRenderer.material.SetFloat("_Level", 0f);
            //m_baseColor = GetComponentInChildren<MeshRenderer>().material.color;
            generatorEmitter = GetComponent<StudioEventEmitter>();
            SetColor(m_baseColor);
            GameManager.instance.OnRoundStart += CleanUpGenerator;
        }

        private void Update()
        {
           
            if (isServer)
            {
                if(playersInArea.Count > 0)
                {
                    if (!generatorEmitter.IsPlaying())
                    {
                        SendSound(0f, false);
                    }
                    if (resetGenerator != null)
                    {
                        StopCoroutine(resetGenerator);
                        resetGenerator = null;
                    }
                    // Coquest only when there's only one team inside and it's not the owner
                    if (m_teamInside != Team.None && m_teamOwner != m_teamInside)
                    {
                        // Increase conquest value according to number of players inside
                        m_conquestValue += (float)m_teamInside * playersInArea.Count * m_conquestSpeed * Time.deltaTime;
                        m_conquestValue = Mathf.Clamp(m_conquestValue, -1f, 1f);

                        // Neutrality
                        if (m_teamOwner != Team.None && Mathf.Sign(m_conquestValue) != Mathf.Sign((float)m_teamOwner))
                        {
                            m_teamOwner = Team.None;
                            GameManager.instance.NetRpcSendMessage(NetMsg.UpdateGenerator, (int)m_teamOwner);
                            SendSound(1f, false);
                            foreach (GamePlayer player in playersInArea)
                            {
                                player.LobbyPlayer.GeneratorNeutralized++;
                                UpdatePlayerStats(player);
                            }
                            if (deliveringCo != null)
                            {
                                StopCoroutine(deliveringCo);
                                deliveringCo = null;
                            }
                        }

                        // Successful conquest
                        if (m_conquestValue == (float)m_teamInside)
                        {
                            m_teamOwner = m_teamInside;
                            GameManager.instance.NetRpcSendMessage(NetMsg.UpdateGenerator, (int)m_teamOwner);
                            SendSound(2f, false);
                            if (deliveringCo != null)
                            {
                                StopCoroutine(deliveringCo);
                            }
                            foreach (GamePlayer player in playersInArea)
                            {
                                player.LobbyPlayer.GeneratorConquered++;
                                UpdatePlayerStats(player);
                            }
                            m_titan = GameManager.instance.GetTitan(m_teamOwner);
                            deliveringCo = StartCoroutine(CoGenerateEnergy());
                        }
                        SyncGenerator();
                    }
                    else if (m_teamOwner == m_teamInside && m_conquestValue != (float)m_teamOwner && resetGenerator == null)
                    {
                        resetGenerator = StartCoroutine(CoResetGenerator());
                        SyncGenerator();
                    }

                }
                else
                {
                    if (m_conquestValue != (float)m_teamOwner && resetGenerator == null)
                    {
                        SendSound(0f, true);
                        resetGenerator = StartCoroutine(CoResetGenerator());
                        SyncGenerator();                       
                    }
                }
            }
            circleRenderer.material.SetFloat("_Level", Mathf.Abs(m_conquestValue));
        }

        public void SyncGenerator()
        {            
            NetRpcSendMessage(NetMsg.UpdateGenerator, m_conquestValue.ToString("F2"), (int)m_teamOwner,(int)m_teamInside);
        }

        public void UpdatePlayerStats(GamePlayer player)
        {
            player.SyncPlayerStats();
            //player.NetRpcSendMessage(NetMsg.SetStats, player.LobbyPlayer.stats.SerializeAll());
        }

        public bool IsPlayerInside(GamePlayer player)
        {
            if (playersInArea == null) return false;
            return playersInArea.Contains(player);
        }

        public void SetColor(Color color, bool circleOnly = true)
        {
            //circleRenderer.material.color = m_baseColor;//GetComponentInChildren<MeshRenderer>().material.color = GameManager.instance.allyColor;
            circleRenderer.material.color = color;
            circleRenderer.material.SetColor("_Color", color);
            if(!circleOnly)
            {
                generatorKorEye.materials[0].SetColor("_Color", color);
                generatorKorEye.materials[0].SetColor("_EmissionColor", color);
                generatorKorEye.materials[1].SetColor("_TintColor", color);
            }
           
        }

        public void CleanUpGenerator()
        {
            Debug.Log(name + " resetted for round");
            StopAllCoroutines();
            actualGlow.name = neutral_glow_prefab.name;
            m_teamOwner = Team.None;
            m_teamInside = Team.None;
            m_conquestValue = 0f;
            deliveringCo = null;
            resetGenerator = null;
            playersInArea = new List<GamePlayer>();
            circleRenderer.material.SetFloat("_Level", 0f);
            SetColor(m_baseColor);
            SyncGenerator();
            //m_baseColor = circleRenderer.material.color;//GetComponentInChildren<MeshRenderer>().material.color;
        }

        private void OnTriggerEnter(Collider other)
        {
            if(other.tag == "Player")
            {
                GamePlayer player = other.gameObject.GetComponent<GamePlayer>();
                if (player != null)
                {
                    if (!player.IsDead)
                    {
                        playersInArea.Add(player);
                        m_teamInside = CheckTeamInside();
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
                m_teamInside = CheckTeamInside();
                if (isServer)
                {
                    player.OnPlayerDeath -= RemovePlayer;
                }
            }
        }
        
        public Team CheckTeamInside()
        {
            bool foundTeamA = false;
            bool foundTeamB = false;
            foreach(GamePlayer player in playersInArea)
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

        public void SendSound(float parameter, bool stop = false)
        {
            NetRpcSendMessage(NetMsg.PlaySound, parameter, stop);
        }

        public override bool CheckMessage(string msg, NetMsgType netMsgType)
        {
            string[] msgArray = DeserializeMessage(msg);
            NetMsg msgIndex = (NetMsg)System.Convert.ToInt32(msgArray[0]);
            switch (msgIndex)
            {
                case NetMsg.PlaySound:
                case NetMsg.UpdateGenerator:
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
                case NetMsg.PlaySound:
                {
                    float param = System.Convert.ToSingle(msgArray[1]);
                    bool stop = System.Convert.ToBoolean(msgArray[2]);
                    if(stop)
                    {
                        generatorEmitter.Stop();
                    }
                    else
                    {
                        if (!generatorEmitter.IsPlaying()) generatorEmitter.Play();
                        generatorEmitter.SetParameter("GeneratorConquering", param);
                    }
                    break;
                }
                case NetMsg.UpdateGenerator:
                {
                    if (netMsgType == NetMsgType.Rpc)
                    {
                        if (!isServer)
                        {
                            m_conquestValue = System.Convert.ToSingle(msgArray[1]);
                            m_teamOwner = (Team)System.Convert.ToInt32(msgArray[2]);
                            m_teamInside = (Team)System.Convert.ToInt32(msgArray[3]);
                        }
                        if(m_teamOwner == GamePlayer.local.Team)
                        {
                            if(m_conquestValue == (float)m_teamOwner)
                            {
                               Instantiate(explosion_ally_prefab, explosionPosition.position, explosionPosition.rotation);
                            }
                            if (actualGlow.name != ally_glow_prefab.name)
                            {
                                Destroy(actualGlow);
                                actualGlow = Instantiate(ally_glow_prefab, actualGlowTransform);
                                actualGlow.name = ally_glow_prefab.name;
                                SetColor(m_circle_allyColor, false);
                            }
                            //circleRenderer.material.color = GameManager.instance.allyColor;//GetComponentInChildren<MeshRenderer>().material.color = GameManager.instance.allyColor;
                            //circleRenderer.material.SetColor("_Color", m_circle_allyColor);
                        }
                        else if(m_teamOwner == Team.None)
                        {
                            if (actualGlow.name != neutral_glow_prefab.name)
                            {
                                Destroy(actualGlow);
                                actualGlow = Instantiate(neutral_glow_prefab, actualGlowTransform);
                                actualGlow.name = neutral_glow_prefab.name;
                                SetColor(m_baseColor, false);
                            }
                            if (m_teamInside != Team.None)
                            {
                                if (m_teamInside == GamePlayer.local.LobbyPlayer.team)
                                {
                                    //circleRenderer.material.SetColor("_Color", m_circle_allyColor);
                                    SetColor(m_circle_allyColor);
                                }
                                else
                                {
                                    //circleRenderer.material.SetColor("_Color", m_circle_enemyColor);
                                    SetColor(m_circle_enemyColor);
                                }
                            }
                        }
                        else
                        {                          
                            if (actualGlow.name != enemy_glow_prefab.name)
                            {
                                Destroy(actualGlow);
                                actualGlow = Instantiate(enemy_glow_prefab, actualGlowTransform);
                                actualGlow.name = enemy_glow_prefab.name;
                                Instantiate(explosion_enemy_prefab, explosionPosition.position, explosionPosition.rotation);
                                SetColor(m_circle_enemyColor, false);
                            }
                            //circleRenderer.material.color = GameManager.instance.enemyColor; //GetComponentInChildren<MeshRenderer>().material.color = GameManager.instance.enemyColor;
                            //circleRenderer.material.SetColor("_Color", m_circle_enemyColor);
                        }
                    }
                    break;
                }
            }
        }


        IEnumerator CoGenerateEnergy()
        {
            //Debug.Log(name + " starting...");
            if (!m_startDeliveringOnConquest)
            {
                yield return new WaitForSeconds(m_deliverTime);
            }
            while (this.m_teamOwner != Team.None)
            {
                //Debug.Log(name + " energy to " + m_titan.name);
                m_titan.AddEnergy(m_deliveredEnergy);
                yield return new WaitForSeconds(m_deliverTime);
            }
            deliveringCo = null;
        }

        IEnumerator CoResetGenerator()
        {
            // Reset neutralized generator
            if (m_teamOwner == Team.None)
            {
                float startingSign = Mathf.Sign(m_conquestValue);
                while (m_conquestValue * startingSign >= 0f)
                {
                    yield return null;
                    m_conquestValue += -1f * startingSign * m_conquestSpeed * Time.deltaTime;
                    SyncGenerator();
                }
            }
            // Reset owned generator 
            else
            {
                while (Mathf.Abs(m_conquestValue) < Mathf.Abs((float)m_teamOwner))
                {
                    yield return null;
                    m_conquestValue += m_conquestSpeed * Time.deltaTime * (float)m_teamOwner;
                    SyncGenerator();
                }
            }
            
            m_conquestValue = (float)m_teamOwner;
            SyncGenerator();
            resetGenerator = null;
        }
    }
}
