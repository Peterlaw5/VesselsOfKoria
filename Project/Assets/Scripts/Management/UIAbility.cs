using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.UI;

namespace VoK
{
    public class UIAbility : MonoBehaviour
    {
        [Header("Player-AbilityRecap")]
        //public TextMeshProUGUI nameChar;
        public Image abilityImage;
        public TextMeshProUGUI abilityName;
        public TextMeshProUGUI abilityDescription;
        public Image abilityCommandIcon;
        public TextMeshProUGUI abilityCD;



        //nameChar.text = netLobbyPlayer.seeker.name;


        public void SetAbilityRecap(Seeker seeker, int id)
        {
            abilityImage.sprite = seeker.abilities[id].abilitySprite;
            abilityName.text = seeker.abilities[id].abilityName;
            abilityDescription.text = seeker.abilities[id].abilityDescription;
            abilityCommandIcon.sprite = seeker.abilities[id].abilityCommandIcon;
            abilityCD.text = seeker.abilities[id].cooldown.ToString();

        }
    }
}
