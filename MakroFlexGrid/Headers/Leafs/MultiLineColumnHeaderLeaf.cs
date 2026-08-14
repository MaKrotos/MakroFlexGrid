using System.Windows;
using System.Windows.Controls;

namespace MakroFlexGrid.Headers
{
    /// <summary>
    /// Листовая колонка для отображения и редактирования многострочного текста.
    /// В режиме просмотра показывает текст с переносом строк.
    /// При редактировании — TextBox с AcceptsReturn=True.
    /// Параметры автоматически копируются в CellViewModel.Config через ApplyHeaderConfig().
    /// </summary>
    [CellTemplate("UnifiedCellTemplate")]
    public class MultiLineColumnHeaderLeaf : ColumnHeaderLeaf
    {
        static MultiLineColumnHeaderLeaf()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(MultiLineColumnHeaderLeaf),
                new FrameworkPropertyMetadata(typeof(MultiLineColumnHeaderLeaf)));
        }

        #region Dependency Properties

        /// <summary>
        /// Максимальное количество отображаемых строк. По умолчанию 5.
        /// </summary>
        public static readonly DependencyProperty MaxLinesProperty =
            DependencyProperty.Register(
                "MaxLines",
                typeof(int),
                typeof(MultiLineColumnHeaderLeaf),
                new FrameworkPropertyMetadata(5));

        public int MaxLines
        {
            get => (int)GetValue(MaxLinesProperty);
            set => SetValue(MaxLinesProperty, value);
        }

        /// <summary>
        /// Запретить редактирование. По умолчанию false.
        /// </summary>
        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register(
                "IsReadOnly",
                typeof(bool),
                typeof(MultiLineColumnHeaderLeaf),
                new FrameworkPropertyMetadata(false));

        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }

        #endregion
    }
}