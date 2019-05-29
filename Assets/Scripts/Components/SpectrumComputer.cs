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

        private GanglionManager ganglion;
        private int sampleRate = 200;
        private int FFTSize = 256, LOG_N = 8;
        private float[] re, im;
        private float[][] spectrums;
        private OrderedChunkBuffer<float>[] waveBuffers;
        private int bufferChunkSize = 64, bufferChunks = 4;

        private int MCV_SCALE = 1000000;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else return;
            waveBuffers = new OrderedChunkBuffer<float>[4];
            for (int i = 0; i < 4; i++)
                waveBuffers[i] = new OrderedChunkBuffer<float>(bufferChunkSize, bufferChunks);
            HzPerIndex = (float)sampleRate / FFTSize;
            SpectrumSize = (int)((45 + HzPerIndex / 2) / HzPerIndex);
            re = new float[FFTSize];
            im = new float[FFTSize];
            spectrums = new float[4][];
            for (int i = 0; i < 4; i++)
                spectrums[i] = new float[SpectrumSize];
        }

        private void Start()
        {
            ganglion = GanglionManager.Instance;
            ganglion.OnEEGReceived.AddListener(OnEEGReceived);
        }

        public void OnEEGReceived(List<EEGSample> eegs)
        {
            for (int i = 0; i < eegs.Count; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (ganglion.IsChannelActive(j))
                    {
                        waveBuffers[j].Write((float)eegs[i].channelData[j]);
                        if (waveBuffers[j].IsFull)
                        {
                            ProcessBufferedData(j);
                        }
                    }
                }
            }
        }

        private void ProcessBufferedData(int channelIndex)
        {
            Array.Copy(waveBuffers[channelIndex].GetAllValues(), re, re.Length);
            Array.Clear(im, 0, im.Length);
            for (int i = 0; i < re.Length; i++)
                re[i] *= MCV_SCALE;
            DataProcessing.FFT(ref re, ref im, FFTSize, LOG_N, true);
            if (filter)
                spectrums[channelIndex] = DataProcessing.GetFilteredSpectrum(re, im, sampleRate, 3, 45, SpectrumSize);
            else
                for (int i = 0; i < SpectrumSize; i++)
                    spectrums[channelIndex][i] = (Math.Abs(re[i]) + Math.Abs(im[i])) / re.Length;
            OnNewSpectrumData.Invoke(spectrums[channelIndex], channelIndex);
        }
    }
    [Serializable]
    public class SpectrumEvent : UnityEvent<float[], int> { };
}