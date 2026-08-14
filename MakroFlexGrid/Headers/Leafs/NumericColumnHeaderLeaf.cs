using System.Windows;
using System.Windows.Controls;

namespace MakroFlexGrid.Headers
{
    /// <summary>
    /// Листовая колонка для числового ввода с форматированием.
    /// Поддерживает decimal, int, double, float.
    /// Параметры автоматически копируются в CellViewModel.Config через ApplyHeaderConfig().
    /// </summary>
    [CellTemplate("UnifiedCellTemplate")]
    public class NumericColumnHeaderLeaf : ColumnHeaderLeaf
    {
        static NumericColumnHeaderLeaf()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(NumericColumnHeaderLeaf),
                new FrameworkPropertyMetadata(typeof(NumericColumnHeaderLeaf)));
        }

        #region Dependency Properties

        /// <summary>
        /// Количество знаков после запятой. По умолчанию 2.
        /// </summary>
        public static readonly DependencyProperty DecimalPlacesProperty =
            DependencyProperty.Register(
                "DecimalPlaces",
                typeof(int),
                typeof(NumericColumnHeaderLeaf),
                new FrameworkPropertyMetadata(2));

        public int DecimalPlaces
        {
            get => (int)GetValue(DecimalPlacesProperty);
            set => SetValue(DecimalPlacesProperty, value);
        }

        /// <summary>
        /// Минимальное значение (для валидации).
        /// </summary>
        public static readonly DependencyProperty MinValueProperty =
            DependencyProperty.Register(
                "MinValue",
                typeof(decimal),
                typeof(NumericColumnHeaderLeaf),
                new FrameworkPropertyMetadata(decimal.MinValue));

        public decimal MinValue
        {
            get => (decimal)GetValue(MinValueProperty);
            set => SetValue(MinValueProperty, value);
        }

        /// <summary>
        /// Максимальное значение (для валидации).
        /// </summary>
        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register(
                "MaxValue",
                typeof(decimal),
                typeof(NumericColumnHeaderLeaf),
                new FrameworkPropertyMetadata(decimal.MaxValue));

        public decimal MaxValue
        {
            get => (decimal)GetValue(MaxValueProperty);
            set => SetValue(MaxValueProperty, value);
        }

        /// <summary>
        /// Формат отображения числа (например, "C2" для валюты, "N0" для целых).
        /// Если не задан, используется "F{DecimalPlaces}".
        /// </summary>
        public static readonly DependencyProperty FormatProperty =
            DependencyProperty.Register(
                "Format",
                typeof(string),
                typeof(NumericColumnHeaderLeaf),
                new FrameworkPropertyMetadata(null));

        public string Format
        {
            get => (string)GetValue(FormatProperty);
            set => SetValue(FormatProperty, value);
        }

        /// <summary>
        /// Символ валюты (если нужен). Отображается перед числом.
        /// </summary>
        public static readonly DependencyProperty CurrencySymbolProperty =
            DependencyProperty.Register(
                "CurrencySymbol",
                typeof(string),
                typeof(NumericColumnHeaderLeaf),
                new FrameworkPropertyMetadata(null));

        public string CurrencySymbol
        {
            get => (string)GetValue(CurrencySymbolProperty);
            set => SetValue(CurrencySymbolProperty, value);
        }

        /// <summary>
        /// Разрешить ввод отрицательных чисел. По умолчанию true.
        /// </summary>
        public static readonly DependencyProperty AllowNegativeProperty =
            DependencyProperty.Register(
                "AllowNegative",
                typeof(bool),
                typeof(NumericColumnHeaderLeaf),
                new FrameworkPropertyMetadata(true));

        public bool AllowNegative
        {
            get => (bool)GetValue(AllowNegativeProperty);
            set => SetValue(AllowNegativeProperty, value);
        }

        #endregion
    }
}