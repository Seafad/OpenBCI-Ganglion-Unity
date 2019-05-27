using GanglionUnity.Internal;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GanglionUnity.Components
{
    /// <summary>
    /// Continuously computes the spectrum of the input
    /// </summary>
    public class SpectrumComputer : MonoBehaviour
    {
        [SerializeField]
        private bool filter;
        [SerializeField]
        public SpectrumEvent OnNewSpectrumData;
        public float HzPerIndex { get; private set; }
        public int SpectrumSize { get; private set; }
        public static SpectrumComputer Instance { get; private set; }

        private int sampleRate = 200;
        private int FFTSize = 256, LOG_N = 8;
        private float[] re, im, spectrum;
        private OrderedChunkBuffer<float> waveBuffer;
        private int bufferChunkSize = 64, bufferChunks = 4;

        private int MCV_SCALE = 1000000;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else return;
            waveBuffer = new OrderedChunkBuffer<float>(bufferChunkSize, bufferChunks);
            HzPerIndex = (float)sampleRate / FFTSize;
            SpectrumSize = (int)((45 + HzPerIndex / 2) / HzPerIndex);
            re = new float[FFTSize];
            im = new float[FFTSize];
            spectrum = new float[SpectrumSize];
        }

        private void Start()
        {
            GanglionManager.Instance.OnEEGReceived.AddListener(OnEEGReceived);
        }

        public void OnData(float data)
        {
            waveBuffer.Write(data);
        }

        public void OnEEGReceived(List<EEGSample> eegs)
        {
            for (int i = 0; i < eegs.Count; i++)
            {
                waveBuffer.Write((float)eegs[i].channelData[0]);
                if (waveBuffer.IsFull)
                {
                    ProcessBufferedData();
                }
            }
        }

        private void ProcessBufferedData()
        {
            Array.Copy(waveBuffer.GetAllValues(), re, re.Length);
            Array.Clear(im, 0, im.Length);
            for (int i = 0; i < re.Length; i++)
                re[i] *= MCV_SCALE;
            DataProcessing.FFT(ref re, ref im, FFTSize, LOG_N, true);
            if (filter)
                spectrum = DataProcessing.GetFilteredSpectrum(re, im, sampleRate, 3, 45, SpectrumSize);
            else
                for (int i = 0; i < SpectrumSize; i++)
                    spectrum[i] = (Math.Abs(re[i]) + Math.Abs(im[i])) / re.Length;
            OnNewSpectrumData.Invoke(spectrum);
        }
    }
    [Serializable]
    public class SpectrumEvent : UnityEvent<float[]> { };
}