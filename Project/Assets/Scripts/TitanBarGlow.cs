using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

namespace VoK
{
    public class TitanBarGlow : MonoBehaviour
    {
        public Image fillToCheck;
        public Image imageTarget;
        public float oldFillValue;
        public float glowTime = 0.25f;
        public Coroutine glowCo;
        public float maxAlpha = 1f;
        Color endColor;
        Color transparentColor;
        public void Start()
        {
            glowCo = null;
            oldFillValue = -1f;
            transparentColor = new Color(imageTarget.color.r, imageTarget.color.g, imageTarget.color.b, 0f);
            endColor = new Color(imageTarget.color.r, imageTarget.color.g, imageTarget.color.b, 1f);
            imageTarget.color = transparentColor;
        }

        IEnumerator CoGlowEffect()
        {
            float timer = 0f;
            imageTarget.color = transparentColor;
            while (timer < glowTime)
            {
                yield return null;
                imageTarget.color = Color.Lerp(transparentColor, endColor, timer / glowTime);
                timer += Time.deltaTime;
            }
            imageTarget.color = endColor;
            //yield return new WaitForSeconds(glowTime);
            while (timer < glowTime)
            {
                yield return null;
                imageTarget.color = Color.Lerp(endColor, Color.clear, timer / glowTime);
                timer += Time.deltaTime;
            }
            imageTarget.color = Color.clear;
            glowCo = null;
        }

        // Update is called once per frame
        void Update()
        {
            if (fillToCheck.fillAmount < 1f)
            {
                if (oldFillValue < fillToCheck.fillAmount)
                {
                    if (glowCo == null) glowCo = StartCoroutine(CoGlowEffect());
                    oldFillValue = fillToCheck.fillAmount;
                }
                else if(oldFillValue > fillToCheck.fillAmount)
                {
                    oldFillValue = -1f;
                    imageTarget.color = endColor;
                }
            }
            else
            {
                oldFillValue = -1f;
                imageTarget.color = endColor;
            }
        }

        void StartGlow()
        {
            imageTarget.color = new Color(imageTarget.color.r, imageTarget.color.g, imageTarget.color.b, 0f);
            if (glowCo != null) StopCoroutine(glowCo);
            glowCo = StartCoroutine(CoGlowEffect());
            oldFillValue = fillToCheck.fillAmount;
        }

    }
}
