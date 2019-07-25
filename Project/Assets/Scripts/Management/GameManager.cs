using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using FMODUnity;

namespace VoK
{
    #region Enums

    // Team identific ation
    public enum Team
    {
        None = 0,
        A = 1,
        B = -1
    }

    // Type of ability
    public enum AbilityType
    {
        None = -1,
        Base,
        Movement,
        First,
        Second,
        Ultimate,
        Passive,
        FirstSecondary,
        SecondSecondary,
        MobilitySecondary,
        UltimateSecondary
    }

    // Type Of Damage
    public enum HitType
    {
        Hit,
        Heal,
        HeadShot
    }
    
    // Game Player statistic types
    public enum StatType
    {
        Health,
        SpeedMult,
        RespawnTime,
        Shield,
        Energy
    }

    // Game player animation states
    public enum PlayerState
    {
        Idle,
        Walking,
        Dead
    }

    // Lobby player match informations
    public enum PlayerInfo
    {
        Kills,
        Deaths,        
        EnergyDelivered,
        DamageDealt,
        KillDeathRatio,
        MaxStreak,
        GeneratorConquered,
        GeneratorNeutralized,
        GeneratorTime,
        Assists,
        DamageReceived,
        HealDealt,
        HealReceived,
        Streak,
        Headshots,
        TotalHeadshots
       

    }

    public enum EffectCondition
    {
        None,
        PlayerHit,
        EnvironmentHit,
        Kill,
        HealthLevel
    }

    public enum InputKey
    {
        Vertical,
        Horizontal,
        Jump,
        Dash,
        BaseShoot,
        FirstShoot,
        SecondShoot,
        UltimateShoot,
        Pause,
        RecapAbility,
        Any
    }

    public enum SeekerList
    {
        None = 0,
        Vayvin = 1,
        Ogertha = 2,
        Soris = 3
    }

    public enum ItemType
    {
        Energy = 0,
        Medikit = 1
    }

    public enum AttackSpawnPoint
    {
        RightHand,
        LeftHand,
        BodyCenter,
        Head,
        Hips,
        None
    }

    public enum GamePlayerState
    {
        CanInput,
        CanInputMouse,
        SpeedMult,
        Gravity,
        Physics,
        Ammo,
        BodyRotation,
        CanAttack,
        Position,
        NoClipTime,
        Grounded,
        CanUseMobility
    }

    public enum TargetType
    {
        Nobody,
        Me,
        MyTeam,
        EnemyTeam,
        EveryOne,
        EveryOneButMe
    }

    public enum SoundEmitterType
    {
        Walk,
        Energy
    }

    public enum SoundType
    {
        Soundtrack,
        Effects
    }

    #endregion

    #region Interfaces
    public interface IWinCondition
    {
        bool CheckCondition();
    }
    #endregion

    // This class handles a match
    public class GameManager : NetBehaviour
    {
        public static GameManager instance;

        public int RoundNumber { get { return teamARounds + teamBRounds + 1; } }
        public int maxRounds;
        public int teamARounds;
        public int teamBRounds;
        public float matchTime = 0f;
        [SerializeField]
        float matchTimeSyncInterval = 1f;
        public float deathZoneLimit = 500f;
        public float endRoundTime = 7f;
        public bool isRoundOver = false;
        //public event Action<GamePlayer,GamePlayer> OnPlayerDeath;
        //public event Action OnGameOver;
        //public event Action<Team> OnGeneratorTaken;
        //public event Action OnEndPhaseStarted;
        //public event Action OnTitanMoved;

        public event Action OnRoundStart;

        [Header("GamePlay entities")]
        public Generator[] generators;
        public GameTitan[] gameTitans;
        //public ItemSpawner[] itemSpawners;
        public TeamBase[] teamBases;
        public Camera mainMapCamera;
        public List<BaseSpawnManager> spawnBases;
        public GameObject postProcessing;
        public StudioEventEmitter mapSurroundSound;

        public Color allyColor = Color.blue;
        public Color enemyColor = Color.red;

        [HideInInspector]
        public bool hasMatchStarted;

        [EventRef]
        public string hitSound;
        public string hitParameter;

        private void Awake()
        {
            hasMatchStarted = false;
            instance = this;
            maxRounds = 0;
            teamARounds = 0;
            teamBRounds = 0;
            isRoundOver = false;
        }

        // Start is called before the first frame update
        void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            if (isServer)
            {
                StartCoroutine(CoSyncMatchTime());
                maxRounds = MenuManager.Instance.roundNumber;
                NetRpcSendMessage(NetMsg.SetIndex, maxRounds);//uso SetIndex per sincronizzare maxRounds
            }
            hasMatchStarted = true;

            
        }
        
        public void UpdateRoundScore(Team winner)
        {
            if(winner == Team.A)
            {
                teamARounds++;
            }
            else if (winner == Team.B)
            {
                teamBRounds++;
            }
            SyncManager();
            if(IsGameOver())
            {
                foreach(GamePlayer p in NetLobbyManager.singleton.loadedPlayers)
                {
                    p.LobbyPlayer.stats.SummStats(p.LobbyPlayer.totalStats);
                    p.SyncPlayerStats();
                }
                
            }
            float cinematicDuration = GetTitan(winner).GetComponent<EndGameCamera>().AnimDuration;
            if (winner != Team.None)
            {
                NetRpcSendMessage(NetMsg.Update, (int)winner, IsGameOver());
                StartCoroutine(CoRestartRound(IsGameOver(), cinematicDuration + endRoundTime));
            }
        }

        public void SetMapSoundtrack(float parameter, bool restart = false)
        {
            if (restart)
            {
                mapSurroundSound.Play();
            }
            mapSurroundSound.SetParameter("GamePhase", parameter);
            //mapSurroundSound.Play();
        }

        public void SyncManager()
        {
            NetRpcSendMessage(NetMsg.SetStats, maxRounds, teamARounds, teamBRounds);

        }

        IEnumerator CoRestartRound(bool gameOver, float totalEndRoundTime)
        {
            yield return new WaitForSeconds(totalEndRoundTime);
            if (!gameOver)
            {
                OnRoundStart();
                matchTime = 0f;
            }
        }

        public bool IsGameOver()
        {
            if (teamARounds > maxRounds / 2) return true;
            if (teamBRounds > maxRounds / 2) return true;
            return false;
        }

        public override bool CheckMessage(string msg, NetMsgType netMsgType)
        {
            string[] msgArray = DeserializeMessage(msg);
            NetMsg msgIndex = (NetMsg)System.Convert.ToInt32(msgArray[0]);
            switch (msgIndex)
            {
                case NetMsg.Update:
                case NetMsg.UpdateGenerator:
                case NetMsg.Die:
                case NetMsg.Hit:
                case NetMsg.ScreenMsg:
                case NetMsg.UpdateBase:
                case NetMsg.Time:
                case NetMsg.SetReady:
                case NetMsg.Kill:
                case NetMsg.PlayersInArea:
                case NetMsg.SpawnPS:
                case NetMsg.PlaySound:
                case NetMsg.SetIndex:
                case NetMsg.SetStats:
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
                case NetMsg.UpdateGenerator:
                {
                    if (netMsgType == NetMsgType.Rpc)
                    {
                        UIGame.instance.GeneratorTaken((Team)Convert.ToInt32(msgArray[1]));

                    }
                    break;
                }
                case NetMsg.PlaySound:
                {
                    if (netMsgType == NetMsgType.Rpc)
                    {
                        //SetMapSoundtrack(Convert.ToSingle(msgArray[1]));
                        if (msgArray.Length > 2)
                        {
                            SetMapSoundtrack(Convert.ToSingle(msgArray[1]), true);
                        }
                        else
                        {
                            SetMapSoundtrack(Convert.ToSingle(msgArray[1]));
                        }
                    }
                    break;
                }
                case NetMsg.Die:
                {
                    if (netMsgType == NetMsgType.Rpc)
                    {
                        GamePlayer killer = GamePlayer.Find(Convert.ToInt32(msgArray[2]));
                        UIGame.instance.PlayerDeath(GamePlayer.Find(Convert.ToInt32(msgArray[1])), killer);
                        if (killer == GamePlayer.local)
                        {
                            UIGame.instance.PlayerKill();
                        }
                    }
                    break;
                }
                case NetMsg.Hit:
                {
                    if (netMsgType == NetMsgType.Rpc)
                    {
                        GamePlayer hitted = GamePlayer.Find(Convert.ToInt32(msgArray[1]));
                        GamePlayer hitter = GamePlayer.Find(Convert.ToInt32(msgArray[2]));
                        float Damage = Convert.ToSingle(msgArray[3]);
                        float hitParameterValue = Convert.ToSingle(msgArray[8]);
                        //bool isDamage = Damage > 0f ? true : false;
                        if (hitter == GamePlayer.local)
                        {                          
                            UIGame.instance.PlayerHit((HitType)Convert.ToInt32(msgArray[7]), hitted, Damage);
                            if (hitParameterValue >= 0f)
                            {
                                GamePlayer.local.PlaySound(hitSound, SerializeMessage(hitParameter, hitParameterValue));
                            }
                        }
                        if (hitted == GamePlayer.local)
                        {
                            int hitTypeId = Convert.ToInt32(msgArray[7]);
                            if (hitTypeId == (int)HitType.Hit || hitTypeId == (int)HitType.HeadShot)
                            { 
                                hitted.SpawnDamageInput(Damage,new Vector3(Convert.ToSingle(msgArray[4]),  Convert.ToSingle(msgArray[5]), Convert.ToSingle(msgArray[6])));
                            }
                        }
                        if(Damage > 0f && hitted.Team != GamePlayer.local.Team)
                        {
                            hitted.SpawnHp();
                        }
                    }
                    break;
                }
                case NetMsg.SpawnPS:
                {
                    if (netMsgType == NetMsgType.Rpc)
                    {
                        //string particleName = msgArray[1];
                        Vector3 spawnedPos = new Vector3(Convert.ToSingle(msgArray[2]), Convert.ToSingle(msgArray[3]), Convert.ToSingle(msgArray[4]));
                        Vector3 spawnedRotVector = new Vector3(Convert.ToSingle(msgArray[5]), Convert.ToSingle(msgArray[6]), Convert.ToSingle(msgArray[7]));
                        Quaternion spawnedRot = (spawnedRotVector == Vector3.zero ? Quaternion.identity : Quaternion.LookRotation(spawnedRotVector));// Quaternion.Euler(Convert.ToSingle(msgArray[5]), Convert.ToSingle(msgArray[6]), Convert.ToSingle(msgArray[7]));
                        GameObject psToSpawn = MenuManager.Instance.FindParticleEffect(msgArray[1]);
                        if (psToSpawn != null)
                        {
                            GameObject newPS = Instantiate(psToSpawn);
                            Destroy(newPS, 10f);
                            newPS.transform.position = spawnedPos;
                            newPS.transform.rotation = spawnedRot;
                            if (newPS.GetComponent<ParticleSystem>() != null) newPS.GetComponent<ParticleSystem>().Play();
                            foreach (ParticleSystem ps in newPS.GetComponentsInChildren<ParticleSystem>())
                            {
                                ps.Play();
                            }
                        }
                    }
                    break;
                }
                case NetMsg.ScreenMsg:
                {
                    if (netMsgType == NetMsgType.Rpc || netMsgType == NetMsgType.Target)
                    {
                        if (!isRoundOver)
                        {
                            if (msgArray.Length <= 2)
                            {
                                UIGame.instance.AddScreenMessage(UIGame.instance.textManager.FindText(msgArray[1]));
                            }
                            else
                            {
                                ScreenText t = Instantiate(UIGame.instance.textManager.FindText(msgArray[1]));
                                t.content = string.Format(t.content, msgArray[2]);
                                UIGame.instance.AddScreenMessage(t);
                                Destroy(t);
                            }
                        }
                    }
                    break;
                }
                case NetMsg.UpdateBase:
                {
                    if (netMsgType == NetMsgType.Rpc)
                    {
                        UIGame.instance.BaseConquestStart((Team)Convert.ToInt32(msgArray[1]));
                    }
                    break;
                }
                case NetMsg.SetReady:
                {
                    if (netMsgType == NetMsgType.Rpc)
                    {
                        isRoundOver = false;                        
                    }
                    break;
                }
                case NetMsg.Update:
                {
                    if (netMsgType == NetMsgType.Rpc)
                    {
                        if (!isRoundOver)
                        {
                            //UIGame.instance.InstaCleanScreenMessage();
                            Team winner = (Team)Convert.ToInt32(msgArray[1]);
                            UIGame.instance.WinMessage(winner);
                            GamePlayer.local.m_canInput = false;
                            GamePlayer.local.CanRotateVisual = false;
                            bool matchEnd = Convert.ToBoolean(msgArray[2]);
                            UIGame.instance.EnableEndgame(winner, matchEnd);
                            isRoundOver = true;
                        }
                    }
                    break;
                }
                case NetMsg.Time:
                {
                    if (netMsgType == NetMsgType.Rpc)
                    {
                        matchTime = Convert.ToSingle(msgArray[1]);
                    }
                    break;
                }
                case NetMsg.Kill:
                {
                    if (netMsgType == NetMsgType.Target)
                    {
                        //UIGame.instance.AddMultiKillMessage(UIGame.instance.textManager.FindText(msgArray[1]));
                    }
                    break;
                }
                case NetMsg.PlayersInArea:
                {
                    if (netMsgType == NetMsgType.Target)
                    {
                        UIGame.instance.canGiveEnergy = Convert.ToBoolean(msgArray[1]);
                    }
                    break;
                }
                case NetMsg.SetIndex:
                {
                    maxRounds = Convert.ToInt32(msgArray[1]);
                    break;
                }
                case NetMsg.SetStats:
                {
                    if (netMsgType == NetMsgType.Rpc)
                    {
                        maxRounds = Convert.ToInt32(msgArray[1]);
                        teamARounds = Convert.ToInt32(msgArray[2]);
                        teamBRounds = Convert.ToInt32(msgArray[3]);
                        Debug.Log("Team A: " + teamARounds + " Team B: " + teamBRounds + " Round: " + (teamARounds + teamBRounds) + "/" + maxRounds);
                    }
                    break;
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            if(Input.GetKeyDown(KeyCode.F1))
            {
                Cursor.lockState = (Cursor.lockState == CursorLockMode.None ? CursorLockMode.Locked : CursorLockMode.None);
                Cursor.visible = !Cursor.visible;
            }/*
            if (Input.GetKeyDown(KeyCode.F2))
            {
                if(postProcessing)
                {
                    if(postProcessing.activeSelf)
                    {
                        postProcessing.SetActive(false);
                    }
                    else
                    {
                        postProcessing.SetActive(true);
                    }
                }
            }*/
            if (UIGame.instance && !isRoundOver)
            {
                UIGame.instance.showStats = Input.GetKey(KeyCode.Tab);
            }
            if(!isRoundOver)
            {
                matchTime += Time.deltaTime;
            }
        }

        public GameTitan GetTitan(Team team)
        {
            foreach(GameTitan g in gameTitans)
            {
                if(g.Team == team)
                {
                    return g;
                }
            }
            return null;
        }

        public void StartEndPhase(Team team)
        {
            foreach (GamePlayer player in GamePlayer.allPlayers)
            {
                if (team == player.Team)
                {
                    NetTargetSendMessage(NetMsg.ScreenMsg, player.connectionToClient, UIGame.instance.textManager.allyFinalAttackMsg);
                }
                else
                {
                    NetTargetSendMessage(NetMsg.ScreenMsg, player.connectionToClient, UIGame.instance.textManager.enemyFinalAttackMsg);
                }
            }
        }

        public void OnDisable()
        {
            StopAllCoroutines();
        }

        // This static function is generic for any void function to call
        public static IEnumerator WaitCondition(System.Func<bool> condition, System.Action<object[]> doNext, object[] parameters = null)
        {
            for (; ; )
            {
                try
                {
                    if (condition())
                    {
                        doNext(parameters);
                        yield break;
                    }
                }
                catch
                {
                    yield break;
                }
                yield return null;
            }
        }
        public static IEnumerator DoWaitDo(System.Action doBefore, float waitTime, System.Action doNext)
        {
            doBefore();
            yield return new WaitForSeconds(waitTime);
            doNext();
        }

        IEnumerator CoSyncMatchTime()
        {
            while(true)
            {
                yield return new WaitForSeconds(matchTimeSyncInterval);
                if (matchTimeSyncInterval == 0) yield return null;
                NetRpcSendMessage(NetMsg.Time, matchTime.ToString("F0"));
            }
        }

    }
}
