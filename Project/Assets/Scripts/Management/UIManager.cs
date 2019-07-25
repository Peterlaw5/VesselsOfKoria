using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;
using System.Text.RegularExpressions;
using System.Net;
using FMODUnity;


namespace VoK
{
    public class UIManager : MonoBehaviour
    {
        [HideInInspector]
        public static UIManager instance;    
        


        [Header("Lobby Objects references")]
        [Tooltip("Lobby panel")]
        public GameObject m_connectedToLobbyPanel;
        [Tooltip("Players panel in lobby panel")]
        public GameObject m_lobbyPlayersPanel;
        [Tooltip("Lobby panel start game button")]
        public GameObject[] m_startGameButtons;
        [Tooltip("Lobby panel enter name textfield")]
        public InputField m_inputName;
        [Tooltip("Prelobby panel connect to server button")]
        public GameObject m_connectButton;
        [Tooltip("Players ui prefab for players panel in lobby panel")]
        public GameObject m_uiLobbyPlayerPrefab;
        [Tooltip("Seekers buttons")]
        public GameObject[] m_seekersButton;
        [Tooltip("Connect to server Text Input field")]
        public InputField m_ipAddress;

        public LobbyModelManager teamAModels;
        public LobbyModelManager teamBModels;

        public TextMeshProUGUI teamAInfo;
        public TextMeshProUGUI teamBInfo;
        
        public string[] textIpErrors;
        public TextMeshProUGUI IPText;

        [Header("InfoAbility- LobbyPlayer")]
        public GameObject abilityLobbyInfoPrefab;
        public GameObject abilityInfoPanelA;
        public GameObject abilityInfoPanelB;

        public TextManager textManager;

        bool canStartGame;
        public Color ownButtonSelected = Color.yellow;
        public Color readyButtonSelected = Color.yellow;
        public Button readyButton;

        public StudioEventEmitter seekerSelection;


        //[Header("Ingame Objects references")]
        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {

            canStartGame = true;
            IPText.text = "";
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Get local IP Address
            if (m_ipAddress)
            {
                m_ipAddress.text = NetLobbyManager.singleton.GetMyIP();
            }

            teamAInfo.text = "";
            teamBInfo.text = "";
        }


        // Network commands 
        public void StartHostWithPanel(int roundNumber)
        {
            if (NetworkClient.singleton != null) return;
            MenuManager.Instance.roundNumber = roundNumber;
            MenuManager.Instance.StartHost();
            EnableClientPanel();
        }
        public void StartHost()
        {
            if (NetworkClient.singleton != null) return;
            MenuManager.Instance.StartHost();
        }

        public void StartClient()
        {
            MenuManager.Instance.StartClient();
        }

        public void StartClientWithPanel()
        {
            // Avoid start client while trying to connect
            if (NetworkClient.singleton != null) return;

            // Check ip address string
            if (!IpError(m_ipAddress.text)) return;

            // Start connection
            MenuManager.Instance.StartClient();

            // Enable Lobby panel only if the connection to server works
            StartCoroutine(GameManager.WaitCondition(() => NetworkClient.singleton != null && NetworkClient.singleton.isConnected, EnableClientPanel));
            StartCoroutine(WaitConnectButton());
        }


        public void StartGame(int mapID = 0)
        {
            if (canStartGame)
            {
                canStartGame = false;
                MenuManager.Instance.StartGame(mapID);
            }
        }

        public void CleanUpNetwork()
        {
            if (NetLobbyManager.singleton.allPlayersReady && NetLobbyManager.singleton.GetCurrentScene() == NetLobbyManager.singleton.LobbyScene) return;
            m_inputName.text = "";
            MenuManager.Instance.CleanUpNetwork();
        }

        // Lobby commands
        public void ChooseSeeker(ButtonEnum buttonEnum)
        {
            MenuManager.Instance.SetLobbyPlayerSeeker((uint)buttonEnum.seeker);
        }

        public void ChooseTeam(ButtonEnum buttonEnum)
        {
            MenuManager.Instance.SetLobbyPlayerTeam((Team) buttonEnum.team);
        }

        public void ChooseTeamAndSeeker(ButtonEnum buttonEnum)
        {
            MenuManager.Instance.SetLobbyPlayerTeamAndSeeker(buttonEnum);
        }

        // Enable an ui gameobject(panel) and disable all siblings
        public void EnablePanel(GameObject panel)
        {
            if (NetLobbyManager.singleton.allPlayersReady && NetLobbyManager.singleton.GetCurrentScene() == NetLobbyManager.singleton.LobbyScene) return;
            for(int i = 0; i < panel.transform.parent.childCount; i++)
            {
                panel.transform.parent.GetChild(i).gameObject.SetActive(false);
            }
            panel?.SetActive(true);
        }

        // Disable an ui gameobject(panel) and disable all siblings
        public void DisablePanel(GameObject panel)
        {
            if (NetLobbyManager.singleton.allPlayersReady && NetLobbyManager.singleton.GetCurrentScene() == NetLobbyManager.singleton.LobbyScene) return;
            panel?.SetActive(false);
        }

        //Enable an ui gameobject(panel) 
        public void OnlyEnablePanel(GameObject panel)
        {
            panel?.SetActive(true);
        }

        public void ExitButton(GameObject button)
        {
            Application.Quit();
        }

        // Ad Hoc Functions
        public void EnableClientPanel(params object[] p)
        {
            for (int i = 0; i < m_connectedToLobbyPanel.transform.parent.childCount; i++)
            {
                m_connectedToLobbyPanel.transform.parent.GetChild(i).gameObject.SetActive(false);
            }
            m_connectedToLobbyPanel?.SetActive(true);
        }

        // Net Lobby Player functions
        public GameObject AddLobbyPlayer(bool localPlayer)
        {
            GameObject lobbyPlayer = Instantiate(m_uiLobbyPlayerPrefab, m_lobbyPlayersPanel.transform);
            if (!localPlayer)
                lobbyPlayer.GetComponentInChildren<Button>().gameObject.SetActive(false);
            return lobbyPlayer;
        }

        public void UpdateLobbyPlayers()
        {
            foreach (NetLobbyPlayer nlp in NetLobbyManager.singleton.netLobbyPlayers)
            {
                StartCoroutine(CoUpdateLobbyPlayer(nlp));
            }
        }
        public void UpdateLobbyPlayer(NetLobbyPlayer netLobbyPlayer)
        {
            StartCoroutine(CoUpdateLobbyPlayer(netLobbyPlayer));
        }

        IEnumerator CoUpdateLobbyPlayer(NetLobbyPlayer netLobbyPlayer)
        {
            yield return new WaitUntil(() => netLobbyPlayer.LobbyObject != null);
                        
            // Initialize name field
            if (NetLobbyPlayer.local == netLobbyPlayer && m_inputName.text.Length == 0)
            {
                UpdateLocalNameField();
            }

            if(NetLobbyPlayer.local == netLobbyPlayer)
            {
                m_inputName.interactable = !netLobbyPlayer.readyToBegin;
            }

            foreach (GameObject g in m_seekersButton)
            {
                Button button = g.GetComponent<Button>();
                ButtonEnum buttonInfo = g.GetComponent<ButtonEnum>();
                bool interactability = false;
                buttonInfo.seekerImage.enabled = false;
                buttonInfo.playerName.text = "";
                g.transform.GetChild(2).GetComponent<TextMeshProUGUI>().enabled = false;
                bool nobodyChosen = true;
                button.targetGraphic.color = button.colors.normalColor;

                foreach (NetLobbyPlayer nlp in NetLobbyManager.singleton.netLobbyPlayers)
                {
                    if (nlp.seeker != null)
                    {
                        if ((uint)buttonInfo.seeker == nlp.seeker.id && (int)nlp.team == (int)buttonInfo.team)
                        {
                            nobodyChosen = false;
                            if (NetLobbyPlayer.local == nlp)
                            {
                                interactability = true;
                                buttonInfo.seekerImage.enabled = true;
                                button.targetGraphic.color = ownButtonSelected;
                            }
                            buttonInfo.playerName.text = nlp.playerName;
                            buttonInfo.playerName.alignment = TextAlignmentOptions.Center;
                            if (nlp.readyToBegin)
                            {
                                g.transform.GetChild(2).GetComponent<TextMeshProUGUI>().enabled = true;
                            }
                        }
                    }
                }
                if(nobodyChosen)
                {
                    interactability = true;
                }
                button.interactable = interactability;
            }

            if(NetLobbyPlayer.local.team != Team.None)
            {
                if (NetLobbyPlayer.local.team == Team.A)
                {
                    teamBModels.Clean();
                    teamAModels.ActivateModel(NetLobbyPlayer.local.seeker.seekerId);
                    //chosenSeekerA.sprite = NetLobbyPlayer.local.seeker.seekerSpriteBody;
                    abilityInfoPanelA.SetActive(true);
                    abilityInfoPanelB.SetActive(false);
                    // disable passive. Change 5 to 6 for all abilities
                    for (int i = 0; i < 6; i++)
                    {
                        UIAbilityLobbyInfo lobbyInfoAbility = abilityInfoPanelA.transform.GetChild(i).GetComponent<UIAbilityLobbyInfo>();
                        lobbyInfoAbility.SetAbilityLobbyInfo(NetLobbyPlayer.local.seeker, i);
                    }
                    teamAInfo.text = NetLobbyPlayer.local.seeker.seekerName + "\n" + NetLobbyPlayer.local.seeker.heroTitle;
                    teamBInfo.text = "";

                }
                if (NetLobbyPlayer.local.team == Team.B)
                {
                    teamAModels.Clean();
                    teamBModels.ActivateModel(NetLobbyPlayer.local.seeker.seekerId);
                    //chosenSeekerA.sprite = NetLobbyPlayer.local.seeker.seekerSpriteBody;

                    abilityInfoPanelA.SetActive(false);
                    abilityInfoPanelB.SetActive(true);

                    for (int i = 0; i < 6; i++)
                    {
                        UIAbilityLobbyInfo lobbyInfoAbility = abilityInfoPanelB.transform.GetChild(i).GetComponent<UIAbilityLobbyInfo>();
                        lobbyInfoAbility.SetAbilityLobbyInfo(NetLobbyPlayer.local.seeker, i);
                    }
                    teamBInfo.text = NetLobbyPlayer.local.seeker.seekerName + "\n" + NetLobbyPlayer.local.seeker.heroTitle; 
                    teamAInfo.text = "";
                }
                    
            }
            else
            {
                teamBModels.Clean();
                teamAModels.Clean();
                abilityInfoPanelA.SetActive(false);
                abilityInfoPanelB.SetActive(false);

                teamAInfo.text = "";
                teamBInfo.text = "";

            }
            if(NetLobbyPlayer.local == netLobbyPlayer)
            {
                if(netLobbyPlayer.readyToBegin)
                {
                    readyButton.targetGraphic.color = readyButtonSelected;
                }
                else
                {
                    readyButton.targetGraphic.color = readyButton.colors.normalColor;
                }
            }
        }

        public bool IpError(string ipaddress)
        {
            IPAddress ip;

            Regex rx = new Regex(@"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}$");
            bool validateIp = rx.IsMatch(ipaddress) && IPAddress.TryParse(ipaddress, out ip);

            if (!validateIp)
            {
                //ip non valido
                if (string.IsNullOrWhiteSpace(ipaddress))
                {
                    IPText.text = textIpErrors[2];
                }
                else
                {
                    IPText.text = textIpErrors[0];
                }
                return false;

            }
            else 
            {
                //ip valido
                IPText.text = "";
                return true;
            }
        }




        //ready button
        public void ReadyButton()
        {
            MenuManager.Instance.SetLobbyPlayerReady();      
            
        }

        public void SetName(InputField inputField)
        {
            MenuManager.Instance.SetLobbyPlayerName(inputField.text);
        }

        public void EnableStartButton(bool enable)
        {
            foreach(GameObject g in m_startGameButtons)
            {
                g.SetActive(enable);
            }
        }

        public 

        IEnumerator WaitConnectButton()
        {
            m_connectButton.GetComponent<Button>().interactable = false;
            while(NetworkClient.singleton != null)
            {
                if (!NetworkClient.singleton.isConnected)
                {
                    IPText.text = textIpErrors[3];
                }
                else
                {
                    break;
                }
                yield return null;
            }

            if(NetworkClient.singleton != null && NetworkClient.singleton.isConnected)
            {
                IPText.text = "";
            }
            else
            {
                IPText.text = textIpErrors[1];
            }
            m_connectButton.GetComponent<Button>().interactable = true;

        }

        public void UpdateLocalNameField()
        {
            if(NetLobbyPlayer.local != null)
            {
                m_inputName.text = NetLobbyPlayer.local.playerName;
            }
        }

    }
}