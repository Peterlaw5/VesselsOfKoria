using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoK;
public class AttackCustomTrail : MonoBehaviour
{
    // Start is called before the first frame update
    Transform parent;
    LineRenderer ray;
    AttackBehaviour attack;
    void Start()
    {
        attack = GetComponentInParent<AttackBehaviour>();
        ray = GetComponent<LineRenderer>();
        StartCoroutine(startRay());
    }


    IEnumerator startRay()
    {
        yield return new WaitUntil(() => attack.m_isAttackSetup == true);
        float maxlenght = Vector3.Distance(transform.position, attack.m_playerOwner.m_attackSpawnPoints[(int)attack.m_linkedAbility.attackPrefabSpawnPoint].position);
        transform.LookAt(attack.m_playerOwner.m_attackSpawnPoints[(int)attack.m_linkedAbility.attackPrefabSpawnPoint].position);
        //float maxlenght = Vector3.Distance(transform.position, attack.m_linkedAbility.s.startPosition);
        //ray.SetPosition(1, new Vector3(0, 0, maxlenght));
        float lifeTime = attack.m_linkedAbility.lifeTime;

        //float maxlenght = Vector3.Distance(transform.position, attack.startPosition);
        ray.SetPosition(0, new Vector3(0, 0, maxlenght));
        ray.SetPosition(1, new Vector3(0, 0, maxlenght));

        float lenght = maxlenght;
        while (lenght > 0.1f)
        {
            lenght -= maxlenght / (lifeTime * 0.5f) * Time.deltaTime;
            lenght = Mathf.Max(0f, lenght);
            ray.SetPosition(0, new Vector3(0, 0, lenght));
            yield return null;
        }
        ray.SetPosition(0, new Vector3(0, 0, 0));
        StartCoroutine(endRay(maxlenght, lifeTime));
    }

    IEnumerator endRay(float maxlenght, float lifeTime)
    {
        float lenght = maxlenght;
        
        while (lenght > 0.1f)
        {
            lenght -= maxlenght / (lifeTime * 0.5f) * Time.deltaTime;
            lenght = Mathf.Max(0f, lenght);
            ray.SetPosition(1, new Vector3(0, 0, lenght));
            yield return null;
        }
        ray.SetPosition(1, new Vector3(0, 0, 0));
    }
}
