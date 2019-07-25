using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace VoK
{
    public class SeekerGallery : MonoBehaviour
    {
        //public SeekerList seekerList;

        //base
        [Header("Base-Info")]
        public Seeker seeker;
        //public Ability ability;
        public Image avatar;
        public TextMeshProUGUI textNameSeeker;

        //InfoDesc
        [Header("Info-Desc")]
        public TextMeshProUGUI textInfoDescription;
        public TextMeshProUGUI textHeroTitle;

        public UIAbilityLobbyInfo[] abilitiesImage;
        public TextMeshProUGUI[] abilitiesText;
        public GameObject abilityLobbyInfoPrefab;
        public GameObject abilityInfoPanel;

        //AbilitiesDesc
        //[Header("Abilities-Desc")]
        //public TextMeshProUGUI textRoleDesc;
        //public TextMeshProUGUI textCatchphraseDesc;



        public void SetGallerySeekerInfo()
        {
            //base + infoDescr
            avatar.sprite = seeker.seekerSpriteBody;
            textNameSeeker.text = seeker.seekerName;
            textInfoDescription.text = seeker.seekerDesc;
            textHeroTitle.text = seeker.heroTitle;

        }

        public void SetGallerySeekerAbilities()
        {
            for (int i = 0; i < 6; i++)
            {
                //abilitiesImage[i].sprite = seeker.abilities[i].abilitySprite;
                //abilitiesText[i].text = seeker.abilities[i].abilityName;                
                //UIAbilityLobbyInfo lobbyInfoAbility = Instantiate(abilityLobbyInfoPrefab, abilitiesImage[i].gameObject.transform).GetComponent<UIAbilityLobbyInfo>();
                abilitiesImage[i].SetAbilityLobbyInfo(seeker, i);
            }
           
        }



    }
}

