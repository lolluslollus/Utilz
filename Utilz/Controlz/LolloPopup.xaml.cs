using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Utilz.Data;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236
// LOLLO NOTE multiselect has a problem: https://stackoverflow.com/questions/43873431/uwp-how-to-deal-with-multiple-selections

namespace Utilz.Controlz
{
    public sealed partial class LolloPopup : BackOrientOpenObservControl
    {
        #region properties
        private const string DefaultPlaceholderText = "Select an item";
        private const string DefaultListHeaderText = "Choose an item";

        public FrameworkElement PopupContainer
        {
            get { return (FrameworkElement)GetValue(PopupContainerProperty); }
            set { SetValue(PopupContainerProperty, value); }
        }
        public static readonly DependencyProperty PopupContainerProperty =
            DependencyProperty.Register("PopupContainer", typeof(FrameworkElement), typeof(LolloPopup), new PropertyMetadata(Window.Current.Content, OnPopupContainer_PropertyChanged));
        private static void OnPopupContainer_PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var me = obj as LolloPopup;
            if (me == null) return;

            var oldValue = e.OldValue as FrameworkElement;
            if (oldValue != null)
            {
                oldValue.SizeChanged -= me.OnPopupContainer_SizeChanged;
            }
            var newValue = e.NewValue as FrameworkElement;
            if (newValue != null)
            {
                newValue.SizeChanged += me.OnPopupContainer_SizeChanged;
            }
        }

        private void OnPopupContainer_SizeChanged(object sender, SizeChangedEventArgs args)
        {
            //if (args.NewSize.Height == args.PreviousSize.Height && args.NewSize.Width == args.PreviousSize.Width) return; // useless
            Debug.WriteLine($"OnPopupCOntainer_SizeChanged; new height = {args.NewSize.Height}; new width = {args.NewSize.Width}");
            if (!IsPopupOpen) return;
            UpdateMyOpenPopup();
        }

        public bool IsPopupOpen
        {
            get { return (bool)GetValue(IsPopupOpenProperty); }
            set { SetValue(IsPopupOpenProperty, value); }
        }
        public static readonly DependencyProperty IsPopupOpenProperty =
            DependencyProperty.Register("IsPopupOpen", typeof(bool), typeof(LolloPopup), new PropertyMetadata(false, OnIsPopupOpen_PropertyChanged));
        private static void OnIsPopupOpen_PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var me = obj as LolloPopup;
            if (me == null) return;

            bool newValue = (bool)(e.NewValue);
            if (newValue) me.UpdateMyOpenPopup();
            else me.UpdateMyClosedPopup();
        }

        public FrameworkElement PopupChild
        {
            get { return (FrameworkElement)GetValue(ChildProperty); }
            set { SetValue(ChildProperty, value); }
        }
        public static readonly DependencyProperty ChildProperty =
            DependencyProperty.Register("PopupChild", typeof(FrameworkElement), typeof(LolloPopup), new PropertyMetadata(null));
        private static void OnChild_PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var me = obj as LolloPopup;
            if (me == null) return;
            if (me.IsPopupOpen) me.UpdateMyOpenPopup();
            else me.UpdateMyClosedPopup();
        }
        #endregion properties

        #region construct and dispose
        public LolloPopup() : base()
        {
            InitializeComponent();
        }
        #endregion construct and dispose

        #region event handlers
        protected override void OnVisibleBoundsChangedMayOverride(ApplicationView sender, object args)
        {
            var availableBoundsWithinChrome = sender.VisibleBounds;
            var same = AppView.VisibleBounds;
            //base.OnVisibleBoundsChangedMayOverride(sender, args); //useless
        }
        //protected override Task OnLoadedMayOverrideAsync()
        //{
        //    if (IsPopupOpen) UpdateMyOpenPopup();
        //    else UpdateMyClosedPopup();
        //    return base.OnLoadedMayOverrideAsync();
        //}
        protected override void OnHardwareOrSoftwareButtons_BackPressed_MayOverride(object sender, BackOrHardSoftKeyPressedEventArgs e)
        {
            if (!IsPopupOpen) return;

            if (e != null) e.Handled = true;
            IsPopupOpen = false;
        }

        private void OnMyPopup_Closed(object sender, object e)
        {
            IsPopupOpen = false;
        }

        private void OnPointerCanceled(object sender, PointerRoutedEventArgs e)
        {

        }

        private void OnPointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {

        }

        private void OnPointerEntered(object sender, PointerRoutedEventArgs e)
        {

        }

        private void OnPointerExited(object sender, PointerRoutedEventArgs e)
        {

        }

        private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {

        }
        //private void OnFullScreenCanvas_Tapped(object sender, TappedRoutedEventArgs e)
        //{

        //}

        //private void OnFullScreenCanvas_GotFocus(object sender, RoutedEventArgs e)
        //{

        //}

        //private void OnFullScreenCanvas_PointerReleased(object sender, PointerRoutedEventArgs e)
        //{

        //}

        //private void OnFullScreenCanvas_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        //{

        //}
        #endregion event handlers

        #region update
        /// <summary>
        /// Only call this in the IsPopupOpen change handler.
        /// Otherwise, change the dependency property IsPopupOpen.
        /// </summary>
        private void UpdateMyOpenPopup()
        {
            //// update full screen canvas 
            //// update size
            //FullScreenCanvas.Height = AppView.VisibleBounds.Height;
            //FullScreenCanvas.Width = AppView.VisibleBounds.Width;
            //// update placement
            ////var window = Window.Current.Content as FrameworkElement;

            //var fullScreenCanvasTransform = TransformToVisual(Window.Current.Content);
            //var fullScreenCanvasRelativePoint = fullScreenCanvasTransform.TransformPoint(new Point(0.0, 0.0));
            //Canvas.SetLeft(FullScreenCanvas, -fullScreenCanvasRelativePoint.X);
            //Canvas.SetTop(FullScreenCanvas, -fullScreenCanvasRelativePoint.Y);
            //// visible
            //FullScreenCanvas.Visibility = Visibility.Visible;

            // update my popup
            // update theme
            var currentPage = (Window.Current.Content as Frame)?.Content as Page;
            if (currentPage != null) MyPopup.RequestedTheme = currentPage.RequestedTheme;
            // update size
            if (PopupChild != null)
            {
                PopupChild.Height = PopupContainer.ActualHeight;
                PopupChild.Width = PopupContainer.ActualWidth;
            }
            // update placement
            var myPopupTransform = TransformToVisual(PopupContainer);
            var myPopupRelativePoint = myPopupTransform.TransformPoint(new Point(0.0, 0.0));
            Canvas.SetLeft(MyPopup, -myPopupRelativePoint.X);
            Canvas.SetTop(MyPopup, -myPopupRelativePoint.Y);
            // update popup child
            MyPopup.Child = PopupChild;
            // open
            MyPopup.IsOpen = true; // only change this property in the IsPopupOpen change handler. Otherwise, change the dependency property IsPopupOpen.
            // focus on a popup child textbox, if any
            ((PopupChild as Panel)?.Children?.FirstOrDefault() as Control)?.Focus(FocusState.Keyboard);
        }

        /// <summary>
        /// Only call this in the IsPopupOpen change handler.
        /// Otherwise, change the dependency property IsPopupOpen.
        /// </summary>
        private void UpdateMyClosedPopup()
        {
            //FullScreenCanvas.Visibility = Visibility.Collapsed;
            MyPopup.IsOpen = false; // only change this property in the IsPopupOpen change handler. Otherwise, change the dependency property IsPopupOpen.
        }
        #endregion update
    }
}