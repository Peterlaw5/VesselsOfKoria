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
    [CreateAssetMenu(fileName = "New TextManager", menuName = "VoK/TextManager")]
    public class TextManager : ScriptableObject    {

        [Header("START MESSAGE")]
        public ScreenText startMsg;

        [Header("MULTI - KILL MESSAGE")]
        public ScreenText doubleKillMsg;
        public ScreenText tripleKillMsg;

        [Header("GENERATOR TAKEN")]
        public ScreenText allyGenTakenMsg;
        public ScreenText enemyGenTakenMsg;
        public ScreenText neutralizeGenMsg;

        [Header("BASE CONQUEST START")]
        public ScreenText allyFinalAttackMsg;
        public ScreenText enemyFinalAttackMsg;

        [Header("WIN MESSAGE")]
        public ScreenText allyScreenWinMsg;
        [TextArea(0, 30)]
        public string allyEndGameTitleWinMsg;
        public ScreenText enemyScreenWinMsg;
        [TextArea(0, 30)]
        public string enemyEndGameTitleWinMsg;

        [Header("EFFECTS MESSAGE")]
        public ScreenText stunMsg;
        public ScreenText snaredMsg;
        public ScreenText slowMsg;

        [Header("BEST PLAYER MESSAGE")]
        [TextArea(0, 20)]
        public string bestKillerMsg = "Best Killer";
        [Tooltip("{0} = Best Killer Name, {1} = number of kills ")]
        public string bestKillerStringMsg = "{0} \n {1} Kills";

        [TextArea(0, 20)]
        public string bestDamagerMsg = "Best Damager";
        [Tooltip("{0} = Best Damager Player Name, {1} = number of damage dealt ")]
        public string bestDamagerStringMsg = "{0} \n {1} damage";

        [TextArea(0, 20)]
        public string bestCarrierMsg = "Best Carrier";
        [Tooltip("{0} = Best Carrier Name, {1} = number of energy/kor delivered ")]
        public string bestCarrierStringMsg = "{0} \n {1} Kor";

        [Header("ITEM SPAWN MESSAGE")]
        [Tooltip("{0} = number of seconds")]
        public ScreenText energyGoingToSpawnMsg;
        public ScreenText energyHasSpawnedMsg;

        [Header("RESPAWNING MESSAGE")]
        [Tooltip("{0} = number of seconds")]
        [TextArea(0, 20)]
        public string respawningInMessage = "Respawning in\n {0}";

        [Header("ROUND END")]
        public string roundWon = "Round won";
        public string roundLost = "Round lost";
        [Tooltip("{0} = number of seconds")]
        [TextArea(0, 20)]
        public string restartingRound = "Next round will start in {0} seconds";

        [Header("PLAYER DEATH MESSAGE")]
        [TextArea(0, 20)]
        [Tooltip("You're dead...But no one killed you...Shit Happens.")]
        public string nobodyKillYouMsg = "Nobody";
        [TextArea(0, 20)]
        [Tooltip("{0} = Killer Name, {1} = Dead Player Name")]
        public string killerStringMsg = "{0} killed {1}";
        [Tooltip("{0} = Killer Name")]
        public ScreenText killedByMsg;
        [Tooltip("{0} = Dead Player Name")]
        public ScreenText youKilledMsg;

        [Header("FINAL FASE MESSAGE")]
        public ScreenText finalPhaseMsg;
        public ScreenText ultimateStartMsg;

        [Header("Lobby")]
        [Tooltip("{0} = Random number")]
        public string emptyNameAutoFill = "SEEKER{0}";

        public ScreenText[] screenTextList;
        public ScreenText FindText(string name)
        {
            foreach(ScreenText t in screenTextList)
            {
                if (name == t.name) return t;
            }
            return null;
        }
    }
}
