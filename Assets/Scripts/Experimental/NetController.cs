using System;
using System.Net.Sockets;
using System.Text;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using GanglionUnity.Data;
using GanglionUnity.Internal;

namespace GanglionUnity.Experimental
{
    public class NetController : MonoBehaviour
    {

        public string HOST = "127.0.0.1";
        public int PORT = 8080;
        public Text[] channelsUI;


        private TcpClient client;
        private bool isBoardActive;
        private EEGData currValue;

        //Подсоединение к node.js при запуске
        void Start()
        {
            GanglionAPI api;
            GanglionController c = new GanglionController();
            currValue = new EEGData();
            if (client != null)
            {
                Debug.LogError("Connection is already open");
            }
            else
            {
                try
                {
                    client = new TcpClient();
                    client.Connect(HOST, PORT);
                }
                catch (Exception e)
                {
                    Debug.Log("Connection error: " + e.StackTrace);
                }
            }
        }

        //Основной цикл: Получение данных при наличии.
        void Update()
        {
            if (isBoardActive)
            {
                if (client.Available > 0)
                {
                    EEGData[] data = Unpack();
                    UpdateCurrentDataFromGanglion(data);
                }
                UpdateUI();
            }

        }

        //Распаковка данных из JSON
        private EEGData[] Unpack()
        {
            NetworkStream stream = client.GetStream();
            byte[] bytesToRead = new byte[client.ReceiveBufferSize];
            stream.Read(bytesToRead, 0, client.ReceiveBufferSize);

            string input = Encoding.ASCII.GetString(bytesToRead, 0, bytesToRead.Length);
            var json = input.Split('#');
            return json.Skip(1).Select(x => EEGData.CreateFromJSON(x)).ToArray();
        }


        public void StartBoard()
        {
            isBoardActive = true;
            SendData("b");
        }

        public void StopBoard()
        {
            isBoardActive = false;
            SendData("s");
        }

        private void SendData(string data)
        {
            NetworkStream stream = client.GetStream();
            byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(data);
            stream.Write(bytesToSend, 0, bytesToSend.Length);
        }

        //Обновляем состояние последним пришедшим значением 
        private void UpdateCurrentDataFromGanglion(EEGData[] data)
        {
            currValue.channelData = data[data.Length - 1].channelData;
        }

        private void UpdateUI()
        {
            for (int i = 0; i < 4; i++)
                channelsUI[i].text = currValue.channelData[i].ToString();
        }

        //Отсоединение от node.js при завершении работы приложения
        void OnApplicationQuit()
        {
            if (client == null || !client.Connected)
            {
                Debug.LogError("Connection is already closed");
            }
            else
            {
                if (client.Available > 0)
                    Unpack();
                client.Close();
            }
        }
    }
}

/*
    private void TestRecieve()
    {
        string input = "12#{}13#{sampleNumber: 3}";
        string pattern = @"\d+#";            // Split on hyphens
        string[] json = Regex.Split(input, pattern);
        string[] json_2 = json.Where(x => !x.Equals(string.Empty)).ToArray();
        string[] json_3 = json.Skip(1).ToArray();
        Debug.Log(string.Format("json= [{0}]", string.Join("', '", json)));
        Debug.Log(string.Format("json_2= [{0}]", string.Join(", ", json_2)));
        Debug.Log(string.Format("json_3= [{0}]", string.Join(", ", json_3)));

        json.Where(x => !x.Equals(string.Empty)).Select(x => EEGData.CreateFromJSON(x));
    }
*/