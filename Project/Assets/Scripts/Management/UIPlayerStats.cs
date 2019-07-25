using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.UI;

namespace VoK
{
    public class UIPlayerStats : MonoBehaviour
    {
        [Header("Stats Player")]
        public TextMeshProUGUI nickname;

        public Image avatar;
        public TextMeshProUGUI kills;
        public TextMeshProUGUI deaths;
        public TextMeshProUGUI assists;

        public TextMeshProUGUI streak;
        public TextMeshProUGUI maxStreak;
        public TextMeshProUGUI energyDelivered;

        public TextMeshProUGUI generatorConquered;
        public TextMeshProUGUI generatorNeutralized;
        public TextMeshProUGUI generatorTime;

        public TextMeshProUGUI damageDealt;
        public TextMeshProUGUI healingDealt;
        public TextMeshProUGUI damageReceived;
        public TextMeshProUGUI healingReceived;

        public TextMeshProUGUI headshots;
        public TextMeshProUGUI totalHeadshots;
        public TextMeshProUGUI killsDeathsRatio;
        public TextMeshProUGUI eliminations;

        NetLobbyPlayer myLobbyPlayer;
        /*
        public void SetLobbyPlayerStats(NetLobbyPlayer netLobbyPlayer)
        {
            nickname.text = netLobbyPlayer.playerName;
            avatar.sprite = netLobbyPlayer.seeker.seekerSprite;

            //NetlobbyPlayer GetStats()
            float[] playerstats = netLobbyPlayer.GetStats();

            for (int i = 0; i < stats.Length; i++)
            {
                if (stats[i] != null)
                {
                    if (playerstats[i] - Mathf.RoundToInt(playerstats[i]) < 0.001f)
                    {
                        stats[i].text = Mathf.RoundToInt(playerstats[i]).ToString();
                    }
                    else
                    {
                        stats[i].text = playerstats[i].ToString("F2");
                    }
                }
            }
        }*/
        public void SetLobbyPlayer(NetLobbyPlayer netLobbyPlayer,bool isMatchEnded=false)
        {
            myLobbyPlayer = netLobbyPlayer;
            nickname.text = myLobbyPlayer.playerName;
            avatar.sprite = myLobbyPlayer.seeker.seekerSprite;
        }

        private void Update()
        {
            if (myLobbyPlayer != null && myLobbyPlayer.stats != null)
            {
                if (kills != null) kills.text = myLobbyPlayer.Kills.ToString();
                if (deaths != null) deaths.text = myLobbyPlayer.Deaths.ToString();
                if (assists != null) assists.text = myLobbyPlayer.Assists.ToString();

                if (energyDelivered != null) energyDelivered.text = myLobbyPlayer.EnergyDelivered.ToString("F0");
                if (streak != null) streak.text = myLobbyPlayer.Streak.ToString();
                if (maxStreak != null) maxStreak.text = myLobbyPlayer.MaxStreak.ToString();

                if (generatorConquered != null) generatorConquered.text = myLobbyPlayer.GeneratorConquered.ToString();
                if (generatorNeutralized != null) generatorNeutralized.text = myLobbyPlayer.GeneratorNeutralized.ToString();
                if (generatorTime != null) generatorTime.text = myLobbyPlayer.GeneratorTime.ToString("F0");

                if (headshots != null) headshots.text = myLobbyPlayer.Headshots.ToString();
                if (totalHeadshots != null) totalHeadshots.text = myLobbyPlayer.TotalHeadshots.ToString();
                if (damageDealt != null) damageDealt.text = myLobbyPlayer.DamageDealt.ToString("F0");

                if (damageReceived != null) damageReceived.text = myLobbyPlayer.DamageReceived.ToString("F0");
                if (healingDealt != null) healingDealt.text = myLobbyPlayer.HealingDealt.ToString("F0");
                if (healingReceived != null) healingReceived.text = myLobbyPlayer.HealingReceived.ToString("F0");

                if (killsDeathsRatio != null) killsDeathsRatio.text = myLobbyPlayer.KDRatio.ToString("F2");
                if (eliminations != null) eliminations.text = myLobbyPlayer.Eliminations.ToString();
            }
        }
    }
}