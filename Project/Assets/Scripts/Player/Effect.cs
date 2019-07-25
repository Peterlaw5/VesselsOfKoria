using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace VoK
{
    [CreateAssetMenu(fileName = "NewEffect", menuName = "VoK/Effect")]
    public class Effect : ScriptableObject
    {
        public GameObject particleEffect;
        public EffectCondition conditionForEffect;
        [Header("Effect targets")]
        [Tooltip("If true, this effect will apply on the caster")]
        public bool isSelf;
        [Tooltip("If true, this effect will apply on enemies")]
        public bool canHitEnemy = false;
        [Tooltip("If true, this effect will apply on allies")]
        public bool canHitAllies = false;
        [Header("Player state effect")]
        [Tooltip("Health variation applied by this effect(damage > 0, heal < 0)")]
        public float damage;
        [Tooltip("Headshot Multiplier")]
        public float headshotMult = 1f;
        //[Tooltip("Health variation time duration")]
        //public float damageTime;
        //[Tooltip("Health variation time interval between two health alteration events")]
        //public float damageFrequency;
        [Tooltip("Time of applied stun")]
        public float stunTime;
        [Tooltip("Time of applied snare")]
        public float snareTime = 0f;
        [Tooltip("Slow/snare/speed effect multiplier - set 0 to snare, 0-1 to slow, 1 will have no effect, 1+ to speed up")]
        public float speedMult = 1;
        [Tooltip("Slow/snare/speed effect time duration")]
        public float speedTime;
        //snaredAbility;
        [Header("Camera effects")]
        [Tooltip("If true ability is force to the looking direction of the camera")]
        public bool forceOnCameraForward;
        public bool followCameraDirection;
        [Header("Forces")]
        public Vector3 appliedForceDirection;
        public bool applyForceTowardAttacker;
        public float appliedForceTime;
        public float appliedForceIntensity;
        public bool deactivateWithInput;
        [Tooltip("Minimum Camera elevation")]
        [Range(-90f, 90f)]
        public float minCameraElevation = -90f;
        public bool keepForwardMomentum = false;
        public float deactivateGravityTime = 0f;
        public float deactivateMouseTime = 0f;
        public bool deactivateCameraRotation = false;
        public float noClipTime = 0f;

        //public float shieldAmount = 0f;
        //public float shieldTime = 0f;
        [Range(0f,1f)]
        public float cooldownReduction = 0f;
        public AbilityType[] abilitiesToApplyCDR;
        [Range(0f, 1f)]
        [Header("Damage Reduction")]
        public float damageReduction = 1f;
        [Range(0f, 1f)]
        public float healthLevelReduction = 1f;
        public float damageReductionTime = 0f;
        public float invulnerabilityTime = 0f;
        //cdrAbilities
        [Header("Redirection System")]
        public AbilityType redirectedAbility = AbilityType.None;
        public AbilityType substituteAbility = AbilityType.None;
        public float redirectionTime = -1f;

        [Header("Teleport System")]
        public bool saveHitPlayer = false;
        public float keepHitPlayerTime = 0f;
        public bool teleportToSavedPlayer = false;

        [Header("Feedbacks")]
        public bool giveFeedbackOnScreen = true;
        public float hitParameterValue = -1f;
        public float headShotParameterValue = -1f;
    }
}
