using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;
using UnityEngine.SceneManagement;
using FMODUnity;



namespace VoK
{
    public class UIGame : MonoBehaviour
    {
        public static UIGame instance;
        bool m_showUI;
        [Header("Objects references - autoassigned")]
        public GamePlayer player;

        [Header("Objects references - to assign")]

        [Header("Landing Ui shake Parameters")]
        public Image lowerUiPanel;
        public float startLowerUiY;
        public float yLowerUIToGo;
        [Range(0,1)]
        public float speedUp;
        [Range(0,1)]
        public float speedDown;
        private Coroutine waitLanding;
        [Header("Lifebar")]
        // Lifebar
        public UILifeBar lifeBarComponent;
        public Image lifeBar;
        public RectTransform lifeBarRect;
        public Color lifeBarLowColor = Color.red;
        public Color lifeBarHighColor = Color.green;
        public Color feedbackHurtColor = Color.red;
        public Color feedbackHealColor = Color.green;
        float lifeBarRectStartWidth;
        [Header("EnergyBar")]
        public Image energyBar;
        public RectTransform energyBarRect;
        public Color energyBarLowColor = Color.blue;
        public Color energyBarHighColor = Color.cyan;
        public Color cantGiveEnergyColor;
        public bool canGiveEnergy;
        float energyBarRectStartWidth;

        //public TextMeshProUGUI stats;
        public TextMeshProUGUI messagesTextBox;
        public Queue<string> sentMessages;
        [Header("Kill Log")]
        public GameObject killLogPrefab;
        public GameObject killLogPanel;
        public int killLogMax = 5;
        [Header("crosshair")]
        public Image crosshair;
        public float crosshairStartY;
        public float crosshairStartX;
        public Image reloadIcon;
        public GameObject m_crosshairKillImage;
        public float m_crosshairKillTime = 0.5f;

        public Image m_crosshairHitImage;
        public float m_crosshairHitTime = 0.5f;
        public Color crosshairHitColor;
        public Color crosshairHealColor;
        public Color crosshairHeadshotColor;
        public AnimationCurve crossHairHitScaleAnim;

        [Header("Player infos")]
        public Image playerImage;
        public Image playerTeamImage;

        public TextMeshProUGUI lifeText;

        [Header("Titan sliders")]
       // public Slider titanBarEnemySlider;
        //public Slider titanBarAllySlider;
        public Image titanBarEnemyImage;
        public Image titanBarAllyImage;
        public float titanBarIconFixer = 0.95f;
        public Image titanPointerEnemyImage;
        public Image titanPointerAllyImage;

        [Header("Team sliders")]
        public GameObject teamBaseEnemyBar;
        public GameObject teamBaseAllyBar;
        public Image teamBaseEnemyImage;
        public Image teamBaseAllyImage;
        public TextMeshProUGUI allyRoundScore;
        public TextMeshProUGUI enemyRoundScore;

        [Header("Generators")]
        [Range( 0.1f,1f)]
        public float genIconMinScale;
        public float scaleMult;
        public Slider genBarSlider;
        public Image genBarFill;
        public Image conquestPos;
        public Image genAPos;
        public Image genAPosFill;
        public Image genBPos;
        public Image genBPosFill;
        GameObject genA;
        Generator generatorAComponent;
        GameObject genB;
        Generator generatorBComponent;
        public float HorGenMargin = 32f;
        public float VertUpperGenMargin = 32f;
        public float VertLowerGenMargin = 32f;


        // public Image UIGenA;
        // public Image UIGenB;
        
        public Image[] generatorsUI;

        [Header("Ability")]
        public Image baseAbility;
        public TextMeshProUGUI baseAbilityText;

        public TextMeshProUGUI baseAbilityAmmoText;

        public Image movementAbility;
        public TextMeshProUGUI movementAbilityText;
        public Image firstAbility;
        public TextMeshProUGUI firstAbilityText;
        public Image secondAbility;
        public TextMeshProUGUI secondAbilityText;
        public Image ultimateAbility;
        public TextMeshProUGUI ultimateAbilityText;
        public float movementAbilityIconY;
        public float firstAbilityIconY;
        public float secondAbilityIconY;
        public float ultimateAbilityIconY;
        public int lowAmmothreshold;
        public TMP_FontAsset lowAmmoFont;
        public TMP_FontAsset normalAmmoFont;
        Coroutine[] abilitiesCorutines;
        Coroutine baseAttackCoroutine;

        public List<TextMeshProUGUI> abilityText;

        [Header("Feedbacks")]
        public GameObject m_feedbacksPanel;
        public GameObject m_healFeedbackPrefab;
        public float m_healFeedbackTime = 1f;
        public GameObject m_hurtFeedbackPrefab;
        public float m_hurtFeedbackTime = 1f;
        public Image deathCamPanel;



        //public TextMeshProUGUI m_splashScreenMessage;
        public TextMeshProUGUI m_eventKillLogMessage;
        //ciaoDan textMeshProUGUI
        public TextMeshProUGUI m_gamePhasesMessage;
        public TextMeshProUGUI m_energyLogMessage;
        public TextMeshProUGUI m_gameOverMessage;
        public TextMeshProUGUI m_killLogMessage;
        public TextMeshProUGUI m_affectedStatusMessage;


        //public Queue<ScreenText> m_splashMessages;
        // queue ciaodan
        public Queue<ScreenText> m_gamePhases;
        public Queue<ScreenText> m_energyLog;
        public Queue<ScreenText> m_gameOver;
        public Queue<ScreenText> m_killLog;
        public Queue<ScreenText> m_affectedStatus;
        public Queue<ScreenText> m_eventKillLog;

        public Queue<float> m_announcerList;
        public Coroutine m_announcerQueueCo;
        public TextMeshProUGUI respawnTimer;


        [Header("Endgame")]
        public GameObject roundEndPanel;
        public TextMeshProUGUI roundResultText;
        public TextMeshProUGUI roundRestartText;

        public GameObject endGamePanel;
        public TextMeshProUGUI endGameTitle;
        public TextMeshProUGUI bestKillerNameText;
        public Image bestKillerIcon;
        public TextMeshProUGUI bestKillerText;
        public TextMeshProUGUI bestDamagerNameText;
        public Image bestDamagerIcon;
        public TextMeshProUGUI bestDamagerText;
        public TextMeshProUGUI bestCarrierNameText;
        public Image bestCarrierIcon;
        public TextMeshProUGUI bestCarrierText;
        [Header("Slow,stun,snare")]
        //public TextMeshProUGUI m_slowMessage;
        //public TextMeshProUGUI m_stunMessage;
        //public TextMeshProUGUI m_snareMessage;
        public bool showStats = false;

        [HideInInspector]
        public float titantRedPathValue;
        [HideInInspector]
        public float titantBluePathValue;

        [Header("Feedbacks")]
        public Image feedbackDamage;
        public Image feedbackHealth;
        public GameObject damageNumericFeedback;        

        public float timeFeedbackDuration = 1f;

        float currentHealth;

        Coroutine feedbackCo;
        Coroutine m_healFeedbackCo;
        Coroutine m_hurtFeedbackCo;
        Coroutine m_crosshairKillCo;
        Coroutine m_crosshairHitCo;
        Coroutine m_crosshairHealHitCo;

        //Coroutine m_splashMessagesCo;
        Coroutine m_eventKillLogCo;
        // Coroutine ciaodan
        Coroutine m_gamePhasesCo;
        Coroutine m_energyLogCo;
        Coroutine m_gameOverCo;
        Coroutine m_killLogCo;
        Coroutine m_affectedStatusCo;

        public GameObject statsTab;
        //public TextMeshProUGUI statsTabTeamA;
        //public TextMeshProUGUI statsTabTeamB;

        [Header("Timer")]
        public TextMeshProUGUI timerText;

        [Header("EndGame")]
        public GameObject allyTeamPanel;
        public GameObject enemyTeamPanel;
        public GameObject playerStatsPrefab;

        [Header("Prefab INGame _ Stats")]
        public GameObject InGameAllyTeamPanel;
        public GameObject InGameEnemyTeamPanel;
        public GameObject inGameStatsPrefab;

        [Header("Pause")]
        public GameObject pausePanel;
        public GameObject mainPauseMenu;
        public GameObject optionPauseMenu;
        public GameObject exitPauseMenu;
        [HideInInspector]
        public bool isPause = false;
        public GameObject buttonsMainPause;

        [Header("Recap-Ability InGame")]
        public GameObject recapAbilityPanel;
        public GameObject abilitiesPanel;
        public GameObject abilityPrefab;
        public int maxAbilitiesNum = 6;
        public TextMeshProUGUI nameChar;
        public GameObject lowerCanvasDisable;
        public GameObject timerCanvasDisable;
                   

        [Header("COMPASS INFO")]
        //public Vector3 northDirection;
        public Quaternion missionDirection;        
        
        public RectTransform titanArrow;
        public Image compassTitanPoint;
        public Image titanCompass;
        public GameObject compassGlow;
        private bool compassEnabled;
        public AnimationCurve yCompassUpAnim;
        public AnimationCurve xCompassUpAnim;
        public AnimationCurve yCompassDownAnim;
        public AnimationCurve xCompassDownAnim;
        Coroutine compasAnim;
        Transform targetTitan;

        public GameObject effectsSpawnPoint;
        private Canvas canvas;
        [Header("Numeric Damage Feedback")]
        public GameObject effect;
        public TextMeshPro numericDamageText;
        public Material numericFeedMat;
        public Color numericHitColor;
        public Color numericHitGlowColor;
        public Color numericHealColor;
        public Color numericHealGlowColor;
        public Color numericHeadshotColor;
        public Color numericHeadshotGlowColor;
        public Camera numericDamageFeedCam;
        public Material numericDamageFeedTextMat;
        Texture2D texture;
        int width;
        int height;

        [Header("Text Manager Object")]
        public TextManager textManager;

        [Header("Announcer Voice")]
        public StudioEventEmitter announcer;

        bool roundBeginMessage = false;
        bool roundUltimates = false;

        private void Awake()
        {
            instance = this;
            m_showUI = false;
        }


        private void Start()
        {
            canGiveEnergy = true;
            compassTitanPoint.gameObject.SetActive(false);
            compassEnabled = true;
            m_crosshairHitImage.enabled = false;
            reloadIcon.enabled = false;
            abilitiesCorutines = new Coroutine[8];
            canvas = GetComponent<Canvas>();
            currentHealth = 0f;
            player = null;

            roundBeginMessage = false;
            roundUltimates = false;

            ////ciaodan co
            //m_splashMessagesCo = null;
            m_gamePhasesCo = null;
            m_energyLogCo = null;
            m_gameOverCo = null;
            m_killLogCo = null;
            m_affectedStatusCo = null;
            m_eventKillLogCo = null;

            //ciaodan queue
            m_gamePhases = new Queue<ScreenText>(); 
            m_energyLog = new Queue<ScreenText>(); 
            m_gameOver = new Queue<ScreenText>(); 
            m_killLog = new Queue<ScreenText>();
            m_announcerList = new Queue<float>();
            //m_affectedStatus = new Queue<ScreenText>();
            //m_splashMessages = new Queue<ScreenText>();
            m_eventKillLog = new Queue<ScreenText>();

            sentMessages = new Queue<string>();
            // endphaseA = baseRed.GetComponent<TeamBase>();
            // endphaseB = baseBlue.GetComponent<TeamBase>();
            lifeBarRectStartWidth = lifeBarRect.sizeDelta.x;
            energyBarRectStartWidth = energyBarRect.sizeDelta.x;
            energyBarRectStartWidth = energyBarRect.sizeDelta.x;
            genBarSlider.gameObject.SetActive(false);

            teamBaseEnemyBar.gameObject.SetActive(false);
            teamBaseAllyBar.gameObject.SetActive(false);
            feedbackDamage.gameObject.SetActive(false);
            StartCoroutine(CoWaitLocalPlayer());
            isPause = false;
            for (int i = 0; i < abilityText.Count; i++)
            {
                abilityText[i].text = "";
            }
            baseAttackCoroutine = null;
            //GameManager.instance.OnGeneratorTaken += GeneratorTaken;
            //GameManager.instance.OnPlayerDeath += PlayerDeath;
            //korGenBar.SetActive(false);
            numericDamageFeedTextMat = numericDamageText.GetComponent<MeshRenderer>().material;
            if(movementAbility && movementAbility.GetComponentInChildren<ParticleSystem>()) movementAbility.GetComponentInChildren<ParticleSystem>().Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (firstAbility && firstAbility.GetComponentInChildren<ParticleSystem>()) firstAbility.GetComponentInChildren<ParticleSystem>().Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (secondAbility && secondAbility.GetComponentInChildren<ParticleSystem>()) secondAbility.GetComponentInChildren<ParticleSystem>().Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (ultimateAbility && ultimateAbility.GetComponentInChildren<ParticleSystem>()) ultimateAbility.GetComponentInChildren<ParticleSystem>().Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            //GameManager.instance.OnGeneratorTaken -= GeneratorTaken;
            //GameManager.instance.OnPlayerDeath -= PlayerDeath;
        }

        // Update is called once per frame
        void Update()
        {
            if (!m_showUI) return;
            if(canvas.worldCamera != GamePlayer.local.mainCamera)
            {
                canvas.worldCamera = GamePlayer.local.mainCamera;
            }

            if (player == null || player.LobbyPlayer == null)
            {
                player = GamePlayer.local;

                if (player != null && player.LobbyPlayer != null)
                {
                    teamBaseAllyImage.color = GameManager.instance.allyColor;
                    titanBarAllyImage.color = GameManager.instance.allyColor;

                    teamBaseEnemyImage.color = GameManager.instance.enemyColor;
                    titanBarEnemyImage.color = GameManager.instance.enemyColor;

                    //playerTeamImage.color = player.allyColor;
                    playerImage.sprite = player.Seeker.seekerSprite;
                    generatorAComponent = GameManager.instance.generators[0];
                    generatorBComponent = GameManager.instance.generators[1];
                    genA = generatorAComponent.gameObject.GetComponentInChildren<UiTraget>().gameObject;
                    genB = generatorBComponent.gameObject.GetComponentInChildren<UiTraget>().gameObject;

                    StartCoroutine(setupRoundCounter());
                    //lifeBarComponent.SetupLifebar(player, lifeBarHighColor);
                    CreateStatsPanel();
                }
                /*
                foreach (GamePlayerController gp in FindObjectsOfType<GamePlayerController>())
                {
                    if (gp.hasAuthority)
                    {
                        player = gp;
                        currentHealth = player.seeker.maxHealth;
                        foreach (NetworkLobbyPlayer lp in FindObjectsOfType<NetworkLobbyPlayer>())
                        {
                            if (gp.Index == lp.Index)
                            {
                                lobbyPlayer = lp;
                            }
                        }
                        break;
                    }
                }*/
            }
            else
            {
                // Health bar
                lifeText.text = player.Health.ToString("F0") + "/" + player.Seeker.maxHealth.ToString("F0");
                //lifeBarComponent.SetLife(Mathf.Clamp01(player.Health / player.Seeker.maxHealth));
                //lifeBarComponent.SetLifeColor(Color.Lerp(lifeBarLowColor, lifeBarHighColor, player.Health / player.Seeker.maxHealth));
                lifeBar.fillAmount = (Mathf.Clamp01(player.Health / player.Seeker.maxHealth));
                lifeBar.color = (Color.Lerp(lifeBarLowColor, lifeBarHighColor, player.Health / player.Seeker.maxHealth));

                // Energy bar
                //energyText.text = player.Energy.ToString("F0") + " / " + player.Seeker.maxCarriedEnergy.ToString("F0");
                //energyBarRect.sizeDelta = new Vector2(player.Energy / player.Seeker.maxCarriedEnergy * energyBarRectStartWidth, energyBarRect.sizeDelta.y);

                genAPos.rectTransform.localScale=Vector3.one* Mathf.Clamp(1 / Vector3.Distance(player.transform.position, generatorAComponent.transform.position) * scaleMult, genIconMinScale, 1f);
                genBPos.rectTransform.localScale = Vector3.one * Mathf.Clamp(1 / Vector3.Distance(player.transform.position, generatorBComponent.transform.position) * scaleMult, genIconMinScale, 1f);
                /*  for (int i = 0; i < generatorsUI.Length; i++)
                  {

                      generatorsUI[i].rectTransform.localScale=Vector3.one*Mathf.Clamp(1 / Vector3.Distance(player.transform.position, GameManager.instance.generators[i].transform.position) * scaleMult, genIconMinScale, 1f);
                      Debug.Log("gen "+ i+" scale is"+Mathf.Clamp(1 / Vector3.Distance(player.transform.position, GameManager.instance.generators[i].transform.position) * scaleMult, genIconMinScale, 1f));
                  }*/

                energyBar.fillAmount = player.Energy / player.Seeker.maxCarriedEnergy;
                if (canGiveEnergy)
                {
                    energyBar.color = Color.Lerp(energyBarLowColor, energyBarHighColor, player.Energy / player.Seeker.maxCarriedEnergy);
                }
                else
                {
                    if (energyBar.color != cantGiveEnergyColor)
                    {
                        energyBar.color = cantGiveEnergyColor;
                    }
                }
                if (player.Energy > 0f)
                {
                    if(!compassGlow.activeSelf)
                    {
                        compassGlow.SetActive(true);
                    }
                    if (!titanArrow.gameObject.activeSelf && !compassTitanPoint.gameObject.activeSelf)
                    {
                        titanArrow.gameObject.SetActive(true);
                    }
                    ChangeMissionDirection();
                }
                else
                {
                    if (compassGlow.activeSelf)
                    {
                        compassGlow.SetActive(false);
                    }
                    if (titanArrow.gameObject.activeSelf)
                    {
                        titanArrow.gameObject.SetActive(false);
                    }
                }
                /*
                if (player.Energy > 0f)
                {/*
                    if (!compassEnabled && compasAnim==null)
                    {
                        // titanCompass.SetActive(true);
                        compassEnabled=true;
                        compasAnim=StartCoroutine(CoIconAnimation(titanCompass,titanCompass.rectTransform.localPosition.y, titanCompass.rectTransform.localPosition.x, yCompassUpAnim, xCompassUpAnim));
                    }
                    energyBar.fillAmount = player.Energy / player.Seeker.maxCarriedEnergy;
                    if(canGiveEnergy)
                    {
                        energyBar.color = Color.Lerp(energyBarLowColor, energyBarHighColor, player.Energy / player.Seeker.maxCarriedEnergy);
                    }
                    else
                    {
                        if (energyBar.color != cantGiveEnergyColor)
                        {
                            energyBar.color = cantGiveEnergyColor;
                        }
                        
                    }
                    if(titanArrow.gameObject.activeSelf==true)
                    {
                        ChangeMissionDirection();
                    }
                 
                }
                else
                {
                    if (compassEnabled&& compasAnim == null)
                    {
                        compassEnabled = false;
                        compasAnim=StartCoroutine(CoIconAnimation(titanCompass, titanCompass.rectTransform.localPosition.y, titanCompass.rectTransform.localPosition.x, yCompassDownAnim, xCompassDownAnim));
                    }
                }*/

                if (player.Seeker.abilities[(int)AbilityType.Base].maxAmmo != 0)
                {
                    int ammo = player.BaseAbilityAmmo;
                    int maxAmmo = player.Seeker.abilities[(int)AbilityType.Base].maxAmmo;
                    if (!player.CheckCooldown(AbilityType.Base))
                    {
                        if (ammo == maxAmmo)
                        {
                            ammo = 0;
                        }
                    }

                    if(baseAbilityAmmoText.font!=lowAmmoFont && ammo <= maxAmmo / lowAmmothreshold)
                    {
                        baseAbilityAmmoText.font = lowAmmoFont;
                    }
                    if(baseAbilityAmmoText.font !=normalAmmoFont&& ammo >= maxAmmo / lowAmmothreshold)
                    {
                        baseAbilityAmmoText.font = normalAmmoFont;
                    }
                    
                    baseAbilityAmmoText.text = ammo + "/" + maxAmmo + (ammo != maxAmmo && ammo != 0 ? "\n(R)" : "");
                }
                else
                {
                    baseAbilityAmmoText.text = string.Empty;
                }

                if (!player.IsDead && !GameManager.instance.isRoundOver)
                {
                   if (!player.m_canInput && !player.CanRotateVisual)
                   {
                        //m_stunMessage.text = "You are stunned";
                       //m_stunMessage.text = textManager.stunMsg.content;
                       //m_snareMessage.text = "";
                       //m_slowMessage.text = "";
                        m_affectedStatusMessage.text = textManager.stunMsg.content;
                   }
                   else if(player.m_canUseMobility && player.m_speedMultiplier ==0)
                    {
                        m_affectedStatusMessage.text = textManager.snaredMsg.content;
                       // m_stunMessage.text = "";
                       //m_snareMessage.text = textManager.snaredMsg.content;
                       //m_slowMessage.text = "";
                   }
                   else if(player.m_speedMultiplier<1&& player.m_speedMultiplier > 0)
                    {
                        m_affectedStatusMessage.text = textManager.slowMsg.content;
                        //m_stunMessage.text = "";
                        //m_snareMessage.text = "";
                        //m_slowMessage.text = textManager.slowMsg.content;
                   }
                   else
                   {
                        m_affectedStatusMessage.text = "";
                        //m_stunMessage.text = "";
                        //m_snareMessage.text = "";
                        //m_slowMessage.text = "";
                    }
                }

                //Timer <3 <3 <3

                timerText.text = System.TimeSpan.FromSeconds(GameManager.instance.matchTime).ToString(@"mm\:ss");
                if (GameManager.instance.matchTime >= 3600)
                {

                    timerText.text = System.TimeSpan.FromSeconds(GameManager.instance.matchTime).ToString(@"hh\:mm\:ss");
                }

                //COMPASS FUNCTIONS
                //ChangeNorthDirection();

                //feedback generator
                //genAPos.transform.position = Camera.main.WorldToScreenPoint(genA.transform.position)

                //Generator A
                if (!generatorAComponent.playersInArea.Contains(player))
                {
                    Vector3 genScreenPos = player.mainCamera.WorldToScreenPoint(genA.transform.position);
                    float posX = Mathf.Clamp(genScreenPos.x, HorGenMargin, Screen.width - HorGenMargin);
                    float posY = Mathf.Clamp(genScreenPos.y, VertLowerGenMargin, Screen.height - VertUpperGenMargin);
                    if (genScreenPos.z < 0f)
                    {
                        posY = Screen.height - Mathf.Clamp(genScreenPos.y, VertUpperGenMargin, Screen.height - VertLowerGenMargin);
                        if (posX < Screen.width * 0.5f)
                        {
                            posX = (Screen.width - HorGenMargin);
                        }
                        else
                        {
                            posX = HorGenMargin;
                        }
                    }
                    genAPos.transform.position = new Vector3(posX, posY, genA.transform.position.z);
                }
                else
                {
                    genAPos.transform.position = conquestPos.transform.position;
                }
               
                genAPosFill.fillAmount = Mathf.Abs(generatorAComponent.m_conquestValue);
                genAPosFill.color = ((int)player.Team == (int)Mathf.Sign(generatorAComponent.m_conquestValue) ? GameManager.instance.allyColor : GameManager.instance.enemyColor);

                //Generator B
                if (!generatorBComponent.playersInArea.Contains(player))
                {
                    Vector3 genScreenPos = player.mainCamera.WorldToScreenPoint(genB.transform.position);
                    float posX = Mathf.Clamp(genScreenPos.x, HorGenMargin, Screen.width - HorGenMargin);
                    float posY = Mathf.Clamp(genScreenPos.y, VertLowerGenMargin, Screen.height - VertUpperGenMargin);
                    if (genScreenPos.z < 0f)
                    {
                        posY = Screen.height - Mathf.Clamp(genScreenPos.y, VertUpperGenMargin, Screen.height - VertLowerGenMargin);
                        if (posX < Screen.width * 0.5f)
                        {
                            posX = (Screen.width - HorGenMargin);
                        }
                        else
                        {
                            posX = HorGenMargin;
                        }
                    }
                    genBPos.transform.position = new Vector3(posX, posY, genB.transform.position.z);
                }
                else
                {
                    genBPos.transform.position = conquestPos.transform.position;
                }
                genBPosFill.fillAmount = Mathf.Abs(generatorBComponent.m_conquestValue);
                genBPosFill.color = ((int)player.Team == (int)Mathf.Sign(generatorBComponent.m_conquestValue) ? GameManager.instance.allyColor : GameManager.instance.enemyColor);
                
                //Feedback Damage/Health
                if (player.Health > 0)
                {
                    if (currentHealth != player.Health)
                    {
                        if (player.Health < currentHealth)
                        {
                            feedbackDamage.gameObject.SetActive(true);
                            if (feedbackCo != null)
                            {
                                StopCoroutine(feedbackCo);
                            }
                            feedbackCo = StartCoroutine(ResetFeedback());
                        }
                        else
                        {
                            if(currentHealth != 0f)
                            {
                                if(m_healFeedbackCo != null)
                                {
                                    StopCoroutine(m_healFeedbackCo);
                                }
                                m_healFeedbackCo = StartCoroutine(CoHealFeedback());
                            }
                        }                       
                        currentHealth = player.Health;
                    }

                }
                else
                {
                    feedbackDamage.gameObject.SetActive(false);
                }
            }

            for (int i = 0; i < generatorsUI.Length; i++)
            {
                if (GameManager.instance.generators[i].m_teamOwner != Team.None)
                {
                    MeshRenderer meshRenderer = GameManager.instance.generators[i].GetComponentInChildren<MeshRenderer>();
                    if (meshRenderer)
                    {
                        generatorsUI[i].color = meshRenderer.material.color;
                    }
                }
                else
                {
                    generatorsUI[i].color = Color.clear;
                }
            }
            /*
                if(GameManager.instance.generators[i].m_conquestValue!=1&& GameManager.instance.generators[i].m_conquestValue != -1 )
                {
                    if(generatorsUI[i].fillAmount != 0)
                    {
                        generatorsUI[i].fillAmount = 0;
                    }
                   
                }
                else
                {
                    if (generatorsUI[i].fillAmount != 1)
                    {
                        generatorsUI[i].fillAmount = 1;
                        generatorsUI[i].color = GameManager.instance.generators[i].GetComponentInChildren<MeshRenderer>().material.color;
                    }
                   
                }
             
            }*/
            //UIGenA.color = (genA genA.GetComponentInChildren<MeshRenderer>().material.color;
            //UIGenB.color = genB.GetComponentInChildren<MeshRenderer>().material.color;

            foreach(GameTitan titan in GameManager.instance.gameTitans)
            {
                if(player.Team == titan.Team)
                {
                    if (titan.Progress < 1f)
                    {
                        if (!titanPointerAllyImage.gameObject.activeSelf)
                        {
                            titanPointerAllyImage.gameObject.SetActive(true);
                            titanPointerAllyImage.color = new Color(titanPointerAllyImage.color.r, titanPointerAllyImage.color.g, titanPointerAllyImage.color.b, 1f);
                        }
                        titanBarAllyImage.fillAmount = titan.Progress;
                        titanPointerAllyImage.rectTransform.anchoredPosition = new Vector2(titanBarAllyImage.rectTransform.sizeDelta.x * (titanBarAllyImage.fillAmount - 0.5f) * titanBarIconFixer, titanPointerAllyImage.rectTransform.anchoredPosition.y);
                    }
                    else
                    {
                        titanBarAllyImage.fillAmount = 1f;
                        if (titanPointerAllyImage.color.a > 0.001f)
                        {
                            Color c = titanPointerAllyImage.color;
                            c.a = 0f;
                            titanPointerAllyImage.color = Color.Lerp(titanPointerAllyImage.color, c, 0.2f);
                        }
                        else
                        {
                            if (titanPointerAllyImage.gameObject.activeSelf)
                                titanPointerAllyImage.gameObject.SetActive(false);
                        }
                    }
                    //set transform for compass
                    targetTitan = titan.transform;
                }
                else
                {
                    if (titan.Progress < 1f)
                    {
                        if (!titanPointerEnemyImage.gameObject.activeSelf)
                        {
                            titanPointerEnemyImage.gameObject.SetActive(true);
                            titanPointerEnemyImage.color = new Color(titanPointerEnemyImage.color.r, titanPointerEnemyImage.color.g, titanPointerEnemyImage.color.b, 1f);
                        }
                        titanBarEnemyImage.fillAmount = titan.Progress;
                        titanPointerEnemyImage.rectTransform.anchoredPosition = new Vector2(titanBarEnemyImage.rectTransform.sizeDelta.x * (0.5f - titanBarEnemyImage.fillAmount) * titanBarIconFixer, titanPointerEnemyImage.rectTransform.anchoredPosition.y);
                    }
                    else
                    {
                        titanBarEnemyImage.fillAmount = 1f;
                        if (titanPointerEnemyImage.color.a > 0.001f)
                        {
                            Color c = titanPointerEnemyImage.color;
                            c.a = 0f;
                            titanPointerEnemyImage.color = Color.Lerp(titanPointerEnemyImage.color, c, 0.2f);
                        }
                        else
                        {
                            if (titanPointerEnemyImage.gameObject.activeSelf)
                                titanPointerEnemyImage.gameObject.SetActive(false);
                        }
                    }

                    //titanPointerEnemyImage.transform
                }
            }
            /*
            titantRedPathValue = titanRed.GetComponent<TitanBehaviour>().pathCompletition;
            titantBluePathValue = titanBlue.GetComponent<TitanBehaviour>().pathCompletition;


            titanBarEnemy.value = titantRedPathValue;
            titanBarAlly.value = titantBluePathValue;
            */
            
            foreach(TeamBase teamBase in GameManager.instance.teamBases)
            {
                if(teamBase.IsActivated)
                {
                    if(teamBase.Team == player.Team)
                    {
                       teamBaseAllyBar.gameObject.SetActive(true);
                       teamBaseAllyImage.fillAmount = teamBase.Progress;
                    }
                    else
                    {
                        teamBaseEnemyBar.gameObject.SetActive(true);
                        teamBaseEnemyImage.fillAmount = teamBase.Progress;
                    }
                }
                else
                {
                    if (teamBase.Team == player.Team)
                    {
                        if (teamBaseAllyBar.gameObject.activeSelf)
                        {
                            teamBaseAllyBar.gameObject.SetActive(false);
                        }
                    }
                    else
                    {
                        if (teamBaseEnemyBar.gameObject.activeSelf)
                        {
                            teamBaseEnemyBar.gameObject.SetActive(false);
                        }
                    }
                }
            }
            /*
            if (endphaseA.phaseActive == true)
            {
                teamBaseAllySlider.gameObject.SetActive(true);
                teamBaseAllySlider.value = endphaseA.completition / endphaseA.maxCompletition;
            }
            if (endphaseB.phaseActive == true)
            {
                teamBaseEnemySlider.gameObject.SetActive(true);
                teamBaseEnemySlider.value = endphaseB.completition / endphaseB.maxCompletition;
            }

            */
            //Landing lower canvas vibration;
            if(player != null && !player.IsGrounded() && waitLanding==null)
            {
                //Debug.Log("corutine partita");
                waitLanding = StartCoroutine(CoLandingShake());
            }
            /*
            bool inGenFlag = false;
            foreach(Generator g in GameManager.instance.generators)
            {
                if (g.IsPlayerInside(player))
                {
                    if (Mathf.Abs(g.Progress) == 1f && g.Team == player.Team)
                    {
                        if (genBarSlider.gameObject.activeSelf)
                            genBarSlider.gameObject.SetActive(false);
                        break;
                    }
                    inGenFlag = true;
                    if (!genBarSlider.gameObject.activeSelf)
                        genBarSlider.gameObject.SetActive(true);
                    genBarSlider.value = Mathf.Abs(g.Progress);
                    genBarFill.color = (Mathf.Sign(g.Progress) * (float)player.Team > 0f ? GameManager.instance.allyColor : GameManager.instance.enemyColor);
                    break;
                }
            }
            if (!inGenFlag && genBarSlider.gameObject.activeSelf)
            {
                genBarSlider.gameObject.SetActive(false);
            }
            */
            if(showStats)
            {
                if(!statsTab.activeSelf)
                {
                    statsTab.SetActive(true);                    
                }
            }
            else
            {
                if (statsTab.activeSelf)
                {
                    statsTab.SetActive(false);
                }
            }

            //PAUSE     
            if(!GameManager.instance.isRoundOver)
            {
                if (GamePlayer.local.m_inputManager.GetKeyDown(InputKey.Pause))
                {
                    if (isPause)
                    {
                        DisablePlayerPause();
                    }
                    else
                    {
                        EnablePlayerPause();
                    }
                }
            }

            //RECAP ABILITY
            if (GamePlayer.local.m_inputManager.GetKeyDown(InputKey.RecapAbility) && !GameManager.instance.isRoundOver)
            {
                recapAbilityPanel.SetActive(true);
                nameChar.text = player.Seeker.name;

                for (int i = 0; i< maxAbilitiesNum; i++)
                {
                    UIAbility abilityRec = Instantiate(abilityPrefab, abilitiesPanel.transform).GetComponent<UIAbility>();
                    abilityRec.SetAbilityRecap(NetLobbyPlayer.local.seeker, i);
                }
                lowerCanvasDisable.SetActive(false);
                timerCanvasDisable.SetActive(false);
            }
            if (GamePlayer.local.m_inputManager.GetKeyUp(InputKey.RecapAbility))
            {
                for(int j = abilitiesPanel.transform.childCount-1 ; j >= 0; j--)
                {
                    Destroy(abilitiesPanel.transform.GetChild(j).gameObject);
                }
                recapAbilityPanel.SetActive(false);
                lowerCanvasDisable.SetActive(true);
                timerCanvasDisable.SetActive(true);
            }
        }

        void CleanScreen()
        {
            messagesTextBox.text = "";
        }

        public void GeneratorTaken(Team team)
        {
            if (GamePlayer.local.Team == team)
                //AddScreenMessage("Your team has conquered a generator");
                 AddScreenMessage(textManager.allyGenTakenMsg);
            else if (team == Team.None)
                AddScreenMessage(textManager.neutralizeGenMsg);
            //AddScreenMessage("A generator has been neutralized");
            else
                AddScreenMessage(textManager.enemyGenTakenMsg);
            //AddScreenMessage("A generator has been neutralized");
        }

        public void BaseConquestStart(Team team)
        {
            if (GamePlayer.local.Team == team)
                AddScreenMessage(textManager.allyFinalAttackMsg);
            else
                AddScreenMessage(textManager.enemyFinalAttackMsg);
        }

        public void WinMessage(Team team)
        {
            if (GamePlayer.local.Team == team)
            {
                AddScreenMessage(textManager.allyScreenWinMsg);
                endGameTitle.text = textManager.allyEndGameTitleWinMsg;
            }
            else
            {
                AddScreenMessage(textManager.enemyScreenWinMsg);
                endGameTitle.text = textManager.enemyEndGameTitleWinMsg;
            }
        }

        public void PlayAnnouncer(float parameter)
        {
            m_announcerList.Enqueue(parameter);
            if(m_announcerQueueCo == null)
            {
                m_announcerQueueCo = StartCoroutine(CoPlayAnnouncer(parameter));
            }
        }

        IEnumerator CoPlayAnnouncer(float parameter)
        {
            while (m_announcerList.Count > 0)
            {
                announcer.Play();
                announcer.SetParameter("AnnouncerTrackID", m_announcerList.Dequeue());
                Debug.Log("<b><color=#00FF00>Announcer message started</color></b>");
                yield return new WaitUntil(() => !announcer.IsPlaying());
                Debug.Log("<b><color=#FF0000>Announcer message ended</color></b>");
            }
            announcer.Stop();
            m_announcerQueueCo = null;

        }

        public void PlayerDeath(GamePlayer deadPlayer, GamePlayer killer)
        {
            GameObject killLogObj = Instantiate(killLogPrefab);
            killLogObj.transform.SetParent(killLogPanel.transform,false);
            killLogObj.GetComponent<UIKillLog>().SetKillLog(killer, deadPlayer);
            PlayAnnouncer((killer.Team == GamePlayer.local.Team ? 1f : 2f));
            if (killLogMax < killLogPanel.transform.childCount)
            {
                List<GameObject> logToDestroy = new List<GameObject>();
                for (int i = killLogMax; i < killLogPanel.transform.childCount; i++)
                {
                    logToDestroy.Add(killLogPanel.transform.GetChild(i - killLogMax).gameObject);
                }
                foreach(GameObject g in logToDestroy)
                {
                    Destroy(g);
                }
            }
        }

        public void PlayerKill()
        {
            m_crosshairKillImage.SetActive(true);
            if (m_crosshairKillCo != null) StopCoroutine(m_crosshairKillCo);
            m_crosshairKillCo = StartCoroutine(CoCrosshairKill());
        }

        public void StartBaseAttackCooldown(int index = 0)
        {
            if (baseAttackCoroutine != null)
            {
                StopCoroutine(baseAttackCoroutine);
                baseAbility.fillAmount = 1f;
            }
            baseAttackCoroutine = null;
            if (index != 0)
            {
                baseAttackCoroutine = StartCoroutine(CoBaseAttackCD(index));
            }
        }

        IEnumerator CoBaseAttackCD(int maxCDindex)
        {
            float maxCD = player.Abilities[maxCDindex].cooldown;
            maxCD = (player.BaseAbilityAmmo == player.Abilities[(int)AbilityType.Base].maxAmmo && player.Abilities[(int)AbilityType.Base].maxAmmo > 0 ? player.Abilities[(int)AbilityType.Base].reloadCooldown : maxCD);
            baseAbility.fillAmount = 0f;
            while (baseAbility.fillAmount < 1f)
            {
                yield return null;
                baseAbility.fillAmount = Mathf.Clamp01(1f - (player.abilitiesCooldowns[maxCDindex] / maxCD));
                baseAbilityText.text = Mathf.Abs(player.abilitiesCooldowns[(int)AbilityType.Base]).ToString("0");
            }
            baseAbilityText.text = "";
        }

        IEnumerator CoCrosshairKill()
        {
            yield return new WaitForSeconds(m_crosshairKillTime);
            m_crosshairKillImage.SetActive(false);
            m_crosshairKillCo = null;
        }


        IEnumerator CoDamageFeedback(GameObject g)
        {
            float timer = 0f;
            while(timer < 0.7f)
            {
                yield return null;
                timer += Time.deltaTime;
            }
            Destroy(g);
        }

        public void PlayerHit(HitType type, GamePlayer hitted, float damage)
        {
            //GameObject g = Instantiate(damageNumericFeedback);
           /* g.transform.position = hitted.transform.position + Vector3.up * Random.Range(2f, 3f);
            g.GetComponent<TextMeshPro>().text = damage.ToString("F0");
            StartCoroutine(CoDamageFeedback(g));*/
            switch (type)
            {
                case HitType.Hit:
                    {
                        numericDamageText.color = numericHitColor;
                        numericDamageFeedTextMat.SetColor("_GlowColor", numericHitGlowColor);
                        m_crosshairHitImage.enabled = true;
                        m_crosshairHitImage.color = crosshairHitColor;
                        if (m_crosshairHitCo != null) StopCoroutine(m_crosshairHitCo);
                        m_crosshairHitCo = StartCoroutine(CoCrosshairHit());
                        break;
                    }
                case HitType.Heal:
                    {
                        numericDamageText.color = numericHealColor;
                        numericDamageFeedTextMat.SetColor("_GlowColor", numericHealGlowColor);
                        m_crosshairHitImage.enabled = true;
                        m_crosshairHitImage.color = crosshairHealColor;
                        if (m_crosshairHitCo != null) StopCoroutine(m_crosshairHitCo);
                        m_crosshairHitCo = StartCoroutine(CoCrosshairHit());
                        break;
                    }
                case HitType.HeadShot:
                    {
                        numericDamageText.color = numericHeadshotColor;
                        numericDamageFeedTextMat.SetColor("_GlowColor", numericHeadshotGlowColor);
                        m_crosshairHitImage.color = crosshairHeadshotColor;
                        m_crosshairHitImage.enabled = true;
                        if (m_crosshairHitCo != null) StopCoroutine(m_crosshairHitCo);
                        m_crosshairHitCo = StartCoroutine(CoCrosshairHit());
                        break;
                    }
             /*   default:
                    break;*/
            }
           
            StartCoroutine(CoDamageFeedBack(damage,hitted)); 
           // p.GetComponent<CharacterJoint>().enableProjection = true;
        }
        IEnumerator CoDamageFeedBack(float damage, GamePlayer hitted)
        {
           
            numericDamageText.text = Mathf.Abs(damage) + "";
            yield return null;
        
            RenderTexture.active = numericDamageFeedCam.targetTexture;
            width = numericDamageFeedCam.targetTexture.width;
            height = numericDamageFeedCam.targetTexture.height;
            texture = new Texture2D(width, height);
            texture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
            texture.Apply();
            ParticleSystemRenderer p = Instantiate(effect, hitted.numericDamageFeedbackPosition).GetComponent<ParticleSystemRenderer>();
            p.material = new Material(numericFeedMat);
            p.material.mainTexture = texture;
        }

        IEnumerator CoCrosshairHit()
        {
            /* yield return new WaitForSeconds(m_crosshairHitTime);
             m_crosshairHitImage.enabled=false;
             m_crosshairHitCo = null;*/
            float timer = 0f;
            while(crossHairHitScaleAnim.keys[crossHairHitScaleAnim.keys.Length - 1].time > timer)
            {
                m_crosshairHitImage.rectTransform.localScale = Vector3.one*crossHairHitScaleAnim.Evaluate(timer);
                yield return null;
                timer += Time.deltaTime;
            }
            //  crosshair.rectTransform.localScale = Vector3.one * crossHairHitScaleAnim.Evaluate(crossHairHitScaleAnim.keys[crossHairHitScaleAnim.keys.Length - 1].time);
            m_crosshairHitImage.enabled = false;
            m_crosshairHitCo = null;
        }



        // ENDGAME
        public void EnableEndgame(Team winnerTeam, bool matchEnd = true)
        {
            StartCoroutine(CoEnableEndgamePanel(winnerTeam, matchEnd));
        }

        IEnumerator CoEnableEndgamePanel(Team winnerTeam, bool matchEnd)
        {
            float timer = 0f;

            player.mainCamera.enabled = false;
            player.secondaryCamera.enabled = false;
            EndGameCamera titanCam = GameManager.instance.GetTitan(winnerTeam).GetComponent<EndGameCamera>();
            titanCam.startEndGameAnimation();
            roundBeginMessage = false;
            roundUltimates = false;
            while (timer < titanCam.AnimDuration)
            {
                yield return null;
                timer += Time.deltaTime;
            }

            lowerCanvasDisable.SetActive(false);
            timerCanvasDisable.SetActive(false);
            int maxRounds = Mathf.CeilToInt(GameManager.instance.maxRounds / 2f);
            int fontSize = (int)(allyRoundScore.fontSize * 0.5f);
            int allyScore = (player.Team == Team.A ? GameManager.instance.teamARounds : GameManager.instance.teamBRounds);
            int enemyScore = (player.Team != Team.A ? GameManager.instance.teamARounds : GameManager.instance.teamBRounds);
            if (maxRounds > 1)
            {
                allyRoundScore.text = allyScore.ToString("F0") + "<size=" + fontSize + ">/" + maxRounds + "</size>";
                enemyRoundScore.text = enemyScore.ToString("F0") + "<size=" + fontSize + ">/" + maxRounds + "</size>";
            }

            if (!matchEnd)
            {
                timer = 0f;
                showStats = true;
                if (!statsTab.activeSelf)
                {
                    statsTab.SetActive(true);
                }
                roundEndPanel.SetActive(true);
                roundResultText.text = (player.Team == winnerTeam ? textManager.roundWon : textManager.roundLost);
                while (timer < GameManager.instance.endRoundTime)
                {
                    yield return null;
                    roundRestartText.text = System.String.Format(textManager.restartingRound, Mathf.Ceil(GameManager.instance.endRoundTime - timer).ToString("F0"));
                    timer += Time.deltaTime;
                }
                showStats = false;
                if (statsTab.activeSelf)
                {
                    statsTab.SetActive(false);
                }
                roundEndPanel.SetActive(false);
                player.mainCamera.enabled = true;
                player.secondaryCamera.enabled = false;
                teamBaseAllyImage.fillAmount = 0f;
                teamBaseEnemyImage.fillAmount = 0f;
                teamBaseAllyBar.gameObject.SetActive(false);
                teamBaseEnemyBar.gameObject.SetActive(false);

                lowerCanvasDisable.SetActive(true);
                timerCanvasDisable.SetActive(true);
                yield break;
            }
            

            genAPos.gameObject.SetActive(false);
            genBPos.gameObject.SetActive(false);
            endGamePanel.SetActive(true);
            DisablePlayerPause();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            NetLobbyPlayer bestKiller = null;
            NetLobbyPlayer bestDamager = null;
            NetLobbyPlayer bestCarrier = null;

            //stats player
            foreach (NetLobbyPlayer nlp in NetLobbyManager.singleton.netLobbyPlayers)
            {
                if (nlp.team == NetLobbyPlayer.local.team)
                {
                    UIPlayerStats playerStats = Instantiate(playerStatsPrefab, allyTeamPanel.transform).GetComponent<UIPlayerStats>();

                    playerStats.SetLobbyPlayer(nlp);
                    //playerStats.SetLobbyPlayerStats(nlp);
                }
                else
                {
                    UIPlayerStats playerStats = Instantiate(playerStatsPrefab, enemyTeamPanel.transform).GetComponent<UIPlayerStats>();
                    playerStats.SetLobbyPlayer(nlp);
                    //playerStats.SetLobbyPlayerStats(nlp);
                }
                if (bestKiller == null)
                {
                    bestKiller = nlp;
                }
                else
                {
                    if(nlp.Kills > bestKiller.Kills)
                    {
                        bestKiller = nlp;
                    }
                }
                if (bestDamager == null)
                {
                    bestDamager = nlp;
                }
                else
                {
                    if (nlp.DamageDealt > bestDamager.DamageDealt)
                    {
                        bestDamager = nlp;
                    }
                }
                if (bestCarrier == null)
                {
                    bestCarrier = nlp;
                }
                else
                {
                    if (nlp.EnergyDelivered > bestCarrier.EnergyDelivered)
                    {
                        bestCarrier = nlp;
                    }
                }


            }


            bestKillerNameText.text = textManager.bestKillerMsg;
            //bestKillerNameText.text = "Best Killer";
            bestKillerIcon.sprite = bestKiller.seeker.seekerSprite;
            bestKillerText.text = string.Format(textManager.bestKillerStringMsg, bestKiller.playerName, bestKiller.Kills );
            //bestKillerText.text = bestKiller.playerName + "\n" + bestKiller.Kills + " Kills";

            bestDamagerNameText.text = textManager.bestDamagerMsg;
            //bestDamagerNameText.text = "Best Damager";            
            bestDamagerIcon.sprite = bestDamager.seeker.seekerSprite;
            bestDamagerText.text = string.Format(textManager.bestDamagerStringMsg, bestDamager.playerName, bestDamager.DamageDealt.ToString("F0"));
            //bestDamagerText.text = bestDamager.playerName + "\n" + bestDamager.DamageDealt.ToString("F0") + " damage";

            bestCarrierNameText.text = textManager.bestCarrierMsg;
            //bestCarrierNameText.text = "Best Carrier";
            bestCarrierIcon.sprite = bestCarrier.seeker.seekerSprite;
            bestCarrierText.text = string.Format(textManager.bestCarrierStringMsg, bestCarrier.playerName , bestCarrier.EnergyDelivered.ToString("F0"));
            //bestCarrierText.text = bestCarrier.playerName + "\n" + bestCarrier.EnergyDelivered.ToString("F0") + " Kor";
        }
             

        public void Endgame()
        {
            //Destroy(NetLobbyManager.singleton.gameObject);
            if(NetLobbyPlayer.local.isServer)
                NetLobbyManager.singleton.StopHost();
            else
                NetLobbyManager.singleton.StopClient();

        }

        public void AddMessage(string msg)
        {
            sentMessages.Enqueue(msg);
            while(sentMessages.Count > 10)
            {
                sentMessages.Dequeue();
            }
            messagesTextBox.text = "";
            foreach(string s in sentMessages)
            {
                messagesTextBox.text += "\n" + s ;
            }
        }

        public void AddScreenMessage(ScreenText msg)
        {
            switch(msg.type)
            {            
                case ScreenMsgType.GamePhases:
                {
                    if (msg.content == textManager.startMsg.content)
                    {
                        if (!roundBeginMessage)
                        {
                            roundBeginMessage = true;
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (msg.content == textManager.ultimateStartMsg.content)
                    {
                        if (!roundUltimates)
                        {
                            roundUltimates = true;
                        }
                        else
                        {
                            break;
                        }
                    }
                    m_gamePhases.Enqueue(msg);
                    Debug.Log("<color=#00FF00>Added</color> GamePhase message" + msg.content.Replace("<#", "<color=#"));
                    if (m_gamePhasesCo == null)
                    {
                        m_gamePhasesCo = StartCoroutine(CleanScreenMessageGamePhase());
                        Debug.Log("<color=#00FF00>Started</color> <color=#FFFF00>Game Phase Screen Coroutine</color>");
                    }
                    break;
                }
                case ScreenMsgType.EnergyLog:
                {
                    m_energyLog.Enqueue(msg);
                    Debug.Log("<color=#00FF00>Added</color> EnergyLog message" + msg.content.Replace("<#","<color=#"));
                    if (m_energyLogCo == null)
                    {
                        m_energyLogCo = StartCoroutine(CleanScreenMessageEnergyLog());
                        Debug.Log("<color=#00FF00>Started</color> <color=#FFFF00>EnergyLog Screen Coroutine</color>");
                    }
                    break;
                }
                case ScreenMsgType.GameOver:
                {
                    InstaCleanScreenMessage();
                    m_gameOver.Enqueue(msg);
                    Debug.Log("<color=#00FF00>Added</color> GameOver message" + msg.content.Replace("<#", "<color=#"));
                    if (m_gameOverCo == null)
                    {
                        m_gameOverCo = StartCoroutine(CleanScreenMessageGameOver());
                        Debug.Log("<color=#00FF00>Started</color> <color=#FFFF00>GameOver Screen Coroutine</color>");
                    }
                    break;
                }
                case ScreenMsgType.KillLog:
                {
                    m_killLog.Enqueue(msg);
                    Debug.Log("<color=#00FF00>Added</color> KillLog message" + msg.content.Replace("<#", "<color=#"));
                    if (m_killLogCo == null)
                    {
                        m_killLogCo = StartCoroutine(CleanScreenMessageKillLog());
                        Debug.Log("<color=#00FF00>Started</color> <color=#FFFF00>KillLog Screen Coroutine</color>");
                    }
                    break;
                }
                case ScreenMsgType.EventKillLog:
                {
                    m_eventKillLog.Enqueue(msg);
                    Debug.Log("<color=#00FF00>Added</color> EventKillLog message" + msg.content.Replace("<#", "<color=#"));
                    if (m_eventKillLogCo == null)
                    {
                        m_eventKillLogCo = StartCoroutine(CleanScreenMessageEventLogKill());
                        Debug.Log("<color=#00FF00>Started</color> <color=#FFFF00>EventKillLog Screen Coroutine</color>");
                    }
                    break;
                }

                case ScreenMsgType.AffectedStatus:
                {
                    m_affectedStatus.Enqueue(msg);
                    Debug.Log("<color=#00FF00>Added</color> AffectedStatus message" + msg.content.Replace("<#", "<color=#"));
                    if (m_affectedStatusCo == null)
                    {
                        m_affectedStatusCo = StartCoroutine(CleanScreenMessageAffectedStatus());
                        Debug.Log("<color=#00FF00>Started</color> <color=#FFFF00>AffectedStatus Screen Coroutine</color>");
                    }
                    break;
                }
            }       
           
        }

        public void InstaCleanScreenMessage(bool soundsToo = true)
        {
            //StopCoroutine(m_splashMessagesCo);
            //m_splashMessages = new Queue<ScreenText>();
            //m_splashScreenMessage.text = "";
            //m_splashMessagesCo = null;

            m_gamePhases = new Queue<ScreenText>();
            m_energyLog = new Queue<ScreenText>();
            m_eventKillLog = new Queue<ScreenText>();
            m_killLog = new Queue<ScreenText>();
            m_affectedStatus = new Queue<ScreenText>();

            //Stop all Co 
            if (m_gamePhasesCo != null) StopCoroutine(m_gamePhasesCo);
            if (m_energyLogCo != null) StopCoroutine(m_energyLogCo);
            if (m_eventKillLogCo != null) StopCoroutine(m_eventKillLogCo);
            if (m_killLogCo != null) StopCoroutine(m_killLogCo);
            if (m_affectedStatusCo != null) StopCoroutine(m_affectedStatusCo);
            if (m_gameOverCo != null) StopCoroutine(m_gameOverCo);

            m_gameOver = new Queue<ScreenText>();

            m_gamePhasesMessage.text = "";
            m_energyLogMessage.text = "";
            m_eventKillLogMessage.text = "";
            m_affectedStatusMessage.text = "";
            m_killLogMessage.text = "";
            m_gameOverMessage.text = "";

            m_gamePhasesCo = null;
            m_energyLogCo = null;
            m_eventKillLogCo = null;
            m_killLogCo = null;
            m_affectedStatusCo = null;
            m_announcerList = new Queue<float>();
            if(m_announcerQueueCo != null) StopCoroutine(m_announcerQueueCo);
            if(soundsToo) announcer.Stop();
            m_announcerQueueCo = null;
            m_gameOverCo = null;
            Debug.Log("<color=#FF0000>CLEANED SCREEN MESSAGES</color>");
        }

        //ciaoDan CO
        IEnumerator CleanScreenMessageEventLogKill()
        {
            while (m_eventKillLog.Count > 0)
            {
                ScreenText text = m_eventKillLog.Dequeue();
                m_eventKillLogMessage.text = text.content;
                m_eventKillLogMessage.transform.localScale = Vector3.one * text.animationCurve.Evaluate(0f);
                m_eventKillLogMessage.alpha = text.transparentCurve.Evaluate(0f);
                if (text.parameter != -1f)
                {
                    PlayAnnouncer(text.parameter);
                }
                float timer = 0f;
                while (timer < text.duration)
                {
                    yield return null;
                    m_eventKillLogMessage.transform.localScale = Vector3.one * text.animationCurve.Evaluate(timer / text.duration);
                    m_eventKillLogMessage.alpha = text.transparentCurve.Evaluate(timer / text.duration);
                    timer += Time.deltaTime;
                }
                //m_eventKillLogCo = StartCoroutine(CleanScreenMessageEventLogKill());
            }
            m_eventKillLogMessage.text = "";
            m_eventKillLogCo = null;
        }

        IEnumerator CleanScreenMessageGamePhase()
        {
            while (m_gamePhases.Count > 0)
            {
                ScreenText text = m_gamePhases.Dequeue();
                m_gamePhasesMessage.text = text.content;
                m_gamePhasesMessage.transform.localScale = Vector3.one * text.animationCurve.Evaluate(0f);
                m_gamePhasesMessage.alpha = text.transparentCurve.Evaluate(0f);
                if (text.parameter != -1f)
                {
                    PlayAnnouncer(text.parameter);
                }
                float timer = 0f;
                while(timer < text.duration)
                {
                    yield return null;                    
                    m_gamePhasesMessage.transform.localScale = Vector3.one * text.animationCurve.Evaluate(timer / text.duration);
                    m_gamePhasesMessage.alpha = text.transparentCurve.Evaluate(timer / text.duration);
                    timer += Time.deltaTime;
                }
                //yield return new WaitForSeconds(text.duration);
                //m_gamePhasesCo = StartCoroutine(CleanScreenMessageGamePhase());
            }
            m_gamePhasesMessage.text = "";
            m_gamePhasesCo = null;
        }
        IEnumerator CleanScreenMessageEnergyLog()
        {
            while (m_energyLog.Count > 0)
            {
                ScreenText text = m_energyLog.Dequeue();
                m_energyLogMessage.text = text.content;
                m_energyLogMessage.transform.localScale = Vector3.one * text.animationCurve.Evaluate(0f);
                m_energyLogMessage.alpha = text.transparentCurve.Evaluate(0f);
                if (text.parameter != -1f)
                {
                    PlayAnnouncer(text.parameter);
                }     
                float timer = 0f;
                while (timer < text.duration)
                {
                    yield return null;
                    m_energyLogMessage.transform.localScale = Vector3.one * text.animationCurve.Evaluate(timer / text.duration);
                    m_energyLogMessage.alpha = text.transparentCurve.Evaluate(timer / text.duration);
                    timer += Time.deltaTime;
                }
                //m_energyLogCo = StartCoroutine(CleanScreenMessageEnergyLog());
            }
            m_energyLogMessage.text = "";
            m_energyLogCo = null;
        }
        IEnumerator CleanScreenMessageGameOver()
        {
            while (m_gameOver.Count > 0)
            {                
                ScreenText text = m_gameOver.Dequeue();
                m_gameOverMessage.text = text.content;
                m_gameOverMessage.transform.localScale = Vector3.one * text.animationCurve.Evaluate(0f);
                m_gameOverMessage.alpha = text.transparentCurve.Evaluate(0f);
                if (text.parameter != -1f)
                {
                    PlayAnnouncer(text.parameter);
                }
                float timer = 0f;
                while (timer < text.duration)
                {
                    yield return null;
                    m_gameOverMessage.transform.localScale = Vector3.one * text.animationCurve.Evaluate(timer / text.duration);
                    m_gameOverMessage.alpha = text.transparentCurve.Evaluate(timer / text.duration);
                    timer += Time.deltaTime;
                }
                InstaCleanScreenMessage(false);
                //m_gameOverCo = StartCoroutine(CleanScreenMessageGameOver());
            }
            m_gameOverMessage.text = "";
            m_gameOverCo = null;
        }
        IEnumerator CleanScreenMessageKillLog()
        {
            while (m_killLog.Count > 0)
            {
                ScreenText text = m_killLog.Dequeue();
                m_killLogMessage.text = text.content;
                m_killLogMessage.transform.localScale = Vector3.one * text.animationCurve.Evaluate(0f);
                m_killLogMessage.alpha = text.transparentCurve.Evaluate(0f);
                if (text.parameter != -1f)
                {
                    PlayAnnouncer(text.parameter);
                }
                float timer = 0f;
                while (timer < text.duration)
                {
                    yield return null;
                    m_killLogMessage.transform.localScale = Vector3.one * text.animationCurve.Evaluate(timer / text.duration);
                    m_killLogMessage.alpha = text.transparentCurve.Evaluate(timer / text.duration);
                    timer += Time.deltaTime;
                }
                //m_killLogCo = StartCoroutine(CleanScreenMessageKillLog());
            }
            m_killLogMessage.text = "";
            m_killLogCo = null;
        }
        IEnumerator CleanScreenMessageAffectedStatus()
        {
            while (m_affectedStatus.Count > 0)
            {
                ScreenText text = m_affectedStatus.Dequeue();
                m_affectedStatusMessage.text = text.content;
                m_affectedStatusMessage.transform.localScale = Vector3.one * text.animationCurve.Evaluate(0f);
                m_affectedStatusMessage.alpha = text.transparentCurve.Evaluate(0f);
                if (text.parameter != -1f)
                {
                    PlayAnnouncer(text.parameter);
                }
                float timer = 0f;
                while (timer < text.duration)
                {
                    yield return null;
                    m_affectedStatusMessage.transform.localScale = Vector3.one * text.animationCurve.Evaluate(timer / text.duration);
                    m_affectedStatusMessage.alpha = text.transparentCurve.Evaluate(timer / text.duration);
                    timer += Time.deltaTime;
                }
                //m_affectedStatusCo = StartCoroutine(CleanScreenMessageAffectedStatus());
            }
            m_affectedStatusMessage.text = "";
            m_affectedStatusCo = null;
        }

        //IEnumerator CleanScreenMessage(float time = 2f)
        //{
        //    if (m_splashMessages.Count > 0)
        //    {
        //        ScreenText text = m_splashMessages.Dequeue();
        //        m_splashScreenMessage.text = text.content;
        //        if (text.parameter != -1f)
        //        {
        //            PlayAnnouncer(text.parameter);
        //        }
        //        yield return new WaitForSeconds(text.duration);
        //        m_splashMessagesCo = StartCoroutine(CleanScreenMessageGamePhase());
        //    }
        //    else
        //    {
        //        m_splashScreenMessage.text = "";
        //        m_splashMessagesCo = null;
        //    }

        //}

        IEnumerator ResetFeedback()
        {
            float timer = 0f;
            Color hurtC = feedbackHurtColor;
            while (timer < timeFeedbackDuration)
            {
                yield return null;
                feedbackDamage.GetComponent<Image>().color = new Color(hurtC.r, hurtC.g, hurtC.b, hurtC.a * (timeFeedbackDuration - timer) / timeFeedbackDuration);
                timer += Time.deltaTime;
            }
            feedbackDamage.gameObject.SetActive(false);
            feedbackCo = null;
        }

        IEnumerator CoWaitLocalPlayer()
        {
            yield return new WaitUntil(() => GamePlayer.local != null && GamePlayer.local.LobbyPlayer != null);
            m_showUI = true;
            Seeker s = GamePlayer.local.LobbyPlayer.seeker;
            baseAbility.sprite = s.abilities[(int)AbilityType.Base].abilitySprite;
            movementAbility.sprite = s.abilities[(int)AbilityType.Movement].abilitySprite;
            firstAbility.sprite = s.abilities[(int)AbilityType.First].abilitySprite;
            secondAbility.sprite = s.abilities[(int)AbilityType.Second].abilitySprite;
            ultimateAbility.sprite = s.abilities[(int)AbilityType.Ultimate].abilitySprite;

        }

        // Feedbacks

        IEnumerator CoHealFeedback()
        {
            float timer = 0f;
            float speed = 0.1f;
            float tspeed = 0f;
            while (timer < m_healFeedbackTime)
            {
                if (tspeed > speed)
                {
                    GameObject healObj = Instantiate(m_healFeedbackPrefab, m_feedbacksPanel.transform);
                    healObj.transform.position = new Vector3(Random.value * Screen.width, Random.value * Screen.height, 0f);
                    tspeed = 0f;
                }
                yield return null;
                timer += Time.deltaTime;
                tspeed += Time.deltaTime;

            }
            m_healFeedbackCo = null;

        }


       
       
        //Enable Player Controls - Pause
        public void EnablePlayerPause()
        {
            isPause = true;
            pausePanel.SetActive(true);
            mainPauseMenu.SetActive(true);
            optionPauseMenu.SetActive(false);
            exitPauseMenu.SetActive(false);
            buttonsMainPause.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void DisablePlayerPause()
        {
            isPause = false;
            pausePanel.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }


        // Enable an ui gameobject(panel) and disable all siblings
        public void EnablePanel(GameObject panel)
        {
            for (int i = 0; i < panel.transform.parent.childCount; i++)
            {
                panel.transform.parent.GetChild(i).gameObject.SetActive(false);
            }
            panel?.SetActive(true);
        }

        // Disable an ui gameobject(panel) and disable all siblings
        public void DisablePanel(GameObject panel)
        {
            panel?.SetActive(false);
        }

        //Enable an ui gameobject(panel) 
        public void OnlyEnablePanel(GameObject panel)
        {
            panel?.SetActive(true);
        }

        public void CreateStatsPanel()
        {
            foreach (NetLobbyPlayer nlp in NetLobbyManager.singleton.netLobbyPlayers)
            {
                Transform teamPanel = (nlp.team == NetLobbyPlayer.local.team ? InGameAllyTeamPanel.transform : InGameEnemyTeamPanel.transform);
                GameObject g = Instantiate(inGameStatsPrefab, teamPanel);
                g.GetComponent<UIPlayerStats>().SetLobbyPlayer(nlp);
            }
        }

        public void ChangeMissionDirection()
        {
            Vector3 dir = targetTitan.position - player.transform.position;
            dir.y = 0f;
            titanArrow.localRotation = Quaternion.Euler(Vector3.forward * (Vector3.SignedAngle(dir,player.transform.forward,Vector3.up))); // missionDirection * Quaternion.Euler(dir);
        }
        public void StartAbilityIconCooldown(AbilityType a, bool showNumber = true)
        {          
            switch (a)
            {
                case AbilityType.Base:
                {
                    //StartBaseAttackCooldown((int)AbilityType.Base);
                    break;
                }
                case AbilityType.Movement:
                    if(abilitiesCorutines[0]==null)
                    {
                        abilitiesCorutines[0] = StartCoroutine(CoAbilityIcon(movementAbilityIconY, movementAbility, (int)a, movementAbilityText));
                    }
                    else
                    {
                        StopCoroutine(abilitiesCorutines[0]);
                        abilitiesCorutines[0] = StartCoroutine(CoAbilityIcon(movementAbilityIconY, movementAbility, (int)a, movementAbilityText));
                    }
                    break;
                case AbilityType.First:
                    if (abilitiesCorutines[1] == null)
                    {
                        abilitiesCorutines[1] = StartCoroutine(CoAbilityIcon(firstAbilityIconY,firstAbility, (int)a, firstAbilityText));
                    }
                    else
                    {
                        StopCoroutine(abilitiesCorutines[1]);
                        abilitiesCorutines[1] = StartCoroutine(CoAbilityIcon(firstAbilityIconY, firstAbility, (int)a, firstAbilityText));
                    }
                    break;
                case AbilityType.Second:
                    if (abilitiesCorutines[2] == null)
                    {
                        abilitiesCorutines[2] = StartCoroutine(CoAbilityIcon(secondAbilityIconY,secondAbility, (int)a, secondAbilityText));
                    }
                    else
                    {
                        StopCoroutine(abilitiesCorutines[2]);
                        abilitiesCorutines[2] = StartCoroutine(CoAbilityIcon(secondAbilityIconY, secondAbility, (int)a, secondAbilityText));
                    }
                    break;
                case AbilityType.Ultimate:
                {
                    if (abilitiesCorutines[3] != null)
                    {
                        StopCoroutine(abilitiesCorutines[3]);
                    }
                    abilitiesCorutines[3] = StartCoroutine(CoAbilityIcon(ultimateAbilityIconY, ultimateAbility, (int)a, ultimateAbilityText));
                    break;
                }
                case AbilityType.FirstSecondary:
                {
                    if (abilitiesCorutines[6] == null)
                    {
                        abilitiesCorutines[6] = StartCoroutine(CoAbilityIcon(firstAbilityIconY, firstAbility, (int)a, firstAbilityText));
                    }
                    else
                    {
                        StopCoroutine(abilitiesCorutines[6]);
                        abilitiesCorutines[6] = StartCoroutine(CoAbilityIcon(firstAbilityIconY, firstAbility, (int)a, firstAbilityText));
                    }
                    break;
                }
                default:
                    
                    break;
            }


        }
        IEnumerator CoAbilityIcon( float yToStart,Image abilityImage, int ability, TextMeshProUGUI cooldownText)
        {
            yield return new WaitUntil(()=> player.abilitiesCooldowns[ability] != 0f);
            if (abilityImage.GetComponentInChildren<ParticleSystem>()) abilityImage.GetComponentInChildren<ParticleSystem>().Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            while (abilityImage.rectTransform.anchoredPosition.y >= 0.001)
            {
                cooldownText.text = Mathf.Abs(player.abilitiesCooldowns[ability]).ToString("0");
                if (player.abilitiesCooldowns[ability] == 0f)
                {
                    break;
                }
                abilityImage.rectTransform.anchoredPosition = new Vector2(abilityImage.rectTransform.anchoredPosition.x, Mathf.Lerp(abilityImage.rectTransform.anchoredPosition.y, 0, 0.1f));
                yield return null;
            }
            //Debug.Log("primo..............");
            abilityImage.rectTransform.anchoredPosition = new Vector2(abilityImage.rectTransform.anchoredPosition.x, 0);
            while (player.abilitiesCooldowns[ability] > 0f)
            {
                cooldownText.text = Mathf.Abs(player.abilitiesCooldowns[ability]).ToString("0");
              
                yield return null;
            }
            cooldownText.text = "";
            if (ability == (int)AbilityType.Ultimate) player.PlaySound(player.soundReadyUltimate);
            else if (ability != (int)AbilityType.Base) player.PlaySound(player.soundReadyAbility);
            //Debug.Log("secondo..............");
            if (abilityImage.GetComponentInChildren<ParticleSystem>()) abilityImage.GetComponentInChildren<ParticleSystem>().Play(true);
            while (Mathf.Abs(abilityImage.rectTransform.anchoredPosition.y -yToStart)>=0.001f)
            {
                abilityImage.rectTransform.anchoredPosition = new Vector2(abilityImage.rectTransform.anchoredPosition.x, Mathf.Lerp(abilityImage.rectTransform.anchoredPosition.y, yToStart, 0.1f));
                yield return null;
            }
            //Debug.Log("terzo..............");
            abilityImage.rectTransform.anchoredPosition = new Vector2(abilityImage.rectTransform.anchoredPosition.x, yToStart);
            
        }
        IEnumerator CoLandingShake()
        {
            yield return new WaitUntil(() => player == null || player.IsGrounded());
            lowerUiPanel.rectTransform.anchoredPosition = (new Vector2(lowerUiPanel.rectTransform.anchoredPosition.x, startLowerUiY));
         
            while (Mathf.Abs(lowerUiPanel.rectTransform.anchoredPosition.y - yLowerUIToGo) >= 0.1)
            {
      
                lowerUiPanel.rectTransform.anchoredPosition = (new Vector2(lowerUiPanel.rectTransform.anchoredPosition.x, Mathf.Lerp(lowerUiPanel.rectTransform.anchoredPosition.y, yLowerUIToGo, speedDown)));
                yield return null;
            }
          
            lowerUiPanel.rectTransform.anchoredPosition = (new Vector2(lowerUiPanel.rectTransform.anchoredPosition.x, yLowerUIToGo));
            while (Mathf.Abs(lowerUiPanel.rectTransform.anchoredPosition.y - startLowerUiY)>=0.1)
            {
                lowerUiPanel.rectTransform.anchoredPosition = (new Vector2(lowerUiPanel.rectTransform.anchoredPosition.x, Mathf.Lerp(lowerUiPanel.rectTransform.anchoredPosition.y, startLowerUiY, speedUp)));
                yield return null;
            }
         
            lowerUiPanel.rectTransform.anchoredPosition = (new Vector2(lowerUiPanel.rectTransform.anchoredPosition.x, startLowerUiY));
            waitLanding = null;
        }
        public void StartReload()
        {
            StartCoroutine(Reload(player.LobbyPlayer.seeker.abilities[(int)AbilityType.Base].reloadCooldown));
        }
        public void StartCrosshairShake(AnimationCurve yCurve,AnimationCurve xCurve)
        {
            StartCoroutine(CoIconAnimation(crosshair, crosshairStartY, crosshairStartX, yCurve, xCurve));
        }
        IEnumerator CoIconAnimation(Image targetImage,float startY,float startX,AnimationCurve yCurve, AnimationCurve xCurve)
        {           
          
            float timer = 0f;
            while (yCurve.keys[yCurve.keys.Length-1].time>timer)
            {
                targetImage.rectTransform.anchoredPosition = (new Vector2(startX + xCurve.Evaluate(timer), startY + yCurve.Evaluate(timer)));
                yield return null;
                timer += Time.deltaTime;
                
            }
            targetImage.rectTransform.anchoredPosition = (new Vector2(startX + xCurve.Evaluate(xCurve.keys[xCurve.keys.Length - 1].time), startY + yCurve.Evaluate(yCurve.keys[yCurve.keys.Length - 1].time)));
            if(targetImage == titanCompass)
            {
                compasAnim = null;
            }
        }
        IEnumerator setupRoundCounter()
        {
            yield return new WaitUntil(() => GameManager.instance.maxRounds != 0);
            if (GameManager.instance.maxRounds == 1)
            {
                allyRoundScore.text = "";
                enemyRoundScore.text = "";
            }
            else
            {
                int fontSize = (int)(allyRoundScore.fontSize * 0.5f);
                Debug.Log(GameManager.instance.maxRounds);
                allyRoundScore.text = "0<size=" + fontSize + ">/" + Mathf.CeilToInt(GameManager.instance.maxRounds / 2f) + "</size>";
                enemyRoundScore.text = "0<size=" + fontSize + ">/" + Mathf.CeilToInt(GameManager.instance.maxRounds / 2f) + "</size>";
            }

        }
        IEnumerator Reload(float time)
        {
            //Debug.Log("start Reload");
            reloadIcon.enabled =true;
            reloadIcon.fillAmount = 0;
            float timer = 0;
            while(timer<time)
            {
                yield return null;
                timer += Time.deltaTime;
                reloadIcon.fillAmount = timer / time;
               
            }
            reloadIcon.fillAmount =1f;
            reloadIcon.enabled = false;
           // Debug.Log("end Reload");
        }
    }

}
