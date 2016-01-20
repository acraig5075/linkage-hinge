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
        public class Hinge
        {
            private Linkage _primary = new Linkage();
            private Linkage _secondary = new Linkage();
            private Linkage _primaryOpen = new Linkage();
            private Linkage _secondaryOpen = new Linkage();
            private double _armature;

            public void SetSize(Measurements m)
            {
                double a = m.Linkage1TopX;
                double b = m.Linkage1TopY;
                double c = m.Linkage1BottomX;
                double d = m.Linkage1BottomY;
                double e = m.Linkage2TopX;
                double f = m.Linkage2TopY;
                double g = m.Linkage2BottomX;
                double h = m.Linkage2BottomY;

                SetPrimaryFixed(    new Point(c,  d));
                SetPrimaryMovable(  new Point(a, -b));
                SetSecondaryFixed(  new Point(g,  h));
                SetSecondaryMovable(new Point(e, -f));
            }

            public void SetPrimaryFixed(Point p)
            {
                _primary.FixedPivot = p;
                Update();
            }

            public void SetPrimaryMovable(Point p)
            {
                _primary.MovablePivot = p;
                Update();
            }

            public void SetSecondaryFixed(Point p)
            {
                _secondary.FixedPivot = p;
                Update();
            }

            public void SetSecondaryMovable(Point p)
            {
                _secondary.MovablePivot = p;
                Update();
            }

            public Point GetPrimaryFixed()
            {
                return _primary.FixedPivot;
            }

            public Point GetSecondaryFixed()
            {
                return _secondary.FixedPivot;
            }

            private Point GetPrimaryMovable()
            {
                return _primary.MovablePivot;
            }

            private Point GetSecondaryMovable()
            {
                return _secondary.MovablePivot;
            }

            public Point GetOpenPrimaryMovable()
            {
                return _primaryOpen.MovablePivot;
            }

            public Point GetOpenSecondaryMovable()
            {
                return _secondaryOpen.MovablePivot;
            }

            private void Update()
            {
                _armature = GetPrimaryMovable().DistanceTo(GetSecondaryMovable());
                _primaryOpen = _primary;
                _secondaryOpen = _secondary;
            }

            public bool OpenByAngle(int degrees)
            {
                _primaryOpen = _primary;
                _secondaryOpen = _secondary;

                try
                {
                    Point source = _secondaryOpen.Rotate(degrees);
                    Point dest = MoveArmatureTo(source);

                    _primaryOpen.MovablePivot = dest;
                    _secondaryOpen.MovablePivot = source;

                    return true;
                }
                catch (ArithmeticException)
                {
                    // no intersection
                    return false;
                }
            }

            private Point MoveArmatureTo(Point start)
            {
                Point fixedPivot = _primary.FixedPivot;
                double r = _primary.GetRadius();

                var circle1 = new Circle(start.X, start.Y, _armature);
                var circle2 = new Circle(fixedPivot.X, fixedPivot.Y, r);

                Tuple<Circle.CirclePoint, Circle.CirclePoint> itx = circle1.intersections(circle2);
                var itx1 = new Point(itx.Item1.x, itx.Item1.y);
                var itx2 = new Point(itx.Item2.x, itx.Item2.y);

                Point moveable = _primary.MovablePivot;
                double d1 = moveable.DistanceTo(itx1);
                double d2 = moveable.DistanceTo(itx2);

                if (d1 < d2)
                    return itx1;
                else
                    return itx2;

                //if (itx1.Y > itx2.Y)
                //    return itx1;
                //else
                //    return itx2;
            }

            public Vector GetArmatureVector()
            {
                return new Vector
                {
                    X = _secondaryOpen.MovablePivot.X - _primaryOpen.MovablePivot.X,
                    Y = _secondaryOpen.MovablePivot.Y - _primaryOpen.MovablePivot.Y
                };
            }

            public Box.Circle[] GetGeometryCircles()
            {
                var circles = new Box.Circle[3];
                circles[0] = new Circle(_primary.FixedPivot.X, _primary.FixedPivot.Y, _primary.GetRadius());
                circles[1] = new Circle(_secondary.FixedPivot.X, _secondary.FixedPivot.Y, _secondary.GetRadius());
                circles[2] = new Circle(_secondaryOpen.MovablePivot.X, _secondaryOpen.MovablePivot.Y, this._armature);

                return circles;
            }
        }
    }
}
