using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace MakroFlexGrid.Headers
{
    /// <summary>
    /// Листовая колонка для отображения радиокнопок.
    /// Позволяет выбрать один вариант из нескольких в пределах группы.
    /// Параметры автоматически копируются в CellViewModel.Config через ApplyHeaderConfig().
    /// </summary>
    [CellTemplate("UnifiedCellTemplate")]
    public class RadioButtonColumnHeaderLeaf : ColumnHeaderLeaf
    {
        static RadioButtonColumnHeaderLeaf()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(RadioButtonColumnHeaderLeaf),
                new FrameworkPropertyMetadata(typeof(RadioButtonColumnHeaderLeaf)));
        }

        #region Dependency Properties

        /// <summary>
        /// Имя группы радиокнопок. Все RadioButton в одной группе — взаимоисключающие.
        /// </summary>
        public static readonly DependencyProperty GroupNameProperty =
            DependencyProperty.Register(
                "GroupName",
                typeof(string),
                typeof(RadioButtonColumnHeaderLeaf),
                new FrameworkPropertyMetadata(null));

        public string GroupName
        {
            get => (string)GetValue(GroupNameProperty);
            set => SetValue(GroupNameProperty, value);
        }

        /// <summary>
        /// Список вариантов для выбора.
        /// </summary>
        public static readonly DependencyProperty OptionsProperty =
            DependencyProperty.Register(
                "Options",
                typeof(IEnumerable),
                typeof(RadioButtonColumnHeaderLeaf),
                new FrameworkPropertyMetadata(null));

        public IEnumerable Options
        {
            get => (IEnumerable)GetValue(OptionsProperty);
            set => SetValue(OptionsProperty, value);
        }

        #endregion
    }
}