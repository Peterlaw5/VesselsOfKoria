using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace VoK
{
    public class TeamBasedEffectsManager : MonoBehaviour
    {
        // Start is called before the first frame update
        public Team myTeam;
        public GameObject allyModel;
        public GameObject enemyModel;
        void Start()
        {
            StartCoroutine(CoWaitPlayerLoaded());
        }

        // Update is called once per frame
        void Update()
        {

        }
        IEnumerator CoWaitPlayerLoaded()
        {
            yield return new WaitUntil(() => GamePlayer.local != null && GamePlayer.local.LobbyPlayer != null);
            //Debug.Log(myTeam);
            //Debug.Log(GamePlayer.local.Team);
            if (myTeam == GamePlayer.local.Team)
            {
                allyModel.SetActive(true);
                enemyModel.SetActive(false);
            }
            else
            {
                allyModel.SetActive(false);
                enemyModel.SetActive(true);
            }
        }

    }
}
