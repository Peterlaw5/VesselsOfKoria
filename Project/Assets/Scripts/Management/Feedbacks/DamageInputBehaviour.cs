using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoK
{
    public class DamageInputBehaviour : MonoBehaviour
    {
        // Start is called before the first frame update
        public Material mat;
        public Vector3 DamagePosition;
        private GamePlayer player;
        ParticleSystem ps;
        GameObject pointer;
        //void Start()
        //{
        //    player =GamePlayer.local;
        //    ps = GetComponentInChildren<ParticleSystem>();
        //    mat = GetComponentInChildren <ParticleSystemRenderer>().material;
        //}

        public void Setup(Vector3 target, float amount)
        {

            player = GamePlayer.local;
            ps = GetComponentInChildren<ParticleSystem>();
            mat = GetComponentInChildren<ParticleSystemRenderer>().material;

            DamagePosition = target;
            mat.SetFloat("_Level", amount);
        }

        // Update is called once per frame
        void Update()
        {
            Vector3 targetPosition = new Vector3(DamagePosition.x,player.transform.position.y, DamagePosition.z);



            Vector3 old = transform.localRotation.eulerAngles;
            Transform fakeTransform = new GameObject().transform;
            pointer = fakeTransform.gameObject;
            fakeTransform.parent = player.damageInputPoint.transform;
            fakeTransform.localPosition = Vector3.zero;
            fakeTransform.LookAt(targetPosition);

            transform.localRotation = Quaternion.Euler(old.x, old.y, 360f-fakeTransform.localRotation.eulerAngles.y);
            if(ps == null || player.IsDead)
            {
                Destroy(gameObject);
            }
            
        }
        private void OnDestroy()
        {
            Destroy(pointer);
        }
    }

}
