using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace VoK
{
    public class OgerthaPull : MonoBehaviour
    {
        public GameObject chainPrefab;
        AttackBehaviour attack;
        ChainCreator chain;
        // Start is called before the first frame update
        void Start()
        {
            //  Instantiate(chainPrefab, transform.position, transform.rotation);
            attack = GetComponentInParent<AttackBehaviour>();
            StartCoroutine(CoWaitAttackSetup());
        }
        
        IEnumerator CoWaitAttackSetup()
        {
            yield return new WaitUntil(() => attack.m_isAttackSetup == true);
            var g = Instantiate(chainPrefab, attack.startPosition, Quaternion.identity);
            g.GetComponentInParent<Empty>().transform.localScale *= attack.m_linkedAbility.radius;
            chain = g.GetComponentInChildren<ChainCreator>();
            chain.target =attack.gameObject;
            Debug.Assert(attack.m_playerOwner!=null,"PLAYER OWNER NON ASSEGNATO");
            chain.father.transform.parent = attack.m_playerOwner.gameObject.transform;
            chain.StarCofollow();
            
        }
        private void OnDestroy()
        {
            chain.target = null;
        }
    }
}

