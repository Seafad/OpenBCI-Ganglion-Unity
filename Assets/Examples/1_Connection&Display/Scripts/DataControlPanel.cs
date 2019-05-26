using GanglionUnity.Components;
using GanglionUnity.Internal;
using UnityEngine;
using UnityEngine.UI;

namespace GanglionUnity.Examples
{

    /// <summary>
    /// Example of UI controlling Ganglion data with GanglionManager
    /// </summary>
    public class DataControlPanel : MonoBehaviour
    {
        [SerializeField] private GanglionManager ganglion;
        [SerializeField] private Button startStreamButton, stopStreamButton;
        [SerializeField] private Toggle channel1Toggle, channel2Toggle, channel3Toggle, channel4Toggle;
        [SerializeField] private Toggle accelToggle, impedanceToggle, synthToggle;

        private void Awake()
        {
            AddListeners();
            UpdateUIState(false);
        }

        private void AddListeners()
        {
            startStreamButton.onClick.AddListener(OnStartStreamClick);
            stopStreamButton.onClick.AddListener(OnStopStreamClick);
            channel1Toggle.onValueChanged.AddListener(OnChannel1ToggleChange);
            channel2Toggle.onValueChanged.AddListener(OnChannel2ToggleChange);
            channel3Toggle.onValueChanged.AddListener(OnChannel3ToggleChange);
            channel4Toggle.onValueChanged.AddListener(OnChannel4ToggleChange);
            synthToggle.onValueChanged.AddListener(OnSynthToggleChange);
            impedanceToggle.onValueChanged.AddListener(OnImpedanceToggleChange);
            accelToggle.onValueChanged.AddListener(OnAccelToggleChange);
            ganglion.OnConnected.AddListener(OnConnected);
            ganglion.OnDisconnected.AddListener(OnDisconnected);
        }

        private void UpdateUIState(bool isGanglionConnected)
        {
            if (isGanglionConnected)
            {
                startStreamButton.interactable = true;
                stopStreamButton.interactable = true;
                channel1Toggle.interactable = true;
                channel2Toggle.interactable = true;
                channel3Toggle.interactable = true;
                channel4Toggle.interactable = true;
                channel1Toggle.isOn = true;
                channel2Toggle.isOn = true;
                channel3Toggle.isOn = true;
                channel4Toggle.isOn = true;
            }
            else
            {
                startStreamButton.interactable = false;
                stopStreamButton.interactable = false;
                channel1Toggle.interactable = false;
                channel2Toggle.interactable = false;
                channel3Toggle.interactable = false;
                channel4Toggle.interactable = false;
            }
        }

        #region Listeners
        private void OnStartStreamClick()
        {
            ganglion.StartDataStream();
            startStreamButton.interactable = false;
            stopStreamButton.interactable = true;
        }

        private void OnStopStreamClick()
        {
            ganglion.StopDataStream();
            startStreamButton.interactable = true;
            stopStreamButton.interactable = false;
        }

        private void OnChannel1ToggleChange(bool isOn)
        {
            ganglion.SetChannelActive(1, isOn);
        }

        private void OnChannel2ToggleChange(bool isOn)
        {
            ganglion.SetChannelActive(2, isOn);
        }

        private void OnChannel3ToggleChange(bool isOn)
        {
            ganglion.SetChannelActive(3, isOn);
        }

        private void OnChannel4ToggleChange(bool isOn)
        {
            ganglion.SetChannelActive(4, isOn);
        }

        private void OnAccelToggleChange(bool isOn)
        {
            ganglion.SetSendAccelerometerData(isOn);
        }

        private void OnImpedanceToggleChange(bool isOn)
        {
            ganglion.SetImpedanceMode(isOn);
        }

        private void OnSynthToggleChange(bool isOn)
        {
            ganglion.SetFakeSquareWaveMode(isOn);
        }

        private void OnConnected()
        {
            UpdateUIState(true);
        }

        private void OnDisconnected()
        {
            UpdateUIState(false);
        }
        #endregion
    }
}
