namespace GanglionUnity.Internal
{
    /// <summary>
    /// Object representation of server response.
    /// </summary>
    [System.Serializable]
    public class NodeResponse
    {
        public byte type;
        public object data;

        public static NodeResponse FromJSON(string jsonString)
        {
            return UnityEngine.JsonUtility.FromJson<NodeResponse>(jsonString);
        }

        public override string ToString()
        {
            return "type: " + type;
        }
    }
}