using System;
using System.Runtime.InteropServices;

namespace GanglionUnity.Internal
{
    /// <summary>
    /// Point of access for Data Processing functions
    /// </summary>
    public class DataProcessing
    {
        /// <summary>
        /// Computes FFT (IFFT) of the gived data.
        /// </summary>
        /// <param name="re">Real part of data (raw signal values or real part of FFT)</param>
        /// <param name="im">Imaginary part of data (0s for raw signal)</param>
        /// <param name="N">Size of the <paramref name="re"/> and <paramref name="im"/> arrays (should be LogN!)</param>
        /// <param name="LogN">Log2 of the array size (2 for 4, 3 for 8..)</param>
        /// <param name="isDirect">If true - computes FFT. If false - cmomputes inverse FFT</param>
        public static unsafe bool FFT(ref float[] re, ref float[] im, int N, int LogN, bool isDirect)
        {
            fixed (float* rePtr = re, imPtr = im)
            {
                return FFT(rePtr, imPtr, N, LogN, isDirect ? -1 : 1) == 1 ? true : false;
            }
        }

        public static float[] GetFilteredSpectrum(float[] re, float[] im, int sampleRate, int cutoffHzMin, int cutoffHzMax, int? spectrumLength = null)
        {
            float[] spectrum = new float[spectrumLength.HasValue ? spectrumLength.Value : re.Length / 2];
            float bucketWidth = (float)sampleRate / re.Length;
            int startIndex = (int)((cutoffHzMin + bucketWidth / 2) / bucketWidth);
            int endIndex = Math.Min((int)((cutoffHzMax + bucketWidth / 2) / bucketWidth), spectrum.Length - 1);
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
            float bucketWidth = (float)sampleRate / signal.Length;
            int startIndex = (int)((10 + bucketWidth / 2) / bucketWidth);
            int endIndex = Math.Min((int)((50 + bucketWidth / 2) / bucketWidth), signal.Length - 1);

            FFT(ref signal, ref im, signal.Length, logN, true);
            for (int i = 0; i <= startIndex; i++)
            {
                signal[i] = 0;
                im[i] = 0;
            }
            for (int i = endIndex; i < signal.Length; i++)
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
