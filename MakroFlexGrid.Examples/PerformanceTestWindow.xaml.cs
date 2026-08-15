using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using MakroFlexGrid.Rows;

namespace MakroFlexGrid.TestWindows
{
    /// <summary>
    /// Демонстрационное окно MakroFlexGrid: все типы ячеек, замороженные зоны,
    /// нижняя панель итогов, фильтрация и замер производительности (FPS/память).
    /// </summary>
    public partial class PerformanceTestWindow : Window
    {
        private DispatcherTimer _fpsTimer;
        private int _frameCount;
        private DateTime _lastFpsUpdate;
        private Stopwatch _loadStopwatch;

        public PerformanceTestWindow()
        {
            // Регистрируем обработчики для NumericColumnHeaderLeaf
            // Статический конструктор NumericCellBehavior вызывается автоматически
            // при первом обращении к классу

            InitializeComponent();
            SetupFPSTimer();
        }

        private void SetupFPSTimer()
        {
            _fpsTimer = new DispatcherTimer(
                TimeSpan.FromSeconds(0.5),
                DispatcherPriority.Background,
                (s, e) => UpdateFPS(),
                Dispatcher);
        }

        private void UpdateFPS()
        {
            var now = DateTime.Now;
            var elapsed = (now - _lastFpsUpdate).TotalSeconds;
            if (elapsed > 0)
            {
                var fps = _frameCount / elapsed;
                FPSText.Text = $"FPS: {fps:F1}";
                _frameCount = 0;
                _lastFpsUpdate = now;
            }

            // Обновляем метрики памяти
            UpdateMemoryMetrics();
        }

        private void UpdateMemoryMetrics()
        {
            var process = Process.GetCurrentProcess();
            var memoryMB = process.WorkingSet64 / (1024 * 1024);
            MemoryText.Text = $"Memory: {memoryMB} MB";

            if (TestGrid.ItemsSource is ICollection<TestItem> collection)
            {
                RowCountText.Text = $"Rows: {collection.Count:N0}";
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _lastFpsUpdate = DateTime.Now;

            // Подписываемся на событие рендеринга для подсчета FPS
            CompositionTarget.Rendering += (s, args) => _frameCount++;
        }

        private void Load1k_Click(object sender, RoutedEventArgs e)
        {
            LoadTestData(1000);
        }

        private void Load10k_Click(object sender, RoutedEventArgs e)
        {
            LoadTestData(10000);
        }

        private void Load50k_Click(object sender, RoutedEventArgs e)
        {
            LoadTestData(50000);
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            TestGrid.ItemsSource = null;
            StatusText.Text = "Таблица очищена";
            TimeText.Text = "—";
            UpdateMemoryMetrics();
        }

        private void StartScrollTest_Click(object sender, RoutedEventArgs e)
        {
            StatusText.Text = "Прокручивайте таблицу вертикально и горизонтально";
        }

        private void LoadTestData(int rowCount)
        {
            _loadStopwatch = Stopwatch.StartNew();
            StatusText.Text = $"Формирование {rowCount:N0} строк…";

            // Генерируем данные в фоновом потоке
            Dispatcher.BeginInvoke(new Action(() =>
            {
                var data = new List<TestItem>();

                var categories = new[] { "Электроника", "Одежда", "Продукты", "Книги", "Игрушки", "Мебель", "Спорт", "Авто" };
                var statuses = new[] { "Активен", "Неактивен", "Ожидание", "Завершён", "Отменён" };
                var rng = new Random(42); // Фиксированный seed для воспроизводимости

                for (int i = 0; i < rowCount; i++)
                {
                    var item = new TestItem
                    {
                        Id = i,
                        Name = $"Элемент {i}",
                        Description = $"Описание для элемента {i}. Длинный текст для проверки переноса слов и разной ширины колонок.",
                        Value = rng.Next(0, 10000),
                        Status = statuses[i % statuses.Length],
                        Category = categories[i % categories.Length],
                        Date = DateTime.Now.AddDays(-rng.Next(0, 365)),
                        LastColumn = $"Значение последней колонки {i}",
                        IsActive = i % 2 == 0,
                        // Поля для демонстрации всех типов ячеек
                        Price = (decimal)(rng.NextDouble() * 10000),
                        IsChecked = i % 3 == 0,
                        NullableDate = i % 5 == 0 ? DateTime.Now.AddDays(-rng.Next(0, 365)) : (DateTime?)null,
                        Url = i % 4 == 0 ? $"https://example.com/item/{i}" : null,
                        Progress = rng.NextDouble() * 100,
                        ImageUrl = null, // Можно указать URL картинки для теста
                        Rating = rng.Next(1, 6),
                        Color = new[] { "Red", "Green", "Blue", "Orange", "Purple", "Teal" }[i % 6],
                        MultiLineText = $"Строка 1: элемент {i}\nСтрока 2: статус = {statuses[i % statuses.Length]}\nСтрока 3: значение = {rng.Next(0, 10000)}\nСтрока 4: категория = {categories[i % categories.Length]}",
                        RadioOption = new[] { "Вариант А", "Вариант Б", "Вариант В" }[i % 3]
                    };

                    data.Add(item);

                    // Обновляем прогресс каждые 1000 строк
                    if (i % 1000 == 0 && i > 0)
                    {
                        StatusText.Text = $"Формирование… {i:N0}/{rowCount:N0}";
                        Dispatcher.Invoke(() => { }, DispatcherPriority.Background);
                    }
                }

                _loadStopwatch.Stop();
                StatusText.Text = $"Загружено {rowCount:N0} строк за {_loadStopwatch.Elapsed.TotalSeconds:F2} с";
                TimeText.Text = $"{_loadStopwatch.Elapsed.TotalSeconds:F2} с";

                TestGrid.ItemsSource = data;
                UpdateMemoryMetrics();

            }), DispatcherPriority.Background);
        }

        private void TestGrid_Loaded(object sender, RoutedEventArgs e)
        {
            // Синхронизируем заголовки, заданные в XAML, с колонками DataGrid.
            // Это необходимо, так как при XAML-парсинге заголовки создаются,
            // но SyncColumnsWithHeaders() вызывается только через OnHeaderCollectionChanged
            // с debounce, и может не успеть выполниться до загрузки грида.
            TestGrid.SyncColumnsWithHeaders();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LoadTestData(10);
        }


    }
}