using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using FMODUnity;

namespace VoK
{
    public class ItemSpawner : NetBehaviour
    {
        [Header("Spawned item settings")]
        [Tooltip("Item spawned prefab")]
        public GameObject m_itemToSpawn;
        [Tooltip("Item spawned min quantity")]
        public float m_itemMinAmount = 1f;
        [Tooltip("Item spawned max quantity")]
        public float m_itemMaxAmount = 1f;
        [Tooltip("Time to despawn for spawned item")]
        public float m_itemDespawnTime = 5f;

        [Header("Item Spawner settings")]
        [Tooltip("Min radius of the circular area to spawn into")]
        public float m_spawnAreaMinRadius = 0f;
        [Tooltip("Max radius of the circular area to spawn into")]
        public float m_spawnAreaMaxRadius = 0f;
        [Tooltip("Time between spawn event")]
        public float m_spawnTime = 5f;
        [Tooltip("how long before the ray is activated before the spawn ")]
        public float m_activateBeamBeforeSpawn = 2f;


        //public float timerParabola;
        [Tooltip("Min time to reach final point")]
        public float minTimerParabola;
        [Tooltip("Max time to reach final point")]
        public float maxTimerParabola;

        //public float height;
        [Tooltip("Min height of parabola")]
        public float minHeight;
        [Tooltip("Max height of parabola")]
        public float maxHeight;

        public bool useParabolicPath = false;


        [Tooltip("Vertical offset for spawned items position")]
        public float m_spawnVerticalOffset = 1f;
        [Tooltip("Number of items to spawn each time")]
        public int m_spawnAmountPerTime = 1;
        [Tooltip("How many times the spawner will spawn objects - set it to 0 for infinity")]
        public int m_spawnCycles = 0;
        [Tooltip("If true, item spawned will respawn only after that old ones are picked up/destroyed")]
        public bool m_respawnOnlyAfterPickup = false;
        public bool m_spawnInfoMessage = false;
        [Tooltip("Time prespawn message")]
        public float m_spawnTimePreMessage = 5f;

        public StudioEventEmitter spawnerEmitter;
        public List<GameObject> ObjectsToRotate;
        public float maxRotatonSpeed;
        public float minRotatonSpeed;
        List<Coroutine> korRotating;
        public GameObject lightBeam;
        List<GameObject> m_spawnedItems;
        // Start is called before the first frame update
        void Start()
        {
            korRotating = new List<Coroutine>();
            if(lightBeam != null)
            {
                lightBeam.SetActive(false);
            }
            else
            {
                m_activateBeamBeforeSpawn = 0f;
            }
            if (isServer)
            {
                m_spawnedItems = new List<GameObject>();
                StartCoroutine(CoSpawn());
                GameManager.instance.OnRoundStart += CleanUpSpawner;
            }
        }

        public void SpawnItem()
        {
            GameObject spawnedItem = Instantiate(m_itemToSpawn);
            m_spawnedItems.Add(spawnedItem);
            Item item = spawnedItem.GetComponent<Item>();
            if (useParabolicPath)
            {
                spawnedItem.transform.position = transform.position + (m_spawnVerticalOffset * Vector3.up);
                float distance = Random.Range(m_spawnAreaMinRadius, m_spawnAreaMaxRadius); //l           
                float angle = Random.Range(0f, 360f);

                item.StartPath(distance, Random.Range(minHeight, maxHeight), Random.Range(minTimerParabola, maxTimerParabola), m_spawnVerticalOffset, angle);
            }
            else
            {
                Vector3 dropPosition = Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.up) * Vector3.right * Random.Range(m_spawnAreaMinRadius, m_spawnAreaMaxRadius);
                spawnedItem.transform.position = new Vector3(dropPosition.x, m_spawnVerticalOffset, dropPosition.z) + transform.position;
            }
            item.SetQuantity(Random.Range(m_itemMinAmount, m_itemMaxAmount));
            item.m_despawnTime = (m_respawnOnlyAfterPickup ? -1 : m_itemDespawnTime);
            //spawnedItem.GetComponent<Item>().SetQuantity(Random.Range(m_itemMinAmount,m_itemMaxAmount));
            //spawnedItem.GetComponent<Item>().m_despawnTime = (m_respawnOnlyAfterPickup ? -1 : m_itemDespawnTime);

            NetworkServer.Spawn(spawnedItem);
        }


        IEnumerator CoSpawn()
        {
            int counter = (m_spawnCycles == 0 ? -1 : 0);
            while(counter < m_spawnCycles)
            {
                yield return new WaitForSeconds(m_spawnTime - m_spawnTimePreMessage);
                if (m_spawnInfoMessage)
                {
                    GameManager.instance.NetRpcSendMessage(NetMsg.ScreenMsg, UIGame.instance.textManager.energyGoingToSpawnMsg, m_spawnTimePreMessage);
                    if(lightBeam)
                    {
                        NetRpcSendMessage(NetMsg.Update, System.Convert.ToString(true));
                    }
                   
                    //yield return CoActivateBeam();
                }
			
                if(m_spawnTimePreMessage != 0f)
                {
					yield return new WaitForSeconds(m_spawnTimePreMessage);
                }     
                m_spawnedItems = new List<GameObject>();
                for (int i = 0; i < m_spawnAmountPerTime; i++)
                {
                    SpawnItem();
                }
                if(m_spawnInfoMessage)
                {
                    if (lightBeam)
                    {
                        NetRpcSendMessage(NetMsg.Update, System.Convert.ToString(false));
                    }
                    GameManager.instance.NetRpcSendMessage(NetMsg.ScreenMsg, UIGame.instance.textManager.energyHasSpawnedMsg);
                }
                if(m_spawnCycles > 0) counter++;
                if (m_respawnOnlyAfterPickup)
                {
                    yield return new WaitUntil(() => isItemListEmpty());
                }
            }
        }

        bool isItemListEmpty()
        {
            if (m_spawnedItems.Count == 0) return true;
            foreach (GameObject g in m_spawnedItems)
            {
                if (g != null)
                {
                    return false;
                }
            }
            return true;
        }

        public void CleanUpSpawner()
        {
            StopAllCoroutines();
            foreach(GameObject g in m_spawnedItems)
            {
                Destroy(g);
            }
            if (lightBeam != null)
            {
                lightBeam.SetActive(false);
            }
            else
            {
                m_activateBeamBeforeSpawn = 0f;
            }
            m_spawnedItems = new List<GameObject>();
            if (lightBeam)
            {
                NetRpcSendMessage(NetMsg.Update, System.Convert.ToString(false));
            }
            StartCoroutine(CoSpawn());
        }

        public override bool CheckMessage(string msg, NetMsgType netMsgType)
        {
            string[] msgArray = DeserializeMessage(msg);
            NetMsg msgIndex = (NetMsg)System.Convert.ToInt32(msgArray[0]);
            switch (msgIndex)
            {
                case NetMsg.Update:
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
                case NetMsg.Update:
                {
                    if (System.Convert.ToBoolean(msgArray[1]))
                    {
                        StartCoActivateBeam();
                        if(spawnerEmitter != null) spawnerEmitter.Play();
                    }
                    else
                    {
                        StartCoStopBeam();
                    }
                  
                }
                break;
            }
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }
        void StartCoActivateBeam()
        {
            StartCoroutine(CoActivateBeam());
        }

        IEnumerator CoActivateBeam()
        {
            
              
            if(lightBeam)
            {
                foreach (GameObject g in ObjectsToRotate)
                {
                    korRotating.Add(StartCoroutine(CoRotateObject(g)));
                }

                lightBeam.SetActive(true);
                var lenght = 0f;
                var line = lightBeam.GetComponent<LineRenderer>();
               
                var maxlenght = 600f;//Vector3.Distance(line.transform.position, new Vector3(line.transform.position.x, line.transform.position.y, line.transform.position.z));
                
                while (Mathf.Abs(lenght-maxlenght)>1f)
                {
                    lenght = Mathf.Lerp(lenght, maxlenght, Time.deltaTime);
                    line.SetPosition(1, new Vector3(0, 0, lenght));
                    yield return null;
                }
                line.SetPosition(1, new Vector3(0, 0, maxlenght));
            }
           

        }

        IEnumerator CoRotateObject(GameObject g)
        {
            float speed = Random.Range(maxRotatonSpeed, minRotatonSpeed);
            if(Random.Range(0f,1f)<0.5f)
            {
                speed = speed * -1;
            }
            for(; ; )
            {
                g.transform.Rotate(0, Time.deltaTime * speed, 0);
                yield return null;
            }
        }
        void StartCoStopBeam()
        {
            StartCoroutine(CoStopBeam());
        }
        IEnumerator CoStopBeam()
        {
            foreach (Coroutine c in korRotating)
            {
                StopCoroutine(c);
            }
            var length = 0f;
            var line = lightBeam.GetComponent<LineRenderer>();
            var maxlenght = 600f;
            Transform[] particles= lightBeam.GetComponentsInChildren<Transform>();
            foreach (Transform g in particles )
            {
                if(g.gameObject != lightBeam)
                {
                    g.GetComponent<ParticleSystem>().Stop(false,ParticleSystemStopBehavior.StopEmitting);
                }
               
            }
            while (Mathf.Abs(length - maxlenght) > 1f)
            {               
                length = Mathf.Lerp(length, maxlenght, Time.deltaTime);
                line.SetPosition(0, new Vector3(0, 0, length));
                yield return null;
            }

      
            line.SetPosition(0, new Vector3(0f, 0f, 0f));
            lightBeam.SetActive(false);
        }

        private void Update()
        {
            if (GameManager.instance != null)
            {
                if(spawnerEmitter != null)
                {
                    if(GameManager.instance.isRoundOver && spawnerEmitter.IsPlaying())
                    {
                        spawnerEmitter.Stop();
                    }
                }
            }
        }


    }
}
