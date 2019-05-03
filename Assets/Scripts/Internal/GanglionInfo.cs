namespace GanglionUnity.Internal
{
    /// <summary>
    /// Stores essential attributes of Ganglion device data
    /// </summary>
    [System.Serializable]
    public struct GanglionInfo
    {
        public GanglionInfo(string id, string name, string state, bool connectable)
        {
            this.id = id;
            this.name = name;
            this.state = state;
            this.connectable = connectable;
        }

        public string id;
        public string name;
        public string state;
        public bool connectable;
    }
}