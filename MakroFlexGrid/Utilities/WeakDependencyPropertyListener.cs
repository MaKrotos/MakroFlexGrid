using System.ComponentModel;
using System.Reflection;

namespace MakroFlexGrid.Utilities
{
    /// <summary>
    /// Слушатель изменений DependencyProperty через слабую ссылку на подписчика.
    /// Автоматически отписывается, когда целевой объект (subscriber) собран GC.
    /// Решает проблему утечки памяти через DependencyPropertyDescriptor.AddValueChanged,
    /// который хранит делегат в глобальном статическом EventHandlerStore.
    /// </summary>
    public sealed class WeakDependencyPropertyListener : IDisposable
    {
        private readonly DependencyPropertyDescriptor _descriptor;
        private readonly object _source;
        private readonly EventHandler _handler;
        private readonly WeakReference _weakTarget;
        private readonly MethodInfo _methodInfo;
        private bool _disposed;

        /// <summary>
        /// Объект-источник, на котором отслеживается изменение DependencyProperty.
        /// </summary>
        internal object Source => _source;

        /// <summary>
        /// Создаёт слабую подписку на изменение DependencyProperty.
        /// </summary>
        /// <param name="descriptor">Дескриптор DependencyProperty.</param>
        /// <param name="source">Объект-источник, на котором отслеживается изменение свойства.</param>
        /// <param name="target">Объект-подписчик (будет храниться через WeakReference).</param>
        /// <param name="handler">Обработчик события (instance-метод target).</param>
        public WeakDependencyPropertyListener(
            DependencyPropertyDescriptor descriptor,
            object source,
            object target,
            EventHandler handler)
        {
            _descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _weakTarget = new WeakReference(target ?? throw new ArgumentNullException(nameof(target)));
            _methodInfo = (handler ?? throw new ArgumentNullException(nameof(handler))).Method;
            _handler = OnEvent;
            descriptor.AddValueChanged(source, _handler);
        }

        /// <summary>
        /// Финализатор гарантирует отписку от глобального EventHandlerStore
        /// даже если Dispose() не был вызван явно. Без финализатора
        /// WeakDependencyPropertyListener навсегда остаётся в EventHandlerStore,
        /// если событие ни разу не произошло после того, как подписчик был собран GC.
        /// </summary>
        ~WeakDependencyPropertyListener()
        {
            Dispose();
        }

        /// <summary>
        /// Вызывается при изменении DependencyProperty.
        /// Если целевой объект ещё жив — вызывает его метод через рефлексию.
        /// Если целевой объект собран GC — автоматически отписывается.
        /// </summary>
        private void OnEvent(object sender, EventArgs e)
        {
            if (_disposed) return;

            var target = _weakTarget.Target;
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
                // Целевой объект собран сборщиком мусора — автоматически отписываемся
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
                _descriptor.RemoveValueChanged(_source, _handler);
                GC.SuppressFinalize(this);
            }
        }
    }
}
