using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIHealFeedback : MonoBehaviour
{
    public float m_lifeTime = 0.5f;
    public float m_vertSpeed = 100f;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(CoDestroyMe());
    }

    IEnumerator CoDestroyMe()
    {
        float timer = 0f;
        while(timer < m_lifeTime)
        {
            yield return null;
            transform.position.Set(transform.position.x, transform.position.y + m_vertSpeed * Time.deltaTime, transform.position.z);
            timer += Time.deltaTime;
        }
        Destroy(gameObject);
    }
}
