using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace MakroFlexGrid.Headers
{
    /// <summary>
    /// Листовая колонка с выпадающим списком (ComboBox) для выбора значения.
    /// Позволяет задать источник данных, отображаемое поле и поле значения.
    /// Шаблон загружается автоматически через атрибут [CellTemplate].
    /// </summary>
    [CellTemplate("ComboBoxCellTemplate")]
    public class ComboBoxColumnHeaderLeaf : ColumnHeaderLeaf
    {
        static ComboBoxColumnHeaderLeaf()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ComboBoxColumnHeaderLeaf),
                new FrameworkPropertyMetadata(typeof(ComboBoxColumnHeaderLeaf)));
        }

        // Конструктор не требуется — CellTemplate загружается автоматически
        // через ColumnHeaderLeaf.LoadCellTemplateFromAttribute()

        #region Dependency Properties

        /// <summary>
        /// Источник данных для ComboBox в ячейках этой колонки.
        /// </summary>
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                "ItemsSource",
                typeof(IEnumerable),
                typeof(ComboBoxColumnHeaderLeaf),
                new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Путь к отображаемому свойству в элементах ItemsSource.
        /// Аналог DisplayMemberPath в ComboBox.
        /// </summary>
        public static readonly DependencyProperty DisplayMemberPathProperty =
            DependencyProperty.Register(
                "DisplayMemberPath",
                typeof(string),
                typeof(ComboBoxColumnHeaderLeaf),
                new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Путь к свойству значения в элементах ItemsSource.
        /// Аналог SelectedValuePath в ComboBox.
        /// </summary>
        public static readonly DependencyProperty SelectedValuePathProperty =
            DependencyProperty.Register(
                "SelectedValuePath",
                typeof(string),
                typeof(ComboBoxColumnHeaderLeaf),
                new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Путь к свойству в Item (модели данных строки), в которое будет сохраняться
        /// выбранное значение ComboBox. Используется для TwoWay-привязки SelectedValue.
        /// </summary>
        public static readonly DependencyProperty SelectedValueBindingProperty =
            DependencyProperty.Register(
                "SelectedValueBinding",
                typeof(string),
                typeof(ComboBoxColumnHeaderLeaf),
                new FrameworkPropertyMetadata(null));

        #endregion

        #region CLR Properties

        /// <summary>
        /// Источник данных для ComboBox в ячейках этой колонки.
        /// </summary>
        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        /// <summary>
        /// Путь к отображаемому свойству в элементах ItemsSource.
        /// Аналог DisplayMemberPath в ComboBox.
        /// </summary>
        public string DisplayMemberPath
        {
            get => (string)GetValue(DisplayMemberPathProperty);
            set => SetValue(DisplayMemberPathProperty, value);
        }

        /// <summary>
        /// Путь к свойству значения в элементах ItemsSource.
        /// Аналог SelectedValuePath в ComboBox.
        /// </summary>
        public string SelectedValuePath
        {
            get => (string)GetValue(SelectedValuePathProperty);
            set => SetValue(SelectedValuePathProperty, value);
        }

        /// <summary>
        /// Путь к свойству в Item (модели данных строки), в которое будет сохраняться
        /// выбранное значение ComboBox. Используется для TwoWay-привязки SelectedValue.
        /// </summary>
        public string SelectedValueBinding
        {
            get => (string)GetValue(SelectedValueBindingProperty);
            set => SetValue(SelectedValueBindingProperty, value);
        }

        #endregion
    }
}