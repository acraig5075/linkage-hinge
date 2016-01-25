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
    using System.Globalization;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Measurements _measurements = new Measurements();

        public MainWindow()
        {
            InitializeComponent();

            this.Top = Properties.Settings.Default.Top;
            this.Left = Properties.Settings.Default.Left;
            this.Height = Properties.Settings.Default.Height;
            this.Width = Properties.Settings.Default.Width;
            if (Properties.Settings.Default.Maximised)
                WindowState = WindowState.Maximized;

            var assemblyName = Assembly.GetExecutingAssembly().GetName();
            this.Title = assemblyName.Name + 
                " " + assemblyName.Version.Major +
                "." + assemblyName.Version.Minor;

            CanvasBorder.BorderThickness = new Thickness(1);

            _measurements.FromSettings();

            _box = new Box();
            _box.SetSize(_measurements);

            _hinge = new Box.Hinge();
            _hinge.SetSize(_measurements);

            _box.AttachHinge(_hinge);

            this.DataContext = _measurements;
        }

        private Object thisLock = new Object();

        private void Draw()
        {
            lock (thisLock)
            {
                this.Dispatcher.Invoke((Action)(() =>
                    {
                        MyCanvas.Children.Clear();
                        DrawGrid();
                        DrawBox();
                        DrawLinkages();

                        if (GeometryCheckbox.IsChecked == true)
                            DrawGeometry();

                        if (DimensionsCheckbox.IsChecked == true)
                            DrawDimensions();

                        if (BusyAnimating)
                            _timer.Enabled = true;
                    }));
            }
        }

        private Point BoxCoordToCanvasCoord(Point p)
        {
            var origin = new Point { X = MyCanvas.ActualWidth / 2, Y = MyCanvas.ActualHeight / 2 };
            var canvasCoord = new Point { X = origin.X + p.X, Y = origin.Y + p.Y };
            return canvasCoord;
        }

        private void DrawGrid()
        {
            var vCrosshair = new Line();
            vCrosshair.Stroke = System.Windows.Media.Brushes.LightSteelBlue;
            vCrosshair.StrokeThickness = 0.35;
            vCrosshair.X1 = 0.0;
            vCrosshair.Y1 = MyCanvas.ActualHeight / 2;
            vCrosshair.X2 = MyCanvas.ActualWidth;
            vCrosshair.Y2 = MyCanvas.ActualHeight / 2;

            var hCrosshair = new Line();
            hCrosshair.Stroke = System.Windows.Media.Brushes.LightSteelBlue;
            hCrosshair.StrokeThickness = 0.35;
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
            translated[(int)Corner.BottomLeft].Y   = origin.Y + corners[(int)Corner.BottomLeft].Y;
            translated[(int)Corner.BottomRight].Y  = origin.Y + corners[(int)Corner.BottomRight].Y;
            translated[(int)Corner.TopRight].Y     = origin.Y + corners[(int)Corner.TopRight].Y;
            translated[(int)Corner.TopLeft].Y      = origin.Y + corners[(int)Corner.TopLeft].Y;

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

        private void DrawDimensions()
        {
            double gap = 15;
            var origin = new Point { X = MyCanvas.ActualWidth / 2, Y = MyCanvas.ActualHeight / 2 };
            var start = new Point(origin.X, gap);

            var m = _measurements;
            double a = m.Linkage1TopX;
            double b = m.Linkage1TopY;
            double c = m.Linkage1BottomX;
            double d = m.Linkage1BottomY;
            double e = m.Linkage2TopX;
            double f = m.Linkage2TopY;
            double g = m.Linkage2BottomX;
            double h = m.Linkage2BottomY;

            DrawHorizontalDimension(origin, 1 * gap, gap, "W", new Point(m.BoxWidth, m.BoxHeight2));
            DrawHorizontalDimension(origin, 2 * gap, gap, "E", new Point(e, -f));
            DrawHorizontalDimension(origin, 3 * gap, gap, "A", new Point(a, -b));
            DrawHorizontalDimension(origin, MyCanvas.ActualHeight - 1 * gap, gap, "G", new Point(g, h));
            DrawHorizontalDimension(origin, MyCanvas.ActualHeight - 2 * gap, gap, "C", new Point(c, d));

            DrawVerticalDimension(origin, MyCanvas.ActualWidth - 2 * gap, gap, "H1", new Point(m.BoxWidth,  m.BoxHeight1));
            DrawVerticalDimension(origin, MyCanvas.ActualWidth - 2 * gap, gap, "H2", new Point(m.BoxWidth, -m.BoxHeight2));
            DrawVerticalDimension(origin, MyCanvas.ActualWidth - 4 * gap, gap, "H", new Point(g,  h));
            DrawVerticalDimension(origin, MyCanvas.ActualWidth - 4 * gap, gap, "F", new Point(e, -f));
            DrawVerticalDimension(origin, MyCanvas.ActualWidth - 6 * gap, gap, "D", new Point(c,  d));
            DrawVerticalDimension(origin, MyCanvas.ActualWidth - 6 * gap, gap, "B", new Point(a, -b));
        }

        private void DrawHorizontalDimension(Point p1, double canvasY, double gap, String label, Point p2)
        {
            var start = new Point(p1.X, canvasY);
            var end = new Point(p1.X + p2.X, canvasY);
            var text = new Point(p1.X + p2.X / 2.0, canvasY - gap);
            var origin = new Point(MyCanvas.ActualWidth / 2.0, MyCanvas.ActualHeight / 2.0);

            var dimLine = new Line();
            dimLine.Stroke = System.Windows.Media.Brushes.Black;
            dimLine.StrokeThickness = 0.15;
            dimLine.X1 = start.X;
            dimLine.Y1 = start.Y;
            dimLine.X2 = end.X;
            dimLine.Y2 = end.Y;
            MyCanvas.Children.Add(dimLine);

            var dimWitness = new Line();
            dimWitness.Stroke = System.Windows.Media.Brushes.Black;
            dimWitness.StrokeThickness = 0.15;
            dimWitness.X1 = end.X;
            dimWitness.Y1 = end.Y;
            dimWitness.X2 = end.X;
            dimWitness.Y2 = origin.Y + p2.Y;
            MyCanvas.Children.Add(dimWitness);

            FormattedText ft = new FormattedText(label, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface("Verdana"), 12, System.Windows.Media.Brushes.Black);

            var textBlock = new TextBlock();
            textBlock.Text = label;
            //textBlock.Height = 12;
            textBlock.TextAlignment = TextAlignment.Center;
            textBlock.VerticalAlignment = VerticalAlignment.Top;
            Canvas.SetLeft(textBlock, text.X - ft.Width / 2);
            Canvas.SetTop(textBlock, text.Y);
            MyCanvas.Children.Add(textBlock);

            var length = gap * 0.75;
            DrawArrow(start, length, 0.0);
            DrawArrow(end, length, 180.0);
        }

        private void DrawVerticalDimension(Point p1, double canvasX, double gap, String label, Point p2)
        {
            var start = new Point(canvasX, p1.Y);
            var end = new Point(canvasX, p1.Y + p2.Y);
            var text = new Point(canvasX, p1.Y + p2.Y / 2.0 - gap);
            var origin = new Point(MyCanvas.ActualWidth / 2.0, MyCanvas.ActualHeight / 2.0);

            var dimLine = new Line();
            dimLine.Stroke = System.Windows.Media.Brushes.Black;
            dimLine.StrokeThickness = 0.15;
            dimLine.X1 = start.X;
            dimLine.Y1 = start.Y;
            dimLine.X2 = end.X;
            dimLine.Y2 = end.Y;
            MyCanvas.Children.Add(dimLine);

            var dimWitness = new Line();
            dimWitness.Stroke = System.Windows.Media.Brushes.Black;
            dimWitness.StrokeThickness = 0.15;
            dimWitness.X1 = end.X;
            dimWitness.Y1 = end.Y;
            dimWitness.X2 = origin.X + p2.X;
            dimWitness.Y2 = end.Y;
            MyCanvas.Children.Add(dimWitness);

            FormattedText ft = new FormattedText(label, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface("Arial"), 12, System.Windows.Media.Brushes.Black);

            var textBlock = new TextBlock();
            textBlock.Text = label;
            //textBlock.Height = 12;
            //textBlock.TextAlignment = TextAlignment.Center;
            textBlock.VerticalAlignment = VerticalAlignment.Top;
            Canvas.SetLeft(textBlock, text.X + gap / 4);
            Canvas.SetTop(textBlock, text.Y + ft.Height / 2);
            MyCanvas.Children.Add(textBlock);

            var length = gap * 0.75;
            DrawArrow(start, length, p2.Y < 0 ? 90.0 : 270.0);
            DrawArrow(end, length, p2.Y < 0 ? 270.0 : 90.0);
        }

        private void DrawArrow(Point terminal, double length, double degrees)
        {
            var p1 = new Point(terminal.X + length, terminal.Y + length / 4.0);
            var p2 = new Point(terminal.X + length, terminal.Y - length / 4.0);

            p1 = p1.RotateAbout(terminal, degrees);
            p2 = p2.RotateAbout(terminal, degrees);

            var arrowLine1 = new Line();
            arrowLine1.Stroke = System.Windows.Media.Brushes.Black;
            arrowLine1.StrokeThickness = 0.15;
            arrowLine1.X1 = p1.X;
            arrowLine1.Y1 = p1.Y;
            arrowLine1.X2 = terminal.X;
            arrowLine1.Y2 = terminal.Y;
            MyCanvas.Children.Add(arrowLine1);

            var arrowLine2 = new Line();
            arrowLine2.Stroke = System.Windows.Media.Brushes.Black;
            arrowLine2.StrokeThickness = 0.15;
            arrowLine2.X1 = p2.X;
            arrowLine2.Y1 = p2.Y;
            arrowLine2.X2 = terminal.X;
            arrowLine2.Y2 = terminal.Y;
            MyCanvas.Children.Add(arrowLine2);

        }

        private void MyCanvas_SizeChanged(object sender, SizeChangedEventArgs e)        { ResetBox(); }

        private void BoxWidth_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)        { ResetBox(); }
        private void BoxHeight1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)      { ResetBox(); }
        private void BoxHeight2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)      { ResetBox(); }
        private void Linkage1TopX_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)    { ResetBox(); }
        private void Linkage1TopY_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)    { ResetBox(); }
        private void Linkage1BottomX_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e) { ResetBox(); }
        private void Linkage1BottomY_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e) { ResetBox(); }
        private void Linkage2TopX_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)    { ResetBox(); }
        private void Linkage2TopY_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)    { ResetBox(); }
        private void Linkage2BottomX_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e) { ResetBox(); }
        private void Linkage2BottomY_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e) { ResetBox(); }



        private Box _box;
        private Box.Hinge _hinge;
        private System.Timers.Timer _timer = new System.Timers.Timer(100);
        private int _angle = 1;

        private bool BusyAnimating { get; set; }


        private void ResetBox()
        {
            _box.SetSize(_measurements);
            _hinge.SetSize(_measurements);

            Draw();
        }

        private void AnimateButton_Click(object sender, RoutedEventArgs e)
        {
            _angle = 1;

            if (_timer.Enabled == true)
            {
                _timer.Enabled = false;
                BusyAnimating = false;
                AnimateButton.Content = "Animate";

                _timer.Close();
            }
            else
            {
                ResetBox();
                BusyAnimating = true;
                AnimateButton.Content = "Stop";

                _timer.Elapsed += new ElapsedEventHandler(HandleTimerElapsed);
                _timer.AutoReset = false;
                _timer.Enabled = true;
            }
        }

        public void HandleTimerElapsed(object sender, ElapsedEventArgs e)
        {
            _timer.Stop();

            bool collision = !_box.OpenByAngle(_angle);
            if (collision)
            {
                BusyAnimating = false;
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

            _angle = 5;

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

        private void DimensionsCheckbox_Click(object sender, RoutedEventArgs e)
        {
            if (_timer.Enabled == false)
                Draw();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                Properties.Settings.Default.Top = RestoreBounds.Top;
                Properties.Settings.Default.Left = RestoreBounds.Left;
                Properties.Settings.Default.Height = RestoreBounds.Height;
                Properties.Settings.Default.Width = RestoreBounds.Width;
                Properties.Settings.Default.Maximised = true;
            }
            else
            {
                Properties.Settings.Default.Top = this.Top;
                Properties.Settings.Default.Left = this.Left;
                Properties.Settings.Default.Height = this.Height;
                Properties.Settings.Default.Width = this.Width;
                Properties.Settings.Default.Maximised = false;
            }

            _measurements.ToSettings();

            Properties.Settings.Default.Save();
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

        public void FromSettings()
        {
            BoxWidth        = Properties.Settings.Default.BoxWidth;
            BoxHeight1      = Properties.Settings.Default.BoxHeight1;
            BoxHeight2      = Properties.Settings.Default.BoxHeight2;
            Linkage1BottomX = Properties.Settings.Default.Linkage1BottomX;
            Linkage1BottomY = Properties.Settings.Default.Linkage1BottomY;
            Linkage1TopX    = Properties.Settings.Default.Linkage1TopX;
            Linkage1TopY    = Properties.Settings.Default.Linkage1TopY;
            Linkage2BottomX = Properties.Settings.Default.Linkage2BottomX;
            Linkage2BottomY = Properties.Settings.Default.Linkage2BottomY;
            Linkage2TopX    = Properties.Settings.Default.Linkage2TopX;
            Linkage2TopY    = Properties.Settings.Default.Linkage2TopY;
        }

        public void ToSettings()
        {
            Properties.Settings.Default.BoxWidth        = BoxWidth;
            Properties.Settings.Default.BoxHeight1      = BoxHeight1;
            Properties.Settings.Default.BoxHeight2      = BoxHeight2;
            Properties.Settings.Default.Linkage1BottomX = Linkage1BottomX;
            Properties.Settings.Default.Linkage1BottomY = Linkage1BottomY;
            Properties.Settings.Default.Linkage1TopX    = Linkage1TopX;
            Properties.Settings.Default.Linkage1TopY    = Linkage1TopY;
            Properties.Settings.Default.Linkage2BottomX = Linkage2BottomX;
            Properties.Settings.Default.Linkage2BottomY = Linkage2BottomY;
            Properties.Settings.Default.Linkage2TopX    = Linkage2TopX;
            Properties.Settings.Default.Linkage2TopY    = Linkage2TopY;
        }
    }

}
