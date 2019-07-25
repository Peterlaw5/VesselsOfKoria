using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace VoK
{
    public class OgerthaUltimate : MonoBehaviour
    {
        public Team myTeam;
        public AttackBehaviour attack;
        public GameObject chainCentre;
        public List<ChainCreatorUltimate> chains;
        public  List<GamePlayer> enemyInArea;
      
        // Start is called before the first frame update
        void Start()
        {
            foreach (ChainCreatorUltimate c in chainCentre.GetComponentsInChildren<ChainCreatorUltimate>())
            {
                chains.Add(c);
            }
            attack = GetComponentInParent<AttackBehaviour>();
            StartCoroutine(CoWaitAttackSetup());
            enemyInArea = new List<GamePlayer>();
            myTeam = Team.None;
        }
     
        // Update is called once per frame
        void Update()
        {

        }

        public void BondChains()
        {
            for(int i = 0;i < enemyInArea.Count; i++)
            {
                if(chains[i].bonded==false)
                {
                    chains[i].lsatNodeTarget = enemyInArea[i].gameObject;
                    chains[i].SartFollowTarget();
                    Debug.Log("bonded chain n°" + i);
                }
            }
        }
        public void RemovePlayer(GamePlayer player)
        {
            if (enemyInArea.Contains(player))
            {
                DeBondChains(player);
                enemyInArea.Remove(player);
                if (attack.isServer)
                {
                    player.OnPlayerDeath -= RemovePlayer;
                }
            }
        }
        public void DeBondChains(GamePlayer c)
        {
            for (int i = 0; i < enemyInArea.Count; i++)
            {
                if (enemyInArea[i]==c && chains[i].bonded == true)
                {
                    chains[i].lsatNodeTarget = null;
                    
                }
            }
        }
        private void OnTriggerEnter(Collider other)
        {
            StartCoroutine(DelayTriggerEnter(other.gameObject.GetComponent<GamePlayer>()));
        }
        private void OnTriggerExit(Collider other)
        {
            GamePlayer player = other.gameObject.GetComponent<GamePlayer>();
            if (player != null)
            {
                if (player.Team != myTeam)
                {
                    RemovePlayer(player);
                }
            }
        }
        IEnumerator CoWaitAttackSetup()
        {
            yield return new WaitUntil(() => attack.m_isAttackSetup == true);
            attack = GetComponentInParent<AttackBehaviour>();
            myTeam = attack.m_playerOwner.Team;
        }

        IEnumerator DelayTriggerEnter(GamePlayer player)
        {
            yield return new WaitUntil(() => attack.m_isAttackSetup == true);
            if (player != null)
            {
                if (!player.IsDead && player.Team != myTeam)
                {
                    enemyInArea.Add(player);
                    BondChains();
                    if (attack.isServer)
                    {
                        player.OnPlayerDeath += RemovePlayer;
                    }
                }
            }
        }
    }
}

