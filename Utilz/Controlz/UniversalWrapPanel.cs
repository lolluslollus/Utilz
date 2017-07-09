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
        #region properties
        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }
        public static readonly DependencyProperty OrientationProperty =
          DependencyProperty.Register("Orientation",
          typeof(Orientation), typeof(UniversalWrapPanel), new PropertyMetadata(Orientation.Horizontal));

        public HorizontalAlignment HorizontalContentAlignment
        {
            get { return (HorizontalAlignment)GetValue(HorizontalContentAlignmentProperty); }
            set { SetValue(HorizontalContentAlignmentProperty, value); }
        }
        public static readonly DependencyProperty HorizontalContentAlignmentProperty =
            DependencyProperty.Register("HorizontalContentAlignment", typeof(HorizontalAlignment), typeof(UniversalWrapPanel), new PropertyMetadata(HorizontalAlignment.Left));

        public VerticalAlignment VerticalContentAlignment
        {
            get { return (VerticalAlignment)GetValue(VerticalContentAlignmentProperty); }
            set { SetValue(VerticalContentAlignmentProperty, value); }
        }
        public static readonly DependencyProperty VerticalContentAlignmentProperty =
            DependencyProperty.Register("VerticalContentAlignment", typeof(VerticalAlignment), typeof(UniversalWrapPanel), new PropertyMetadata(VerticalAlignment.Top));
        #endregion properties

        public UniversalWrapPanel() { }

        protected override Size MeasureOverride(Size availableSize)
        {
            Point point = new Point(0, 0);

            if (Orientation == Orientation.Horizontal)
            {
                int i = 0;
                double largestHeightInRow = 0.0;
                double maxRowWidth = 0.0;

                foreach (var child in Children)
                {
                    child.Measure(availableSize);

                    if (i > 0 && (point.X + child.DesiredSize.Width) > availableSize.Width)
                    {
                        // start new row
                        point.X = 0;
                        point.Y = point.Y + largestHeightInRow;
                        largestHeightInRow = 0.0;
                        i = -1;
                    }

                    point.X += child.DesiredSize.Width;
                    largestHeightInRow = Math.Max(largestHeightInRow, child.DesiredSize.Height);
                    maxRowWidth = Math.Max(maxRowWidth, point.X);
                    i++;
                }
                return new Size(maxRowWidth, point.Y + largestHeightInRow);
            }
            else
            {
                int i = 0;
                double largestWidthInColumn = 0.0;
                double maxHeight = 0;

                foreach (var child in Children)
                {
                    child.Measure(availableSize);

                    if (i > 0 && (point.Y + child.DesiredSize.Height) > availableSize.Height)
                    {
                        // start new column
                        point.Y = 0;
                        point.X = point.X + largestWidthInColumn;
                        largestWidthInColumn = 0.0;
                        i = -1;
                    }

                    point.Y += child.DesiredSize.Height;
                    largestWidthInColumn = Math.Max(largestWidthInColumn, child.DesiredSize.Width);
                    maxHeight = Math.Max(maxHeight, point.Y);
                    i++;
                }
                return new Size(point.X + largestWidthInColumn, maxHeight);
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
                var maxHeightInRow = rowDatas[j].MaxHeightInRow;

                for (int i = 0; i < rowDatas[j].Point0s.Count; i++)
                {
                    var child = rowDatas[j].Children[i];
                    var point0 = rowDatas[j].Point0s[i];
                    var point1 = rowDatas[j].Point1s[i];
                    // LOLLO TODO BODGE to give a bit more space, there may be a rounding problem
                    point1.X++;
                    point1.Y++;

                    double deltaY = 0.0;
                    switch (VerticalContentAlignment)
                    {
                        case VerticalAlignment.Center:
                            deltaY = (maxHeightInRow - point1.Y - 1) / 2.0;
                            if (deltaY > 0)
                            {
                                point0.Y += deltaY;
                                point1.Y += deltaY;
                            }
                            break;
                        case VerticalAlignment.Bottom:
                            deltaY = maxHeightInRow - point1.Y - 1;
                            if (deltaY > 0)
                            {
                                point0.Y += deltaY;
                                point1.Y += deltaY;
                            }
                            break;
                        default: // default is top, we don't do stretch
                            break;
                    }
                    child.Arrange(new Rect(point0, point1));
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

        private Size ArrangeOverrideVerticalAlignment(Size finalSize)
        {
            var columnDatas = GetColumnData4VerticalAlignment(Children, finalSize);

            for (int j = 0; j < columnDatas.Count; j++)
            {
                var maxWidthInColumn = columnDatas[j].MaxWidthInColumn;

                for (int i = 0; i < columnDatas[j].Point0s.Count; i++)
                {
                    var child = columnDatas[j].Children[i];
                    var point0 = columnDatas[j].Point0s[i];
                    var point1 = columnDatas[j].Point1s[i];

                    double deltaX = 0.0;
                    switch (HorizontalContentAlignment)
                    {
                        case HorizontalAlignment.Center:
                            deltaX = (maxWidthInColumn - point1.X - 1) / 2.0;
                            if (deltaX > 0)
                            {
                                point0.X += deltaX;
                                point1.X += deltaX;
                            }
                            break;
                        case HorizontalAlignment.Right:
                            deltaX = maxWidthInColumn - point1.X - 1;
                            if (deltaX > 0)
                            {
                                point0.X += deltaX;
                                point1.X += deltaX;
                            }
                            break;
                        default: // default is left, we don't do stretch
                            break;
                    }
                    child.Arrange(new Rect(point0, point1));
                }
            }
            // this is the old code
            //Point point = new Point(0, 0);
            //int i = 0; int j = 0;

            //double largestWidth = 0.0;

            //foreach (UIElement child in Children)
            //{
            //    child.Arrange(new Rect(point, new Point(point.X +
            //      child.DesiredSize.Width, point.Y + child.DesiredSize.Height)));

            //    if (child.DesiredSize.Width > largestWidth)
            //        largestWidth = child.DesiredSize.Width;

            //    point.Y = point.Y + child.DesiredSize.Height;

            //    if ((i + 1) < Children.Count)
            //    {
            //        if ((point.Y + Children[i + 1].DesiredSize.Height) > finalSize.Height)
            //        {
            //            point.Y = 0;
            //            point.X = point.X + largestWidth;
            //            largestWidth = 0.0;
            //        }
            //    }

            //    i++;
            //}

            return base.ArrangeOverride(finalSize);
        }

        private static List<ColumnData> GetColumnData4VerticalAlignment(UIElementCollection Children, Size finalSize)
        {
            List<ColumnData> columnDatas = new List<ColumnData>(Children.Count);

            int i = 0; int j = 0;
            foreach (UIElement child in Children)
            {
                if (columnDatas.Count - 1 < j) columnDatas.Add(new ColumnData());
                var currentColumnData = columnDatas[j];
                currentColumnData.Column = j;

                var currentPoint0 = new Point();
                var currentPoint1 = new Point();

                if (i == 0)
                {
                    // first element of a column
                    currentPoint0.Y = 0.0;
                    currentPoint1.Y = child.DesiredSize.Height - 1;
                    currentPoint0.X = columnDatas.Take(j).Sum(rd => rd.MaxWidthInColumn);
                    currentPoint1.X = currentPoint0.X + child.DesiredSize.Width - 1;
                    currentColumnData.Point0s.Add(currentPoint0);
                    currentColumnData.Point1s.Add(currentPoint1);
                    currentColumnData.Children.Add(child);
                    currentColumnData.MaxWidthInColumn = Math.Max(child.DesiredSize.Width, currentColumnData.MaxWidthInColumn);
                    i++;
                }
                else if ((currentColumnData.Point1s[i - 1].Y + child.DesiredSize.Height) <= finalSize.Height)
                {
                    // append to existing column
                    currentPoint0.Y = currentColumnData.Point1s[i - 1].Y + 1;
                    currentPoint1.Y = currentPoint0.Y + child.DesiredSize.Height - 1;
                    currentPoint0.X = columnDatas.Take(j).Sum(rd => rd.MaxWidthInColumn);
                    currentPoint1.X = currentPoint0.X + child.DesiredSize.Width - 1;
                    currentColumnData.Point0s.Add(currentPoint0);
                    currentColumnData.Point1s.Add(currentPoint1);
                    currentColumnData.Children.Add(child);
                    currentColumnData.MaxWidthInColumn = Math.Max(child.DesiredSize.Width, currentColumnData.MaxWidthInColumn);
                    i++;
                }
                else
                {
                    // start new column                            
                    currentPoint0.Y = 0.0;
                    currentPoint1.Y = child.DesiredSize.Height - 1;
                    currentPoint0.X = columnDatas.Sum(rd => rd.MaxWidthInColumn);
                    currentPoint1.X = currentPoint0.X + child.DesiredSize.Width - 1;
                    var newColumnData = new ColumnData();
                    newColumnData.Point0s.Add(currentPoint0);
                    newColumnData.Point1s.Add(currentPoint1);
                    newColumnData.Children.Add(child);
                    newColumnData.MaxWidthInColumn = child.DesiredSize.Width;
                    columnDatas.Add(newColumnData);
                    i = 1;
                    j++;
                }
            }

            return columnDatas;
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

                var currentPoint0 = new Point();
                var currentPoint1 = new Point();

                if (i == 0)
                {
                    // first element of a row
                    currentPoint0.X = 0.0;
                    currentPoint1.X = child.DesiredSize.Width - 1;
                    currentPoint0.Y = rowDatas.Take(j).Sum(rd => rd.MaxHeightInRow);
                    currentPoint1.Y = currentPoint0.Y + child.DesiredSize.Height - 1;
                    currentRowData.Point0s.Add(currentPoint0);
                    currentRowData.Point1s.Add(currentPoint1);
                    currentRowData.Children.Add(child);
                    currentRowData.MaxHeightInRow = Math.Max(child.DesiredSize.Height, currentRowData.MaxHeightInRow);
                    i++;
                }
                else if ((currentRowData.Point1s[i - 1].X + child.DesiredSize.Width) <= finalSize.Width)
                {
                    // append to existing row
                    currentPoint0.X = currentRowData.Point1s[i - 1].X + 1;
                    currentPoint1.X = currentPoint0.X + child.DesiredSize.Width - 1;
                    currentPoint0.Y = rowDatas.Take(j).Sum(rd => rd.MaxHeightInRow);
                    currentPoint1.Y = currentPoint0.Y + child.DesiredSize.Height - 1;
                    currentRowData.Point0s.Add(currentPoint0);
                    currentRowData.Point1s.Add(currentPoint1);
                    currentRowData.Children.Add(child);
                    currentRowData.MaxHeightInRow = Math.Max(child.DesiredSize.Height, currentRowData.MaxHeightInRow);
                    i++;
                }
                else
                {
                    // start new row                            
                    currentPoint0.X = 0.0;
                    currentPoint1.X = child.DesiredSize.Width - 1;
                    currentPoint0.Y = rowDatas.Sum(rd => rd.MaxHeightInRow);
                    currentPoint1.Y = currentPoint0.Y + child.DesiredSize.Height - 1;
                    var newRowData = new RowData();
                    newRowData.Point0s.Add(currentPoint0);
                    newRowData.Point1s.Add(currentPoint1);
                    newRowData.Children.Add(child);
                    newRowData.MaxHeightInRow = child.DesiredSize.Height;
                    rowDatas.Add(newRowData);
                    i = 1;
                    j++;
                }
            }

            return rowDatas;
        }

        internal class ColumnData
        {
            internal int Column;
            internal double MaxWidthInColumn;
            internal List<Point> Point0s = new List<Point>();
            internal List<Point> Point1s = new List<Point>();
            internal List<UIElement> Children = new List<UIElement>();
            internal ColumnData() { }
        }
        internal class RowData
        {
            internal int Row;
            internal double MaxHeightInRow;
            internal List<Point> Point0s = new List<Point>();
            internal List<Point> Point1s = new List<Point>();
            internal List<UIElement> Children = new List<UIElement>();
            internal RowData() { }
        }
    }
}
