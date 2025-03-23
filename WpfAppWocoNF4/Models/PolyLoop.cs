using System;
using System.Collections.Generic;
using System.Linq;

namespace WpfAppWoCoNF4
{

    public class PolyLoop
    {
        public IEnumerable<CartesianPoint> CartesianPoints { get; set; }

        public double CalculateVolume()
        {
            var pointX = CartesianPoints.Select(cp => cp.X).Distinct().ToList();
            var pointY = CartesianPoints.Select(cp => cp.Y).Distinct().ToList();
            var pointZ = CartesianPoints.Select(cp => cp.Z).Distinct().ToList();


            var diffX = Math.Abs(pointX.Max() - pointX.Min());
            var diffY = Math.Abs(pointY.Max() - pointY.Min());
            var diffZ = Math.Abs(pointZ.Max() - pointZ.Min());
            var volume = diffX * diffY * diffZ;
            return volume;
        }
    }
}