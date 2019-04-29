using GanglionUnity.Data;
using GanglionUnity.Internal;
using UnityEngine;
using UnityEngine.UI;

namespace GanglionUnity.Examples
{
    /// <summary>
    /// Example UI for Ganglion connection management using GanglionController.
    /// </summary>
    public class ConnectionPanel : MonoBehaviour
    {

        [SerializeField] private GanglionController controller;
        [SerializeField] private Button SearchButton, ConnectButton, DisconnectButton, StartStreamButton, StopStreamButton;

        private void Awake()
        {
            ConnectButton.interactable = false;
            DisconnectButton.interactable = false;
            StartStreamButton.interactable = false;
            StopStreamButton.interactable = false;
        }

        private void Start()
        {
            SearchButton.onClick.AddListener(OnSearchClick);
            GanglionController.Instance.OnGanglionFound.AddListener(OnFound);
        }

        private void OnSearchClick()
        {
            controller.Search();
        }

        private void OnFound(GanglionInfo info)
        {
            Debug.Log("Info came: " + info.name);
        }
    }
}
