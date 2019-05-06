namespace GanglionUnity.Internal
{
    public interface IGanglion
    {
        void Search(int timeoutSecs);
        void Connect(GanglionInfo info);
        void Disconnect();
        void StartDataStream();
        void StopDataStream();
        void SoftReset();
        void SetChannelActive(int channelNumber, bool turnOn);
        void SetImpedanceMode(bool isImpedanceMode);
        void SetFakeSquareWaveMode(bool isFakeSquareWaveMode);
        void SetSendAccelerometerData(bool turnOn);
        void StartSDCardLogging(GanglionAPI.SDCardLoggingMode loggingMode);
        void StopSDCardLogging();
        void ReportRegisterSettings();
    }
}