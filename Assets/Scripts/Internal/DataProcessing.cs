using System;
using System.Runtime.InteropServices;

namespace GanglionUnity.Internal
{
    public class DataProcessing
    {

        public static unsafe bool FFT(ref float[] re, ref float[] im, int N, int LogN, bool isDirect)
        {
            fixed (float* rePtr = re, imPtr = im)
            {
                return FFT(rePtr, imPtr, N, LogN, isDirect ? -1 : 1) == 1 ? true : false;
            }
        }

        public static float[] GetFilteredSpectrum(float[] re, float[] im, int sampleRate, int cutoffHzMin, int cutoffHzMax)
        {
            float[] spectrum = new float[re.Length / 2];
            float bucketWidth = (float)sampleRate / re.Length;
            int startIndex = (int)((cutoffHzMin + bucketWidth / 2) / bucketWidth);
            int endIndex = (int)((cutoffHzMax + bucketWidth / 2) / bucketWidth);
            for (int i = startIndex; i <= endIndex; i++)
            {
                spectrum[i] = (Math.Abs(re[i]) + Math.Abs(im[i])) / re.Length;
            }
            return spectrum;
        }

        public static float[] GetSpectrum(float[] re, float[] im, int cutoffIndex)
        {
            var res = new float[cutoffIndex];
            for (int i = 0; i < cutoffIndex; i++)
                res[i] = re[i] * re[i] + im[i] * im[i];
            return res;
        }

        public static float[] FilterFreqsAbove50Hz(ref float[] signal, int sampleRate, int logN)
        {
            float[] im = new float[signal.Length];
            int Hz50Index = (int)(50 / ((float)sampleRate / signal.Length)) + 1;

            FFT(ref signal, ref im, signal.Length, logN, true);
            for (int i = Hz50Index; i < signal.Length; i++)
            {
                signal[i] = 0;
                im[i] = 0;
            }
            FFT(ref signal, ref im, signal.Length, logN, false);
            return signal;
        }

        [DllImport("DataProcessing")]
        private static extern unsafe int FFT(float* Rdat, float* Idat, int N, int LogN, int Ft_Flag);
    }
}
