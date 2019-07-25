using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using VoK;

namespace Mirror
{
    public class PlayerStats 
    {
        public int kills;
        public int deaths;
        public int assists;
        public float Ratio { get { return (deaths == 0 ? kills * 10f : (float)kills / deaths); } }
        public int Eliminations { get { return kills + assists; } }
        public float energyDelivered;
        public int streak;
        public int maxStreak;
        public int generatorsConquered;
        public int generatorsNeutralized;
        public float generatorTime;
        public int headshots;
        public int totalHeadshots;
        public float damageDealt;
        public float healingDealt;
        public float damageReceived;
        public float healReceived;


        public PlayerStats()
        {
            kills = 0;
            assists = 0;
            deaths = 0;

            energyDelivered = 0f;
            streak = 0;
            maxStreak = 0;
            headshots = 0;
            totalHeadshots = 0;

            generatorsConquered = 0;
            generatorsNeutralized = 0;
            generatorTime = 0f;

            damageDealt = 0f;
            healingDealt = 0f;
            damageReceived = 0f;
            healReceived = 0f;
        }

        public void SummStats(PlayerStats b)
        {
            kills += b.kills;
            assists +=b.assists;
            deaths += b.deaths;

            energyDelivered +=b.energyDelivered;
            streak +=b.streak;

            if(maxStreak<b.maxStreak)
            {
                maxStreak = b.maxStreak;
            }
            
            headshots +=b.headshots;
            totalHeadshots +=b.totalHeadshots;

            generatorsConquered +=b.generatorsConquered;
            generatorsNeutralized +=b.generatorsNeutralized;
            generatorTime += b.generatorTime;

            damageDealt += b.damageDealt;
            healingDealt += b.healingDealt;
            damageReceived +=b.damageReceived;
            healReceived += b.healReceived;

            
        }

        public string SerializeAll()
        {
            string s = NetBehaviour.SerializeMessage((int)PlayerInfo.Kills, kills) + NetBehaviour.NET_MSG_CHAR;
            s += NetBehaviour.SerializeMessage((int)PlayerInfo.Deaths, deaths) + NetBehaviour.NET_MSG_CHAR;
            s += NetBehaviour.SerializeMessage((int)PlayerInfo.Assists, assists) + NetBehaviour.NET_MSG_CHAR;

            s += NetBehaviour.SerializeMessage((int)PlayerInfo.EnergyDelivered, energyDelivered) + NetBehaviour.NET_MSG_CHAR;
            s += NetBehaviour.SerializeMessage((int)PlayerInfo.Streak, streak) + NetBehaviour.NET_MSG_CHAR;
            s += NetBehaviour.SerializeMessage((int)PlayerInfo.MaxStreak, maxStreak) + NetBehaviour.NET_MSG_CHAR;
            s += NetBehaviour.SerializeMessage((int)PlayerInfo.Headshots, headshots) + NetBehaviour.NET_MSG_CHAR;
            s += NetBehaviour.SerializeMessage((int)PlayerInfo.TotalHeadshots, totalHeadshots) + NetBehaviour.NET_MSG_CHAR;

            s += NetBehaviour.SerializeMessage((int)PlayerInfo.GeneratorConquered, generatorsConquered) + NetBehaviour.NET_MSG_CHAR;
            s += NetBehaviour.SerializeMessage((int)PlayerInfo.GeneratorNeutralized, generatorsNeutralized) + NetBehaviour.NET_MSG_CHAR;
            s += NetBehaviour.SerializeMessage((int)PlayerInfo.GeneratorTime, generatorTime) + NetBehaviour.NET_MSG_CHAR;

            s += NetBehaviour.SerializeMessage((int)PlayerInfo.DamageDealt, damageDealt) + NetBehaviour.NET_MSG_CHAR;
            s += NetBehaviour.SerializeMessage((int)PlayerInfo.HealDealt, healingDealt) + NetBehaviour.NET_MSG_CHAR;
            s += NetBehaviour.SerializeMessage((int)PlayerInfo.DamageReceived, damageReceived) + NetBehaviour.NET_MSG_CHAR;
            s += NetBehaviour.SerializeMessage((int)PlayerInfo.HealReceived, healReceived);
            //Debug.Log(s);
            return s;
        }


    }

    public class NetLobbyPlayer : NetBehaviour
    {
        // Constants
        public static readonly int PLAYER_STATS = System.Enum.GetValues(typeof(PlayerInfo)).Length;
        public static readonly int MAX_PLAYER_NAME_CHARS = 16;

        [HideInInspector]
        public static NetLobbyPlayer local;

        [Header("Lobby Player Stats")]
        [Tooltip("Player nickname")]
        public string playerName;
        [Tooltip("Player team")]
        public Team team;
        [Tooltip("Player seeker")]
        public Seeker seeker;
        [Tooltip("Player k/d/a/en stats")]
        //public uint[] stats;
        public PlayerStats stats;
        public PlayerStats totalStats;
        #region Unused
        /*
        public bool ShowLobbyGUI = true;
        //*/
        #endregion


        [Tooltip("Player ready state")]
        public bool readyToBegin;

        [Tooltip("Player identifier")]
        public int id;

        GameObject m_myLobbyObject;
        public GameObject LobbyObject
        {
            get { return m_myLobbyObject; }
        }
        /// <summary>
        /// Do not use Start - Override OnStartrHost / OnStartClient instead!
        /// </summary>
        public void Start()
        {
            // Server initialization of player stats
            if (isLocalPlayer)
            {
                local = this;
            }
            if (isServer)
            {
                InitializePlayerStats();
            }
            if (isClient) SceneManager.sceneLoaded += ClientLoadedScene;            
            OnClientEnterLobby();
        }

        public void UpdateLobbyPlayer()
        {
            UIManager.instance?.UpdateLobbyPlayer(this);
        }

        void OnDisable()
        {
            SceneManager.sceneLoaded -= ClientLoadedScene;
            Destroy(m_myLobbyObject);
        }

        public virtual void ClientLoadedScene(Scene arg0, LoadSceneMode arg1)
        {
            NetLobbyManager lobby = NetLobbyManager.singleton;
            if (lobby != null && SceneManager.GetActiveScene().name == lobby.LobbyScene)
                return;

            if (this != null && isLocalPlayer)
                CmdSendLevelLoaded();
        }

        // External accessors / Command / Rpc

        #region Index
            /*
        public void SetIndex(int index)
        {
            CmdSetIndex(index);
        }

        [Command]
        public void CmdSetIndex(int index)
        {
            RpcSetIndex(index);
        }

        [ClientRpc]
        public void RpcSetIndex(int index)
        {
            id = index;
        }*/
        #endregion

        #region Ready State
        /*
        public void SetReadyState(bool state)
        {
            CmdChangeReadyState(state);
        }

        [Command]
        public void CmdChangeReadyState(bool ReadyState)
        {
            if (!IsPlayerSet()) return;
            readyToBegin = ReadyState;
            NetLobbyManager.singleton?.ReadyStatusChanged();
            RpcChangeReadyState(ReadyState);
        }

        [ClientRpc]
        public void RpcChangeReadyState(bool readyState)
        {
            readyToBegin = readyState;
            UpdateLobbyPlayer();
        }
        */
        #endregion

        #region Player Name
        /*
        public void SetName(string newName)
        {
            CmdChangeName(newName);
        }

        [Command]
        public void CmdChangeName(string newName)
        {
            if(newName.Length > MAX_PLAYER_NAME_CHARS)
                playerName = newName.Substring(0, MAX_PLAYER_NAME_CHARS);
            else
                playerName = newName;
            RpcChangeName(playerName);
        }

        [ClientRpc]
        public void RpcChangeName(string newName)
        {
            playerName = newName;
            UpdateLobbyPlayer();
        }
            */
        #endregion

        #region Player Team
            /*
        public void SetTeam(int t)
        {
            CmdSetTeam(t);
        }

        [Command]
        public void CmdSetTeam(int t)
        {
            // Ignore commands from ready players
            if (readyToBegin) return;

            // Assign the team and send to client the info
            team = (Team)t;
            RpcSetTeam(t);
        }

        [ClientRpc]
        public void RpcSetTeam(int t)
        {
            team = (Team)t;
            UpdateLobbyPlayer();
        }
        */
        #endregion

        #region Player Seeker
        /*
        public void SetSeeker(uint seekerId)
        {
            CmdSetSeeker(seekerId);
        }

        [Command]
        public void CmdSetSeeker(uint id)
        {
            // Ignore commands from ready players
            if (readyToBegin) return;

            // Ignore request to take an already chosen seeker
            if (IsSeekerAlreadyChosen(id))
            {
                //CmdSetTeam((int)Team.None);
                return;
            }

            // Assign the seeker and send to client the info
            seeker = Seeker.FindSeeker(id);           
            RpcSetSeeker(id);
        }

        [ClientRpc]
        public void RpcSetSeeker(uint id)
        {
            seeker = Seeker.FindSeeker(id);
            UpdateLobbyPlayer();
        }*/
        #endregion


        #region Network Messaging
                               

        // Check if a message is allowed
        public override bool CheckMessage(string msg, NetMsgType netMsgType)
        {
            string[] msgArray = DeserializeMessage(msg);
            NetMsg msgIndex = (NetMsg)System.Convert.ToInt32(msgArray[0]);
            switch (msgIndex)
            {
                case NetMsg.SetName:
                {
                    if(netMsgType == NetMsgType.Cmd)
                    {
                        if (readyToBegin) return false;
                        if (msgArray[1].Length > MAX_PLAYER_NAME_CHARS)/* || msgArray[1].Length == 0)*/ return false;
                    }
                    return true;
                }
                case NetMsg.SetReady:
                {
                    if (netMsgType == NetMsgType.Cmd)
                    {
                        if (!IsPlayerSet()) return false;
                    }

                    return true;
                }
                case NetMsg.SetSeeker:
                {
                    if (netMsgType == NetMsgType.Cmd)
                    {
                        if (readyToBegin) return false;
                        if (IsSeekerAlreadyChosen(System.Convert.ToUInt32(msgArray[1]), (Team)System.Convert.ToInt32(msgArray[2])))
                        {
                            Debug.Log("Already chosen");
                            return false;
                        }
                    }
                    return true;
                }
                case NetMsg.SetTeam:
                {
                    if (netMsgType == NetMsgType.Cmd)
                    {
                        if (readyToBegin) return false;
                    }
                    return true;
                }
                case NetMsg.SetIndex:
                {
                    // Anti index set by clients
                    if (netMsgType == NetMsgType.Cmd)
                    {
                        return false;
                    }
                    return true;
                }
                case NetMsg.SetStats:
                {
                    // Anti index set by clients
                    if (netMsgType == NetMsgType.Cmd)
                    {
                        return false;
                    }
                    return true;
                }
            }
            return false;
        }

        // == Messages handling
        public override void ExecuteMessage(string msg, NetMsgType netMsgType)
        {
            string[] msgArray = DeserializeMessage(msg);
            NetMsg msgIndex = (NetMsg)System.Convert.ToInt32(msgArray[0]);

            switch (msgIndex)
            {
                case NetMsg.SetName:
                {
                    if (netMsgType == NetMsgType.Cmd)
                    {
                        playerName = msgArray[1];                        
                        RpcSendMessage(msg);
                    }
                    if (netMsgType == NetMsgType.Rpc)
                    {
                        playerName = msgArray[1];
                        UpdateLobbyPlayer();
                    }
                    break;
                }
                case NetMsg.SetReady:
                {
                    if (netMsgType == NetMsgType.Cmd)
                    {
                        readyToBegin = (Convert.ToInt32(msgArray[1]) == 1 ? true : false);
                        NetLobbyManager.singleton?.ReadyStatusChanged();
                        if (playerName.Length == 0)
                        {
                            if (UIManager.instance)
                            {
                                playerName = string.Format(UIManager.instance.textManager.emptyNameAutoFill, UnityEngine.Random.Range(100, 1000)); // "SEEKER" + UnityEngine.Random.Range(100, 1000);
                            }
                            else
                            {
                                playerName = "GUEST" + UnityEngine.Random.Range(100, 1000);
                            }
                        }
                        if (readyToBegin && this == local)
                        {
                            SeekerSelectionFeedback();
                        }
                        NetRpcSendMessage(NetMsg.SetName, playerName);
                        RpcSendMessage(msg);
                    }
                    if (netMsgType == NetMsgType.Rpc)
                    {
                        bool newReadyState = (Convert.ToInt32(msgArray[1]) == 1 ? true : false);
                        if (!readyToBegin && this == local && newReadyState)
                        {
                            SeekerSelectionFeedback();
                        }
                        readyToBegin = newReadyState;                        
                        
                        UpdateLobbyPlayer();
                    }
                    break;
                }
                case NetMsg.SetSeeker:
                {
                    if (netMsgType == NetMsgType.Cmd)
                    {
                        seeker = Seeker.FindSeeker(Convert.ToUInt32(msgArray[1]));
                        team = (Team)Convert.ToInt32(msgArray[2]);
                        RpcSendMessage(msg);
                    }
                    if (netMsgType == NetMsgType.Rpc)
                    {
                        seeker = Seeker.FindSeeker(Convert.ToUInt32(msgArray[1]));
                        team = (Team)Convert.ToInt32(msgArray[2]);
                        UpdateLobbyPlayer();
                    }
                    break;
                }
                case NetMsg.SetTeam:
                {
                    if (netMsgType == NetMsgType.Cmd)
                    {
                        team = (Team)Convert.ToInt32(msgArray[1]);
                        RpcSendMessage(msg);
                    }
                    if (netMsgType == NetMsgType.Rpc)
                    {
                        team = (Team)Convert.ToInt32(msgArray[1]);
                        UpdateLobbyPlayer();
                    }
                    break;
                }
                case NetMsg.SetIndex:
                {
                    if (netMsgType == NetMsgType.Rpc)
                    {
                        id = Convert.ToInt32(msgArray[1]);
                    }
                    break;
                }
                case NetMsg.SetStats:
                {
                    if (netMsgType == NetMsgType.Rpc)
                    {
                        if (stats == null)
                        {
                            stats = new PlayerStats();
                        }
                        for(int i = 1; i < msgArray.Length - 1; i=i+2)
                        {
                             switch((PlayerInfo) Convert.ToInt32(msgArray[i]))
                            {
                                case PlayerInfo.Kills:
                                {
                                    stats.kills = Convert.ToInt32(msgArray[i + 1]);
                                    break;
                                }
                                case PlayerInfo.Deaths:
                                {
                                    stats.deaths = Convert.ToInt32(msgArray[i + 1]);
                                    break;
                                }
                                case PlayerInfo.Assists:
                                {
                                    stats.assists = Convert.ToInt32(msgArray[i + 1]);
                                    break;
                                }
                                case PlayerInfo.GeneratorConquered:
                                {
                                    stats.generatorsConquered = Convert.ToInt32(msgArray[i + 1]);
                                    break;
                                }
                                case PlayerInfo.GeneratorNeutralized:
                                {
                                    stats.generatorsNeutralized = Convert.ToInt32(msgArray[i + 1]);
                                    break;
                                }
                                case PlayerInfo.Headshots:
                                {
                                    stats.headshots = Convert.ToInt32(msgArray[i + 1]);
                                    break;
                                }
                                case PlayerInfo.TotalHeadshots:
                                {
                                    stats.totalHeadshots = Convert.ToInt32(msgArray[i + 1]);
                                    break;
                                }
                                case PlayerInfo.Streak:
                                {
                                    stats.streak = Convert.ToInt32(msgArray[i + 1]);
                                    break;
                                }
                                case PlayerInfo.MaxStreak:
                                {
                                    stats.maxStreak = Convert.ToInt32(msgArray[i + 1]);
                                    break;
                                }
                                case PlayerInfo.EnergyDelivered:
                                {
                                    stats.energyDelivered = Convert.ToSingle(msgArray[i + 1]);
                                    break;
                                }
                                case PlayerInfo.GeneratorTime:
                                {
                                    stats.generatorTime = Convert.ToSingle(msgArray[i + 1]);
                                    break;
                                }
                                case PlayerInfo.DamageDealt:
                                {
                                    stats.damageDealt = Convert.ToSingle(msgArray[i + 1]);
                                    break;
                                }
                                case PlayerInfo.DamageReceived:
                                {
                                    stats.damageReceived = Convert.ToSingle(msgArray[i + 1]);
                                    break;
                                }
                                case PlayerInfo.HealDealt:
                                {
                                    stats.healingDealt = Convert.ToSingle(msgArray[i + 1]);
                                    break;
                                }
                                case PlayerInfo.HealReceived:
                                {
                                    stats.healReceived = Convert.ToSingle(msgArray[i + 1]);
                                    break;
                                }
                            }

                        }
                    }
                    break;
                }
            }
        }

        #endregion

        
        // Rpcs
        public void SyncPlayer()
        {
            NetRpcSendMessage(NetMsg.SetIndex, id.ToString());
            NetRpcSendMessage(NetMsg.SetName, playerName);
            NetRpcSendMessage(NetMsg.SetSeeker, (seeker != null ? seeker.id : 0).ToString(), ((int)team)); // with team
            NetRpcSendMessage(NetMsg.SetReady, (Convert.ToInt32(readyToBegin)).ToString());
            NetRpcSendMessage(NetMsg.SetStats, stats.SerializeAll());
        }

        public void SeekerSelectionFeedback()
        {
            if (team != Team.None)
            {
                if (team == Team.A)
                {
                    UIManager.instance.teamAModels.AnimateModel(seeker.seekerId);
                }
                else
                {
                    UIManager.instance.teamBModels.AnimateModel(seeker.seekerId);
                }
            }
            FMODUnity.StudioEventEmitter seekerSelection = UIManager.instance.seekerSelection;
            seekerSelection.Play();
            seekerSelection.SetParameter("Button", 2);

            switch (local.seeker.seekerId)
            {
                case SeekerList.Vayvin:
                seekerSelection.SetParameter("Character", 2);
                break;
                case SeekerList.Ogertha:
                seekerSelection.SetParameter("Character", 0);
                break;
                case SeekerList.Soris:
                seekerSelection.SetParameter("Character", 1);
                break;
            }
        }
        
        public static NetLobbyPlayer Find(int index)
        {
            foreach (NetLobbyPlayer nlp in NetLobbyManager.singleton.netLobbyPlayers)
            {
                if(nlp.id == index)
                {
                    return nlp;
                }
            }
            return null;
        }
        bool IsPlayerSet()
        {
            return seeker != null && /*!string.IsNullOrEmpty(playerName) &&*/ team != Team.None;
        }
        
        bool IsSeekerAlreadyChosen(uint seekerId, Team myTeam)
        {
            // Check every lobby player
            foreach (NetLobbyPlayer nlp in NetLobbyManager.singleton.netLobbyPlayers)
            {
                // If player has a valid seeker
                if(nlp.seeker != null)
                {
                    // Check if there's already the same seeker in the same team
                    if (nlp.seeker.id == seekerId && nlp.team == myTeam)
                        return true;
                }
            }
            return false;
        }

        [Command]
        public void CmdSendLevelLoaded()
        {
            NetLobbyManager lobby = NetLobbyManager.singleton;
            lobby?.PlayerLoadedScene(GetComponent<NetworkIdentity>().connectionToClient);
        }


        #region lobby client virtuals

        public virtual void OnClientReady(bool readyState)
        {
            Debug.Log(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        #endregion

        #region optional UI

        /* // OLD GUI
        public virtual void OnGUI()
        {
            if (!ShowLobbyGUI)
                return;

            NetLobbyManager lobby = NetLobbyManager.singleton;
            if (lobby)
            {
                if (!lobby.showLobbyGUI)
                    return;

                if (SceneManager.GetActiveScene().name != lobby.LobbyScene)
                    return;

                GUILayout.BeginArea(new Rect(20f + (Index * 100), 200f, 90f, 130f));

                GUILayout.Label($"Player [{Index + 1}]");

                if (ReadyToBegin)
                    GUILayout.Label("Ready");
                else
                    GUILayout.Label("Not Ready");

                if (isServer && Index > 0 && GUILayout.Button("REMOVE"))
                {
                    // This button only shows on the Host for all players other than the Host
                    // Host and Players can't remove themselves (stop the client instead)
                    // Host can kick a Player this way.
                    GetComponent<NetworkIdentity>().connectionToClient.Disconnect();
                }

                GUILayout.EndArea();

                if (NetworkClient.active && isLocalPlayer)
                {
                    GUILayout.BeginArea(new Rect(20f, 300f, 120f, 20f));

                    if (ReadyToBegin)
                    {
                        if (GUILayout.Button("Cancel"))
                            CmdChangeReadyState(false);
                    }
                    else
                    {
                        if (GUILayout.Button("Ready"))
                            CmdChangeReadyState(true);
                    }

                    GUILayout.EndArea();
                }
            }
        }//*/

        #endregion
        public override void OnStartClient()
        {
            if (LogFilter.Debug) Debug.LogFormat("OnStartClient {0}", SceneManager.GetActiveScene().name);

            base.OnStartClient();
            NetLobbyManager lobby = NetLobbyManager.singleton;

            /*
                This demonstrates how to set the parent of the LobbyPlayerPrefab to an arbitrary scene object
                A similar technique would be used if a full canvas layout UI existed and we wanted to show
                something more visual for each player in that layout, such as a name, avatar, etc.

                Note: LobbyPlayer prefab will be marked DontDestroyOnLoad and carried forward to the game scene.
                      Because of this, NetworkLobbyManager must automatically set the parent to null
                      in ServerChangeScene and OnClientChangeScene.
            */

            if (lobby != null && SceneManager.GetActiveScene().name == lobby.LobbyScene)
                gameObject.transform.SetParent(NetLobbyManager.singleton.lobbyPlayers.transform);
        }

        public void OnClientEnterLobby()
        {
            if (LogFilter.Debug) Debug.LogFormat("OnClientEnterLobby {0}", SceneManager.GetActiveScene().name);
            
            // Add ui lobby player
            m_myLobbyObject = UIManager.instance.AddLobbyPlayer(isLocalPlayer);         
            

            // When a client enter lobby, server sends rpcs info to every client
            if (isServer)
            {
                foreach (NetLobbyPlayer nlp in NetLobbyManager.singleton.netLobbyPlayers)
                {
                    nlp.SyncPlayer();
                }
            }

        }

        public void OnClientExitLobby()
        {
            if (LogFilter.Debug) Debug.LogFormat("OnClientExitLobby {0}", SceneManager.GetActiveScene().name);
        }

        public void InitializePlayerStats()
        {
            playerName = "";//"GUEST" + UnityEngine.Random.Range(10000, 100000);
            readyToBegin = false;
            team = Team.None;
            seeker = null;
            stats = new PlayerStats();
            totalStats = new PlayerStats();
            //stats = new uint[PLAYER_STATS];
            //for(int i = 0; i < PLAYER_STATS; i++)
            //{
            //    stats[i] = 0;
            //}
        }

        //public float[] GetStats()
        //{
        //    float[] statistics = new float[9];
        //    statistics[0] = stats[(int)PlayerInfo.Kills];
        //    statistics[1] = stats[(int)PlayerInfo.Deaths];
        //    statistics[2] = stats[(int)PlayerInfo.EnergyDelivered];
        //    statistics[5] = stats[(int)PlayerInfo.DamageDealt];
        //    statistics[6] = stats[(int)PlayerInfo.HealDealt];
        //    statistics[3] = ((float)stats[(int)PlayerInfo.Kills] / (float)(stats[(int)PlayerInfo.Deaths] == 0 ? 1 : stats[(int)PlayerInfo.Deaths]));
        //    statistics[4] = stats[(int)PlayerInfo.MaxStreak];
        //    statistics[7] = stats[(int)PlayerInfo.GeneratorConquered];
        //    statistics[8] = stats[(int)PlayerInfo.GeneratorNeutralized];


        //    return statistics;
        //}
        

        // These accessors are used for ingame/end match stats infos
        #region Stats Accessors
        public int Kills
        {
            get { return stats.kills; }
            set { stats.kills = value; }
        }
        public int Deaths
        {
            get { return stats.deaths; }
            set { stats.deaths = value; }
        }
        public int Assists
        {
            get { return stats.assists; }
            set { stats.assists = value; }
        }
        public int Eliminations { get { return stats.Eliminations; } }
        public float KDRatio { get { return stats.Ratio; } }
        public float EnergyDelivered
        {
            get { return stats.energyDelivered; }
            set { stats.energyDelivered = value; }
        }
        public float DamageDealt
        {
            get { return stats.damageDealt; }
            set { stats.damageDealt = value; }
        }

        public float DamageReceived
        {
            get { return stats.damageReceived; }
            set { stats.damageReceived = value; }
        }

        public float HealingDealt
        {
            get { return stats.healingDealt; }
            set { stats.healingDealt = value; }
        }

        public float HealingReceived
        {
            get { return stats.healReceived; }
            set { stats.healReceived = value; }
        }

        public int GeneratorConquered
        {
            get { return stats.generatorsConquered; }
            set { stats.generatorsConquered = value; }
        }

        public int GeneratorNeutralized
        {
            get { return stats.generatorsNeutralized; }
            set { stats.generatorsNeutralized = value; }
        }

        public float GeneratorTime
        {
            get { return stats.generatorTime; }
            set { stats.generatorTime = value; }
        }

        public int Headshots
        {
            get { return stats.headshots; }
            set { stats.headshots = value; }
        }
        public int TotalHeadshots
        {
            get { return stats.totalHeadshots; }
            set { stats.totalHeadshots = value; }
        }
        public int Streak
        {
            get { return stats.streak; }
            set { stats.streak = value; }
        }
        public int MaxStreak
        {
            get { return stats.maxStreak; }
            set { stats.maxStreak = value; }
        }

        #endregion
    }
}
