using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace LinkageHinge
{
    namespace CustomExtensions
    {
        public static class PointExtensions
        {
            public static double DistanceTo(this Point p1, Point p2)
            {
                return Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y)); ;
            }

            public static Point RotateAbout(this Point p, Point origin, double degrees)
            {
                double x = p.X - origin.X;
                double y = p.Y - origin.Y;
                double radians = degrees * Math.PI / 180.0;

                return new Point
                {
                    X = origin.X + (Math.Cos(radians) * x - Math.Sin(radians) * y),
                    Y = origin.Y + (Math.Sin(radians) * x + Math.Cos(radians) * y)
                };
            }

            public static Point Add(this Point p, Vector v)
            {
                return new Point
                {
                    X = p.X + v.X,
                    Y = p.Y + v.Y
                };
            }
        }

        public static class VectorExtensions
        {
            public static double DegreesTo(this Vector a, Vector b)
            {
                var radians = Math.Atan2(a.Y, a.X) - Math.Atan2(b.Y, b.X);
                return radians * 180.0 / Math.PI;
            }

            public static Vector RotateBy(this Vector v, double degrees)
            {
                double radians = degrees * Math.PI / 180.0;
                return new Vector
                {
                    X = v.X * Math.Cos(radians) - v.Y * Math.Sin(radians),
                    Y = v.X * Math.Sin(radians) + v.Y * Math.Cos(radians)
                };
            }

            public static Vector Perpendicular(this Vector v)
            {
                return new Vector
                {
                    X = -v.Y,
                    Y =  v.X
                };
            }

            public static Vector SetLength(this Vector v, double length)
            {
                try
                {
                    var factor = length / v.Length;
                    return new Vector
                    {
                        X = v.X * factor,
                        Y = v.Y * factor
                    };
                }
                catch (DivideByZeroException)
                {
                    return new Vector();
                }
            }
        }
    }
}
