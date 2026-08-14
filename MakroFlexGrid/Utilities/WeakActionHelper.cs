using System.Reflection;

namespace MakroFlexGrid.Utilities
{
    /// <summary>
    /// Вспомогательный класс для создания слабых подписок на события.
    /// Позволяет подписчику быть собранным GC, даже если он не отписался явно.
    ///
    /// ВАЖНО: Не хранит оригинальный делегат, чтобы избежать сильной ссылки на target.
    /// Вместо этого использует WeakReference на target и MethodInfo для вызова метода.
    /// Оригинальный делегат используется ТОЛЬКО для получения MethodInfo, после чего
    /// он может быть собран GC (лямбда внутри этого метода не захватывает делегат).
    ///
    /// Использование:
    ///   source.Event += WeakActionHelper.CreateWeakAction(target, target.HandlerMethod);
    ///
    /// Где target — объект, который подписывается, а HandlerMethod — его instance-метод.
    /// Если target будет собран GC, делегат перестаёт вызывать HandlerMethod.
    /// </summary>
    public static class WeakActionHelper
    {
        /// <summary>
        /// Создаёт слабую подписку на событие Action{T}.
        /// Не хранит оригинальный делегат — использует WeakReference и MethodInfo.
        /// </summary>
        public static Action<T> CreateWeakAction<T>(object subscriber, Action<T> action)
        {
            if (subscriber == null) throw new ArgumentNullException(nameof(subscriber));
            if (action == null) throw new ArgumentNullException(nameof(action));

            var weakRef = new WeakReference(subscriber);
            var methodInfo = action.Method;

            Action<T> weakDelegate = (param) =>
            {
                var target = weakRef.Target;
                if (target == null) return;

                try
                {
                    methodInfo.Invoke(target, new object[] { param });
                }
                catch
                {
                    // Игнорируем ошибки вызова через рефлексию
                }
            };

            return weakDelegate;
        }

        /// <summary>
        /// Создаёт слабую подписку на событие Action{T} с автоматической отпиской.
        /// </summary>
        /// <typeparam name="T">Тип параметра события.</typeparam>
        /// <param name="subscriber">Объект-подписчик.</param>
        /// <param name="action">Метод-обработчик подписчика.</param>
        /// <param name="unsubscribeAction">Действие для отписки созданного делегата (например, source.Event -= handler).</param>
        /// <returns>Делегат для подписки на событие.</returns>
        public static Action<T> CreateWeakAction<T>(object subscriber, Action<T> action, Action<Action<T>> unsubscribeAction)
        {
            if (subscriber == null) throw new ArgumentNullException(nameof(subscriber));
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (unsubscribeAction == null) throw new ArgumentNullException(nameof(unsubscribeAction));

            var weakRef = new WeakReference(subscriber);
            var methodInfo = action.Method;
            var weakDelegateRef = new WeakReference<Action<T>>(null);

            Action<T> weakDelegate = (param) =>
            {
                var target = weakRef.Target;
                if (target != null)
                {
                    try
                    {
                        methodInfo.Invoke(target, new object[] { param });
                    }
                    catch
                    {
                        // Игнорируем ошибки вызова через рефлексию
                    }
                }
                else
                {
                    // Подписчик собран GC — автоматически отписываемся.
                    // Используем WeakReference<Action<T>> для хранения weakDelegate,
                    // чтобы избежать цикла ссылок через замыкание.
                    if (weakDelegateRef.TryGetTarget(out var del))
                    {
                        unsubscribeAction(del);
                    }
                }
            };

            weakDelegateRef.SetTarget(weakDelegate);
            return weakDelegate;
        }

        /// <summary>
        /// Создаёт слабую подписку на событие EventHandler.
        /// Не хранит оригинальный делегат — использует WeakReference и MethodInfo.
        /// </summary>
        public static EventHandler CreateWeakEventHandler(object subscriber, EventHandler handler)
        {
            if (subscriber == null) throw new ArgumentNullException(nameof(subscriber));
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            var weakRef = new WeakReference(subscriber);
            var methodInfo = handler.Method;

            return (sender, args) =>
            {
                var target = weakRef.Target;
                if (target == null) return;

                try
                {
                    methodInfo.Invoke(target, new object[] { sender, args });
                }
                catch
                {
                    // Игнорируем ошибки вызова через рефлексию
                }
            };
        }

        /// <summary>
        /// Создаёт слабую подписку на событие EventHandler с автоматической отпиской.
        /// </summary>
        /// <param name="subscriber">Объект-подписчик.</param>
        /// <param name="handler">Метод-обработчик подписчика.</param>
        /// <param name="unsubscribeAction">Действие для отписки созданного делегата (например, source.Event -= handler).</param>
        /// <returns>Делегат для подписки на событие.</returns>
        public static EventHandler CreateWeakEventHandler(object subscriber, EventHandler handler, Action<EventHandler> unsubscribeAction)
        {
            if (subscriber == null) throw new ArgumentNullException(nameof(subscriber));
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            if (unsubscribeAction == null) throw new ArgumentNullException(nameof(unsubscribeAction));

            var weakRef = new WeakReference(subscriber);
            var methodInfo = handler.Method;
            var weakDelegateRef = new WeakReference<EventHandler>(null);

            EventHandler weakDelegate = (sender, args) =>
            {
                var target = weakRef.Target;
                if (target != null)
                {
                    try
                    {
                        methodInfo.Invoke(target, new object[] { sender, args });
                    }
                    catch
                    {
                        // Игнорируем ошибки вызова через рефлексию
                    }
                }
                else
                {
                    // Подписчик собран GC — автоматически отписываемся.
                    // Используем WeakReference<EventHandler> для хранения weakDelegate,
                    // чтобы избежать цикла ссылок через замыкание.
                    if (weakDelegateRef.TryGetTarget(out var del))
                    {
                        unsubscribeAction(del);
                    }
                }
            };

            weakDelegateRef.SetTarget(weakDelegate);
            return weakDelegate;
        }
    }

    /// <summary>
    /// Представляет слабую подписку на событие с автоматической отпиской
    /// при сборке GC подписчика.
    /// </summary>
    /// <typeparam name="TEventArgs">Тип аргументов события.</typeparam>
    public sealed class WeakEventSubscription<TEventArgs> : IDisposable
    {
        private readonly WeakReference _weakSubscriber;
        private readonly MethodInfo _methodInfo;
        private readonly Action<EventHandler> _subscribeAction;
        private readonly Action<EventHandler> _unsubscribeAction;
        private readonly EventHandler _weakHandler;
        private bool _disposed;

        /// <summary>
        /// Создаёт слабую подписку на событие.
        /// </summary>
        /// <param name="subscriber">Объект-подписчик (будет храниться через WeakReference).</param>
        /// <param name="handler">Метод-обработчик подписчика.</param>
        /// <param name="subscribeAction">Действие для подписки (например, () => source.Event += handler).</param>
        /// <param name="unsubscribeAction">Действие для отписки (например, handler => source.Event -= handler).</param>
        public WeakEventSubscription(
            object subscriber,
            EventHandler handler,
            Action<EventHandler> subscribeAction,
            Action<EventHandler> unsubscribeAction)
        {
            if (subscriber == null) throw new ArgumentNullException(nameof(subscriber));
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            if (subscribeAction == null) throw new ArgumentNullException(nameof(subscribeAction));
            if (unsubscribeAction == null) throw new ArgumentNullException(nameof(unsubscribeAction));

            _weakSubscriber = new WeakReference(subscriber);
            _methodInfo = handler.Method;
            _subscribeAction = subscribeAction;
            _unsubscribeAction = unsubscribeAction;
            _weakHandler = OnEvent;

            // Подписываемся на событие через слабый делегат
            _subscribeAction(_weakHandler);
        }

        /// <summary>
        /// Вызывается при возникновении события.
        /// Если подписчик ещё жив — вызывает его метод через рефлексию.
        /// Если подписчик собран GC — автоматически отписывается.
        /// </summary>
        private void OnEvent(object sender, EventArgs e)
        {
            if (_disposed) return;

            var target = _weakSubscriber.Target;
            if (target != null)
            {
                try
                {
                    _methodInfo.Invoke(target, new[] { sender, e });
                }
                catch
                {
                    // Игнорируем ошибки вызова через рефлексию
                }
            }
            else
            {
                // Подписчик собран GC — автоматически отписываемся
                Dispose();
            }
        }

        /// <summary>
        /// Явная отписка от события.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _unsubscribeAction(_weakHandler);
            }
        }
    }
}
