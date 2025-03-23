using WpfAppWocoNF4.Models;

namespace WpfAppWoCoNF4
{
    public class ShellGeometry
    {
        public string Id { get; set; }
        public string Unit { get; set; }

        public AnalyticalShell AnalyticalShell { get; set; }
        public ClosedShell ClosedShell { get; set; }
    }
}