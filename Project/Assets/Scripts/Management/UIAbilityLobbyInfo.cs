using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;


namespace VoK {
    public class UIAbilityLobbyInfo : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Image iconAbility;
        public GameObject descriptionPanel;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI descriptionText;
        public Image keyIcon;
        bool followMouseState = false;
        RectTransform rectTransform;
        public Transform descriptionEnabledParent;
        Transform oldParent;

        private void Start()
        {
            followMouseState = false;
            rectTransform = descriptionPanel.GetComponent<RectTransform>();
            descriptionPanel.gameObject.SetActive(false);
            oldParent = transform.parent;
        }

        private void Update()
        {
            if(followMouseState)
            {

                Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                rectTransform.position = pos;
                rectTransform.anchoredPosition3D = new Vector3(rectTransform.anchoredPosition3D.x, rectTransform.anchoredPosition3D.y, 0f);
                //descriptionPanel.transform.position = Input.mousePosition;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            followMouseState = true;
            descriptionPanel.gameObject.SetActive(true);
            if (descriptionEnabledParent) descriptionPanel.transform.SetParent(descriptionEnabledParent);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            followMouseState = false;
            descriptionPanel.gameObject.SetActive(false);
            if (descriptionEnabledParent) descriptionPanel.transform.SetParent(oldParent);
        }

        public void SetAbilityLobbyInfo(Seeker seeker, int id)
        {
            iconAbility.sprite = seeker.abilities[id].abilitySprite;
            nameText.text = seeker.abilities[id].abilityName;
            descriptionText.text = seeker.abilities[id].abilityDescription;
            keyIcon.sprite = seeker.abilities[id].abilityCommandIcon;
        }
    }
}
