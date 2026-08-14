using System.Windows;
using System.Windows.Controls;

namespace MakroFlexGrid.Headers
{
    /// <summary>
    /// Листовая колонка для отображения и редактирования bool/nullable bool значений
    /// в виде CheckBox. Поддерживает трёх状态 (IsThreeState).
    /// Параметры автоматически копируются в CellViewModel.Config через ApplyHeaderConfig().
    /// </summary>
    [CellTemplate("UnifiedCellTemplate")]
    public class CheckBoxColumnHeaderLeaf : ColumnHeaderLeaf
    {
        static CheckBoxColumnHeaderLeaf()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(CheckBoxColumnHeaderLeaf),
                new FrameworkPropertyMetadata(typeof(CheckBoxColumnHeaderLeaf)));
        }

        #region Dependency Properties

        /// <summary>
        /// Включить трёх状态 (true/false/null). По умолчанию false (два состояния).
        /// </summary>
        public static readonly DependencyProperty IsThreeStateProperty =
            DependencyProperty.Register(
                "IsThreeState",
                typeof(bool),
                typeof(CheckBoxColumnHeaderLeaf),
                new FrameworkPropertyMetadata(false));

        public bool IsThreeState
        {
            get => (bool)GetValue(IsThreeStateProperty);
            set => SetValue(IsThreeStateProperty, value);
        }

        #endregion
    }
}