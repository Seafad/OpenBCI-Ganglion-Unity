using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace GanglionUnity.Internal
{
    /// <summary>
    /// Implementation of API with OpenBCI Ganglion Node.js server
    /// </summary>
    public class GanglionNodeAPI : GanglionAPI
    {
        private enum ResponseType : byte { StatusOk = 0, EEG = 1, Impedance = 2, GanglionInfo = 3, Message = 8, Error = 9 };

        private TcpClient client;
        private NetworkStream networkStream;
        private Timer searchTimer;
        private byte[] dataBuffer;
        private string[] EEGPacketsSplit = new string[] { "1|" };


        public GanglionNodeAPI(string host, int port)
        {
            try
            {
                client = new TcpClient();
                dataBuffer = new byte[client.ReceiveBufferSize];
                client.Connect(host, port);
                networkStream = client.GetStream();
                networkStream.BeginRead(dataBuffer, 0, client.ReceiveBufferSize, OnDataReceived, null);
            }
            catch (Exception e)
            {
                Debug.LogError(GetType().Name + ">>Connection error. Please set up and launch node.js server\n>>" + e.Message);
            }
        }

        ~GanglionNodeAPI()
        {
            client.Close();
            client.Dispose();
        }

        private void OnDataReceived(IAsyncResult ar)
        {
            networkStream.EndRead(ar);
            networkStream.Flush();

            var response = Encoding.ASCII.GetString(dataBuffer);
            ResponseType type = (ResponseType)(response[0] - '0');
            var json = response.Substring(2);

            switch (type)
            {
                case ResponseType.StatusOk:
                    OperationSuccessInvoke();
                    break;
                case ResponseType.GanglionInfo:
                    var g = JsonUtility.FromJson<GanglionInfo>(json);
                    GanglionFoundInvoke(g);
                    break;
                case ResponseType.EEG:
                    var eegs = json.Split(EEGPacketsSplit, StringSplitOptions.None);
                    EEGSample[] samples = new EEGSample[eegs.Length];
                    var i = 0;
                    foreach (var eegjson in eegs)
                    {
                        var sample = JsonUtility.FromJson<DataSample>(eegjson);
                        if (sample.accelData != null)
                            AccelDataReceivedInvoke(sample.accelData);
                        samples[i++] = new EEGSample(sample);
                    }
                    EEGReceivedInvoke(samples);

                    break;
                case ResponseType.Impedance:
                    var impedance = JsonUtility.FromJson<Impedance>(json);
                    ImpedanceReceivedInvoke(new int[] { impedance.channelNumber, impedance.impedanceValue });
                    break;
                case ResponseType.Message:
                    MessageInvoke(json);
                    break;
                case ResponseType.Error:
                    Debug.Log(GetType().Name + ">>Server error\n>>" + json);
                    break;
            }

            for (int i = 0; i < dataBuffer.Length; i++)
                dataBuffer[i] = 0;
            networkStream.BeginRead(dataBuffer, 0, client.ReceiveBufferSize, OnDataReceived, null);
        }

        public override void Connect(GanglionInfo info)
        {
            SendData("c" + info.name);
        }

        public override void Disconnect()
        {
            SendData('d');
        }

        public override void ReportRegisterSettings()
        {
            SendData('?');
        }

        public override void Search(int timeoutSecs)
        {
            try
            {
                SendData('i');
            }
            catch (Exception e)
            {
                Debug.LogError(GetType().Name + ">>Connection error. Please set up and launch node.js server. \n>>" + e.Message);
            }
            searchTimer = new Timer(OnSearchTimeEnded, null, timeoutSecs * 1000, 0);
        }

        private void OnSearchTimeEnded(object obj)
        {
            SendData('e');
            SearchEndedInvoke();
            searchTimer.Dispose();
        }

        public override void SetFakeSquareWaveMode(bool isFakeSquareWaveMode)
        {
            SendData(isFakeSquareWaveMode ? '[' : ']');
        }

        public override void SetImpedanceMode(bool isImpedanceMode)
        {
            SendData(isImpedanceMode ? 'z' : 'Z');
        }

        public override void SetSendAccelerometerData(bool turnOn)
        {
            SendData(turnOn ? 'n' : 'N');
        }

        public override void SoftReset()
        {
            SendData('v');
        }

        public override void StartDataStream()
        {
            SendData('b');
        }

        public override void StopDataStream()
        {
            SendData('s');
        }

        public override void StartSDCardLogging(SDCardLoggingMode loggingMode)
        {
            switch (loggingMode)
            {
                case SDCardLoggingMode.TEST:
                    SendData('a');
                    break;
                case SDCardLoggingMode.MIN5:
                    SendData('A');
                    break;
                case SDCardLoggingMode.MIN15:
                    SendData('S');
                    break;
                case SDCardLoggingMode.MIN30:
                    SendData('F');
                    break;
                case SDCardLoggingMode.H1:
                    SendData('G');
                    break;
                case SDCardLoggingMode.H2:
                    SendData('H');
                    break;
                case SDCardLoggingMode.H4:
                    SendData('J');
                    break;
                case SDCardLoggingMode.H12:
                    SendData('K');
                    break;
                case SDCardLoggingMode.H24:
                    SendData('L');
                    break;
            }
        }

        public override void StopSDCardLogging()
        {
            SendData('j');
        }

        public override void SetChannelActive(int channelNumber, bool turnOn)
        {
            switch (channelNumber)
            {
                case 1:
                    SendData(turnOn ? '!' : '1');
                    break;
                case 2:
                    SendData(turnOn ? '@' : '2');
                    break;
                case 3:
                    SendData(turnOn ? '#' : '3');
                    break;
                case 4:
                    SendData(turnOn ? '$' : '4');
                    break;
                default:
                    throw new ArgumentOutOfRangeException("channelNumber");
            }
        }

        private void SendData(char data)
        {
            NetworkStream stream = client.GetStream();
            var b = Convert.ToByte(data);
            stream.WriteByte(b);
            stream.Flush();
        }

        private void SendData(string data)
        {
            NetworkStream stream = client.GetStream();
            byte[] bytesToSend = Encoding.ASCII.GetBytes(data);
            stream.Write(bytesToSend, 0, bytesToSend.Length);
            stream.Flush();
        }

        public override void Dispose()
        {
            client.Dispose();
        }
    }
}

