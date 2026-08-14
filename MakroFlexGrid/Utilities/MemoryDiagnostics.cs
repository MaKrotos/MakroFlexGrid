#if DEBUG
using System.Diagnostics;
using System.Windows.Threading;

namespace MakroFlexGrid.Utilities
{
    /// <summary>
    /// Диагностика утечек памяти. Работает только в DEBUG-сборках.
    /// Отслеживает количество созданных/удалённых объектов, Binding-ов и подписок.
    /// Автоматически логирует снапшот каждые 5 секунд через DispatcherTimer.
    /// </summary>
    public static class MemoryDiagnostics
    {
        // Счётчики объектов
        private static int _rowViewModelCreated;
        private static int _rowViewModelDisposed;
        private static int _cellViewModelCreated;
        private static int _cellViewModelDisposed;
        private static int _rowContainerCreated;
        private static int _rowContainerCleared;

        // Счётчики Binding
        private static int _bindingsCreated;
        private static int _bindingsCleared;

        // Счётчики слабых подписок
        private static int _weakSubscriptionsCreated;
        private static int _weakSubscriptionsDisposed;

        // GC статистика
        private static int _lastGen0;
        private static int _lastGen1;
        private static int _lastGen2;
        private static long _lastMemory;

        // Таймер для автоматического логирования
        private static DispatcherTimer _timer;
        private static bool _initialized;

        // Порог троттлинга для ручных вызовов LogSnapshot
        private static readonly TimeSpan LogThrottle = TimeSpan.FromMilliseconds(1000);
        private static DateTime _lastLogTime = DateTime.MinValue;

        /// <summary>
        /// Инициализирует таймер автоматического логирования (каждые 5 секунд).
        /// Вызывается один раз при старте приложения.
        /// </summary>
        public static void Initialize()
        {
            if (_initialized) return;
            _initialized = true;

            _timer = new DispatcherTimer(
                TimeSpan.FromSeconds(5),
                DispatcherPriority.Background,
                (s, e) => LogSnapshot("Timer"),
                Dispatcher.CurrentDispatcher);
            _timer.Start();

            Debug.WriteLine("[MemoryDiagnostics] ? Инициализирован. Логирование каждые 5с.");
        }

        /// <summary>
        /// Останавливает таймер и сбрасывает счётчики.
        /// </summary>
        public static void Shutdown()
        {
            _timer?.Stop();
            _timer = null;
            _initialized = false;
            Reset();
            Debug.WriteLine("[MemoryDiagnostics] ? Остановлен.");
        }

        // ===== RowViewModel =====
        public static void OnRowViewModelCreated()
        {
            Interlocked.Increment(ref _rowViewModelCreated);
            var alive = _rowViewModelCreated - _rowViewModelDisposed;
            if (alive > 0 && alive % 100 == 0)
                LogSnapshot($"RowViewModel x{alive}");
        }

        public static void OnRowViewModelDisposed()
        {
            Interlocked.Increment(ref _rowViewModelDisposed);
        }

        // ===== CellViewModel =====
        public static void OnCellViewModelCreated()
        {
            Interlocked.Increment(ref _cellViewModelCreated);
            var alive = _cellViewModelCreated - _cellViewModelDisposed;
            if (alive > 0 && alive % 500 == 0)
                LogSnapshot($"CellViewModel x{alive}");
        }

        public static void OnCellViewModelDisposed()
        {
            Interlocked.Increment(ref _cellViewModelDisposed);
        }

        // ===== RowContainer =====
        public static void OnRowContainerCreated()
        {
            Interlocked.Increment(ref _rowContainerCreated);
            var alive = _rowContainerCreated - _rowContainerCleared;
            if (alive > 0 && alive % 100 == 0)
                LogSnapshot($"RowContainer x{alive}");
        }

        public static void OnRowContainerCleared()
        {
            Interlocked.Increment(ref _rowContainerCleared);
        }

        // ===== Binding =====
        public static void OnBindingCreated()
        {
            Interlocked.Increment(ref _bindingsCreated);
        }

        public static void OnBindingCleared()
        {
            Interlocked.Increment(ref _bindingsCleared);
        }

        // ===== Weak Subscriptions =====
        public static void OnWeakSubscriptionCreated()
        {
            Interlocked.Increment(ref _weakSubscriptionsCreated);
        }

        public static void OnWeakSubscriptionDisposed()
        {
            Interlocked.Increment(ref _weakSubscriptionsDisposed);
        }

        /// <summary>
        /// Логирует текущий снапшот состояния счётчиков.
        /// </summary>
        public static void LogSnapshot(string context = "")
        {
            var now = DateTime.Now;
            if (now - _lastLogTime < LogThrottle)
                return;
            _lastLogTime = now;

            var rvAlive = _rowViewModelCreated - _rowViewModelDisposed;
            var cvAlive = _cellViewModelCreated - _cellViewModelDisposed;
            var rcAlive = _rowContainerCreated - _rowContainerCleared;
            var bindingLeaked = _bindingsCreated - _bindingsCleared;
            var wsAlive = _weakSubscriptionsCreated - _weakSubscriptionsDisposed;

            var gen0 = GC.CollectionCount(0);
            var gen1 = GC.CollectionCount(1);
            var gen2 = GC.CollectionCount(2);
            var memory = GC.GetTotalMemory(false);

            var gen0Delta = gen0 - _lastGen0;
            var gen1Delta = gen1 - _lastGen1;
            var gen2Delta = gen2 - _lastGen2;
            var memDelta = memory - _lastMemory;

            _lastGen0 = gen0;
            _lastGen1 = gen1;
            _lastGen2 = gen2;
            _lastMemory = memory;

            var ctx = string.IsNullOrEmpty(context) ? "" : $" [{context}]";

            Debug.WriteLine($"[{now:HH:mm:ss.fff}] ?? MEMORY SNAPSHOT{ctx}");
            Debug.WriteLine($"  RowViewModel:    Created={_rowViewModelCreated,6} | Disposed={_rowViewModelDisposed,6} | Alive={rvAlive,6}");
            Debug.WriteLine($"  CellViewModel:   Created={_cellViewModelCreated,6} | Disposed={_cellViewModelDisposed,6} | Alive={cvAlive,6}");
            Debug.WriteLine($"  RowContainer:    Created={_rowContainerCreated,6} | Cleared={_rowContainerCleared,6} | Alive={rcAlive,6}");
            Debug.WriteLine($"  Bindings:        Created={_bindingsCreated,6} | Cleared={_bindingsCleared,6} | Leaked={bindingLeaked,6}");
            Debug.WriteLine($"  WeakSubs:        Created={_weakSubscriptionsCreated,6} | Disposed={_weakSubscriptionsDisposed,6} | Alive={wsAlive,6}");
            Debug.WriteLine($"  GC:              Gen0={gen0,4} (+{gen0Delta,3}) | Gen1={gen1,4} (+{gen1Delta,3}) | Gen2={gen2,4} (+{gen2Delta,3}) | Mem={memory / 1024 / 1024,4}MB ({memDelta / 1024 / 1024:+0;-0}MB)");
        }

        /// <summary>
        /// Сбрасывает все счётчики в ноль.
        /// </summary>
        public static void Reset()
        {
            _rowViewModelCreated = 0;
            _rowViewModelDisposed = 0;
            _cellViewModelCreated = 0;
            _cellViewModelDisposed = 0;
            _rowContainerCreated = 0;
            _rowContainerCleared = 0;
            _bindingsCreated = 0;
            _bindingsCleared = 0;
            _weakSubscriptionsCreated = 0;
            _weakSubscriptionsDisposed = 0;
            _lastGen0 = 0;
            _lastGen1 = 0;
            _lastGen2 = 0;
            _lastMemory = 0;
            _lastLogTime = DateTime.MinValue;

            Debug.WriteLine("[MemoryDiagnostics] ?? Счётчики сброшены.");
        }
    }
}
#endif
