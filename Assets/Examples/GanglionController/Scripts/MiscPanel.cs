using GanglionUnity.Components;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace GanglionUnity.Examples
{
    /// <summary>
    /// Example of UI for accessing additional Ganglion commands with GanglionController
    /// </summary>
    public class MiscPanel : MonoBehaviour
    {
        [SerializeField] private GanglionController controller;
        [SerializeField] private Button softResetButton, reportRegistersButton;

        private LinkedList<string> lastMessages = new LinkedList<string>();
        private StringBuilder sb = new StringBuilder();

        void Start()
        {
            UpdateUIState(false);
            softResetButton.onClick.AddListener(OnSoftResetClick);
            reportRegistersButton.onClick.AddListener(OnReportRegistersClick);
            controller.OnConnected.AddListener(OnConnected);
            controller.OnDisconnected.AddListener(OnDisconnected);
        }

        private void UpdateUIState(bool isGanglionConnected)
        {
            if (isGanglionConnected)
            {
                softResetButton.interactable = true;
                reportRegistersButton.interactable = true;
            }
            else
            {
                softResetButton.interactable = false;
                reportRegistersButton.interactable = false;
            }
        }

        private void OnSoftResetClick()
        {
            controller.SoftReset();
        }

        private void OnReportRegistersClick()
        {
            controller.ReportRegisterSettings();
        }

        private void OnConnected()
        {
            UpdateUIState(true);
        }

        private void OnDisconnected()
        {
            UpdateUIState(false);
        }
    }
}
