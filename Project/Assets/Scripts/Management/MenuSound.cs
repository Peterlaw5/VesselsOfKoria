using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using Mirror;

namespace VoK
{
    public class MenuSound : MonoBehaviour
    {
        public StudioEventEmitter menuEmitter;
        public StudioEventEmitter lobbyEmitter;
        // Start is called before the first frame update
        void Start()
        {
            StartCoroutine(CoFindMenu());
        }
        IEnumerator CoFindMenu()
        {
            yield return new WaitUntil(() => MenuManager.Instance != null);
            MenuManager.Instance.lobbyEmitter = lobbyEmitter;
            MenuManager.Instance.menuEmitter = menuEmitter;
            if (MenuManager.Instance.lobbyEmitter)
            {
                MenuManager.Instance.lobbyEmitter.Stop();
            }
            if (MenuManager.Instance.menuEmitter)
            {
                MenuManager.Instance.menuEmitter.Play();
            }
            //MenuManager.Instance.emitter = emitter;
            //MenuManager.Instance.soundManager = gameObject;
            //MenuManager.Instance.audioEvent = emitter.EventInstance;
            //MenuManager.Instance.SetVolume(MenuManager.Instance.volumeMaster);
        }
    }
}
