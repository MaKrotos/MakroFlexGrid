using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MakroFlexGrid.Rows
{
    /// <summary>
    /// Attached behavior для реализации редактирования ячеек в EditableCellTemplate.
    /// Позволяет обрабатывать клик для входа в режим редактирования,
    /// потерю фокуса и Enter для сохранения, Escape для отмены.
    ///
    /// Используется в DataTemplate через attached property,
    /// что позволяет обойти ограничение DataTemplate (отсутствие code-behind).
    ///
    /// Также регистрирует обработчики через CellBehaviorBase для использования
    /// в UnifiedCellTemplate.
    /// </summary>
    public static class EditableCellBehavior
    {
        /// <summary>
        /// Статический конструктор — регистрирует обработчики через CellBehaviorBase.
        /// Позволяет использовать EditableCellBehavior как через старый attached property,
        /// так и через новый CellBehaviorBase.IsEnabled.
        /// </summary>
        static EditableCellBehavior()
        {
            // Регистрируем setup-обработчик для Grid (контейнер ячейки)
            CellBehaviorBase.RegisterSetupHandler<System.Windows.Controls.Grid>(grid =>
            {
                grid.PreviewMouseDown += OnElementPreviewMouseDown;
                grid.Loaded += OnElementLoaded;
            });

            // Регистрируем setup-обработчик для TextBox
            CellBehaviorBase.RegisterSetupHandler<TextBox>(textBox =>
            {
                textBox.LostFocus += OnTextBoxLostFocus;
                textBox.KeyDown += OnTextBoxKeyDown;
            });
        }

        #region IsEnabled attached property

        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof(bool),
                typeof(EditableCellBehavior),
                new FrameworkPropertyMetadata(false, OnIsEnabledChanged));

        public static bool GetIsEnabled(DependencyObject obj)
            => (bool)obj.GetValue(IsEnabledProperty);

        public static void SetIsEnabled(DependencyObject obj, bool value)
            => obj.SetValue(IsEnabledProperty, value);

        #endregion

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement element)
            {
                if ((bool)e.NewValue)
                {
                    // Используем PreviewMouseDown (tunneling) вместо MouseDown (bubbling),
                    // так как DataGrid может перехватывать MouseDown для своей логики
                    // (выделение строки, начало редактирования).
                    // PreviewMouseDown гарантированно доходит до элемента.
                    element.PreviewMouseDown += OnElementPreviewMouseDown;
                    element.Loaded += OnElementLoaded;
                }
                else
                {
                    element.PreviewMouseDown -= OnElementPreviewMouseDown;
                    element.Loaded -= OnElementLoaded;
                    UnsubscribeTextBoxEvents(element);
                }
            }
        }

        private static void OnElementLoaded(object sender, RoutedEventArgs e)
        {
            // При загрузке находим TextBox и подписываемся на его события
            if (sender is FrameworkElement element)
            {
                SubscribeToTextBoxEvents(element);
            }
        }

        private static void OnElementPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                // Проверяем, что клик был именно по TextBlock (режим просмотра)
                if (e.OriginalSource is TextBlock || e.OriginalSource is Border)
                {
                    var dataContext = element.DataContext;
                    if (dataContext != null)
                    {
                        // Устанавливаем IsEditing через рефлексию или через поиск свойства
                        SetIsEditing(dataContext, true);
                        e.Handled = true;

                        // После входа в режим редактирования фокусируем TextBox.
                        // Используем ContextIdle, чтобы WPF успел применить триггеры
                        // (сделать TextBox видимым) до того, как мы попытаемся сфокусироваться.
                        element.Dispatcher.BeginInvoke(
                            new System.Action(() => FocusTextBox(element)),
                            System.Windows.Threading.DispatcherPriority.ContextIdle);
                    }
                }
            }
        }

        private static void SubscribeToTextBoxEvents(FrameworkElement element)
        {
            var textBox = FindTextBox(element);
            if (textBox != null)
            {
                textBox.LostFocus += OnTextBoxLostFocus;
                textBox.KeyDown += OnTextBoxKeyDown;
            }
        }

        private static void UnsubscribeTextBoxEvents(FrameworkElement element)
        {
            var textBox = FindTextBox(element);
            if (textBox != null)
            {
                textBox.LostFocus -= OnTextBoxLostFocus;
                textBox.KeyDown -= OnTextBoxKeyDown;
            }
        }

        private static void OnTextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            // При потере фокуса сохраняем значение.
            // Проверяем, что TextBox действительно был в фокусе (IsVisible),
            // чтобы не срабатывало при начальной загрузке, когда TextBox
            // только появился, но ещё не получил фокус.
            if (sender is TextBox textBox && textBox.IsVisible)
            {
                var dataContext = textBox.DataContext;
                if (dataContext != null)
                {
                    CommitEdit(dataContext);
                }
            }
        }

        private static void OnTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (sender is FrameworkElement textBox)
            {
                var dataContext = textBox.DataContext;
                if (dataContext == null) return;

                if (e.Key == Key.Enter)
                {
                    // Enter — сохраняем
                    CommitEdit(dataContext);
                    e.Handled = true;
                }
                else if (e.Key == Key.Escape)
                {
                    // Escape — отменяем
                    CancelEdit(dataContext);
                    e.Handled = true;
                }
            }
        }

        private static void FocusTextBox(FrameworkElement element)
        {
            var textBox = FindTextBox(element);
            if (textBox != null)
            {
                textBox.Focus();
                // Дополнительно выбираем весь текст для удобства редактирования
                textBox.SelectAll();
            }
        }

        private static TextBox FindTextBox(FrameworkElement element)
        {
            // Сначала пробуем FindName (работает для именованных элементов в NameScope)
            var textBox = element.FindName("EditBox") as TextBox;
            if (textBox != null)
                return textBox;

            // Если не нашли через FindName, ищем через визуальное дерево
            return FindTextBoxInVisualTree(element);
        }

        private static TextBox FindTextBoxInVisualTree(DependencyObject parent)
        {
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is TextBox tb && tb.Name == "EditBox")
                    return tb;

                var found = FindTextBoxInVisualTree(child);
                if (found != null)
                    return found;
            }
            return null;
        }

        private static void SetIsEditing(object dataContext, bool value)
        {
            var prop = dataContext.GetType().GetProperty("IsEditing");
            if (prop != null && prop.CanWrite)
            {
                prop.SetValue(dataContext, value);
            }
        }

        private static void CommitEdit(object dataContext)
        {
            var commitMethod = dataContext.GetType().GetMethod("CommitEdit");
            if (commitMethod != null)
            {
                commitMethod.Invoke(dataContext, null);
            }
            else
            {
                // Fallback: просто выходим из режима редактирования
                SetIsEditing(dataContext, false);
            }
        }

        private static void CancelEdit(object dataContext)
        {
            var cancelMethod = dataContext.GetType().GetMethod("CancelEdit");
            if (cancelMethod != null)
            {
                cancelMethod.Invoke(dataContext, null);
            }
            else
            {
                SetIsEditing(dataContext, false);
            }
        }
    }
}