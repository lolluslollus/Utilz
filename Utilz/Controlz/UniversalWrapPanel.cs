using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// LOLLO original version downloaded from https://github.com/gregstoll/UniversalWrapPanel
namespace Utilz.Controlz
{
    public sealed class UniversalWrapPanel : Panel
    {
        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }
        public static readonly DependencyProperty OrientationProperty =
          DependencyProperty.Register("Orientation",
          typeof(Orientation), typeof(UniversalWrapPanel), null);

        public VerticalAlignment VerticalContentAlignment
        {
            get { return (VerticalAlignment)GetValue(VerticalContentAlignmentProperty); }
            set { SetValue(VerticalContentAlignmentProperty, value); }
        }
        public static readonly DependencyProperty VerticalContentAlignmentProperty =
            DependencyProperty.Register("VerticalContentAlignment", typeof(VerticalAlignment), typeof(UniversalWrapPanel), new PropertyMetadata(VerticalAlignment.Top));


        public UniversalWrapPanel()
        {
            // default orientation
            Orientation = Orientation.Vertical;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            Point point = new Point(0, 0);

            if (Orientation == Orientation.Horizontal)
            {
                double largestHeightInRow = 0.0;
                double maxWidth = 0.0;

                for (int c = 0; c < Children.Count; c++)
                {
                    UIElement child = Children[c];
                    child.Measure(availableSize);

                    double verticalMargin = GetVerticalMargin(child);
                    double horizontalMargin = GetHorizontalMargin(child);

                    if ((point.X + child.DesiredSize.Width + horizontalMargin) > availableSize.Width)
                    {
                        // start new row
                        point.X = 0;
                        point.Y = point.Y + largestHeightInRow;
                        largestHeightInRow = 0.0;
                    }
                    point.X += child.DesiredSize.Width + horizontalMargin;

                    // Tallest item in the row
                    largestHeightInRow = Math.Max(largestHeightInRow, child.DesiredSize.Height + verticalMargin);

                    // Furthest right edge
                    maxWidth = Math.Max(maxWidth, point.X);
                }
                return new Size(maxWidth, point.Y + largestHeightInRow);
            }
            else
            {
                double largestWidth = 0.0;
                double maxHeight = 0;

                // Loop invariant:
                // at top of loop, point is top-left of where this child will try to go
                // point.Y should never be 0 at the top except when c = 0
                for (int c = 0; c < Children.Count; c++)
                {
                    UIElement child = Children[c];
                    child.Measure(availableSize);

                    if ((point.Y + child.DesiredSize.Height) > availableSize.Height)
                    {
                        point.Y = 0;
                        point.X = point.X + largestWidth;
                        largestWidth = 0.0;
                    }
                    point.Y += child.DesiredSize.Height;

                    // Widest item in the column
                    largestWidth = Math.Max(largestWidth, child.DesiredSize.Width);

                    // Furthest bottom edge
                    maxHeight = Math.Max(maxHeight, point.Y);
                }
                return new Size(point.X + largestWidth, maxHeight);
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (Orientation == Orientation.Horizontal)
            {
                return ArrangeOverrideHorizontalAlignment(finalSize);
            }
            else
            {
                return ArrangeOverrideVerticalAlignment(finalSize);
            }
        }

        private Size ArrangeOverrideHorizontalAlignment(Size finalSize)
        {
            var rowDatas = GetRowData4HorizontalAlignment(Children, finalSize);

            for (int j = 0; j < rowDatas.Count; j++)
            {
                for (int i = 0; i < rowDatas[j].Points0.Count; i++)
                {
                    var maxHeightInRow = rowDatas[j].MaxHeightInRow;
                    var child = rowDatas[j].Children[i];
                    var point0 = rowDatas[j].Points0[i];
                    var point1 = rowDatas[j].Points1[i];
                    double deltaY = 0.0;
                    // LOLLO TODO deal with margins, and remember they can be like  like 5 top, 20 bottom.
                    switch (VerticalContentAlignment)
                    {
                        case VerticalAlignment.Top:
                            child.Arrange(new Rect(point0, point1));
                            break;
                        case VerticalAlignment.Center:
                            deltaY = (maxHeightInRow - point1.Y - 1) / 2.0;
                            if (deltaY > 0)
                            {
                                point0.Y += deltaY;
                                point1.Y += deltaY;
                            }
                            child.Arrange(new Rect(point0, point1));
                            break;
                        case VerticalAlignment.Bottom:
                            deltaY = maxHeightInRow - point1.Y - 1;
                            if (deltaY > 0)
                            {
                                point0.Y += deltaY;
                                point1.Y += deltaY;
                            }
                            child.Arrange(new Rect(point0, point1));
                            break;
                        //case VerticalAlignment.Stretch:
                        //    break;
                        default:
                            deltaY = (maxHeightInRow - point1.Y - 1) / 2.0;
                            if (deltaY > 0)
                            {
                                point0.Y += deltaY;
                                point1.Y += deltaY;
                            }
                            child.Arrange(new Rect(point0, point1));
                            break;
                    }
                }
            }
            // this is the old code
            //foreach (UIElement child in Children)
            //{
            //    if (child.DesiredSize.Height > largestHeightInRow)
            //        largestHeightInRow = child.DesiredSize.Height;

            //    child.Arrange(new Rect(point, new Point(point.X +
            //        child.DesiredSize.Width, point.Y + child.DesiredSize.Height)));

            //    point.X = point.X + child.DesiredSize.Width;

            //    if ((i + 1) < Children.Count)
            //    {
            //        if ((point.X + Children[i + 1].DesiredSize.Width) > finalSize.Width)
            //        {
            //            // start new row
            //            point.X = 0;
            //            point.Y = point.Y + largestHeightInRow;
            //            largestHeightInRow = 0.0;
            //        }
            //    }
            //    i++;
            //}

            return base.ArrangeOverride(finalSize);
        }

        private static List<RowData> GetRowData4HorizontalAlignment(UIElementCollection Children, Size finalSize)
        {
            List<RowData> rowDatas = new List<RowData>(Children.Count);

            int i = 0; int j = 0;
            foreach (UIElement child in Children)
            {
                if (rowDatas.Count - 1 < j) rowDatas.Add(new RowData());
                var currentRowData = rowDatas[j];
                currentRowData.Row = j;

                double verticalMargin = GetVerticalMargin(child);
                double horizontalMargin = GetHorizontalMargin(child);

                var currentPoint0 = new Point();
                var currentPoint1 = new Point();

                if (i == 0)
                {
                    // first element of a row
                    currentPoint0.X = 0.0;
                    currentPoint1.X = child.DesiredSize.Width - 1 + horizontalMargin;
                    currentPoint0.Y = rowDatas.Take(j).Sum(rd => rd.MaxHeightInRow);
                    currentPoint1.Y = currentPoint0.Y + child.DesiredSize.Height - 1 + verticalMargin;
                    currentRowData.Points0.Add(currentPoint0);
                    currentRowData.Points1.Add(currentPoint1);
                    currentRowData.Children.Add(child);
                    currentRowData.MaxHeightInRow = Math.Max(child.DesiredSize.Height + verticalMargin, rowDatas[j].MaxHeightInRow);
                    i++;
                }
                else if ((currentRowData.Points1[i - 1].X + child.DesiredSize.Width + horizontalMargin) <= finalSize.Width)
                {
                    // append to existing row
                    currentPoint0.X = currentRowData.Points1[i - 1].X + 1;
                    currentPoint1.X = currentPoint0.X + child.DesiredSize.Width - 1 + horizontalMargin;
                    currentPoint0.Y = rowDatas.Take(j).Sum(rd => rd.MaxHeightInRow);
                    currentPoint1.Y = currentPoint0.Y + child.DesiredSize.Height - 1 + verticalMargin;
                    currentRowData.Points0.Add(currentPoint0);
                    currentRowData.Points1.Add(currentPoint1);
                    currentRowData.Children.Add(child);
                    currentRowData.MaxHeightInRow = Math.Max(child.DesiredSize.Height + verticalMargin, rowDatas[j].MaxHeightInRow);
                    i++;
                }
                else
                {
                    // start new row                            
                    currentPoint0.X = 0.0;
                    currentPoint1.X = child.DesiredSize.Width - 1 + horizontalMargin;
                    currentPoint0.Y = rowDatas.Sum(rd => rd.MaxHeightInRow);
                    currentPoint1.Y = currentPoint0.Y + child.DesiredSize.Height - 1 + verticalMargin;
                    var newRowData = new RowData();
                    newRowData.Points0.Add(currentPoint0);
                    newRowData.Points1.Add(currentPoint1);
                    newRowData.Children.Add(child);
                    newRowData.MaxHeightInRow = child.DesiredSize.Height + verticalMargin;
                    rowDatas.Add(newRowData);
                    i = 1;
                    j++;
                }
            }

            return rowDatas;
        }
        private static double GetHorizontalMargin(UIElement child)
        {
            if (!(child is FrameworkElement)) return 0.0;
            var verticalMargin = (child as FrameworkElement).Margin;
            return verticalMargin.Left + verticalMargin.Right;
        }
        private static double GetVerticalMargin(UIElement child)
        {
            if (!(child is FrameworkElement)) return 0.0;
            var verticalMargin = (child as FrameworkElement).Margin;
            return verticalMargin.Top + verticalMargin.Bottom;
        }
        private Size ArrangeOverrideVerticalAlignment(Size finalSize)
        {
            Point point = new Point(0, 0);
            int i = 0; int j = 0;

            double largestWidth = 0.0;

            foreach (UIElement child in Children)
            {
                child.Arrange(new Rect(point, new Point(point.X +
                  child.DesiredSize.Width, point.Y + child.DesiredSize.Height)));

                if (child.DesiredSize.Width > largestWidth)
                    largestWidth = child.DesiredSize.Width;

                point.Y = point.Y + child.DesiredSize.Height;

                if ((i + 1) < Children.Count)
                {
                    if ((point.Y + Children[i + 1].DesiredSize.Height) > finalSize.Height)
                    {
                        point.Y = 0;
                        point.X = point.X + largestWidth;
                        largestWidth = 0.0;
                    }
                }

                i++;
            }

            return base.ArrangeOverride(finalSize);
        }

        internal class RowData
        {
            internal int Row;
            internal double MaxHeightInRow;
            internal List<Point> Points0 = new List<Point>();
            internal List<Point> Points1 = new List<Point>();
            internal List<UIElement> Children = new List<UIElement>();
            internal RowData() { }
        }
    }
}
