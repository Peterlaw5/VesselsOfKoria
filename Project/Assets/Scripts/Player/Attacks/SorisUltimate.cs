using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace VoK
{
    public class SorisUltimate : MonoBehaviour
    {
        public Team myTeam;
        public GameObject lightningPrefab;
        public GameObject lightningSpawnPoint;
        public Transform spawnPoint;
        public AttackBehaviour attack;

        void Start()
        {
            attack = GetComponentInParent<AttackBehaviour>();
            StartCoroutine(CoWaitAttackSetup());
        }

        IEnumerator CoWaitAttackSetup()
        {
            yield return new WaitUntil(() => attack.m_isAttackSetup == true);
            attack = GetComponentInParent<AttackBehaviour>();
            myTeam = attack.m_playerOwner.Team;
        }

        private void OnTriggerEnter(Collider other)
        {
            StartCoroutine(DelayTriggerEnter(other.gameObject.GetComponent<GamePlayer>()));
        }

        IEnumerator DelayTriggerEnter(GamePlayer player)
        {
            yield return new WaitUntil(() => attack.m_isAttackSetup == true);
            if (player != null)
            {
                if (!player.IsDead && player.Team != myTeam)
                {
                    LightningBoltScript r = Instantiate(lightningPrefab, spawnPoint.position, transform.rotation, transform).GetComponent<LightningBoltScript>();
                    r.StartObject = lightningSpawnPoint;
                    r.EndObject = player.gameObject;
                }
            }
        }
    }

}
