using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using VoK;

namespace Mirror
{
    public class PlayerSpawner
    {
        public Vector3 position;
        public Quaternion rotation;
        public Team team;
    }

    [DisallowMultipleComponent]
    public class NetPlayerSpawner : MonoBehaviour
    {
        [Tooltip("The team that owns this players spawner")]
        public Team team;

        public void Awake()
        {
            Debug.Assert(team != Team.None);
            NetLobbyManager.RegisterStartPosition(transform, team);
        }

        public void OnDestroy()
        {
            NetLobbyManager.UnRegisterStartPosition(transform);
        }
    }
}
