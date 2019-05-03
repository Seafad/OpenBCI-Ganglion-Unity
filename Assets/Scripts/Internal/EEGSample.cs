namespace GanglionUnity.Internal
{
    [System.Serializable]
    public struct EEGSample
    {
        public int sampleNumber;
        public double[] channelData;

        public EEGSample(DataSample sample) : this()
        {
            sampleNumber = sample.sampleNumber;
            channelData = sample.channelData;
        }
    }
}
