using GanglionUnity.Internal;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GanglionUnity.Components
{
    public class SpectrumChart : MonoBehaviour
    {
        [SerializeField]
        private bool filtered;

        private SpectrumChartDisplay chartView;
        private int sampleRate = 200;
        private int FFTSize = 256, LOG_N = 8;
        private float[] waveBuffer, re, im, spectrum;
        private int bufferChunkSize = 64, bufferChunksCount = 4;
        private int bufferWriteIndex;
        private int SpectrumSize;
        private float hzPerIndex;

        private int MCV_SCALE = 1000000;

        private void Awake()
        {
            SpectrumSize = FFTSize / 2;
            hzPerIndex = (float)sampleRate / FFTSize;
            waveBuffer = new float[bufferChunkSize * bufferChunksCount];
            bufferWriteIndex = bufferChunkSize * (bufferChunksCount - 1) - 1;
            re = new float[FFTSize];
            im = new float[FFTSize];
            spectrum = new float[SpectrumSize];
        }

        private void Start()
        {
            chartView = GetComponentInChildren<SpectrumChartDisplay>();
            chartView.InitDisplay(SpectrumSize, hzPerIndex, 10);
        }

        public void OnEEGReceived(List<EEGSample> eegs)
        {
            for (int i = 0; i < eegs.Count; i++)
            {
                waveBuffer[bufferWriteIndex++] = (float)eegs[i].channelData[0];
                if (bufferWriteIndex == waveBuffer.Length)
                    FlushBuffer();
            }
        }

        private void FlushBuffer()
        {
            bufferWriteIndex = bufferChunkSize * (bufferChunksCount - 1) - 1;
            Array.Copy(waveBuffer, re, waveBuffer.Length);
            Array.Clear(im, 0, im.Length);
            for (int i = 0; i < re.Length; i++)
                re[i] *= MCV_SCALE;
            DataProcessing.FFT(ref re, ref im, FFTSize, LOG_N, true);
            if (filtered)
                spectrum = DataProcessing.GetFilteredSpectrum(re, im, sampleRate, 8, 49);
            else
                for (int i = 0; i < re.Length; i++)
                    spectrum[i] = re[i];
            ShiftBufferToLeft();
            chartView.DisplayValues(spectrum);
        }

        private void ShiftBufferToLeft()
        {
            for (int i = 0; i < bufferChunkSize * (bufferChunksCount - 1) - 1; i++)
                waveBuffer[i] = waveBuffer[i + bufferChunkSize];
            for (int i = bufferChunkSize * (bufferChunksCount - 1); i < waveBuffer.Length; i++)
                waveBuffer[i] = 0;
        }

        /*Shift left
                        for (int j = 0; j < 3 * waveBuffer.Length / 4; j++)
                        {
                            waveBuffer[j] = waveBuffer[j + waveBuffer.Length / 4];
                        }
                        for(int j = 3 * waveBuffer.Length / 4; j < waveBuffer.Length; j++)
                        {
                            waveBuffer[j] = 0;
                        }*/
    }
}
