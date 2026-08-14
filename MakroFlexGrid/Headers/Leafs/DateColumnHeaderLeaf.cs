using System;
using System.Windows;
using System.Windows.Controls;

namespace MakroFlexGrid.Headers
{
    /// <summary>
    /// Листовая колонка для отображения и выбора даты.
    /// В режиме просмотра показывает дату в заданном формате.
    /// При клике открывает DatePicker для выбора даты.
    /// Параметры автоматически копируются в CellViewModel.Config через ApplyHeaderConfig().
    /// </summary>
    [CellTemplate("UnifiedCellTemplate")]
    public class DateColumnHeaderLeaf : ColumnHeaderLeaf
    {
        static DateColumnHeaderLeaf()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(DateColumnHeaderLeaf),
                new FrameworkPropertyMetadata(typeof(DateColumnHeaderLeaf)));
        }

        #region Dependency Properties

        /// <summary>
        /// Формат отображения даты. По умолчанию "dd.MM.yyyy".
        /// </summary>
        public static readonly DependencyProperty FormatProperty =
            DependencyProperty.Register(
                "Format",
                typeof(string),
                typeof(DateColumnHeaderLeaf),
                new FrameworkPropertyMetadata("dd.MM.yyyy"));

        public string Format
        {
            get => (string)GetValue(FormatProperty);
            set => SetValue(FormatProperty, value);
        }

        /// <summary>
        /// Первый день недели в DatePicker.
        /// </summary>
        public static readonly DependencyProperty FirstDayOfWeekProperty =
            DependencyProperty.Register(
                "FirstDayOfWeek",
                typeof(DayOfWeek),
                typeof(DateColumnHeaderLeaf),
                new FrameworkPropertyMetadata(DayOfWeek.Monday));

        public DayOfWeek FirstDayOfWeek
        {
            get => (DayOfWeek)GetValue(FirstDayOfWeekProperty);
            set => SetValue(FirstDayOfWeekProperty, value);
        }

        #endregion
    }
}