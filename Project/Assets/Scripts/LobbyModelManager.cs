using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace VoK
{
    
    public class LobbyModelManager : MonoBehaviour
    {
        // Start is called before the first frame update
        public Animator soris;
        public Animator ogertha;
        public Animator vayvin;
        void Start()
        {
            Clean();
        }
        public void Clean()
        {
            soris.gameObject.SetActive(false);
            ogertha.gameObject.SetActive(false);
            vayvin.gameObject.SetActive(false);
        }

        public void ActivateModel(SeekerList target)
        {
            switch (target)
            {
                case SeekerList.None:
                    break;
                case SeekerList.Vayvin:
                    {
                        if(vayvin.gameObject.activeSelf==false)
                        {
                            soris.gameObject.SetActive(false);
                            ogertha.gameObject.SetActive(false);
                            vayvin.gameObject.SetActive(true);
                        }
                        
                    }
                    break;
                case SeekerList.Ogertha:
                    {
                        if (ogertha.gameObject.activeSelf == false)
                        {
                            soris.gameObject.SetActive(false);
                            ogertha.gameObject.SetActive(true);
                            vayvin.gameObject.SetActive(false);
                        }
                    }
                    break;
                case SeekerList.Soris:
                    {
                        if (soris.gameObject.activeSelf == false)
                        {
                            soris.gameObject.SetActive(true);
                            ogertha.gameObject.SetActive(false);
                            vayvin.gameObject.SetActive(false);
                        }
                    }
                    break;
                default:
                    break;
            }
        }
        public void AnimateModel(SeekerList target)
        {
            switch (target)
            {
                case SeekerList.None:
                    break;
                case SeekerList.Vayvin:
                    vayvin.SetTrigger("selection");
                    break;
                case SeekerList.Ogertha:
                    ogertha.SetTrigger("selection");
                    break;
                case SeekerList.Soris:
                    soris.SetTrigger("selection");
                    break;
                default:
                    break;
            }
        }
    }

}
