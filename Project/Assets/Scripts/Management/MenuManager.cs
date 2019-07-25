using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Mirror;
using FMODUnity;

namespace VoK
{
    public class MenuManager : Singleton<MenuManager>
    {
        [Scene]
        public string currentScene;

        [Header("Debug")]
        public bool showDebugLog = false;

        [Header("Game bag")]
        public Seeker[] gameSeekers;
        public GameObject[] particleEffectsList;

        [Header("Game settings")]
        [HideInInspector]
        public float m_mouseSensivity = 5f;
        [Range(0f,1f)]
        public float m_startMouseSensivityPercent = 0.18f;
        [HideInInspector]
        public float m_zoomMouseSensivity = 5f;
        [Range(0f, 1f)]
        public float m_zoomStartMouseSensivityPercent = 0.1f;
        public float OptionMouseSensitivity { get { return m_mouseSensivity; } }
        public float OptionMouseSensitivityPercent { get { return (m_mouseSensivity - m_minMouseSensitivity) / (m_maxMouseSensitivity - m_minMouseSensitivity); } }
        public float OptionZoomMouseSensitivity { get { return m_zoomMouseSensivity; } }
        public float OptionZoomMouseSensitivityPercent { get { return (m_zoomMouseSensivity - m_minMouseSensitivity) / (m_maxMouseSensitivity - m_minMouseSensitivity); } }
        public float m_minMouseSensitivity = 0.1f;
        public float m_maxMouseSensitivity = 10f;

        public bool invertHorizontalAxis = false;
        public bool invertVerticalAxis = false;

        [BankRef]
        public string masterBank;

        [FMODUnity.EventRef]
        public FMOD.Studio.EventInstance audioEvent;
        public FMOD.Studio.ParameterInstance parameterInstance;
        public StudioEventEmitter menuEmitter;
        public StudioEventEmitter lobbyEmitter;
        public GameObject soundManager;

        public float m_startVolumeMaster = 1f;
        public float volumeMaster;

        public int roundNumber = 1;

        private void Start()
        {
            Application.targetFrameRate = 60;
            SetMouseSensitivity(m_startMouseSensivityPercent);
            SetZoomMouseSensitivity(m_zoomStartMouseSensivityPercent);
            SetVolume(m_startVolumeMaster);
            //emitter = soundManager.GetComponent<StudioEventEmitter>();
            //audioEvent = emitter.EventInstance;
        }

        public float GetMouseSensitivityNormalized()
        {
            return ((m_mouseSensivity - m_minMouseSensitivity) / (m_maxMouseSensitivity - m_minMouseSensitivity));
        }

        public void SetMouseSensitivity(float t)
        {
            m_mouseSensivity = Mathf.Lerp(m_minMouseSensitivity, m_maxMouseSensitivity, t);
        }
        
        public float GetZoomMouseSensitivityNormalized()
        {
            return ((m_zoomMouseSensivity - m_minMouseSensitivity) / (m_maxMouseSensitivity - m_minMouseSensitivity));
        }

        public void SetZoomMouseSensitivity(float t)
        {
            m_zoomMouseSensivity = Mathf.Lerp(m_minMouseSensitivity, m_maxMouseSensitivity, t);
        }
        

        // Lobby buttons
        public void StartHost()
        {
            NetLobbyManager.singleton.StartHost();
            if (lobbyEmitter)
            {
                lobbyEmitter.Play();
            }
            if (menuEmitter)
            {
                menuEmitter.Stop();
            }
        }

        public void StartClient()
        {
            NetLobbyManager.singleton.networkAddress = UIManager.instance.m_ipAddress.text;
            NetLobbyManager.singleton.StartClient();

            if (NetLobbyManager.singleton.client != null)
            {
                if (lobbyEmitter)
                {
                    lobbyEmitter.Play();
                }
                if (menuEmitter)
                {
                    menuEmitter.Stop();
                }
            }
        }

        public void StartGame(int mapID)
        {
            NetLobbyManager.singleton.ServerChangeScene(NetLobbyManager.singleton.GameplayScenes[mapID]);
        } 

        public void CleanUpNetwork()
        {
            NetLobbyManager.singleton.StopHost();
        }

        public void SetLobbyPlayerReady()
        {    
            NetLobbyPlayer.local.NetCmdSendMessage(NetMsg.SetReady, System.Convert.ToInt32(!NetLobbyPlayer.local.readyToBegin).ToString());
        }

        public void SetLobbyPlayerName(string newName)
        {
            Debug.Assert(NetLobbyPlayer.local != null);
            NetLobbyPlayer.local.NetCmdSendMessage(NetMsg.SetName, newName);
        }

        public void SetLobbyPlayerSeeker(uint id)
        {
            NetLobbyPlayer.local.NetCmdSendMessage(NetMsg.SetSeeker, id.ToString());
        }

        public void SetLobbyPlayerTeam(Team t)
        {
            NetLobbyPlayer.local.NetCmdSendMessage(NetMsg.SetTeam, ((int)t).ToString());
        }

        public void SetLobbyPlayerTeamAndSeeker(ButtonEnum buttonEnum)
        {
            if (!NetLobbyPlayer.local.readyToBegin)
            {
                uint seekerId = (uint)buttonEnum.seeker;
                Team team = buttonEnum.team;
                if (NetLobbyPlayer.local.seeker != null)
                {
                    if (NetLobbyPlayer.local.seeker.id == seekerId && NetLobbyPlayer.local.team == team)
                    {
                        seekerId = (uint)SeekerList.None;
                        team = Team.None;
                    }
                }
                NetLobbyPlayer.local.NetCmdSendMessage(NetMsg.SetSeeker, seekerId.ToString(), (int)team);
            }
        }

        public GameObject FindParticleEffect(string particleSystemName)
        {
            foreach (GameObject p in particleEffectsList)
            {
                if (p.name == particleSystemName)
                {
                    return p;
                }
            }
            return null;
        }

        public void SetVolume(float vol)
        {
            volumeMaster = vol;
            FMOD.Studio.Bank mainBank;
            RuntimeManager.StudioSystem.getBank(masterBank,out mainBank);
            FMOD.Studio.Bus[] buses;
            mainBank.getBusList(out buses);
            foreach (FMOD.Studio.Bus bus in buses)
            {
                bus.setVolume(vol);
            }
            //foreach (FMOD.Studio.Bank bank in banks)
            //{
            //    FMOD.Studio.Bus[] buses;
            //    bank.getBusList(out buses);
            //    string sorrata2;
            //    bank.getPath(out sorrata2);
            //    foreach (FMOD.Studio.Bus bus in buses)
            //    {
            //        string sorrata;
            //        bus.getPath(out sorrata);
            //        bus.setVolume(vol);
            //    }
            //}

            //volumeMaster = vol;
            //audioEvent.setVolume(vol);
        }

    }
}
