using GanglionUnity.Data;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;


namespace GanglionUnity.Internal
{
    /// <summary>
    /// API for node.js server with noble.
    /// </summary>
    public class GanglionNodeAPI : GanglionAPI
    {
        private TcpClient client;
        private NetworkStream networkStream;
        private Timer searchTimer;
        private byte[] dataBuffer;

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
                Debug.LogError(GetType().Name + ">>Connection error. Please set up and launch node.js server. \n>>" + e.Message);

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
            var json = Encoding.ASCII.GetString(dataBuffer);
            var response = NodeResponse.FromJSON(json);
            Debug.Log("Data received: " + json);
            Debug.Log("Response received: " + response.ToString());
            networkStream.BeginRead(dataBuffer, 0, client.ReceiveBufferSize, OnDataReceived, null);
        }

        public override void Connect(GanglionInfo info)
        {
            Debug.Log("Connecting to " + info.id);
            SendData("c" + info.id);
        }

        public override void Disconnect()
        {
            Debug.Log("Disconnecting");
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
            }catch(Exception e)
            {
                Debug.LogError(GetType().Name + ">>Connection error. Please set up and launch node.js server. \n>>" + e.Message);
            }
            searchTimer = new Timer(OnSearchTimeEnded, null, timeoutSecs * 1000, 0);
        }

        private void OnSearchTimeEnded(object obj)
        {
            SendData('e');
            GanglionController.Instance.OnSearchEnded.Invoke();
            searchTimer.Dispose();
        }

        public override void SetFakeSquareWaveMode(bool isFakeSquareWaveMode)
        {
            SendData('?');
        }

        public override void SetImpendanceMode(bool isImpendanceMode)
        {
            if (isImpendanceMode)
            {
                SendData('z');
            }
            else
            {
                SendData('Z');
            }
        }

        public override void SetSendAccelerometerData(bool isAccelerometerOn)
        {
            throw new NotImplementedException();
        }

        public override void SoftReset()
        {
            SendData('v');
        }

        public override void StartDataStream()
        {
            SendData('b');
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

        public override void StopDataStream()
        {
            SendData('s');
        }

        public override void StopSDCardLogging()
        {
            SendData('j');
        }

        public override void TurnOffChannel(int channelNumber)
        {
            if(channelNumber < 1 || channelNumber > 4)
            {
                throw new ArgumentOutOfRangeException("channelNumber");
            }
            switch (channelNumber)
            {
                case 1:
                    SendData('!');
                    break;
                case 2:
                    SendData('@');
                    break;
                case 3:
                    SendData('#');
                    break;
                case 4:
                    SendData('$');
                    break;
            }
        }

        public override void TurnOnChannel(int channelNumber)
        {
            if (channelNumber < 1 || channelNumber > 4)
            {
                throw new ArgumentOutOfRangeException("channelNumber");
            }
            switch (channelNumber)
            {
                case 1:
                    SendData('1');
                    break;
                case 2:
                    SendData('2');
                    break;
                case 3:
                    SendData('3');
                    break;
                case 4:
                    SendData('4');
                    break;
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
    }
}

