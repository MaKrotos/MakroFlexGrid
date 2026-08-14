using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MakroFlexGrid.Rows
{
    /// <summary>
    /// Attached behavior для настройки ComboBox в ComboBoxCellTemplate.
    /// При загрузке элемента считывает настройки из CellViewModel
    /// (ItemsSource, DisplayMemberPath, SelectedValuePath, SelectedValueBinding)
    /// и применяет их к ComboBox, включая динамическую привязку SelectedValue к Item.
    ///
    /// Также регистрирует обработчики через CellBehaviorBase для использования
    /// в UnifiedCellTemplate.
    /// </summary>
    public static class ComboBoxCellBehavior
    {
        /// <summary>
        /// Статический конструктор — регистрирует обработчик через CellBehaviorBase.
        /// </summary>
        static ComboBoxCellBehavior()
        {
            CellBehaviorBase.RegisterSetupHandler<ComboBox>(SetupComboBox);
        }

        #region IsEnabled attached property

        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof(bool),
                typeof(ComboBoxCellBehavior),
                new FrameworkPropertyMetadata(false, OnIsEnabledChanged));

        public static bool GetIsEnabled(DependencyObject obj)
            => (bool)obj.GetValue(IsEnabledProperty);

        public static void SetIsEnabled(DependencyObject obj, bool value)
            => obj.SetValue(IsEnabledProperty, value);

        #endregion

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ComboBox comboBox && (bool)e.NewValue)
            {
                comboBox.Loaded += OnComboBoxLoaded;
            }
        }

        private static void OnComboBoxLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                comboBox.Loaded -= OnComboBoxLoaded;
                SetupComboBox(comboBox);
            }
        }

        private static void SetupComboBox(ComboBox comboBox)
        {
            var dataContext = comboBox.DataContext;
            if (dataContext == null) return;

            var dataContextType = dataContext.GetType();

            // ItemsSource
            var itemsSourceProp = dataContextType.GetProperty("ItemsSource");
            if (itemsSourceProp != null)
            {
                var itemsSource = itemsSourceProp.GetValue(dataContext);
                if (itemsSource is System.Collections.IEnumerable enumerable)
                {
                    comboBox.ItemsSource = enumerable;
                }
            }

            // DisplayMemberPath
            var displayMemberProp = dataContextType.GetProperty("ComboBoxDisplayMemberPath");
            if (displayMemberProp != null)
            {
                var displayMember = displayMemberProp.GetValue(dataContext) as string;
                if (!string.IsNullOrEmpty(displayMember))
                {
                    comboBox.DisplayMemberPath = displayMember;
                }
            }

            // SelectedValuePath
            var selectedValuePathProp = dataContextType.GetProperty("ComboBoxSelectedValuePath");
            if (selectedValuePathProp != null)
            {
                var selectedValuePath = selectedValuePathProp.GetValue(dataContext) as string;
                if (!string.IsNullOrEmpty(selectedValuePath))
                {
                    comboBox.SelectedValuePath = selectedValuePath;
                }
            }

            // SelectedValueBinding — динамическая привязка к Item.{SelectedValueBinding}
            var selectedValueBindingProp = dataContextType.GetProperty("ComboBoxSelectedValueBinding");
            if (selectedValueBindingProp != null)
            {
                var selectedValueBinding = selectedValueBindingProp.GetValue(dataContext) as string;
                if (!string.IsNullOrEmpty(selectedValueBinding))
                {
                    // Создаём привязку к Item.{SelectedValueBinding}
                    var binding = new Binding($"Item.{selectedValueBinding}")
                    {
                        Mode = BindingMode.TwoWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    };
                    comboBox.SetBinding(ComboBox.SelectedValueProperty, binding);
                }
            }
        }
    }
}