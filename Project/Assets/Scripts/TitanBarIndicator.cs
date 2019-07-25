using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

namespace VoK
{
    public class TitanBarIndicator : MonoBehaviour
    {
        public Image fillCheck;
        public Image myImage;
        [Range(0f,1f)]
        public float fillAmountBorder;
        private void Start()
        {
            myImage = GetComponent<Image>();
        }
        private void Update()
        {
            if(fillCheck != null && myImage != null)
            {
                if(fillCheck.fillAmount >= fillAmountBorder && fillCheck.fillAmount <= 1f - fillAmountBorder)
                {
                    if (!myImage.enabled) myImage.enabled = true;
                }
                else
                {
                    if (myImage.enabled) myImage.enabled = false;
                }
            }
        }
    }
}
