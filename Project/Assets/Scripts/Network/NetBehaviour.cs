using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mirror
{
    #region Enums
    public enum NetMsg
    {
        SetName,
        SetReady,
        SetSeeker,
        SetTeam,
        SetIndex,
        SetStats,           //5
        CastAbility,
        SetHealth,
        Die,
        Respawn,
        Respawned,          //10
        SetLobbyPlayer,
        StartCooldown,
        SetEnergyCarried,
        SetItem,
        UpdateGenerator,    //15
        UpdateBase,
        PlayersInArea,
        Update,
        SetCooldown,
        Hit,               //20
        SpawnPS,
        ScreenMsg,
        Animation,
        Reload,
        Time,
        RedirectionClear,
        UpdateLastPlayerHitId,
        Kill,
        PlaySound,
        PlaySoundAt,
        AnimateCrosshair,
        SetParameterAt
        
    }

    public enum NetMsgType
    {
        Cmd,
        Rpc,
        Target
    }

    #endregion

    public class NetBehaviour : NetworkBehaviour
    {       

       

        #region Message Serialization Management

        // == Message management ==

        public readonly static char NET_MSG_CHAR = '|';

        public static string SerializeMessage(params object[] parameters)
        {
            string serializedMessage = string.Empty;
            
            foreach (var s in parameters)
            {
                if (serializedMessage == string.Empty)
                {
                    serializedMessage = s.ToString();
                }
                else
                {
                    serializedMessage = System.String.Format("{0}{1}{2}", serializedMessage, NET_MSG_CHAR.ToString(), s.ToString());
                }
            }
            return serializedMessage;
        }

        public static string SerializeArray<T>(T[] arrayToSerialize)
        {
            string serializedMessage = string.Empty;

            foreach (var s in arrayToSerialize)
            {
                if (serializedMessage == string.Empty)
                {
                    serializedMessage = s.ToString();
                }
                else
                {
                    serializedMessage = System.String.Format("{0}{1}{2}", serializedMessage, NET_MSG_CHAR.ToString(), s.ToString());
                }
            }
            return serializedMessage;
        }

        public static string SerializeMessage(string firstPart, string lastPart)
        {
            return System.String.Format("{0}{1}{2}", firstPart, NET_MSG_CHAR.ToString(), lastPart);
        }

        public static string[] DeserializeMessage(string msg)
        {
            return msg.Split(NET_MSG_CHAR);
        }

        #endregion


        // == Hook functions ==

        public void NetCmdSendMessage(NetMsg msgType, params object[] msg)
        {
            CmdSendMessage(SerializeMessage((int)msgType, SerializeArray(msg)));
        }

        public void NetTargetSendMessage(NetMsg msgType, NetworkConnection conn, params object[] msg)
        {
            TargetSendMessage(conn, SerializeMessage((int)msgType, SerializeArray(msg)));
        }

        public void NetRpcSendMessage(NetMsg msgType, params object[] msg)
        {
            RpcSendMessage(SerializeMessage((int)msgType, SerializeArray(msg)));
        }

        // == Mirror message networking implementation ==

        [Command]
        public void CmdSendMessage(string msg)
        {
            if (!CheckMessage(msg, NetMsgType.Cmd)) return;
            ExecuteMessage(msg, NetMsgType.Cmd);
        }

        [ClientRpc]
        public void RpcSendMessage(string msg)
        {
            if (!CheckMessage(msg, NetMsgType.Rpc)) return;
            ExecuteMessage(msg, NetMsgType.Rpc);
        }

        [TargetRpc]
        public void TargetSendMessage(NetworkConnection conn, string msg)
        {
            if (!CheckMessage(msg, NetMsgType.Target)) return;
            ExecuteMessage(msg, NetMsgType.Target);
        }

        // Check if a message is allowed
        public virtual bool CheckMessage(string msg, NetMsgType netMsgType)
        {
            /*
            string[] msgArray = DeserializeMessage(msg);
            NetMsg msgIndex = (NetMsg)System.Convert.ToInt32(msgArray[0]);
            switch (msgIndex)
            {
                case ... :
                {
                    if (netMsgType == NetMsgType.Cmd)
                    {
                        if (condition) return false;
                    }
                    if (netMsgType == NetMsgType.Rpc)
                    {
                        if (condition2) return false;
                    }
                    return true;
                }  
                case ... : if(netMsgType == NetMsgType.Cmd) return true;
            }*/
            return false;
        }

        // == Messages handling
        public virtual void ExecuteMessage(string msg, NetMsgType netMsgType)
        {

            /*
            string[] msgArray = DeserializeMessage(msg);
            NetMsg msgIndex = (NetMsg)System.Convert.ToInt32(msgArray[0]);

            switch (msgIndex)
            {
                case ...1 : if(netMsgType == NetMsgType.Cmd) DoThis();
                case ...2 :
                {
                    if(netMsgType == NetMsgType.Cmd) DoThat();
                    if(netMsgType == NetMsgType.Rpc) DoSomething();
                }
            }*/
        }
    }
}
