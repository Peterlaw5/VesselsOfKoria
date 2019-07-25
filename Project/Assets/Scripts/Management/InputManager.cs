using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace VoK
{
    public interface IInputActor
    {
        float GetAxis(InputKey inputAxes);
        bool GetKeyDown(InputKey inputButton);
        bool GetKeyUp(InputKey inputButton);
        bool GetKey(InputKey inputButton);
    }
    public class InputManager : MonoBehaviour, IInputActor
    {
        public KeyCode m_attackKey = KeyCode.Mouse0;
        public KeyCode m_jumpKey = KeyCode.Space;
        public KeyCode m_mobilityKey = KeyCode.LeftShift;
        public KeyCode m_firstAbilityKey = KeyCode.Mouse1;
        public KeyCode m_secondAbilityKey = KeyCode.Q;
        public KeyCode m_ultimateAbilityKey = KeyCode.E;        
        public KeyCode m_pause = KeyCode.P;
        public KeyCode m_recapAbility = KeyCode.K;


        public float GetAxis(InputKey inputAxes)
        {
            switch (inputAxes)
            {
                case InputKey.Vertical: return Input.GetAxis("Vertical");
                case InputKey.Horizontal: return Input.GetAxis("Horizontal");
            }
            return 0f;
        }

        public bool GetKeyDown(InputKey inputKey)
        {
            switch(inputKey)
            {
                case InputKey.BaseShoot: return Input.GetKeyDown(m_attackKey);
                case InputKey.Jump: return Input.GetKeyDown(m_jumpKey);
                case InputKey.Dash: return Input.GetKeyDown(m_mobilityKey);
                case InputKey.FirstShoot: return Input.GetKeyDown(m_firstAbilityKey);
                case InputKey.SecondShoot: return Input.GetKeyDown(m_secondAbilityKey);
                case InputKey.UltimateShoot: return Input.GetKeyDown(m_ultimateAbilityKey);
                case InputKey.Any: return Input.GetKeyDown(m_ultimateAbilityKey) || Input.GetKeyDown(m_secondAbilityKey) || Input.GetKeyDown(m_firstAbilityKey) || Input.GetKeyDown(m_mobilityKey) || Input.GetKeyDown(m_attackKey) || Input.GetKeyDown(m_jumpKey);
                case InputKey.Pause: return Input.GetKeyDown(m_pause);
                case InputKey.RecapAbility: return Input.GetKeyDown(m_recapAbility);
            }
            return false;
        }
        public bool GetKeyUp(InputKey inputKey)
        {
            switch (inputKey)
            {
                case InputKey.BaseShoot: return Input.GetKeyUp(m_attackKey);
                case InputKey.Jump: return Input.GetKeyUp(m_jumpKey);
                case InputKey.Dash: return Input.GetKeyUp(m_mobilityKey);
                case InputKey.FirstShoot: return Input.GetKeyUp(m_firstAbilityKey);
                case InputKey.SecondShoot: return Input.GetKeyUp(m_secondAbilityKey);
                case InputKey.UltimateShoot: return Input.GetKeyUp(m_ultimateAbilityKey);
                case InputKey.Any: return Input.GetKeyUp(m_ultimateAbilityKey) || Input.GetKeyUp(m_secondAbilityKey) || Input.GetKeyUp(m_firstAbilityKey) || Input.GetKeyUp(m_mobilityKey) || Input.GetKeyUp(m_attackKey) || Input.GetKeyUp(m_jumpKey);
                case InputKey.Pause: return Input.GetKeyUp(m_pause);
                case InputKey.RecapAbility: return Input.GetKeyUp(m_recapAbility);
            }
            return false;
        }
        public bool GetKey(InputKey inputKey)
        {
            switch (inputKey)
            {
                case InputKey.Pause: return Input.GetKey(m_pause);
                case InputKey.BaseShoot: return Input.GetKey(m_attackKey);
                case InputKey.Jump: return Input.GetKey(m_jumpKey);
                case InputKey.Dash: return Input.GetKey(m_mobilityKey);
                case InputKey.FirstShoot: return Input.GetKey(m_firstAbilityKey);
                case InputKey.SecondShoot: return Input.GetKey(m_secondAbilityKey);
                case InputKey.UltimateShoot: return Input.GetKey(m_ultimateAbilityKey);
                case InputKey.Any: return Input.GetKey(m_ultimateAbilityKey) || Input.GetKey(m_secondAbilityKey) || Input.GetKey(m_firstAbilityKey) || Input.GetKey(m_mobilityKey) || Input.GetKey(m_attackKey) || Input.GetKey(m_jumpKey);
                case InputKey.RecapAbility: return Input.GetKey(m_recapAbility);
    }
            return false;
        }
    }
}
