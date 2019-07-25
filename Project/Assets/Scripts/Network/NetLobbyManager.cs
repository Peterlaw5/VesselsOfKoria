using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using TMPro;
using VoK;

namespace Mirror
{
    public enum PlayerSpawnType
    {
        Random,
        RoundRobin,
        TeamBased
    }

    public class NetLobbyManager : MonoBehaviour
    {

        // configuration
        [FormerlySerializedAs("m_DontDestroyOnLoad")] public bool dontDestroyOnLoad = true;
        [FormerlySerializedAs("m_RunInBackground")] public bool runInBackground = true;
        public bool startOnHeadless = true;
        [Tooltip("Server Update frequency, per second. Use around 60Hz for fast paced games like Counter-Strike to minimize latency. Use around 30Hz for games like WoW to minimize computations. Use around 1-10Hz for slow paced games like EVE.")]
        public int serverTickRate = 30;
        [FormerlySerializedAs("m_ShowDebugMessages")] public bool showDebugMessages;

        [Scene]
        [FormerlySerializedAs("m_OfflineScene")] public string offlineScene = "";

        [Scene]
        [FormerlySerializedAs("m_OnlineScene")] public string onlineScene = "";

        [Header("Network Info")]
        // transport layer
        [SerializeField] protected Transport transport;
        [FormerlySerializedAs("m_NetworkAddress")] public string networkAddress = "localhost";
        [FormerlySerializedAs("m_MaxConnections")] public int maxConnections = 4;

        [Header("Spawn Info")]
        [FormerlySerializedAs("m_PlayerPrefab")] public GameObject playerPrefab;
        [FormerlySerializedAs("m_AutoCreatePlayer")] public bool autoCreatePlayer = true;
        [FormerlySerializedAs("m_PlayerSpawnMethod")] public PlayerSpawnType playerSpawnMethod;

        [FormerlySerializedAs("m_SpawnPrefabs")]
        public List<GameObject> spawnPrefabs = new List<GameObject>();

        //public static List<Transform> startPositions = new List<Transform>();
        public static List<PlayerSpawnPoint> startPositions = new List<PlayerSpawnPoint>();
        public List<GamePlayer> loadedPlayers = new List<GamePlayer>();


        [NonSerialized]
        public bool clientLoadedScene;

        // only really valid on the server
        public int numPlayers => NetworkServer.connections.Count(kv => kv.Value.playerController != null);

        // runtime data
        // this is used to make sure that all scene changes are initialized by Mirror.
        // Loading a scene manually wont set networkSceneName, so Mirror would still load it again on start.
        public static string networkSceneName = "";
        [NonSerialized]
        public bool isNetworkActive;
        public NetworkClient client;
        static int s_StartPositionIndex;

        public static NetLobbyManager singleton;

        public GameObject loadingUI;
        static AsyncOperation s_LoadingSceneAsync;
        static NetworkConnection s_ClientReadyConnection;

        // this is used to persist network address between scenes.
        static string s_Address;

        [Header("Lobby Management")]
        public GameObject lobbyPlayers;
        public NetLobbyPlayer[] netLobbyPlayers
        {
            get
            {
                return lobbyPlayers.GetComponentsInChildren<NetLobbyPlayer>();
            }
        }


        // virtual so that inheriting classes' Awake() can call base.Awake() too
        public virtual void Awake()
        {
            Debug.Log("Thank you for using Mirror! https://forum.unity.com/threads/mirror-networking-for-unity-aka-hlapi-community-edition.425437/");

            // Set the networkSceneName to prevent a scene reload
            // if client connection to server fails.
            networkSceneName = offlineScene;
            InitializeSingleton();
        }

        // headless mode detection
        public static bool IsHeadless()
        {
            return SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null;
        }

        void InitializeSingleton()
        {
            if (singleton != null && singleton == this)
            {
                return;
            }

            // do this early
            LogFilter.Debug = showDebugMessages;

            if (dontDestroyOnLoad)
            {
                if (singleton != null)
                {
                    Debug.LogWarning("Multiple NetworkManagers detected in the scene. Only one NetworkManager can exist at a time. The duplicate NetworkManager will be destroyed.");
                    Destroy(gameObject);
                    return;
                }
                if (LogFilter.Debug) Debug.Log("NetworkManager created singleton (DontDestroyOnLoad)");
                singleton = this;
                if (Application.isPlaying) DontDestroyOnLoad(gameObject);
            }
            else
            {
                if (LogFilter.Debug) Debug.Log("NetworkManager created singleton (ForScene)");
                singleton = this;
            }

            // set active transport AFTER setting singleton.
            // so only if we didn't destroy ourselves.
            Transport.activeTransport = transport;

            // persistent network address between scene changes
            if (networkAddress != "")
            {
                s_Address = networkAddress;
            }
            else if (s_Address != "")
            {
                networkAddress = s_Address;
            }
        }

        public virtual void Start()
        {
            // headless mode? then start the server
            // can't do this in Awake because Awake is for initialization.
            // some transports might not be ready until Start.
            //
            // (tick rate is applied in StartServer!)
            if (IsHeadless() && startOnHeadless)
            {
                StartServer();
            }
        }

        // NetworkIdentity.UNetStaticUpdate is called from UnityEngine while LLAPI network is active.
        // if we want TCP then we need to call it manually. probably best from NetworkManager, although this means
        // that we can't use NetworkServer/NetworkClient without a NetworkManager invoking Update anymore.
        //
        // virtual so that inheriting classes' LateUpdate() can call base.LateUpdate() too
        public virtual void LateUpdate()
        {
            // call it while the NetworkManager exists.
            // -> we don't only call while Client/Server.Connected, because then we would stop if disconnected and the
            //    NetworkClient wouldn't receive the last Disconnect event, result in all kinds of issues
            NetworkServer.Update();
            NetworkClient.UpdateClient();
            UpdateScene();
        }

        // When pressing Stop in the Editor, Unity keeps threads alive until we
        // press Start again (which might be a Unity bug).
        // Either way, we should disconnect client & server in OnApplicationQuit
        // so they don't keep running until we press Play again.
        // (this is not a problem in builds)
        //
        // virtual so that inheriting classes' OnApplicationQuit() can call base.OnApplicationQuit() too
        public virtual void OnApplicationQuit()
        {
            Transport.activeTransport.Shutdown();
        }

   
        public virtual void RegisterServerMessages()//internal void RegisterServerMessages()
        {
            NetworkServer.RegisterHandler<ConnectMessage>(OnServerConnectInternal);
            NetworkServer.RegisterHandler<DisconnectMessage>(OnServerDisconnectInternal);
            NetworkServer.RegisterHandler<ReadyMessage>(OnServerReadyMessageInternal);
            NetworkServer.RegisterHandler<AddPlayerMessage>(OnServerAddPlayerMessageInternal);
            NetworkServer.RegisterHandler<RemovePlayerMessage>(OnServerRemovePlayerMessageInternal);
            NetworkServer.RegisterHandler<ErrorMessage>(OnServerErrorInternal);
        }

        public bool StartServer()
        {
            InitializeSingleton();

            if (runInBackground)
                Application.runInBackground = true;

            // set a fixed tick rate instead of updating as often as possible
            // * if not in Editor (it doesn't work in the Editor)
            // * if not in Host mode
#if !UNITY_EDITOR
            if (!NetworkClient.active)
            {
                Application.targetFrameRate = serverTickRate;
                Debug.Log("Server Tick Rate set to: " + Application.targetFrameRate + " Hz.");
            }
#endif

            if (!NetworkServer.Listen(maxConnections))
            {
                Debug.LogError("StartServer listen failed.");
                return false;
            }

            // call OnStartServer AFTER Listen, so that NetworkServer.active is
            // true and we can call NetworkServer.Spawn in OnStartServer
            // overrides.
            // (useful for loading & spawning stuff from database etc.)
            //
            // note: there is no risk of someone connecting after Listen() and
            //       before OnStartServer() because this all runs in one thread
            //       and we don't start processing connects until Update.
            OnStartServer();

            // this must be after Listen(), since that registers the default message handlers
            RegisterServerMessages();

            if (LogFilter.Debug) Debug.Log("NetworkManager StartServer");
            isNetworkActive = true;

            // Only change scene if the requested online scene is not blank, and is not already loaded
            string loadedSceneName = SceneManager.GetSceneAt(0).name;
            if (!string.IsNullOrEmpty(onlineScene) && onlineScene != loadedSceneName && onlineScene != offlineScene)
            {
                ServerChangeScene(onlineScene);
            }
            else
            {
                NetworkServer.SpawnObjects();
            }
            return true;
        }

        public string GetCurrentScene()
        {
            return SceneManager.GetSceneAt(0).name;
        }

        internal void RegisterClientMessages(NetworkClient client)
        {
            client.RegisterHandler<ConnectMessage>(OnClientConnectInternal);
            client.RegisterHandler<DisconnectMessage>(OnClientDisconnectInternal);
            client.RegisterHandler<NotReadyMessage>(OnClientNotReadyMessageInternal);
            client.RegisterHandler<ErrorMessage>(OnClientErrorInternal);
            client.RegisterHandler<SceneMessage>(OnClientSceneInternal);

            // Register player prefab
            if (playerPrefab != null)
            {
                ClientScene.RegisterPrefab(playerPrefab);
            }

            // Register spawnable prefabs
            for (int i = 0; i < spawnPrefabs.Count; i++)
            {
                GameObject prefab = spawnPrefabs[i];
                if (prefab != null)
                {
                    ClientScene.RegisterPrefab(prefab);
                }
            }
        }

        public NetworkClient StartClient(string address)
        {
            networkAddress = address;
            return StartClient();
        }

        public string GetMyIP()
        {
            var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return string.Empty;
        }

        public NetworkClient StartClient()
        {
            InitializeSingleton();

            if (runInBackground)
                Application.runInBackground = true;

            isNetworkActive = true;

            client = new NetworkClient();

            RegisterClientMessages(client);

            if (string.IsNullOrEmpty(networkAddress))
            {
                Debug.LogError("Must set the Network Address field in the manager");
                return null;
            }
            if (LogFilter.Debug) Debug.Log("NetworkManager StartClient address:" + networkAddress);

            client.Connect(networkAddress);

            OnStartClient(client);
            s_Address = networkAddress;
            return client;
        }

        public virtual NetworkClient StartHost()
        {
            loadedPlayers = new List<GamePlayer>();
            OnStartHost();
            if (StartServer())
            {
                NetworkClient localClient = ConnectLocalClient();
                OnStartClient(localClient);
                return localClient;
            }
            return null;
        }

        NetworkClient ConnectLocalClient()
        {
            if (LogFilter.Debug) Debug.Log("NetworkManager StartHost");
            networkAddress = "localhost";
            client = ClientScene.ConnectLocalServer();
            RegisterClientMessages(client);
            return client;
        }

        public void StopHost()
        {
            OnStopHost();

            StopServer();
            StopClient();
        }

        public void StopServer()
        {
            if (!NetworkServer.active)
                return;

            OnStopServer();

            if (LogFilter.Debug) Debug.Log("NetworkManager StopServer");
            isNetworkActive = false;
            NetworkServer.Shutdown();
            if (!string.IsNullOrEmpty(offlineScene))
            {
                ServerChangeScene(offlineScene);
            }
            CleanupNetworkIdentities();
        }

        public void StopClient()
        {
            OnStopClient();

            if (LogFilter.Debug) Debug.Log("NetworkManager StopClient");
            isNetworkActive = false;
            if (client != null)
            {
                // only shutdown this client, not ALL clients.
                client.Disconnect();
                client.Shutdown();
                client = null;
            }

            ClientScene.DestroyAllClientObjects();
            if (!string.IsNullOrEmpty(offlineScene))
            {
                ClientChangeScene(offlineScene, false);
            }
            CleanupNetworkIdentities();
        }

      
        void CleanupNetworkIdentities()
        {
            foreach (NetworkIdentity identity in Resources.FindObjectsOfTypeAll<NetworkIdentity>())
            {
                identity.MarkForReset();
            }
        }

        internal void ClientChangeScene(string newSceneName, bool forceReload)
        {
            if (string.IsNullOrEmpty(newSceneName))
            {
                Debug.LogError("ClientChangeScene empty scene name");
                return;
            }

            if (LogFilter.Debug) Debug.Log("ClientChangeScene newSceneName:" + newSceneName + " networkSceneName:" + networkSceneName);

            if (newSceneName == networkSceneName)
            {
                if (!forceReload)
                {
                    FinishLoadScene();
                    return;
                }
            }

            // vis2k: pause message handling while loading scene. otherwise we will process messages and then lose all
            // the state as soon as the load is finishing, causing all kinds of bugs because of missing state.
            // (client may be null after StopClient etc.)
            if (client != null)
            {
                if (LogFilter.Debug) Debug.Log("ClientChangeScene: pausing handlers while scene is loading to avoid data loss after scene was loaded.");
                Transport.activeTransport.enabled = false;
            }

            // Let client prepare for scene change
            OnClientChangeScene(newSceneName);

            s_LoadingSceneAsync = SceneManager.LoadSceneAsync(newSceneName);
            networkSceneName = newSceneName;
        }

       

        void FinishLoadScene()
        {
            // NOTE: this cannot use NetworkClient.allClients[0] - that client may be for a completely different purpose.

            if (client != null)
            {
                // process queued messages that we received while loading the scene
                if (LogFilter.Debug) Debug.Log("FinishLoadScene: resuming handlers after scene was loading.");
                Transport.activeTransport.enabled = true;

                if (s_ClientReadyConnection != null)
                {
                    clientLoadedScene = true;
                    OnClientConnect(s_ClientReadyConnection);
                    s_ClientReadyConnection = null;
                }
            }
            else
            {
                if (LogFilter.Debug) Debug.Log("FinishLoadScene client is null");
            }

            if (NetworkServer.active)
            {
                NetworkServer.SpawnObjects();
                OnServerSceneChanged(networkSceneName);
            }

            if (IsClientConnected() && client != null)
            {
                RegisterClientMessages(client);
                OnClientSceneChanged(client.connection);
            }
        }

        internal static void UpdateScene()
        {
            if (singleton != null && s_LoadingSceneAsync != null)
            {
                if (!singleton.loadingUI.activeSelf)
                    StartLoadingPanel();
                //if(s_LoadingSceneAsync.isDone)
                //{
                //    if (LogFilter.Debug) Debug.Log("ClientChangeScene done readyCon:" + s_ClientReadyConnection);
                //    singleton.FinishLoadScene();
                //    s_LoadingSceneAsync.allowSceneActivation = true;
                //    s_LoadingSceneAsync = null;
                //}
                //else
                //{
                //    if(!singleton.loadingUI.activeSelf)
                //        StartLoadingPanel();
                //}
            }
        }

        static void StartLoadingPanel()
        {
            singleton.StartCoroutine(singleton.CoLoadingScene());
        }

        IEnumerator CoLoadingScene()
        {
            float progress = 0f;
            Debug.Log("Loading...");
            loadingUI.SetActive(true);
            Slider slider = loadingUI.GetComponentInChildren<Slider>();
            TextMeshProUGUI loadingText = loadingUI.GetComponentInChildren<TextMeshProUGUI>();
            string startText = loadingText.text;
            float timer = 0f;
            string startingScene = GetCurrentScene();
            float loadingFake = UnityEngine.Random.Range(0.9f, 0.99f);
            while (!s_LoadingSceneAsync.isDone)
            {
                yield return null;
                progress = s_LoadingSceneAsync.progress / loadingFake; //(s_LoadingSceneAsync.progress < 0.9f ? s_LoadingSceneAsync.progress : Mathf.Lerp(progress, 1f, 0.1f));
                slider.value = progress;
                timer += Time.deltaTime;
                loadingText.text = startText + "(" + (Mathf.Clamp01(progress) * 100f).ToString("F0") + "%)";
            }
            if (LogFilter.Debug) Debug.Log("ClientChangeScene done readyCon:" + s_ClientReadyConnection);
            singleton.FinishLoadScene();
            s_LoadingSceneAsync.allowSceneActivation = true;
            s_LoadingSceneAsync = null;
            if (GameObject.Find("GameManager") == null)
            {
                loadingText.text = startText;
                loadingUI.SetActive(false);
                yield break;
            }
            while (!GameManager.instance.hasMatchStarted)
            {
                yield return null;
                slider.value = Mathf.Lerp(slider.value, 1f, 0.1f);
                loadingText.text = startText + "(100%)";
            }
            Debug.Log("AA1zz");
            loadingText.text = startText;
            loadingUI.SetActive(false);
        }

        // virtual so that inheriting classes' OnDestroy() can call base.OnDestroy() too
        public virtual void OnDestroy()
        {
            if (LogFilter.Debug) Debug.Log("NetworkManager destroyed");
        }

        public static void RegisterStartPosition(Transform start, Team team = Team.None)
        {
            if (LogFilter.Debug) Debug.Log("RegisterStartPosition: (" + start.gameObject.name + ") " + start.position + " for team " + team);
            startPositions.Add(new PlayerSpawnPoint(start, team));
        }
        
        public static void UnRegisterStartPosition(Transform start, Team team = Team.None)
        {
            if (LogFilter.Debug) Debug.Log("UnRegisterStartPosition: (" + start.gameObject.name + ") " + start.position);
            startPositions.Remove(new PlayerSpawnPoint(start, team));
        }

        public bool IsClientConnected()
        {
            return client != null && client.isConnected;
        }

        // this is the only way to clear the singleton, so another instance can be created.
        public static void Shutdown()
        {
            if (singleton == null)
                return;

            startPositions.Clear();
            s_StartPositionIndex = 0;
            s_ClientReadyConnection = null;

            singleton.StopHost();
            singleton = null;
        }

        #region Server Internal Message Handlers
        internal void OnServerConnectInternal(NetworkConnection conn, ConnectMessage connectMsg)
        {
            if (LogFilter.Debug) Debug.Log("NetworkManager.OnServerConnectInternal");

            if (networkSceneName != "" && networkSceneName != offlineScene)
            {
                SceneMessage msg = new SceneMessage(networkSceneName);
                conn.Send(msg);
            }

            OnServerConnect(conn);
        }

        internal void OnServerDisconnectInternal(NetworkConnection conn, DisconnectMessage msg)
        {
            if (LogFilter.Debug) Debug.Log("NetworkManager.OnServerDisconnectInternal");
            OnServerDisconnect(conn);
        }

        internal void OnServerReadyMessageInternal(NetworkConnection conn, ReadyMessage msg)
        {
            if (LogFilter.Debug) Debug.Log("NetworkManager.OnServerReadyMessageInternal");
            OnServerReady(conn);
        }

        internal void OnServerAddPlayerMessageInternal(NetworkConnection conn, AddPlayerMessage msg)
        {
            if (LogFilter.Debug) Debug.Log("NetworkManager.OnServerAddPlayerMessageInternal");

            OnServerAddPlayer(conn, msg);
        }

        internal void OnServerRemovePlayerMessageInternal(NetworkConnection conn, RemovePlayerMessage msg)
        {
            if (LogFilter.Debug) Debug.Log("NetworkManager.OnServerRemovePlayerMessageInternal");

            if (conn.playerController != null)
            {
                OnServerRemovePlayer(conn, conn.playerController);
                conn.RemovePlayerController();
            }
        }

        internal void OnServerErrorInternal(NetworkConnection conn, ErrorMessage msg)
        {
            if (LogFilter.Debug) Debug.Log("NetworkManager.OnServerErrorInternal");
            OnServerError(conn, msg.value);
        }
        #endregion

        #region Client Internal Message Handlers
        internal void OnClientConnectInternal(NetworkConnection conn, ConnectMessage message)
        {
            if (LogFilter.Debug) Debug.Log("NetworkManager.OnClientConnectInternal");

            string loadedSceneName = SceneManager.GetActiveScene().name;
            if (string.IsNullOrEmpty(onlineScene) || onlineScene == offlineScene || loadedSceneName == onlineScene)
            {
                clientLoadedScene = false;
                OnClientConnect(conn);
            }
            else
            {
                // will wait for scene id to come from the server.
                s_ClientReadyConnection = conn;
            }
        }

        internal void OnClientDisconnectInternal(NetworkConnection conn, DisconnectMessage msg)
        {
            if (LogFilter.Debug) Debug.Log("NetworkManager.OnClientDisconnectInternal");
            OnClientDisconnect(conn);
        }

        internal void OnClientNotReadyMessageInternal(NetworkConnection conn, NotReadyMessage msg)
        {
            if (LogFilter.Debug) Debug.Log("NetworkManager.OnClientNotReadyMessageInternal");

            ClientScene.ready = false;
            OnClientNotReady(conn);

            // NOTE: s_ClientReadyConnection is not set here! don't want OnClientConnect to be invoked again after scene changes.
        }

        internal void OnClientErrorInternal(NetworkConnection conn, ErrorMessage msg)
        {
            if (LogFilter.Debug) Debug.Log("NetworkManager:OnClientErrorInternal");
            OnClientError(conn, msg.value);
        }

        internal void OnClientSceneInternal(NetworkConnection conn, SceneMessage msg)
        {
            if (LogFilter.Debug) Debug.Log("NetworkManager.OnClientSceneInternal");

            string newSceneName = msg.value;

            if (IsClientConnected() && !NetworkServer.active)
            {
                ClientChangeScene(newSceneName, true);
            }
        }
        #endregion

        #region Server System Callbacks


        public virtual void OnServerReady(NetworkConnection conn)
        {
            if (conn.playerController == null)
            {
                // this is now allowed (was not for a while)
                if (LogFilter.Debug) Debug.Log("Ready with no player object");
            }
            NetworkServer.SetClientReady(conn);
        }

        [Obsolete("Use OnServerAddPlayer(NetworkConnection conn, AddPlayerMessage extraMessage) instead")]
        public virtual void OnServerAddPlayer(NetworkConnection conn, NetworkMessage extraMessage)
        {
            OnServerAddPlayerInternal(conn);
        }

        void OnServerAddPlayerInternal(NetworkConnection conn)
        {
            if (playerPrefab == null)
            {
                Debug.LogError("The PlayerPrefab is empty on the NetworkManager. Please setup a PlayerPrefab object.");
                return;
            }

            if (playerPrefab.GetComponent<NetworkIdentity>() == null)
            {
                Debug.LogError("The PlayerPrefab does not have a NetworkIdentity. Please add a NetworkIdentity to the player prefab.");
                return;
            }

            if (conn.playerController != null)
            {
                Debug.LogError("There is already a player for this connections.");
                return;
            }

            Transform startPos = GetStartPosition();
            GameObject player = startPos != null
                ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
                : Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);

            NetworkServer.AddPlayerForConnection(conn, player);
        }

        public Transform GetStartPosition(Team team = Team.None)
        {
            // first remove any dead transforms
            startPositions.RemoveAll(t => t == null);
            
            // Check if there are Player Spawn Points
            if (startPositions.Count > 0)
            {
                // Totally random spawn
                if (playerSpawnMethod == PlayerSpawnType.Random)
                {
                    int index = UnityEngine.Random.Range(0, startPositions.Count);
                    return startPositions[index].transform;
                }
                
                // Reset round robin spawn point selection
                if (s_StartPositionIndex >= startPositions.Count)
                {
                    s_StartPositionIndex = 0;
                }

                // Round Robin selection
                if (playerSpawnMethod == PlayerSpawnType.RoundRobin)
                {
                    if (s_StartPositionIndex >= startPositions.Count)
                    {
                        s_StartPositionIndex = 0;
                    }

                    Transform startPos = startPositions[s_StartPositionIndex].transform;
                    s_StartPositionIndex += 1;
                    return startPos;
                }

                // Team based Round Robin selection
                if (playerSpawnMethod == PlayerSpawnType.TeamBased)
                {
                    // If player spawn point has no team
                    if (team == Team.None)
                    {
                        Debug.Log("NO TEAM");
                        Transform startPos = startPositions[s_StartPositionIndex].transform;
                        s_StartPositionIndex += 1;
                        return startPos;
                    }
                    else
                    {
                        // Set a default spawn point
                        Transform startPos = startPositions[s_StartPositionIndex].transform;

                        // Check remained spawn points
                        for (int i = s_StartPositionIndex; i < startPositions.Count; i++)
                        {
                            if (startPositions[i].team == team)
                            {
                                startPos = startPositions[i].transform;
                                s_StartPositionIndex = i + 1;
                                return startPos;
                            }
                        }

                        // If there aren't any, check the previous on restarting from the first of the list
                        for (int i = 0; i < s_StartPositionIndex ; i++)
                        {
                            if (startPositions[i].team == team)
                            {
                                startPos = startPositions[i].transform;
                                s_StartPositionIndex = i + 1;
                                return startPos;
                            }
                        }
                        return startPos;
                    }
                }
            }
            return null;
        }

        public virtual void OnServerRemovePlayer(NetworkConnection conn, NetworkIdentity player)
        {
            if (player.gameObject != null)
            {
                NetworkServer.Destroy(player.gameObject);
            }
        }

        public virtual void OnServerError(NetworkConnection conn, int errorCode) { }

        #endregion

        #region Client System Callbacks
        
        public virtual void OnClientError(NetworkConnection conn, int errorCode) { }

        public virtual void OnClientNotReady(NetworkConnection conn) {}

        // Called from ClientChangeScene immediately before SceneManager.LoadSceneAsync is executed
        // This allows client to do work / cleanup / prep before the scene changes.

        #endregion



        struct PendingPlayer
        {
            public NetworkConnection conn;
            public GameObject lobbyPlayer;
        }

        // configuration
        [Header("Lobby Settings")]
        [FormerlySerializedAs("m_ShowLobbyGUI")] [SerializeField] internal bool showLobbyGUI = true;
        [FormerlySerializedAs("m_MinPlayers")] [SerializeField] int minPlayers = 1;
        [FormerlySerializedAs("m_LobbyPlayerPrefab")] [SerializeField] NetLobbyPlayer lobbyPlayerPrefab;

        [Scene]
        public string LobbyScene;

        [Scene]
        public string[] GameplayScenes;

        // runtime data
        [FormerlySerializedAs("m_PendingPlayers")] List<PendingPlayer> pendingPlayers = new List<PendingPlayer>();
        List<NetLobbyPlayer> lobbySlots = new List<NetLobbyPlayer>();

        public bool allPlayersReady;

        public void OnValidate()
        {
            // always >= 0
            maxConnections = Mathf.Max(maxConnections, 0);

            // always <= maxConnections
            minPlayers = Mathf.Min(minPlayers, maxConnections);

            // always >= 0
            minPlayers = Mathf.Max(minPlayers, 0);

            if (lobbyPlayerPrefab != null)
            {
                NetworkIdentity identity = lobbyPlayerPrefab.GetComponent<NetworkIdentity>();
                if (identity == null)
                {
                    lobbyPlayerPrefab = null;
                    Debug.LogError("LobbyPlayer prefab must have a NetworkIdentity component.");
                }
            }

            // add transport if there is none yet. makes upgrading easier.
            if (transport == null)
            {
                // was a transport added yet? if not, add one
                transport = GetComponent<Transport>();
                if (transport == null)
                {
                    transport = gameObject.AddComponent<TelepathyTransport>();
                    Debug.Log("NetworkManager: added default Transport because there was none yet.");
                }
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(gameObject);
#endif
            }

            maxConnections = Mathf.Max(maxConnections, 0); // always >= 0

            if (playerPrefab != null && playerPrefab.GetComponent<NetworkIdentity>() == null)
            {
                Debug.LogError("NetworkManager - playerPrefab must have a NetworkIdentity.");
                playerPrefab = null;
            }
        }

        public void PlayerLoadedScene(NetworkConnection conn)
        {
            if (LogFilter.Debug) Debug.Log("NetworkLobbyManager OnSceneLoadedMessage");
            SceneLoadedForPlayer(conn, conn.playerController.gameObject);
        }

        internal void ReadyStatusChanged()
        {
            int CurrentPlayers = 0;
            int ReadyPlayers = 0;

            foreach (NetLobbyPlayer item in lobbySlots)
            {
                if (item != null)
                {
                    CurrentPlayers++;
                    if (item.readyToBegin)
                        ReadyPlayers++;
                }
            }

            if (CurrentPlayers == ReadyPlayers)
                CheckReadyToBegin();
            else
            {
                NotAllPlayersReady();
            }
        }

        void NotAllPlayersReady()
        {
            if (UIManager.instance != null)
            {
                UIManager.instance.EnableStartButton(false);
            }
            allPlayersReady = false;
        }

        void SceneLoadedForPlayer(NetworkConnection conn, GameObject lobbyPlayerGameObject)
        {
            // if not a lobby player.. dont replace it
            if (lobbyPlayerGameObject.GetComponent<NetLobbyPlayer>() == null) return;

            if (LogFilter.Debug) Debug.LogFormat("NetworkLobby SceneLoadedForPlayer scene: {0} {1}", SceneManager.GetActiveScene().name, conn);

            if (SceneManager.GetActiveScene().name == LobbyScene)
            {
                // cant be ready in lobby, add to ready list
                PendingPlayer pending;
                pending.conn = conn;
                pending.lobbyPlayer = lobbyPlayerGameObject;
                pendingPlayers.Add(pending);
                return;
            }

            GameObject gamePlayer = OnLobbyServerCreateGamePlayer(conn);
            if (gamePlayer == null)
            {
                // get start position from base class
                Transform startPos = GetStartPosition(lobbyPlayerGameObject.GetComponent<NetLobbyPlayer>().team);
                gamePlayer = startPos != null
                    ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
                    : Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
                gamePlayer.name = playerPrefab.name;

                // Set up player with lobby player info in server
                GamePlayer player = gamePlayer.GetComponent<GamePlayer>();
                player.LobbyPlayer = lobbyPlayerGameObject.GetComponent<NetLobbyPlayer>();
                player.LastSpawn = new TransformInfo(startPos);
            }

            if (!OnLobbyServerSceneLoadedForPlayer(lobbyPlayerGameObject, gamePlayer))
                return;

            // replace lobby player with game player
            NetworkServer.ReplacePlayerForConnection(conn, gamePlayer);
        }

        public void CheckReadyToBegin()
        {
            if (SceneManager.GetActiveScene().name != LobbyScene) return;

            if (minPlayers > 0 && NetworkServer.connections.Count(conn => conn.Value != null && conn.Value.playerController.gameObject.GetComponent<NetLobbyPlayer>().readyToBegin) < minPlayers)
            {
                NotAllPlayersReady();
                return;
            }

            pendingPlayers.Clear();
            allPlayersReady = true;
            OnLobbyServerPlayersReady();
        }

        void CallOnClientEnterLobby()
        {
            OnLobbyClientEnter();
            foreach (NetLobbyPlayer player in lobbySlots)
                player?.OnClientEnterLobby();
        }

        void CallOnClientExitLobby()
        {
            OnLobbyClientExit();
            foreach (NetLobbyPlayer player in lobbySlots)
                player?.OnClientExitLobby();
        }

        #region server handlers

        public void OnServerConnect(NetworkConnection conn)
        {
            if (numPlayers >= maxConnections)
            {
                conn.Disconnect();
                return;
            }

            // cannot join game in progress
            if (SceneManager.GetActiveScene().name != LobbyScene)
            {
                conn.Disconnect();
                return;
            }

            OnLobbyServerConnect(conn);
        }

        public void OnServerDisconnect(NetworkConnection conn)
        {
            if (conn.playerController != null)
            {
                NetLobbyPlayer player = conn.playerController.GetComponent<NetLobbyPlayer>();

                if (player != null)
                    lobbySlots.Remove(player);
            }

            NotAllPlayersReady();

            foreach (NetLobbyPlayer player in lobbySlots)
            {
                if (player != null)
                    player.GetComponent<NetLobbyPlayer>().readyToBegin = false;
            }

            if (SceneManager.GetActiveScene().name == LobbyScene)
            {
                RecalculateLobbyPlayerIndices();
                SyncIndexes();
            }
            
            NetworkServer.DestroyPlayerForConnection(conn);
            if (LogFilter.Debug) Debug.Log("OnServerDisconnect: Client disconnected.");
            OnLobbyServerDisconnect(conn);
        }

        [System.Obsolete("Use OnServerAddPlayer(NetworkConnection conn, AddPlayerMessage extraMessage) instead")]
        public void OnServerAddPlayer(NetworkConnection conn)
        {
            OnServerAddPlayer(conn, null);
        }

        public void OnServerAddPlayer(NetworkConnection conn, AddPlayerMessage extraMessage)
        {
            if (SceneManager.GetActiveScene().name != LobbyScene) return;

            if (lobbySlots.Count == maxConnections) return;

            NotAllPlayersReady();

            if (LogFilter.Debug) Debug.LogFormat("NetworkLobbyManager.OnServerAddPlayer playerPrefab:{0}", lobbyPlayerPrefab.name);

            GameObject newLobbyGameObject = OnLobbyServerCreateLobbyPlayer(conn);
            if (newLobbyGameObject == null)
                newLobbyGameObject = (GameObject)Instantiate(lobbyPlayerPrefab.gameObject, Vector3.zero, Quaternion.identity);

            NetLobbyPlayer newLobbyPlayer = newLobbyGameObject.GetComponent<NetLobbyPlayer>();

            lobbySlots.Add(newLobbyPlayer);

            RecalculateLobbyPlayerIndices();

            NetworkServer.AddPlayerForConnection(conn, newLobbyGameObject);

            SyncIndexes();
        }

        void RecalculateLobbyPlayerIndices()
        {
            if (lobbySlots.Count > 0)
            {
                for (int i = 0; i < lobbySlots.Count; i++)
                {
                    lobbySlots[i].id = i;
                }
            }
        }
        
        void SyncIndexes()
        {
            if (lobbySlots.Count > 0)
            {
                for (int i = 0; i < lobbySlots.Count; i++)
                {
                    lobbySlots[i].NetRpcSendMessage(NetMsg.SetIndex, i.ToString());
                }
            }
        }
        

        public void ServerChangeScene(string sceneName)
        {
            if (sceneName == LobbyScene)
            {
                foreach (NetLobbyPlayer lobbyPlayer in lobbySlots)
                {
                    if (lobbyPlayer == null) continue;

                    // find the game-player object for this connection, and destroy it
                    NetworkIdentity identity = lobbyPlayer.GetComponent<NetworkIdentity>();

                    NetworkIdentity playerController = identity.connectionToClient.playerController;
                    NetworkServer.Destroy(playerController.gameObject);

                    if (NetworkServer.active)
                    {
                        // re-add the lobby object
                        lobbyPlayer.GetComponent<NetLobbyPlayer>().readyToBegin = false;
                        NetworkServer.ReplacePlayerForConnection(identity.connectionToClient, lobbyPlayer.gameObject);
                    }
                }
            }/*
            else
            {
                if (dontDestroyOnLoad)
                {
                    foreach (NetLobbyPlayer lobbyPlayer in lobbySlots)
                    {
                        if (lobbyPlayer != null)
                        {
                            // Dan Obsolete
                            //lobbyPlayer.transform.SetParent(null);
                            //DontDestroyOnLoad(lobbyPlayer);
                        }
                    }
                }
            }//*/

            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("ServerChangeScene empty scene name");
                return;
            }

            if (LogFilter.Debug) Debug.Log("ServerChangeScene " + sceneName);
            NetworkServer.SetAllClientsNotReady();
            networkSceneName = sceneName;

            s_LoadingSceneAsync = SceneManager.LoadSceneAsync(sceneName);

            SceneMessage msg = new SceneMessage(networkSceneName);
            NetworkServer.SendToAll(msg);

            s_StartPositionIndex = 0;
            startPositions.Clear();
        }

        public void OnServerSceneChanged(string sceneName)
        {
            if (sceneName != LobbyScene)
            {
                // call SceneLoadedForPlayer on any players that become ready while we were loading the scene.
                foreach (PendingPlayer pending in pendingPlayers)
                {
                    SceneLoadedForPlayer(pending.conn, pending.lobbyPlayer);
                }

                pendingPlayers.Clear();
            }

            OnLobbyServerSceneChanged(sceneName);
        }

        public void OnStartServer()
        {
            if (string.IsNullOrEmpty(LobbyScene))
            {
                Debug.LogError("NetworkLobbyManager LobbyScene is empty. Set the LobbyScene in the inspector for the NetworkLobbyMangaer");
                return;
            }

            if (GameplayScenes.Length == 0 || string.IsNullOrEmpty(GameplayScenes[0])) //(string.IsNullOrEmpty(GameplayScene))
            {
                Debug.LogError("NetworkLobbyManager PlayScene is empty. Set the PlayScene in the inspector for the NetworkLobbyMangaer");
                return;
            }

            OnLobbyStartServer();
        }

        public void OnStartHost()
        {
            OnLobbyStartHost();
        }

        public void OnStopServer()
        {
            lobbySlots.Clear();
        }

        public void OnStopHost()
        {
            OnLobbyStopHost();
        }

        #endregion

        #region client handlers

        public void OnStartClient(NetworkClient lobbyClient)
        {
            if (lobbyPlayerPrefab == null || lobbyPlayerPrefab.gameObject == null)
                Debug.LogError("NetworkLobbyManager no LobbyPlayer prefab is registered. Please add a LobbyPlayer prefab.");
            else
                ClientScene.RegisterPrefab(lobbyPlayerPrefab.gameObject);

            if (playerPrefab == null)
                Debug.LogError("NetworkLobbyManager no GamePlayer prefab is registered. Please add a GamePlayer prefab.");
            else
                ClientScene.RegisterPrefab(playerPrefab);

            OnLobbyStartClient(lobbyClient);
        }

        public void OnClientConnect(NetworkConnection conn)
        {
            OnLobbyClientConnect(conn);
            CallOnClientEnterLobby();
            if (!clientLoadedScene)
            {
                // Ready/AddPlayer is usually triggered by a scene load completing. if no scene was loaded, then Ready/AddPlayer it here instead.
                ClientScene.Ready(conn);
                if (autoCreatePlayer)
                {
                    ClientScene.AddPlayer();
                }
            }
        }

        public void OnClientDisconnect(NetworkConnection conn)
        {
            OnLobbyClientDisconnect(conn);
            StopClient();
        }

        public void OnStopClient()
        {
            OnLobbyStopClient();
            CallOnClientExitLobby();

            if (!string.IsNullOrEmpty(offlineScene) && offlineScene != SceneManager.GetActiveScene().name)
            {
                // Move the LobbyManager from the virtual DontDestroyOnLoad scene to the Game scene.
                // This let's it be destroyed when client changes to the Offline scene.
                SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetActiveScene());
            }
        }

        public void OnClientChangeScene(string newSceneName)
        {
            if (LogFilter.Debug) Debug.LogFormat("OnClientChangeScene from {0} to {1}", SceneManager.GetActiveScene().name, newSceneName);

            bool isAGameScene = false;
            foreach(string s in GameplayScenes )
            {
                if(newSceneName == s)
                {
                    isAGameScene = true;
                    break;
                }
            }
            if (SceneManager.GetActiveScene().name == LobbyScene && isAGameScene && dontDestroyOnLoad && IsClientConnected() && client != null)
            {
                GameObject lobbyPlayer = client?.connection?.playerController?.gameObject;
                if (lobbyPlayer != null)
                {
                    // Dan Obsolete
                    //lobbyPlayer.transform.SetParent(null);
                    //DontDestroyOnLoad(lobbyPlayer);
                }
                else
                    Debug.LogWarningFormat("OnClientChangeScene: lobbyPlayer is null");
            }
            else
               if (LogFilter.Debug) Debug.LogFormat("OnClientChangeScene {0} {1} {2}", dontDestroyOnLoad, IsClientConnected(), client != null);
        }

        public void OnClientSceneChanged(NetworkConnection conn)
        {
            if (SceneManager.GetActiveScene().name == LobbyScene)
            {
                if (client.isConnected)
                    CallOnClientEnterLobby();
            }
            else
                CallOnClientExitLobby();

            // always become ready.
            ClientScene.Ready(conn);

            // vis2k: replaced all this weird code with something more simple
            if (autoCreatePlayer)
            {
                // add player if existing one is null
                if (ClientScene.localPlayer == null)
                {
                    ClientScene.AddPlayer();
                }
            }
            OnLobbyClientSceneChanged(conn);
        }

        #endregion

        #region lobby server virtuals

        public virtual void OnLobbyStartHost() { }

        public virtual void OnLobbyStopHost() { }

        public virtual void OnLobbyStartServer() { }

        public virtual void OnLobbyServerConnect(NetworkConnection conn) { }

        public virtual void OnLobbyServerDisconnect(NetworkConnection conn) { }

        public virtual void OnLobbyServerSceneChanged(string sceneName) { }

        public virtual GameObject OnLobbyServerCreateLobbyPlayer(NetworkConnection conn)
        {
            return null;
        }

        public virtual GameObject OnLobbyServerCreateGamePlayer(NetworkConnection conn)
        {
            return null;
        }

        // for users to apply settings from their lobby player object to their in-game player object
        public virtual bool OnLobbyServerSceneLoadedForPlayer(GameObject lobbyPlayer, GameObject gamePlayer)
        {
            GamePlayer player = gamePlayer.GetComponent<GamePlayer>();
            Debug.Assert(player != null);
            //player.LobbyPlayer = lobbyPlayer.GetComponent<NetLobbyPlayer>();
            //player.SetLobbyPlayer(player.LobbyPlayer);
            return true;
        }

        public virtual void OnLobbyServerPlayersReady()
        {
            if (allPlayersReady)
            {
                UIManager.instance.EnableStartButton(true);
            }
        }

        #endregion

        #region lobby client virtuals

        public virtual void OnLobbyClientEnter() { }

        public virtual void OnLobbyClientExit() { }

        public virtual void OnLobbyClientConnect(NetworkConnection conn) { }

        public virtual void OnLobbyClientDisconnect(NetworkConnection conn) { }

        public virtual void OnLobbyStartClient(NetworkClient lobbyClient) { }

        public virtual void OnLobbyStopClient() { }

        public virtual void OnLobbyClientSceneChanged(NetworkConnection conn) { }

        // for users to handle adding a player failed on the server
        public virtual void OnLobbyClientAddPlayerFailed() { }

        #endregion

        #region optional UI

        public virtual void OnGUI()
        {
            if (!showLobbyGUI)
                return;

            if (SceneManager.GetActiveScene().name != LobbyScene)
                return;

        }

        #endregion
       
    }

}
