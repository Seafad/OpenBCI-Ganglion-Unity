using GanglionUnity.Internal;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Enables communication with Ganglion board. Currently, the only option is to use a <see cref="https://github.com/OpenBCI/OpenBCI_NodeJS_Ganglion">node.js Ganglion library</see>.
/// </summary>
namespace GanglionUnity.Data
{
    public class GanglionController : MonoBehaviour
    {
        public enum ConnectionType { Node, NativeCppDriver }
        public enum State { NotConnected, Searching, Connected, EEG, Accel, Impedance, SynthWave }

        public static GanglionController Instance;
        public State CurrentState { get; private set; }
        public GanglionFoundEvent OnGanglionFound;
        public UnityEvent OnSearchEnded;
        
        [SerializeField] private ConnectionType connectionType;
        [SerializeField] private string nodeHost = "127.0.0.1";
        [SerializeField] private int nodePort = 8080;
        [SerializeField, Range(1, 30)] private int searchTimeout = 1;

        private GanglionAPI api;
        private bool isConnected;
        private bool isStreaming;
        private bool[] activeChannels = new bool[4];

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else return;

            switch (connectionType)
            {
                case ConnectionType.Node:
                    api = new GanglionNodeAPI(nodeHost, nodePort);
                    break;
                case ConnectionType.NativeCppDriver:
                    throw new System.NotImplementedException("Native driver hasn't been implemented yet. Please use Node.js driver. More info at https://github.com/OpenBCI/OpenBCI_NodeJS_Ganglion");
            }
            OnSearchEnded.AddListener(SearchEnded);
        }

        public void Search()
        {
            if (CurrentState == State.NotConnected)
            {
                api.Search(searchTimeout);
                CurrentState = State.Searching;
            }
        }

        private void SearchEnded()
        {
            CurrentState = State.NotConnected;
        }

        public void Connect(GanglionInfo info)
        {
            if (CurrentState == State.NotConnected || CurrentState == State.Searching)
            {
                api.Connect(info);
            }
        }

        public void Disconnect()
        {
            if(CurrentState != State.NotConnected && CurrentState != State.Searching)
                api.Disconnect();
        }

    }

    [System.Serializable] public class GanglionFoundEvent : UnityEvent<GanglionInfo> { };
}
