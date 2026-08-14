using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MakroFlexGrid.Rows
{
    /// <summary>
    /// Единый attached behavior для настройки элементов управления в шаблонах ячеек.
    /// Заменяет отдельные классы EditableCellBehavior, ComboBoxCellBehavior и т.д.
    /// 
    /// Использование в XAML:
    /// <code>
    /// <TextBox rows:CellBehaviorBase.IsEnabled="True" />
    /// </code>
    /// 
    /// Регистрация обработчиков (при старте приложения):
    /// <code>
    /// CellBehaviorBase.RegisterSetupHandler<TextBox>(OnTextBoxSetup);
    /// CellBehaviorBase.RegisterHandler<TextBox, MouseButtonEventArgs>(OnTextBoxPreviewMouseDown);
    /// </code>
    /// </summary>
    public static class CellBehaviorBase
    {
        #region IsEnabled attached property

        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof(bool),
                typeof(CellBehaviorBase),
                new FrameworkPropertyMetadata(false, OnIsEnabledChanged));

        public static bool GetIsEnabled(DependencyObject obj)
            => (bool)obj.GetValue(IsEnabledProperty);

        public static void SetIsEnabled(DependencyObject obj, bool value)
            => obj.SetValue(IsEnabledProperty, value);

        #endregion

        #region CellType attached property

        /// <summary>
        /// Опциональное свойство для указания типа ячейки.
        /// Позволяет разным типам ячеек иметь разные обработчики
        /// для одного и того же типа элемента управления.
        /// </summary>
        public static readonly DependencyProperty CellTypeProperty =
            DependencyProperty.RegisterAttached(
                "CellType",
                typeof(string),
                typeof(CellBehaviorBase),
                new FrameworkPropertyMetadata(null));

        public static string GetCellType(DependencyObject obj)
            => (string)obj.GetValue(CellTypeProperty);

        public static void SetCellType(DependencyObject obj, string value)
            => obj.SetValue(CellTypeProperty, value);

        #endregion

        #region Handler Registration

        /// <summary>
        /// Делегат для настройки элемента управления.
        /// </summary>
        public delegate void SetupHandler<T>(T element) where T : FrameworkElement;

        /// <summary>
        /// Делегат для обработки события.
        /// </summary>
        public delegate void EventHandler<TElement, TEventArgs>(TElement element, TEventArgs args)
            where TElement : FrameworkElement
            where TEventArgs : EventArgs;

        // Словари зарегистрированных обработчиков
        private static readonly Dictionary<Type, List<object>> _setupHandlers = new Dictionary<Type, List<object>>();
        private static readonly Dictionary<string, List<Delegate>> _eventHandlers = new Dictionary<string, List<Delegate>>();

        /// <summary>
        /// Регистрирует обработчик настройки для указанного типа элемента.
        /// Вызывается при загрузке элемента в визуальном дереве.
        /// </summary>
        public static void RegisterSetupHandler<T>(SetupHandler<T> handler) where T : FrameworkElement
        {
            var type = typeof(T);
            if (!_setupHandlers.TryGetValue(type, out var handlers))
            {
                handlers = new List<object>();
                _setupHandlers[type] = handlers;
            }
            handlers.Add(handler);
        }

        /// <summary>
        /// Регистрирует обработчик события для указанного типа элемента.
        /// </summary>
        public static void RegisterEventHandler<TElement, TEventArgs>(
            string eventName,
            EventHandler<TElement, TEventArgs> handler)
            where TElement : FrameworkElement
            where TEventArgs : EventArgs
        {
            var key = $"{typeof(TElement).FullName}:{eventName}";
            if (!_eventHandlers.TryGetValue(key, out var handlers))
            {
                handlers = new List<Delegate>();
                _eventHandlers[key] = handlers;
            }
            handlers.Add(handler);
        }

        #endregion

        #region Event Handling

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement element && (bool)e.NewValue)
            {
                element.Loaded += OnElementLoaded;
                element.Unloaded += OnElementUnloaded;
            }
            else if (d is FrameworkElement oldElement)
            {
                oldElement.Loaded -= OnElementLoaded;
                oldElement.Unloaded -= OnElementUnloaded;
                UnsubscribeEvents(oldElement);
            }
        }

        private static void OnElementLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                element.Loaded -= OnElementLoaded;
                SetupElement(element);
                SubscribeEvents(element);
            }
        }

        private static void OnElementUnloaded(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                element.Unloaded -= OnElementUnloaded;
                UnsubscribeEvents(element);
            }
        }

        private static void SetupElement(FrameworkElement element)
        {
            var elementType = element.GetType();

            // Вызываем все зарегистрированные setup-обработчики для этого типа
            if (_setupHandlers.TryGetValue(elementType, out var handlers))
            {
                foreach (var handler in handlers)
                {
                    try
                    {
                        ((Delegate)handler).DynamicInvoke(element);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            $"[CellBehaviorBase] Setup handler failed for {elementType.Name}: {ex.Message}");
                    }
                }
            }

            // Также проверяем базовые типы (например, TextBox наследует Control)
            CheckBaseTypes(element, elementType);
        }

        private static void CheckBaseTypes(FrameworkElement element, Type elementType)
        {
            var baseType = elementType.BaseType;
            while (baseType != null && baseType != typeof(object) && baseType != typeof(DependencyObject))
            {
                if (_setupHandlers.TryGetValue(baseType, out var handlers))
                {
                    foreach (var handler in handlers)
                    {
                        try
                        {
                            ((Delegate)handler).DynamicInvoke(element);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine(
                                $"[CellBehaviorBase] Setup handler failed for {baseType.Name}: {ex.Message}");
                        }
                    }
                }
                baseType = baseType.BaseType;
            }
        }

        private static void SubscribeEvents(FrameworkElement element)
        {
            var elementType = element.GetType();
            var cellType = GetCellType(element);

            // Ищем обработчики для точного типа + cellType
            foreach (var kvp in _eventHandlers)
            {
                var keyParts = kvp.Key.Split(':');
                if (keyParts.Length != 2) continue;

                var handlerTypeName = keyParts[0];
                var eventName = keyParts[1];

                if (handlerTypeName != elementType.FullName)
                    continue;

                foreach (var handler in kvp.Value)
                {
                    SubscribeToEvent(element, eventName, handler);
                }
            }
        }

        private static void SubscribeToEvent(FrameworkElement element, string eventName, Delegate handler)
        {
            try
            {
                var eventInfo = element.GetType().GetEvent(eventName);
                if (eventInfo != null)
                {
                    eventInfo.AddEventHandler(element, handler);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[CellBehaviorBase] Failed to subscribe to {eventName}: {ex.Message}");
            }
        }

        private static void UnsubscribeEvents(FrameworkElement element)
        {
            var elementType = element.GetType();

            foreach (var kvp in _eventHandlers)
            {
                var keyParts = kvp.Key.Split(':');
                if (keyParts.Length != 2) continue;

                var handlerTypeName = keyParts[0];
                var eventName = keyParts[1];

                if (handlerTypeName != elementType.FullName)
                    continue;

                foreach (var handler in kvp.Value)
                {
                    UnsubscribeFromEvent(element, eventName, handler);
                }
            }
        }

        private static void UnsubscribeFromEvent(FrameworkElement element, string eventName, Delegate handler)
        {
            try
            {
                var eventInfo = element.GetType().GetEvent(eventName);
                if (eventInfo != null)
                {
                    eventInfo.RemoveEventHandler(element, handler);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[CellBehaviorBase] Failed to unsubscribe from {eventName}: {ex.Message}");
            }
        }

        #endregion
    }
}