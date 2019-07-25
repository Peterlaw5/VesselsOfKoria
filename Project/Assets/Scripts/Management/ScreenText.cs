using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace VoK
{
    //CIAODAN ENUM
    public enum ScreenMsgType
    {
        GamePhases,
        EnergyLog,
        GameOver,
        KillLog,
        EventKillLog,
        AffectedStatus

    }


    [CreateAssetMenu(fileName = "NewScreenText", menuName = "VoK/Screen Text")]
    public class ScreenText : ScriptableObject
    {
        [TextArea(0, 20)]
        [Tooltip("Text content")]
        public string content;
        [Tooltip("Time duration")]
        public float duration;
        public float parameter = -1f;
        public ScreenMsgType type;
        public AnimationCurve animationCurve;
        public AnimationCurve transparentCurve;

        public ScreenText(string text, float time, float par)
        {
            content = text;
            duration = time;
            parameter = par;
        }
        public override string ToString()
        {
            return name; //NetBehaviour.SerializeMessage(content,duration);
        }

    }
}
