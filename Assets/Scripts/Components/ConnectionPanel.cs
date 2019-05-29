using GanglionUnity.Internal;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace GanglionUnity.Components
{
    /// <summary>
    /// Example of UI managing Ganglion connection with GanglionManager
    /// </summary>
    public class ConnectionPanel : MonoBehaviour
    {
        [SerializeField] private GanglionManager ganglion;
        [SerializeField] private Button searchButton, connectButton, streamButton;
        [SerializeField] private Dropdown ganglionDropdown;
        [SerializeField] private Toggle channel1Toggle, channel2Toggle, channel3Toggle, channel4Toggle;
        [SerializeField] private Toggle accelToggle, impedanceToggle, synthToggle;
        private ImpedanceIndicator[] impedanceIndicators;

        private List<GanglionInfo> ganglions = new List<GanglionInfo>();
        private GanglionInfo? selectedGanglion;
        private Text searchButtonText, connectButtonText, streamButtonText;
        private bool isStreaming, isConnected;

        private void Awake()
        {
            searchButtonText = searchButton.GetComponentInChildren<Text>();
            connectButtonText = connectButton.GetComponentInChildren<Text>();
            streamButtonText = streamButton.GetComponentInChildren<Text>();
            ganglionDropdown.AddOptions(new List<string> { "Not selected" });
            GanglionNotSelected();
            UpdateUIState();
        }

        private void Start()
        {
            ganglion = GanglionManager.Instance;
            AddListeners();
            impedanceIndicators = GetComponentsInChildren<ImpedanceIndicator>();

        }

        private void AddListeners()
        {
            searchButton.onClick.AddListener(OnSearchClick);
            connectButton.onClick.AddListener(OnConnectClick);
            streamButton.onClick.AddListener(OnStreamClick);

            ganglion.OnGanglionFound.AddListener(OnFound);
            ganglion.OnSearchEnded.AddListener(OnSearchEnded);
            ganglion.OnConnected.AddListener(OnConnected);
            ganglion.OnDisconnected.AddListener(OnDisconnected);
            ganglionDropdown.onValueChanged.AddListener(OnGanglionDropdownValueChange);
            ganglion.OnImpedanceReceived.AddListener(OnImpedance);
           
            channel1Toggle.onValueChanged.AddListener(OnChannel1ToggleChange);
            channel2Toggle.onValueChanged.AddListener(OnChannel2ToggleChange);
            channel3Toggle.onValueChanged.AddListener(OnChannel3ToggleChange);
            channel4Toggle.onValueChanged.AddListener(OnChannel4ToggleChange);
            synthToggle.onValueChanged.AddListener(OnSynthToggleChange);
            impedanceToggle.onValueChanged.AddListener(OnImpedanceToggleChange);
            accelToggle.onValueChanged.AddListener(OnAccelToggleChange);
        }

        #region Listeners
        private void OnSearchClick()
        {
            if (ganglion.CurrentState == GanglionManager.State.NotConnected)
            {
                ganglion.Search();
                connectButton.interactable = false;
                searchButtonText.text = "Stop Search";
            }
            else if (ganglion.CurrentState == GanglionManager.State.Searching)
            {
                ganglion.StopSearch();
                searchButtonText.text = "Start Search";
            }
        }

        private void OnConnectClick()
        {
            if (!isConnected)
            {
                if (selectedGanglion.HasValue)
                    ganglion.Connect(selectedGanglion.Value);
            }
            else
            {
                ganglion.Disconnect();
            }
        }

        private void OnDisconnectClick()
        {
            ganglion.Disconnect();
        }

        private void OnGanglionDropdownValueChange(int index)
        {
            if (index < ganglions.Count)
                GanglionSelected(ganglions[index]);
            else
                GanglionNotSelected();
        }

        private void OnSearchEnded()
        {
            searchButtonText.text = "Start Search";
            if (selectedGanglion.HasValue)
                connectButton.interactable = true;
        }

        private void OnFound(GanglionInfo info)
        {
            ganglions.Add(info);
            List<string> list = ganglions.Select(x => x.name).ToList();
            list.Add("Not selected");
            ganglionDropdown.ClearOptions();
            ganglionDropdown.AddOptions(list);
            ganglionDropdown.value = list.Count - 1;
        }

        private void OnConnected()
        {
            connectButtonText.text = "Disconnect";
            isConnected = true;
            UpdateUIState();
        }

        private void OnDisconnected()
        {
            connectButtonText.text = "Connect";
            isConnected = false;
            UpdateUIState();
        }

        private void OnStreamClick()
        {
            if (!isStreaming)
            {
                ganglion.StartDataStream();
                streamButtonText.text = "Stop Stream";
                isStreaming = true;
            }
            else { 
                ganglion.StopDataStream();
                streamButtonText.text = "Start Stream";
                isStreaming = false;
            }  
        }

        private void OnChannel1ToggleChange(bool isOn)
        {
            ganglion.SetChannelActive(0, isOn);
        }

        private void OnChannel2ToggleChange(bool isOn)
        {
            ganglion.SetChannelActive(1, isOn);
        }

        private void OnChannel3ToggleChange(bool isOn)
        {
            ganglion.SetChannelActive(2, isOn);
        }

        private void OnChannel4ToggleChange(bool isOn)
        {
            ganglion.SetChannelActive(3, isOn);
        }

        private void OnAccelToggleChange(bool isOn)
        {
            ganglion.SetSendAccelerometerData(isOn);
        }

        private void OnImpedanceToggleChange(bool isOn)
        {
            ganglion.SetImpedanceMode(isOn);
            if (!isOn)
                foreach (var indicator in impedanceIndicators)
                    indicator.Disable();
        }

        private void OnSynthToggleChange(bool isOn)
        {
            ganglion.SetFakeSquareWaveMode(isOn);
        }

        private void OnImpedance(int index, int value)
        {
            impedanceIndicators[index].SetValue(value);
        }
        #endregion

        private void GanglionSelected(GanglionInfo ganglionInfo)
        {
            selectedGanglion = ganglionInfo;
            if (ganglion.CurrentState == GanglionManager.State.NotConnected)
            {
                connectButton.interactable = true;
            }
        }

        private void GanglionNotSelected()
        {
            selectedGanglion = null;
            connectButton.interactable = false;
        }

        private void UpdateUIState()
        {
            if (isConnected)
            {
                streamButton.interactable = true;
                channel1Toggle.interactable = true;
                channel2Toggle.interactable = true;
                channel3Toggle.interactable = true;
                channel4Toggle.interactable = true;
                channel1Toggle.isOn = true;
                channel2Toggle.isOn = true;
                channel3Toggle.isOn = true;
                channel4Toggle.isOn = true;
                accelToggle.interactable = true;
                impedanceToggle.interactable = true;
                synthToggle.interactable = true;
            }
            else
            {
                streamButton.interactable = false;
                streamButtonText.text = "Start Stream";
                channel1Toggle.interactable = false;
                channel2Toggle.interactable = false;
                channel3Toggle.interactable = false;
                channel4Toggle.interactable = false;
                accelToggle.interactable = false;
                impedanceToggle.interactable = false;
                synthToggle.interactable = false;
                accelToggle.isOn = false;
                impedanceToggle.isOn = false;
                synthToggle.isOn = false;
            }
        }
    }
}
