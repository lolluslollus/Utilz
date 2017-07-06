using System;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Utilz.Controlz
{
    public sealed class TextBlockScaler : UserControl
    {
        private readonly long _maxWidthChangedToken = default(long);
        private readonly long _maxHeightChangedToken = default(long);
        private long _textBlockChangedToken = default(long);
        private double _origFontSize = default(double);
        private const double SAFETY_FACTOR = 0.9;

        #region properties
        public TextBlock TextBlock
        {
            get { return (TextBlock)GetValue(TextBlockProperty); }
            set { SetValue(TextBlockProperty, value); }
        }
        public static readonly DependencyProperty TextBlockProperty = DependencyProperty.Register(
            "TextBlock", typeof(TextBlock), typeof(TextBlockScaler), new PropertyMetadata(new TextBlock(), OnTextBlockChanged));
        #endregion properties

        #region lifecycle
        public TextBlockScaler() : base()
        {
            IsTabStop = false;
            _maxHeightChangedToken = RegisterPropertyChangedCallback(MaxHeightProperty, OnMaxHeightChanged);
            _maxWidthChangedToken = RegisterPropertyChangedCallback(MaxWidthProperty, OnMaxWidthChanged);
        }
        #endregion lifecycle

        #region event handlers
        private static void OnTextBlockChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var instance = obj as TextBlockScaler;
            if (instance == null || args == null || args.NewValue == args.OldValue) return;

            var oldTb = args.OldValue as TextBlock;
            if (oldTb != null)
            {
                if (instance._textBlockChangedToken != 0) oldTb.UnregisterPropertyChangedCallback(TextBlock.TextProperty, instance._textBlockChangedToken);
                oldTb.SizeChanged -= instance.OnTb_SizeChanged;
            }

            var newTb = args.NewValue as TextBlock;
            if (newTb != null)
            {
                newTb.Height = instance.Height;
                newTb.MaxHeight = instance.MaxHeight;
                newTb.MinHeight = instance.MinHeight;
                newTb.Width = instance.Width;
                newTb.MaxWidth = double.IsNaN(instance.Width) ? double.PositiveInfinity : instance.Width;
                newTb.MinWidth = instance.MinWidth;

                instance._origFontSize = newTb.FontSize;
                instance.Content = newTb;

                instance._textBlockChangedToken = newTb.RegisterPropertyChangedCallback(TextBlock.TextProperty, instance.OnTextBlockChanged);
                newTb.SizeChanged += instance.OnTb_SizeChanged;
            }
            else
            {
                instance._origFontSize = default(double);
                instance.Content = null;
            }
        }

        private void OnTb_SizeChanged(object sender, SizeChangedEventArgs args)
        {
            if (args.NewSize.Equals(args.PreviousSize)) return;
            UpdateTB();
        }

        private void OnTextBlockChanged(DependencyObject obj, DependencyProperty prop)
        {
            UpdateTB();
        }

        private void OnMaxHeightChanged(DependencyObject obj, DependencyProperty prop)
        {
            UpdateTB();
        }
        private void OnMaxWidthChanged(DependencyObject obj, DependencyProperty prop)
        {
            UpdateTB();
        }
        #endregion event handlers

        #region core
        private void UpdateTB()
        {
            if ((TextBlock == null) || TextBlock.ActualHeight <= 0.0 || TextBlock.ActualWidth <= 0.0 || string.IsNullOrEmpty(TextBlock.Text)) return;

            double height = ActualHeight;
            double width = ActualWidth;
            if (!double.IsNaN(MaxHeight) && !double.IsInfinity(MaxHeight)) height = MaxHeight;
            if (!double.IsNaN(MaxWidth) && !double.IsInfinity(MaxWidth)) width = MaxWidth;

            double testFontSize = Math.Sqrt((height - TextBlock.Padding.Top - Padding.Top - TextBlock.Padding.Bottom - Padding.Bottom)
                                            * (width - TextBlock.Padding.Left - Padding.Left - TextBlock.Padding.Right - Padding.Right) / TextBlock.Text.Length)
                                  * SAFETY_FACTOR;
            TextBlock.FontSize = testFontSize < _origFontSize ? testFontSize : _origFontSize;
        }
        #endregion core
    }
}
