using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoK;

public class AttackModelManager : MonoBehaviour
{
    public GameObject allyModel;
    public GameObject enemyModel;
    public AttackBehaviour attack;

    void Start()
    {
        attack = GetComponentInParent<AttackBehaviour>();
        StartCoroutine(CoWaitAttackSetup());        
    }

    IEnumerator CoWaitAttackSetup()
    {
        yield return new WaitUntil(() => attack.m_isAttackSetup == true);
        bool sameTeam = attack.m_playerOwner.Team == GamePlayer.local.Team;
        allyModel.SetActive(sameTeam);
        enemyModel.SetActive(!sameTeam);
       
    }
}
