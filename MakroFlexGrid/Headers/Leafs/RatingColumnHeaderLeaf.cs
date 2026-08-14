using System.Windows;
using System.Windows.Controls;

namespace MakroFlexGrid.Headers
{
    /// <summary>
    /// Листовая колонка для отображения рейтинга в виде звёзд.
    /// Поддерживает настраиваемое количество звёзд и иконки.
    /// Параметры автоматически копируются в CellViewModel.Config через ApplyHeaderConfig().
    /// </summary>
    [CellTemplate("UnifiedCellTemplate")]
    public class RatingColumnHeaderLeaf : ColumnHeaderLeaf
    {
        static RatingColumnHeaderLeaf()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(RatingColumnHeaderLeaf),
                new FrameworkPropertyMetadata(typeof(RatingColumnHeaderLeaf)));
        }

        #region Dependency Properties

        /// <summary>
        /// Максимальный рейтинг (количество звёзд). По умолчанию 5.
        /// </summary>
        public static readonly DependencyProperty MaxRatingProperty =
            DependencyProperty.Register(
                "MaxRating",
                typeof(int),
                typeof(RatingColumnHeaderLeaf),
                new FrameworkPropertyMetadata(5));

        public int MaxRating
        {
            get => (int)GetValue(MaxRatingProperty);
            set => SetValue(MaxRatingProperty, value);
        }

        /// <summary>
        /// Тип иконки для рейтинга (Star, Heart, Thumb и т.д.).
        /// </summary>
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(
                "Icon",
                typeof(string),
                typeof(RatingColumnHeaderLeaf),
                new FrameworkPropertyMetadata("Star"));

        public string Icon
        {
            get => (string)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        /// <summary>
        /// Запретить редактирование рейтинга. По умолчанию false.
        /// </summary>
        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register(
                "IsReadOnly",
                typeof(bool),
                typeof(RatingColumnHeaderLeaf),
                new FrameworkPropertyMetadata(false));

        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }

        #endregion
    }
}