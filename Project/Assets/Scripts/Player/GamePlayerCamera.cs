using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace VoK
{
    public class GamePlayerCamera : MonoBehaviour
    {
        [Tooltip("Connected transform to avoid camera children")]
        public Transform m_connectedTransform;
        [Tooltip("If true mouse vertical axis is inverted")]
        public bool m_invertVerticalAxis = false;
        [Tooltip("If true mouse horizontal axis is inverted")]
        public bool m_invertHorizontalAxis = false;
        //[Tooltip("Mouse movement speed")]
        //public float m_mouseSensitivity = 5.0f;
        [Range(1f, 10f)]
        [Tooltip("Mouse smooth factor")]
        public float m_mouseSmoothing = 2.0f;
        [Range(0f, 90f)]
        [Tooltip("Mouse vertical max angle limit")]
        public float m_maxVerticalAngle = 45f;

        private Vector2 m_mouseLook;
        private Vector2 m_smoothV;

        private GameObject m_player;
        private GamePlayer m_playerController;

        private Vector3 m_offsetToConnectedTransform;

        public float Angle { get { return transform.rotation.eulerAngles.x; } }

        void Awake()
        {
            m_player = transform.parent.gameObject;
            m_playerController = m_player.GetComponent<GamePlayer>();
            m_smoothV = Vector2.zero;
            m_offsetToConnectedTransform = m_connectedTransform.position - transform.position;
            m_mouseLook = new Vector2(Vector3.SignedAngle(Vector3.forward, m_player.transform.forward, Vector3.up), 0f);
        }

        // Update is called once per frame
        void Update()
        {
            // Apply only to local player
            if (!m_playerController.isLocalPlayer) return;
            if (m_playerController.IsDead) return;
            if (!m_playerController.CanRotateVisual) return;
            if (UIGame.instance.isPause) return;

            Vector2 mouseDirection = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

            float sensitivity = MenuManager.Instance.OptionMouseSensitivity;

            if(m_playerController.startingFOV != m_playerController.mainCamera.fieldOfView)
            {
                sensitivity = MenuManager.Instance.OptionZoomMouseSensitivity;
            }

            mouseDirection.x *= sensitivity * (MenuManager.Instance.invertHorizontalAxis ? -1f : 1f);
            mouseDirection.y *= sensitivity * (MenuManager.Instance.invertVerticalAxis ? -1f : 1f);

            m_smoothV = Vector2.Lerp(m_smoothV, mouseDirection, 1f / m_mouseSmoothing);

            m_mouseLook += m_smoothV;
            m_mouseLook.y = Mathf.Clamp(m_mouseLook.y, -m_maxVerticalAngle, m_maxVerticalAngle);
            transform.localRotation = Quaternion.AngleAxis(-m_mouseLook.y, Vector3.right);
            m_player.transform.rotation = Quaternion.AngleAxis(m_mouseLook.x, m_player.transform.up);
            if (m_connectedTransform != null)
            {
                m_connectedTransform.position = transform.position + transform.TransformVector(m_offsetToConnectedTransform);
                m_connectedTransform.localRotation = transform.localRotation;
            }
        }

        public void ResetCamera()
        {
            m_mouseLook = new Vector2(Vector3.SignedAngle(Vector3.forward, m_player.transform.forward, Vector3.up), 0f);
        }
        
       

    }
}