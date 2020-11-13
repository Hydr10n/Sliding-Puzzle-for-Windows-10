using System;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace Sliding_Puzzle
{
    struct Cell
    {
        public int Row, Column;

        public Cell(int row, int column)
        {
            Row = row;
            Column = column;
        }

        public static bool operator ==(Cell a, Cell b)
        {
            if (a == null && b == null)
                return true;
            if (a == null || b == null)
                return false;
            return a.Row == b.Row && a.Column == b.Column;
        }

        public static bool operator !=(Cell a, Cell b) => !(a == b);

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;
            return this == (Cell)obj;
        }

        public override int GetHashCode() => base.GetHashCode();
    }

    sealed class Tile : Button
    {
        private const int TileForegroundIndex = 0, TileBackgroundIndex = 1;
        private const double MinSizeScale = 0.5, ScaleDuration = 150, MovementDurationPerCell = 100;

        private static readonly Color[,] TileColors = new Color[,] {    // [0]: text color; [1]: background color
            { new Color(), Color.FromArgb(0xff, 0xcd, 0xc1, 0xb4) },
            { Color.FromArgb(0xff, 0x77, 0x6e, 0x65), Color.FromArgb(0xff, 0xee, 0xe4, 0xda) }
        };

        private readonly double fullSideLength;
        private readonly TextBlock textBlock;

        private int number;
        public int Number
        {
            get => number;
            private set
            {
                number = value;
                if (textBlock != null)
                    textBlock.Text = value.ToString();
                Background = new SolidColorBrush(TileColors[number == 0 ? 0 : 1, TileBackgroundIndex]);
                Foreground = new SolidColorBrush(TileColors[number == 0 ? 0 : 1, TileForegroundIndex]);
            }
        }

        private Cell cell;

        public static Storyboard Storyboard { get; set; }

        public Tile(Grid parent, int row, int column, int number)
        {
            Style = (Style)Resources["ButtonRevealStyle"];
            Opacity = 0.85;
            IsTabStop = false;
            HorizontalAlignment = HorizontalAlignment.Center;
            VerticalAlignment = VerticalAlignment.Center;
            Padding = new Thickness(0, 0, 0, 6);
            fullSideLength = parent.RowDefinitions[0].Height.Value;
            Width = Height = fullSideLength - parent.Padding.Top * 2;
            CornerRadius = new CornerRadius(fullSideLength * 0.07);
            if (number != 0)
            {
                textBlock = new TextBlock() { FontSize = fullSideLength * 0.6, FontWeight = FontWeights.Bold, };
                Content = new Viewbox() { Child = textBlock };
            }
            parent.Children.Add(this);
            SetCell(row, column);
            cell = new Cell(row, column);
            Number = number;
            AnimateScale(MinSizeScale, 1);
        }

        private void SetCell(int row, int column)
        {
            Grid.SetRow(this, row);
            Grid.SetColumn(this, column);
        }

        private void AnimateScale(double fromScale, double toScale)
        {
            RenderTransform = new ScaleTransform { CenterX = Width / 2, CenterY = Height / 2 };
            Duration duration = new Duration(TimeSpan.FromMilliseconds(ScaleDuration));
            DoubleAnimation doubleAnimation = new DoubleAnimation { From = fromScale, To = toScale, Duration = duration, EnableDependentAnimation = true }, doubleAnimation2 = new DoubleAnimation { From = fromScale, To = toScale, Duration = duration, EnableDependentAnimation = true };
            Storyboard.SetTargetProperty(doubleAnimation, "ScaleX");
            Storyboard.SetTargetProperty(doubleAnimation2, "ScaleY");
            Storyboard.SetTarget(doubleAnimation, RenderTransform);
            Storyboard.SetTarget(doubleAnimation2, RenderTransform);
            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(doubleAnimation);
            storyboard.Children.Add(doubleAnimation2);
            storyboard.Begin();
        }

        public void RemoveSelf() => (Parent as Panel).Children.Remove(this);

        private void MoveTo(int row, int column, EventHandler<object> animationCompleted)
        {
            RenderTransform = new TranslateTransform();
            int rowDistance = row - cell.Row, columnDistance = column - cell.Column;
            cell = new Cell(row, column);
            Duration duration = new Duration(TimeSpan.FromMilliseconds(MovementDurationPerCell * Math.Max(Math.Abs(rowDistance), Math.Abs(columnDistance))));
            DoubleAnimation doubleAnimation = new DoubleAnimation { To = fullSideLength * columnDistance, Duration = duration, EnableDependentAnimation = true }, doubleAnimation2 = new DoubleAnimation { To = fullSideLength * rowDistance, Duration = duration };
            Storyboard.SetTargetProperty(doubleAnimation, "X");
            Storyboard.SetTargetProperty(doubleAnimation2, "Y");
            Storyboard.SetTarget(doubleAnimation, RenderTransform);
            Storyboard.SetTarget(doubleAnimation2, RenderTransform);
            Storyboard.Completed += delegate
            {
                SetCell(row, column);
                animationCompleted?.Invoke(null, null);
            };
            Storyboard.Children.Add(doubleAnimation);
            Storyboard.Children.Add(doubleAnimation2);
        }

        public void MoveTo(int row, int column) => MoveTo(row, column, null);
    }
}