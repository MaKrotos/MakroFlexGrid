using System.Windows;
using System.Windows.Controls;

namespace MakroFlexGrid.Headers
{
    /// <summary>
    /// Листовая колонка для отображения цвета.
    /// Показывает цветной прямоугольник и опционально название цвета.
    /// Параметры автоматически копируются в CellViewModel.Config через ApplyHeaderConfig().
    /// </summary>
    [CellTemplate("UnifiedCellTemplate")]
    public class ColorColumnHeaderLeaf : ColumnHeaderLeaf
    {
        static ColorColumnHeaderLeaf()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ColorColumnHeaderLeaf),
                new FrameworkPropertyMetadata(typeof(ColorColumnHeaderLeaf)));
        }

        #region Dependency Properties

        /// <summary>
        /// Показывать название цвета рядом с прямоугольником. По умолчанию true.
        /// </summary>
        public static readonly DependencyProperty ShowColorNameProperty =
            DependencyProperty.Register(
                "ShowColorName",
                typeof(bool),
                typeof(ColorColumnHeaderLeaf),
                new FrameworkPropertyMetadata(true));

        public bool ShowColorName
        {
            get => (bool)GetValue(ShowColorNameProperty);
            set => SetValue(ShowColorNameProperty, value);
        }

        /// <summary>
        /// Разрешить редактирование цвета (открывать ColorPicker). По умолчанию false.
        /// </summary>
        public static readonly DependencyProperty EditableProperty =
            DependencyProperty.Register(
                "Editable",
                typeof(bool),
                typeof(ColorColumnHeaderLeaf),
                new FrameworkPropertyMetadata(false));

        public bool Editable
        {
            get => (bool)GetValue(EditableProperty);
            set => SetValue(EditableProperty, value);
        }

        #endregion
    }
}