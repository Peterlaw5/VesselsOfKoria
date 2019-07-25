using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace VoK
{
    public class Item : NetBehaviour
    {
        
        [HideInInspector]
        public GameObject creator;
        [Header("Item settings")]
        [Tooltip("Pickupable item type")]
        public ItemType itemType;
        [Tooltip("Quantity owned by this item")]
        public float m_quantityOwned;
        //public float m_mainTimer = 5f;
        [Tooltip("Time to despawn")]
        public float m_despawnTime = 10f;
        public float m_rotationXSpeed = 1f;
        public float m_rotationYSpeed = 0f;
        public float m_rotationZSpeed = 0f;

        bool itemUsed;

        void Start()
        {
            itemUsed = false;
            if (isServer)
            {
                StartCoroutine(CoVanishAfterTot());
            }
        }
        
        void Update()
        {
            transform.Rotate(m_rotationXSpeed * Time.deltaTime, m_rotationYSpeed * Time.deltaTime, m_rotationZSpeed * Time.deltaTime);
        }

        public void OnDisable()
        {
            StopAllCoroutines();
        }

        private void OnTriggerStay(Collider other)
        {
            if (isServer && !itemUsed)
            {
                if (other.tag == "Player")
                {
                    GamePlayer player = other.GetComponent<GamePlayer>();
                    if (player != null && !player.IsDead && player.m_canGetEnergy)
                    {
                        switch (itemType)
                        {
                            case ItemType.Energy:
                            {
                                if (!player.HasFullEnergy())
                                {
                                    player.AddEnergy(m_quantityOwned);
                                    itemUsed = true;
                                    NetRpcSendMessage(NetMsg.Update);
                                    NetTargetSendMessage(NetMsg.PlaySound, player.connectionToClient);  
                                }
                                break;
                            }
                            case ItemType.Medikit:
                            {
                                if (!player.HasFullHealth())
                                {
                                    player.AddHealth(m_quantityOwned);
                                    itemUsed = true;
                                    NetRpcSendMessage(NetMsg.Update);
                                    NetTargetSendMessage(NetMsg.PlaySound, player.connectionToClient);
                                }
                                break;
                            }
                        }
                    }
                }
            }
        }

        public override bool CheckMessage(string msg, NetMsgType netMsgType)
        {
            string[] msgArray = DeserializeMessage(msg);
            NetMsg msgIndex = (NetMsg)System.Convert.ToInt32(msgArray[0]);
            switch (msgIndex)
            {
                case NetMsg.PlaySound:
                case NetMsg.Update:
                case NetMsg.SetItem:
                {
                    return true;
                }
            }
            return false;
        }

        public override void ExecuteMessage(string msg, NetMsgType netMsgType)
        {
            string[] msgArray = DeserializeMessage(msg);
            NetMsg msgIndex = (NetMsg)System.Convert.ToInt32(msgArray[0]);
            switch (msgIndex)
            {
                case NetMsg.PlaySound:
                {
                    if (netMsgType == NetMsgType.Target)
                    {
                        GetComponent<FMODUnity.StudioEventEmitter>()?.Play();
                        CmdSendMessage(msg);
                    }
                    if (netMsgType == NetMsgType.Cmd)
                    {
                        Destroy(gameObject);
                    }
                    break;
                }
                case NetMsg.Update:
                {
                    if (netMsgType == NetMsgType.Rpc)
                    {
                        itemUsed = true;
                        foreach (MeshRenderer mr in GetComponentsInChildren<MeshRenderer>())
                        {
                            mr.enabled = false;
                        }
                        foreach (ParticleSystem mr in GetComponentsInChildren<ParticleSystem>())
                        {
                            Destroy(mr);
                        }
                        Destroy(gameObject, 2f);
                    }
                    break;
                }
                case NetMsg.SetItem:
                {
                    if (netMsgType == NetMsgType.Rpc)
                    {
                        m_quantityOwned = System.Convert.ToSingle(msgArray[1]);
                    }
                    break;
                }
            }
        }

        public void SetQuantity(float amount)
        {
            m_quantityOwned = amount;
            StartCoroutine(CoWaitItemSpawned());
        }

        IEnumerator CoVanishAfterTot()
        {
            //yield return new WaitForSeconds(m_mainTimer);
            //creator = null;
            if (m_despawnTime == -1) yield break;
            yield return new WaitForSeconds(m_despawnTime);
            Destroy(gameObject);
        }

        IEnumerator CoWaitItemSpawned()
        {
            yield return new WaitUntil(() => transform != null && gameObject != null);
            NetRpcSendMessage(NetMsg.SetItem, m_quantityOwned);
        }

        public void  StartPath(float l, float h, float time, float c, float angle)
        {
            StartCoroutine(CoItemPath(l, h, time, c, angle));
        }

        public IEnumerator CoItemPath(float l, float h, float time, float c, float angle)
        {
            float t = 0;
            Vector3 startPos = transform.position;

            while(t < time)
            {

                yield return null;
                t += Time.deltaTime;

                Vector3 parabolaPoint = Vector3.zero;                
                parabolaPoint.x = l / time * t;
                parabolaPoint.y = (1f - (t / time)) * ((4f * h * t) / time) + c;

                //rotation
                parabolaPoint = Quaternion.Euler(0, angle, 0) * parabolaPoint;
                transform.position = startPos + parabolaPoint;

                

            }

            Vector3 parabolaFinalPoint = Vector3.zero;
            parabolaFinalPoint.x = l ;
            parabolaFinalPoint.y = c;

            //rotation
            parabolaFinalPoint = Quaternion.Euler(0, angle, 0) * parabolaFinalPoint;
            transform.position = startPos + parabolaFinalPoint;


        }



    }
}
