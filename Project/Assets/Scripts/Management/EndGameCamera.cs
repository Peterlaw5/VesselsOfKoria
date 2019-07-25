using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace VoK
{
    public class EndGameCamera : MonoBehaviour
    {
        public Camera endCamera;
        public Animator titanAnimator;
        Animation anim = null;
        public LineRenderer allyBeam;
        public LineRenderer enemyBeam;
        public LineRenderer realBeam;
        public ParticleSystem allyGlow;
        public ParticleSystem enemyGlow;
        public ParticleSystem realGlow;
        public ParticleSystem  allyGlow2;
        public ParticleSystem enemyGlow2;
        public bool startBeam;
        public bool startTitanAim;
        public bool stopBeam;
        public bool stopTitanAim;

        public float AnimDuration { get { if (anim == null) anim = GetComponent<Animation>(); return anim.clip.length; } }
        private void Awake()
        {
            anim = null;
        }
        private void Start()
        {
            endCamera.enabled = false;
            anim = GetComponent<Animation>();
            anim.Stop();
            allyGlow.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            enemyGlow.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            allyGlow2.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            enemyGlow2.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        public void startEndGameAnimation()
        {
            
            StartCoroutine(CoendGame());
            StartCoroutine(CoWaitStartBeam());
            StartCoroutine(CoWaitStartTitanAnim());
            StartCoroutine(CoWaitStopBeam());
            StartCoroutine(CoWaitStopTitanAnim());
        }
        IEnumerator CoWaitStopTitanAnim()
        {
            yield return new WaitWhile(() => stopTitanAim != true);
            yield return new WaitForSeconds(GameManager.instance.endRoundTime);
            titanAnimator.SetTrigger("reset");
            stopTitanAim = false;
        }
        IEnumerator CoWaitStopBeam()
        {
            yield return new WaitWhile(() => stopBeam != true);
            yield return new WaitForSeconds(GameManager.instance.endRoundTime);
            allyGlow.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            enemyGlow.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            allyGlow2.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            enemyGlow2.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            realBeam.SetPosition(1, Vector3.zero);
            realBeam.gameObject.SetActive(false);
            stopBeam = false;
        }
        IEnumerator CoWaitStartTitanAnim()
        {
            yield return new WaitWhile(() => startTitanAim != true);
            titanAnimator.SetTrigger("attack");
            startTitanAim = false;
        }
        IEnumerator CoWaitStartBeam()
        {
            yield return new WaitWhile(() => startBeam != true);
            realGlow.Play(true);
            yield return new WaitForSeconds(2.5f);
           

            realBeam.gameObject.SetActive(true);
            var lenght = 0f;
            var maxlenght = 300f;//Vector3.Distance(line.transform.position, new Vector3(line.transform.position.x, line.transform.position.y, line.transform.position.z));

            while (Mathf.Abs(lenght - maxlenght) > 1f)
            {
                lenght = Mathf.Lerp(lenght, maxlenght, Time.deltaTime*0.8f);
                realBeam.SetPosition(1, new Vector3(0, 0, lenght));
                yield return null;
            }
            realBeam.SetPosition(1, new Vector3(0, 0, maxlenght));
            
            startBeam = false;
        }

        IEnumerator CoendGame()
        {
            endCamera.enabled = true;
            if(realGlow==allyGlow)
            {
                allyGlow2.Play();
                
            }else
            {
                enemyGlow2.Play();
            }
            anim.Play();
            yield return new WaitForSeconds(anim.clip.length);
            yield return new WaitUntil(() => VoK.GamePlayer.local.mainCamera.enabled == true);
            endCamera.enabled = false;
        }
    }

}
