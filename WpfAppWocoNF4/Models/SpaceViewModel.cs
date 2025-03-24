using System.Collections.Generic;
using WpfAppWoCoNF4;

namespace WpfAppWocoNF4.Models
{
    public class SpaceViewModel
    {
        public string Id { get; internal set; }
        public double Area { get; internal set; }
        public double Volume { get; internal set; }
        public string Name { get; internal set; }
        public List<CartesianPoint> CartesianPoints { get; set; }
        public double CalculatedArea { get; set; }
        public double CalculatedVolume { get; set; }
        public ClosedShell ClosedShell { get; set; }
    }
}
