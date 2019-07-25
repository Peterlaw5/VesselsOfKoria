using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;
using UnityEngine.SceneManagement;
using FMODUnity;

namespace VoK
{
    public class UIOptions : MonoBehaviour
    {
        public Slider masterVolumeSlider;
        public TextMeshProUGUI masterVolumeText;
        public Slider mouseSensitivitySlider;
        public TextMeshProUGUI mouseSensitivityValue;
        public Slider mouseZoomSensitivitySlider;
        public TextMeshProUGUI mouseZoomSensitivityValue;
        public Toggle invertXAxisToggle;
        public Toggle invertYAxisToggle;
        bool invertHorAxis;
        bool invertVertAxis;
        public float volumeMasterOption;


        private void Start()
        {
            mouseSensitivitySlider.value = MenuManager.Instance.GetMouseSensitivityNormalized();
            mouseZoomSensitivitySlider.value = MenuManager.Instance.GetZoomMouseSensitivityNormalized();
            invertXAxisToggle.isOn = MenuManager.Instance.invertHorizontalAxis;
            invertYAxisToggle.isOn = MenuManager.Instance.invertVerticalAxis;
            masterVolumeSlider.value = MenuManager.Instance.volumeMaster;
            masterVolumeText.text = (masterVolumeSlider.value * 100f).ToString("F0") + "%";
        }

        public void OnChangeSensitivityMouse()
        {
            MenuManager.Instance.SetMouseSensitivity(mouseSensitivitySlider.value);
            mouseSensitivityValue.text = ((MenuManager.Instance.OptionMouseSensitivityPercent) * 100f).ToString("F0") + "%";
        }

        public void OnChangeSensitivityZoomMouse()
        {
            MenuManager.Instance.SetZoomMouseSensitivity(mouseZoomSensitivitySlider.value);
            mouseZoomSensitivityValue.text = ((MenuManager.Instance.OptionZoomMouseSensitivityPercent) * 100f).ToString("F0") + "%";
        }

        public void OnAxisXInvert()
        {
            MenuManager.Instance.invertHorizontalAxis = invertXAxisToggle.isOn;
        }

        public void OnAxisYInvert()
        {
            MenuManager.Instance.invertVerticalAxis = invertYAxisToggle.isOn;
        }

        public void OnChangeMasterVolume()
        {
            //masterVolumeSlider.value = volumeMasterOption;
            masterVolumeText.text = (masterVolumeSlider.value * 100f).ToString("F0") + "%";
            MenuManager.Instance.SetVolume(masterVolumeSlider.value);           
        }

    }
}

