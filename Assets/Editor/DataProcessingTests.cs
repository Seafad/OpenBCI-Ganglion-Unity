using GanglionUnity.Internal;
using NUnit.Framework;
using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine.TestTools;
using Debug = UnityEngine.Debug;

public class DataProcessingTests
{
    Stopwatch stopWatch = new Stopwatch();
 
   [Test]
   public void IsSpectrum10And28Valid()
   {
        int FFTSize = 256;
        int pow2 = 8;
        int sampleRate = 200;
        float bucketWidth = (float) sampleRate / FFTSize;

        float[] wave10 = GenerateFakeWave(sampleRate, 10, FFTSize);
        float[] wave28 = GenerateFakeWave(sampleRate, 28, FFTSize);
        float[] power;

        float[] re = new float[FFTSize];
        float[] im = new float[FFTSize];
        for (int i = 0; i < FFTSize; i++)
        {
            re[i] = (wave10[i] + wave28[i])/2;
            im[i] = 0;
        }
        
        DataProcessing.FFT(ref re, ref im, FFTSize, pow2, true);
        power = DataProcessing.GetSpectrum(re, im, re.Length / 2);
        for(int i = 0; i < power.Length; i++)
        {
            if(power[i] > 3000)
                Debug.Log(i + "| " + (i * bucketWidth - bucketWidth / 2) + ".." + (i * bucketWidth + bucketWidth / 2) + "Hz: " + power[i]);
        }
        int hz10 = (int)(10 / bucketWidth + bucketWidth / 2);
        int hz28 = (int)(28 / bucketWidth + bucketWidth / 2);
        Assert.Greater(power[hz10], 3000);
        Assert.Greater(power[hz28], 3000);
    }

    [Test]
    public void CompareFFTSpeed()
    {
        CompareFFTSpeedFor(128, 7);
        CompareFFTSpeedFor(256, 8);
        CompareFFTSpeedFor(512, 9);
    }

    private void CompareFFTSpeedFor(int N, int pow2)
    {
        float time1;
        int NTests = 150;

        int FFTSize = N;
        int sampleRate = 200;
        float[] re = new float[FFTSize];
        float[] im = new float[FFTSize];
        float[] wave28 = GenerateFakeWave(sampleRate, 28, FFTSize);
        
        Complex[] wave = new Complex[FFTSize];
        for(int i = 0; i < wave.Length; i++)
        {
            wave[i] = new Complex(wave28[i]);
        }

        stopWatch.Restart();
        for (int i = 0; i < NTests; i++)
        {
            var freqComplex = FFT.fft(wave);
        }
        time1 = (float)stopWatch.ElapsedMilliseconds / NTests;

        
        for (int i = 0; i < FFTSize; i++)
        {
            re[i] = wave28[i];
            im[i] = 0;
        }

        stopWatch.Restart();
        DataProcessing.FFT(ref re, ref im, FFTSize, pow2, true);
        for (int i = 0; i < NTests - 1; i++)
        {
            stopWatch.Stop();
            for (int j = 0; j < FFTSize; j++)
            {
                re[j] = wave28[j];
                im[j] = 0;
            }
            stopWatch.Start();
            DataProcessing.FFT(ref re, ref im, FFTSize, 8, true);
        }
        Debug.Log("FFT Speed test (N = " + FFTSize + "):  C# = " + time1 + "ms; C = " + (float)stopWatch.ElapsedMilliseconds / NTests + "ms");
        stopWatch.Stop();
    }

    private static float[] GenerateFakeWave(int samplingFrequency, float waveFreq, int samples)
    {
        float[] wave = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            wave[i] = (float)Math.Cos(2 * Math.PI * waveFreq * i / samplingFrequency);
        }
        return wave;
    }
}
