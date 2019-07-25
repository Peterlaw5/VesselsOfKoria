using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace VoK
{
    public class UIBuildDebug : MonoBehaviour
    {
        public Image panel;
        GamePlayer player;
        public TextMeshProUGUI text;
        // Start is called before the first frame update
        void Start()
        {
            StartCoroutine(CoWaitLocalPlayer());
            panel.enabled = false;
            text.enabled = false;
        }

        // Update is called once per frame
        void Update()
        {
            if (player != null)
            {/*
                if (Input.GetKeyDown(KeyCode.F11))
                {
                    if (panel.enabled == false)
                    {
                        panel.enabled = true;
                        text.enabled = true;
                    }
                    else
                    {
                        panel.enabled = false;
                        text.enabled = false;
                    }
                }*/

                if (panel.enabled == true)
                {
                    text.text = "IsAffectedByGravity: " + player.IsAffectedByGravity
                              + "\nIsAffectedByForces: " + player.IsAffectedByForces
                              + "\nCanInput: " + player.m_canInput
                              + "\nisInvulnerable: " + player.m_isInvulnerable
                              + "\nCanGetEnergy: " + player.m_canGetEnergy
                              + "\nCanRotateVisual: " + player.CanRotateVisual
                              + "\nClientSidedModel.transform.localPosition: " + player.m_clientSidedModel.transform.localPosition;
                    ;

                }
            }
        }
        IEnumerator CoWaitLocalPlayer()
        {
            yield return new WaitUntil(() => GamePlayer.local != null && GamePlayer.local.LobbyPlayer != null);
            player = GamePlayer.local;
        }
    }
}
