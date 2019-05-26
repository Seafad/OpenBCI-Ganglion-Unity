using GanglionUnity.Components;
using GanglionUnity.Internal;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace GanglionUnity.Examples
{
    /// <summary>
    /// Example of UI managing Ganglion connection with GanglionManager
    /// </summary>
    public class ConnectionPanel : MonoBehaviour
    {
        [SerializeField] private GanglionManager ganglion;
        [SerializeField] private Button searchButton, connectButton, disconnectButton;
        [SerializeField] private Dropdown ganglionDropdown;

        private List<GanglionInfo> ganglions = new List<GanglionInfo>();
        private GanglionInfo? selectedGanglion;
        private Text searchButtonText;

        private void Awake()
        {
            GanglionNotSelected();
            searchButtonText = searchButton.GetComponentInChildren<Text>();
            AddListeners();
            ganglionDropdown.AddOptions(new List<string> { "Not selected" });
        }

        private void AddListeners()
        {
            searchButton.onClick.AddListener(OnSearchClick);
            connectButton.onClick.AddListener(OnConnectClick);
            disconnectButton.onClick.AddListener(OnDisconnectClick);

            ganglion.OnGanglionFound.AddListener(OnFound);
            ganglion.OnSearchEnded.AddListener(OnSearchEnded);
            ganglion.OnConnected.AddListener(OnConnected);
            ganglion.OnDisconnected.AddListener(OnDisconnected);
            ganglionDropdown.onValueChanged.AddListener(OnGanglionDropdownValueChange);
        }

        #region Listeners
        private void OnSearchClick()
        {
            if (ganglion.CurrentState == GanglionManager.State.NotConnected)
            {
                ganglion.Search();
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
            if (selectedGanglion.HasValue)
                ganglion.Connect(selectedGanglion.Value);
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
            connectButton.interactable = false;
            disconnectButton.interactable = true;
        }

        private void OnDisconnected()
        {
            connectButton.interactable = true;
            disconnectButton.interactable = false;
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
            disconnectButton.interactable = false;
        }
    }
}
