namespace WpfAppWoCoNF4
{
    public class Space
    {
        public string Id { get; set; }
        public string ZoneIdRef { get; set; }
        public string Name { get; set; }
        public double Area { get; set; }
        public double Volume { get; set; }
        public ShellGeometry ShellGeometry { get; set; }
        public string CadObjectId { get; set; }
    }
}
