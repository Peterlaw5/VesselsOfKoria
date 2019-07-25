using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoK
{
    public class TitanCompass : MonoBehaviour
    {
        // Start is called before the first frame update
        GameTitan titan;
        public GamePlayer player;
        public GameObject arrow;
        void Start()
        {
          
            if(player.isLocalPlayer)
            {
                StartCoroutine(CoWaitTitan());
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (!titan) return;

            if(player.isLocalPlayer)
            {
                if (player.Energy > 0)
                {
                    if (arrow.activeSelf == false)
                    {
                        arrow.SetActive(true);
                    }

                    Vector3 targetPostition = new Vector3(titan.transform.position.x, transform.position.y, titan.transform.position.z);
                    Vector3 old = transform.localRotation.eulerAngles;
                    transform.LookAt(targetPostition);

                    transform.localRotation = Quaternion.Euler(old.x, transform.localRotation.eulerAngles.y, old.z);
                }
                else
                {
                    arrow.SetActive(false);
                }
            }
          
        }
        IEnumerator CoWaitTitan()
        {
            yield return new WaitUntil(() => GameManager.instance.GetTitan(player.Team) != null);
            titan = GameManager.instance.GetTitan(player.Team);
        }
    }
}

