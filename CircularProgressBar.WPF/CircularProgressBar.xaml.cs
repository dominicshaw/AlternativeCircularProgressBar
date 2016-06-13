using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace DesignInControl
{
    /// <summary>
    /// Interaction logic for CircularProgressBar.xaml
    /// </summary>
    public partial class CircularProgressBar
    {
        private readonly DispatcherTimer _animationTimer;

        public CircularProgressBar()
        {
            InitializeComponent();
            Angle = (Percentage * 360) / 100;
            RenderArc();

            _animationTimer = new DispatcherTimer(DispatcherPriority.ContextIdle, Dispatcher) { Interval = new TimeSpan(0, 0, 0, 0, 20) };
        }
        
        private void Start()
        {
            if (ChangeCursor)
                Mouse.OverrideCursor = Cursors.Wait;

            _animationTimer.Tick += HandleAnimationTick;
            _animationTimer.Start();
        }

        private void Stop()
        {
            _animationTimer.Stop();

            if (ChangeCursor)
                Mouse.OverrideCursor = Cursors.Arrow;

            _animationTimer.Tick -= HandleAnimationTick;
        }

        private string _direction = "down";

        private void HandleAnimationTick(object sender, EventArgs e)
        {
            if (Bounce)
            {
                if (_direction == "up")
                {
                    Percentage = Percentage + 1;

                    if (Percentage > 100)
                    {
                        _direction = "down";
                        Percentage = 100;
                    }
                }
                else
                {
                    Percentage = Percentage - 1;

                    if (Percentage < 0)
                    {
                        _direction = "up";
                        Percentage = 0;
                    }
                }
            }
            else
            {
                Percentage = Percentage + 1;

                if (Percentage > 100)
                    Percentage = -1;
            }
        }

        public bool Bounce         { get { return (bool)   GetValue(BounceProperty);          } set { SetValue(BounceProperty,          value); } }
        public bool ChangeCursor   { get { return (bool)   GetValue(ChangeCursorProperty);    } set { SetValue(ChangeCursorProperty,    value); } }
        public int Radius          { get { return (int)    GetValue(RadiusProperty);          } set { SetValue(RadiusProperty,          value); } }
        public Brush SegmentColor  { get { return (Brush)  GetValue(SegmentColorProperty);    } set { SetValue(SegmentColorProperty,    value); } }
        public int StrokeThickness { get { return (int)    GetValue(StrokeThicknessProperty); } set { SetValue(StrokeThicknessProperty, value); } }
        public double Percentage   { get { return (double) GetValue(PercentageProperty);      } set { SetValue(PercentageProperty,      value); } }
        public double Angle        { get { return (double) GetValue(AngleProperty);           } set { SetValue(AngleProperty,           value); } }

        public static readonly DependencyProperty BounceProperty          = DependencyProperty.Register("Bounce",          typeof (bool),   typeof (CircularProgressBar), new PropertyMetadata(false));
        public static readonly DependencyProperty ChangeCursorProperty    = DependencyProperty.Register("ChangeCursor",    typeof (bool),   typeof (CircularProgressBar), new PropertyMetadata(false));
        public static readonly DependencyProperty PercentageProperty      = DependencyProperty.Register("Percentage",      typeof (double), typeof (CircularProgressBar), new PropertyMetadata(65d, OnPercentageChanged));
        public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register("StrokeThickness", typeof (int),    typeof (CircularProgressBar), new PropertyMetadata(5));
        public static readonly DependencyProperty SegmentColorProperty    = DependencyProperty.Register("SegmentColor",    typeof (Brush),  typeof (CircularProgressBar), new PropertyMetadata(new SolidColorBrush(Colors.Red)));
        public static readonly DependencyProperty RadiusProperty          = DependencyProperty.Register("Radius",          typeof (int),    typeof (CircularProgressBar), new PropertyMetadata(25, OnPropertyChanged));
        public static readonly DependencyProperty AngleProperty           = DependencyProperty.Register("Angle",           typeof (double), typeof (CircularProgressBar), new PropertyMetadata(120d, OnPropertyChanged));

        private static void OnPercentageChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var circle = (CircularProgressBar) sender;
            circle.Angle = (circle.Percentage * 360) / 100;
        }

        private static void OnPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var circle = (CircularProgressBar) sender;
            circle.RenderArc();
        }

        public void RenderArc()
        {
            var startPoint = new Point(Radius, 0);
            var endPoint = ComputeCartesianCoordinate(Angle, Radius);
            endPoint.X += Radius;
            endPoint.Y += Radius;

            pathRoot.Width = Radius * 2 + StrokeThickness;
            pathRoot.Height = Radius * 2 + StrokeThickness;
            pathRoot.Margin = new Thickness(StrokeThickness, StrokeThickness, 0, 0);

            var largeArc = Angle > 180.0;

            var outerArcSize = new Size(Radius, Radius);

            pathFigure.StartPoint = startPoint;

            if (startPoint.X == Math.Round(endPoint.X) && startPoint.Y == Math.Round(endPoint.Y))
                endPoint.X -= 0.01;

            arcSegment.Point = endPoint;
            arcSegment.Size = outerArcSize;
            arcSegment.IsLargeArc = largeArc;
        }

        private static Point ComputeCartesianCoordinate(double angle, double radius)
        {
            // convert to radians
            var angleRad = (Math.PI / 180.0) * (angle - 90);

            var x = radius * Math.Cos(angleRad);
            var y = radius * Math.Sin(angleRad);

            return new Point(x, y);
        }

        private void CircularProgressBar_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var isVisible = (bool) e.NewValue;

            if (isVisible)
                Start();
            else
                Stop();
        }

        private void PathRoot_OnUnloaded(object sender, RoutedEventArgs e)
        {
            Stop();
        }
    }
}
