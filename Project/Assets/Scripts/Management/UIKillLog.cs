using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;

namespace VoK
{
    public class UIKillLog : MonoBehaviour
    {
        public float disappearTime = 5f;
        public float fadeTime = 1f;

        public TextMeshProUGUI killerText;
        public TextMeshProUGUI deadText;

        public Image killerIcon;
        public Image deadIcon;

        public Image killIcon;

        public bool useMySelfColor = true;
        public Color mySelfColor;

        public void SetKillLog(GamePlayer killer, GamePlayer dead)
        {
            string killerString;
            string deadString;
            string allyColor = ColorUtility.ToHtmlStringRGB(GameManager.instance.allyColor);
            string enemyColor = ColorUtility.ToHtmlStringRGB(GameManager.instance.enemyColor);

            if(useMySelfColor)
            {
                if (killer == GamePlayer.local || dead == GamePlayer.local)
                {
                    allyColor = ColorUtility.ToHtmlStringRGB(mySelfColor);
                }
            }
            if (killer == null) killer = GamePlayer.local;
            if (GamePlayer.local.Team == killer.Team)
            {
                killerString = "<#" + allyColor + ">" + killer.LobbyPlayer.playerName + "</color>";
                deadString = "<#" + enemyColor + ">" + dead.LobbyPlayer.playerName + "</color>";
            }
            else
            {
                killerString = "<#" + enemyColor + ">" + killer.LobbyPlayer.playerName + "</color>";
                deadString = "<#" + allyColor + ">" + dead.LobbyPlayer.playerName + "</color>";
            }
            killerText.text = killerString;
            deadText.text = deadString;
            killerIcon.sprite = killer.Seeker.seekerSprite;
            deadIcon.sprite = dead.Seeker.seekerSprite;
            StartCoroutine(CoDisappear());
        }

        IEnumerator CoDisappear()
        {
            float timer = 0f;
            while(timer < disappearTime - fadeTime)
            {
                yield return null;
                timer += Time.deltaTime;
            }
            timer = 0f;
            killerText.CrossFadeAlpha(0f, fadeTime, true);
            deadText.CrossFadeAlpha(0f, fadeTime, true);
            killerIcon.CrossFadeAlpha(0f, fadeTime, true);
            deadIcon.CrossFadeAlpha(0f, fadeTime, true);
            killIcon.CrossFadeAlpha(0f, fadeTime, true);
            while (timer < fadeTime)
            {
                yield return null;
                timer += Time.deltaTime; 
            }
            Destroy(gameObject);
        }

    }
}
