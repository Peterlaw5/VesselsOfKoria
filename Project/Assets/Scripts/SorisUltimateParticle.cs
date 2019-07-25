using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace VoK
{
    public class SorisUltimateParticle : MonoBehaviour
    {
        // Start is called before the first frame update
        AttackBehaviour attack;
        public GameObject allyPrefab;
        public GameObject enemyPrefab;
        Team myTeam;
        void Start()
        {
            attack = GetComponentInParent<AttackBehaviour>();
            StartCoroutine(CowaitAttackSetup());
        }

        private void OnDestroy()
        {
            GameObject g;
            if( myTeam!=Team.None)
            {
                if (myTeam == GamePlayer.local.Team)
                {
                   g=Instantiate(allyPrefab,attack.lastPosition, Quaternion.identity);
                }
                else
                {
                   g=Instantiate(enemyPrefab, transform.position, Quaternion.identity);
                }
                if(attack.m_linkedAbility.castAbilityOnDestroy)
                {
                   g.transform.localScale *= attack.m_linkedAbility.castAbilityOnDestroy.radius;
                }
               
            }
               
        }
        IEnumerator CowaitAttackSetup()
        {
            yield return new WaitUntil(() => attack.m_isAttackSetup == true);
            myTeam = attack.m_playerOwner.Team;
        }
    }
}

