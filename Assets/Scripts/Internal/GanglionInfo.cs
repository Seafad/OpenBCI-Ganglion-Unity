namespace GanglionUnity.Internal
{
    /// <summary>
    /// Stores essential attributes of noble device data.
    /// </summary>
    [System.Serializable]
    public class GanglionInfo
    {
        public string id;
        public string name;
        public string state;
        public bool connectable;
    }
}