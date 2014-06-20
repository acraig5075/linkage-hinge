using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace LinkageHinge
{
    using CustomExtensions;
    partial class Box
    {
        private class Linkage
        {
            public Point FixedPivot { get; set; }
            public Point MovablePivot { get; set; }

            public double GetRadius()
            {
                return FixedPivot.DistanceTo(MovablePivot);
            }

            public Point Rotate(int degrees)
            {
                return MovablePivot.RotateAbout(FixedPivot, degrees);
            }
        }
    }
}
