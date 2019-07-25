using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoK
{
    public class KorFinalAttack : MonoBehaviour
    {
        public LineRenderer line;
        public GameObject kor;
        public GameObject enemyParticleEffect;
        public GameObject allyParticleEffect;
        public GameTitan mytitan;
        public Material allyBeam;
        public Material enemyBeam;
        bool laserStarted = false;
        // Start is called before the first frame update
        void Start()
        {
            laserStarted = false;
            // particleEffect.GetComponent<ParticleSystem>().Play(false);

            //line.enabled=false;
        }

        // Update is called once per frame
        void Update()
        {
            line.transform.LookAt(mytitan.titanCentre.transform);
        }
        private void OnEnable()
        {
            /*
            
            if (mytitan)
            {
                Debug.Log(Vector3.Distance(line.transform.position, mytitan.titanCentre.position));
                var pos = line.GetPosition(0);
              
                

            }*/
        }
        public void activate()
        {
            if(mytitan.Team== GamePlayer.local.LobbyPlayer.team)
            {
                allyParticleEffect.SetActive(true);
                enemyParticleEffect.SetActive(false);
                allyParticleEffect.GetComponent<ParticleSystem>().Play(true);
                line.GetComponent<LineRenderer>().material = allyBeam;
            }
            else
            {
                allyParticleEffect.SetActive(false);
                enemyParticleEffect.SetActive(true);
                enemyParticleEffect.GetComponent<ParticleSystem>().Play(true);
                line.GetComponent<LineRenderer>().material= enemyBeam;
            }
  //line.SetPosition(1, new Vector3(0, 0, Vector3.Distance(line.transform.position, mytitan.titanCentre.position)));
            StartCoroutine(CoStartLaser());
        }

        IEnumerator CoStartLaser()
        {
            if (laserStarted) yield break;
            var lenght = 0f;
            var maxlenght = Vector3.Distance(line.transform.position, mytitan.titanCentre.position);
            while (Mathf.Abs(lenght - maxlenght) < 0.1)
            {
                //Debug.Log(lenght);
                lenght = Mathf.Lerp(lenght, maxlenght, 0.2f);
                line.SetPosition(1, new Vector3(0, 0, lenght));
              
                yield return null;
            }
            line.SetPosition(1, new Vector3(0, 0,maxlenght));
            laserStarted = true;
        }
    }
}

