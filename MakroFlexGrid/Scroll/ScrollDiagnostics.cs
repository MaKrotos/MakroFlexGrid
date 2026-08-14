using System.Diagnostics;

namespace MakroFlexGrid.Scroll
{
    public static class ScrollDiagnostics
    {
        private static DateTime _lastLogTime = DateTime.MinValue;
        private static readonly TimeSpan LogThrottle = TimeSpan.FromMilliseconds(500);

        public static void LogVirtualizationStats(int visibleCount, int realizedCount, int totalItems, int poolSize)
        {
            var now = DateTime.Now;
            if (now - _lastLogTime < LogThrottle && !Debugger.IsAttached)
                return;

            _lastLogTime = now;
            Debug.WriteLine($"[{now:HH:mm:ss.fff}] ?? VIRTUALIZATION | Visible: {visibleCount} | Realized: {realizedCount} | Total: {totalItems} | Pool: {poolSize}");
        }
    }
}
