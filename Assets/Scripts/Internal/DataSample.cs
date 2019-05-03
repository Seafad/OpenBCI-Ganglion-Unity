namespace GanglionUnity.Internal
{
    /// <summary>
    /// Used to deserialize DataSample from JSON
    /// </summary>
    [System.Serializable]
    public struct DataSample
    {
        public int sampleNumber;
        public double[] channelData;
        public float[] accelData;
    }
}