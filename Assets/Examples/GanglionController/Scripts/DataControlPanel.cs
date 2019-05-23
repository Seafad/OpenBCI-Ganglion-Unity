using GanglionUnity.Components;
using GanglionUnity.Internal;
using UnityEngine;
using UnityEngine.UI;

namespace GanglionUnity.Examples
{

    /// <summary>
    /// Example of UI controlling Ganglion data with GanglionController
    /// </summary>
    public class DataControlPanel : MonoBehaviour
    {
        [SerializeField] private GanglionController controller;
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
            controller.OnConnected.AddListener(OnConnected);
            controller.OnDisconnected.AddListener(OnDisconnected);
            //controller.OnEEGReceived.AddListener(OnEEG);
            controller.OnAccelDataReceived.AddListener(OnAccelerometer);
            controller.OnImpedanceReceived.AddListener(OnImpedance);
        }

        private void OnEEG(EEGSample[] eegs)
        {
            foreach (var eeg in eegs)
            {
                Debug.Log("EEG Sample Received in UI: " + eeg.sampleNumber);
            }
        }

        private void OnAccelerometer(Vector3 acceleration)
        {
            Debug.Log("Acceleration Received in UI: " + acceleration.ToString());
        }

        private void OnImpedance(int channelNumber, int value)
        {
            Debug.Log("Impedance Received in UI: " + channelNumber + " - " + value);
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
            controller.StartDataStream();
            startStreamButton.interactable = false;
            stopStreamButton.interactable = true;
        }

        private void OnStopStreamClick()
        {
            controller.StopDataStream();
            startStreamButton.interactable = true;
            stopStreamButton.interactable = false;
        }

        private void OnChannel1ToggleChange(bool isOn)
        {
            controller.SetChannelActive(1, isOn);
        }

        private void OnChannel2ToggleChange(bool isOn)
        {
            controller.SetChannelActive(2, isOn);
        }

        private void OnChannel3ToggleChange(bool isOn)
        {
            controller.SetChannelActive(3, isOn);
        }

        private void OnChannel4ToggleChange(bool isOn)
        {
            controller.SetChannelActive(4, isOn);
        }

        private void OnAccelToggleChange(bool isOn)
        {
            controller.SetSendAccelerometerData(isOn);
        }

        private void OnImpedanceToggleChange(bool isOn)
        {
            controller.SetImpedanceMode(isOn);
        }

        private void OnSynthToggleChange(bool isOn)
        {
            controller.SetFakeSquareWaveMode(isOn);
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
