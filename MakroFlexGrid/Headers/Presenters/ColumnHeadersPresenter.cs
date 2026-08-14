using MakroFlexGrid.Core;
using MakroFlexGrid.Rows;
using MakroFlexGrid.Utilities;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

// using для WeakDependencyPropertyListener (тот же namespace)

namespace MakroFlexGrid.Headers
{
    /// <summary>
    /// Presenter для отображения многоуровневых заголовков колонок.
    /// Рендерит Frozen (замороженные слева) и Scrollable (скроллируемые) заголовки
    /// с поддержкой ColumnSpan/RowSpan для многоуровневой вложенности.
    /// Аналог BandHeadersPresenter из FlexGrid.
    /// </summary>
    [TemplatePart(Name = FrozenHeadersWrapperPartName, Type = typeof(Grid))]
    [TemplatePart(Name = HeadersScrollViewerPartName, Type = typeof(ScrollViewer))]
    [TemplatePart(Name = HeadersWrapperPartName, Type = typeof(Grid))]
    [TemplatePart(Name = RightFrozenHeadersWrapperPartName, Type = typeof(Grid))]
    public sealed class ColumnHeadersPresenter : Control
    {
        static ColumnHeadersPresenter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ColumnHeadersPresenter),
                new FrameworkPropertyMetadata(typeof(ColumnHeadersPresenter)));
        }

        public ColumnHeadersPresenter()
        {
            // Подписываемся на Preview-события мыши с handledEventsToo: true.
            // Это необходимо, чтобы получать события даже после того,
            // как ButtonBase (родитель ColumnHeader) пометил их как обработанные.
            AddHandler(PreviewMouseLeftButtonDownEvent,
                new MouseButtonEventHandler(OnPreviewMouseLeftButtonDownHandler), true);
            AddHandler(PreviewMouseMoveEvent,
                new MouseEventHandler(OnPreviewMouseMoveHandler), true);
            AddHandler(PreviewMouseLeftButtonUpEvent,
                new MouseButtonEventHandler(OnPreviewMouseLeftButtonUpHandler), true);
        }

        #region Constants

        public const string FrozenHeadersWrapperPartName = "PART_FrozenHeadersWrapper";
        public const string HeadersScrollViewerPartName = "PART_HeadersScrollViewer";
        public const string HeadersWrapperPartName = "PART_HeadersWrapper";
        public const string RightFrozenHeadersWrapperPartName = "PART_RightFrozenHeadersWrapper";

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty ScrollableWidthProperty =
            DependencyProperty.Register(
                nameof(ScrollableWidth),
                typeof(double),
                typeof(ColumnHeadersPresenter),
                new FrameworkPropertyMetadata(0.0));

        public static readonly DependencyProperty HeadersScrollHorizontalOffsetProperty =
            DependencyProperty.Register(
                "HeadersScrollHorizontalOffset",
                typeof(double),
                typeof(ColumnHeadersPresenter),
                new FrameworkPropertyMetadata(0d, OnHeadersScrollHorizontalOffsetChanged));

        private static void OnHeadersScrollHorizontalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var presenter = d as ColumnHeadersPresenter;
            presenter?._headersScrollViewer?.ScrollToHorizontalOffset((double)e.NewValue);
        }

        public static readonly DependencyProperty MaxHeadersScrollHorizontalOffsetProperty =
            DependencyProperty.Register(
                "MaxHeadersScrollHorizontalOffset",
                typeof(double),
                typeof(ColumnHeadersPresenter),
                new FrameworkPropertyMetadata(0.0));

        public static readonly DependencyProperty LeftFrozenPanelBackgroundProperty =
            DependencyProperty.Register(
                nameof(LeftFrozenPanelBackground),
                typeof(Brush),
                typeof(ColumnHeadersPresenter),
                new FrameworkPropertyMetadata(Brushes.Transparent));

        public static readonly DependencyProperty RightFrozenPanelBackgroundProperty =
            DependencyProperty.Register(
                nameof(RightFrozenPanelBackground),
                typeof(Brush),
                typeof(ColumnHeadersPresenter),
                new FrameworkPropertyMetadata(Brushes.Transparent));

        public static readonly DependencyProperty GridLineBrushProperty =
            DependencyProperty.Register(
                nameof(GridLineBrush),
                typeof(Brush),
                typeof(ColumnHeadersPresenter),
                new FrameworkPropertyMetadata(Brushes.LightGray));

        public static readonly DependencyProperty LeftMarginProperty =
            DependencyProperty.Register(
                nameof(LeftMargin),
                typeof(Thickness),
                typeof(ColumnHeadersPresenter),
                new FrameworkPropertyMetadata(new Thickness(0)));

        public static readonly DependencyProperty RightMarginProperty =
            DependencyProperty.Register(
                nameof(RightMargin),
                typeof(Thickness),
                typeof(ColumnHeadersPresenter),
                new FrameworkPropertyMetadata(new Thickness(0)));

        public static readonly DependencyProperty SeparatorWidthProperty =
            DependencyProperty.Register(
                nameof(SeparatorWidth),
                typeof(double),
                typeof(ColumnHeadersPresenter),
                new FrameworkPropertyMetadata(0.0));

        public static readonly DependencyProperty SeparatorBrushProperty =
            DependencyProperty.Register(
                nameof(SeparatorBrush),
                typeof(Brush),
                typeof(ColumnHeadersPresenter),
                new FrameworkPropertyMetadata(Brushes.Gray));

        public static readonly DependencyProperty LeftFrozenColumnsCountProperty =
            DependencyProperty.Register(
                nameof(LeftFrozenColumnsCount),
                typeof(int),
                typeof(ColumnHeadersPresenter),
                new FrameworkPropertyMetadata(0));

        public static readonly DependencyProperty RightFrozenColumnsCountProperty =
            DependencyProperty.Register(
                nameof(RightFrozenColumnsCount),
                typeof(int),
                typeof(ColumnHeadersPresenter),
                new FrameworkPropertyMetadata(0));

        #endregion

        #region CLR Properties

        public double ScrollableWidth
        {
            get => (double)GetValue(ScrollableWidthProperty);
            set => SetValue(ScrollableWidthProperty, value);
        }

        public Brush LeftFrozenPanelBackground
        {
            get => (Brush)GetValue(LeftFrozenPanelBackgroundProperty);
            set => SetValue(LeftFrozenPanelBackgroundProperty, value);
        }

        public Brush RightFrozenPanelBackground
        {
            get => (Brush)GetValue(RightFrozenPanelBackgroundProperty);
            set => SetValue(RightFrozenPanelBackgroundProperty, value);
        }

        public Brush GridLineBrush
        {
            get => (Brush)GetValue(GridLineBrushProperty);
            set => SetValue(GridLineBrushProperty, value);
        }

        public Thickness LeftMargin
        {
            get => (Thickness)GetValue(LeftMarginProperty);
            set => SetValue(LeftMarginProperty, value);
        }

        public Thickness RightMargin
        {
            get => (Thickness)GetValue(RightMarginProperty);
            set => SetValue(RightMarginProperty, value);
        }

        public double SeparatorWidth
        {
            get => (double)GetValue(SeparatorWidthProperty);
            set => SetValue(SeparatorWidthProperty, value);
        }

        public Brush SeparatorBrush
        {
            get => (Brush)GetValue(SeparatorBrushProperty);
            set => SetValue(SeparatorBrushProperty, value);
        }

        public int LeftFrozenColumnsCount
        {
            get => (int)GetValue(LeftFrozenColumnsCountProperty);
            set => SetValue(LeftFrozenColumnsCountProperty, value);
        }

        public int RightFrozenColumnsCount
        {
            get => (int)GetValue(RightFrozenColumnsCountProperty);
            set => SetValue(RightFrozenColumnsCountProperty, value);
        }

        #endregion

        #region Private Variables

        private Grid _frozenHeadersWrapper;
        private ScrollViewer _headersScrollViewer;
        private Grid _headersWrapper;
        private Grid _rightFrozenHeadersWrapper;
        private CustomDataGrid _ownerGrid;

        // Храним WeakDependencyPropertyListener для автоматической отписки при сборке GC
        private readonly List<WeakDependencyPropertyListener> _columnWidthListeners
            = new List<WeakDependencyPropertyListener>();

        // --- Drag & Drop state ---
        private ColumnHeaderItem _dragSourceItem;
        private ColumnHeader _dragHeaderElement;
        private Point _dragStartPoint;
        private bool _isDragActive;
        private ColumnHeaderDragAdorner _dragAdorner;
        private ColumnHeaderDropTargetAdorner _dropTargetAdorner;
        private AdornerLayer _dragAdornerLayer;
        private ColumnHeaderItem _lastDropTargetItem;
        private bool _lastInsertBefore;

        #endregion

        #region Public Properties

        public CustomDataGrid OwnerGrid
        {
            get
            {
                if (_ownerGrid == null)
                    _ownerGrid = FindVisualParent<CustomDataGrid>(this);
                return _ownerGrid;
            }
        }

        public double HeadersScrollHorizontalOffset
        {
            get => (double)GetValue(HeadersScrollHorizontalOffsetProperty);
            set => SetValue(HeadersScrollHorizontalOffsetProperty, value);
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Синхронизирует горизонтальный offset заголовков.
        /// Вызывается из CustomDataGrid при изменении HorizontalScrollChanged.
        /// </summary>
        internal void SyncScrollOffset(double offset)
        {
            _headersScrollViewer?.ScrollToHorizontalOffset(offset);
        }

        #region Drag & Drop Event Handlers (AddHandler with handledEventsToo)

        /// <summary>
        /// Обработчик PreviewMouseLeftButtonDown, подписанный через AddHandler с handledEventsToo=true.
        /// Это позволяет получать событие даже после того, как ButtonBase пометил его как обработанное.
        /// </summary>
        private void OnPreviewMouseLeftButtonDownHandler(object sender, MouseButtonEventArgs e)
        {
            // Если уже есть активная DnD-сессия, игнорируем
            if (_dragSourceItem != null)
                return;

            // Проверяем глобальное разрешение DnD на уровне грида
            if (OwnerGrid != null && !OwnerGrid.AllowDrag)
                return;

            // Если клик был по Gripper (Thumb для ресайза), не начинаем DnD,
            // чтобы не конфликтовать с ресайзом колонок
            var originalSource = e.OriginalSource as DependencyObject;
            if (originalSource != null && FindVisualParent<Thumb>(originalSource) != null)
                return;

            var header = FindHeaderAtPoint(e.GetPosition(this));
            if (header?.OwnerItem == null)
                return;

            var item = header.OwnerItem;

            // DnD только для корневых заголовков с AllowDrag
            if (!item.AllowDrag || item.ParentItem != null || !IsRootHeader(item))
                return;

            _dragSourceItem = item;
            _dragHeaderElement = header;
            _dragStartPoint = e.GetPosition(this);
            _isDragActive = false;

            // Помечаем источник как перетаскиваемый
            item.IsDragging = true;

            // Помечаем событие как обработанное, чтобы ButtonBase не начинал свою обработку
            // (Click, захват мыши и т.д.), которая может помешать DnD.
            e.Handled = true;

            // Захватываем мышь для получения всех последующих событий
            CaptureMouse();
        }

        /// <summary>
        /// Обработчик PreviewMouseMove, подписанный через AddHandler с handledEventsToo=true.
        /// </summary>
        private void OnPreviewMouseMoveHandler(object sender, MouseEventArgs e)
        {
            if (_dragSourceItem == null || !IsMouseCaptured)
                return;

            var currentPosition = e.GetPosition(this);
            var delta = currentPosition - _dragStartPoint;

            // Начинаем визуальное перетаскивание только после превышения threshold
            if (!_isDragActive && Math.Abs(delta.X) > SystemParameters.MinimumHorizontalDragDistance)
            {
                _isDragActive = true;
                StartDragVisual(currentPosition);
            }

            if (_isDragActive)
            {
                UpdateDragVisual(currentPosition);
                UpdateDropTarget(currentPosition);
            }
        }

        /// <summary>
        /// Обработчик PreviewMouseLeftButtonUp, подписанный через AddHandler с handledEventsToo=true.
        /// </summary>
        private void OnPreviewMouseLeftButtonUpHandler(object sender, MouseButtonEventArgs e)
        {
            if (_dragSourceItem == null)
            {
                CleanupDrag();
                return;
            }

            if (IsMouseCaptured)
                ReleaseMouseCapture();

            if (_isDragActive)
            {
                var releasePosition = e.GetPosition(this);
                var targetInfo = HitTestDropTarget(releasePosition);
                if (targetInfo != null)
                {
                    ExecuteDrop(_dragSourceItem, targetInfo);
                }
            }

            CleanupDrag();
        }

        /// <summary>
        /// Находит ColumnHeader под указанной точкой (в координатах Presenter).
        /// Сначала пробует InputHitTest на Presenter, затем на каждом wrapper-е по очереди.
        /// </summary>
        private ColumnHeader FindHeaderAtPoint(Point point)
        {
            // Сначала пробуем InputHitTest на самом Presenter
            var hitElement = InputHitTest(point) as DependencyObject;
            if (hitElement != null)
            {
                var result = FindVisualParent<ColumnHeader>(hitElement);
                if (result != null)
                    return result;
            }

            // Если не нашли через Presenter, пробуем каждый wrapper отдельно
            // (нужно для ScrollViewer, который может не пропускать InputHitTest)
            var wrappers = new UIElement[] { _frozenHeadersWrapper, _headersWrapper, _rightFrozenHeadersWrapper };
            foreach (var wrapper in wrappers)
            {
                if (wrapper == null || !wrapper.IsVisible || !wrapper.IsHitTestVisible)
                    continue;

                try
                {
                    var wrapperPoint = wrapper.PointFromScreen(PointToScreen(point));
                    var wrapperHit = wrapper.InputHitTest(wrapperPoint) as DependencyObject;
                    if (wrapperHit != null)
                    {
                        var result = FindVisualParent<ColumnHeader>(wrapperHit);
                        if (result != null)
                            return result;
                    }
                }
                catch
                {
                    // Игнорируем ошибки преобразования координат
                }
            }

            return null;
        }

        #endregion

        #region Drag & Drop Core Methods

        /// <summary>
        /// Начинает DnD-сессию для указанного заголовка.
        /// </summary>
        private void BeginHeaderDrag(ColumnHeaderItem item, Point mousePosition)
        {
            if (item == null || !item.AllowDrag || item.ParentItem != null)
                return;

            if (!IsRootHeader(item))
                return;

            _dragSourceItem = item;
            _dragStartPoint = mousePosition;
            _isDragActive = false;

            item.IsDragging = true;
            CaptureMouse();
        }

        /// <summary>
        /// Обновляет состояние DnD при движении мыши.
        /// </summary>
        private void UpdateHeaderDrag(Point currentPosition)
        {
            if (_dragSourceItem == null)
                return;

            var delta = currentPosition - _dragStartPoint;

            if (!_isDragActive && Math.Abs(delta.X) > SystemParameters.MinimumHorizontalDragDistance)
            {
                _isDragActive = true;
                StartDragVisual(currentPosition);
            }

            if (_isDragActive)
            {
                UpdateDragVisual(currentPosition);
                UpdateDropTarget(currentPosition);
            }
        }

        /// <summary>
        /// Завершает DnD-сессию.
        /// </summary>
        private void EndHeaderDrag(Point releasePosition)
        {
            if (_dragSourceItem == null)
            {
                CleanupDrag();
                return;
            }

            if (_isDragActive)
            {
                var targetInfo = HitTestDropTarget(releasePosition);
                if (targetInfo != null)
                {
                    ExecuteDrop(_dragSourceItem, targetInfo);
                }
            }

            CleanupDrag();
            ReleaseMouseCapture();
        }

        #endregion

        internal void GenerateHeaderElements()
        {
            if (OwnerGrid == null || _frozenHeadersWrapper == null || _headersWrapper == null)
                return;

            var frozenHeaders = OwnerGrid.FrozenColumnHeaders;
            var scrollableHeaders = OwnerGrid.ScrollableColumnHeaders;
            var rightFrozenHeaders = OwnerGrid.RightFrozenColumnHeaders;

            ClearWrapper(_frozenHeadersWrapper);
            ClearWrapper(_headersWrapper);

            int maxRowCount = Math.Max(frozenHeaders.MaxDepth, scrollableHeaders.MaxDepth);

            // Системный заголовок (с треугольником выделения) — только если включен
            bool systemColumnEnabled = OwnerGrid.IsSystemColumnEnabled;
            int systemColumnOffset = systemColumnEnabled ? 1 : 0;

            if (systemColumnEnabled)
            {
                var systemHeader = new SystemColumnHeaderItem(OwnerGrid);
                ApplyColumnsAndRows(_frozenHeadersWrapper, maxRowCount, GetVisibleBottomItemsCount(frozenHeaders) + 1);

                var systemHeaderElement = systemHeader.HeaderElement;
                systemHeaderElement.Width = systemHeader.Width;
                Grid.SetColumn(systemHeaderElement, 0);
                Grid.SetRow(systemHeaderElement, 0);
                Grid.SetRowSpan(systemHeaderElement, maxRowCount);
                Grid.SetColumnSpan(systemHeaderElement, 1);
                systemHeaderElement.Visibility = Visibility.Visible;
                _frozenHeadersWrapper.Children.Add(systemHeaderElement);
            }
            else
            {
                ApplyColumnsAndRows(_frozenHeadersWrapper, maxRowCount, GetVisibleBottomItemsCount(frozenHeaders));
            }

            // Остальные заголовки смещаем на 1 колонку вправо, если системный заголовок включен
            InsertHeaders(_frozenHeadersWrapper, frozenHeaders, maxRowCount, 0, systemColumnOffset, false);

            ApplyColumnsAndRows(_headersWrapper, maxRowCount, GetVisibleBottomItemsCount(scrollableHeaders));
            InsertHeaders(_headersWrapper, scrollableHeaders, maxRowCount, 0, 0, false);

            // Правая frozen-секция
            if (_rightFrozenHeadersWrapper != null)
            {
                ClearWrapper(_rightFrozenHeadersWrapper);
                int rightMaxRowCount = rightFrozenHeaders.MaxDepth;
                ApplyColumnsAndRows(_rightFrozenHeadersWrapper, rightMaxRowCount, GetVisibleBottomItemsCount(rightFrozenHeaders));
                InsertHeaders(_rightFrozenHeadersWrapper, rightFrozenHeaders, rightMaxRowCount, 0, 0, true);
            }

            // После перестроения заголовков подписываемся на изменения ширины колонок,
            // чтобы при ресайзе обновлять ColumnDefinitions Grid (ширины колонок в разметке).
            // Сами ColumnHeaderItem обновляют свою ширину через WidthProperty,
            // но Grid.ColumnDefinitions нужно синхронизировать с актуальными размерами.
            SubscribeToColumnWidths();

            // Обновляем ScrollableWidth после перестроения заголовков
            UpdateScrollableWidth();

            // Принудительно инвалидируем layout всех wrapper-ов, чтобы WPF пересчитал
            // позиции заголовков с учётом нового порядка колонок.
            // Это необходимо после Drag&Drop, когда порядок элементов в коллекциях
            // изменился, и Grid.SetColumn() для заголовков установлен в новые позиции.
            _frozenHeadersWrapper.InvalidateMeasure();
            _frozenHeadersWrapper.InvalidateArrange();
            _headersWrapper.InvalidateMeasure();
            _headersWrapper.InvalidateArrange();
            if (_rightFrozenHeadersWrapper != null)
            {
                _rightFrozenHeadersWrapper.InvalidateMeasure();
                _rightFrozenHeadersWrapper.InvalidateArrange();
            }
        }

        /// <summary>
        /// Обновляет ScrollableWidth на основе ScrollViewer.ScrollableWidth.
        /// ScrollViewer сам вычисляет ScrollableWidth = ExtentWidth - ViewportWidth,
        /// что и есть ширина контента, не помещающаяся в видимую область.
        /// </summary>
        internal void UpdateScrollableWidth()
        {
            var scrollViewer = GetTemplateChild(HeadersScrollViewerPartName) as ScrollViewer;
            if (scrollViewer == null) return;

            ScrollableWidth = Math.Max(0, scrollViewer.ScrollableWidth);
        }

        #endregion

        #region Private Methods

        #region Drag & Drop Helpers

        /// <summary>
        /// Проверяет, является ли элемент корневым заголовком (находится напрямую в одной из трёх коллекций).
        /// </summary>
        private bool IsRootHeader(ColumnHeaderItem item)
        {
            if (OwnerGrid == null) return false;

            return OwnerGrid.FrozenColumnHeaders.Contains(item) ||
                   OwnerGrid.ScrollableColumnHeaders.Contains(item) ||
                   OwnerGrid.RightFrozenColumnHeaders.Contains(item);
        }

        /// <summary>
        /// Возвращает коллекцию, в которой находится элемент, или null.
        /// </summary>
        private ColumnHeaderCollection GetParentCollection(ColumnHeaderItem item)
        {
            if (OwnerGrid == null) return null;

            if (OwnerGrid.FrozenColumnHeaders.Contains(item))
                return OwnerGrid.FrozenColumnHeaders;
            if (OwnerGrid.ScrollableColumnHeaders.Contains(item))
                return OwnerGrid.ScrollableColumnHeaders;
            if (OwnerGrid.RightFrozenColumnHeaders.Contains(item))
                return OwnerGrid.RightFrozenColumnHeaders;

            return null;
        }

        /// <summary>
        /// Создаёт визуальный элемент — стилизованную полупрозрачную копию перетаскиваемого заголовка.
        /// Использует VisualBrush для точной визуальной копии заголовка один-в-один,
        /// включая все стили, шаблоны, фоны, ContentTemplate и т.д.
        /// </summary>
        private void StartDragVisual(Point currentPosition)
        {
            if (_dragSourceItem == null || _dragSourceItem.HeaderElement == null)
                return;

            // Определяем, над какой Grid-обёрткой показывать Adorner
            var adornerHost = GetAdornerHost();
            if (adornerHost == null) return;

            _dragAdornerLayer = AdornerLayer.GetAdornerLayer(adornerHost);
            if (_dragAdornerLayer == null) return;

            var sourceElement = _dragSourceItem.HeaderElement;

            // Создаём Rectangle с VisualBrush для точной визуальной копии заголовка.
            // VisualBrush захватывает полное визуальное состояние элемента:
            // фон, border, ContentTemplate, шрифты, выравнивание и т.д.
            var dragVisual = new System.Windows.Shapes.Rectangle
            {
                Width = sourceElement.ActualWidth,
                Height = sourceElement.ActualHeight,
                Fill = new VisualBrush(sourceElement)
                {
                    Stretch = Stretch.None,
                    AlignmentX = AlignmentX.Left,
                    AlignmentY = AlignmentY.Top,
                    Viewbox = new Rect(0, 0, sourceElement.ActualWidth, sourceElement.ActualHeight),
                    ViewboxUnits = BrushMappingMode.Absolute,
                    Viewport = new Rect(0, 0, sourceElement.ActualWidth, sourceElement.ActualHeight),
                    ViewportUnits = BrushMappingMode.Absolute
                }
            };

            // Измеряем и располагаем визуальный элемент
            dragVisual.Measure(new Size(sourceElement.ActualWidth, sourceElement.ActualHeight));
            dragVisual.Arrange(new Rect(0, 0, sourceElement.ActualWidth, sourceElement.ActualHeight));

            // Создаём стилизованный Adorner с тенью, скруглениями и рамкой
            _dragAdorner = new ColumnHeaderDragAdorner(
                adornerHost,
                dragVisual,
                opacity: 0.85,
                cornerRadius: 4,
                borderBrush: new SolidColorBrush(Color.FromArgb(0x99, 0x1E, 0x90, 0xFF)),
                borderThickness: 1.5,
                shadowDepth: 5);

            // Вычисляем позицию относительно adornerHost
            var positionRelativeToHost = GetPositionRelativeTo(adornerHost, currentPosition);
            _dragAdorner.SetPosition(
                positionRelativeToHost.X - sourceElement.ActualWidth / 2,
                positionRelativeToHost.Y - sourceElement.ActualHeight / 2);

            _dragAdornerLayer.Add(_dragAdorner);

            // Создаём стилизованный Adorner для индикатора вставки
            _dropTargetAdorner = new ColumnHeaderDropTargetAdorner(
                adornerHost,
                lineColor: Color.FromRgb(0x1E, 0x90, 0xFF),
                lineThickness: 3,
                highlightColor: Color.FromArgb(0x1A, 0x1E, 0x90, 0xFF),
                dotRadius: 4);
            _dropTargetAdorner.SetPosition(0, 0); // Пока скрыт
            _dragAdornerLayer.Add(_dropTargetAdorner);
        }

        /// <summary>
        /// Обновляет позицию полупрозрачной копии.
        /// </summary>
        private void UpdateDragVisual(Point currentPosition)
        {
            if (_dragAdorner == null || _dragSourceItem?.HeaderElement == null)
                return;

            var adornerHost = GetAdornerHost();
            if (adornerHost == null) return;

            var positionRelativeToHost = GetPositionRelativeTo(adornerHost, currentPosition);
            _dragAdorner.SetPosition(
                positionRelativeToHost.X - _dragSourceItem.HeaderElement.ActualWidth / 2,
                positionRelativeToHost.Y - _dragSourceItem.HeaderElement.ActualHeight / 2);
        }

        /// <summary>
        /// Определяет целевой элемент и позицию вставки на основе положения курсора.
        /// Обновляет визуальный индикатор (линию).
        /// </summary>
        private void UpdateDropTarget(Point currentPosition)
        {
            if (_dropTargetAdorner == null) return;

            var targetInfo = HitTestDropTarget(currentPosition);

            // Сбрасываем подсветку предыдущего target
            if (_lastDropTargetItem != null)
            {
                _lastDropTargetItem.IsDropTarget = false;
                _lastDropTargetItem = null;
            }

            if (targetInfo != null)
            {
                var adornerHost = GetAdornerHost();
                if (adornerHost == null) return;

                // Вычисляем X-позицию линии
                double lineX = GetDropTargetLineX(targetInfo, adornerHost);
                double hostHeight = adornerHost is FrameworkElement fe ? fe.ActualHeight : 30;

                _dropTargetAdorner.SetPosition(lineX, hostHeight > 0 ? hostHeight : 30);
                _dropTargetAdorner.Visibility = Visibility.Visible;

                // Подсвечиваем целевой элемент (если есть)
                if (targetInfo.TargetItem != null)
                {
                    targetInfo.TargetItem.IsDropTarget = true;
                    _lastDropTargetItem = targetInfo.TargetItem;
                }

                _lastInsertBefore = targetInfo.InsertBefore;
            }
            else
            {
                // Прячем линию, если нет цели
                _dropTargetAdorner.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Определяет целевую коллекцию и индекс вставки на основе позиции курсора
        /// (в координатах Presenter).
        /// </summary>
        private DropTargetInfo HitTestDropTarget(Point presenterPosition)
        {
            if (OwnerGrid == null) return null;

            // Определяем, над какой секцией находится курсор
            var sectionInfo = HitTestSection(presenterPosition);
            if (sectionInfo == null) return null;

            var collection = sectionInfo.Value.Collection;
            var sectionWrapper = sectionInfo.Value.Wrapper;

            // Если у перетаскиваемого элемента запрещён межсекционный перенос,
            // не показываем индикатор вставки при наведении на другую секцию
            if (_dragSourceItem != null && !_dragSourceItem.AllowCrossSectionDrag)
            {
                var sourceCollection = GetParentCollection(_dragSourceItem);
                if (sourceCollection != null && sourceCollection != collection)
                    return null;
            }

            // Получаем позицию курсора относительно wrapper через TranslatePoint
            var positionInWrapper = TranslatePoint(presenterPosition, sectionWrapper);

            // Проходим по корневым элементам коллекции и определяем индекс вставки
            int insertIndex = 0;
            ColumnHeaderItem targetItem = null;
            bool insertBefore = true;

            for (int i = 0; i < collection.Count; i++)
            {
                var header = collection[i];
                var headerElement = header.HeaderElement;

                // Пропускаем скрытые заголовки
                if (!header.IsVisible || headerElement == null)
                    continue;

                // Получаем позицию элемента относительно wrapper
                var elementPosition = headerElement.TranslatePoint(new Point(0, 0), sectionWrapper);
                double elementMidX = elementPosition.X + headerElement.ActualWidth / 2;

                if (positionInWrapper.X < elementMidX)
                {
                    insertIndex = i;
                    targetItem = header;
                    insertBefore = true;
                    break;
                }

                insertIndex = i + 1;
                targetItem = header;
                insertBefore = false;
            }

            return new DropTargetInfo
            {
                Collection = collection,
                InsertIndex = insertIndex,
                TargetItem = targetItem,
                InsertBefore = insertBefore
            };
        }

        /// <summary>
        /// Определяет, над какой секцией заголовков находится курсор
        /// (координаты относительно Presenter).
        /// </summary>
        private (ColumnHeaderCollection Collection, UIElement Wrapper)? HitTestSection(Point presenterPosition)
        {
            if (OwnerGrid == null) return null;

            // Проверяем левую frozen секцию
            if (_frozenHeadersWrapper != null && IsPointOverElement(presenterPosition, _frozenHeadersWrapper))
            {
                return (OwnerGrid.FrozenColumnHeaders, _frozenHeadersWrapper);
            }

            // Проверяем scrollable секцию (через ScrollViewer)
            if (_headersScrollViewer != null && IsPointOverElement(presenterPosition, _headersScrollViewer))
            {
                return (OwnerGrid.ScrollableColumnHeaders, _headersWrapper);
            }

            // Проверяем правую frozen секцию
            if (_rightFrozenHeadersWrapper != null && IsPointOverElement(presenterPosition, _rightFrozenHeadersWrapper))
            {
                return (OwnerGrid.RightFrozenColumnHeaders, _rightFrozenHeadersWrapper);
            }

            return null;
        }

        /// <summary>
        /// Проверяет, находится ли точка (в координатах Presenter) над указанным элементом.
        /// </summary>
        private bool IsPointOverElement(Point presenterPoint, UIElement element)
        {
            if (!element.IsVisible || !element.IsHitTestVisible)
                return false;

            try
            {
                // Конвертируем точку из координат Presenter в координаты element
                var elementPosition = TranslatePoint(presenterPoint, element);
                return elementPosition.X >= 0 && elementPosition.X <= element.RenderSize.Width &&
                       elementPosition.Y >= 0 && elementPosition.Y <= element.RenderSize.Height;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Вычисляет X-координату линии-индикатора вставки относительно adornerHost.
        /// </summary>
        private double GetDropTargetLineX(DropTargetInfo targetInfo, UIElement adornerHost)
        {
            if (targetInfo.TargetItem?.HeaderElement == null)
                return 0;

            var headerElement = targetInfo.TargetItem.HeaderElement;

            // Определяем wrapper, в котором находится элемент
            UIElement sectionWrapper = GetSectionWrapper(targetInfo.Collection);
            if (sectionWrapper == null) return 0;

            // Получаем позицию элемента относительно wrapper
            var elementPos = headerElement.TranslatePoint(new Point(0, 0), sectionWrapper);

            // Вычисляем X линии (левый край элемента или правый)
            double localX = targetInfo.InsertBefore
                ? elementPos.X
                : elementPos.X + headerElement.ActualWidth;

            // Конвертируем в координаты adornerHost
            var linePoint = sectionWrapper.TranslatePoint(new Point(localX, 0), adornerHost);
            return linePoint.X;
        }

        /// <summary>
        /// Возвращает Grid-обёртку для указанной коллекции.
        /// </summary>
        private UIElement GetSectionWrapper(ColumnHeaderCollection collection)
        {
            if (OwnerGrid == null) return null;

            if (collection == OwnerGrid.FrozenColumnHeaders)
                return _frozenHeadersWrapper;
            if (collection == OwnerGrid.ScrollableColumnHeaders)
                return _headersWrapper;
            if (collection == OwnerGrid.RightFrozenColumnHeaders)
                return _rightFrozenHeadersWrapper;

            return null;
        }

        /// <summary>
        /// Возвращает элемент, который будет использоваться как хост для AdornerLayer.
        /// </summary>
        private UIElement GetAdornerHost()
        {
            // Используем сам ColumnHeadersPresenter как хост для Adorner
            return this;
        }

        /// <summary>
        /// Преобразует координаты из системы Presenter в координаты относительно указанного элемента.
        /// </summary>
        private Point GetPositionRelativeTo(UIElement target, Point presenterPosition)
        {
            try
            {
                return TranslatePoint(presenterPosition, target);
            }
            catch
            {
                return new Point(0, 0);
            }
        }

        /// <summary>
        /// Выполняет перемещение заголовка из исходной коллекции в целевую.
        /// После перемещения выполняет полную синхронизацию: перестраивает заголовки,
        /// синхронизирует колонки DataGrid, пересоздаёт строки и обновляет нижнюю панель.
        /// </summary>
        private void ExecuteDrop(ColumnHeaderItem item, DropTargetInfo target)
        {
            if (OwnerGrid == null) return;

            var sourceCollection = GetParentCollection(item);
            if (sourceCollection == null) return;

            int sourceIndex = sourceCollection.IndexOf(item);
            if (sourceIndex < 0) return;

            // Если у элемента запрещён межсекционный перенос,
            // и целевая коллекция отличается от исходной — отменяем drop
            if (!item.AllowCrossSectionDrag && sourceCollection != target.Collection)
                return;

            // Если перемещение внутри одной коллекции
            if (sourceCollection == target.Collection)
            {
                // Корректируем индекс вставки с учётом удаления исходного элемента
                int adjustedIndex = target.InsertIndex;
                if (sourceIndex < adjustedIndex)
                    adjustedIndex--;

                if (adjustedIndex != sourceIndex && adjustedIndex >= 0 && adjustedIndex < sourceCollection.Count)
                {
                    sourceCollection.MoveItem(sourceIndex, adjustedIndex);
                }
            }
            else
            {
                // Перемещение между разными коллекциями
                ColumnHeaderCollection.MoveToCollection(
                    sourceCollection, target.Collection, item, target.InsertIndex);

                // Обновляем GripperPosition для перемещаемого элемента и всех его детей.
                // В правой frozen-панели gripper должен быть слева, в остальных — справа.
                bool isRightPanel = target.Collection == OwnerGrid.RightFrozenColumnHeaders;
                UpdateGripperPositionRecursive(item, isRightPanel);
            }

            // Уведомляем грид, что синхронизация будет выполнена синхронно,
            // чтобы отложенный вызов из OnHeaderCollectionChanged был пропущен.
            OwnerGrid.NotifySyncExecuted();

            // Полная синхронизация после DnD:
            // SyncColumnsWithHeaders() сам вызывает GenerateHeaderElements(),
            // синхронизирует DataGrid.Columns и обновляет строки через UpdateRows().
            // RefreshHeaders() и RefreshRows() не нужны — они были бы дублирующими вызовами.
            OwnerGrid.SyncColumnsWithHeaders();

            // Обновляем агрегаты в нижней панели
            OwnerGrid.RefreshAggregates();
        }

        /// <summary>
        /// Рекурсивно обновляет GripperPosition для элемента и всех его дочерних элементов.
        /// Вызывается при межсекционном переносе заголовка через Drag&Drop.
        /// </summary>
        private static void UpdateGripperPositionRecursive(ColumnHeaderItem item, bool isRightPanel)
        {
            var newPosition = isRightPanel ? GripperPositionType.Left : GripperPositionType.Right;
            item.GripperPosition = newPosition;

            // Обновляем видимость gripper-ов в HeaderElement
            if (item.HeaderElement != null)
            {
                item.HeaderElement.UpdateGripperVisibility();
            }

            // Рекурсивно обновляем дочерние элементы
            if (item.HasChildren)
            {
                foreach (var child in item.Children)
                {
                    UpdateGripperPositionRecursive(child, isRightPanel);
                }
            }
        }

        /// <summary>
        /// Очищает состояние DnD-сессии.
        /// </summary>
        private void CleanupDrag()
        {
            // Снимаем пометку IsDragging с источника
            if (_dragSourceItem != null)
            {
                _dragSourceItem.IsDragging = false;
                _dragSourceItem = null;
            }

            // Снимаем подсветку с target
            if (_lastDropTargetItem != null)
            {
                _lastDropTargetItem.IsDropTarget = false;
                _lastDropTargetItem = null;
            }

            // Удаляем Adorner-ы
            if (_dragAdornerLayer != null)
            {
                if (_dragAdorner != null)
                {
                    _dragAdornerLayer.Remove(_dragAdorner);
                    _dragAdorner = null;
                }
                if (_dropTargetAdorner != null)
                {
                    _dragAdornerLayer.Remove(_dropTargetAdorner);
                    _dropTargetAdorner = null;
                }
                _dragAdornerLayer = null;
            }

            _isDragActive = false;
        }

        #endregion

        private void ApplyColumnsAndRows(Grid wrapper, int rowCount, int colCount)
        {
            wrapper.ColumnDefinitions.Clear();
            wrapper.RowDefinitions.Clear();

            // Используем Height="*" для всех строк, чтобы они были одинаковой высоты.
            // Это гарантирует, что в многоуровневых заголовках все уровни (ряды Grid)
            // будут синхронизированы по высоте, даже если содержимое разное
            // (например, HeaderTemplate с кнопкой и стандартный текст).
            // MinHeight="28" синхронизирует минимальную высоту всех строк,
            // чтобы строка с компактным HeaderTemplate (кнопка 22px) не была ниже
            // строки со стандартным заголовком (MinHeight=28 из стиля ColumnHeader).
            for (int i = 0; i < rowCount; i++)
                wrapper.RowDefinitions.Add(new RowDefinition
                {
                    Height = new GridLength(1, GridUnitType.Star),
                    MinHeight = 28
                });

            for (int i = 0; i < colCount; i++)
                wrapper.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Последняя колонка на растяжение
            wrapper.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        }

        private void InsertHeaders(Grid wrapper, ColumnHeaderCollection headers, int maxRowCount, int row, int col, bool isRightPanel = false)
        {
            int usedColumnCount = 0;
            foreach (var header in headers)
            {
                if (isRightPanel)
                {
                    header.GripperPosition = GripperPositionType.Left;
                }

                if (header.HasChildren)
                {
                    InsertHeaders(wrapper, header.Children, maxRowCount - 1, row + 1, col + usedColumnCount, isRightPanel);

                    var headerElement = header.HeaderElement;
                    headerElement.Width = header.Width;
                    Grid.SetColumn(headerElement, col + usedColumnCount);
                    Grid.SetRow(headerElement, row);

                    if (header.Children.Count > 0)
                    {
                        Grid.SetRowSpan(headerElement, 1);

                        // ВАЖНО: ColSpan должен учитывать только ВИДИМЫЕ дочерние элементы
                        var visibleBottomCount = GetVisibleBottomItemsCount(header.Children);
                        if (visibleBottomCount != 0)
                        {
                            Grid.SetColumnSpan(headerElement, visibleBottomCount);
                            usedColumnCount += visibleBottomCount;
                        }
                    }
                    else
                    {
                        Grid.SetRowSpan(headerElement, maxRowCount);
                        usedColumnCount++;
                    }

                    headerElement.Visibility = header.IsVisible ? Visibility.Visible : Visibility.Collapsed;

                    if (headerElement.Parent != null)
                    {
                        if (headerElement.Parent is Panel oldPanel)
                            oldPanel.Children.Remove(headerElement);
                    }
                    wrapper.Children.Add(headerElement);
                }
                else
                {
                    var headerElement = header.HeaderElement;
                    headerElement.Width = header.Width;

                    if (header.IsVisible)
                    {
                        Grid.SetColumn(headerElement, col + usedColumnCount);
                        Grid.SetRow(headerElement, row);
                        Grid.SetRowSpan(headerElement, maxRowCount);

                        usedColumnCount++;
                        headerElement.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        headerElement.Visibility = Visibility.Collapsed;
                    }

                    if (headerElement.Parent != null)
                    {
                        if (headerElement.Parent is Panel oldPanel)
                            oldPanel.Children.Remove(headerElement);
                    }
                    wrapper.Children.Add(headerElement);
                }
            }
        }

        private static void ClearWrapper(Grid wrapper)
        {
            if (wrapper == null) return;

            while (wrapper.Children.Count > 0)
            {
                var child = wrapper.Children[0];
                wrapper.Children.RemoveAt(0);
            }

            wrapper.ColumnDefinitions.Clear();
            wrapper.RowDefinitions.Clear();
        }

        private int GetVisibleBottomItemsCount(ColumnHeaderCollection collection)
        {
            int count = 0;
            foreach (var item in collection)
            {
                if (!item.IsVisible) continue;

                if (!item.HasChildren)
                    count++;
                else
                    count += GetVisibleBottomItemsCount(item.Children);
            }
            return count;
        }

        /// <summary>
        /// Подписывается на изменение ActualWidth всех колонок DataGrid через WeakDependencyPropertyListener.
        /// Автоматически отписывается при сборке GC подписчика, предотвращая утечку памяти
        /// через глобальный EventHandlerStore.
        /// </summary>
        /// <summary>
        /// Подписывается на изменение ActualWidth всех колонок DataGrid через WeakDependencyPropertyListener.
        /// Также подписывается на CollectionChanged колонок, чтобы отписывать слушатели
        /// при удалении колонок (предотвращает утечку через глобальный EventHandlerStore).
        /// </summary>
        private void SubscribeToColumnWidths()
        {
            var grid = OwnerGrid;
            if (grid == null) return;

            // Отписываемся от старых подписок перед переподпиской
            UnsubscribeFromColumnWidths();

            foreach (DataGridColumn column in grid.Columns)
            {
                AddColumnWidthListener(column);
            }

            // Подписываемся на CollectionChanged колонок, чтобы при удалении колонки
            // отписывать соответствующий WeakDependencyPropertyListener.
            // Без этого слушатели удалённых колонок навсегда остаются в EventHandlerStore.
            grid.Columns.CollectionChanged += OnColumnsCollectionChangedForWidths;
        }

        private void OnColumnsCollectionChangedForWidths(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Replace)
            {
                if (e.OldItems != null)
                {
                    foreach (DataGridColumn oldColumn in e.OldItems)
                    {
                        RemoveColumnWidthListener(oldColumn);
                    }
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Replace)
            {
                if (e.NewItems != null)
                {
                    foreach (DataGridColumn newColumn in e.NewItems)
                    {
                        AddColumnWidthListener(newColumn);
                    }
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                // При полном сбросе переподписываемся на все текущие колонки
                UnsubscribeFromColumnWidths();
                var grid = OwnerGrid;
                if (grid != null)
                {
                    foreach (DataGridColumn column in grid.Columns)
                    {
                        AddColumnWidthListener(column);
                    }
                }
            }
        }

        private void AddColumnWidthListener(DataGridColumn column)
        {
            var descriptor = DependencyPropertyDescriptor.FromProperty(
                DataGridColumn.ActualWidthProperty, typeof(DataGridColumn));
            if (descriptor != null)
            {
                var listener = new WeakDependencyPropertyListener(
                    descriptor, column, this, OnColumnWidthChanged);
                _columnWidthListeners.Add(listener);
            }
        }

        private void RemoveColumnWidthListener(DataGridColumn column)
        {
            // Ищем слушатель для указанной колонки и отписываем его
            for (int i = _columnWidthListeners.Count - 1; i >= 0; i--)
            {
                if (ReferenceEquals(_columnWidthListeners[i].Source, column))
                {
                    _columnWidthListeners[i].Dispose();
                    _columnWidthListeners.RemoveAt(i);
                    break;
                }
            }
        }

        /// <summary>
        /// Отписывается от изменения ActualWidth всех колонок.
        /// </summary>
        private void UnsubscribeFromColumnWidths()
        {
            var grid = OwnerGrid;
            if (grid != null)
            {
                grid.Columns.CollectionChanged -= OnColumnsCollectionChangedForWidths;
            }

            foreach (var listener in _columnWidthListeners)
            {
                listener.Dispose();
            }
            _columnWidthListeners.Clear();
        }

        private void OnColumnWidthChanged(object sender, EventArgs e)
        {
            if (sender is DataGridColumn column)
            {
                var headerItem = OwnerGrid?.GetColumnHeaderItem(column);
                if (headerItem != null && !headerItem.HasChildren)
                {
                    var actualWidth = column.ActualWidth;
                    if (actualWidth > 0 && Math.Abs(headerItem.Width - actualWidth) > 0.1)
                    {
                        headerItem.Width = actualWidth;
                    }
                }
            }

            // При изменении ширины колонки обновляем ColumnDefinitions в Grid-обёртках.
            // Полная перестройка всех заголовков (GenerateHeaderElements) не требуется,
            // так как ColumnHeaderItem сами отслеживают свою ширину через WidthProperty.
            // Но Grid.ColumnDefinitions имеют ширину GridLength.Auto, и при изменении
            // ActualWidth колонки нужно принудительно обновить layout.
            if (_frozenHeadersWrapper != null)
                _frozenHeadersWrapper.InvalidateMeasure();
            if (_headersWrapper != null)
                _headersWrapper.InvalidateMeasure();
            if (_rightFrozenHeadersWrapper != null)
                _rightFrozenHeadersWrapper.InvalidateMeasure();

            // Обновляем ScrollableWidth при изменении ширины колонок
            UpdateScrollableWidth();
        }

        private void OnHeadersScrollViewerScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // ScrollChanged срабатывает при изменении ExtentWidth, ViewportWidth, ScrollableWidth.
            // Обновляем ScrollableWidth, чтобы ScrollBar.Maximum всегда был актуален.
            UpdateScrollableWidth();
        }

        private static T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            var parentObject = GetParentObject(child);
            if (parentObject == null)
                return null;

            if (parentObject is T parent)
                return parent;
            else
                return FindVisualParent<T>(parentObject);
        }

        private static DependencyObject GetParentObject(DependencyObject child)
        {
            if (child == null)
                return null;

            if (child is ContentElement contentElement)
            {
                DependencyObject parent = ContentOperations.GetParent(contentElement);
                if (parent != null)
                    return parent;

                return contentElement is FrameworkContentElement fce ? fce.Parent : null;
            }

            if (child is FrameworkElement frameworkElement)
            {
                DependencyObject parent = frameworkElement.Parent;
                if (parent != null)
                    return parent;
            }

            return VisualTreeHelperEx.GetParent(child);
        }

        #endregion

        #region Public Override Methods

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _frozenHeadersWrapper = GetTemplateChild(FrozenHeadersWrapperPartName) as Grid;
            _headersScrollViewer = GetTemplateChild(HeadersScrollViewerPartName) as ScrollViewer;
            _headersWrapper = GetTemplateChild(HeadersWrapperPartName) as Grid;
            _rightFrozenHeadersWrapper = GetTemplateChild(RightFrozenHeadersWrapperPartName) as Grid;

            if (_headersScrollViewer != null)
            {
                // OneWay binding: заголовки только читают offset из ScrollManager
                var presenter = OwnerGrid?.FindName("PART_CentralRowsPresenter") as UnifiedRowsPresenter;
                if (presenter?.ScrollManager != null)
                {
                    // Очищаем старые Binding-и перед созданием новых (защита от повторного OnApplyTemplate)
                    BindingOperations.ClearBinding(this, HeadersScrollHorizontalOffsetProperty);
                    BindingOperations.ClearBinding(this, MaxHeadersScrollHorizontalOffsetProperty);
#if DEBUG
                    MemoryDiagnostics.OnBindingCleared();
                    MemoryDiagnostics.OnBindingCleared();
#endif

                    var offsetBinding = new Binding
                    {
                        Source = presenter.ScrollManager,
                        Path = new PropertyPath("HorizontalOffset"),
                        Mode = BindingMode.OneWay
                    };
                    SetBinding(HeadersScrollHorizontalOffsetProperty, offsetBinding);

                    var maxBinding = new Binding
                    {
                        Source = presenter.ScrollManager,
                        Path = new PropertyPath("MaxHorizontalOffset"),
                        Mode = BindingMode.OneWay
                    };
                    SetBinding(MaxHeadersScrollHorizontalOffsetProperty, maxBinding);

#if DEBUG
                    MemoryDiagnostics.OnBindingCreated();
                    MemoryDiagnostics.OnBindingCreated();
#endif
                }

                // Отписываем предыдущий обработчик перед подпиской (защита от повторного OnApplyTemplate)
                _headersScrollViewer.ScrollChanged -= OnHeadersScrollViewerScrollChanged;
                // Подписываемся на ScrollChanged, чтобы обновлять ScrollableWidth
                // при изменении размеров контента или вьюпорта ScrollViewer
                _headersScrollViewer.ScrollChanged += OnHeadersScrollViewerScrollChanged;
            }

            // Подписываемся на Unloaded для очистки Binding-ов при удалении из визуального дерева.
            // Binding создаёт подписку через PropertyChanged событие ScrollManager,
            // и без явной отписки Binding остаётся в памяти навсегда.
            // Отписываем предыдущий обработчик перед подпиской (защита от повторного OnApplyTemplate).
            Unloaded -= OnUnloaded;
            Unloaded += OnUnloaded;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            // Завершаем активную DnD-сессию, если она была
            if (_dragSourceItem != null)
            {
                if (IsMouseCaptured)
                    ReleaseMouseCapture();
                CleanupDrag();
            }

            // Очищаем Binding-и на ScrollManager, чтобы предотвратить утечку памяти
            // через PropertyChanged подписку при удалении из визуального дерева.
            BindingOperations.ClearBinding(this, HeadersScrollHorizontalOffsetProperty);
            BindingOperations.ClearBinding(this, MaxHeadersScrollHorizontalOffsetProperty);
#if DEBUG
            MemoryDiagnostics.OnBindingCleared();
            MemoryDiagnostics.OnBindingCleared();
#endif

            // Отписываемся от ScrollChanged в ScrollViewer заголовков
            if (_headersScrollViewer != null)
            {
                _headersScrollViewer.ScrollChanged -= OnHeadersScrollViewerScrollChanged;
            }

            // Отписываемся от изменения ширины всех колонок через WeakDependencyPropertyListener.
            // Без этого слушатели остаются в глобальном EventHandlerStore DependencyPropertyDescriptor,
            // что приводит к утечке памяти при повторной загрузке/выгрузке.
            UnsubscribeFromColumnWidths();

            // Отписываемся от Unloaded, чтобы не накапливать подписки
            Unloaded -= OnUnloaded;
        }



        #endregion

        /// <summary>
        /// Результат определения целевой позиции вставки при Drag & Drop.
        /// </summary>
        internal class DropTargetInfo
        {
            /// <summary>Целевая коллекция заголовков.</summary>
            public ColumnHeaderCollection Collection { get; set; }

            /// <summary>Индекс вставки в целевой коллекции.</summary>
            public int InsertIndex { get; set; }

            /// <summary>Целевой элемент заголовка (может быть null, если коллекция пуста).</summary>
            public ColumnHeaderItem TargetItem { get; set; }

            /// <summary>true — вставить перед TargetItem, false — после.</summary>
            public bool InsertBefore { get; set; }
        }
    }

    /// <summary>
    /// Вспомогательный класс для обхода визуального дерева.
    /// </summary>
    internal static class VisualTreeHelperEx
    {
        public static DependencyObject GetParent(DependencyObject reference)
        {
            if (reference == null)
                return null;

            // Пробуем через VisualTreeHelper
            try
            {
                var parent = VisualTreeHelper.GetParent(reference);
                if (parent != null)
                    return parent;
            }
            catch
            {
                // Игнорируем
            }

            // Пробуем через FrameworkElement.Parent
            if (reference is FrameworkElement fe)
                return fe.Parent;

            return null;
        }
    }
}
