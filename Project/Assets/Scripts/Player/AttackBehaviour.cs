using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace VoK
{

    public class AttackBehaviour : NetBehaviour
    {
        public LayerMask collisionMask;
        public LayerMask wallMask;
        public Ability m_linkedAbility;
        int abilityId; 
        public Vector3 startPosition;
        public GamePlayer m_playerOwner;
        public List<Effect> m_effects;// { get { return linkedAbility.effects; } }
        public bool m_isMelee;
        public bool m_isAttackSetup = false;

        public bool m_destroyOnFirstHit = true;
        public bool m_canHitMultipleTimes = false;
        public List<GamePlayer> alreadyHitPlayers;

        public Vector3 lastPosition;
        public Vector3 lastHitPoint;
        public Vector3 lastHitDirection;

        GameObject attackModel;
        GameObject firstPersonAttackModel;
        float lastFrameVerticalSpeed;
        public Collider[] myColliders;
        Rigidbody m_body = null;

        private void Awake()
        {
            attackModel = null;
            m_isAttackSetup = false;
            m_playerOwner = null;
        }

        void DisableColliders()
        {
            foreach (Collider col in myColliders)
            {
                if (col != null)
                {
                    col.enabled = false;
                }
            }
        }

        void EnableColliders()
        {
            foreach (Collider col in myColliders)
            {
                if (col != null)
                {
                    col.enabled = true;
                }
            }
        }

        private void Start()
        {
            if (isServer)
            {
                m_body = GetComponent<Rigidbody>();

                // Initialize players hit list
                alreadyHitPlayers = new List<GamePlayer>();
                
                // Initialize positions for each frame raycast
                startPosition = transform.position;
                lastPosition = m_body.position;

                // Initialize exact collision position
                lastHitPoint = Vector3.zero;
                lastHitDirection = Vector3.zero;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            //if(MenuManager.Instance.showDebugLog) Debug.Log(name + " hit " + other.name);
            if (isServer)
            {
                // Check exact collision point
                RaycastHit hit;
                if (Physics.Raycast(lastPosition, m_body.position - lastPosition, out hit, (m_body.position - lastPosition).magnitude, wallMask))
                {
                    // Get point and normal
                    lastHitPoint = hit.point;
                    lastHitDirection = hit.normal;
                }

                // When a player has been hit
                if (other.tag == "Player")
                {
                    GamePlayer hitPlayer = other.GetComponentInParent<GamePlayer>();

                    // Avoid self hit and check if the player hasn't been hit yet
                    if ((hitPlayer != m_playerOwner || m_linkedAbility.canHitMe) && !alreadyHitPlayers.Contains(hitPlayer))
                    {                        
                        // If the attack can't hit multiple times, add player hit on list
                        if (!m_canHitMultipleTimes)
                        {
                            alreadyHitPlayers.Add(hitPlayer);
                        }

                        bool headshot = other.GetComponent<GamePlayerHead>() != null;

                        // Check every effect in the attack
                        foreach (Effect e in m_effects)
                        {
                            if ((e.canHitEnemy && hitPlayer.LobbyPlayer.team != m_playerOwner.LobbyPlayer.team) || (e.canHitAllies && hitPlayer.LobbyPlayer.team == m_playerOwner.LobbyPlayer.team))
                            {
                                hitPlayer.ApplyEffect(e, m_playerOwner.LobbyPlayer.id, this, abilityId, headshot && e.headshotMult > 1f);
                                if (e.particleEffect != null)
                                {
                                    if(e.damage >= 0f)
                                    {
                                        if (m_isMelee)
                                        {
                                            lastHitPoint = hitPlayer.transform.position + Vector3.up * 2f;
                                            lastHitDirection = hitPlayer.transform.position - m_playerOwner.transform.position;
                                        }
                                        SpawnParticleEffect(e.particleEffect.name);
                                    }
                                    else if(!hitPlayer.HasFullHealth())
                                    {
                                        SpawnParticleEffect(e.particleEffect.name);
                                    }
                                }
                            }
                        }
                        // If the attack can hit only one entity
                        if (m_destroyOnFirstHit)
                        {
                            StartCoroutine(CoDestroyAttack());
                        }
                       
                    }
                }
                // If hit layer is in collision layer mask -> Destroy it

                if (m_linkedAbility.attackDestroysOnWall && wallMask == (wallMask | (1 << other.gameObject.layer)))
                {
                    if (m_linkedAbility.hitWallPrefab != null)
                    {
                        SpawnParticleEffect(m_linkedAbility.hitWallPrefab.name);
                    }
                    if (m_linkedAbility.missPrefab != null)
                    {
                        SpawnParticleEffect(m_linkedAbility.missPrefab.name);
                    }
                    StartCoroutine(CoDestroyAttack());
                }     
                
            }
        }

        private void Update()
        {
            if (isServer)
            {
                if (m_isAttackSetup)
                {
                    // Follow player bone for melee
                    if (m_isMelee)
                    {
                        transform.position = m_playerOwner.m_attackSpawnPoints[(int)m_linkedAbility.attackPrefabSpawnPoint].position;
                        if(m_linkedAbility.attackFollowBoneRotation)
                        {
                            transform.rotation = m_playerOwner.m_attackSpawnPoints[(int)m_linkedAbility.attackPrefabSpawnPoint].rotation;
                        }
                       
                    }
                    if(myColliders != null)
                    {
                        if(myColliders.Length > 0)
                        {
                            if(!IsAnyColliderEnabled())
                            {
                                EnableColliders();
                            }
                        }
                    }
                    if(m_playerOwner != null)
                    {
                        if(m_playerOwner.IsDead)
                        {
                            if(m_linkedAbility != null && m_linkedAbility.destroyAtDeath)
                            {
                                //StartCoroutine(CoDestroyAttack(0f));
                                Destroy(gameObject);
                            }
                        }
                    }
                }
            }
        }

        public bool IsAnyColliderEnabled()
        {
            foreach(Collider c in myColliders)
            {
                if(c != null)
                {
                    if (c.enabled) return true;
                }
            }
            return false;
        }

        private void FixedUpdate()
        {
            if (isServer)
            {
                if (m_isAttackSetup)
                {
                    // Check with a raycast the exact point
                    RaycastHit hit;
                    if (m_body.position != lastPosition)
                    {
                        if (Physics.Raycast(lastPosition, m_body.position - lastPosition, out hit, (m_body.position - lastPosition).magnitude, collisionMask))
                        {
                            lastHitPoint = hit.point;
                            lastHitDirection = hit.normal;
                            OnTriggerEnter(hit.collider);
                        }
                    }

                    // Destroy attack that traveled over its range
                    if (!m_isMelee && Vector3.Distance(startPosition, transform.position) > m_linkedAbility.range)
                    {
                        StartCoroutine(CoDestroyAttack());
                    }

                    // Save new last clear position
                    lastPosition = m_body.position;
                    if (m_linkedAbility.attackDestroysOnGround && lastFrameVerticalSpeed < 0f && m_body.velocity.y >= 0f)
                    {
                        if (m_linkedAbility.hitWallPrefab != null)
                        {
                            SpawnParticleEffect(m_linkedAbility.hitWallPrefab.name);
                        }
                        StartCoroutine(CoDestroyAttack());
                    }
                    lastFrameVerticalSpeed = m_body.velocity.y;
                }
            }
        }

        public void SpawnParticleEffect(string particleEffectName)
        {
            GameManager.instance.NetRpcSendMessage(NetMsg.SpawnPS, particleEffectName, lastHitPoint.x, lastHitPoint.y, lastHitPoint.z, lastHitDirection.x, lastHitDirection.y, lastHitDirection.z);
        }

        public void SetupAttack(GamePlayer attacker, Ability ability, Vector3 targetPosition)
        {
            StartCoroutine(CoSetupStart(attacker, ability, targetPosition));
        }

        IEnumerator CoSetupStart(GamePlayer attacker, Ability ability, Vector3 targetPosition)
        {
            // Wait until gameobject has spawned
            yield return new WaitUntil(() => transform != null && gameObject != null);
            // Setup attack properties
            m_playerOwner = attacker;
            m_linkedAbility = ability;
            m_destroyOnFirstHit = ability.attackDestroysOnFirstHit;
            m_canHitMultipleTimes = ability.canHitMultipleTimes;
            m_effects = new List<Effect>();
            if(ability.affectedByGravity)
            {
                m_body.isKinematic = false;
                m_body.useGravity = true;
            }
            for (int i = 0; i < attacker.Abilities.Length; i++)
            {
                if (attacker.Abilities[i] == ability)
                {
                    abilityId = i;
                }
            }

            // Check if it's melee
            if (ability.isAttachedToPlayer)
            {
                m_isMelee = true;
            }

            // Apply ability effects on attack
            foreach (Effect e in ability.effects)
            {
                if (!e.isSelf || (e.isSelf && e.conditionForEffect == EffectCondition.PlayerHit))
                {                    
                    m_effects.Add(e);
                }
            }

            // Set the right attack model
            if (ability.attackModel != null)
            {
                for(int i = 0; i < attacker.Seeker.abilities.Length; i++)
                {
                    if (attacker.Seeker.abilities[i] == ability)
                    {
                        NetRpcSendMessage(NetMsg.SetItem, attacker.Seeker.id, i, attacker.LobbyPlayer.id);
                        break;
                    }
                }                    
            }

            // Delay the setup ending if there's a cast speed(no instacast)
            if(ability.castSpeed != 0f)
            {
                float timer = 0f;
                while(timer < ability.castSpeed)
                {
                    // Follow player's attack root bone
                    transform.position = attacker.AttackSpawnPoints[(int)ability.attackPrefabSpawnPoint].position;
                    yield return null;
                    timer += Time.deltaTime;
                }
            }

            // Set up attack speed
            if (ability.travelSpeed >= 0f)
            {
                Vector3 attackDirection = (targetPosition == m_playerOwner.transform.position ? transform.forward : (targetPosition - transform.position).normalized);
                m_body.velocity = ability.travelSpeed * attackDirection;
            }
            else
            {
                transform.position = targetPosition;
            }

            // Start destroy timer
            StartCoroutine(CoDestroyAttack(m_linkedAbility.lifeTime));

            // Attack is ready to act
            m_isAttackSetup = true;
            EnableColliders();
            NetRpcSendMessage(NetMsg.SetReady, attacker.LobbyPlayer.id, m_linkedAbility.abilityName,startPosition.x,startPosition.y,startPosition.z);
        }
        
        IEnumerator CoDestroyAttack(float destroyTime = 0f)
        {
            // Avoid attack to interact with
            m_isAttackSetup = false;
            yield return null;
            
            // Destroy timer
            if (destroyTime > 0f)
            {
                yield return new WaitForSeconds(destroyTime);
            }
            // If the object is already destroyed, stop this coroutine
            if (gameObject == null) yield break;
            if (m_linkedAbility.missPrefab != null)
            {
                GameManager.instance.NetRpcSendMessage(NetMsg.SpawnPS, m_linkedAbility.missPrefab.name, lastPosition.x, lastPosition.y, lastPosition.z, transform.rotation.x, transform.rotation.y, transform.rotation.z);
            }
            if (isServer)
            {
                // If the attack can cast an ability on destroy
                if (m_linkedAbility.castAbilityOnDestroy != null)
                {
                    RaycastHit hit;
                    if (Physics.Raycast(lastPosition, m_body.position - lastPosition, out hit, (m_body.position - lastPosition).magnitude, collisionMask))
                    {
                        lastPosition = hit.point;                        
                    }
                    for (int i = 0; i < m_playerOwner.Abilities.Length; i++)
                    {
                        if (m_linkedAbility.castAbilityOnDestroy.name == m_playerOwner.Abilities[i].name)
                        {
                            m_playerOwner.NetCmdSendMessage(NetMsg.CastAbility, i, lastPosition.x, lastPosition.y, lastPosition.z, 0);
                            break;
                        }
                    }
                }
            }
           
            Destroy(gameObject);
        }

        public override bool CheckMessage(string msg, NetMsgType netMsgType)
        {
            string[] msgArray = DeserializeMessage(msg);
            NetMsg msgIndex = (NetMsg)System.Convert.ToInt32(msgArray[0]);
            switch (msgIndex)
            {
                case NetMsg.SetReady:
                case NetMsg.SetItem: // riuso setitem perchè si
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
                case NetMsg.SetItem: // riuso setitem perchè si
                {
                    if(netMsgType == NetMsgType.Rpc)
                    {
                        Seeker seeker = Seeker.FindSeeker(System.Convert.ToUInt32(msgArray[1]));
                        int abilityId = System.Convert.ToInt32(msgArray[2]);
                        int ownerId = System.Convert.ToInt32(msgArray[3]);
                        GamePlayer owner = GamePlayer.Find(ownerId);
                        attackModel = Instantiate(seeker.abilities[abilityId].attackModel, transform);
                        attackModel.transform.localPosition = Vector3.zero;
                        if(seeker.abilities[abilityId].firstPersonSync && GamePlayer.local.LobbyPlayer.id == ownerId)
                        {
                            int spawnPointId = (int)seeker.abilities[abilityId].attackPrefabSpawnPoint;
                            AttackFirstPersonSync sync = attackModel.GetComponent<AttackFirstPersonSync>();
                            if(sync)
                            {
                                foreach(var c in sync.clientOnlyParticleSystems)
                                {
                                    Destroy(c);
                                }
                                foreach (var c in sync.clientOnlyRenderers)
                                {
                                    Destroy(c);
                                }
                                foreach (var c in sync.clientOnlyTrailRenderers)
                                {
                                    Destroy(c);
                                }
                                firstPersonAttackModel = Instantiate(seeker.abilities[abilityId].attackModel, attackModel.transform);
                                AttackFirstPersonSync firstSync = firstPersonAttackModel.GetComponent<AttackFirstPersonSync>();
                                foreach (var c in firstSync.serverOnlyColliders)
                                {
                                    Destroy(c);
                                }
                                Vector3 localSpawnPoint = owner.m_clientSidedModel.GetComponent<ModelTransformReference>().spawnPoints[spawnPointId].position;
                                firstPersonAttackModel.transform.position = localSpawnPoint;
                                StartCoroutine(CoSyncFirstPerson(firstPersonAttackModel));
                            }                           
                        }
                        attackModel.transform.localRotation = Quaternion.identity;
                        attackModel.transform.localScale *= seeker.abilities[abilityId].radius;
                        if (isServer)
                        {
                            myColliders = GetComponentsInChildren<Collider>();
                            if (seeker.abilities[abilityId].castSpeed != 0f)
                            {
                                DisableColliders();
                            }
                        }
                    }
                    break;
                }
                case NetMsg.SetReady: // riuso setitem perchè si
                {
                    if (netMsgType == NetMsgType.Rpc)
                    {
                        if (!isServer)
                        {
                            Debug.Log(msg);
                            m_playerOwner = GamePlayer.Find(System.Convert.ToInt32(msgArray[1]));
                            Debug.Assert(m_playerOwner != null, "Attack player owner not found");
                            m_isAttackSetup = true;
                            startPosition = new Vector3(System.Convert.ToSingle(msgArray[3]), System.Convert.ToSingle(msgArray[4]), System.Convert.ToSingle(msgArray[5]));
                        
                            foreach (Ability ab in m_playerOwner.Abilities)
                            {
                                if (ab.abilityName == msgArray[2])
                                {
                                    m_linkedAbility = ab;
                                }
                            }
                        }
                    }
                    break;
                }
            }
        }

        IEnumerator CoSyncFirstPerson(GameObject firstPersonBullet)
        {
            float timer = 0f;
            float duration = 0.5f;
            Vector3 startLocalPosition = firstPersonBullet.transform.localPosition;
            while (timer < duration)
            {
                yield return null;
                timer += Time.deltaTime;
                firstPersonBullet.transform.localPosition = Vector3.Lerp(startLocalPosition, Vector3.zero, timer / duration);
            }
            firstPersonBullet.transform.localPosition = Vector3.zero;
        }
        public void OnDisable()
        {
            StopAllCoroutines();
        }
    }
}
