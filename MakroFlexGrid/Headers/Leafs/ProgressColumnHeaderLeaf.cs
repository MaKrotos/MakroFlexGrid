using System.Windows;
using System.Windows.Controls;

namespace MakroFlexGrid.Headers
{
    /// <summary>
    /// Листовая колонка для отображения прогресса в виде ProgressBar.
    /// Показывает процент выполнения текстом поверх полосы прогресса.
    /// Параметры автоматически копируются в CellViewModel.Config через ApplyHeaderConfig().
    /// </summary>
    [CellTemplate("UnifiedCellTemplate")]
    public class ProgressColumnHeaderLeaf : ColumnHeaderLeaf
    {
        static ProgressColumnHeaderLeaf()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ProgressColumnHeaderLeaf),
                new FrameworkPropertyMetadata(typeof(ProgressColumnHeaderLeaf)));
        }

        #region Dependency Properties

        /// <summary>
        /// Минимальное значение прогресса. По умолчанию 0.
        /// </summary>
        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register(
                "Minimum",
                typeof(double),
                typeof(ProgressColumnHeaderLeaf),
                new FrameworkPropertyMetadata(0.0));

        public double Minimum
        {
            get => (double)GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }

        /// <summary>
        /// Максимальное значение прогресса. По умолчанию 100.
        /// </summary>
        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register(
                "Maximum",
                typeof(double),
                typeof(ProgressColumnHeaderLeaf),
                new FrameworkPropertyMetadata(100.0));

        public double Maximum
        {
            get => (double)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        /// <summary>
        /// Показывать процент текстом поверх ProgressBar. По умолчанию true.
        /// </summary>
        public static readonly DependencyProperty ShowPercentageProperty =
            DependencyProperty.Register(
                "ShowPercentage",
                typeof(bool),
                typeof(ProgressColumnHeaderLeaf),
                new FrameworkPropertyMetadata(true));

        public bool ShowPercentage
        {
            get => (bool)GetValue(ShowPercentageProperty);
            set => SetValue(ShowPercentageProperty, value);
        }

        #endregion
    }
}