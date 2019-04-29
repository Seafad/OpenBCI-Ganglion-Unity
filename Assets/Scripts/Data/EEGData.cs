namespace GanglionUnity.Data
{
    [System.Serializable]
    public class EEGData
    {
        public int sampleNumber;
        public double[] channelData;

        public EEGData()
        {
            channelData = new double[4];
        }

        public static EEGData CreateFromJSON(string jsonString)
        {
            return UnityEngine.JsonUtility.FromJson<EEGData>(jsonString);
        }
    }
}