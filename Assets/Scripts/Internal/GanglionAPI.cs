using System.Collections.Generic;

namespace GanglionUnity.Internal
{
    /// <summary>
    /// Base interface class for Ganglion drivers. Based on <see cref="https://docs.openbci.com/OpenBCI%20Software/06-OpenBCI_Ganglion_SDK"> OpenBCI commands protocol overview</.
    /// </summary>
    public abstract class GanglionAPI
    {
        public enum SDCardLoggingMode : byte { TEST, MIN5, MIN15, MIN30, H1, H2, H4, H12, H24 }

        public abstract void Search(int timeoutSecs);
        public abstract void Connect(GanglionInfo info);
        public abstract void Disconnect();
        public abstract void StartDataStream();
        public abstract void StopDataStream();
        public abstract void SoftReset();
        public abstract void TurnOnChannel(int channelNumber);
        public abstract void TurnOffChannel(int channelNumber);
        public abstract void SetImpendanceMode(bool isImpendanceMode);
        public abstract void SetFakeSquareWaveMode(bool isFakeSquareWaveMode);
        public abstract void SetSendAccelerometerData(bool isAccelerometerOn);
        public abstract void StartSDCardLogging(SDCardLoggingMode loggingMode);
        public abstract void StopSDCardLogging();
        public abstract void ReportRegisterSettings();
    }
}