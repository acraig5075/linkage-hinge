using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;


namespace LinkageHinge
{
    using CustomExtensions;

    enum Corner : int { BottomLeft = 0, BottomRight, TopRight, TopLeft };

    partial class Box
    {
        public Hinge _hinge = new Hinge();
        private Point[] _topCorners = new Point[4];
        private Point[] _topCornersOpen = new Point[4];

        public double Width { private get; set; }
        public double BottomHeight { private get; set; }
        public double TopHeight { private get; set; }

        private double _axisAngle = 0.0;
        private Vector _offsetBL = new Vector();

        public void SetSize(Measurements m)
        {
            Width = m.BoxWidth;
            BottomHeight = m.BoxHeight1;
            TopHeight = m.BoxHeight2;

            _topCorners[(int)Corner.BottomLeft].X   = 0.0;
            _topCorners[(int)Corner.BottomRight].X  = Width;
            _topCorners[(int)Corner.TopRight].X     = Width;
            _topCorners[(int)Corner.TopLeft].X      = 0.0;
            _topCorners[(int)Corner.BottomLeft].Y   = 0.0;
            _topCorners[(int)Corner.BottomRight].Y  = 0.0;
            _topCorners[(int)Corner.TopRight].Y     = TopHeight;
            _topCorners[(int)Corner.TopLeft].Y      = TopHeight;

            var vector = new Vector
            {
                X = m.Linkage2TopX - m.Linkage1TopX,
                Y = m.Linkage2TopY - m.Linkage1TopY
            };
            var xAxis = new Vector
            {
                X = m.BoxWidth
            };

            _offsetBL.X = m.Linkage1TopX;
            _offsetBL.Y = m.Linkage1TopY - BottomHeight;

            _axisAngle = vector.DegreesTo(xAxis);

            _topCornersOpen = _topCorners;
        }

        public void AttachHinge(Box.Hinge hinge)
        {
            _hinge = hinge;
        }

        public bool OpenByAngle(int degrees)
        {
            bool succeeded = _hinge.OpenByAngle(degrees);

            if (succeeded)
                OpenTopBoxByAngle(degrees);

            return succeeded;
        }

        private void OpenTopBoxByAngle(double degrees)
        {
            var armature = _hinge.GetArmatureVector();
            var x = armature.RotateBy(-_axisAngle);
            var y = x.Perpendicular();

            var xAxis = x.SetLength(Width);
            var yAxis = y.SetLength(TopHeight);
            var xOffset = x.SetLength(_offsetBL.X);
            var yOffset = y.SetLength(_offsetBL.Y);

            var primary = _hinge.GetOpenPrimaryMovable();
            var bl = primary.Add(-xOffset).Add(-yOffset);
            bl.Y -= BottomHeight;

            _topCornersOpen[(int)Corner.BottomLeft] = bl;
            _topCornersOpen[(int)Corner.BottomRight] = bl.Add(xAxis);
            _topCornersOpen[(int)Corner.TopLeft] = bl.Add(yAxis);
            _topCornersOpen[(int)Corner.TopRight] = bl.Add(xAxis).Add(yAxis);
        }

        public Point[] GetTopBox()
        {
            return _topCornersOpen;
        }

    }
}
