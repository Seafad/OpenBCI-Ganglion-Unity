using System.Runtime.InteropServices;

namespace GanglionUnity.Internal
{
    public class DataProcessing
    {

        public unsafe static bool FFT(ref float[] Re, ref float[] Im, int N, int LogN, bool isDirect)
        {
            fixed (float* rePtr = Re, imPtr = Im)
            {
                    return FFT(rePtr, imPtr, N, LogN, isDirect ? -1 : 1) == 1 ? true : false;
            }  
        }

        public static float[] GetSpectrum(float[] re, float[] im, int cutoffIndex)
        {
            var res = new float[cutoffIndex];
            for (int i = 0; i < cutoffIndex; i++)
                res[i] = re[i] * re[i] + im[i] * im[i];
            return res;
        }

        [DllImport("DataProcessing")]
        private static unsafe extern int FFT(float* Rdat, float* Idat, int N, int LogN, int Ft_Flag);
    }
}
