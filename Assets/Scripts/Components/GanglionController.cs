using GanglionUnity.Internal;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Enables communication with Ganglion board. Currently, the only option is to use API implemented with <see cref="https://github.com/OpenBCI/OpenBCI_NodeJS_Ganglion">node.js Ganglion library</see>.
/// </summary>
namespace GanglionUnity.Components
{
    public class GanglionController : MonoBehaviour, IGanglion
    {
        public enum ConnectionType { Node, NativeCppDriver }
        public enum State { NotConnected, Searching, Connected, StreamingData, ImpedanceTest }

        public static GanglionController Instance { get; private set; }
        public State CurrentState { get; private set; }

        public GanglionFoundEvent OnGanglionFound;
        public MessageEvent OnMessage;
        public EEGEvent OnEEGReceived;
        public AccelerationEvent OnAccelDataReceived;
        public ImpedanceEvent OnImpedanceReceived;
        public UnityEvent OnSearchEnded, OnConnected, OnDisconnected, OnStreamStart, OnStreamEnd;

        [SerializeField] private ConnectionType connectionType;
        [SerializeField] private string nodeHost = "127.0.0.1";
        [SerializeField] private int nodePort = 8080;
        [SerializeField] private bool showDebug = false;
        [SerializeField, Range(1, 30)] private int searchTimeout = 1;
        private bool isGanglionFoundFlag, isSearchEndedFlag, isMessageFlag, isOperationSuccessFlag, isEEGReceivedFlag, isAccelDataReceivedFlag, isImpedanceReceivedFlag;

        private GanglionInfo ganglionInfo;
        private string message;
        private List<EEGSample> eegBuffer = new List<EEGSample>();
        private Vector3 lastAccel = new Vector3();
        private int[] lastImpedance = new int[2];

        private GanglionAPI api;
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
                    throw new NotImplementedException("Native driver hasn't been implemented yet. Please use Node.js driver. More info at https://github.com/OpenBCI/OpenBCI_NodeJS_Ganglion");
            }
            api.GanglionFound += GanglionFoundNotify;
            api.SearchEnded += SearchEndedNotify;
            api.OperationSuccess += OnOperationSuccessNotify;
            api.Message += OnMessageNotify;
            api.EEGReceived += OnEEGReceivedNotify;
            api.AccelDataReceived += OnAccelDataReceivedNotify;
            api.ImpedanceReceived += OnImpedanceDataReceivedNotify;
            OnSearchEnded.AddListener(SearchEnded);
        }

        private void Update()
        {
            if (CurrentState == State.StreamingData)
            {
                if (isEEGReceivedFlag)
                {
                    OnEEGReceived.Invoke(eegBuffer.ToArray());
                    eegBuffer.Clear();
                    isEEGReceivedFlag = false;
                    return;
                }
                if (isAccelDataReceivedFlag)
                {
                    OnAccelDataReceived.Invoke(lastAccel);
                    isAccelDataReceivedFlag = false;
                    return;
                }
                if (isImpedanceReceivedFlag)
                {
                    OnImpedanceReceived.Invoke(lastImpedance[0], lastImpedance[1]);
                    isImpedanceReceivedFlag = false;
                    return;
                }
            }
            if (isGanglionFoundFlag)
            {
                OnGanglionFound.Invoke(ganglionInfo);
                isGanglionFoundFlag = false;
            }
            if (isSearchEndedFlag)
            {
                OnSearchEnded.Invoke();
                isSearchEndedFlag = false;
            }
            if (isOperationSuccessFlag)
            {
                switch (CurrentState)
                {
                    case State.NotConnected:
                        CurrentState = State.Connected;
                        OnConnected.Invoke();
                        break;
                    case State.Connected:
                    case State.StreamingData:
                    case State.ImpedanceTest:
                        CurrentState = State.NotConnected;
                        OnDisconnected.Invoke();
                        break;
                }
                isOperationSuccessFlag = false;
            }
            if (isMessageFlag)
            {
                if (showDebug)
                    Debug.Log(message);
                OnMessage.Invoke(message);
                isMessageFlag = false;
            }
        }

        public void Search()
        {
            if (CurrentState == State.NotConnected)
            {
                api.Search(searchTimeout);
                CurrentState = State.Searching;
            }
            else
            {
                OnMessage.Invoke("Search can be performed only when not connected to Ganglion");
            }
        }

        private void SearchEnded()
        {
            CurrentState = State.NotConnected;
        }

        private void GanglionFoundNotify(object sender, GanglionInfo info)
        {
            isGanglionFoundFlag = true;
            ganglionInfo = info;
        }

        private void SearchEndedNotify(object sender, EventArgs e)
        {
            isSearchEndedFlag = true;
        }

        private void OnOperationSuccessNotify(object sender, EventArgs e)
        {
            isOperationSuccessFlag = true;
        }

        private void OnMessageNotify(object sender, string message)
        {
            this.message = message;
            isMessageFlag = true;
        }

        private void OnEEGReceivedNotify(object sender, EEGSample[] eegs)
        {
            eegBuffer.AddRange(eegs);
            isEEGReceivedFlag = true;
        }
        private void OnAccelDataReceivedNotify(object sender, float[] accel)
        {
            lastAccel.x = accel[0];
            lastAccel.y = accel[1];
            lastAccel.z = accel[2];
            isAccelDataReceivedFlag = true;
        }

        private void OnImpedanceDataReceivedNotify(object sender, int[] impedanceData)
        {
            lastImpedance[0] = impedanceData[0];
            lastImpedance[1] = impedanceData[1];
            isImpedanceReceivedFlag = true;
        }

        #region Gangion Interface
        public void Search(int timeoutSecs)
        {
            searchTimeout = timeoutSecs;
            Search();
        }

        public void Connect(GanglionInfo info)
        {
            if (CurrentState == State.NotConnected || CurrentState == State.Searching)
            {
                api.Connect(info);
            }
            else
            {
                OnMessage.Invoke("Please, disconnect from Ganglion to initiate a new connection");
            }
        }

        public void Disconnect()
        {
            if (CurrentState != State.NotConnected && CurrentState != State.Searching)
            {
                api.Disconnect();
            }
            else
            {
                OnMessage.Invoke("Only connected Ganglion can be disconnected");
            }
        }

        public void StartDataStream()
        {
            if (CurrentState == State.Connected)
            {
                api.StartDataStream();
                OnStreamStart.Invoke();
                CurrentState = State.StreamingData;
            }
            else
            {
                OnMessage.Invoke("Ganglion should be connected to start data stream");
            }
        }

        public void StopDataStream()
        {
            if (CurrentState == State.StreamingData || CurrentState == State.ImpedanceTest)
            {
                api.StopDataStream();
                OnStreamEnd.Invoke();
                CurrentState = State.Connected;
            }
            else
            {
                OnMessage.Invoke("No active data stream to stop");
            }
        }

        public void SoftReset()
        {
            if (CurrentState != State.NotConnected && CurrentState != State.Searching)
            {
                api.SoftReset();
            }
            else
            {
                OnMessage.Invoke("Soft reset can only be performed on a connected device");
            }
        }

        public void SetChannelActive(int channelNumber, bool isOn)
        {
            if (CurrentState != State.NotConnected && CurrentState != State.Searching)
            {
                api.SetChannelActive(channelNumber, isOn);
            }
            else
            {
                OnMessage.Invoke("Ganglion should be connected for channels control");
            }
        }

        public void SetImpedanceMode(bool isImpedanceMode)
        {
            if (CurrentState == State.StreamingData || CurrentState == State.ImpedanceTest)
            {
                api.SetImpedanceMode(isImpedanceMode);
            }
            else
            {
                OnMessage.Invoke("Impedance test can only be initiated when streaming");
            }
        }

        public void SetFakeSquareWaveMode(bool isFakeSquareWaveMode)
        {
            if (CurrentState != State.NotConnected && CurrentState != State.Searching)
            {
                api.SetFakeSquareWaveMode(isFakeSquareWaveMode);
            }
            else
            {
                OnMessage.Invoke("Square wave mode can only be initiated on a connected device");
            }
        }

        public void SetSendAccelerometerData(bool isAccelerometerOn)
        {
            if (CurrentState != State.NotConnected && CurrentState != State.Searching)
            {
                api.SetSendAccelerometerData(isAccelerometerOn);
            }
            else
            {
                OnMessage.Invoke("Ganglion should be connected to control accelerometer");
            }
        }

        public void StartSDCardLogging(GanglionAPI.SDCardLoggingMode loggingMode)
        {
            throw new NotImplementedException("Scheduled for future releases");
        }

        public void StopSDCardLogging()
        {
            throw new NotImplementedException("Scheduled for future releases");
        }

        public void ReportRegisterSettings()
        {
            if (CurrentState != State.NotConnected && CurrentState != State.Searching)
            {
                api.ReportRegisterSettings();
            }
            else
            {
                OnMessage.Invoke("Ganglion should be connected to report registers data");
            }
        }
        #endregion

        private void OnDestroy()
        {
            api.Dispose();
        }
    }
    [Serializable] public class EEGEvent : UnityEvent<EEGSample[]> { };
    [Serializable] public class AccelerationEvent : UnityEvent<Vector3> { };
    [Serializable] public class ImpedanceEvent : UnityEvent<int, int> { };
    [Serializable] public class MessageEvent : UnityEvent<string> { };
    [Serializable] public class GanglionFoundEvent : UnityEvent<GanglionInfo> { };
}
