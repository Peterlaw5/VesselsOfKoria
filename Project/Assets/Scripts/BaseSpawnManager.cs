using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace VoK
{
    public class BaseSpawnManager : MonoBehaviour
    {
        // Start is called before the first frame update
        public Team myTeam;
        public ParticleSystem[] allySpawn;
        public ParticleSystem[] enemySpawn;
        void Start()
        {
            myTeam = GetComponent<TeamBasedEffectsManager>().myTeam;
            foreach(ParticleSystem p in allySpawn)
            {
                p.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
            foreach (ParticleSystem p in enemySpawn)
            {
                p.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
            //GameManager.instance.spawnBases.Add(gameObject.GetComponent<BaseSpawnManager>());
        }

        // Update is called once per frame
        public void SpawnEffect(Vector3 position,ParticleSystem[] list)
        {
            ParticleSystem nearest=list[0];
            foreach (ParticleSystem p in list)
            {
               if(Vector3.Distance(position,p.transform.position)<Vector3.Distance(position,nearest.transform.position))
               {
                    nearest = p;
               }
            }
            nearest.Play(true);
        }
    }

}
