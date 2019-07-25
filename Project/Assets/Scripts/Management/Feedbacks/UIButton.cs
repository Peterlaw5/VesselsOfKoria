using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using FMODUnity;


public class UIButton : MonoBehaviour, IPointerEnterHandler,IPointerDownHandler
{
    [Tooltip("Emitter reference")]
    public StudioEventEmitter emitterEvent;
    [Header("Button Parameters")]
    [Tooltip("Set to true to silence this button")]
    public bool silenced = false;
    public string parameterName = "Button";
    public float mouseEnterValue = 0f;
    public float mouseOverValue = 1f;
    Button button;
    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (silenced) return;
        if(button.IsInteractable())
        {
            emitterEvent.Play();
            emitterEvent.SetParameter(parameterName, mouseOverValue);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (silenced) return;
        if (button.IsInteractable())
        {
            emitterEvent.Play();
            emitterEvent.SetParameter(parameterName, mouseEnterValue);
        }
    }
}
