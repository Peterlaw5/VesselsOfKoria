using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using FMODUnity;
using UnityEngine.UI;

namespace VoK
{
    public class GamePlayer : NetBehaviour
    {


        public static List<GamePlayer> allPlayers = new List<GamePlayer>();
        public event Action<GamePlayer> OnPlayerDeath;
        public GameObject hud;
        public GameObject damageInputPoint;
        public GameObject damageInput;
        #region Fields

        // NetLobbyPlayer
        private NetLobbyPlayer m_lobbyPlayer;
        public NetLobbyPlayer LobbyPlayer { get { return m_lobbyPlayer; } set { m_lobbyPlayer = value; } }
        public Team Team { get { return LobbyPlayer.team; } }
        private TransformInfo m_lastSpawn;
        public TransformInfo LastSpawn { get { return m_lastSpawn; } set { m_lastSpawn = value; } }

        [Header("Player state")]
        [SerializeField]
        private bool m_isDead;
        public bool IsDead { get { return m_isDead; } }

        [SerializeField]
        private float m_health;
        public float Health { get { return m_health; } set { m_health = value; } }

        [SerializeField]
        private bool m_isAffectedByGravity;
        public bool IsAffectedByGravity { get { return m_isAffectedByGravity; } }
        [SerializeField]
        private bool m_isAffectedByForces;
        public bool IsAffectedByForces { get { return m_isAffectedByForces; } }

        [SerializeField]
        private float m_energyCarried;
        public float Energy { get { return m_energyCarried; } }

        public bool m_canInput;
        public bool m_canAttack;
        public bool m_canUseMobility;
        public bool m_isInvulnerable;
        [HideInInspector]
        public bool m_canGetEnergy;
        private bool m_canInputMouse;
        public bool CanRotateVisual { get { return m_canInputMouse; } set { m_canInputMouse = value; } }

        [Header("Player Layers")]
        [Tooltip("Base player layer mask")]
        public LayerMask baseLayer;
        [Tooltip("Layer mask for aim raycast")]
        public LayerMask aimableLayer;
        [Tooltip("Layer mask to check where jumping is allowed")]
        public LayerMask walkableLayer;
        public LayerMask notTeamLayer;
        public LayerMask temporaryGhostLayer;
        [Tooltip("Team A layer reference")]
        public LayerMask teamALayer;
        [Tooltip("Team B layer reference")]
        public LayerMask teamBLayer;
        [HideInInspector]
        [Tooltip("Jump force overall multiplier. It's multiplied for the seeker's base jump force")]
        public float m_jumpMultiplier = 1f;
        [HideInInspector]
        [Tooltip("Movement speed overall multiplier. It's multiplied for the seeker's base speed")]
        public float m_speedMultiplier = 1f;
        [HideInInspector]
        public float m_speedLaneMult = 1f;
        [HideInInspector]
        [Tooltip("Damage reduction multiplier")]
        public float m_damageReductionMultiplier = 1f;
        [Range(0f, 1f)]
        [HideInInspector]
        [Tooltip("Cooldown overall multiplier, used for cdr")]
        public float m_cooldownMultiplier = 1f;
        [Header("Player Tunable settings")]
        [Tooltip("Time to start ultimate cooldown at match start")]
        public float m_ultimateStartCooldown = 240f;
        [Tooltip("Movement speed multiplier over speed lane. It's multiplied for the player speed")]
        public float m_speedLaneMultiplier = 1f;
        [Tooltip("Time to respawn after death")]
        public float m_respawnTime = 5f;
        [Tooltip("Time to respawn after death when your titan has ended its path")]
        public float m_endPhaseRespawnTime = 10f;
        [Tooltip("Invulnerability time after spawn")]
        public float m_antiSpawnKillTime = 3f;
        [Tooltip("Same team color")]
        public Color allyColor = Color.blue;
        [Tooltip("Other team color")]
        public Color enemyColor = Color.red;
        [Range(0f, 1f)]
        public float m_linearDrag = 0.95f;
        [HideInInspector]
        [Tooltip("Last player who hit this player")]
        public GamePlayer m_lastDamager;
        [HideInInspector]
        [Tooltip("Last player who healed this player")]
        public GamePlayer m_lastHealer;
        [HideInInspector]
        [Tooltip("The list of the players who caused damage in the last <Assist Timer> seconds")]
        public List<GamePlayer> m_lastDamagersList;
        [Tooltip("The time to consider a player in assist for a kill")]
        public float m_assistTimer = 5f;
        [Range(0f, 1f)]
        [Tooltip("Critical Health level in %")]
        public float m_criticalHealthLevel = 0.25f;
        [Range(0f, 1f)]
        public float spineRotationFactor = 1f;
        //[SerializeField]
        Color myColor;

        public Seeker Seeker { get { return m_lobbyPlayer.seeker; } }
        [Header("Object references - Auto assigned")]
        public Animator m_serverSeekerAnimator;
        public Animator m_clientSeekerAnimator;
        public Ability[] Abilities { get { return m_lobbyPlayer.seeker.abilities; } }
        public List<float> abilitiesCooldowns;
        [Header("Object references - To assign")]
        public Camera mainCamera;
        GamePlayerCamera mainCameraScript;
        public Camera secondaryCamera;
        //public Transform m_attackSpawnPoint;
        public Transform[] m_attackSpawnPoints;
        public Transform[] AttackSpawnPoints { get { return m_attackSpawnPoints; } }
        Transform[] m_modelBodySpines;
        Transform[] m_modelBodySpinesOutsideAnimator;
        public float m_modelBodySpineRotation;
        public Transform m_modelBody;
        public Transform m_modelBodyArms;
        public Transform numericDamageFeedbackPosition;
        public GameObject m_energyDroppedPrefab;
        //public SpriteRenderer lifeBarRenderer;
        //public SpriteRenderer lifeBarFullRenderer;
        public Image lifeBarRenderer;
        public Image lifeBarFullRenderer;
        public SpriteRenderer markerRenderer;
        public TextMeshPro playerNameText;

        public Material invisibleMat;

        public GameObject m_serverSidedModel;
        public GameObject m_clientSidedModel;


        [Header("Player Materials")]
        public Material AllyOutline;
        public Material EnemyOutline;
        public Material AllyLifebar;
        public Material AllyNameText;
        private Outline outline;
        // Components
        [Header("Player particle DebufEffects")]
        public ParticleSystem stunnedParticle;
        public ParticleSystem slowedParticle;
        public ParticleSystem vayvinMark;
        public ParticleSystem snaredParticles;

        [Header("Player status icon")]
        public GameObject criticalIcon;
        /*public GameObject slowedIcon;
        public GameObject snaredIcon;
        public GameObject markedIcon;
        public GameObject stunnedIcon;*/

        Rigidbody m_body;
        NetTransform[] m_netTransforms;
        //StudioEventEmitter playerSoundEmitter;
        [Header("Sounds")]
        public List<StudioEventEmitter> playerSoundEmitters;
        List<StudioEventEmitter> tmpSoundEmitters;
        [EventRef]
        public string soundReadyUltimate;
        [EventRef]
        public string soundReadyAbility;
        [EventRef]
        public string soundNotReadyAbility;
        [EventRef]
        public string soundGetHealed;
        [EventRef]
        public string soundEnergyDelivery;
        [EventRef]
        public string soundEnergyNoDelivery;
        [Header("Player Colliders")]
        public CapsuleCollider m_collider;
        public SphereCollider m_headCollider;
        Collider[] m_serverColliders;
        List<Vector3> m_serverBonesLocalPos;
        List<Vector3> m_serverBonesLocalRot;
        GamePlayerHead m_serverPlayerHead;

        [Header("Player Effects")]
        public ParticleSystem giveEnergyEffect;
        public TrailRenderer trail;
        public LineRenderer deathRay;
        public float deathRayLength;

        [Header("Death Camera")]
        public Transform cameraPosition;

        public IInputActor m_inputManager;
        public RuntimeAnimatorController m_baseAnimator;

        public Material enemyLifebarMaterial;
        // Find localPlayer easily
        public static GamePlayer local;

        Coroutine netTransformSync;
        Coroutine gameTitanCo;
        Coroutine ultimateUnlockCo;
        List<Coroutine> destroyOnDeathCos;
        GameObject myTeamTitan = null;
        int basicAttackAmmo = 0;
        public int BaseAbilityAmmo { get { return basicAttackAmmo; } }

        List<AbilityType[]> abilityRedirection;

        int lastPlayerHitID = -1;
        [HideInInspector]
        public float startingFOV = 60f;

        //RaycastHit groundHit;
        bool lastFrameGrounded;
        bool isWalking = false;
        float m_airTime = 0f;
        #endregion

        private GameObject zoomMask;
        public float showHpTime;
        private Coroutine ShowHp;

        private int multikillCount;
        public float multikillTime;
        #region Methods

        // When the game object is created, before the first Update()
        private void Start()
        {
            // Add player to players static list
            allPlayers.Add(this);

            // GamePlayer Default values
            m_netTransforms = GetComponents<NetTransform>();
            mainCameraScript = mainCamera.GetComponent<GamePlayerCamera>();
            //playerSoundEmitter = GetComponent<StudioEventEmitter>();

            abilitiesCooldowns = new List<float>();
            destroyOnDeathCos = new List<Coroutine>();
            abilityRedirection = new List<AbilityType[]>();
            m_lastDamagersList = new List<GamePlayer>();
            tmpSoundEmitters = new List<StudioEventEmitter>();

            m_canGetEnergy = true;
            m_canUseMobility = true;
            m_isAffectedByGravity = true;
            m_isAffectedByForces = false;
            lastFrameGrounded = false;
            m_canInput = false;
            m_canAttack = false;
            m_canInputMouse = false;
            m_canInput = false;
            m_canAttack = false;
            m_canInputMouse = false;
            m_isInvulnerable = false;
            m_isDead = true;
            isWalking = false;

            multikillCount = 0;
            m_energyCarried = 0f;
            m_modelBodySpineRotation = 0f;
            m_airTime = 0f;

            gameTitanCo = null;
            m_lastDamager = null;
            m_lastHealer = null;
            m_serverPlayerHead = null;
            m_serverColliders = null;
            m_serverBonesLocalPos = null;
            m_serverBonesLocalRot = null;
            ultimateUnlockCo = null;
            startingFOV = mainCamera.fieldOfView;

            StopAllParticles();

            // Local player initialization
            if (isLocalPlayer)
            {
                local = this;
                myColor = allyColor;

                // Set Input Manager for human player, eventually will change
                m_inputManager = GetComponent<IInputActor>();
                StartCoroutine(CoWaitPlayersLoaded());
            }

            // Server initialization
            if (isServer)
            {
                OnPlayerDeath += PlayerDeathServer;
                GameManager.instance.OnRoundStart += StartRound;
                StartCoroutine(CoCheckBoundaries());
            }
        }

        private void OnDestroy()
        {
            if (isServer)
            {
                OnPlayerDeath -= PlayerDeathServer;
            }
            allPlayers.Remove(this);
        }

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();

            // Turn off main camera because GamePlayer prefab has its own camera
            mainCamera.enabled = true;
            Camera.main.enabled = false;

            // Add rigidbody only local player 
            m_body = gameObject.AddComponent<Rigidbody>();
            m_body.useGravity = false;
            m_body.freezeRotation = true;
            m_body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        private void Update()
        {
            if (GameManager.instance.isRoundOver)
            {
                if (stunnedParticle.isPlaying)
                {
                    stunnedParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
                if (slowedParticle.transform.parent.gameObject.activeSelf == true)
                {
                    slowedParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    slowedParticle.transform.parent.gameObject.SetActive(false);
                }
                if (snaredParticles)
                {
                    snaredParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
                if (isLocalPlayer)
                {
                    foreach (StudioEventEmitter see in playerSoundEmitters)
                    {
                        if (see.IsPlaying())
                        {
                            see.Stop();
                        }
                    }
                }
            }
            if (!m_canInputMouse && !m_canInput && stunnedParticle.isStopped/*stunnedIcon.activeSelf==false*/ && !GameManager.instance.isRoundOver && !m_isDead)
            {
                Debug.Log("Stun Particle Feedback ON");
                stunnedParticle.Play(true);
                //stunnedIcon.SetActive(true);
            }
            else if (m_canInputMouse && m_canInput && /*stunnedIcon.activeSelf== true*/stunnedParticle.isPlaying)
            {
                stunnedParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                //stunnedIcon.SetActive(false);
            }

            if (m_speedMultiplier < 1 && m_speedMultiplier > 0 && slowedParticle.transform.parent.gameObject.activeSelf == false/*slowedParticle.isStopped */&& !GameManager.instance.isRoundOver)
            {

                //slowedIcon.SetActive(true);
                slowedParticle.transform.parent.gameObject.SetActive(true);
                slowedParticle.Play(true);

            }
            else if (m_speedMultiplier >= 1f && slowedParticle.transform.parent.gameObject.activeSelf == true/*slowedParticle.isPlaying*/)
            {
                slowedParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                slowedParticle.transform.parent.gameObject.SetActive(false);
                //slowedIcon.SetActive(false);
            }

            if (m_speedMultiplier == 0 && /*snaredIcon.activeSelf == false*/snaredParticles.isStopped && !GameManager.instance.isRoundOver)
            {
                // snaredIcon.SetActive(true);
                snaredParticles.Play(true);
            }
            else if (m_speedMultiplier != 0 && /*snaredIcon.activeSelf == true*/snaredParticles.isPlaying)
            {
                snaredParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                //snaredIcon.SetActive(false);
            }

            if (!trail.emitting && m_energyCarried > 0 && !isLocalPlayer)
            {
                trail.emitting = true;
            }
            else if (m_energyCarried == 0 && trail.emitting)
            {
                trail.emitting = false;
            }

            if (isServer)
            {
                if (CheckCooldown(AbilityType.Passive))
                {
                    foreach (Effect e in LobbyPlayer.seeker.abilities[(int)AbilityType.Passive].effects)
                    {
                        if (e.damageReduction != 1f && e.healthLevelReduction >= m_health / Seeker.maxHealth)
                        {
                            m_damageReductionMultiplier = e.damageReduction;
                            NetCmdSendMessage(NetMsg.CastAbility, (int)AbilityType.Passive, 0f, 0f, 0f);
                        }
                    }
                }
            }

            // Local client only
            if (!isLocalPlayer) return;
            if (!m_canInput) return;
            if (!m_canAttack) return;
            if (UIGame.instance.isPause) return;
            if (Input.GetKeyDown(KeyCode.R) && CheckCooldown(AbilityType.Base) && Abilities[(int)AbilityType.Base].maxAmmo > 0 && Abilities[(int)AbilityType.Base].maxAmmo != basicAttackAmmo)
            {
                NetCmdSendMessage(NetMsg.Reload);
            }
            if (m_inputManager.GetKey(InputKey.BaseShoot))
            {
                Ray cameraRay = mainCamera.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f));
                Vector3 pointToShoot = cameraRay.direction.normalized;
                NetCmdSendMessage(NetMsg.CastAbility, (int)AbilityType.Base, pointToShoot.x, pointToShoot.y, pointToShoot.z);
            }
            else
            {
                NetCmdSendMessage(NetMsg.Animation, "b", "attack", false);
            }
            if (m_inputManager.GetKeyDown(InputKey.Dash) && !CheckCooldown(AbilityType.Movement))
            {
                PlaySound(soundNotReadyAbility, "", true);
            }
            if (m_inputManager.GetKey(InputKey.Dash))
            {
                NetCmdSendMessage(NetMsg.CastAbility, (int)AbilityType.Movement, 0f, 0f, 0f);
            }
            if (m_inputManager.GetKeyDown(InputKey.FirstShoot) && !CheckCooldown(AbilityType.First))
            {
                PlaySound(soundNotReadyAbility, "", true);
            }
            if (m_inputManager.GetKey(InputKey.FirstShoot))
            {
                Ray cameraRay = mainCamera.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f));
                Vector3 pointToShoot = cameraRay.direction.normalized;
                NetCmdSendMessage(NetMsg.CastAbility, (int)AbilityType.First, pointToShoot.x, pointToShoot.y, pointToShoot.z);
                if (Abilities[(int)AbilityType.First].applyZoom && mainCamera.fieldOfView != startingFOV / Abilities[(int)AbilityType.First].zoomMult)
                {
                    mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, startingFOV / Abilities[(int)AbilityType.First].zoomMult, Time.deltaTime * Abilities[(int)AbilityType.First].zoomSpeed);
                    damageInputPoint.transform.localScale = Vector3.one*startingFOV/Abilities[(int)AbilityType.First].zoomMult ;
                    if (mainCamera.fieldOfView - startingFOV / Abilities[(int)AbilityType.First].zoomMult <= 0.1f)
                    {
                        mainCamera.fieldOfView = startingFOV / Abilities[(int)AbilityType.First].zoomMult;
                    }
                    if (zoomMask == null)
                    {
                        zoomMask = Instantiate(Abilities[(int)AbilityType.First].zoomMask, UIGame.instance.effectsSpawnPoint.transform);
                    }
                }
            }
            if (m_inputManager.GetKeyUp(InputKey.FirstShoot) && mainCamera.fieldOfView != startingFOV)
            {
                mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, startingFOV, Time.deltaTime * Abilities[(int)AbilityType.First].zoomSpeed);
                damageInputPoint.transform.localScale= Vector3.one;
                if (mainCamera.fieldOfView - startingFOV <= 0.1f)
                {
                    mainCamera.fieldOfView = startingFOV;

                }
                foreach (Effect e in Abilities[(int)AbilityType.First].effects)
                {
                    if (e.redirectedAbility != AbilityType.None)
                    {
                        NetCmdSendMessage(NetMsg.RedirectionClear, (int)e.redirectedAbility);
                    }
                }
                Destroy(zoomMask);
                zoomMask = null;
            }

            if (m_inputManager.GetKeyDown(InputKey.SecondShoot) && !CheckCooldown(AbilityType.Second))
            {
                PlaySound(soundNotReadyAbility, "", true);
            }
            if (m_inputManager.GetKey(InputKey.SecondShoot))// && CheckCooldown(AbilityType.Second))
            {
                Ray cameraRay = mainCamera.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f));
                Vector3 pointToShoot = cameraRay.direction.normalized;
                NetCmdSendMessage(NetMsg.CastAbility, (int)AbilityType.Second, pointToShoot.x, pointToShoot.y, pointToShoot.z);
                // Set Trigger Ability
                //NetCmdSendMessage(NetMsg.Animation, "t", "ability");
            }

            if (m_inputManager.GetKeyDown(InputKey.UltimateShoot) && !CheckCooldown(AbilityType.Ultimate))
            {
                PlaySound(soundNotReadyAbility, "", true);
            }
            if (m_inputManager.GetKey(InputKey.UltimateShoot))// && CheckCooldown(AbilityType.Second))
            {
                Ray cameraRay = mainCamera.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f));
                Vector3 pointToShoot = cameraRay.direction.normalized;
                NetCmdSendMessage(NetMsg.CastAbility, (int)AbilityType.Ultimate, pointToShoot.x, pointToShoot.y, pointToShoot.z);
                // Set Trigger Ability
                //NetCmdSendMessage(NetMsg.Animation, "t", "ability");
            }

        }

        private void FixedUpdate()
        {
            if (!isLocalPlayer) return;

            if (LobbyPlayer == null) return;

            float verticalInput = m_inputManager.GetAxis(InputKey.Vertical);
            float horizontalInput = m_inputManager.GetAxis(InputKey.Horizontal);
            bool jumpInput = m_inputManager.GetKey(InputKey.Jump);

            if (!m_canInput || UIGame.instance.isPause)
            {
                verticalInput = 0f;
                horizontalInput = 0f;
                jumpInput = false;
                // Set Speed 0
                NetCmdSendMessage(NetMsg.Animation, "f", "speed", 0);
                NetCmdSendMessage(NetMsg.Animation, "f", "sidewalk", 0);
                //PET//SeekerAnimator.SetFloat("speed", 0);
            }

            Vector3 velocity = Vector3.zero;
            velocity.z = verticalInput;
            velocity.x = horizontalInput;
            velocity = velocity.normalized * Seeker.baseSpeed * m_speedMultiplier * m_speedLaneMult;
            NetCmdSendMessage(NetMsg.Animation, "f", "speed", verticalInput);
            NetCmdSendMessage(NetMsg.Animation, "f", "sidewalk", -horizontalInput);
            if (!m_isDead)
            {
                bool grounded = IsGrounded();
                if (!isWalking)
                {
                    if ((Mathf.Abs(verticalInput) + Mathf.Abs(horizontalInput)) > 0.01f && grounded)
                    {
                        isWalking = true;
                        NetCmdSendMessage(NetMsg.PlaySoundAt, (int)SoundEmitterType.Walk, true);
                    }
                }
                else
                {
                    if (!grounded || (Mathf.Abs(verticalInput) + Mathf.Abs(horizontalInput)) <= 0.01f)
                    {
                        isWalking = false;
                        NetCmdSendMessage(NetMsg.PlaySoundAt, (int)SoundEmitterType.Walk, false);
                    }
                }
            }
            //PET// SeekerAnimator.SetFloat("speed", Mathf.Clamp01(velocity.magnitude));
            NetCmdSendMessage(NetMsg.Update, (int)GamePlayerState.BodyRotation, mainCameraScript.Angle);


            // When on ground
            if (IsGrounded())
            {
                NetCmdSendMessage(NetMsg.Animation, "b", "onground", true);
                
                // Jump
                if (jumpInput)
                {
                    velocity.y = Seeker.jumpForce * m_jumpMultiplier;
                    // set bool isgrounded false
                    NetCmdSendMessage(NetMsg.Animation, "b", "onground", false);
                    //PET// SeekerAnimator.SetBool("onground", false);
                    lastFrameGrounded = false;
                    PlaySound(Seeker.jumpSound, SerializeMessage(Seeker.jumpSoundParameter,Seeker.takeoffParameterValue));
                }
                else
                {
                    if (!lastFrameGrounded && m_airTime > 0.3f)
                    {
                        PlaySound(Seeker.jumpSound, SerializeMessage(Seeker.jumpSoundParameter, Seeker.landParameterValue));
                    }
                    velocity.y = 0f;//m_body.velocity.y;
                    lastFrameGrounded = true;
                }
                m_airTime = 0f;
            }
            else
            {
                velocity.y = m_body.velocity.y + Physics.gravity.y * Time.fixedDeltaTime;
                if (lastFrameGrounded)
                {
                    velocity.y = 0f;
                }
                else
                {
                    NetCmdSendMessage(NetMsg.Animation, "b", "onground", false);
                }
                lastFrameGrounded = false;
                m_airTime += Time.deltaTime;
            }

            if (m_isDead) velocity = Vector3.zero;

            if (m_isAffectedByForces)
            {
                // Add horizontal effect given by input
                Vector3 horVelocity = transform.TransformDirection(new Vector3(velocity.x * Time.fixedDeltaTime, 0f, velocity.z * Time.fixedDeltaTime));
                horVelocity += new Vector3(m_body.velocity.x, 0f, m_body.velocity.z);
                if (horVelocity.magnitude > Seeker.baseSpeed * m_speedMultiplier)
                {
                    horVelocity = horVelocity.normalized * Mathf.Lerp(horVelocity.magnitude, Seeker.baseSpeed * m_speedMultiplier, m_linearDrag);
                }
                float gravityForce = (m_isAffectedByGravity ? Physics.gravity.y * Time.fixedDeltaTime : 0f);
                m_body.velocity = new Vector3(horVelocity.x, m_body.velocity.y + gravityForce, horVelocity.z);
            }
            else
            {
                m_body.velocity = transform.TransformDirection(velocity);
            }
        }

        private void LateUpdate()
        {
            if (!IsDead)
            {
                if (m_modelBodySpines != null)
                {
                    Vector3 axis = m_modelBodySpines[0].right;
                    float angle = m_modelBodySpineRotation;
                    while (angle > 180f) angle -= 360f;
                    while (angle < -180f) angle += 360f;

                    for (int i = 1; i < m_modelBodySpines.Length; i++)
                    {
                        m_modelBodySpines[i].Rotate(axis, angle * spineRotationFactor, Space.World);
                    }

                }
                if (m_modelBodySpinesOutsideAnimator != null)
                {
                    foreach (Transform t in m_modelBodySpinesOutsideAnimator)
                    {
                        Vector3 axis = t.right;
                        t.transform.localRotation = Quaternion.Euler(m_modelBodySpineRotation, 0, 0);
                    }
                }
            }
        }

        public void AddEnergy(float amount)
        {
            SetEnergy(m_energyCarried + amount);
        }

        public void SetEnergy(float newValue)
        {
            m_energyCarried = Mathf.Clamp(newValue, 0f, Seeker.maxCarriedEnergy);
            NetRpcSendMessage(NetMsg.SetEnergyCarried, m_energyCarried.ToString("F0"));
        }

        public bool HasFullEnergy()
        {
            return m_energyCarried >= Seeker.maxCarriedEnergy;
        }

        public void AddHealth(float amount)
        {
            float healthLevel = m_health / Seeker.maxHealth;
            float healthAmount = amount;
            if (amount < 0f) healthAmount *= m_damageReductionMultiplier;

            SetHealth(m_health + healthAmount);
        }

        public void SetHealth(float newValue)
        {
            m_health = Mathf.Clamp(newValue, 0f, Seeker.maxHealth);
            NetRpcSendMessage(NetMsg.SetHealth, m_health);
            if (m_health <= 0f)
            {
                OnPlayerDeath(this);
            }
        }

        public bool HasFullHealth()
        {
            return m_health >= Seeker.maxHealth;
        }

        public void RespawnPlayer()
        {
            SpawnPlayer();
        }

        public void FirstRespawnPlayer()
        {
            SpawnPlayer(false, true);
        }
        public void SpawnPlayer(bool rotateSpawnPoint = true, bool ultimateReset = false)
        {
            // Set server player state
            SetPlayerFresh();

            // Send rpcs to sync clients
            NetRpcSendMessage(NetMsg.SetLobbyPlayer, LobbyPlayer.id.ToString());
            TransformInfo spawnPos = m_lastSpawn;
            if (rotateSpawnPoint)
            {
                Transform t = NetLobbyManager.singleton.GetStartPosition(LobbyPlayer.team);
                spawnPos.position = t.position;
                spawnPos.rotation = t.rotation;
            }
            NetRpcSendMessage(NetMsg.Respawn, spawnPos.ToString());
        }

        void SetPlayerFresh()
        {
            m_health = Seeker.maxHealth;
            m_energyCarried = 0f;
            m_canUseMobility = true;
            m_isAffectedByGravity = true;
            m_isAffectedByForces = false;
            m_lastDamager = null;
            m_canInput = true;
            m_canAttack = true;
            m_canInputMouse = true;
            m_isDead = false;
            isWalking = false;
            m_speedMultiplier = 1f;
            m_damageReductionMultiplier = 1f;
            lastPlayerHitID = -1;
            basicAttackAmmo = Seeker.abilities[(int)AbilityType.Base].maxAmmo;
            mainCamera.fieldOfView = startingFOV;
            // set bool death false
            if (isLocalPlayer)
            {
                NetCmdSendMessage(NetMsg.Animation, "b", "death", false);
            }
            destroyOnDeathCos.Add(StartCoroutine(CoApplyInvulnerability(m_antiSpawnKillTime)));
            SetHitLayer(baseLayer);
            SetRagdoll(false);
            ResetLayer();
        }

        public bool IsGrounded()
        {
            PlayerBaseCollider playerBaseCollider = GetComponent<PlayerBaseCollider>();
            if (playerBaseCollider) return playerBaseCollider.isGrounded;
            return false;
            //return Physics.SphereCast(transform.position, m_collider.radius, Vector3.down,out groundHit, 0.1f, walkableLayer);
            //return Physics.Raycast(transform.position, Vector3.down, GetComponent<CapsuleCollider>().radius * 1.2f, walkableLayer);
        }

        public void PlayerDeathLocalPlayer()
        {
            abilityRedirection = new List<AbilityType[]>();
            mainCamera.fieldOfView = startingFOV;
            secondaryCamera.gameObject.SetActive(true);
            StopAllParticles();
            if (zoomMask != null)
            {
                Destroy(zoomMask);
                zoomMask = null;
            }
            foreach (Coroutine c in destroyOnDeathCos)
            {
                if (c != null)
                {
                    StopCoroutine(c);
                }
            }
            destroyOnDeathCos = new List<Coroutine>();
        }

        public void PlayerDeathServer(GamePlayer playerDied)
        {
            m_isDead = true;
            NetRpcSendMessage(NetMsg.Animation, "b", "death", true);
            GameManager.instance.NetRpcSendMessage(NetMsg.Die, LobbyPlayer.id, (m_lastDamager != null ? m_lastDamager.LobbyPlayer.id : -1));
            // Stop any coroutine
            foreach (Coroutine c in destroyOnDeathCos)
            {
                if (c != null)
                {
                    StopCoroutine(c);
                }
            }
            destroyOnDeathCos = new List<Coroutine>();
            Debug.Log(m_lastDamager?.ToString() + " killed " + this.ToString());

            // Stop titan giving energy coroutine
            if (gameTitanCo != null)
            {
                UIGame.instance.canGiveEnergy = true;
                giveEnergyEffect.gameObject.SetActive(false);
                NetRpcSendMessage(NetMsg.SpawnPS, "energy", false);
                UIGame.instance.titanArrow.gameObject.SetActive(true);
                if (isLocalPlayer)
                {
                    UIGame.instance.compassTitanPoint.gameObject.SetActive(false);
                }
                StopCoroutine(gameTitanCo);
                gameTitanCo = null;
            }

            // Increase player deaths stat
            LobbyPlayer.Deaths++;
            LobbyPlayer.Streak = 0;
            if (m_lastDamager)
            {
                // Give kill points
                m_lastDamager.LobbyPlayer.Kills++;
                m_lastDamager.LobbyPlayer.Streak++;
                m_lastDamager.StartMultiKill();
                if (m_lastDamager.LobbyPlayer.Streak > m_lastDamager.LobbyPlayer.MaxStreak)
                {
                    m_lastDamager.LobbyPlayer.MaxStreak = m_lastDamager.LobbyPlayer.Streak;
                }
                GameManager.instance.GetTitan(m_lastDamager.LobbyPlayer.team).AddEnergyKill();
                m_lastDamager.SyncPlayerStats();//.LobbyPlayer.NetRpcSendMessage(NetMsg.SetStats, m_lastDamager.LobbyPlayer.stats.SerializeAll());
                foreach (Effect e in m_lastDamager.LobbyPlayer.seeker.abilities[(int)AbilityType.Passive].effects)
                {
                    if (e.conditionForEffect == EffectCondition.Kill)
                    {
                        m_lastDamager.ApplyEffectSelf(e, this, (int)AbilityType.Passive);
                    }
                }
                while (m_lastDamagersList.Contains(m_lastDamager))
                {
                    m_lastDamagersList.Remove(m_lastDamager);
                }
            }

            // Give assists point
            foreach (GamePlayer player in m_lastDamagersList.Distinct().ToArray())
            {
                player.LobbyPlayer.Assists++;
                player.SyncPlayerStats();//.LobbyPlayer.NetRpcSendMessage(NetMsg.SetStats, player.LobbyPlayer.stats.SerializeAll());//SerializeArray(player.LobbyPlayer.stats));
            }
            m_lastDamagersList = new List<GamePlayer>();
            SyncPlayerStats();
            //LobbyPlayer.NetRpcSendMessage(NetMsg.SetStats, LobbyPlayer.stats.SerializeAll());//;SerializeArray(LobbyPlayer.stats));
            //m_isDead = true;
            //m_canInput = false;
            if (m_energyCarried > 0f)
            {
                GameObject droppedEnergy = Instantiate(m_energyDroppedPrefab, transform.position, transform.rotation);
                droppedEnergy.GetComponent<Item>().SetQuantity(m_energyCarried);
                NetworkServer.Spawn(droppedEnergy);
            }

            float respawnTime = (myTeamTitan.GetComponent<GameTitan>().HasCompletedPath ? m_endPhaseRespawnTime : m_respawnTime);
            NetRpcSendMessage(NetMsg.Die, respawnTime);
            StartCoroutine(CoNoEnergyPickup(respawnTime + 0.5f));
            m_lastDamager = null;
            //StartCoroutine(CoRespawn(respawnTime));
            //Invoke("RespawnPlayer", (myTeamTitan.GetComponent<GameTitan>().HasCompletedPath  ? m_endPhaseRespawnTime : m_respawnTime));
        }

        IEnumerator CoRespawn(float respawnTime)
        {
            float timer = 0f;
            while (timer < respawnTime)
            {
                yield return null;
                timer += Time.deltaTime;
                if(GameManager.instance.isRoundOver)
                {
                    UIGame.instance.respawnTimer.text = "";
                }
                else if (isLocalPlayer)
                {
                    UIGame.instance.respawnTimer.text = String.Format(UIGame.instance.textManager.respawningInMessage, Mathf.Ceil(respawnTime - timer).ToString("F0"));
                    UIGame.instance.m_affectedStatusMessage.text = "";
                }
            }
            UIGame.instance.respawnTimer.text = "";
            if (isServer) RespawnPlayer();
        }

        public void StartAssistTimer(GamePlayer assistPlayer)
        {
            m_lastDamager = assistPlayer;
            m_lastDamagersList.Add(assistPlayer);
            StartCoroutine(CoCleanupLastDamager(assistPlayer));
        }

        public bool CheckCooldown(AbilityType abilityType)
        {
            if (abilitiesCooldowns.Count == 0) return false;
            return abilitiesCooldowns[(int)abilityType] == 0f;
        }

        //private void OnTriggerStay(Collider other)
        //{
        //    if (isServer)
        //    {
        //        if (other.tag == "Titan" && other.gameObject == myTeamTitan)
        //        {
        //            if (!IsDead && Energy > 0f && gameTitanCo == null)
        //            {
        //                if (gameTitanCo != null)
        //                {
        //                    giveEnergyEffect.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        //                    StopCoroutine(gameTitanCo);
        //                }
        //                gameTitanCo = StartCoroutine(CoDeliverEnergy(GameManager.instance.GetTitan(Team)));
        //            }
        //        }
        //    }
        //}

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "SpeedLane")
            {
                m_speedLaneMult = m_speedLaneMultiplier;
            }
        }
        private void OnTriggerStay(Collider other)
        {
            if (isLocalPlayer)
            {
                if (other.tag == "Titan" && other.gameObject == myTeamTitan)
                {
                    if (!UIGame.instance.compassTitanPoint.gameObject.activeSelf)
                    {
                        UIGame.instance.compassTitanPoint.gameObject.SetActive(true);
                    }
                    if (UIGame.instance.titanArrow.gameObject.activeSelf)
                    {
                        UIGame.instance.titanArrow.gameObject.SetActive(false);
                    }
                }
            }

            if (isServer)
            {
                if (other.tag == "Titan" && other.gameObject == myTeamTitan)
                {

                    if (!IsDead && Energy > 0f)//&& gameTitanCo == null)
                    {
                        if (GameManager.instance.GetTitan(Team).enemyInTitanArea)
                        {
                            GameManager.instance.NetTargetSendMessage(NetMsg.PlayersInArea, connectionToClient, false);
                            if (gameTitanCo != null)
                            {

                                giveEnergyEffect.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                                NetTargetSendMessage(NetMsg.PlaySoundAt, connectionToClient, (int)SoundEmitterType.Energy, false, false);
                                NetRpcSendMessage(NetMsg.SpawnPS, "energy", false);
                                StopCoroutine(gameTitanCo);
                                gameTitanCo = null;
                            }
                        }
                        else
                        {
                            GameManager.instance.NetTargetSendMessage(NetMsg.PlayersInArea, connectionToClient, true);
                            if (gameTitanCo == null)
                            {
                                gameTitanCo = StartCoroutine(CoDeliverEnergy(GameManager.instance.GetTitan(Team)));
                            }
                        }
                    }
                }
            }
            if (other.tag == "SpeedLane")
            {
                m_speedLaneMult = m_speedLaneMultiplier;
            }
        }

        public void SendTarget(NetMsg msgType, string msg)
        {
            NetTargetSendMessage(msgType,connectionToClient, msg);
        }



        private void OnTriggerExit(Collider other)
        {
            if (isLocalPlayer)
            {
                if (other.tag == "Titan" && other.gameObject == myTeamTitan)
                {
                    if (Energy > 0f)
                    {
                        UIGame.instance.titanArrow.gameObject.SetActive(true);
                    }
                    UIGame.instance.compassTitanPoint.gameObject.SetActive(false);
                }
            }
            if (isServer)
            {
                if (other.tag == "Titan" && other.gameObject == myTeamTitan)
                {
                    GameManager.instance.NetTargetSendMessage(NetMsg.PlayersInArea, connectionToClient, true);
                    if (!IsDead && gameTitanCo != null)
                    {
                        NetTargetSendMessage(NetMsg.PlaySoundAt, connectionToClient, (int)SoundEmitterType.Energy, false, false);
                        NetRpcSendMessage(NetMsg.SpawnPS, "energy", false);
                        StopCoroutine(gameTitanCo);
                        gameTitanCo = null;
                    }
                }
            }
            if (other.tag == "SpeedLane")
            {
                m_speedLaneMult = 1f;
            }
        }

        public void ApplyEffectSelf(Effect effect, GamePlayer sender, int abilityTypeID = -1, bool headshot = false)
        {
            ApplyEffect(effect, sender.LobbyPlayer.id, null, abilityTypeID);
        }

        public void ApplyEffect(Effect effect, int id, AttackBehaviour attack, int abilityTypeID = -1, bool headshot = false)
        {
            GamePlayer player = this;
            if (effect.isSelf && effect.conditionForEffect == EffectCondition.PlayerHit)
            {
                player = Find(id);
            }
            if (player.m_isInvulnerable && !effect.isSelf) return;

            if (!player.m_isDead)
            {
                if (effect.damage != 0f)
                {
                    float damage = effect.damage;
                    if (headshot)
                    {
                        damage *= effect.headshotMult;
                    }
                    if (damage > 0f && id != player.LobbyPlayer.id)
                    {
                        player.m_lastDamager = Find(id);
                        //player.StartAssistTimer(player.m_lastDamager);
                    }
                    player.AddHealth(-damage);

                    GamePlayer applier = Find(id);
                    if (headshot) applier.LobbyPlayer.TotalHeadshots++;
                    if (damage > 0f)
                    {
                        applier.LobbyPlayer.DamageDealt += (uint)damage;
                        player.LobbyPlayer.DamageReceived += (uint)damage;
                        if (headshot)
                        {
                            applier.LobbyPlayer.Headshots++;
                        }

                    }
                    else
                    {
                        applier.LobbyPlayer.HealingDealt += (uint)-damage;
                        player.LobbyPlayer.HealingReceived += (uint)-damage;
                        if (abilityTypeID == (int)AbilityType.Base)
                        {
                            player.SendTarget(NetMsg.PlaySound, soundGetHealed);
                        }
                    }
                    applier.SyncPlayerStats();//.LobbyPlayer.NetRpcSendMessage(NetMsg.SetStats, applier.LobbyPlayer.stats.SerializeAll());//SerializeArray(applier.LobbyPlayer.stats));
                    player.SyncPlayerStats();//.LobbyPlayer.NetRpcSendMessage(NetMsg.SetStats, player.LobbyPlayer.stats.SerializeAll());//SerializeArray(player.LobbyPlayer.stats));

                    // We use negative damages as heals
                    
                    int senderID = (damage > 0f ? (player.m_lastDamager != null ? player.m_lastDamager.LobbyPlayer.id : -1) : id);
                    if (effect.giveFeedbackOnScreen && attack != null)
                    {
                        if (headshot && damage > 0f)
                        {
                            GameManager.instance.NetRpcSendMessage(NetMsg.Hit, player.LobbyPlayer.id, senderID, damage * m_damageReductionMultiplier, attack.startPosition.x, attack.startPosition.y, attack.startPosition.z, (int)HitType.HeadShot, effect.headShotParameterValue);
                        }
                        else if (damage < 0)
                        {
                            GameManager.instance.NetRpcSendMessage(NetMsg.Hit, player.LobbyPlayer.id, senderID, damage, attack.startPosition.x, attack.startPosition.y, attack.startPosition.z, (int)HitType.Heal, effect.hitParameterValue);
                        }
                        else
                        {
                            GameManager.instance.NetRpcSendMessage(NetMsg.Hit, player.LobbyPlayer.id, senderID, damage * m_damageReductionMultiplier, attack.startPosition.x, attack.startPosition.y, attack.startPosition.z, (int)HitType.Hit, effect.hitParameterValue);
                        }
                    }
                }
            }
            if (effect.speedTime > 0f && effect.speedMult != 1f)
            {
                player.StopCoroutine("CoSpeedMultiplier");
                player.destroyOnDeathCos.Add(player.StartCoroutine(player.CoSpeedMultiplier(effect.speedMult, effect.speedTime)));
            }
            //if (effect.deactivateGravityTime > 0f)
            //{
            //    player.StopCoroutine("CoDisableGravity");
            //    player.destroyOnDeathCos.Add(player.StartCoroutine(player.CoDisableGravity(effect.deactivateGravityTime)));
            //}
            if (effect.deactivateMouseTime > 0f)
            {
                player.StopCoroutine("CoDisableMouse");
                player.destroyOnDeathCos.Add(player.StartCoroutine(player.CoDisableMouse(effect.deactivateMouseTime)));
            }
            if (effect.noClipTime > 0f)
            {
                player.StopCoroutine("CoNoClipTime");
                player.destroyOnDeathCos.Add(player.StartCoroutine(player.CoNoClipTime(effect.noClipTime)));
            }
            if (effect.stunTime > 0f)
            {
                player.StopCoroutine("CoApplyStun");
                player.destroyOnDeathCos.Add(player.StartCoroutine(player.CoApplyStun(effect.stunTime)));
            }
            if (effect.snareTime > 0f)
            {
                player.StopCoroutine("CoApplySnare");
                player.destroyOnDeathCos.Add(player.StartCoroutine(player.CoApplySnare(effect.snareTime)));
            }
            if (abilityTypeID != -1 && effect.appliedForceIntensity != 0f)
            {
                GamePlayer applier = (local.LobbyPlayer.id == id ? local : Find(id));
                for (int i = 0; i < applier.Abilities[abilityTypeID].effects.Length; i++)
                {
                    if (effect == applier.Abilities[abilityTypeID].effects[i])
                    {
                        player.NetTargetSendMessage(NetMsg.Update, player.connectionToClient, (int)GamePlayerState.Physics, abilityTypeID, i, id);
                    }
                }
            }
            if (effect.cooldownReduction != 0f)
            {
                foreach (AbilityType at in effect.abilitiesToApplyCDR)
                {
                    abilitiesCooldowns[(int)at] = Mathf.Clamp(abilitiesCooldowns[(int)at] - player.Seeker.abilities[(int)at].cooldown * effect.cooldownReduction, 0f, player.Seeker.abilities[(int)at].cooldown);
                    player.NetTargetSendMessage(NetMsg.SetCooldown, player.connectionToClient, (int)at, abilitiesCooldowns[(int)at]);
                }
            }
            if (effect.redirectedAbility != AbilityType.None)
            {
                AbilityType[] redirection = new AbilityType[] { effect.redirectedAbility, effect.substituteAbility };
                bool flag = true;
                foreach (AbilityType[] ab in player.abilityRedirection)
                {
                    if (ab[0] == redirection[0] && ab[1] == effect.substituteAbility)
                    {
                        flag = false;
                        break;
                    }
                }
                if (flag)
                {
                    player.abilityRedirection.Add(redirection);
                    if (effect.redirectionTime > 0f)
                    {
                        player.destroyOnDeathCos.Add(player.StartCoroutine(player.CoRedirectionTime(effect.redirectionTime, redirection[0], redirection[1])));
                    }
                }
            }
            if (effect.damageReduction != 1f)
            {
                player.StopCoroutine("CoApplyDmgReduction");
                player.destroyOnDeathCos.Add(player.StartCoroutine(player.CoApplyDmgReduction(effect.damageReductionTime)));
            }
            if (effect.saveHitPlayer)
            {
                // Save hit player
                player.lastPlayerHitID = LobbyPlayer.id;
                player.NetTargetSendMessage(NetMsg.UpdateLastPlayerHitId, player.connectionToClient, player.lastPlayerHitID);
                if (effect.keepHitPlayerTime > 0f)
                {
                    player.StopCoroutine("CoSavePlayerTimer");
                    player.destroyOnDeathCos.Add(player.StartCoroutine(player.CoSavePlayerTimer(effect.keepHitPlayerTime)));
                }
            }
            if (effect.teleportToSavedPlayer && player.lastPlayerHitID != -1)
            {
                // Teleport
                Vector3 hitPlayerPos = Find(player.lastPlayerHitID).transform.position;
                player.NetRpcSendMessage(NetMsg.Update, (int)GamePlayerState.Position);
                player.NetTargetSendMessage(NetMsg.Update, player.connectionToClient, (int)GamePlayerState.Position, hitPlayerPos.x, hitPlayerPos.y, hitPlayerPos.z);
                player.NetTargetSendMessage(NetMsg.UpdateLastPlayerHitId, player.connectionToClient, -1);
            }

        }

        public void ApplyPhysics(Effect effect, int applierId)
        {
            if (effect.appliedForceIntensity != 0f)
            {
                if (effect.applyForceTowardAttacker)
                {
                    destroyOnDeathCos.Add(StartCoroutine(CoAddPull(effect, () => Find(applierId).m_isDead == true, applierId)));
                }
                else
                {
                    if (effect.appliedForceTime > 0f)
                    {
                        destroyOnDeathCos.Add(StartCoroutine(CoAddForce(effect, () => effect.deactivateWithInput && m_inputManager.GetKeyDown(InputKey.Any), applierId)));
                    }
                    else
                    {
                        destroyOnDeathCos.Add(StartCoroutine(CoAddImpulse(effect, () => IsGrounded() /*&& m_body.velocity.y <= 0f*/, 0.1f, applierId)));
                    }
                }
            }
        }
        public void SpawnDamageInput(float damage, Vector3 target)
        {
            DamageInputBehaviour d = Instantiate(damageInput, UIGame.instance.effectsSpawnPoint.transform).GetComponent<DamageInputBehaviour>();
            d.Setup(target, Mathf.Clamp(damage / Seeker.maxHealth, 0.1f, 0.5f));
            //float amount =damage/Seeker.maxHealth;
            //amount = Mathf.Clamp(amount, 0.1f, 0.5f);
            //d.DamagePosition = target;
            //d.mat.SetFloat("_Level",amount);
        }
        #endregion


        #region Network Messaging                    

        // Check if a message is allowed
        public override bool CheckMessage(string msg, NetMsgType netMsgType)
        {
            //if (MenuManager.Instance.showDebugLog) Debug.Log("Checking " + msg + "<b>(" + netMsgType + "</b>");
            string[] msgArray = DeserializeMessage(msg);
            NetMsg msgIndex = (NetMsg)System.Convert.ToInt32(msgArray[0]);
            switch (msgIndex)
            {
                case NetMsg.CastAbility:
                {
                    // No cast ability from dead people
                    if (netMsgType == NetMsgType.Cmd)
                    {
                        int abilityTypeID = Convert.ToInt32(msgArray[1]);
                        foreach (AbilityType[] ab in abilityRedirection)
                        {
                            if ((int)ab[0] == abilityTypeID)
                            {
                                abilityTypeID = (int)ab[1];
                                break;
                            }
                        }
                        if (!m_canUseMobility && abilityTypeID == (int)AbilityType.Movement) return false;
                        if (Seeker == null || Seeker.abilities == null) return false;
                        if (Seeker.abilities[abilityTypeID].consumeBaseAbilityAmmo && Seeker.abilities[(int)AbilityType.Base].maxAmmo == basicAttackAmmo && !CheckCooldown(AbilityType.Base)) return false;
                        if (!CheckCooldown((AbilityType)abilityTypeID))
                        {
                            return false;
                        }
                        //AbilityType redirectedType = Abilities[abilityTypeID].RedirectedAbility();
                        //if (redirectedType != AbilityType.None)
                        //{
                        //    if (!CheckCooldown(redirectedType)) return false;
                        //}
                        if (m_isDead || !m_canInput) return false;
                    }
                    return true;
                }
                case NetMsg.SetHealth:
                {
                    // Allow only server SetHealth messages [Rpc]
                    //if (netMsgType == NetMsgType.Cmd)
                    //    return false;
                    return true;
                }
                case NetMsg.Respawn:
                {
                    // Allow only server Respawned messages [Rpc]
                    //if (netMsgType == NetMsgType.Cmd)
                    //    return false;
                    if (GameManager.instance.isRoundOver)
                        return false;
                    return true;
                }
                case NetMsg.Die:
                {
                    // Allow only server Died messages [Rpc]
                    if (netMsgType == NetMsgType.Cmd)
                        return false;
                    return true;
                }
                case NetMsg.SetLobbyPlayer:
                case NetMsg.StartCooldown:
                case NetMsg.SetCooldown:
                case NetMsg.SetEnergyCarried:
                case NetMsg.Update:
                case NetMsg.SetReady:
                case NetMsg.Animation:
                case NetMsg.Reload:
                case NetMsg.RedirectionClear:
                case NetMsg.UpdateLastPlayerHitId:
                case NetMsg.SpawnPS:
                case NetMsg.AnimateCrosshair:
                case NetMsg.PlaySound:
                case NetMsg.PlaySoundAt:
                case NetMsg.SetParameterAt:
                {
                    return true;
                }
               
            }
            return false;
        }

        // == Messages handling
        public override void ExecuteMessage(string msg, NetMsgType netMsgType)
        {
            //if (MenuManager.Instance.showDebugLog) Debug.Log("Executing " + msg + "<b>(" + netMsgType + "</b>");
            string[] msgArray = DeserializeMessage(msg);
            NetMsg msgIndex = (NetMsg)System.Convert.ToInt32(msgArray[0]);

            switch (msgIndex)
            {
                case NetMsg.CastAbility: // EXECastAbil
                {
                    if (netMsgType == NetMsgType.Cmd)
                    {
                        // Get the type of ability used
                        int abilityTypeID = Convert.ToInt32(msgArray[1]);
                        int realAbilityTypeID = abilityTypeID;

                        // Check if the ability is redirected
                        foreach (AbilityType[] ab in abilityRedirection)
                        {
                            if ((int)ab[0] == abilityTypeID)
                            {
                                abilityTypeID = (int)ab[1];
                                break;
                            }
                        }

                        // Get ability infos
                        Ability ability = Abilities[abilityTypeID];
                        AbilityType abilityType = (AbilityType)abilityTypeID;
                        if (ability.uiEffect)
                        {
                                //  Destroy(Instantiate(ability.uiEffect, UIGame.instance.effectsSpawnPoint.transform), ability.lifeTime);
                                NetTargetSendMessage(NetMsg.SpawnPS, connectionToClient, ability.uiEffect.name,ability.lifeTime);
                        }
                        if (ability.animateCrosshair)
                        {
                            NetTargetSendMessage(NetMsg.AnimateCrosshair, connectionToClient, abilityTypeID);
                            
                        }
                        // Check if passed coords are direction or position
                        bool isDirection = true;
                        if (msgArray.Length > 5)
                        {
                            isDirection = (Convert.ToInt32(msgArray[5]) == 1);
                        }
                        // Animation ability
                        switch (abilityType)
                        {
                            case AbilityType.Base:
                            NetRpcSendMessage(NetMsg.Animation, "b", "attack", true);
                            break;
                            case AbilityType.Movement:
                            NetRpcSendMessage(NetMsg.Animation, "t", "mobility");
                            break;
                            case AbilityType.First:
                            NetRpcSendMessage(NetMsg.Animation, "t", "ability1");
                            break;
                            case AbilityType.Second:
                            NetRpcSendMessage(NetMsg.Animation, "t", "ability2");
                            break;
                            case AbilityType.Ultimate:
                            NetRpcSendMessage(NetMsg.Animation, "t", "ultimate");
                            break;
                        }
                        // PETER

                        // Get the point where the attack is directed to
                        Vector3 hitPoint = new Vector3(Convert.ToSingle(msgArray[2]), Convert.ToSingle(msgArray[3]), Convert.ToSingle(msgArray[4]));
                        if (isDirection)
                        {
                            Vector3 lookDirection = hitPoint;
                            RaycastHit cameraHitPoint;
                            hitPoint = mainCamera.transform.position + lookDirection * ability.range;
                            SetLayer(temporaryGhostLayer);
                            SetHitLayer(temporaryGhostLayer);
                            if (Physics.Raycast(mainCamera.transform.position, lookDirection, out cameraHitPoint, ability.range, aimableLayer))
                            {
                                hitPoint = cameraHitPoint.point;
                            }
                            SetHitLayer(baseLayer);
                            ResetLayer();
                        }

                        // Check if can be casted only on ground
                        if (ability.onlyCastOnGround && !Physics.Raycast(transform.position, Vector3.down, m_collider.radius * 1.2f, walkableLayer)) break;

                        // Apply self effects
                        foreach (Effect effect in ability.effects)
                        {
                            if (effect.isSelf && effect.conditionForEffect == EffectCondition.None)
                            {
                                ApplyEffectSelf(effect, this, abilityTypeID);
                                if (effect.particleEffect)
                                {
                                    GameManager.instance.NetRpcSendMessage(NetMsg.SpawnPS, effect.particleEffect.name, transform.position.x, transform.position.y, transform.position.z, 0f, 0f, 0f);
                                }

                            }
                        }

                        if (ability.attackPrefab)
                        {
                            Vector3 spawnPos;
                            Quaternion spawnRot;
                            if (ability.attackPrefabSpawnPoint == AttackSpawnPoint.None)
                            {
                                spawnPos = hitPoint;
                                spawnRot = transform.rotation;
                            }
                            else
                            {
                                spawnPos = m_attackSpawnPoints[(int)ability.attackPrefabSpawnPoint].position;
                                spawnRot = Quaternion.LookRotation(hitPoint - spawnPos, Vector3.up); // m_attackSpawnPoints[(int)ability.attackPrefabSpawnPoint].rotation;
                                if (ability.isAttachedToPlayer)
                                    spawnRot = m_attackSpawnPoints[(int)ability.attackPrefabSpawnPoint].rotation;
                            }

                            // Instantiate bullet
                            GameObject attack = Instantiate(ability.attackPrefab, spawnPos, spawnRot);
                            //attack.transform.forward = m_attackSpawnPoints[(int)ability.attackPrefabSpawnPoint].forward;
                            attack.GetComponent<AttackBehaviour>().SetupAttack(this, ability, hitPoint);
                            NetworkServer.Spawn(attack);
                            StopCoroutine("CoSpawnAttack");
                            if (ability.spawnNumber > 1)
                            {
                                destroyOnDeathCos.Add(StartCoroutine(CoSpawnAttack(this, ability, hitPoint)));
                            }
                        }
                        StartCoroutine(CoApplyCooldown(abilityType));
                        NetTargetSendMessage(NetMsg.StartCooldown, connectionToClient, abilityTypeID);//, realAbilityTypeID);
                        SendSound(ability);
                        if (ability.consumeBaseAbilityAmmo)
                        {
                            StartCoroutine(CoApplyCooldown(AbilityType.Base));
                            NetTargetSendMessage(NetMsg.StartCooldown, connectionToClient, (int)AbilityType.Base);
                        }

                        // Check if the ability is redirected
                        if (realAbilityTypeID != abilityTypeID)
                        {
                            foreach (AbilityType[] ab in abilityRedirection)
                            {
                                if ((int)ab[0] == realAbilityTypeID)
                                {
                                    abilityRedirection.Remove(ab);
                                    //StartCoroutine(CoApplyCooldown((AbilityType)realAbilityTypeID));
                                    //NetTargetSendMessage(NetMsg.StartCooldown, connectionToClient, realAbilityTypeID);
                                    break;
                                }
                            }
                        }
                    }
                    break;
                }
                case NetMsg.RedirectionClear:
                {
                    int abilityTypeID = Convert.ToInt32(msgArray[1]);
                    foreach (AbilityType[] ab in abilityRedirection)
                    {
                        if ((int)ab[0] == abilityTypeID)
                        {
                            abilityRedirection.Remove(ab);
                            break;
                        }
                    }
                    break;
                }
                case NetMsg.SetHealth:
                {
                    if (netMsgType == NetMsgType.Rpc || netMsgType == NetMsgType.Target)
                    {
                        m_health = Convert.ToSingle(msgArray[1]);
                        m_health = Mathf.Clamp(m_health, 0f, Seeker.maxHealth);
                        float healthPercent = m_health / Seeker.maxHealth;
                        //lifeBarRenderer.size = new Vector2(lifeBarFullRenderer.size.x * healthPercent, lifeBarFullRenderer.size.y);
                        lifeBarRenderer.fillAmount = healthPercent;
                        if (healthPercent <= m_criticalHealthLevel)
                        {
                            if (!criticalIcon.activeSelf)
                            {
                                criticalIcon.SetActive(true);
                            }
                        }
                        else
                        {
                            if (criticalIcon.activeSelf)
                            {
                                criticalIcon.SetActive(false);
                            }
                        }
                    }
                    break;
                }
                case NetMsg.SetLobbyPlayer:
                {
                    // Command from clients to know if they loaded their players
                    if (netMsgType == NetMsgType.Cmd)
                    {
                        if (!NetLobbyManager.singleton.loadedPlayers.Contains(this))
                        {
                            NetLobbyManager.singleton.loadedPlayers.Add(this);
                            Debug.Log("<b>Added " + LobbyPlayer.playerName + " in LOADED PLAYERS</b>");
                            if (NetLobbyManager.singleton.loadedPlayers.Count == NetLobbyManager.singleton.netLobbyPlayers.Length)
                            {
                                if (GameManager.instance.teamARounds + GameManager.instance.teamBRounds == 0)
                                {
                                    StartRound();
                                }
                                GameManager.instance.SyncManager();
                            }
                        }
                    }
                    // Server message of lobby player ref
                    if (netMsgType == NetMsgType.Rpc || netMsgType == NetMsgType.Target)
                    {
                        int _id = Convert.ToInt32(msgArray[1]);
                        LobbyPlayer = NetLobbyPlayer.Find(_id);
                        Debug.Assert(LobbyPlayer != null, "Lobby player for " + _id + " loading error");
                        StartCoroutine(CoWaitLobbyPlayerLocal(netMsgType));
                    }
                    break;
                }
                case NetMsg.Respawn:
                {
                    if (netMsgType == NetMsgType.Rpc || netMsgType == NetMsgType.Target)
                    {
                        Vector3 pos = new Vector3(Convert.ToSingle(msgArray[1]), Convert.ToSingle(msgArray[2]), Convert.ToSingle(msgArray[3]));
                        if (isLocalPlayer)
                        {                           
                            Quaternion rot = Quaternion.Euler(Convert.ToSingle(msgArray[4]), Convert.ToSingle(msgArray[5]), Convert.ToSingle(msgArray[6]));
                            transform.position = pos;
                            transform.rotation = rot;
                            mainCameraScript.ResetCamera();
                            m_body.velocity = Vector3.zero;
                        }
                        else
                        {
                            hud.SetActive(true);
                            // Activate temporarily forced teleport (no net transform interpolation)
                            foreach (NetTransform nt in m_netTransforms)
                            {
                                nt.forceTeleport = true;
                            }

                            // Start a timer to disable the forced teleport
                            if (netTransformSync != null)
                            {
                                StopCoroutine(netTransformSync);
                            }
                            netTransformSync = StartCoroutine(CoResetTeleport());
                        }
                        foreach(BaseSpawnManager b in GameManager.instance.spawnBases)
                        {
                            if(b.myTeam==Team)
                            {
                                if (b.myTeam == local.Team)
                                {
                                    b.SpawnEffect(pos, b.allySpawn);
                                }
                                else
                                {
                                    b.SpawnEffect(pos, b.enemySpawn);
                                }    
                            }                           
                        }
                        SetPlayerFresh();
                        criticalIcon.SetActive(false);
                        //lifeBarRenderer.size = new Vector2(lifeBarFullRenderer.size.x, lifeBarFullRenderer.size.y);
                        lifeBarRenderer.fillAmount = 1f;
                    }
                    break;
                }
                case NetMsg.Die:
                {
                    if (netMsgType == NetMsgType.Rpc || netMsgType == NetMsgType.Target)
                    {
                        m_isDead = true;
                        m_canInput = false;
                        m_canAttack = false;
                        m_canInputMouse = false;
                        m_isAffectedByForces = false;
                        m_isAffectedByGravity = true;
                        isWalking = false;
                        m_energyCarried = 0f;
                        lastPlayerHitID = -1;
                        hud.SetActive(false);
                        if (isLocalPlayer)
                        {
                            PlayerDeathLocalPlayer();
                            PlaySoundAt(SoundEmitterType.Energy, false);
                            NetCmdSendMessage(NetMsg.PlaySoundAt, (int)SoundEmitterType.Walk, false);
                            PlaySound(Seeker.breathanddeath);
                        }
                        if (isServer || isLocalPlayer) StartCoroutine(CoRespawn(Convert.ToSingle(msgArray[1])));
                        StopAllParticles();
                        StartCoroutine(CoStartDeathRay());
                        if (local.lastPlayerHitID == LobbyPlayer.id)
                        {
                            local.NetCmdSendMessage(NetMsg.UpdateLastPlayerHitId, -1);
                        }
                        SetLayer(notTeamLayer);
                        SetHitLayer(notTeamLayer);
                        SetRagdoll(true);
                    }
                    break;
                }
                case NetMsg.StartCooldown:
                {
                    if (netMsgType == NetMsgType.Target)
                    {
                        if (msgArray.Length > 2)
                        {
                            StartCoroutine(CoApplyCooldownForced((AbilityType)Convert.ToInt32(msgArray[1]), Convert.ToSingle(msgArray[2])));
                        }
                        else if (!isServer)
                        {
                            StartCoroutine(CoApplyCooldown((AbilityType)Convert.ToInt32(msgArray[1])));
                        }
                    }
                    break;
                }
                case NetMsg.SetCooldown:
                {
                    if (netMsgType == NetMsgType.Target)
                    {
                        abilitiesCooldowns[Convert.ToInt32(msgArray[1])] = Convert.ToSingle(msgArray[2]);
                    }
                    break;
                }
                case NetMsg.SetEnergyCarried:
                {
                    if (netMsgType == NetMsgType.Rpc || netMsgType == NetMsgType.Target)
                    {
                        m_energyCarried = Convert.ToSingle(msgArray[1]);
                    }
                    break;
                }
                case NetMsg.PlaySound:
                {
                    if (netMsgType == NetMsgType.Rpc || netMsgType == NetMsgType.Target)
                    {
                        string soundEvent = msgArray[1];//msgArray[(netMsgType == NetMsgType.Target ? 2 : 1)];
                        if (soundEvent != "")
                        {
                            PlaySound(soundEvent);
                        }
                    }
                    break;
                }
                case NetMsg.PlaySoundAt:
                {
                    if (netMsgType == NetMsgType.Cmd)
                    {
                        RpcSendMessage(msg);
                    }
                    if (netMsgType == NetMsgType.Rpc || netMsgType == NetMsgType.Target)
                    {
                        bool toRestart = false;
                        if(msgArray.Length > 3) toRestart = Convert.ToBoolean(msgArray[3]);
                        bool toSound = Convert.ToBoolean(msgArray[2]);
                        int soundEmitterIndex = Convert.ToInt32(msgArray[1]);
                        PlaySoundAt((SoundEmitterType)soundEmitterIndex, toSound, toRestart);
                    }
                    break;
                }
                case NetMsg.SetParameterAt:
                {
                    if (netMsgType == NetMsgType.Rpc || netMsgType == NetMsgType.Target)
                    {
                        int soundEmitterIndex = Convert.ToInt32(msgArray[1]);
                        float parameterValue = Convert.ToSingle(msgArray[3]);
                        SetParameterAt((SoundEmitterType)soundEmitterIndex, msgArray[2], parameterValue);
                    }
                    break;
                }
                case NetMsg.Animation:
                {
                    if (netMsgType == NetMsgType.Cmd)
                    {
                        RpcSendMessage(msg);
                    }
                    if (netMsgType == NetMsgType.Rpc || netMsgType == NetMsgType.Target)
                    {
                        if (m_serverSeekerAnimator != null)
                        {
                            if (msgArray[1] == "t") m_serverSeekerAnimator.SetTrigger(msgArray[2]);
                            else if (msgArray[1] == "b") m_serverSeekerAnimator.SetBool(msgArray[2], Convert.ToBoolean(msgArray[3]));
                            else if (msgArray[1] == "f") m_serverSeekerAnimator.SetFloat(msgArray[2], Convert.ToSingle(msgArray[3]));
                            else if (msgArray[1] == "i") m_serverSeekerAnimator.SetInteger(msgArray[2], Convert.ToInt32(msgArray[3]));
                        }
                        if (isLocalPlayer)
                        {
                            if (m_clientSeekerAnimator != null)
                            {
                                if (msgArray[1] == "t") m_clientSeekerAnimator.SetTrigger(msgArray[2]);
                                else if (msgArray[1] == "b") m_clientSeekerAnimator.SetBool(msgArray[2], Convert.ToBoolean(msgArray[3]));
                                else if (msgArray[1] == "f") m_clientSeekerAnimator.SetFloat(msgArray[2], Convert.ToSingle(msgArray[3]));
                                else if (msgArray[1] == "i") m_clientSeekerAnimator.SetInteger(msgArray[2], Convert.ToInt32(msgArray[3]));

                            }
                        }
                    }
                    break;
                }
                case NetMsg.Update:
                {
                    GamePlayerState whichStat = (GamePlayerState)Convert.ToInt32((msgArray[1]));
                    switch (whichStat)
                    {
                        case GamePlayerState.CanInput:
                        {
                            m_canInput = Convert.ToBoolean(msgArray[2]);
                            break;
                        }
                        case GamePlayerState.CanAttack:
                        {
                            m_canAttack = Convert.ToBoolean(msgArray[2]);
                            break;
                        }
                        case GamePlayerState.CanInputMouse:
                        {
                            m_canInputMouse = Convert.ToBoolean(msgArray[2]);
                            break;
                        }
                        case GamePlayerState.SpeedMult:
                        {
                            m_speedMultiplier = Convert.ToSingle(msgArray[2]);
                            break;
                        }
                        case GamePlayerState.Gravity:
                        {
                            m_isAffectedByGravity = Convert.ToBoolean(msgArray[2]);
                            break;
                        }
                        case GamePlayerState.Physics:
                        {
                            // Only target rpc
                            if (netMsgType == NetMsgType.Target)
                            {
                                int applierId = Convert.ToInt32(msgArray[4]);
                                GamePlayer applier = Find(Convert.ToInt32(applierId));
                                int abilityIndex = Convert.ToInt32(msgArray[2]);
                                int effectId = Convert.ToInt32(msgArray[3]);
                                // Debug.Log("PHYSICS:" + applier.LobbyPlayer.playerName + " abilityID:" + abilityIndex+ " effectID:" + effectId);
                                ApplyPhysics(applier.LobbyPlayer.seeker.abilities[abilityIndex].effects[effectId], applierId);
                            }
                            break;
                        }
                        case GamePlayerState.Ammo:
                        {
                            if (netMsgType == NetMsgType.Target)
                            {
                                int baseID = (int)AbilityType.Base;
                                basicAttackAmmo = Convert.ToInt32(msgArray[2]);
                                if (basicAttackAmmo == Seeker.abilities[baseID].maxAmmo && Seeker.abilities[baseID].maxAmmo > 0)
                                {
                                    StartCoroutine(CoApplyCooldown(AbilityType.Base, true));
                                }
                            }
                            break;
                        }
                        case GamePlayerState.BodyRotation:
                        {
                            if (netMsgType == NetMsgType.Cmd)
                            {
                                RpcSendMessage(msg);
                            }
                            if (netMsgType == NetMsgType.Rpc)
                            {
                                m_modelBodySpineRotation = Convert.ToSingle(msgArray[2]);
                            }
                            break;
                        }
                        case GamePlayerState.Position:
                        {
                            if (netMsgType == NetMsgType.Target)
                            {
                                Vector3 pos = new Vector3(Convert.ToSingle(msgArray[2]), Convert.ToSingle(msgArray[3]), Convert.ToSingle(msgArray[4]));
                                transform.position = pos;
                            }
                            if (netMsgType == NetMsgType.Rpc)
                            {
                                foreach (NetTransform nt in m_netTransforms)
                                {
                                    nt.forceTeleport = true;
                                }
                                // Start a timer to disable the forced teleport
                                if (netTransformSync != null)
                                {
                                    StopCoroutine(netTransformSync);
                                }
                                netTransformSync = StartCoroutine(CoResetTeleport());
                            }

                            break;
                        }
                        case GamePlayerState.NoClipTime:
                        {
                            bool isGhost = Convert.ToBoolean(msgArray[2]);
                            if (isGhost)
                            {
                                SetLayer(notTeamLayer);
                            }
                            else
                            {
                                ResetLayer();
                            }
                            break;
                        }
                        case GamePlayerState.CanUseMobility:
                        {
                            if (netMsgType == NetMsgType.Rpc)
                            {
                                m_canUseMobility = Convert.ToBoolean(msgArray[2]);
                            }
                            break;
                        }
                        default: break;
                    }
                    break;
                }
                case NetMsg.SetReady:
                {
                    if (netMsgType == NetMsgType.Cmd)
                    {
                        Debug.Log("Client READY");
                        NetTargetSendMessage(NetMsg.SetLobbyPlayer, connectionToClient, LobbyPlayer.id.ToString());
                    }
                    break;
                }
                case NetMsg.Reload:
                {
                    if (netMsgType == NetMsgType.Cmd)
                    {
                        basicAttackAmmo = 1;
                        StartCoroutine(CoApplyCooldown(0));
                    }
                    if (netMsgType == NetMsgType.Target)
                    {
                        UIGame.instance.StartReload();
                        NetCmdSendMessage(NetMsg.Animation, "b", "attack", false);
                        NetCmdSendMessage(NetMsg.Animation, "t", "reload");
                            PlaySound(Seeker.reload);
                    }
                    break;
                }
                case NetMsg.UpdateLastPlayerHitId:
                {
                    if (netMsgType == NetMsgType.Cmd)
                    {
                        lastPlayerHitID = Convert.ToInt32(msgArray[1]);
                        TargetSendMessage(connectionToClient, msg);
                    }
                    if (netMsgType == NetMsgType.Target)
                    {
                        Debug.Log("Received update hit id:" + Convert.ToInt32(msgArray[1]) + " oldid: " + lastPlayerHitID);
                        GamePlayer oldPlayer = Find(lastPlayerHitID);
                        lastPlayerHitID = Convert.ToInt32(msgArray[1]);
                        GamePlayer hitplayer = Find(lastPlayerHitID);
                        if (oldPlayer)
                        {
                            //oldPlayer.markedIcon.SetActive(false);
                            oldPlayer.vayvinMark.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                        }
                        if (hitplayer)
                        {
                            //hitplayer.markedIcon.SetActive(true);
                            hitplayer.vayvinMark.Play(true);
                        }

                    }

                    break;
                }
                case NetMsg.SpawnPS:
                {
                    if(netMsgType==NetMsgType.Target)
                    {
                        Destroy(Instantiate(MenuManager.Instance.FindParticleEffect(msgArray[1]), UIGame.instance.effectsSpawnPoint.transform),Convert.ToSingle(msgArray[2]));
                    }
                    else if (msgArray[1] == "energy")
                    {
                        bool on = Convert.ToBoolean(msgArray[2]);
                        if (on)
                        {
                            giveEnergyEffect.Play(true);
                        }
                        else
                        {
                            giveEnergyEffect.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                        }
                    }
                    break;
                }
                case NetMsg.AnimateCrosshair:
                {
                   if(netMsgType== NetMsgType.Target)
                   {
                      Ability ability = Abilities[Convert.ToInt32(msgArray[1])];
                      UIGame.instance.StartCrosshairShake(ability.yMovment, ability.xMovment);
                   }
                   break;
                }
            }
        }

        #endregion


        public static GamePlayer Find(int index)
        {
            foreach (GamePlayer g in allPlayers)
            {
                if (g.LobbyPlayer != null)
                {
                    if (g.LobbyPlayer.id == index)
                    {
                        return g;
                    }
                }
            }
            return null;
        }

        public void OnDisable()
        {
            StopAllCoroutines();
        }

        public void StartRound()
        {
            Debug.Log("<color=#0000FF>Round Start</color>");
            GameManager.instance.NetRpcSendMessage(NetMsg.SetReady);
            foreach (GamePlayer p in NetLobbyManager.singleton.loadedPlayers)
            {
                p.FirstRespawnPlayer();
            }
            GameManager.instance.NetRpcSendMessage(NetMsg.PlaySound, 0f, true);
            GameManager.instance.NetRpcSendMessage(NetMsg.ScreenMsg, UIGame.instance.textManager.startMsg);
            GameManager.instance.NetRpcSendMessage(NetMsg.PlaySound, 1f, true);
            if (ultimateUnlockCo != null) StopCoroutine(ultimateUnlockCo);
            ultimateUnlockCo = StartCoroutine(CoUltimateUnlock());
            
            foreach (GamePlayer p in NetLobbyManager.singleton.loadedPlayers)
            { 
                p.m_lobbyPlayer.totalStats.SummStats(p.m_lobbyPlayer.stats);
                p.m_lobbyPlayer.stats = new PlayerStats();
                p.SyncPlayerStats();//.m_lobbyPlayer.NetRpcSendMessage(NetMsg.SetStats, p.m_lobbyPlayer.stats.SerializeAll());
                p.NetTargetSendMessage(NetMsg.StartCooldown, p.connectionToClient, (int)AbilityType.Ultimate, m_ultimateStartCooldown);
                if (!p.isLocalPlayer)
                {
                    p.StartCoroutine(p.CoApplyCooldownForced(AbilityType.Ultimate, m_ultimateStartCooldown));
                }
            }
        }

        public void SetLayer(LayerMask layerMask)
        {
            StopCoroutine("CoSetLayer");
            destroyOnDeathCos.Add(StartCoroutine(CoSetLayer(layerMask)));
        }

        public void SetParameterAt(SoundEmitterType emitterType, string parameter, float paramValue)
        {
            playerSoundEmitters[(int)emitterType].SetParameter(parameter, paramValue);
        }

        public void PlaySoundAt(SoundEmitterType emitterType, bool toPlay = true, bool restart = false)
        {
            if (playerSoundEmitters != null && playerSoundEmitters.Count > (int)emitterType)
            {
                if (toPlay)
                {
                    if(restart)
                    {
                        playerSoundEmitters[(int)emitterType].Stop();
                        playerSoundEmitters[(int)emitterType].Play();
                    }
                    else if (!playerSoundEmitters[(int)emitterType].IsPlaying())
                    {
                        playerSoundEmitters[(int)emitterType].Play();
                    }
                }
                else
                {
                    playerSoundEmitters[(int)emitterType].Stop();
                }
            }
        }


        public void PlaySound(string sound, string parametersString = "", bool notSame = false)
        {
            if (sound == "") return;
            if(notSame)
            {
                foreach(StudioEventEmitter see in tmpSoundEmitters)
                {
                    if (see.Event == sound) return;
                }
            }
            StudioEventEmitter tmpEmitter = gameObject.AddComponent<StudioEventEmitter>();
            tmpSoundEmitters.Add(tmpEmitter);
            tmpEmitter.Event = sound;
            tmpEmitter.Play();
            if (parametersString != "")
            {
                string[] parameters = DeserializeMessage(parametersString);
                for(int i = 0; i < parameters.Length; i+=2)
                {
                    tmpEmitter.SetParameter(parameters[i], Convert.ToSingle(parameters[i + 1]));
                }
            }
            StartCoroutine(CoDestroyOnStop(tmpEmitter));
        }

        IEnumerator CoDestroyOnStop(StudioEventEmitter emitter)
        {
            yield return new WaitUntil(() => emitter.IsPlaying() == false);
            tmpSoundEmitters.Remove(emitter);
            Destroy(emitter);
        }

        public void SendSoundAt(TargetType targetType, SoundEmitterType emitterType, bool start)
        {

        }

        public void SendSound(Ability ability)
        {
            TargetType targetType = ability.soundTarget;
            string sound = ability.abilitySound;
            string parameter = ability.soundParameter;
            float parameterValue = ability.soundParameterValue;
            if (parameter != "") sound = SerializeMessage(sound, parameter, parameterValue);
            switch (targetType)
            {
                case TargetType.Me:
                {
                    NetTargetSendMessage(NetMsg.PlaySound, connectionToClient, sound);
                    break;
                }
                case TargetType.MyTeam:
                {
                    foreach (GamePlayer player in allPlayers)
                    {
                        if (player.Team == Team)
                        {
                            player.NetTargetSendMessage(NetMsg.PlaySound, player.connectionToClient, sound);
                        }
                    }
                    break;
                }
                case TargetType.EnemyTeam:
                {
                    foreach (GamePlayer player in allPlayers)
                    {
                        if (player.Team != Team)
                        {
                            player.NetTargetSendMessage(NetMsg.PlaySound, player.connectionToClient, sound);
                        }
                    }
                    break;
                }
                case TargetType.EveryOne:
                {
                    NetRpcSendMessage(NetMsg.PlaySound, sound);
                    break;
                }
            }
        }

        public void SetHitLayer(LayerMask layerMask)
        {
            if (m_serverColliders == null) return;
            foreach (Collider c in m_serverColliders)
            {
                c.gameObject.layer = (int)Mathf.Log(layerMask.value, 2);                
            }
        }

        public void SetRagdoll(bool on)
        {
            if (m_serverSeekerAnimator != null)
            {
                if (on)
                {
                    m_serverSeekerAnimator.enabled = false;
                }
                else
                {
                    m_serverSeekerAnimator.enabled = true;
                }
            }
            Collider baseCollider = GetComponent<Collider>();
            if (m_serverPlayerHead == null)
            {
                m_serverPlayerHead = GetComponentInChildren<GamePlayerHead>();
            }
            
            Collider bodyCollider = GetComponentInChildren<GamePlayerBody>()?.GetComponent<Collider>();
            if (m_serverColliders == null) return;
            foreach (Collider c in m_serverColliders)
            {
                if (c != baseCollider && c.gameObject != m_serverPlayerHead.gameObject && c != bodyCollider)
                {
                    Rigidbody rb = c.GetComponent<Rigidbody>();
                    if (on)
                    {
                        c.isTrigger = false;
                        if (rb)
                        {
                            rb.isKinematic = false;
                            rb.useGravity = true;
                        }
                    }
                    else
                    {
                        c.isTrigger = true;
                        if (rb)
                        {
                            rb.isKinematic = true;
                            rb.useGravity = false;
                        }
                    }
                }
            }
            if(!on && m_serverSidedModel != null && m_serverBonesLocalPos != null && m_serverBonesLocalRot != null)
            {
                int k = 0;
                foreach (Transform t in m_serverSidedModel.GetComponentsInChildren<Transform>())
                {
                    t.localPosition = m_serverBonesLocalPos[k];
                    t.localRotation = Quaternion.Euler(m_serverBonesLocalRot[k]);
                    k++;
                }
            }
        }

        public void ResetLayer()
        {
            if (Team == Team.A)
            {
                SetLayer(teamALayer);
            }
            if (Team == Team.B)
            {
                SetLayer(teamBLayer);
            }
        }

        void StopAllParticles()
        {
            deathRay.gameObject.SetActive(false);
            stunnedParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            slowedParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            slowedParticle.transform.parent.gameObject.SetActive(false);
            vayvinMark.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            snaredParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            giveEnergyEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (isServer)
            {
                NetRpcSendMessage(NetMsg.SpawnPS, "energy", false);
            }
            if (criticalIcon.activeSelf)
            {
                criticalIcon.SetActive(false);
            }
        }

        #region Coroutines

        IEnumerator CoSetLayer(LayerMask layerMask)
        {
            if (gameObject == null) yield break;
            gameObject.layer = (int)Mathf.Log(layerMask.value, 2);
            //if (m_serverPlayerHead == null) yield break;
            //m_serverPlayerHead.gameObject.layer = (int)Mathf.Log(layerMask.value, 2);
        }


        IEnumerator CoStartDeathRay()
        {
            //Debug.Log("start ray");
            deathRay.gameObject.SetActive(true);
            var lenght = deathRayLength;  
            var maxlenght = 200f;
  
            while (Mathf.Abs(lenght - maxlenght) > 1f)
            {
                lenght = Mathf.Lerp(lenght, maxlenght, Time.deltaTime * 0.5f);
          
                deathRay.SetPosition(1, new Vector3(0, 0, lenght));
                deathRay.SetPosition(0, new Vector3(0, 0, lenght-deathRayLength));
                yield return null;
            }
            deathRay.SetPosition(1, new Vector3(0, 0, deathRayLength));
            deathRay.SetPosition(0,  Vector3.zero);
            deathRay.gameObject.SetActive(false);
        }

        IEnumerator CoResetTeleport()
        {
            yield return new WaitForSeconds(1f);
            foreach (NetTransform nt in m_netTransforms)
            {
                nt.forceTeleport = false;
            }
            netTransformSync = null;
        }

        public void SyncPlayerStats(bool roundStats = true)
        {
            if (roundStats)
            {
                LobbyPlayer.NetRpcSendMessage(NetMsg.SetStats, LobbyPlayer.stats.SerializeAll());
            }
            else
            {
                LobbyPlayer.NetRpcSendMessage(NetMsg.SetStats, LobbyPlayer.totalStats.SerializeAll());
            }

        }

        //IEnumerator CoDisableGravity(float disableTime)
        //{
        //    m_isAffectedByGravity = false;
        //    NetTargetSendMessage(NetMsg.Update, connectionToClient, (int)GamePlayerState.Gravity, m_isAffectedByGravity);
        //    yield return new WaitForSeconds(disableTime);
        //    m_isAffectedByGravity = true;
        //    NetTargetSendMessage(NetMsg.Update, connectionToClient, (int)GamePlayerState.Gravity, m_isAffectedByGravity);
        //}

        IEnumerator CoSpeedMultiplier(float multiplier, float activeTime)
        {
            m_speedMultiplier *= multiplier;
            NetRpcSendMessage(NetMsg.Update, (int)GamePlayerState.SpeedMult, m_speedMultiplier);
            yield return new WaitForSeconds(activeTime);
            m_speedMultiplier = 1f;
            NetRpcSendMessage(NetMsg.Update, (int)GamePlayerState.SpeedMult, m_speedMultiplier);
        }

        IEnumerator CoDisableMouse(float disableTime)
        {
            m_canInputMouse = false;
            NetRpcSendMessage(NetMsg.Update, (int)GamePlayerState.CanInputMouse, m_canInputMouse);
            yield return new WaitForSeconds(disableTime);
            m_canInputMouse = true;
            NetRpcSendMessage(NetMsg.Update, (int)GamePlayerState.CanInputMouse, m_canInputMouse);
        }

        IEnumerator CoNoClipTime(float disableTime)
        {
            SetLayer(notTeamLayer);
            NetRpcSendMessage(NetMsg.Update, (int)GamePlayerState.NoClipTime, true);
            yield return new WaitForSeconds(disableTime);
            ResetLayer();
            NetRpcSendMessage(NetMsg.Update, (int)GamePlayerState.NoClipTime, false);
        }

        IEnumerator CoApplySnare(float disableTime)
        {
            NetRpcSendMessage(NetMsg.Update, (int)GamePlayerState.SpeedMult, 0f);
            NetRpcSendMessage(NetMsg.Update, (int)GamePlayerState.CanUseMobility, false);
            yield return new WaitForSeconds(disableTime);
            m_speedMultiplier = 1f;
            NetRpcSendMessage(NetMsg.Update, (int)GamePlayerState.SpeedMult, m_speedMultiplier);
            NetRpcSendMessage(NetMsg.Update, (int)GamePlayerState.CanUseMobility, true);
        }

        IEnumerator CoApplyStun(float disableTime)
        {
            m_canInputMouse = false;
            m_canInput = false;
            NetRpcSendMessage(NetMsg.Update, (int)GamePlayerState.CanInputMouse, m_canInputMouse);
            NetRpcSendMessage(NetMsg.Update, (int)GamePlayerState.CanInput, m_canInput);
            yield return new WaitForSeconds(disableTime);
            m_canInputMouse = true;
            m_canInput = true;
            NetRpcSendMessage(NetMsg.Update, (int)GamePlayerState.CanInputMouse, m_canInputMouse);
            NetRpcSendMessage(NetMsg.Update, (int)GamePlayerState.CanInput, m_canInput);
        }

        IEnumerator CoApplyDmgReduction(float disableTime)
        {
            yield return new WaitForSeconds(disableTime);
            m_damageReductionMultiplier = 1f;
        }

        IEnumerator CoApplyInvulnerability(float disableTime)
        {
            m_isInvulnerable = true;
            yield return new WaitForSeconds(disableTime);
            m_isInvulnerable = false;
        }


        IEnumerator CoNoEnergyPickup(float disableTime)
        {
            m_canGetEnergy = false;
            yield return new WaitForSeconds(disableTime);
            m_canGetEnergy = true;
        }

        IEnumerator CoSavePlayerTimer(float disableTime)
        {
            Debug.Log("[" + LobbyPlayer.id + "] Saving id " + lastPlayerHitID + " for " + disableTime + " seconds");
            yield return new WaitForSeconds(disableTime);       
            NetTargetSendMessage(NetMsg.UpdateLastPlayerHitId, connectionToClient, -1);
        }

        IEnumerator CoRedirectionTime(float disableTime, AbilityType redirected, AbilityType substitute)
        {
            yield return new WaitForSeconds(disableTime);
            foreach(AbilityType[] ab in abilityRedirection)
            {
                if(ab[0] == redirected && ab[1] == substitute)
                {
                    abilityRedirection.Remove(ab);
                    break;
                }
            }
        }

        IEnumerator CoAddImpulse(Effect effect, System.Func<bool> stopCondition, float timer, int applierId)
        {
            Vector3 direction = effect.appliedForceDirection.normalized;
            GamePlayer applierPlayer = Find(applierId);
            if (direction == Vector3.zero)
            {
                direction = (effect.forceOnCameraForward ? mainCamera.transform.forward : m_body.velocity.normalized);
                if (effect.forceOnCameraForward)
                {
                    float angle = -Vector3.SignedAngle(transform.forward, direction, mainCamera.transform.right);
                    if(effect.minCameraElevation > angle)
                    {
                        angle = Mathf.Deg2Rad * effect.minCameraElevation;
                        direction = transform.up * Mathf.Sin(angle) + transform.forward * Mathf.Cos(angle);
                    }
                }
                direction = (effect.applyForceTowardAttacker ? applierPlayer.transform.position - transform.position : direction);
            }
            float verticalAngle = Vector3.SignedAngle(direction, transform.forward, mainCamera.transform.right);
            m_body.velocity = direction * effect.appliedForceIntensity * Mathf.Cos(Mathf.Deg2Rad * verticalAngle);
            if (effect.keepForwardMomentum)
            {
                m_isAffectedByForces = true;
            }
            yield return new WaitForSeconds(timer);
            yield return new WaitUntil(stopCondition);
            if (effect.keepForwardMomentum)
            {
                m_isAffectedByForces = false;
            }

        }

        IEnumerator CoAddForce(Effect effect, System.Func<bool> stopCondition, int applierId)
        {
            Vector3 direction = effect.appliedForceDirection.normalized;
            GamePlayer applierPlayer = Find(applierId);
            if (direction == Vector3.zero)
            {
                direction = (effect.forceOnCameraForward ? mainCamera.transform.forward : m_body.velocity.normalized);
            }
            m_body.velocity = direction * effect.appliedForceIntensity;

            if (effect.deactivateGravityTime > 0f)
            {
                m_isAffectedByGravity = false;
            }
            if (effect.keepForwardMomentum)
            {
                m_isAffectedByForces = true;
            }
            if (effect.appliedForceTime > 0f)
            {
                float timer = 0f;
                while(timer < effect.appliedForceTime)
                {
                    m_body.velocity = direction * effect.appliedForceIntensity;
                    yield return null;
                    if(stopCondition())//effect.deactivateWithInput && m_inputManager.GetKey(InputKey.Any))
                    {
                        if (effect.deactivateGravityTime > 0f)
                        {
                            m_isAffectedByGravity = true;
                        }
                        if (effect.keepForwardMomentum)
                        {
                            m_isAffectedByForces = false;
                        }
                        m_body.velocity = new Vector3(m_body.velocity.x, 0f, m_body.velocity.z);
                        yield break;
                    }
                    if (effect.followCameraDirection)
                    {
                        direction = mainCamera.transform.forward;
                    }
                    timer += Time.deltaTime;
                }
            }
            if (effect.deactivateGravityTime > 0f)
            {
                m_isAffectedByGravity = true;
            }
            if (effect.keepForwardMomentum)
            {
                m_isAffectedByForces = false;
            }
            m_body.velocity = new Vector3(m_body.velocity.x, 0f, m_body.velocity.z);
        }

        IEnumerator CoAddPull(Effect effect, System.Func<bool> stopCondition, int applierId)
        {
            Vector3 direction = effect.appliedForceDirection.normalized;
            GamePlayer applierPlayer = Find(applierId);
            direction = applierPlayer.transform.position - transform.position;
            m_body.velocity = direction * effect.appliedForceIntensity;
            //applierPlayer.NetTargetSendMessage(NetMsg.Update, connectionToClient, (int)GamePlayerState.CanAttack, false);
            //NetTargetSendMessage(NetMsg.Update, connectionToClient, (int)GamePlayerState.CanAttack, false);
            if (effect.keepForwardMomentum)
            {
                m_isAffectedByForces = true;
            }
            yield return null;
            if (effect.appliedForceTime > 0f)
            {
                float timer = 0f;
                float appliedDistance = direction.sqrMagnitude;
                while (timer < effect.appliedForceTime && direction.sqrMagnitude > 0.4f)
                {
                    m_body.velocity = direction * effect.appliedForceIntensity;
                    yield return null;
                    if (stopCondition())
                    {
                        break;
                    }
                    direction = applierPlayer.transform.position - transform.position;
                    if(appliedDistance < direction.sqrMagnitude)
                    {
                        break;
                    }
                    else
                    {
                        appliedDistance = direction.sqrMagnitude;
                    }
                    timer += Time.deltaTime;
                }
            }

            if (effect.keepForwardMomentum)
            {
                m_isAffectedByForces = false;
            }
            //applierPlayer.NetTargetSendMessage(NetMsg.Update, connectionToClient, (int)GamePlayerState.CanAttack, true);
            //NetTargetSendMessage(NetMsg.Update, connectionToClient, (int)GamePlayerState.CanAttack, true);
            m_body.velocity = new Vector3(m_body.velocity.x, 0f, m_body.velocity.z);
        }


        IEnumerator CoWaitPlayersLoaded()
        {
            // Wait until all GamePlayers are loaded
            yield return new WaitUntil(() => allPlayers.Count == NetLobbyManager.singleton.netLobbyPlayers.Length);
            Debug.Log("GamePlayers:" + allPlayers.Count);
            // Check if everyone is spawned
            bool flag = true;
            while (flag)
            {
                flag = false;
                foreach (GamePlayer p in allPlayers)
                {
                    if(p.transform == null || p.gameObject == null)
                    {
                        flag = true;
                        Debug.Log("GamePlayer not spawned yet");
                        yield return null;
                        break;
                    }
                }
            }
            NetCmdSendMessage(NetMsg.SetReady);
        }

        IEnumerator CoWaitLobbyPlayerLocal(NetMsgType netMsgType)
        {
            yield return new WaitUntil(() => local != null && local.LobbyPlayer != null);

            // Set player name and titan
            playerNameText.text = LobbyPlayer.playerName;
            myTeamTitan = GameManager.instance.GetTitan(Team).gameObject;
            // Set players colors
            if (Team == local.Team)
            {
                myColor = allyColor;
            }
            else
            {
                myColor = enemyColor;
                //lifeBarFullRenderer.gameObject.SetActive(false);
                //lifeBarFullRenderer.GetComponentInChildren<SpriteRenderer>().material = enemyLifebarMaterial;
                lifeBarFullRenderer.material = enemyLifebarMaterial;
                lifeBarRenderer.material= enemyLifebarMaterial;
            }

            // Set player lifebar settings
            //lifeBarFullRenderer.color = myColor * 0.5f;
            lifeBarRenderer.color = myColor;

            // Initialize cooldowns in localplayers and server
            if ((isServer || isLocalPlayer) && abilitiesCooldowns.Count == 0)
            {
                foreach (Ability ability in Abilities)
                {
                    if (ability != null)
                    {
                        abilitiesCooldowns.Add(0f);
                    }
                }
                Debug.Assert(abilitiesCooldowns.Count == Abilities.Length, "Abilities in seeker set up not correctly, remove None ones");
            }

            // Load right model onto player
            if (m_modelBody.transform.childCount == 0)
            {
                // Load server sided model with animator
                m_serverSidedModel = Instantiate(Seeker.bodyPrefab, m_modelBody);
                outline = m_serverSidedModel.GetComponent<Outline>();
                m_serverColliders = m_serverSidedModel.GetComponentsInChildren<Collider>();
                m_serverPlayerHead = m_serverSidedModel.GetComponentInChildren<GamePlayerHead>();
                SetHitLayer(baseLayer);

                ModelTransformReference mtr = m_serverSidedModel.GetComponent<ModelTransformReference>();
                Vector3 clientLocalPosition = mtr.clientRelativePosition;
                m_serverSeekerAnimator = GetComponentInChildren<Animator>();
                if (!isLocalPlayer)
                {
                    if (Team == local.LobbyPlayer.team)
                    {
                        outline.OutlineColor = allyColor;
                        playerNameText.GetComponent<MeshRenderer>().material = AllyNameText;
                    }
                    else
                    {
                        playerNameText.GetComponent<MeshRenderer>().material = AllyNameText;
                        outline.OutlineColor = enemyColor;
                        outline.OutlineMode = Outline.Mode.OutlineVisible;
                    }
                }
                else
                {
                    outline.OutlineWidth = 0f;
                }
                // Set references for attack spawn points
                for (int i = 0; i < m_attackSpawnPoints.Length; i++)
                {
                    m_attackSpawnPoints[i] = mtr.spawnPoints[i];
                }

                // Reference for body spine bone
                m_modelBodySpines = mtr.verticalRotationBones;
                m_modelBodySpinesOutsideAnimator = mtr.verticalRotationsOutsideAnimator;
                m_serverBonesLocalPos = new List<Vector3>();
                m_serverBonesLocalRot = new List<Vector3>();
                foreach (Transform t in m_serverSidedModel.GetComponentsInChildren<Transform>())
                {
                    m_serverBonesLocalPos.Add(t.localPosition);
                    m_serverBonesLocalRot.Add(t.localRotation.eulerAngles);
                }
                // Sound emitters
                playerSoundEmitters[(int)SoundEmitterType.Walk].Event = Seeker.walk; 

                if (isLocalPlayer)
                {
                    // Disable/enable server/client only components
                    if (GetComponentInChildren<LookAtCamera>() != null)
                    {
                        GetComponentInChildren<LookAtCamera>().gameObject.SetActive(false);
                    }
                    if (GetComponentInChildren<StudioListener>())
                    {
                        GetComponentInChildren<StudioListener>().enabled = true;
                    }
                    outline.enabled = false;
                    SkinnedMeshRenderer localServerSMR = m_serverSidedModel.GetComponentInChildren<SkinnedMeshRenderer>();
                    
                    // Show server model shadow only on local client
                    if (localServerSMR)
                    {
                        localServerSMR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
                    }

                    // Load client sided model with animator
                    m_clientSidedModel = Instantiate(Seeker.bodyPrefab, m_modelBodyArms);
                    m_clientSidedModel.transform.localPosition = mtr.clientRelativePosition;
                    m_clientSidedModel.transform.localRotation = Quaternion.Euler(mtr.clientRelativeRotation);
                    ModelTransformReference localmtr = m_clientSidedModel.GetComponent<ModelTransformReference>();
                    Outline localOutline = m_clientSidedModel.GetComponent<Outline>();
                    
                    // Hide some bones for better animations
                    if (localmtr != null && localmtr.bonesToHide != null)
                    {
                        foreach (Transform t in localmtr.bonesToHide)
                        {
                            t.localScale = Vector3.one * mtr.hideScaleValue;
                        }
                    }
                    if (localOutline)
                    {
                        localOutline.OutlineWidth = 0f;
                    }

                    // Assing local animator
                    m_clientSeekerAnimator = m_clientSidedModel.GetComponentInChildren<Animator>();
                    if (m_clientSeekerAnimator)
                    {
                        m_clientSeekerAnimator.runtimeAnimatorController = mtr.clientAnimatorController;//m_baseAnimator;
                        m_clientSeekerAnimator.logWarnings = false;
                    }

                    // Set only shadow to show for every renderer in local player
                    foreach (MeshRenderer mr in m_serverSidedModel.GetComponentsInChildren<MeshRenderer>())
                    {
                        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
                    }

                    // Set mesh without shadows to show for every renderer in local player
                    foreach (MeshRenderer mr in m_clientSidedModel.GetComponentsInChildren<MeshRenderer>())
                    {
                        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    }

                    // Disable shadow from local player model(1st person)
                    SkinnedMeshRenderer localClientSMR = m_clientSidedModel.GetComponentInChildren<SkinnedMeshRenderer>();
                    if (localClientSMR)
                    {
                        localClientSMR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    }
                    // Destroy hit colliders on client model
                    foreach (CharacterJoint c in m_clientSidedModel.GetComponentsInChildren<CharacterJoint>())
                    {
                        Destroy(c);
                    }
                    foreach (Collider c in m_clientSidedModel.GetComponentsInChildren<Collider>())
                    {
                        Destroy(c);
                    }
                    foreach (Rigidbody c in m_clientSidedModel.GetComponentsInChildren<Rigidbody>())
                    {
                        Destroy(c);
                    }
                    GamePlayerHead tmp = m_clientSidedModel.GetComponentInChildren<GamePlayerHead>();
                    Destroy(tmp);
                    if (netMsgType == NetMsgType.Target) NetCmdSendMessage(NetMsg.SetLobbyPlayer, LobbyPlayer.id);
                }
            }

            // Team based layers                            
            ResetLayer();
            Debug.Log("Player " + LobbyPlayer.id + " loaded on " + (netMsgType == NetMsgType.Target ? "client" : "other clients"));

        }

        IEnumerator CoCheckBoundaries()
        {
            float bound = GameManager.instance.deathZoneLimit;
            while (true)
            {
                yield return new WaitForSeconds(1f);
                if (Mathf.Abs(transform.position.x) > bound || Mathf.Abs(transform.position.y) > bound || Mathf.Abs(transform.position.z) > bound)
                {
                    if (!m_isDead)
                    {
                        OnPlayerDeath(this);
                    }
                    //m_health = 0f;
                }
            }
        }

        IEnumerator CoCleanupLastDamager(GamePlayer damager)
        {
            yield return new WaitForSeconds(m_assistTimer);
            if(m_lastDamagersList.Contains(damager))
                m_lastDamagersList.Remove(damager);
        }
        
        IEnumerator CoApplyCooldown(AbilityType abilityType, bool isReload = false)
        {

            if (isLocalPlayer) UIGame.instance.StartAbilityIconCooldown(abilityType);
            int abilityIndex = (int)abilityType;
            if (abilitiesCooldowns[abilityIndex] > 0f) yield break;
            abilitiesCooldowns[abilityIndex] = Seeker.abilities[abilityIndex].cooldown;
            if (isReload)
            {
                abilitiesCooldowns[abilityIndex] = Seeker.abilities[abilityIndex].reloadCooldown;
            }
            if (Seeker.abilities[abilityIndex].maxAmmo > 0)
            {
                if (isServer)
                {
                    if (basicAttackAmmo > 0)
                    {
                        basicAttackAmmo--;
                        if (basicAttackAmmo == 0)
                        {
                            abilitiesCooldowns[abilityIndex] = Seeker.abilities[abilityIndex].reloadCooldown;
                            basicAttackAmmo = Seeker.abilities[abilityIndex].maxAmmo;
                            NetTargetSendMessage(NetMsg.Reload, connectionToClient);
                        }
                        NetTargetSendMessage(NetMsg.Update, connectionToClient, (int)GamePlayerState.Ammo, basicAttackAmmo);
                    }
                }
            }
            while (abilitiesCooldowns[abilityIndex] > 0f)
            {
                abilitiesCooldowns[abilityIndex] -= Time.deltaTime;
                yield return null;
            }
            abilitiesCooldowns[abilityIndex] = 0f;
        }

        IEnumerator CoApplyCooldownForced(AbilityType abilityType, float time)
        {
            yield return new WaitUntil(() => abilitiesCooldowns.Count > (int)abilityType);
            if(isLocalPlayer) UIGame.instance.StartAbilityIconCooldown(abilityType);
            int abilityIndex = (int)abilityType;
            if (abilitiesCooldowns[abilityIndex] > 0f)
            {
                abilitiesCooldowns[abilityIndex] = time;
                yield break;
            }
            abilitiesCooldowns[abilityIndex] = time;
            while (abilitiesCooldowns[abilityIndex] > 0f)
            {
                abilitiesCooldowns[abilityIndex] -= Time.deltaTime;
                yield return null;
            }
            abilitiesCooldowns[abilityIndex] = 0f;
        }

        IEnumerator CoDeliverEnergy(GameTitan myTitan)
        {
            giveEnergyEffect.Play(true);
            NetTargetSendMessage(NetMsg.PlaySoundAt, connectionToClient, (int)SoundEmitterType.Energy, true, true);
            NetTargetSendMessage(NetMsg.SetParameterAt, connectionToClient, (int)SoundEmitterType.Energy, "TitanCharging", 0f);
            NetRpcSendMessage(NetMsg.SpawnPS, "energy", true);
            while (Energy > 0f)
            {
                if (GameManager.instance.isRoundOver) yield break;
                myTitan.GetEnergyFromPlayer(this);//, Mathf.Clamp(Energy - myTitan.m_energyFromPlayerPerTick,0f,Mathf.Infinity));
                yield return new WaitForSeconds(myTitan.m_energyFromPlayerTickTime);
            }
            giveEnergyEffect.Stop(true,ParticleSystemStopBehavior.StopEmitting);
            NetTargetSendMessage(NetMsg.SetParameterAt, connectionToClient, (int)SoundEmitterType.Energy, "TitanCharging", 1f);
            NetRpcSendMessage(NetMsg.SpawnPS, "energy", false);
            gameTitanCo = null;

        }
        public  void SpawnHp()
        {
            if (ShowHp == null)
            {
                ShowHp = StartCoroutine(CoShowHp());
            }
            else
            {
                StopCoroutine(ShowHp);
                ShowHp = StartCoroutine(CoShowHp());
            }
        }

        IEnumerator CoShowHp()
        {
            //lifeBarFullRenderer.gameObject.SetActive(true);
            outline.OutlineMode = Outline.Mode.OutlineAll; 
            yield return new WaitForSeconds(showHpTime);
            outline.OutlineMode = Outline.Mode.OutlineVisible;
           // lifeBarFullRenderer.gameObject.SetActive(false);
            ShowHp = null;
        }
        
        public void StartMultiKill()
        {
            StopCoroutine("CoMultiKill");
            StartCoroutine(CoMultiKill());
        }

        IEnumerator CoMultiKill()
        {
            multikillCount++;
            if (multikillCount == 2)
            {
                GameManager.instance.NetTargetSendMessage(NetMsg.ScreenMsg, connectionToClient, UIGame.instance.textManager.doubleKillMsg);
            }
            else if(multikillCount == 3)
            {
                GameManager.instance.NetTargetSendMessage(NetMsg.ScreenMsg, connectionToClient, UIGame.instance.textManager.tripleKillMsg);
                multikillCount = 0;
            }
            yield return new WaitForSeconds(multikillTime);            
            multikillCount = 0;
        }

        IEnumerator CoUltimateUnlock()
        {
            yield return new WaitForSeconds(m_ultimateStartCooldown);
            GameManager.instance.NetRpcSendMessage(NetMsg.ScreenMsg, UIGame.instance.textManager.ultimateStartMsg);
            ultimateUnlockCo = null;
        }

        IEnumerator CoSpawnAttack(GamePlayer player, Ability ability, Vector3 hitPoint)
        {
            float timer = 0f;
            for(int i = 1; i < ability.spawnNumber; i++)
            {
                timer = 0f;
                while(timer < ability.multiSpawnInterval)
                {
                    yield return null;
                    if (!m_canInput && !m_canInputMouse && ability.stopIfStunned)
                    {
                        yield break;
                    }
                    if (!m_canAttack && ability.stopIfCannotAttack)
                    {
                        yield break;
                    }
                    if (m_isDead && ability.stopIfDead)
                    {
                        yield break;
                    }
                    timer += Time.deltaTime;
                }
                //Vector3 hitPoint = GetPlayerFacingPoint(player, ability);
                Vector3 spawnPos;
                Quaternion spawnRot;
                if (ability.attackPrefabSpawnPoint == AttackSpawnPoint.None)
                {
                    spawnPos = hitPoint;
                    spawnRot = transform.rotation;
                }
                else
                {
                    hitPoint = GetPlayerFacingPoint(player, ability);
                    spawnPos = m_attackSpawnPoints[(int)ability.attackPrefabSpawnPoint].position;
                    spawnRot = Quaternion.LookRotation(hitPoint - spawnPos, Vector3.up); // m_attackSpawnPoints[(int)ability.attackPrefabSpawnPoint].rotation;
                    if (ability.isAttachedToPlayer)
                    {
                        spawnRot = m_attackSpawnPoints[(int)ability.attackPrefabSpawnPoint].rotation;
                    }
                }
                GameObject attack = Instantiate(ability.attackPrefab, spawnPos, spawnRot);
                attack.GetComponent<AttackBehaviour>().SetupAttack(this, ability, hitPoint);
                NetworkServer.Spawn(attack);
            }
            yield return null;
        }

        Vector3 GetPlayerFacingPoint(GamePlayer player, Ability ability)
        {
            RaycastHit cameraHitPoint;
            Ray cameraRay = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
            Vector3 pointToShoot = cameraRay.origin + cameraRay.direction * ability.range;
            SetLayer(temporaryGhostLayer);
            SetHitLayer(temporaryGhostLayer);
            if (Physics.Raycast(cameraRay, out cameraHitPoint, ability.range, aimableLayer))
            {
                pointToShoot = cameraHitPoint.point;
            }
            SetHitLayer(baseLayer);
            ResetLayer();
            return pointToShoot;
        }
        #endregion
    }
}
