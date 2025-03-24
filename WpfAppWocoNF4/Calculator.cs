using System;
using System.Collections.Generic;

namespace WpfAppWocoNF4
{
    public static class Calculator
    {
        public static double CalculateArea(List<(double X, double Y)> points)
        {
            int n = points.Count;
            if (n < 3)
            {
                throw new ArgumentException("At least 3 points are required to form a polygon.");
            }

            double sum = 0;
            for (int i = 0; i < n; i++)
            {
                int j = (i + 1) % n; // Next point, and wrap around for the last point
                sum += (points[i].X * points[j].Y) - (points[j].X * points[i].Y);
            }

            return Math.Abs(sum) / 2.0;
        }

        public static double CalculateVolume(List<(double X, double Y)> points, double height)
        {
            return height * CalculateArea(points);
        }
    }
}
