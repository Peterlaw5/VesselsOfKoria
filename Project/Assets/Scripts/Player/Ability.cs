using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using FMODUnity;

namespace VoK
{
    [CreateAssetMenu(fileName = "NewAbility", menuName = "VoK/Ability")]
    public class Ability : ScriptableObject
    {
        [Header("Ability informations")]
        public string abilityName;
        public string abilityDescription;
        public Sprite abilitySprite;
        public Sprite abilityCommandIcon;

        [Header("Spawned attack stats")]
        [Tooltip("Duration time of the ability, NOT the effects. For abilities with bullets/attack prefabs it determines the time of autodestruction since its spawn.")]
        public float lifeTime;
        [Tooltip("If true the attack will despawn at owner's death")]
        public bool destroyAtDeath = false;
        [Tooltip("Max distance traveled by the bullet/attack before its destruction")]
        public float range;
        [Tooltip("Radius of AoE effect - it increases the attack prefab model collider")]
        public float radius = 1f;
        [Tooltip("Delay between successful attack input request(key pressing) and bullet/attack spawn")]
        public float castSpeed;
        [Tooltip("Attack/bullet travel speed after spawning")]
        public float travelSpeed;
        [Tooltip("Time between two different cast event of this ability(minimum)")]
        public float cooldown;
        [Tooltip("Time interval needed to fully recharge ammo to full ammo")]
        public float reloadCooldown;
        [Tooltip("Maximum times of use of an ability before constrained reload cooldown")]
        public int maxAmmo;
        public bool consumeBaseAbilityAmmo = false;

        [Header("Spawned attack interaction")]
        [Tooltip("If true, the non-self effects of this attack can hit me")]
        public bool canHitMe = false;
        [Tooltip("If true, the attack/bullet spawned by this ability will be attached to the caster - requires travel speed 0 to work")]
        public bool isAttachedToPlayer;
        [Tooltip("If true, the attack/bullet spawned by this ability will follow the rotation of the caster")]
        public bool attackFollowBoneRotation=true;
        [Tooltip("If true, the attack/bullet is affected by gravity")]
        public bool affectedByGravity = false;
        [Tooltip("If checked, the attack will be destroyed when it hits any wall")]
        public bool attackDestroysOnWall;
        [Tooltip("If checked, the attack will be destroyed when it hits the ground")]
        public bool attackDestroysOnGround = false;
        [Tooltip("If checked, the cast direction will follow camera forward direction")]
        public bool castDirectionCamera;
        [Tooltip("If checked, you can use this ability only when you are on the ground")]
        public bool onlyCastOnGround;

        [Tooltip("If higher than 1 it will start spawning attacks after the first")]
        public int spawnNumber = 1;
        [Tooltip("Time interval between two spawns when spawn number is not 1")]
        public float multiSpawnInterval = 0f;

        [Tooltip("Ability multi spawn will stop if this is marked and the player is stunned")]
        public bool stopIfStunned = false;
        [Tooltip("Ability multi spawn will stop if this is marked and the player dies")]
        public bool stopIfDead = false;
        [Tooltip("Ability multi spawn will stop if this is marked and the player can't attack")]
        public bool stopIfCannotAttack = false;

        [Header("Linked data")]
        [Tooltip("List of effects applied by this ability. If any is Self, it will be applied to the caster, otherwise it will be given to the attack/bullet")]
        public Effect[] effects;
        [Tooltip("Attack/bullet base spawn point")]
        public AttackSpawnPoint attackPrefabSpawnPoint;
        [Tooltip("If true, the attack will spawn in the same point but on local model")]
        public bool firstPersonSync = false;
        [Tooltip("If attacks penetrates players set this true")]
        public bool attackDestroysOnFirstHit = true;
        [Tooltip("If an attack can hit multiple times, set this true")]
        public bool canHitMultipleTimes = false;
        [Tooltip("Attack/bullet base prefab(logic)")]
        public GameObject attackPrefab;
        [Tooltip("Attack/bullet model prefab(aspect)")]
        public GameObject attackModel;
        [Tooltip("The ability that the attack will cast when the bullet it's destroyed")]
        public Ability castAbilityOnDestroy;

        [Header("Particle Effects")]
        public GameObject hitWallPrefab;
        public GameObject missPrefab;
        public GameObject uiEffect;
        [Header("Zoom Effect")]
        public bool applyZoom = false;
        public float zoomMult = 1f;
        public float zoomSpeed = 1f;
        public GameObject zoomMask;

        [Header("crosshair effects")]
        public bool animateCrosshair;
        [Header("xMovement and yMovement MUST HAVE THE SAME MAX TIME VALUE")]
        public AnimationCurve xMovment;
        public AnimationCurve yMovment;
        public AbilityType RedirectedAbility()
        {
            foreach (Effect e in effects)
            {
                if (e.isSelf && e.redirectedAbility != AbilityType.None)
                    return e.redirectedAbility;
            }
            return AbilityType.None;
        }

        [Header("Sound")]
        [EventRef]
        public string abilitySound;
        public string soundParameter = "";
        public float soundParameterValue = 0f;
        public TargetType soundTarget = TargetType.EveryOne;
    }
}
