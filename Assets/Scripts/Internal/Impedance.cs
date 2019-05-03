namespace GanglionUnity.Internal
{
    /// <summary>
    /// Used to parse Impedance values from JSON
    /// </summary>
    [System.Serializable]
    public struct Impedance
    {
        public int channelNumber;
        public int impedanceValue;
    }
}