using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace LinkageHinge
{
    partial class Box
    {
        public class Circle
        {
            public class CirclePoint
            {
                public double x;
                public double y;

                public CirclePoint(double px, double py)
                {
                    x = px;
                    y = py;
                }

                public CirclePoint sub(CirclePoint p2)
                {
                    return new CirclePoint(x - p2.x, y - p2.y);
                }

                public CirclePoint add(CirclePoint p2)
                {
                    return new CirclePoint(x + p2.x, y + p2.y);
                }

                public double distance(CirclePoint p2)
                {
                    return Math.Sqrt((x - p2.x) * (x - p2.x) + (y - p2.y) * (y - p2.y));
                }

                public CirclePoint normal()
                {
                    double length = Math.Sqrt(x * x + y * y);
                    return new CirclePoint(x / length, y / length);
                }

                public CirclePoint scale(double s)
                {
                    return new CirclePoint(x * s, y * s);
                }
            }

            private double x;
            private double y;
            private double r;
            private double left;

            public double Diameter { get { return r + r; } }
            public Point TopLeft { get { return new Point(x - r, y - r);  } }

            public Circle(double cx, double cy, double cr)
            {
                x = cx;
                y = cy;
                r = cr;
                left = x - r;
            }

            public Tuple<CirclePoint, CirclePoint> intersections(Circle c)
            {
                var P0 = new CirclePoint(x, y);
                var P1 = new CirclePoint(c.x, c.y);

                double d = P0.distance(P1);
                double a = (r * r - c.r * c.r + d * d) / (2 * d);

                if (a > r)
                    throw new ArithmeticException();

                double h = Math.Sqrt(r * r - a * a);
                var P2 = P1.sub(P0).scale(a / d).add(P0);

                double x3 = P2.x + h * (P1.y - P0.y) / d;
                double y3 = P2.y - h * (P1.x - P0.x) / d;
                double x4 = P2.x - h * (P1.y - P0.y) / d;
                double y4 = P2.y + h * (P1.x - P0.x) / d;

                return new Tuple<CirclePoint, CirclePoint>(new CirclePoint(x3, y3), new CirclePoint(x4, y4));
            }
        }
    }
}
