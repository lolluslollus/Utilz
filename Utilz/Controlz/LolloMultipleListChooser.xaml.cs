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
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236
// LOLLO NOTE multiselect has a problem: https://stackoverflow.com/questions/43873431/uwp-how-to-deal-with-multiple-selections

namespace Utilz.Controlz
{
    public sealed partial class LolloMultipleListChooser : BackOrientOpenObservControl
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
            DependencyProperty.Register("PopupContainer", typeof(FrameworkElement), typeof(LolloMultipleListChooser), new PropertyMetadata(Window.Current.Content));

        public Visibility SelectorVisibility
        {
            get { return (Visibility)GetValue(SelectorVisibilityProperty); }
            set { SetValue(SelectorVisibilityProperty, value); }
        }
        public static readonly DependencyProperty SelectorVisibilityProperty =
            DependencyProperty.Register("SelectorVisibility", typeof(Visibility), typeof(LolloMultipleListChooser), new PropertyMetadata(Visibility.Visible));

        public bool IsPopupOpen
        {
            get { return (bool)GetValue(IsPopupOpenProperty); }
            set { SetValue(IsPopupOpenProperty, value); }
        }
        public static readonly DependencyProperty IsPopupOpenProperty =
            DependencyProperty.Register("IsPopupOpen", typeof(bool), typeof(LolloMultipleListChooser), new PropertyMetadata(false));

        public string PlaceholderText
        {
            get { return (string)GetValue(PlaceholderTextProperty); }
            set { SetValue(PlaceholderTextProperty, value); }
        }
        public static readonly DependencyProperty PlaceholderTextProperty =
            DependencyProperty.Register("PlaceholderText", typeof(string), typeof(LolloMultipleListChooser), new PropertyMetadata(DefaultPlaceholderText, OnPlaceholderText_PropertyChanged));
        private static void OnPlaceholderText_PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            LolloMultipleListChooser me = obj as LolloMultipleListChooser;
            if (me == null) return;

            string newValue = e.NewValue as string;
            me.ClosedDescriptorTextBlock.Text = newValue ?? string.Empty;
        }

        public string ListHeaderText
        {
            get { return (string)GetValue(ListHeaderTextProperty); }
            set { SetValue(ListHeaderTextProperty, value); }
        }
        public static readonly DependencyProperty ListHeaderTextProperty =
            DependencyProperty.Register("ListHeaderText", typeof(string), typeof(LolloMultipleListChooser), new PropertyMetadata(DefaultListHeaderText));

        public Style TextBlockStyle
        {
            get { return (Style)GetValue(TextBlockStyleProperty); }
            set { SetValue(TextBlockStyleProperty, value); }
        }
        public static readonly DependencyProperty TextBlockStyleProperty =
            DependencyProperty.Register("TextBlockStyle", typeof(Style), typeof(LolloMultipleListChooser), new PropertyMetadata(null));

        public Style AppBarButtonStyle
        {
            get { return (Style)GetValue(AppBarButtonStyleProperty); }
            set { SetValue(AppBarButtonStyleProperty, value); }
        }
        public static readonly DependencyProperty AppBarButtonStyleProperty =
            DependencyProperty.Register("AppBarButtonStyle", typeof(Style), typeof(LolloMultipleListChooser), new PropertyMetadata(null));

        public Style TextItemStyle
        {
            get { return (Style)GetValue(TextItemStyleProperty); }
            set { SetValue(TextItemStyleProperty, value); }
        }
        public static readonly DependencyProperty TextItemStyleProperty =
            DependencyProperty.Register("TextItemStyle", typeof(Style), typeof(LolloMultipleListChooser), new PropertyMetadata(null));

        public Style ListHeaderStyle
        {
            get { return (Style)GetValue(ListHeaderStyleProperty); }
            set { SetValue(ListHeaderStyleProperty, value); }
        }
        public static readonly DependencyProperty ListHeaderStyleProperty =
            DependencyProperty.Register("ListHeaderStyle", typeof(Style), typeof(LolloMultipleListChooser), new PropertyMetadata(null));

        public Collection<TextAndTag> ItemsSource
        {
            get { return (Collection<TextAndTag>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(Collection<TextAndTag>), typeof(LolloMultipleListChooser), new PropertyMetadata(null)); //, OnItemsSource_PropertyChanged));

        public List<TextAndTag> SelectedItems
        {
            get { return (List<TextAndTag>)GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register("SelectedItems", typeof(List<TextAndTag>), typeof(LolloMultipleListChooser), new PropertyMetadata(new List<TextAndTag>(), OnSelectedItems_PropertyChanged));
        private static void OnSelectedItems_PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var me = obj as LolloMultipleListChooser;
            if (me == null) return;

            me.UpdateDescriptor();
            me.UpdateSelection();
        }

        public bool IsMultiSelectCheckBoxEnabled
        {
            get { return (bool)GetValue(IsMultiSelectCheckBoxEnabledProperty); }
            set { SetValue(IsMultiSelectCheckBoxEnabledProperty, value); }
        }
        public static readonly DependencyProperty IsMultiSelectCheckBoxEnabledProperty =
            DependencyProperty.Register("IsMultiSelectCheckBoxEnabled", typeof(bool), typeof(LolloMultipleListChooser), new PropertyMetadata(false));

        public Brush ListBackground
        {
            get { return (Brush)GetValue(ListBackgroundProperty); }
            set { SetValue(ListBackgroundProperty, value); }
        }
        public static readonly DependencyProperty ListBackgroundProperty =
            DependencyProperty.Register("ListBackground", typeof(Brush), typeof(LolloMultipleListChooser), new PropertyMetadata((Brush)Application.Current.Resources["FlyoutBackgroundThemeBrush"]));
        #endregion properties

        #region events
        public event EventHandler<TextAndTag> ItemDeselected;
        public event EventHandler<TextAndTag> ItemSelected;
        #endregion events

        #region construct and dispose
        public LolloMultipleListChooser()
            : base()
        {
            InitializeComponent();
            ClosedDescriptorTextBlock.Text = PlaceholderText;
        }
        #endregion construct and dispose

        #region updaters
        private void UpdateSelection()
        {
            var mlvItems = MyListView.Items;
            if (mlvItems == null || SelectedItems == null) return;

            foreach (var item in mlvItems)
            {
                var stt = item as SelectedAndTextAndTag;
                if (stt?.TextAndTag?.Tag == null) continue;

                if (SelectedItems.Any(si => si.Tag.CompareTo(stt.TextAndTag.Tag) == 0)) stt.IsSelected = true;
                else stt.IsSelected = false;
            }
        }
        private void UpdateDescriptor()
        {
            int howManySelectedItems = SelectedItems?.Count ?? 0;
            if (howManySelectedItems == 0) ClosedDescriptorTextBlock.Text = PlaceholderText;
            else if (howManySelectedItems == 1) ClosedDescriptorTextBlock.Text = SelectedItems[0].Text;
            else ClosedDescriptorTextBlock.Text = $"{howManySelectedItems} items";
        }
        #endregion updaters

        #region popup
        //protected override void OnHardwareOrSoftwareButtons_BackPressed_MayOverride(object sender, BackOrHardSoftKeyPressedEventArgs e)
        //{
        //    if (!IsPopupOpen) return;

        //    if (e != null) e.Handled = true;
        //    IsPopupOpen = false;
        //}

        //private void OnMyPopup_Closed(object sender, object e)
        //{
        //    IsPopupOpen = false;
        //}
        #endregion popup

        #region event handlers
        private void OnSelector_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (ItemsSource != null && !IsPopupOpen)
            {
                IsPopupOpen = true;
            }
        }
        /// <summary>
        /// Fires after <see cref="OnMyListView_ItemClick"/>
        /// </summary>
        private void OnItemBorder_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (IsMultiSelectCheckBoxEnabled) return;
            IsPopupOpen = false;
        }

        private volatile bool _isMyListViewEventHandlersActive = false;
        private void OnMyListViewLoaded(object sender, RoutedEventArgs e)
        {
            var itemsSource = ItemsSource;
            if (itemsSource != null)
            {
                MyListView.ItemsSource = itemsSource.Select(nv => new SelectedAndTextAndTag() { IsSelected = false, TextAndTag = nv }).ToList();
            }
            UpdateDescriptor();
            UpdateSelection();

            var myLVItems = MyListView.Items;
            if (myLVItems == null) return;

            var firstSelectedItem = myLVItems.FirstOrDefault(si => (si as SelectedAndTextAndTag)?.IsSelected == true);
            if (firstSelectedItem != null) MyListView.ScrollIntoView(firstSelectedItem);

            if (_isMyListViewEventHandlersActive) return;
            _isMyListViewEventHandlersActive = true;
            myLVItems.VectorChanged += OnMyListViewItems_VectorChanged;
        }

        private void OnMyListViewUnloaded(object sender, RoutedEventArgs e)
        {
            _isMyListViewEventHandlersActive = false;

            var myLVItems = MyListView.Items;
            if (myLVItems == null) return;
            myLVItems.VectorChanged -= OnMyListViewItems_VectorChanged;
            //MyListView.ItemsSource = null;
        }
        private void OnCheckBox_Click(object sender, RoutedEventArgs e)
        {
            var stt = (sender as FrameworkElement)?.DataContext as SelectedAndTextAndTag;
            if (stt == null) return;
            stt.IsSelected = !stt.IsSelected; // undo the checkbox tick
            ToggleItemIsSelected(stt);
        }

        private void OnMyListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            ToggleItemIsSelected(e?.ClickedItem as SelectedAndTextAndTag);
        }
        private void ToggleItemIsSelected(SelectedAndTextAndTag clickedItem)
        {
            // toggle selection
            if (clickedItem == null) return;

            if (!clickedItem.IsSelected)
            {
                clickedItem.IsSelected = true;
                ItemSelected?.Invoke(this, clickedItem.TextAndTag);
            }
            else if (IsMultiSelectCheckBoxEnabled)
            {
                clickedItem.IsSelected = false;
                ItemDeselected?.Invoke(this, clickedItem.TextAndTag);
            }
        }

        private void OnMyListViewItems_VectorChanged(Windows.Foundation.Collections.IObservableVector<object> sender, Windows.Foundation.Collections.IVectorChangedEventArgs @event)
        {
            Task updSelIdx = RunInUiThreadAsync(delegate
            {
                UpdateDescriptor();
                UpdateSelection();
            });
        }
        #endregion event handlers
    }

    public sealed class TextAndTag
    {
        private string _text = "";
        public string Text { get { return _text; } private set { _text = value; } }

        private IComparable _tag = null;
        public IComparable Tag { get { return _tag; } private set { _tag = value; } }

        public TextAndTag(string text, IComparable tag)
        {
            _text = text;
            _tag = tag;
        }
        //public static Collection<TextAndTag> CreateCollection(string[] texts, Array tags)
        //{
        //    if (texts == null || tags == null || texts.Length != tags.Length) throw new ArgumentException("Texts and tags must not be null and must have the same length");
        //    Collection<TextAndTag> output = new Collection<TextAndTag>();
        //    for (int i = 0; i < texts.Length; i++)
        //    {
        //        output.Add(new TextAndTag(texts[i], tags.GetValue(new int[1] { i })));
        //    }
        //    return output;
        //}
    }

    public sealed class SelectedAndTextAndTag : ObservableData
    {
        private bool _isSelected;

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    RaisePropertyChanged();
                }
            }
        }

        private TextAndTag _textAndTag;
        public TextAndTag TextAndTag
        {
            get { return _textAndTag; }
            set
            {
                _textAndTag = value;
                RaisePropertyChanged();
            }
        }
    }

    public class IsSelectedToBackgroundConverter : IValueConverter
    {
        public static readonly SolidColorBrush SelectedBrush = ((SolidColorBrush)(Application.Current.Resources["SystemControlHighlightListAccentLowBrush"]));
        //public static readonly SolidColorBrush UnselectedBrush = ((SolidColorBrush)(Application.Current.Resources["FlyoutBackgroundThemeBrush"]));
        public static readonly SolidColorBrush UnselectedBrush = new SolidColorBrush(Colors.Transparent);

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!(value is bool) || !((bool)value)) return UnselectedBrush;
            return SelectedBrush;
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new Exception("this is a one-way binding, it should never come here");
        }
    }
}