using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Utilz.Controlz
{
	public sealed class TextBlockScaler : UserControl
	{
		private long _onTextChangedToken = default(long);
		private double _origFontSize = default(double);
		private const double SAFETY_FACTOR = 0.9;

		public TextBlock TextBlock
		{
			get { return (TextBlock)GetValue(TextBlockProperty); }
			set { SetValue(TextBlockProperty, value); }
		}
		public static readonly DependencyProperty TextBlockProperty = DependencyProperty.Register(
			"TextBlock", typeof(TextBlock), typeof(TextBlockScaler), new PropertyMetadata(new TextBlock(), OnTextBlockChanged));


		#region event handlers
		private static void OnTextBlockChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
		{
			var instance = obj as TextBlockScaler;
			if (instance != null && args != null && args.NewValue != args.OldValue)
			{
				var oldTb = args.OldValue as TextBlock;
				if (oldTb != null)
				{
					if (instance._onTextChangedToken != 0) oldTb.UnregisterPropertyChangedCallback(TextBlock.TextProperty, instance._onTextChangedToken);
					oldTb.SizeChanged -= instance.OnTb_SizeChanged;
				}

				var newTb = args.NewValue as TextBlock;
				if (newTb != null)
				{
					newTb.Height = instance.Height;
					newTb.MaxHeight = instance.MaxHeight;
					newTb.MinHeight = instance.MinHeight;
					newTb.Width = instance.Width;
					newTb.MaxWidth = instance.Width;
					newTb.MinWidth = instance.MinWidth;

					instance._origFontSize = newTb.FontSize;
					instance.Content = newTb;

					instance._onTextChangedToken = newTb.RegisterPropertyChangedCallback(TextBlock.TextProperty, instance.OnTextChanged);
					newTb.SizeChanged += instance.OnTb_SizeChanged;
				}
				else
				{
					instance._origFontSize = default(double);
					instance.Content = null;
				}
			}
		}

		private void OnTb_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (e.NewSize.Equals(e.PreviousSize)) return;
			UpdateTB();
		}

		private void OnTextChanged(DependencyObject obj, DependencyProperty prop)
		{
			UpdateTB();
		}
		#endregion event handlers

		private void UpdateTB()
		{
			if (TextBlock.ActualHeight > 0 && TextBlock.ActualWidth > 0 && !string.IsNullOrEmpty(TextBlock.Text))
			{
				double testFontSize = Math.Sqrt((ActualHeight - TextBlock.Padding.Top - Padding.Top - TextBlock.Padding.Bottom - Padding.Bottom)
					* (ActualWidth - TextBlock.Padding.Left - Padding.Left - TextBlock.Padding.Right - Padding.Right) / TextBlock.Text.Length)
					* SAFETY_FACTOR;
				TextBlock.FontSize = testFontSize < _origFontSize ? testFontSize : _origFontSize;
			}
		}
	}
}
