using System.Collections.Generic;
using System.Linq;

namespace WpfAppWoCoNF4
{
    public class CartesianPoint
    {
        public IEnumerable<Coordinate> Coordinates { get; set; }

        public double X => Coordinates.ToArray<Coordinate>()[0].Value;
        public double Y => Coordinates.ToArray<Coordinate>()[1].Value;
        public double Z => Coordinates.ToArray<Coordinate>()[2].Value;
    }
}