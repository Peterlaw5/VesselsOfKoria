using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using FMODUnity;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace VoK
{
    [CreateAssetMenu(fileName = "NewSeeker", menuName = "VoK/Seeker")]
    public class Seeker : ScriptableObject
    {
        public static Seeker[] allSeekers;

        [Tooltip("Unique identifier for seekers, do not duplicate")]
        public SeekerList seekerId;
        [HideInInspector]
        public uint id { get { return (uint)seekerId; } set { seekerId = (SeekerList)value; } }
        [Header("Seeker's Stats")]
        [Tooltip("Seeker's name")]
        public string seekerName;
        [Tooltip("Seeker's title")]
        public string heroTitle = "";
        [Tooltip("Seeker's max health")]
        public float maxHealth;
        [Tooltip("Seeker's base movement speed")]
        public float baseSpeed;
        [Tooltip("Seeker's base movement jump force intensity")]
        public float jumpForce;
        [Tooltip("Max carried amount of energy")]
        public float maxCarriedEnergy;

        

        public Ability[] abilities;

        [Header("Seeker's visual informations")]
        public Sprite seekerSprite;
        public Sprite seekerSpriteBody;
        [Header("Seeker's visual informations - deprecated")]
        public Sprite baseAbilitySprite;
        public Sprite movementAbilitySprite;
        public Sprite firstAbilitySprite;
        public Sprite secondAbilitySprite;
        public Sprite ultimateAbilitySprite;

        [Header("Seeker's FMOD Sounds")]
        [EventRef]
        public string walk;
        [EventRef]
        public string jumpSound;
        public string jumpSoundParameter;
        public float takeoffParameterValue = 0f;
        public float landParameterValue = 1f;

        [EventRef]
        public string breathanddeath;
        [EventRef]
        public string reload;

        [Header("Model prefab for seeker's body")]
        public GameObject bodyPrefab;
        
        [Tooltip("Seeker's desc")]
#if UNITY_EDITOR
        [TextArea(1,500)]
#endif
        public string seekerDesc;


        // Load every seeker info
        public static void LoadSeekers()
        {
            allSeekers = MenuManager.Instance.gameSeekers;
        }

        // Find seeker starting from id
        public static Seeker FindSeeker(uint id)
        {
            LoadSeekers();
            foreach(Seeker s in allSeekers)
            {
                if (s.id == id)
                    return s;
            }
            return null;
        }
        

    }
}
