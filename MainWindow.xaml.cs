using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Reflection;

namespace LinkageHinge
{
    using CustomExtensions;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Measurements _measurements = new Measurements
        {
            BoxWidth = 200,
            BoxHeight1 = 160,
            BoxHeight2 = 40,

            Linkage1BottomX = 20,
            Linkage1BottomY = 110,
            Linkage1TopX = 50,
            Linkage1TopY = 180,

            Linkage2BottomX = 75,
            Linkage2BottomY = 110,
            Linkage2TopX = 180,
            Linkage2TopY = 180
        };

        public MainWindow()
        {
            InitializeComponent();

            var assemblyName = Assembly.GetExecutingAssembly().GetName();
            this.Title = assemblyName.Name + 
                " " + assemblyName.Version.Major +
                "." + assemblyName.Version.Minor + 
                "." + assemblyName.Version.Revision;

            CanvasBorder.BorderThickness = new Thickness(1);

            _box = new Box();
            _box.SetSize(_measurements);

            _hinge = new Box.Hinge();
            _hinge.SetPrimaryFixed(new Point(_measurements.Linkage1BottomX, _measurements.Linkage1BottomY));
            _hinge.SetPrimaryMovable(new Point(_measurements.Linkage1TopX, _measurements.Linkage1TopY));
            _hinge.SetSecondaryFixed(new Point(_measurements.Linkage2BottomX, _measurements.Linkage2BottomY));
            _hinge.SetSecondaryMovable(new Point(_measurements.Linkage2TopX, _measurements.Linkage2TopY));
            _box.AttachHinge(_hinge);

            this.DataContext = _measurements;
        }

        private void Draw()
        {
            this.Dispatcher.Invoke((Action)(() =>
                {
                    MyCanvas.Children.Clear();
                    DrawGrid();
                    DrawBox();
                    DrawLinkages();
                    if (GeometryCheckbox.IsChecked == true)
                        DrawGeometry();
                }));
        }

        private Point BoxCoordToCanvasCoord(Point p)
        {
            var origin = new Point { X = MyCanvas.ActualWidth / 2, Y = MyCanvas.ActualHeight / 2 };
            var bottomLeft = new Point { X = origin.X, Y = origin.Y + _measurements.BoxHeight1 };
            var canvasCoord = new Point { X = bottomLeft.X + p.X, Y = bottomLeft.Y - p.Y };
            return canvasCoord;
        }

        private void DrawGrid()
        {
            var vCrosshair = new Line();
            vCrosshair.Stroke = System.Windows.Media.Brushes.LightSteelBlue;
            vCrosshair.StrokeThickness = 0.25;
            vCrosshair.X1 = 0.0;
            vCrosshair.Y1 = MyCanvas.ActualHeight / 2;
            vCrosshair.X2 = MyCanvas.ActualWidth;
            vCrosshair.Y2 = MyCanvas.ActualHeight / 2;

            var hCrosshair = new Line();
            hCrosshair.Stroke = System.Windows.Media.Brushes.LightSteelBlue;
            hCrosshair.StrokeThickness = 0.25;
            hCrosshair.X1 = MyCanvas.ActualWidth / 2;
            hCrosshair.Y1 = 0.0;
            hCrosshair.X2 = MyCanvas.ActualWidth / 2;
            hCrosshair.Y2 = MyCanvas.ActualHeight;

            MyCanvas.Children.Add(vCrosshair);
            MyCanvas.Children.Add(hCrosshair);
        }

        private void DrawBox()
        {
            var origin = new Point { X = MyCanvas.ActualWidth / 2, Y = MyCanvas.ActualHeight / 2 };

            DrawBottomBoxAt(origin);

            var corners = _box.GetTopBox();
            DrawTopBoxAt(origin, corners);
        }

        private void DrawBottomBoxAt(Point origin)
        {
            var bottomRect = new Rectangle();
            bottomRect.Height = _measurements.BoxHeight1;
            bottomRect.Width = _measurements.BoxWidth;
            bottomRect.Stroke = System.Windows.Media.Brushes.Black;
            bottomRect.Fill = System.Windows.Media.Brushes.SkyBlue;
            bottomRect.StrokeThickness = 2;

            Canvas.SetLeft(bottomRect, origin.X);
            Canvas.SetTop(bottomRect, origin.Y);
            MyCanvas.Children.Add(bottomRect);
        }

        private void DrawTopBoxAt(Point origin, Point[] corners)
        {
            var myPolygon = new Polygon();
            myPolygon.Stroke = System.Windows.Media.Brushes.Black;
            myPolygon.Fill = System.Windows.Media.Brushes.LightSeaGreen;
            myPolygon.StrokeThickness = 2;
            myPolygon.HorizontalAlignment = HorizontalAlignment.Left;
            myPolygon.VerticalAlignment = VerticalAlignment.Center;

            var translated = new Point[4];
            translated[(int)Corner.BottomLeft].X   = origin.X + corners[(int)Corner.BottomLeft].X;
            translated[(int)Corner.BottomRight].X  = origin.X + corners[(int)Corner.BottomRight].X;
            translated[(int)Corner.TopRight].X     = origin.X + corners[(int)Corner.TopRight].X;
            translated[(int)Corner.TopLeft].X      = origin.X + corners[(int)Corner.TopLeft].X;
            translated[(int)Corner.BottomLeft].Y   = origin.Y - corners[(int)Corner.BottomLeft].Y;
            translated[(int)Corner.BottomRight].Y  = origin.Y - corners[(int)Corner.BottomRight].Y;
            translated[(int)Corner.TopRight].Y     = origin.Y - corners[(int)Corner.TopRight].Y;
            translated[(int)Corner.TopLeft].Y      = origin.Y - corners[(int)Corner.TopLeft].Y;

            var myPointCollection = new PointCollection(translated);
            myPolygon.Points = myPointCollection;
            MyCanvas.Children.Add(myPolygon);
        }

        private void DrawLinkages()
        {
            var fixed1 = BoxCoordToCanvasCoord(_box._hinge.GetPrimaryFixed());
            var fixed2 = BoxCoordToCanvasCoord(_box._hinge.GetSecondaryFixed());
            var moveable1 = BoxCoordToCanvasCoord(_box._hinge.GetOpenPrimaryMovable());
            var moveable2 = BoxCoordToCanvasCoord(_box._hinge.GetOpenSecondaryMovable());

            var linkage1 = new Line();
            linkage1.Stroke = System.Windows.Media.Brushes.Chocolate;
            linkage1.StrokeThickness = 8.0;
            linkage1.StrokeStartLineCap = PenLineCap.Round;
            linkage1.StrokeEndLineCap = PenLineCap.Round;
            linkage1.Opacity = 0.75;
            linkage1.X1 = fixed1.X;
            linkage1.Y1 = fixed1.Y;
            linkage1.X2 = moveable1.X;
            linkage1.Y2 = moveable1.Y;

            var linkage2 = new Line();
            linkage2.Stroke = System.Windows.Media.Brushes.Chocolate;
            linkage2.StrokeThickness = 8.0;
            linkage2.StrokeStartLineCap = PenLineCap.Round;
            linkage2.StrokeEndLineCap = PenLineCap.Round;
            linkage2.Opacity = 0.75;
            linkage2.X1 = fixed2.X;
            linkage2.Y1 = fixed2.Y;
            linkage2.X2 = moveable2.X;
            linkage2.Y2 = moveable2.Y;

            MyCanvas.Children.Add(linkage1);
            MyCanvas.Children.Add(linkage2);
        }

        private void DrawGeometry()
        {
            var origin1 = new Point { X = MyCanvas.ActualWidth / 2, Y = MyCanvas.ActualHeight / 2 };
            var circles = _box._hinge.GetGeometryCircles();

            foreach (var c in circles)
            {
                var ellipse = new Ellipse();
                ellipse.Stroke = System.Windows.Media.Brushes.Black;
                ellipse.StrokeThickness = 0.35;
                ellipse.Width = c.Diameter;
                ellipse.Height = c.Diameter;

                var origin = BoxCoordToCanvasCoord(c.TopLeft);
                Canvas.SetLeft(ellipse, origin.X);
                Canvas.SetTop(ellipse, origin.Y);
                MyCanvas.Children.Add(ellipse);
            }
        }

        private void MyCanvas_SizeChanged(object sender, SizeChangedEventArgs e)        { ResetBox(); }
        private void BoxWidth_TextChanged(object sender, TextChangedEventArgs e)        { ResetBox(); }
        private void BoxHeight1_TextChanged(object sender, TextChangedEventArgs e)      { ResetBox(); }
        private void BoxHeight2_TextChanged(object sender, TextChangedEventArgs e)      { ResetBox(); }
        private void Linkage1TopX_TextChanged(object sender, TextChangedEventArgs e)    { ResetBox(); }
        private void Linkage1TopY_TextChanged(object sender, TextChangedEventArgs e)    { ResetBox(); }
        private void Linkage1BottomX_TextChanged(object sender, TextChangedEventArgs e) { ResetBox(); }
        private void Linkage1BottomY_TextChanged(object sender, TextChangedEventArgs e) { ResetBox(); }
        private void Linkage2TopX_TextChanged(object sender, TextChangedEventArgs e)    { ResetBox(); }
        private void Linkage2TopY_TextChanged(object sender, TextChangedEventArgs e)    { ResetBox(); }
        private void Linkage2BottomX_TextChanged(object sender, TextChangedEventArgs e) { ResetBox(); }
        private void Linkage2BottomY_TextChanged(object sender, TextChangedEventArgs e) { ResetBox(); }


        private Box _box;
        private Box.Hinge _hinge;
        private System.Timers.Timer _timer = new System.Timers.Timer(100);
        private int _angle = 0;


        private void ResetBox()
        {
            _angle = 0;

            _box.SetSize(_measurements);

            _hinge.SetPrimaryFixed(new Point(_measurements.Linkage1BottomX, _measurements.Linkage1BottomY));
            _hinge.SetPrimaryMovable(new Point(_measurements.Linkage1TopX, _measurements.Linkage1TopY));
            _hinge.SetSecondaryFixed(new Point(_measurements.Linkage2BottomX, _measurements.Linkage2BottomY));
            _hinge.SetSecondaryMovable(new Point(_measurements.Linkage2TopX, _measurements.Linkage2TopY));

            Draw();
        }

        private void AnimateButton_Click(object sender, RoutedEventArgs e)
        {
            ResetBox();

            _timer.Elapsed += new ElapsedEventHandler(HandleTimerElapsed);
            _timer.AutoReset = true;
            _timer.Enabled = true;
        }

        public void HandleTimerElapsed(object sender, ElapsedEventArgs e)
        {
            _angle++;
            if (_angle >= 360)
            {
                _timer.Stop();
                return;
            }

            bool collision = !_box.OpenByAngle(_angle);
            if (collision)
            {
                _timer.Stop();
                return;
            }

            Draw();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            ResetBox();
        }

        private void StepButton_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();

            _angle += 1;
            if (_angle >= 360)
                return;

            bool collision = !_box.OpenByAngle(_angle);
            if (collision)
                return;

            Draw();
        }

        private void GeometryCheckbox_Click(object sender, RoutedEventArgs e)
        {

            if (_timer.Enabled == false)
                Draw();
        }
    }

    public class Measurements
    {
        public int BoxWidth { get; set; }
        public int BoxHeight1 { get; set; }
        public int BoxHeight2 { get; set; }

        public int Linkage1BottomX { get; set; }
        public int Linkage1BottomY { get; set; }
        public int Linkage1TopX { get; set; }
        public int Linkage1TopY { get; set; }

        public int Linkage2BottomX { get; set; }
        public int Linkage2BottomY { get; set; }
        public int Linkage2TopX { get; set; }
        public int Linkage2TopY { get; set; }
    }

}
