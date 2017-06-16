using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace InteractiveChart
{
    public sealed partial class LolloChart : UserControl // where T : class, new()
    {
        #region properties
        private XYDataSeries_Internal _XY1DataSeries_Internal;
        private XYDataSeries_Internal _XY2DataSeries_Internal;
        private XYDataSeries_Internal _XY3DataSeries_Internal;
        private XYDataSeries_Internal _XY4DataSeries_Internal;
        private XGridLines_Internal _XPrimaryGridLines_Internal;
        private XGridLines_Internal _XSecondaryGridLines_Internal;
        private XGridLabels_Internal _XGridLabels_Internal;
        private YGridLines_Internal _YPrimaryGridLines_Internal;
        private YGridLines_Internal _YSecondaryGridLines_Internal;
        private YGridLabels_Internal _Y1GridLabels_Internal;
        private YGridLabels_Internal _Y2GridLabels_Internal;

        internal static readonly double LogBase = 2.0;
        internal static readonly double DataSeriesThickness = 4.0;
        internal static readonly double PrimaryGridLineThickness = 2.0;
        internal static readonly double SecondaryGridLineThickness = .3;

        private Style GridLineStyle;
        private Style PolylineStyle;

        private XYDataSeries _XY1DataSeries;
        public XYDataSeries XY1DataSeries { get { return _XY1DataSeries; } set { _XY1DataSeries = value; } }

        private XYDataSeries _XY2DataSeries;
        public XYDataSeries XY2DataSeries { get { return _XY2DataSeries; } set { _XY2DataSeries = value; } }

        private XYDataSeries _XY3DataSeries;
        public XYDataSeries XY3DataSeries { get { return _XY3DataSeries; } set { _XY3DataSeries = value; } }

        private XYDataSeries _XY4DataSeries;
        public XYDataSeries XY4DataSeries { get { return _XY4DataSeries; } set { _XY4DataSeries = value; } }

        private GridLines _XPrimaryGridLines;
        public GridLines XPrimaryGridLines { get { return _XPrimaryGridLines; } set { _XPrimaryGridLines = value; } }

        private GridLines _XSecondaryGridLines;
        public GridLines XSecondaryGridLines { get { return _XSecondaryGridLines; } set { _XSecondaryGridLines = value; } }

        private GridLabels _XGridLabels;
        public GridLabels XGridLabels { get { return _XGridLabels; } set { _XGridLabels = value; } }

        private GridLines _YPrimaryGridLines;
        public GridLines YPrimaryGridLines { get { return _YPrimaryGridLines; } set { _YPrimaryGridLines = value; } }

        private GridLines _YSecondaryGridLines;
        public GridLines YSecondaryGridLines { get { return _YSecondaryGridLines; } set { _YSecondaryGridLines = value; } }

        private GridLabels _Y1GridLabels;
        public GridLabels Y1GridLabels { get { return _Y1GridLabels; } set { _Y1GridLabels = value; } }

        private GridLabels _Y2GridLabels;
        public GridLabels Y2GridLabels { get { return _Y2GridLabels; } set { _Y2GridLabels = value; } }

        private GridScale _XGridScale;
        public GridScale XGridScale { get { return _XGridScale; } set { _XGridScale = value; } }

        private GridScale _Y1GridScale;
        public GridScale Y1GridScale { get { return _Y1GridScale; } set { _Y1GridScale = value; } }

        private GridScale _Y2GridScale;
        public GridScale Y2GridScale { get { return _Y2GridScale; } set { _Y2GridScale = value; } }

        private static readonly bool _isProperTouchDevice = new TouchCapabilities().TouchPresent == 1 && Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.Devices.Notification.VibrationDevice");
        public bool IsProperTouchDevice { get { return _isProperTouchDevice; } }

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(LolloChart), new PropertyMetadata(string.Empty));

        public Style TitleStyle
        {
            get { return (Style)GetValue(TitleStyleProperty); }
            set { SetValue(TitleStyleProperty, value); }
        }
        public static readonly DependencyProperty TitleStyleProperty =
            DependencyProperty.Register("TitleStyle", typeof(Style), typeof(LolloChart), new PropertyMetadata(null));

        public Style AppBarButtonStyle
        {
            get { return (Style)GetValue(AppBarButtonStyleProperty); }
            set { SetValue(AppBarButtonStyleProperty, value); }
        }
        public static readonly DependencyProperty AppBarButtonStyleProperty =
            DependencyProperty.Register("AppBarButtonStyle", typeof(Style), typeof(LolloChart), new PropertyMetadata(null));

        public string Footnote
        {
            get { return (string)GetValue(FootnoteProperty); }
            set { SetValue(FootnoteProperty, value); }
        }
        public static DependencyProperty FootnoteProperty =
            DependencyProperty.Register("Footnote", typeof(string), typeof(LolloChart), new PropertyMetadata(string.Empty));

        public Style FootnoteStyle
        {
            get { return (Style)GetValue(FootnoteStyleProperty); }
            set { SetValue(FootnoteStyleProperty, value); }
        }
        public static readonly DependencyProperty FootnoteStyleProperty =
            DependencyProperty.Register("FootnoteStyle", typeof(Style), typeof(LolloChart), new PropertyMetadata(null));

        public double LeftColumnWidth
        {
            get { return (double)GetValue(LeftColumnWidthProperty); }
            set { SetValue(LeftColumnWidthProperty, value); }
        }
        public static readonly DependencyProperty LeftColumnWidthProperty =
            DependencyProperty.Register("LeftColumnWidth", typeof(double), typeof(LolloChart), new PropertyMetadata(50.0));

        public double RightColumnWidth
        {
            get { return (double)GetValue(RightColumnWidthProperty); }
            set { SetValue(RightColumnWidthProperty, value); }
        }
        public static readonly DependencyProperty RightColumnWidthProperty =
            DependencyProperty.Register("RightColumnWidth", typeof(double), typeof(LolloChart), new PropertyMetadata(50.0));


        public double TitleRowHeight
        {
            get { return (double)GetValue(TitleRowHeightProperty); }
            set { SetValue(TitleRowHeightProperty, value); }
        }
        public static readonly DependencyProperty TitleRowHeightProperty =
            DependencyProperty.Register("TitleRowHeight", typeof(double), typeof(LolloChart), new PropertyMetadata(/*_isTouchDevicePresent ? 50.0 : */80.0));

        public double TopRowHeight
        {
            get { return (double)GetValue(TopRowHeightProperty); }
            set { SetValue(TopRowHeightProperty, value); }
        }
        public static readonly DependencyProperty TopRowHeightProperty =
            DependencyProperty.Register("TopRowHeight", typeof(double), typeof(LolloChart), new PropertyMetadata(30.0));

        public double BottomRowHeight
        {
            get { return (double)GetValue(BottomRowHeightProperty); }
            set { SetValue(BottomRowHeightProperty, value); }
        }
        public static readonly DependencyProperty BottomRowHeightProperty =
            DependencyProperty.Register("BottomRowHeight", typeof(double), typeof(LolloChart), new PropertyMetadata(40.0));

        public double FooterHeight
        {
            get { return (double)GetValue(FooterHeightProperty); }
            set { SetValue(FooterHeightProperty, value); }
        }
        public static readonly DependencyProperty FooterHeightProperty =
            DependencyProperty.Register("FooterHeight", typeof(double), typeof(LolloChart), new PropertyMetadata(30.0));

        private Bounds _widestSensibleBounds = new Bounds() { I0 = 0, I1 = 0 };
        private Bounds _currentBounds = new Bounds() { I0 = 0, I1 = 0 };
        public Func<LolloChart, int, int, Task> DataSetter
        {
            get { return (Func<LolloChart, int, int, Task>)GetValue(DataSetterProperty); }
            set { SetValue(DataSetterProperty, value); }
        }
        public static readonly DependencyProperty DataSetterProperty =
            DependencyProperty.Register("DataSetter", typeof(Func<LolloChart, int, int, Task>), typeof(LolloChart), new PropertyMetadata(null));

        private readonly ApplicationView _appView = null;
        private ApplicationViewOrientation _prevOrientation;
        #endregion properties


        #region lifecycle
        private bool IsHandlersActive = false;
        private void AddHandlers()
        {
            if (IsHandlersActive) return;

            IsHandlersActive = true;

            _appView.VisibleBoundsChanged += OnAppView_VisibleBoundsChanged;
            IsEnabledChanged += OnIsEnabledChanged;
            SizeChanged += OnSizeChanged;
        }

        private void RemoveHandlers()
        {
            _appView.VisibleBoundsChanged -= OnAppView_VisibleBoundsChanged;
            IsEnabledChanged -= OnIsEnabledChanged;
            SizeChanged -= OnSizeChanged;

            IsHandlersActive = false;
        }

        public LolloChart()
        {
            InitializeComponent();
            _appView = ApplicationView.GetForCurrentView();
            _prevOrientation = _appView.Orientation;

            //DataContext = this;
            SetLineStyles();
            // LOLLO NOTE the buttons respond at once, the manipulations instead are slow. Their speed improves if I only set a couple.
            // I need to catch TranslateY so I can compare X e Y and do nothing is Abs(Y) > Abs(X)
            GridChartArea.ManipulationMode = ManipulationModes.Scale | ManipulationModes.TranslateX | ManipulationModes.TranslateY;
        }

        public void Open()
        {
            AddHandlers();
        }

        public void Close()
        {
            RemoveHandlers();
        }
        #endregion lifecycle


        #region events
        public class ChartTappedArguments
        {
            public double X;
            public double Y;
            public double XMax;
            public double YMax;
        }
        public event EventHandler<ChartTappedArguments> ChartTapped;
        public class Bounds
        {
            public int I0;
            public int I1;
        }
        public event EventHandler<Bounds> BoundsChanged;
        #endregion events

        private void SetLineStyles()
        {
            if (IsEnabled == false)
            {
                //LayoutRoot.Background = (Brush)Resources["BackgroundLikeOldOscilloscopeOff"];
                GridLineStyle = (Style)Resources["LineDark"];
                PolylineStyle = (Style)Resources["PolylineDark"];
            }
            else
            {
                //LayoutRoot.Background = (Brush)Resources["BackgroundLikeOldOscilloscopeOn"];
                GridLineStyle = (Style)Resources["LineBright"];
                PolylineStyle = (Style)Resources["PolylineBright"];
            }
            foreach (FrameworkElement item in GridChartArea.Children)
            {
                if (item is Line)
                {
                    item.Style = GridLineStyle;
                }
            }
            foreach (FrameworkElement item in GridChartArea.Children)
            {
                if (item is Polyline)
                {
                    item.Style = PolylineStyle;
                }
            }
        }

        #region event handlers
        private void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility == Visibility.Visible && e.OldValue != e.NewValue) SetLineStyles();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            //if (IsVisible && IsMeasureValid) //has worked out the right size - not available on Windows Phone
            if (Visibility == Visibility.Visible
                // && (e.PreviousSize.Height == 0.0 || e.PreviousSize.Width == 0.0)
                && e.NewSize.Height != 0.0 && e.NewSize.Width != 0.0)
            {
                Debug.WriteLine("LolloChart is drawing series " + Title + " because the size changed. The new size is height " + e.NewSize.Height + " by width " + e.NewSize.Width);
                Task draw = ReInitPropsAndDraw(_currentBounds);
            }
        }
        private void OnAppView_VisibleBoundsChanged(ApplicationView sender, object args)
        {
            if (Visibility == Visibility.Visible && _appView.Orientation != _prevOrientation)
            {
                _prevOrientation = _appView.Orientation;
                InvalidateMeasure();
            }
        }
        private void OnGridChartArea_Tapped(object sender, TappedRoutedEventArgs e)
        {
            GridChartArea.Opacity = 1;
            if (GridChartArea.Visibility == Visibility.Visible && GridChartArea.ActualHeight > 0.0 && GridChartArea.ActualWidth > 0.0)
            {
                Point touchPosition = e.GetPosition(GridChartArea);
                ChartTapped?.Invoke(this, new ChartTappedArguments() { X = touchPosition.X, Y = touchPosition.Y, XMax = GridChartArea.ActualWidth, YMax = GridChartArea.ActualHeight });
            }
        }

        private void OnGridChartArea_ManipulationInertiaStarting(object sender, ManipulationInertiaStartingRoutedEventArgs e)
        {
            GridChartArea.Opacity = .2;
        }

        private void OnGridChartArea_ManipulationStarting(object sender, ManipulationStartingRoutedEventArgs e)
        {
            GridChartArea.Opacity = .2;
        }

        private void OnGridChartArea_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            Task respond = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, () =>
            {
                Debug.WriteLine("OnLayoutRoot_ManipulationCompleted" + DateTime.Now.ToString("HH:mm:ss.ffffff"));
                GridChartArea.Opacity = 1;
                double scale = e.Cumulative.Scale;
                var translation = e.Cumulative.Translation;

                if (scale < 1.0)
                {
                    e.Handled = true;
                    if (Math.Abs(scale) < 0.1) scale = 0.1;
                    Debug.WriteLine("zoom out by " + scale);
                    ZoomOutAsync(1.0 / scale, e.Position.X);
                }
                else if (scale > 1.0)
                {
                    e.Handled = true;
                    if (Math.Abs(scale) > 10.0) scale = 10.0;
                    Debug.WriteLine("zoom in by " + scale);
                    ZoomInAsync(scale, e.Position.X);
                }
                else if (translation.X > 0.0)
                {
                    if (Math.Abs(translation.X) > Math.Abs(translation.Y))
                    {
                        e.Handled = true;
                        Debug.WriteLine("shift to lower X by " + translation.X);
                        PanBackAsync(Math.Abs(translation.X) < ActualWidth);
                    }
                }
                else if (translation.X < 0.0)
                {
                    if (Math.Abs(translation.X) > Math.Abs(translation.Y))
                    {
                        e.Handled = true;
                        Debug.WriteLine("shift to higher X by " + translation.X);
                        PanForwardAsync(Math.Abs(translation.X) < ActualWidth);
                    }
                }
            }).AsTask();
        }

        private bool IsBackDoable() { return _currentBounds.I0 > _widestSensibleBounds.I0; }
        private void OnPanBackClicked(object sender, RoutedEventArgs e) { PanBackAsync(); }
        public Task PanBackAsync(bool halfWayOnly = false)
        {
            if (GridChartArea.ActualWidth <= 0.0001 || ActualWidth <= 0.0 || _currentBounds == null || _widestSensibleBounds == null) return Task.CompletedTask;
            if (!IsBackDoable()) return Task.CompletedTask;

            int howManyPointsToDraw = _currentBounds.I1 - _currentBounds.I0 + 1;
            int howManyPointsToPan = halfWayOnly ? howManyPointsToDraw / 2 : howManyPointsToDraw;
            if (howManyPointsToPan <= 0) return Task.CompletedTask;

            var newBounds = new Bounds();
            newBounds.I0 = Math.Max(_currentBounds.I0 - howManyPointsToPan, _widestSensibleBounds.I0);
            newBounds.I1 = Math.Min(newBounds.I0 + howManyPointsToDraw - 1, _widestSensibleBounds.I1);
            if (newBounds.I0 >= newBounds.I1) return Task.CompletedTask;

            return ReInitPropsAndDraw(newBounds);
        }

        private bool IsPanForwardDoable() { return _currentBounds.I1 < _widestSensibleBounds.I1; }
        private void OnPanForwardClicked(object sender, RoutedEventArgs e) { PanForwardAsync(); }
        public Task PanForwardAsync(bool halfWayOnly = false)
        {
            if (GridChartArea.ActualWidth <= 0.0001 || ActualWidth <= 0.0 || _currentBounds == null || _widestSensibleBounds == null) return Task.CompletedTask;
            if (!IsPanForwardDoable()) return Task.CompletedTask;

            int howManyPointsToDraw = _currentBounds.I1 - _currentBounds.I0 + 1;
            int howManyPointsToPan = halfWayOnly ? howManyPointsToDraw / 2 : howManyPointsToDraw;
            if (howManyPointsToPan <= 0) return Task.CompletedTask;

            var newBounds = new Bounds();
            newBounds.I1 = Math.Min(_currentBounds.I1 + howManyPointsToPan, _widestSensibleBounds.I1);
            newBounds.I0 = Math.Max(newBounds.I1 - howManyPointsToDraw + 1, _widestSensibleBounds.I0);
            if (newBounds.I0 >= newBounds.I1) return Task.CompletedTask;

            return ReInitPropsAndDraw(newBounds);
        }

        private bool IsZoomInDoable()
        {
            int howManyPointsMustBeDrawn = _currentBounds.I1 - _currentBounds.I0 + 1;
            int howManyPointsCanBeDrawn = Convert.ToInt32(GridChartArea.ActualWidth * 2); // LOLLO TODO check the * 2
            return howManyPointsCanBeDrawn < howManyPointsMustBeDrawn;
        }
        private void OnZoomInClicked(object sender, RoutedEventArgs e) { ZoomInAsync(); }
        private Task ZoomInAsync(double factor = 2.0, double positionX = -1.0)
        {
            if (GridChartArea.ActualWidth <= 0.0001 || ActualWidth <= 0.0 || _currentBounds == null || _widestSensibleBounds == null) return Task.CompletedTask;
            if (!IsZoomInDoable()) return Task.CompletedTask;

            double howManyPoints = (_currentBounds.I1 - _currentBounds.I0 + 1) / factor;
            if (howManyPoints < 2.0) return Task.CompletedTask;

            int currentCentrePoint = 0;
            if (positionX >= 0.0) currentCentrePoint = _currentBounds.I0 + Convert.ToInt32(Convert.ToDouble(_currentBounds.I1 - _currentBounds.I0) * positionX / ActualWidth);
            else currentCentrePoint = (_currentBounds.I0 + _currentBounds.I1) / 2;

            var newBounds = new Bounds();
            newBounds.I1 = Math.Min(currentCentrePoint + Convert.ToInt32(howManyPoints / 2.0), _widestSensibleBounds.I1);
            newBounds.I0 = Math.Max(currentCentrePoint - Convert.ToInt32(howManyPoints / 2.0), _widestSensibleBounds.I0);

            if (newBounds.I0 > newBounds.I1 - 1) return Task.CompletedTask;

            return ReInitPropsAndDraw(newBounds);
        }

        private bool IsZoomOutDoable() { return !(_currentBounds.I1 >= _widestSensibleBounds.I1 && _currentBounds.I0 <= _widestSensibleBounds.I0); }
        private void OnZoomOutClicked(object sender, RoutedEventArgs e) { ZoomOutAsync(); }
        private Task ZoomOutAsync(double factor = 2.0, double positionX = -1.0)
        {
            if (GridChartArea.ActualWidth <= 0.0001 || ActualWidth <= 0.0 || _currentBounds == null || _widestSensibleBounds == null) return Task.CompletedTask;
            if (!IsZoomOutDoable()) return Task.CompletedTask;

            double howManyPoints = (_currentBounds.I1 - _currentBounds.I0 + 1) * factor;
            if (howManyPoints < 2.0) return Task.CompletedTask;

            int currentCentrePoint = 0;
            if (positionX >= 0.0) currentCentrePoint = _currentBounds.I0 + Convert.ToInt32(Convert.ToDouble(_currentBounds.I1 - _currentBounds.I0) * positionX / ActualWidth);
            else currentCentrePoint = (_currentBounds.I0 + _currentBounds.I1) / 2;

            var newBounds = new Bounds();
            newBounds.I1 = Math.Min(currentCentrePoint + Convert.ToInt32(howManyPoints / 2.0), _widestSensibleBounds.I1);
            newBounds.I0 = Math.Max(currentCentrePoint - Convert.ToInt32(howManyPoints / 2.0), _widestSensibleBounds.I0);

            if (newBounds.I0 > newBounds.I1 - 1) return Task.CompletedTask;

            return ReInitPropsAndDraw(newBounds);
        }
        #endregion event handlers

        protected override Size MeasureOverride(Size availableSize)
        {
            // must do the following otherwise the stupid thing keeps growing and growing forever
            // this method seems useless but it takes into account that the control owner may have reserved space for other objects!

            //// Override MeasureOverride to implement custom layout sizing behavior for your element as it participates in the Windows Presentation Foundation (WPF) layout system. 
            // Your implementation should do the following:
            //1.Iterate your element's particular collection of children that are part of layout, call Measure on each child element.
            //2.Immediately get DesiredSize on the child (this is set as a property after Measure is called).
            //3.Compute the net desired size of the parent based upon the measurement of the child elements.
            if (Visibility == Visibility.Collapsed)
            {
                return new Size(0, 0);
            }
            else
            {
                // Except for MainCol1 and MainRow2, all the elements have fixed size. This is set in the constructor.
                // Account for imposed constraints on width and height
                double imposedWidth = NormaliseDouble(Width);
                double imposedMaxWidth = NormaliseDouble(MaxWidth);
                double imposedHeight = NormaliseDouble(Height);
                double imposedMaxHeight = NormaliseDouble(MaxHeight);

                //double availableWidth = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Bounds.Width; // available bounds all out, no good
                //double availableHeight = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Bounds.Height; // available bounds all out, no good
                double availableWidth = _appView.VisibleBounds.Width; // available bounds minus chrome, ie minus app bars
                double availableHeight = _appView.VisibleBounds.Height; // available bounds minus chrome, ie minus app bars

                double maxAcceptableWidth = Math.Min(Math.Min(Math.Min(imposedWidth, imposedMaxWidth), availableWidth), availableSize.Width);
                double maxAcceptableHeight = Math.Min(Math.Min(Math.Min(imposedHeight, imposedMaxHeight), availableHeight), availableSize.Height);

                double widthAvailableForCentre = Math.Max(maxAcceptableWidth - GridYLabelsLeft.Width - GridYLabelsRight.Width, 0.0);
                double heightAvailableForCentre = Math.Max(maxAcceptableHeight - TitleGrid.Height - GridXLabelsTop.Height - GridXLabelsBottom.Height - FooterGrid.Height, 0.0);

                TitleGrid.Width = maxAcceptableWidth;
                GridXLabelsTop.Width = widthAvailableForCentre;
                GridYLabelsLeft.Height = heightAvailableForCentre;
                GridChartArea.Width = widthAvailableForCentre;
                GridChartArea.Height = heightAvailableForCentre;
                GridYLabelsRight.Height = heightAvailableForCentre;
                GridXLabelsBottom.Width = widthAvailableForCentre;
                FooterGrid.Width = maxAcceptableWidth;

                LayoutRoot.Measure(new Size(maxAcceptableWidth, maxAcceptableHeight));
                TitleGrid.Measure(new Size(maxAcceptableWidth, TitleGrid.Height));
                GridXLabelsTop.Measure(new Size(widthAvailableForCentre, GridXLabelsTop.Height));
                GridYLabelsLeft.Measure(new Size(GridYLabelsLeft.Width, heightAvailableForCentre));
                GridChartArea.Measure(new Size(widthAvailableForCentre, heightAvailableForCentre));
                GridYLabelsRight.Measure(new Size(GridYLabelsRight.Width, heightAvailableForCentre));
                GridXLabelsBottom.Measure(new Size(widthAvailableForCentre, GridXLabelsBottom.Height));
                FooterGrid.Measure(new Size(maxAcceptableWidth, FooterGrid.Height));

                if (maxAcceptableWidth == 0.0 || maxAcceptableHeight == 0.0) return new Size(0.0, 0.0);
                else return new Size(maxAcceptableWidth, maxAcceptableHeight);
            }
        }

        // this method is simpler than MeasureOverride but it does not take into account that the owner may have reserved some space for other stuff, beside the present control.
        //private void SetVariableGridSize()
        //{
        //	if (Visibility == Visibility.Visible)
        //	{
        //		// Except for MainCol1 and MainRow2, all the elements have fixed size. This is set in the constructor.
        //		// Account for imposed constraints on width and height
        //		double imposedWidth = NormaliseDouble(Width);
        //		double imposedMaxWidth = NormaliseDouble(MaxWidth);
        //		double imposedHeight = NormaliseDouble(Height);
        //		double imposedMaxHeight = NormaliseDouble(MaxHeight);

        //		//double availableWidth = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Bounds.Width; // available bounds all out, no good
        //		//double availableHeight = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Bounds.Height; // available bounds all out, no good
        //		double availableWidth = _appView.VisibleBounds.Width; // available bounds minus chrome, ie minus app bars
        //		double availableHeight = _appView.VisibleBounds.Height; // available bounds minus chrome, ie minus app bars

        //		double maxAcceptableWidth = Math.Min(Math.Min(imposedWidth, imposedMaxWidth), availableWidth);
        //		double maxAcceptableHeight = Math.Min(Math.Min(imposedHeight, imposedMaxHeight), availableHeight);

        //		double widthAvailableForCentre = maxAcceptableWidth - MainCol0.Width.Value - MainCol2.Width.Value;
        //		double heightAvailableForCentre = maxAcceptableHeight - MainRow0.Height.Value - MainRow1.Height.Value - MainRow3.Height.Value - MainRow4.Height.Value;

        //		GridChartArea.Width = Math.Max(widthAvailableForCentre, 0.0);
        //		GridChartArea.Height = Math.Max(heightAvailableForCentre, 0.0);
        //	}
        //}

        public Task<bool> DrawAsync(ICollection originalData1, ICollection originalData2, ICollection originalData3, ICollection originalData4, int i0, int i1)
        {
            int maxSensibleI1 = 0;
            if (originalData1 != null) maxSensibleI1 = Math.Max(maxSensibleI1, originalData1.Count);
            if (originalData2 != null) maxSensibleI1 = Math.Max(maxSensibleI1, originalData2.Count);
            if (originalData3 != null) maxSensibleI1 = Math.Max(maxSensibleI1, originalData3.Count);
            if (originalData4 != null) maxSensibleI1 = Math.Max(maxSensibleI1, originalData4.Count);
            _widestSensibleBounds = new Bounds() { I0 = 0, I1 = maxSensibleI1 };
            return ReInitPropsAndDraw(new Bounds() { I0 = i0, I1 = i1 });
        }

        private async Task<bool> ReInitPropsAndDraw(Bounds bounds)
        {
            if (bounds == null || _widestSensibleBounds == null) return false;
            var dataSetter = DataSetter;
            if (dataSetter == null) return false;

            bounds.I0 = Math.Max(bounds.I0, _widestSensibleBounds.I0);
            bounds.I1 = Math.Min(bounds.I1, _widestSensibleBounds.I1);

            bool isCurrentBoundsChanged = _currentBounds == null || _currentBounds.I0 != bounds.I0 || _currentBounds.I1 != bounds.I1;
            if (isCurrentBoundsChanged)
            {
                _currentBounds = new Bounds() { I0 = bounds.I0, I1 = bounds.I1 };
                BoundsChanged?.Invoke(this, _currentBounds);
            }

            PanBackButton.IsEnabled = IsBackDoable();
            PanForwardButton.IsEnabled = IsPanForwardDoable();
            ZoomInButton.IsEnabled = IsZoomInDoable();
            ZoomOutButton.IsEnabled = IsZoomOutDoable();

            await dataSetter(this, bounds.I0, bounds.I1);
            return DrawInternal();
        }

        private bool DrawInternal()
        {
            bool result = false;
            //if (_XAxis != null) _XAxis.Draw();
            //if (_YAxis != null) _YAxis.Draw();
            if (XGridScale == null || Y1GridScale == null) return false;
            if (XGridScale != null && Y1GridScale != null && XY1DataSeries != null)
            {
                if (_XY1DataSeries_Internal == null)
                    _XY1DataSeries_Internal = new XYDataSeries_Internal(GridChartArea, XGridScale.ScaleType, Y1GridScale.ScaleType,
                    XGridScale.Min, XGridScale.Max,
                    Y1GridScale.Min, Y1GridScale.Max,
                    XY1DataSeries.Points, ChartObjectTag.XY1DataSeries, PolylineStyle, XY1DataSeries.IsHistogram);
                else _XY1DataSeries_Internal.ReInit(XGridScale.ScaleType, Y1GridScale.ScaleType,
                     XGridScale.Min, XGridScale.Max,
                     Y1GridScale.Min, Y1GridScale.Max,
                     XY1DataSeries.Points);
                result = result || _XY1DataSeries_Internal.HasPoints;
            }
            if (XGridScale != null && Y2GridScale != null && XY2DataSeries != null)
            {
                if (_XY2DataSeries_Internal == null)
                    _XY2DataSeries_Internal = new XYDataSeries_Internal(GridChartArea, XGridScale.ScaleType, Y2GridScale.ScaleType,
                    XGridScale.Min, XGridScale.Max,
                    Y2GridScale.Min, Y2GridScale.Max,
                    XY2DataSeries.Points, ChartObjectTag.XY2DataSeries, PolylineStyle, XY2DataSeries.IsHistogram, XYDataSeries_Internal.StrokeDashArrays.Dashed);
                else _XY2DataSeries_Internal.ReInit(XGridScale.ScaleType, Y2GridScale.ScaleType,
                     XGridScale.Min, XGridScale.Max,
                     Y2GridScale.Min, Y2GridScale.Max,
                     XY2DataSeries.Points);
                result = result || _XY2DataSeries_Internal.HasPoints;
            }
            if (XGridScale != null && Y1GridScale != null && XY3DataSeries != null)
            {
                if (_XY3DataSeries_Internal == null)
                    _XY3DataSeries_Internal = new XYDataSeries_Internal(GridChartArea, XGridScale.ScaleType, Y1GridScale.ScaleType,
                    XGridScale.Min, XGridScale.Max,
                    Y1GridScale.Min, Y1GridScale.Max,
                    XY3DataSeries.Points, ChartObjectTag.XY3DataSeries, PolylineStyle, XY3DataSeries.IsHistogram);
                else _XY3DataSeries_Internal.ReInit(XGridScale.ScaleType, Y1GridScale.ScaleType,
                     XGridScale.Min, XGridScale.Max,
                     Y1GridScale.Min, Y1GridScale.Max,
                     XY3DataSeries.Points);
                result = result || _XY3DataSeries_Internal.HasPoints;
            }
            if (XGridScale != null && Y2GridScale != null && XY4DataSeries != null)
            {
                if (_XY4DataSeries_Internal == null)
                    _XY4DataSeries_Internal = new XYDataSeries_Internal(GridChartArea, XGridScale.ScaleType, Y2GridScale.ScaleType,
                    XGridScale.Min, XGridScale.Max,
                    Y2GridScale.Min, Y2GridScale.Max,
                    XY4DataSeries.Points, ChartObjectTag.XY4DataSeries, PolylineStyle, XY4DataSeries.IsHistogram, XYDataSeries_Internal.StrokeDashArrays.Dashed);
                else _XY4DataSeries_Internal.ReInit(XGridScale.ScaleType, Y2GridScale.ScaleType,
                     XGridScale.Min, XGridScale.Max,
                     Y2GridScale.Min, Y2GridScale.Max,
                     XY4DataSeries.Points);
                result = result || _XY4DataSeries_Internal.HasPoints;
            }
            if (XPrimaryGridLines != null && XGridScale != null)
            {
                if (_XPrimaryGridLines_Internal == null) _XPrimaryGridLines_Internal = new XGridLines_Internal(GridChartArea, XGridScale.ScaleType, XPrimaryGridLines.Points, XGridScale.Min, XGridScale.Max, PrimaryGridLineThickness, ChartObjectTag.XPrimaryGridlines, GridLineStyle);
                else _XPrimaryGridLines_Internal.ReInit(XGridScale.ScaleType, XPrimaryGridLines.Points, XGridScale.Min, XGridScale.Max);
            }
            if (XSecondaryGridLines != null && XPrimaryGridLines != null && XGridScale != null)
            {
                if (_XSecondaryGridLines_Internal == null) _XSecondaryGridLines_Internal = new XGridLines_Internal(GridChartArea, XGridScale.ScaleType, XSecondaryGridLines.Points, XGridScale.Min, XGridScale.Max, SecondaryGridLineThickness, ChartObjectTag.XSecondaryGridlines, GridLineStyle);
                else _XSecondaryGridLines_Internal.ReInit(XGridScale.ScaleType, XSecondaryGridLines.Points, XGridScale.Min, XGridScale.Max);
            }
            if (XPrimaryGridLines != null && XGridLabels != null && XGridScale != null)
            {
                if (_XGridLabels_Internal == null) _XGridLabels_Internal = new XGridLabels_Internal(this, CanvasXLabelsBottom, XGridScale.ScaleType, XGridLabels.Points, XGridLabels.DataPointsFormat, ChartObjectTag.XGridLabels);
                else _XGridLabels_Internal.ReInit(XGridScale.ScaleType, XGridLabels.Points, XGridLabels.DataPointsFormat);
            }

            if (YPrimaryGridLines != null && Y1GridScale != null)
            {
                if (_YPrimaryGridLines_Internal == null) _YPrimaryGridLines_Internal = new YGridLines_Internal(GridChartArea, Y1GridScale.ScaleType, YPrimaryGridLines.Points, Y1GridScale.Min, Y1GridScale.Max, PrimaryGridLineThickness, ChartObjectTag.YPrimaryGridlines, GridLineStyle);
                else _YPrimaryGridLines_Internal.ReInit(Y1GridScale.ScaleType, YPrimaryGridLines.Points, Y1GridScale.Min, Y1GridScale.Max);
            }
            if (YSecondaryGridLines != null /*&& YPrimaryGridLines != null*/ && Y2GridScale != null)
            {
                if (_YSecondaryGridLines_Internal == null) _YSecondaryGridLines_Internal = new YGridLines_Internal(GridChartArea, Y2GridScale.ScaleType, YSecondaryGridLines.Points, Y2GridScale.Min, Y2GridScale.Max, SecondaryGridLineThickness, ChartObjectTag.YSecondaryGridlines, GridLineStyle);
                else _YSecondaryGridLines_Internal.ReInit(Y2GridScale.ScaleType, YSecondaryGridLines.Points, Y2GridScale.Min, Y2GridScale.Max);
            }
            if (YPrimaryGridLines != null && Y1GridLabels != null && Y1GridScale != null)
            {
                if (_Y1GridLabels_Internal == null) _Y1GridLabels_Internal = new YGridLabels_Internal(this, CanvasYLabelsLeft, Y1GridScale.ScaleType, Y1GridLabels.Points, Y1GridLabels.DataPointsFormat, ChartObjectTag.Y1GridLabels);
                else _Y1GridLabels_Internal.ReInit(Y1GridScale.ScaleType, Y1GridLabels.Points, Y1GridLabels.DataPointsFormat);
            }
            if (YPrimaryGridLines != null && Y2GridLabels != null && Y2GridScale != null)
            {
                if (_Y2GridLabels_Internal == null) _Y2GridLabels_Internal = new YGridLabels_Internal(this, CanvasYLabelsRight, Y2GridScale.ScaleType, Y2GridLabels.Points, Y2GridLabels.DataPointsFormat, ChartObjectTag.Y2GridLabels);
                else _Y2GridLabels_Internal.ReInit(Y2GridScale.ScaleType, Y2GridLabels.Points, Y2GridLabels.DataPointsFormat);
            }

            return result;
        }

        public enum WhichBoundCrossed { None, Left, Right }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="whichDataSeries"></param>
        /// <param name="pointX"></param>
        /// <param name="pointY"></param>
        /// <returns>WhichBoundCrossed (tells if a boundary was crossed)</returns>
        public WhichBoundCrossed CrossPoint(XYDataSeries whichDataSeries, int pointX, double pointY)
        {
            if (whichDataSeries == XY1DataSeries)
            {
                return _XY1DataSeries_Internal.DrawCross(pointX, pointY);
            }
            else if (whichDataSeries == XY2DataSeries)
            {
                return _XY2DataSeries_Internal.DrawCross(pointX, pointY);
            }
            else if (whichDataSeries == XY3DataSeries)
            {
                return _XY3DataSeries_Internal.DrawCross(pointX, pointY);
            }
            else if (whichDataSeries == XY4DataSeries)
            {
                return _XY4DataSeries_Internal.DrawCross(pointX, pointY);
            }
            return WhichBoundCrossed.None;
        }

        public void UncrossPoint(XYDataSeries whichDataSeries)
        {
            if (whichDataSeries == XY1DataSeries)
            {
                _XY1DataSeries_Internal.RemoveCross();
            }
            else if (whichDataSeries == XY2DataSeries)
            {
                _XY2DataSeries_Internal.RemoveCross();
            }
            else if (whichDataSeries == XY3DataSeries)
            {
                _XY3DataSeries_Internal.RemoveCross();
            }
            else if (whichDataSeries == XY4DataSeries)
            {
                _XY4DataSeries_Internal.RemoveCross();
            }
        }

        private static double NormaliseDouble(double input)
        {
            double output = input;
            if (double.IsNaN(input) || double.IsNegativeInfinity(input) || double.IsInfinity(input) || double.IsPositiveInfinity(input)) output = double.MaxValue;
            return output;
        }
    }

    #region internal classes
    internal abstract class ChartObject
    {
        protected Panel Container;
        protected ChartObjectTag Tag;
        internal ChartObject(Panel container, ChartObjectTag tag)
        {
            Container = container;
            Tag = tag;
        }
        protected abstract void Draw();
        internal void ClearChartObjectsWithMyTag()
        {
            for (int i = 0; i < Container.Children.Count; i++)
            {
                FrameworkElement item = Container.Children[i] as FrameworkElement;
                if (item?.Tag is ChartObjectTag && (ChartObjectTag)(item.Tag) == Tag)
                {
                    Container.Children.Remove(item);
                    i -= 1;
                }
            }
        }
    }
    internal abstract class ScalableChartObject : ChartObject
    {
        protected ScaleType ScaleType = ScaleType.Linear;
        internal ScalableChartObject(Panel container, ScaleType scaleType, ChartObjectTag tag)
            : base(container, tag)
        {
            ScaleType = scaleType;
        }
    }
    internal abstract class Axis : ChartObject
    {
        protected Line MyLine;
        protected Orientation _Orientation;
        protected Orientation Orientation { get { return _Orientation; } }

        internal Axis(Panel container, ChartObjectTag tag)
            : base(container, tag)
        {
            MyLine = new Line() { Stroke = new SolidColorBrush(Windows.UI.Colors.Red), StrokeThickness = 2 };
            Container.Children.Add(MyLine);
        }
    }
    internal class XAxis : Axis
    {
        internal XAxis(Panel container, ChartObjectTag tag)
            : base(container, tag)
        {
            _Orientation = Orientation.Horizontal;
            Draw();
        }
        protected override void Draw()
        {
            if (Container.ActualHeight == 0.0 || Container.ActualWidth == 0.0) return;
            MyLine.X1 = 0;
            MyLine.Y1 = MyLine.Y2 = Container.ActualHeight;
            MyLine.X2 = Container.ActualWidth;
        }
    }
    internal class YAxis : Axis
    {
        internal YAxis(Panel container, ChartObjectTag tag)
            : base(container, tag)
        {
            _Orientation = Orientation.Horizontal;
            Draw();
        }
        protected override void Draw()
        {
            if (Container.ActualHeight == 0.0 || Container.ActualWidth == 0.0) return;
            MyLine.X1 = MyLine.X2 = MyLine.Y1 = 0;
            MyLine.Y2 = Container.ActualHeight;
        }
    }
    internal abstract class GridLines_Internal : ScalableChartObject
    {
        protected List<Line> MyLines = new List<Line>();
        protected double[] DataPoints;
        protected double Minimum;
        protected double Maximum;
        protected double Thickness;
        protected Style LineStyle;

        internal GridLines_Internal(Panel container, ScaleType scaleType, double[] points, double minimum, double maximum, double thickness, ChartObjectTag tag, Style lineStyle)
            : base(container, scaleType, tag)
        {
            LineStyle = lineStyle;
            Thickness = thickness;
            ReInit(scaleType, points, minimum, maximum);
        }
        internal virtual void ReInit(ScaleType scaleType, double[] points, double minimum, double maximum)
        {
            if (Container.ActualHeight == 0.0 || Container.ActualWidth == 0.0 || points == null) return;
            if (scaleType != ScaleType || points != DataPoints || minimum != Minimum || maximum != Maximum)
            {
                ScaleType = scaleType;
                DataPoints = points;
                Minimum = minimum;
                Maximum = maximum;
                MyLines.Clear();
                ClearChartObjectsWithMyTag();
                for (int i = 0; i <= DataPoints.GetUpperBound(0); i++)
                {
                    Line line = new Line() { StrokeThickness = Thickness, Tag = Tag, Style = LineStyle };//TEST
                    MyLines.Add(line);
                    Container.Children.Add(line);
                }
            }
            Draw();
        }
    }
    internal class XGridLines_Internal : GridLines_Internal
    {
        internal XGridLines_Internal(Panel container, ScaleType scaleType, double[] points, double minimum, double maximum, double thickness, ChartObjectTag tag, Style lineStyle)
            : base(container, scaleType, points, minimum, maximum, thickness, tag, lineStyle)
        { }
        protected override void Draw()
        {
            if (Container.ActualHeight == 0.0 || Container.ActualWidth == 0.0) return;
            double[] drawPoints = new double[DataPoints.GetUpperBound(0) + 1];
            if (ScaleType == ScaleType.Linear)
            {
                drawPoints = Scaler.ScaleLinear(Minimum, Maximum, 0, Container.ActualWidth, DataPoints);
            }
            else if (ScaleType == ScaleType.Logarithmic)
            {
                drawPoints = Scaler.ScaleLogarithmic(Minimum, Maximum, 0, Container.ActualWidth, DataPoints);
            }
            for (int i = 0; i <= drawPoints.GetUpperBound(0); i++)
            {
                MyLines[i].X1 = MyLines[i].X2 = drawPoints[i];
                MyLines[i].Y1 = Container.ActualHeight;
                MyLines[i].Y2 = 0;
            }
        }
    }
    internal class YGridLines_Internal : GridLines_Internal
    {
        internal YGridLines_Internal(Panel container, ScaleType scaleType, double[] points, double minimum, double maximum, double thickness, ChartObjectTag tag, Style lineStyle)
            : base(container, scaleType, points, minimum, maximum, thickness, tag, lineStyle)
        { }
        protected override void Draw()
        {
            if (Container == null || Container.ActualHeight == 0.0 || Container.ActualWidth == 0.0 || DataPoints == null || MyLines == null) return;
            double[] drawPoints = new double[DataPoints.GetUpperBound(0) + 1];
            if (ScaleType == ScaleType.Linear)
            {
                drawPoints = Scaler.ScaleLinear(Minimum, Maximum, 0, Container.ActualHeight, DataPoints);
            }
            else if (ScaleType == ScaleType.Logarithmic)
            {
                drawPoints = Scaler.ScaleLogarithmic(Minimum, Maximum, 0, Container.ActualHeight, DataPoints);
            }
            for (int i = 0; i <= drawPoints.GetUpperBound(0); i++) //the Panel coordinates start from the top left, while the Y axis starts from the bottom left
            {
                drawPoints[i] = Container.ActualHeight - drawPoints[i];
            }
            for (int i = 0; i <= drawPoints.GetUpperBound(0); i++)
            {
                try
                {
                    MyLines[i].Y1 = MyLines[i].Y2 = drawPoints[i];
                    MyLines[i].X1 = 0;
                    MyLines[i].X2 = Container.ActualWidth;
                }
                catch (ArgumentException)
                { }
            }
        }
    }
    internal abstract class GridLabels_Internal : ScalableChartObject
    {
        protected readonly LolloChart _root = null;
        protected List<TextBlock> MyTBs = new List<TextBlock>();
        //protected List<Label> MyTBs = new List<Label>();
        protected Tuple<double, string>[] DataPoints;
        protected string DataPointsFormat = string.Empty;

        internal GridLabels_Internal(LolloChart root, Panel container, ScaleType scaleType, Tuple<double, string>[] points, string dataPointsFormat, ChartObjectTag tag)
            : base(container, scaleType, tag)
        {
            _root = root;
            ReInit(scaleType, points, dataPointsFormat);
        }
        internal virtual void ReInit(ScaleType scaleType, Tuple<double, string>[] points, string dataPointsFormat)
        {
            if (Container.ActualHeight == 0.0 || Container.ActualWidth == 0.0 || points == null) return;
            if (scaleType != ScaleType || points != DataPoints || dataPointsFormat != DataPointsFormat)
            {
                ScaleType = scaleType;
                DataPoints = points;
                DataPointsFormat = dataPointsFormat;
                MyTBs.Clear();
                ClearChartObjectsWithMyTag();
                for (int i = 0; i <= DataPoints.GetUpperBound(0); i++)
                {
                    TextBlock tb = new TextBlock() { Tag = Tag };
                    MyTBs.Add(tb);
                    Container.Children.Add(tb);
                }
            }

            Draw();
        }
    }
    internal class XGridLabels_Internal : GridLabels_Internal
    {
        internal XGridLabels_Internal(LolloChart root, Panel container, ScaleType scaleType, Tuple<double, string>[] points, string dataPointsFormat, ChartObjectTag tag)
            : base(root, container, scaleType, points, dataPointsFormat, tag)
        { }
        protected override void Draw()
        {
            if (Container == null || Container.ActualHeight == 0.0 || Container.ActualWidth == 0.0 || DataPoints == null || MyTBs == null) return;
            double offset = MyTBs[0].FontSize / 4; //assume font height = twice font width
            double[] drawPoints = new double[DataPoints.GetUpperBound(0) + 1];
            double[] dataPointsDouble = new double[DataPoints.GetUpperBound(0) + 1];
            for (int i = 0; i < DataPoints.Length; i++)
            {
                dataPointsDouble[i] = DataPoints[i].Item1;
            }
            if (ScaleType == ScaleType.Linear)
            {
                drawPoints = Scaler.ScaleLinear(DataPoints.First().Item1, DataPoints.Last().Item1, 0, Container.ActualWidth, dataPointsDouble);
            }
            else if (ScaleType == ScaleType.Logarithmic)
            {
                drawPoints = Scaler.ScaleLogarithmic(DataPoints.First().Item1, DataPoints.Last().Item1, 0, Container.ActualWidth, dataPointsDouble);
            }
            for (int i = 0; i <= drawPoints.GetUpperBound(0); i++)
            {
                try
                {
                    try
                    {
                        if (DataPoints[i].Item2 != null) MyTBs[i].Text = DataPoints[i].Item2;
                        else MyTBs[i].Text = DataPoints[i].Item1.ToString(DataPointsFormat, CultureInfo.CurrentUICulture);
                    }
                    catch (FormatException)
                    {
                        MyTBs[i].Text = DataPoints[i].Item1.ToString(CultureInfo.CurrentUICulture);
                    }
                    double x = drawPoints[i] - (offset * MyTBs[i].Text.Length);
                    double estimatedTBWidth = MyTBs[i].Text.Length * MyTBs[i].FontSize / 2;
                    if (x + estimatedTBWidth > Container.ActualWidth + _root.RightColumnWidth)
                    {
                        x = Container.ActualWidth + _root.RightColumnWidth - estimatedTBWidth; // avoid label exiting the screen right
                        MyTBs[i].FontStretch = Windows.UI.Text.FontStretch.UltraCondensed;
                    }
                    if (x < 0.0 - _root.LeftColumnWidth)
                    {
                        x = 0.0 - _root.LeftColumnWidth; // avoid label exiting the screen left
                        MyTBs[i].FontStretch = Windows.UI.Text.FontStretch.UltraCondensed;
                    }

                    Canvas.SetLeft(MyTBs[i], x);
                    Canvas.SetTop(MyTBs[i], 10.0);
                    Canvas.SetZIndex(MyTBs[i], 999);
                }
                catch (ArgumentException) { }
            }
        }
    }
    internal class YGridLabels_Internal : GridLabels_Internal
    {
        internal YGridLabels_Internal(LolloChart root, Panel container, ScaleType scaleType, Tuple<double, string>[] points, string dataPointsFormat, ChartObjectTag tag)
            : base(root, container, scaleType, points, dataPointsFormat, tag)
        { }
        protected override void Draw()
        {
            if (Container == null || Container.ActualHeight == 0.0 || Container.ActualWidth == 0.0 || DataPoints == null || MyTBs == null) return;
            double offset = MyTBs[0].FontSize / 2;
            double[] drawPoints = new double[DataPoints.GetUpperBound(0) + 1];
            double[] dataPointsDouble = new double[DataPoints.GetUpperBound(0) + 1];
            for (int i = 0; i < DataPoints.Length; i++)
            {
                dataPointsDouble[i] = DataPoints[i].Item1;
            }

            if (ScaleType == ScaleType.Linear)
            {
                drawPoints = Scaler.ScaleLinear(DataPoints.First().Item1, DataPoints.Last().Item1, 0, Container.ActualHeight, dataPointsDouble);
            }
            else if (ScaleType == ScaleType.Logarithmic)
            {
                drawPoints = Scaler.ScaleLogarithmic(DataPoints.First().Item1, DataPoints.Last().Item1, 0, Container.ActualHeight, dataPointsDouble);
            }
            for (int i = 0; i <= drawPoints.GetUpperBound(0); i++) //the Panel coordinates start from the top left, while the Y axis starts from the bottom left
            {
                drawPoints[i] = drawPoints[i] - offset;//account for the font size, too
            }

            for (int i = 0; i <= drawPoints.GetUpperBound(0); i++)
            {
                try
                {
                    try
                    {
                        if (DataPoints[i].Item2 != null) MyTBs[i].Text = DataPoints[i].Item2;
                        else MyTBs[i].Text = DataPoints[i].Item1.ToString(DataPointsFormat, CultureInfo.CurrentUICulture);
                    }
                    catch (FormatException)
                    {
                        MyTBs[i].Text = DataPoints[i].Item1.ToString(CultureInfo.CurrentUICulture);
                    }
                    Canvas.SetLeft(MyTBs[i], 0.0);
                    Canvas.SetTop(MyTBs[i], drawPoints[i]);
                    Canvas.SetZIndex(MyTBs[i], 999);
                }
                catch (ArgumentException) { }
            }
        }
    }

    internal class XYDataSeries_Internal : ChartObject
    {
        protected bool _isHistogram = false;
        protected ScaleType ScaleTypeX;
        protected ScaleType ScaleTypeY;
        protected double X0 = 0;
        protected double XN = 0;
        protected double Y0 = 0;
        protected double YN = 0;
        protected double[,] DataPoints;
        internal bool HasPoints { get { var dp = DataPoints; return dp != null && dp.GetUpperBound(0) > 0; } }
        protected Polyline MyPolyline = new Polyline();
        internal enum StrokeDashArrays { Dashed, Continue }
        protected static DoubleCollection Dashed = new DoubleCollection();
        protected int CrossPointX = -1; // negative means, do not draw the cross
        protected double CrossPointY = default(double);
        protected Ellipse Cross = new Ellipse() { Width = 20, Height = 20, Stroke = new SolidColorBrush(Windows.UI.Colors.Red), StrokeThickness = 4 };
        protected Canvas CrossCanvas = new Canvas();

        internal XYDataSeries_Internal(Panel container, ScaleType scaleTypeX, ScaleType scaleTypeY, double x0, double xN, double y0, double yN, double[,] points,
            ChartObjectTag tag, Style lineStyle, bool isHistogram = false, StrokeDashArrays sda = StrokeDashArrays.Continue)
            : base(container, tag)
        {
            _isHistogram = isHistogram;

            if (Dashed.Count == 0) { Dashed.Add(2); Dashed.Add(2); }

            MyPolyline.Style = lineStyle;
            MyPolyline.StrokeMiterLimit = 0.0;
            MyPolyline.StrokeThickness = LolloChart.DataSeriesThickness;
            switch (sda)
            {
                case StrokeDashArrays.Dashed:
                    MyPolyline.StrokeDashArray = Dashed;
                    break;
                case StrokeDashArrays.Continue:
                    MyPolyline.StrokeDashArray = null;
                    break;
                default:
                    break;
            }
            Container.Children.Add(MyPolyline);
            Container.Children.Add(CrossCanvas);
            ReInit(scaleTypeX, scaleTypeY, x0, xN, y0, yN, points);
        }

        internal void ReInit(ScaleType scaleTypeX, ScaleType scaleTypeY, double x0, double xN, double y0, double yN, double[,] points)
        {
            ScaleTypeX = scaleTypeX; ScaleTypeY = scaleTypeY;
            X0 = x0; XN = xN; Y0 = y0; YN = yN;
            DataPoints = points;
            Draw();
        }
        private void DrawInternal(double[,] dataPointsToBeDrawn)
        {
            if (dataPointsToBeDrawn == null || Container.ActualHeight == 0.0 || Container.ActualWidth == 0.0) return;
#if DEBUG
            Stopwatch sw0 = new Stopwatch(); sw0.Start();
#endif
            double[,] drawPoints = GetDrawPoints(dataPointsToBeDrawn);
#if DEBUG
            sw0.Stop();
            Debug.WriteLine("XYDataSeries_Internal took " + sw0.ElapsedMilliseconds + " msec to calc the polyline data");
            sw0.Restart();
#endif
            MyPolyline.Points.Clear();
            if (_isHistogram)
            {
                int maxI = drawPoints.GetUpperBound(0);

                double halfHistoWidth = maxI > 0 ? Container.ActualWidth * .5 / Convert.ToDouble(maxI) : Container.ActualWidth * .5;

                if (maxI == 0) // the point would be at mid width
                {
                    MyPolyline.Points.Add(new Point(drawPoints[0, 0] - halfHistoWidth, drawPoints[0, 1]));
                    MyPolyline.Points.Add(new Point(drawPoints[0, 0] + halfHistoWidth, drawPoints[0, 1]));
                }
                else // the leftmost point is at x = 0, the rightmost at x = ActualWidth, the others spaced inbetween
                {
                    MyPolyline.Points.Add(new Point(drawPoints[0, 0], drawPoints[0, 1]));
                    MyPolyline.Points.Add(new Point(drawPoints[0, 0] + halfHistoWidth, drawPoints[0, 1]));
                    for (int i = 1; i < maxI; i++)
                    {
                        MyPolyline.Points.Add(new Point(drawPoints[i, 0] - halfHistoWidth, drawPoints[i, 1]));
                        MyPolyline.Points.Add(new Point(drawPoints[i, 0] + halfHistoWidth, drawPoints[i, 1]));
                    }
                    if (maxI > 0)
                    {
                        MyPolyline.Points.Add(new Point(drawPoints[maxI, 0] - halfHistoWidth, drawPoints[maxI, 1]));
                        MyPolyline.Points.Add(new Point(drawPoints[maxI, 0], drawPoints[maxI, 1]));
                    }
                }
            }
            else
            {
                for (int i = 0; i <= drawPoints.GetUpperBound(0); i++)
                {
                    MyPolyline.Points.Add(new Point(drawPoints[i, 0], drawPoints[i, 1]));
                }
            }
#if DEBUG
            sw0.Stop();
            Debug.WriteLine("XYDataSeries_Internal took " + sw0.ElapsedMilliseconds + " msec to draw the polyline");
#endif
            DrawCrossInternal();
        }
        private double[,] GetDrawPoints(double[,] dataPointsToBeDrawn)
        {
            double[,] drawPoints = new double[dataPointsToBeDrawn.GetUpperBound(0) + 1, dataPointsToBeDrawn.GetUpperBound(1) + 1];
            if (ScaleTypeX == ScaleType.Linear)
            {
                drawPoints = Scaler.ScaleLinear(X0, XN, 0, Container.ActualWidth, dataPointsToBeDrawn, 0);
            }
            else if (ScaleTypeX == ScaleType.Logarithmic)
            {
                drawPoints = Scaler.ScaleLogarithmic(X0, XN, 0, Container.ActualWidth, dataPointsToBeDrawn, 0);
            }
            if (ScaleTypeY == ScaleType.Linear)
            {
                drawPoints = Scaler.ScaleLinear(Y0, YN, 0, Container.ActualHeight, drawPoints, 1);
            }
            else if (ScaleTypeY == ScaleType.Logarithmic)
            {
                drawPoints = Scaler.ScaleLogarithmic(Y0, YN, 0, Container.ActualHeight, drawPoints, 1);
            }
            for (int i = 0; i <= drawPoints.GetUpperBound(0); i++) //the Panel coordinates start from the top left, while the Y axis starts from the bottom left
            {
                drawPoints[i, 1] = Container.ActualHeight - drawPoints[i, 1];
            }
            return drawPoints;
        }
        protected override void Draw()
        {
            if (DataPoints == null || Container.ActualHeight == 0.0 || Container.ActualWidth == 0.0) return;

            int sourceArraySize = DataPoints.GetUpperBound(0) + 1;
            int targetArraySize = Math.Min(Convert.ToInt32(Container.ActualWidth * 2), sourceArraySize);

            if (targetArraySize < sourceArraySize)
            {
                var shrunkDataPoints = GetShrunkDataPoints(sourceArraySize, targetArraySize);
                DrawInternal(shrunkDataPoints);
            }
            else
            {
                DrawInternal(DataPoints);
            }
        }

        private double[,] GetShrunkDataPoints(int sourceArraySize, int targetArraySize)
        {
            if (sourceArraySize < 1) return DataPoints;
            double[,] shrunkDataPoints = new double[targetArraySize, 2];
            double shrinkFactor = Convert.ToDouble(targetArraySize) / Convert.ToDouble(sourceArraySize);

            int lastCheckedShrunkIndex = -3;
            double min0 = double.PositiveInfinity;
            double min1 = double.PositiveInfinity;
            double max0 = double.NegativeInfinity;
            double max1 = double.NegativeInfinity;
            for (int sourceIndex = 0; sourceIndex < sourceArraySize; sourceIndex++)
            {
                int shrunkIndex = Convert.ToInt32(Math.Floor(sourceIndex * shrinkFactor));
                if (shrunkIndex == lastCheckedShrunkIndex || shrunkIndex == (lastCheckedShrunkIndex + 1))
                {
                    min0 = Math.Min(min0, DataPoints[sourceIndex, 0]);
                    min1 = Math.Min(min1, DataPoints[sourceIndex, 1]);
                    max0 = Math.Max(max0, DataPoints[sourceIndex, 0]);
                    max1 = Math.Max(max1, DataPoints[sourceIndex, 1]);
                }
                else
                {
                    min0 = DataPoints[sourceIndex, 0];
                    min1 = DataPoints[sourceIndex, 1];
                    max0 = DataPoints[sourceIndex, 0];
                    max1 = DataPoints[sourceIndex, 1];
                }

                int nextShrunkIndex = Convert.ToInt32(Math.Floor((sourceIndex + 1) * shrinkFactor));
                if ((nextShrunkIndex != shrunkIndex && nextShrunkIndex == (lastCheckedShrunkIndex + 2)) || sourceIndex == sourceArraySize - 1) // last of the batch
                {
                    shrunkDataPoints[shrunkIndex - 1, 0] = min0;
                    shrunkDataPoints[shrunkIndex - 1, 1] = min1;
                    shrunkDataPoints[shrunkIndex, 0] = max0;
                    shrunkDataPoints[shrunkIndex, 1] = max1;
                }

                if (shrunkIndex % 2 == 0) lastCheckedShrunkIndex = shrunkIndex;
            }

            return shrunkDataPoints;
        }

        protected void DrawCrossInternal()
        {
            if (DataPoints == null || Container.ActualHeight == 0.0 || Container.ActualWidth == 0.0) return;
            if (CrossPointX >= 0)
            {
                Point crossPoint;
                int crossPointXAdjusted4Shrinking = Convert.ToInt32(
                    Math.Floor(
                    Convert.ToDouble(CrossPointX) / Convert.ToDouble(DataPoints.GetUpperBound(0) + 1) * Convert.ToDouble(MyPolyline.Points.Count)
                    ));
                // get the y coordinate and ignore the x
                double[,] drawPoints = GetDrawPoints(new double[,] { { Convert.ToDouble(crossPointXAdjusted4Shrinking), CrossPointY } });
                if (_isHistogram)
                {
                    crossPoint = new Point((MyPolyline.Points[crossPointXAdjusted4Shrinking].X + MyPolyline.Points[crossPointXAdjusted4Shrinking + 1].X) * .5,
                        drawPoints[0, 1]);
                }
                else
                {
                    crossPoint.X = MyPolyline.Points[crossPointXAdjusted4Shrinking].X;
                    crossPoint.Y = drawPoints[0, 1];
                }

                if (!CrossCanvas.Children.Contains(Cross))
                {
                    CrossCanvas.Children.Add(Cross);
                }
                Canvas.SetLeft(Cross, crossPoint.X - Cross.Width / 2);
                Canvas.SetTop(Cross, crossPoint.Y - Cross.Height / 2);
                Canvas.SetZIndex(Cross, 999);
            }
            else
            {
                CrossCanvas.Children.Clear();
            }
        }

        internal LolloChart.WhichBoundCrossed DrawCross(int pointX, double pointY)
        {
            //if (whichPoint >= 0 && whichPoint < MyPolyline.Points.Count) // no more valid, I may have shrunk the points
            var whichBoundCrossed = LolloChart.WhichBoundCrossed.None;
            if (pointX >= 0 && pointX <= DataPoints.GetUpperBound(0))
            {
                CrossPointX = pointX;
                CrossPointY = pointY;
            }
            else
            {
                if (pointX >= 0) whichBoundCrossed = LolloChart.WhichBoundCrossed.Right;
                else whichBoundCrossed = LolloChart.WhichBoundCrossed.Left;
                CrossPointX = -1;
                CrossPointY = default(double);
            }
            DrawCrossInternal();
            if (whichBoundCrossed != LolloChart.WhichBoundCrossed.None) Debug.WriteLine("---------- whichBoundCrossed = " + whichBoundCrossed);
            return whichBoundCrossed;
        }

        internal void RemoveCross()
        {
            DrawCross(-1, default(double));
        }
    }
    #endregion internal classes

    #region public classes
    public sealed class GridLines
    {
        private readonly double[] _Points;
        public double[] Points { get { return _Points; } }
        public GridLines(double[] points)
        {
            _Points = points;
        }
    }
    public sealed class GridLabels
    {
        private Tuple<double, string>[] _Points;
        public Tuple<double, string>[] Points { get { return _Points; } set { _Points = value; } }
        private string _dataPointsFormat = string.Empty;
        public string DataPointsFormat { get { return _dataPointsFormat; } set { _dataPointsFormat = value; } }
        public GridLabels(Tuple<double, string>[] points, string dataPointsFormat)
        {
            _Points = points;
            _dataPointsFormat = dataPointsFormat;
        }
    }
    public sealed class GridScale
    {
        private double _Min;
        public double Min { get { return _Min; } set { _Min = value; } }
        private double _Max;
        public double Max { get { return _Max; } set { _Max = value; } }
        private readonly ScaleType _ScaleType;
        public ScaleType ScaleType { get { return _ScaleType; } }
        public GridScale(ScaleType scaleType, double min, double max)
        {
            _ScaleType = scaleType;
            _Min = min;
            _Max = max;
        }
    }
    public sealed class XYDataSeries
    {
        private readonly bool _isHistogram = false;
        public bool IsHistogram { get { return _isHistogram; } }
        private double[,] _Points;
        public double[,] Points { get { return _Points; } set { _Points = value; } }
        public XYDataSeries(double[,] points, bool isHistogram, int xColumn = 0, int yColumn = 1) // where T : class, new()
        {
            _isHistogram = isHistogram;
            if (points != null)
            {
                int n = points.GetUpperBound(0);
                _Points = new double[n + 1, 2];
                for (int i = 0; i <= n; i++)
                {
                    _Points[i, 0] = points[i, xColumn];
                    _Points[i, 1] = points[i, yColumn];
                }
            }
            else
            {
                _Points = null;
            }
        }
    }
    #endregion public classes

    #region services
    internal static class Scaler
    {
        internal static double[] ScaleLinear(double sourceValue0, double sourceValueN, double targetValue0, double targetValueN, double[] values)
        {
            double[] output = new double[values.GetUpperBound(0) + 1];
            if (sourceValueN != sourceValue0)
            {
                double targetRatio = (targetValueN - targetValue0) / (sourceValueN - sourceValue0);
                for (int i = 0; i <= values.GetUpperBound(0); i++)
                {
                    output[i] = (values[i] - sourceValue0) * targetRatio + targetValue0;
                }
            }
            else
            {
                double averageTarget = (targetValue0 + targetValueN) / 2;
                for (int i = 0; i <= values.GetUpperBound(0); i++)
                {
                    output[i] = averageTarget;
                }
            }
            return output;
        }
        internal static double[] ScaleLogarithmic(double sourceValue0, double sourceValueN, double targetValue0, double targetValueN, double[] values)
        {
            double[] output = new double[values.GetUpperBound(0) + 1];
            double logBase = LolloChart.LogBase;
            double sourceValue0_log = Math.Log(sourceValue0, logBase);
            double sourceValueN_log = Math.Log(sourceValueN, logBase);

            for (int i = 0; i <= values.GetUpperBound(0); i++)
            {
                output[i] = (Math.Log(values[i], logBase));
            }
            return ScaleLinear(sourceValue0_log, sourceValueN_log, targetValue0, targetValueN, output);
        }
        internal static double[,] ScaleLinear(double sourceValue0, double sourceValueN, double targetValue0, double targetValueN, double[,] values, int column)
        {
            double[,] output = new double[values.GetUpperBound(0) + 1, values.GetUpperBound(1) + 1];
            if (sourceValueN != sourceValue0)
            {
                double targetRatio = (targetValueN - targetValue0) / (sourceValueN - sourceValue0);
                for (int i = 0; i <= values.GetUpperBound(0); i++)
                {
                    output[i, column] = (values[i, column] - sourceValue0) * targetRatio + targetValue0;
                    output[i, 1 - column] = (values[i, 1 - column]);
                }
            }
            else
            {
                double averageTarget = (targetValue0 + targetValueN) / 2;
                for (int i = 0; i <= values.GetUpperBound(0); i++)
                {
                    output[i, column] = averageTarget;
                    output[i, 1 - column] = (values[i, 1 - column]);
                }
            }
            return output;
        }
        internal static double[,] ScaleLogarithmic(double sourceValue0, double sourceValueN, double targetValue0, double targetValueN, double[,] values, int column)
        {
            double[,] output = new double[values.GetUpperBound(0) + 1, values.GetUpperBound(1) + 1];
            double logBase = LolloChart.LogBase;
            double sourceValue0_log = Math.Log(sourceValue0, logBase);
            double sourceValueN_log = Math.Log(sourceValueN, logBase);

            for (int i = 0; i <= values.GetUpperBound(0); i++)
            {
                output[i, column] = (Math.Log(values[i, column], logBase));
                output[i, 1 - column] = (values[i, 1 - column]);
            }
            return ScaleLinear(sourceValue0_log, sourceValueN_log, targetValue0, targetValueN, output, column);
        }
    }
    public enum ScaleType { Linear, Logarithmic }
    internal enum ChartObjectTag { XAxis, YAxis, XY1DataSeries, XY2DataSeries, XY3DataSeries, XY4DataSeries, XPrimaryGridlines, XSecondaryGridlines, XGridLabels, YPrimaryGridlines, YSecondaryGridlines, Y1GridLabels, Y2GridLabels }
    #endregion services
}