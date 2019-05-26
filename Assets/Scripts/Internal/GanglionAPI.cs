using System;

namespace GanglionUnity.Internal
{
    /// <summary>
    /// Base interface class for Ganglion drivers. Based on <see cref="https://docs.openbci.com/OpenBCI%20Software/06-OpenBCI_Ganglion_SDK"> OpenBCI commands protocol overview</.
    /// </summary>
    public abstract class GanglionAPI : IGanglion, IDisposable
    {
        public enum SDCardLoggingMode : byte { TEST, MIN5, MIN15, MIN30, H1, H2, H4, H12, H24 }

        public event EventHandler<GanglionInfo> GanglionFound;
        public event EventHandler SearchEnded, OperationSuccess;
        public event EventHandler<EEGSample[]> EEGReceived;
        public event EventHandler<float[]> AccelDataReceived;
        public event EventHandler<int[]> ImpedanceReceived;
        public event EventHandler<string> Message;

        public abstract void Search(int timeoutSecs);
        public abstract void StopSearch();
        public abstract void Connect(GanglionInfo info);
        public abstract void Disconnect();
        public abstract void StartDataStream();
        public abstract void StopDataStream();
        public abstract void SoftReset();
        public abstract void SetChannelActive(int channelNumber, bool turnOn);
        public abstract void SetImpedanceMode(bool isImpedanceMode);
        public abstract void SetFakeSquareWaveMode(bool isFakeSquareWaveMode);
        public abstract void SetSendAccelerometerData(bool turnOn);
        public abstract void StartSDCardLogging(SDCardLoggingMode loggingMode);
        public abstract void StopSDCardLogging();
        public abstract void ReportRegisterSettings();
        public abstract void Dispose();

        /// <summary>
        /// Should be called by API when Ganlgion device is found
        /// </summary>
        /// <param name="info">Information about Ganglion device to be passed to the client</param>
        protected void GanglionFoundInvoke(GanglionInfo info)
        {
            GanglionFound?.Invoke(this, info);
        }
        /// <summary>
        /// Should be called by API when Ganglion search is completed
        /// </summary>
        protected void SearchEndedInvoke()
        {
            SearchEnded?.Invoke(this, null);
        }

        /// <summary>
        /// Should be called by API when operation successfully completed by Ganglion
        /// </summary>
        protected void OperationSuccessInvoke()
        {
            OperationSuccess?.Invoke(this, null);
        }

        /// <summary>
        /// Should be called by API when message is sent from the driver
        /// </summary>
        protected void MessageInvoke(string message)
        {
            Message?.Invoke(this, message);
        }

        /// <summary>
        /// Should be called by API when EEG is received by the driver
        /// </summary>
        protected void EEGReceivedInvoke(EEGSample[] dataSamples)
        {
            EEGReceived?.Invoke(this, dataSamples);
        }

        /// <summary>
        /// Should be called by API when accelerometer data is received by the driver
        /// </summary>
        protected void AccelDataReceivedInvoke(float[] accelData)
        {
            AccelDataReceived?.Invoke(this, accelData);
        }

        /// <summary>
        /// Should be called by API when impedance data is received by the driver
        /// </summary>
        protected void ImpedanceReceivedInvoke(int[] impedanceData)
        {
            ImpedanceReceived?.Invoke(this, impedanceData);
        }
    }
}