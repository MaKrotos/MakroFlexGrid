using System.Windows.Controls;

namespace MakroFlexGrid.Scroll
{
    public class ScrollViewerManager : IDisposable
    {
        private readonly ScrollManager _scrollManager;
        private readonly HashSet<ScrollViewer> _activeScrollViewers = new HashSet<ScrollViewer>();
        private readonly object _lockObject = new object();
        private volatile bool _isUpdating;
        private bool _disposed;

        public ScrollViewerManager(ScrollManager scrollManager)
        {
            _scrollManager = scrollManager ?? throw new ArgumentNullException(nameof(scrollManager));
            _scrollManager.HorizontalOffsetChanged += OnGlobalOffsetChanged;
        }

        public void Register(ScrollViewer scrollViewer)
        {
            if (scrollViewer == null || _disposed) return;

            lock (_lockObject)
            {
                if (_activeScrollViewers.Add(scrollViewer))
                {
                    scrollViewer.ScrollChanged += OnScrollViewerScrollChanged;

                    // Синхронизируем начальное положение
                    var targetOffset = _scrollManager.HorizontalOffset;
                    if (Math.Abs(scrollViewer.HorizontalOffset - targetOffset) > 0.01)
                    {
                        scrollViewer.ScrollToHorizontalOffset(targetOffset);
                    }
                }
            }
        }

        public void Unregister(ScrollViewer scrollViewer)
        {
            if (scrollViewer == null || _disposed) return;

            lock (_lockObject)
            {
                if (_activeScrollViewers.Remove(scrollViewer))
                {
                    scrollViewer.ScrollChanged -= OnScrollViewerScrollChanged;
                }
            }
        }

        private void OnScrollViewerScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (_disposed) return;
            if (Math.Abs(e.HorizontalChange) < 0.001) return;

            // Блокируем реентерабельность: ScrollToHorizontalOffset внутри OnGlobalOffsetChanged
            // вызывает ScrollChanged синхронно, что приводит к повторному входу.
            if (_isUpdating) return;

            lock (_lockObject)
            {
                if (_isUpdating) return;
                _isUpdating = true;
            }

            try
            {
                var newOffset = e.HorizontalOffset;
                if (Math.Abs(_scrollManager.HorizontalOffset - newOffset) > 0.01)
                {
                    _scrollManager.HorizontalOffset = newOffset;
                }
            }
            finally
            {
                lock (_lockObject)
                {
                    _isUpdating = false;
                }
            }
        }

        /// <summary>
        /// Синхронизирует все зарегистрированные ScrollViewer с глобальным offset.
        /// Вызывается синхронно (без BeginInvoke), чтобы избежать накопления событий.
        /// </summary>
        private void OnGlobalOffsetChanged(double offset)
        {
            if (_disposed) return;

            // Блокируем реентерабельность: ScrollToHorizontalOffset вызывает ScrollChanged
            // синхронно, что приводит к повторному входу в OnScrollViewerScrollChanged,
            // который пытается установить _scrollManager.HorizontalOffset.
            if (_isUpdating) return;

            lock (_lockObject)
            {
                if (_isUpdating) return;
                _isUpdating = true;
            }

            try
            {
                ScrollViewer[] viewers;
                lock (_lockObject)
                {
                    viewers = new ScrollViewer[_activeScrollViewers.Count];
                    _activeScrollViewers.CopyTo(viewers);
                }

                foreach (var scrollViewer in viewers)
                {
                    try
                    {
                        if (scrollViewer.IsVisible &&
                            Math.Abs(scrollViewer.HorizontalOffset - offset) > 0.01)
                        {
                            scrollViewer.ScrollToHorizontalOffset(offset);
                        }
                    }
                    catch
                    {
                        // Игнорируем ошибки для disposed элементов
                    }
                }
            }
            finally
            {
                lock (_lockObject)
                {
                    _isUpdating = false;
                }
            }
        }

        /// <summary>
        /// Очищает все зарегистрированные ScrollViewer без полной утилизации менеджера.
        /// Также отписывается от ScrollManager, чтобы не получать лишние уведомления.
        /// </summary>
        public void ClearAll()
        {
            if (_disposed) return;

            lock (_lockObject)
            {
                foreach (var sv in _activeScrollViewers)
                {
                    try
                    {
                        sv.ScrollChanged -= OnScrollViewerScrollChanged;
                    }
                    catch
                    {
                        // Игнорируем ошибки
                    }
                }
                _activeScrollViewers.Clear();
            }

            // Отписываемся от ScrollManager, чтобы не получать уведомления
            // при отсутствии активных ScrollViewer.
            _scrollManager.HorizontalOffsetChanged -= OnGlobalOffsetChanged;
        }

        public void Dispose()
        {
            if (_disposed) return;

            if (_scrollManager != null)
                _scrollManager.HorizontalOffsetChanged -= OnGlobalOffsetChanged;

            lock (_lockObject)
            {
                foreach (var sv in _activeScrollViewers)
                {
                    try
                    {
                        sv.ScrollChanged -= OnScrollViewerScrollChanged;
                    }
                    catch
                    {
                        // Игнорируем ошибки
                    }
                }
                _activeScrollViewers.Clear();
            }

            _disposed = true;
        }
    }
}
