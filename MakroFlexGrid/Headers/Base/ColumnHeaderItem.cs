using MakroFlexGrid.Core;
using MakroFlexGrid.Filters;
using MakroFlexGrid.Sorting;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MakroFlexGrid.Headers
{
    /// <summary>
    /// Базовый класс для элемента заголовка колонки.
    /// Поддерживает вложенность (группы колонок), сортировку, ресайз.
    /// Аналог Band из FlexGrid.
    /// </summary>
    /// 
    public enum GripperPositionType
    {
        Left,
        Right
    }
    public abstract class ColumnHeaderItem : FrameworkElement
    {
        static ColumnHeaderItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ColumnHeaderItem),
                new FrameworkPropertyMetadata(typeof(ColumnHeaderItem)));
            WidthProperty.OverrideMetadata(
                typeof(ColumnHeaderItem),
                new FrameworkPropertyMetadata(20d));
            MinWidthProperty.OverrideMetadata(
                typeof(ColumnHeaderItem),
                new FrameworkPropertyMetadata(20d));
        }

        protected ColumnHeaderItem()
        {
            Children = new ColumnHeaderCollection(this);
            Children.CollectionChanged += OnChildrenCollectionChanged;
        }

        #region Dependency Properties

        public static readonly DependencyProperty IsVisibleProperty =
            DependencyProperty.Register(
                "IsVisible",
                typeof(bool),
                typeof(ColumnHeaderItem),
                new FrameworkPropertyMetadata(true));

        public bool IsVisible
        {
            get => (bool)GetValue(IsVisibleProperty);
            set => SetValue(IsVisibleProperty, value);
        }

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(
                "Header",
                typeof(object),
                typeof(ColumnHeaderItem),
                new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty HeaderStyleProperty =
            DependencyProperty.Register(
                "HeaderStyle",
                typeof(Style),
                typeof(ColumnHeaderItem),
                new FrameworkPropertyMetadata(null));

        /// <summary>
        /// DataTemplate для кастомного отображения содержимого заголовка колонки.
        /// Если не задан, заголовок отображается как обычный текст (Header.ToString()).
        /// Позволяет показывать в заголовке иконки, кнопки, комбобоксы и т.д.
        /// </summary>
        public static readonly DependencyProperty HeaderTemplateProperty =
            DependencyProperty.Register(
                "HeaderTemplate",
                typeof(DataTemplate),
                typeof(ColumnHeaderItem),
                new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty HorizontalHeaderAlignmentProperty =
            DependencyProperty.Register(
                "HorizontalHeaderAlignment",
                typeof(HorizontalAlignment),
                typeof(ColumnHeaderItem),
                new FrameworkPropertyMetadata(HorizontalAlignment.Center));

        public static readonly DependencyProperty VerticalHeaderAlignmentProperty =
            DependencyProperty.Register(
                "VerticalHeaderAlignment",
                typeof(VerticalAlignment),
                typeof(ColumnHeaderItem),
                new FrameworkPropertyMetadata(VerticalAlignment.Center));

        public static readonly DependencyProperty CanUserSortProperty =
            DependencyProperty.Register(
                "CanUserSort",
                typeof(bool),
                typeof(ColumnHeaderItem),
                new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty SortDirectionProperty =
            DependencyProperty.Register(
                "SortDirection",
                typeof(ListSortDirection?),
                typeof(ColumnHeaderItem),
                new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty SortMemberPathProperty =
            DependencyProperty.Register(
                "SortMemberPath",
                typeof(string),
                typeof(ColumnHeaderItem),
                new FrameworkPropertyMetadata(string.Empty));

        public static readonly DependencyProperty SortDataTypeProperty =
            DependencyProperty.Register(
                "SortDataType",
                typeof(SortDataType),
                typeof(ColumnHeaderItem),
                new FrameworkPropertyMetadata(SortDataType.Text));

        public static readonly DependencyProperty CellTemplateProperty =
            DependencyProperty.Register(
                "CellTemplate",
                typeof(DataTemplate),
                typeof(ColumnHeaderItem),
                new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty AggregateTypeProperty =
            DependencyProperty.Register(
                "AggregateType",
                typeof(Headers.AggregateType),
                typeof(ColumnHeaderItem),
                new FrameworkPropertyMetadata(Headers.AggregateType.None));

        /// <summary>
        /// Кастомный DataTemplate для ячейки нижней панели итогов (BottomPanel).
        /// Если не задан, используется CellTemplate (или стандартный DefaultCellTemplate).
        /// Позволяет задать разный визуал для обычной ячейки и ячейки итога.
        /// </summary>
        public static readonly DependencyProperty BottomCellTemplateProperty =
            DependencyProperty.Register(
                "BottomCellTemplate",
                typeof(DataTemplate),
                typeof(ColumnHeaderItem),
                new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Указывает, можно ли перетаскивать этот заголовок через Drag & Drop.
        /// Для корневых заголовков по умолчанию true, для системных — false.
        /// </summary>
        public static readonly DependencyProperty AllowDragProperty =
            DependencyProperty.Register(
                "AllowDrag",
                typeof(bool),
                typeof(ColumnHeaderItem),
                new FrameworkPropertyMetadata(true));

        /// <summary>
        /// Указывает, можно ли переносить этот заголовок между разделами
        /// (Frozen/Scrollable/RightFrozen).
        /// Если false — колонку можно перемещать только внутри её текущего раздела.
        /// По умолчанию true (межсекционный перенос разрешён).
        /// </summary>
        public static readonly DependencyProperty AllowCrossSectionDragProperty =
            DependencyProperty.Register(
                "AllowCrossSectionDrag",
                typeof(bool),
                typeof(ColumnHeaderItem),
                new FrameworkPropertyMetadata(true));

        /// <summary>
        /// Указывает, что этот заголовок в данный момент перетаскивается.
        /// Используется для визуальной обратной связи (полупрозрачность).
        /// </summary>
        public static readonly DependencyProperty IsDraggingProperty =
            DependencyProperty.Register(
                "IsDragging",
                typeof(bool),
                typeof(ColumnHeaderItem),
                new FrameworkPropertyMetadata(false));

        /// <summary>
        /// Указывает, что этот заголовок является целью для вставки (подсветка при наведении).
        /// </summary>
        public static readonly DependencyProperty IsDropTargetProperty =
            DependencyProperty.Register(
                "IsDropTarget",
                typeof(bool),
                typeof(ColumnHeaderItem),
                new FrameworkPropertyMetadata(false));

        /// <summary>
        /// Определяет, разрешена ли фильтрация для данной колонки.
        /// По умолчанию true. Если установить false, пункты фильтрации
        /// не будут отображаться в контекстном меню заголовка.
        /// </summary>
        public static readonly DependencyProperty CanUserFilterProperty =
            DependencyProperty.Register(
                "CanUserFilter",
                typeof(bool),
                typeof(ColumnHeaderItem),
                new FrameworkPropertyMetadata(true));

        /// <summary>
        /// Разрешена ли фильтрация для данной колонки.
        /// </summary>
        public bool CanUserFilter
        {
            get => (bool)GetValue(CanUserFilterProperty);
            set => SetValue(CanUserFilterProperty, value);
        }

        /// <summary>
        /// Определяет, разрешено ли пользователю скрывать данную колонку
        /// через контекстное меню заголовка.
        /// По умолчанию true. Если установить false, пункты скрытия
        /// не будут отображаться в контекстном меню.
        /// </summary>
        public static readonly DependencyProperty CanUserHideProperty =
            DependencyProperty.Register(
                "CanUserHide",
                typeof(bool),
                typeof(ColumnHeaderItem),
                new FrameworkPropertyMetadata(true));

        /// <summary>
        /// Разрешено ли пользователю скрывать данную колонку через UI.
        /// </summary>
        public bool CanUserHide
        {
            get => (bool)GetValue(CanUserHideProperty);
            set => SetValue(CanUserHideProperty, value);
        }

        /// <summary>
        /// Фильтр для данной колонки. Если не задан, фильтр не применяется.
        /// </summary>
        public static readonly DependencyProperty FilterProperty =
            DependencyProperty.Register(
                "Filter",
                typeof(ColumnFilterBase),
                typeof(ColumnHeaderItem),
                new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Фильтр для данной колонки. Если не задан, фильтр не применяется.
        /// </summary>
        public ColumnFilterBase Filter
        {
            get => (ColumnFilterBase)GetValue(FilterProperty);
            set => SetValue(FilterProperty, value);
        }

        #endregion



        public GripperPositionType GripperPosition { get; set; } = GripperPositionType.Right;

        #region Private Variables

        private ColumnHeader _headerElement;
        private CustomDataGrid _ownerGrid;
        private ColumnHeaderItem _parentItem;
        private bool _settingWithoutParent;
        private bool _settingWithoutChildren;
        private double oldWidth;
        private double oldMinWidth;

        #endregion

        #region Internal Properties

        internal ColumnHeaderItem ParentItem
        {
            get => _parentItem;
            set
            {
                if (_parentItem != value)
                {
                    var oldParent = _parentItem;
                    _parentItem = value;
                    OnParentItemChanged(oldParent, value);
                }
            }
        }

        internal CustomDataGrid OwnerGrid
        {
            get => _ownerGrid;
            set
            {
                if (_ownerGrid != value)
                {
                    var oldOwner = _ownerGrid;
                    _ownerGrid = value;
                    OnOwnerGridChanged(oldOwner, value);
                }
            }
        }

        internal ColumnHeader HeaderElement
        {
            get
            {
                if (_headerElement == null)
                    _headerElement = new ColumnHeader(this);
                return _headerElement;
            }
        }

        internal DataGridColumn SyncColumn { get; set; }

        internal bool IsDeferredResizeEnabled
        {
            get
            {
                if (OwnerGrid != null)
                    return OwnerGrid.IsDeferredResizeEnabled;

                // Fallback: ищем через визуальное дерево
                var header = HeaderElement;
                if (header == null) return false;

                DependencyObject current = VisualTreeHelper.GetParent(header);
                while (current != null)
                {
                    if (current is CustomDataGrid grid)
                        return grid.IsDeferredResizeEnabled;
                    current = VisualTreeHelper.GetParent(current);
                }

                return false;
            }
        }

        #endregion

        #region Public Properties

        public int Depth => Children.MaxDepth + 1;

        public ColumnHeaderCollection Children { get; }

        public bool HasChildren => Children.Count > 0;

        public object Header
        {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public Style HeaderStyle
        {
            get => (Style)GetValue(HeaderStyleProperty);
            set => SetValue(HeaderStyleProperty, value);
        }

        /// <summary>
        /// DataTemplate для кастомного отображения содержимого заголовка колонки.
        /// Если не задан, заголовок отображается как обычный текст.
        /// </summary>
        public DataTemplate HeaderTemplate
        {
            get => (DataTemplate)GetValue(HeaderTemplateProperty);
            set => SetValue(HeaderTemplateProperty, value);
        }

        public HorizontalAlignment HorizontalHeaderAlignment
        {
            get => (HorizontalAlignment)GetValue(HorizontalHeaderAlignmentProperty);
            set => SetValue(HorizontalHeaderAlignmentProperty, value);
        }

        public VerticalAlignment VerticalHeaderAlignment
        {
            get => (VerticalAlignment)GetValue(VerticalHeaderAlignmentProperty);
            set => SetValue(VerticalHeaderAlignmentProperty, value);
        }

        public bool CanUserSort
        {
            get => (bool)GetValue(CanUserSortProperty);
            set => SetValue(CanUserSortProperty, value);
        }

        public ListSortDirection? SortDirection
        {
            get => (ListSortDirection?)GetValue(SortDirectionProperty);
            set => SetValue(SortDirectionProperty, value);
        }

        public string SortMemberPath
        {
            get => (string)GetValue(SortMemberPathProperty);
            set => SetValue(SortMemberPathProperty, value);
        }

        public SortDataType SortDataType
        {
            get => (SortDataType)GetValue(SortDataTypeProperty);
            set => SetValue(SortDataTypeProperty, value);
        }

        public DataTemplate CellTemplate
        {
            get => (DataTemplate)GetValue(CellTemplateProperty);
            set => SetValue(CellTemplateProperty, value);
        }

        /// <summary>
        /// Кастомный DataTemplate для ячейки нижней панели итогов (BottomPanel).
        /// Если не задан, используется CellTemplate (или стандартный DefaultCellTemplate).
        /// </summary>
        public DataTemplate BottomCellTemplate
        {
            get => (DataTemplate)GetValue(BottomCellTemplateProperty);
            set => SetValue(BottomCellTemplateProperty, value);
    }

    public Headers.AggregateType AggregateType
        {
            get => (Headers.AggregateType)GetValue(AggregateTypeProperty);
            set => SetValue(AggregateTypeProperty, value);
        }

        /// <summary>
        /// Можно ли перетаскивать этот заголовок через Drag & Drop.
        /// </summary>
        public bool AllowDrag
        {
            get => (bool)GetValue(AllowDragProperty);
            set => SetValue(AllowDragProperty, value);
        }

        /// <summary>
        /// Можно ли переносить этот заголовок между разделами
        /// (Frozen/Scrollable/RightFrozen).
        /// Если false — колонку можно перемещать только внутри её текущего раздела.
        /// </summary>
        public bool AllowCrossSectionDrag
        {
            get => (bool)GetValue(AllowCrossSectionDragProperty);
            set => SetValue(AllowCrossSectionDragProperty, value);
        }

        /// <summary>
        /// Заголовок в данный момент перетаскивается.
        /// </summary>
        public bool IsDragging
        {
            get => (bool)GetValue(IsDraggingProperty);
            set => SetValue(IsDraggingProperty, value);
        }

        /// <summary>
        /// Заголовок является целью для вставки (подсветка при наведении).
        /// </summary>
        public bool IsDropTarget
        {
            get => (bool)GetValue(IsDropTargetProperty);
            set => SetValue(IsDropTargetProperty, value);
        }

        #endregion

        #region Private Methods

        private void SetChildrenWidth(double newTotalWidth)
        {
            if (_settingWithoutChildren)
                return;

            // Собираем только видимые дочерние элементы
            var visibleChildren = new List<ColumnHeaderItem>();
            foreach (var child in Children)
            {
                if (child.IsVisible)
                    visibleChildren.Add(child);
            }

            if (visibleChildren.Count == 0)
                return;

            // Фаза 1: гарантируем каждому видимому ребёнку MinWidth
            double totalMinWidth = 0;
            foreach (var child in visibleChildren)
                totalMinWidth += child.MinWidth;

            // Если новой ширины не хватает даже на MinWidth — каждый получает свой MinWidth
            if (newTotalWidth <= totalMinWidth)
            {
                foreach (var child in visibleChildren)
                    child.SetWidthWithoutParent(child.MinWidth);
                return;
            }

            // Фаза 2: распределяем остаток пропорционально "гибкости" каждого ребёнка
            double remainingWidth = newTotalWidth - totalMinWidth;
            double totalFlex = 0;
            var flexValues = new double[visibleChildren.Count];

            for (int i = 0; i < visibleChildren.Count; i++)
            {
                var child = visibleChildren[i];
                // "Гибкость" = насколько ребёнок может превысить свой MinWidth
                double flex = Math.Max(0, child.Width - child.MinWidth);
                flexValues[i] = flex;
                totalFlex += flex;
            }

            // Если никто не может растянуться — распределяем поровну
            if (totalFlex <= 0)
            {
                double equalShare = remainingWidth / visibleChildren.Count;
                foreach (var child in visibleChildren)
                    child.SetWidthWithoutParent(child.MinWidth + equalShare);
                return;
            }

            // Распределяем остаток пропорционально гибкости
            double assignedWidth = 0;
            for (int i = 0; i < visibleChildren.Count; i++)
            {
                var child = visibleChildren[i];
                double extra = remainingWidth * (flexValues[i] / totalFlex);
                double newChildWidth = child.MinWidth + extra;
                child.SetWidthWithoutParent(newChildWidth);
                assignedWidth += newChildWidth;
            }

            // Компенсация погрешности округления: добавляем/убираем разницу последнему видимому ребёнку
            double roundingError = newTotalWidth - assignedWidth;
            if (Math.Abs(roundingError) > 0.01 && visibleChildren.Count > 0)
            {
                var lastChild = visibleChildren[visibleChildren.Count - 1];
                lastChild.SetWidthWithoutParent(lastChild.Width + roundingError);
            }
        }

        private void SetParentWidth(double addedWidth)
        {
            if (ParentItem == null || _settingWithoutParent)
                return;

            var newWidth = ParentItem.Width + addedWidth;
            newWidth = Math.Max(ParentItem.MinWidth, newWidth);
            newWidth = Math.Min(ParentItem.MaxWidth, newWidth);

            ParentItem.SetWidthWithoutChildren(newWidth);
        }

        private void SetWidthWithoutParent(double width)
        {
            _settingWithoutParent = true;
            Width = width;
            _settingWithoutParent = false;
        }

        private void SetWidthWithoutChildren(double width)
        {
            _settingWithoutChildren = true;
            Width = width;
            _settingWithoutChildren = false;
        }


        private void UpdateAllWidths()
        {
            double totalWidth = 0;
            double totalMinWidth = 0;
            double totalMaxWidth = 0;

            foreach (var child in Children)
            {
                totalWidth += child.Width;
                totalMinWidth += child.MinWidth;
                totalMaxWidth += child.MaxWidth;
            }

            MinWidth = totalMinWidth;
            MaxWidth = totalMaxWidth;
            Width = totalWidth;

            ParentItem?.UpdateAllWidths();
        }

        /// <summary>
        /// Обновляет только MinWidth и MaxWidth родителя на основе суммы значений детей.
        /// В отличие от UpdateAllWidths, не трогает Width, чтобы не ломать ширину,
        /// установленную через Grid.
        /// </summary>
        private void UpdateMinMaxWidths()
        {
            double totalMinWidth = 0;
            double totalMaxWidth = 0;

            foreach (var child in Children)
            {
                totalMinWidth += child.MinWidth;
                totalMaxWidth += child.MaxWidth;
            }

            MinWidth = totalMinWidth;
            MaxWidth = totalMaxWidth;

            ParentItem?.UpdateMinMaxWidths();
        }

        private void SetWidth(double oldValue, double newValue)
        {
            newValue = Math.Max(MinWidth, newValue);
            newValue = Math.Min(MaxWidth, newValue);

            if (HeaderElement != null)
                HeaderElement.Width = newValue;

            SetChildrenWidth(newValue);
            SetParentWidth(newValue - oldValue);
        }

        internal void SetSyncColumnWidth(double newValue)
        {
            if (SyncColumn != null)
                SyncColumn.Width = new DataGridLength(newValue);
        }

        private void SetSyncColumnMinWidth(double newValue)
        {
            if (SyncColumn != null)
                SyncColumn.MinWidth = newValue;
        }

        private void SetSyncColumnMaxWidth(double newValue)
        {
            if (SyncColumn != null)
                SyncColumn.MaxWidth = newValue;
        }

        private void SetSyncColumnCanUserSort(bool newValue)
        {
            if (SyncColumn != null)
                SyncColumn.CanUserSort = newValue;
        }

        private void SetSyncColumnSortDirection(ListSortDirection? newValue)
        {
            if (SyncColumn != null)
                SyncColumn.SortDirection = newValue;

            if (HeaderElement != null)
                HeaderElement.SortDirection = newValue;
        }

        private void SetSyncColumnSortMemberPath(string newValue)
        {
            if (SyncColumn != null)
                SyncColumn.SortMemberPath = newValue;
        }

        private void SetHeaderElementDataContext(object newValue)
        {
            if (HeaderElement != null)
                HeaderElement.DataContext = newValue;
        }

        #endregion

        #region Event Handlers

        private void OnChildrenCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateAllWidths();
        }

        #endregion

        #region Internal Methods

        internal void PerformSort()
        {
            if (SyncColumn != null)
                OwnerGrid?.PerformSort(SyncColumn);
        }

        /// <summary>
        /// Возвращает корневой (самый верхний) элемент в иерархии заголовков.
        /// </summary>
        internal ColumnHeaderItem GetRootItem()
        {
            var current = this;
            while (current.ParentItem != null)
                current = current.ParentItem;
            return current;
        }

        #endregion

        #region Protected Virtual Methods

        protected virtual void OnParentItemChanged(ColumnHeaderItem oldParent, ColumnHeaderItem newParent) { }

        protected virtual void OnOwnerGridChanged(CustomDataGrid oldOwner, CustomDataGrid newOwner)
        {
            if (Children != null)
                Children.OwnerGrid = newOwner;
        }

        #endregion

        #region Protected Override Methods

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == IsVisibleProperty)
            {
                OnIsVisibleChanged((bool)e.NewValue);
                OwnerGrid?.RefreshHeaders();
            }
            else if (e.Property == WidthProperty)
                SetWidth((double)e.OldValue, (double)e.NewValue);
            else if (e.Property == MinWidthProperty)
                SetSyncColumnMinWidth((double)e.NewValue);
            else if (e.Property == MaxWidthProperty)
                SetSyncColumnMaxWidth((double)e.NewValue);
            else if (e.Property == CanUserSortProperty)
                SetSyncColumnCanUserSort((bool)e.NewValue);
            else if (e.Property == SortDirectionProperty)
                SetSyncColumnSortDirection((ListSortDirection?)e.NewValue);
            else if (e.Property == SortMemberPathProperty)
                SetSyncColumnSortMemberPath((string)e.NewValue);
            else if (e.Property == DataContextProperty)
                SetHeaderElementDataContext(e.NewValue);
            else if (e.Property == HeaderProperty)
                SetHeaderElementContent(e.NewValue);
            else if (e.Property == HeaderTemplateProperty)
                SetHeaderElementContentTemplate(e.NewValue as DataTemplate);
            else if (e.Property == AggregateTypeProperty)
                OnAggregateTypeChanged();
        }

        /// <summary>
        /// Вызывается при изменении типа агрегата для колонки.
        /// Уведомляет грид о необходимости пересчитать итоговые значения.
        /// </summary>
        private void OnAggregateTypeChanged()
        {
            OwnerGrid?.RefreshAggregates();
        }

        private void SetHeaderElementContent(object newValue)
        {
            if (HeaderElement != null)
                HeaderElement.UpdateContent(newValue);
        }

        private void SetHeaderElementContentTemplate(DataTemplate template)
        {
            if (HeaderElement != null)
                HeaderElement.ContentTemplate = template;
        }

        #endregion

        protected virtual void OnIsVisibleChanged(bool isVisible)
        {
            // Защита: предотвращаем скрытие последней видимой колонки
            if (!isVisible && ParentItem != null)
            {
                // Проверяем, можно ли скрыть эту колонку
                var root = GetRootItem();
                var allLeafColumns = GetAllLeafColumns(root);
                var visibleCount = allLeafColumns.Count(col => col.IsVisible);

                // Если это последняя видимая колонка, не даём её скрыть
                if (visibleCount <= 1 && IsVisible)
                {
                    // Можно показать сообщение пользователю (опционально)
                    // MessageBox.Show("Нельзя скрыть последнюю видимую колонку!");
                    return;
                }
            }

            if (!isVisible)
            {
                oldWidth = Width;
                oldMinWidth = MinWidth;

                if (HasChildren)
                {
                    // Для группы: устанавливаем Width/MinWidth через _settingWithoutChildren,
                    // чтобы SetWidth -> SetChildrenWidth не сломал пропорции детей.
                    // SetChildrenWidth принудительно сжимает всех ещё видимых детей до MinWidth,
                    // что перезаписывает их oldWidth ещё до того, как они сами будут скрыты.
                    _settingWithoutChildren = true;
                    try
                    {
                        this.MinWidth = 0;
                        this.Width = 0;
                    }
                    finally
                    {
                        _settingWithoutChildren = false;
                    }
                }
                else
                {
                    // Для листовых: старый код через SetWidth (безопасно)
                    this.MinWidth = 0;
                    this.Width = 0;
                }
            }
            else
            {
                if (HasChildren)
                {
                    // Для группы: сначала показываем детей (они восстановят свои ширины
                    // через собственный OnIsVisibleChanged), потом вычисляем ширину группы
                    // как сумму ширин видимых детей.
                    foreach (var child in Children)
                    {
                        if (!child.IsVisible)
                            child.IsVisible = true;
                    }

                    // Вычисляем ширину группы на основе восстановленных детей
                    double totalWidth = 0;
                    double totalMinWidth = 0;
                    foreach (var child in Children)
                    {
                        if (child.IsVisible)
                        {
                            totalWidth += child.Width;
                            totalMinWidth += child.MinWidth;
                        }
                    }

                    // Устанавливаем MinWidth и Width группы через _settingWithoutChildren,
                    // чтобы SetWidth -> SetChildrenWidth не перераспределил ширину детей
                    // (они уже сами восстановили свои размеры).
                    _settingWithoutChildren = true;
                    try
                    {
                        this.MinWidth = totalMinWidth > 0 ? totalMinWidth : (oldMinWidth > 0 ? oldMinWidth : 20d);
                        this.Width = totalWidth > 0 ? totalWidth : (oldWidth > 0 ? oldWidth : 150d);
                    }
                    finally
                    {
                        _settingWithoutChildren = false;
                    }
                }
                else
                {
                    // Для листовых: старый код
                    if (oldWidth > 0)
                        this.Width = oldWidth;
                    else
                        this.Width = 150;

                    // Восстанавливаем MinWidth, который был до скрытия
                    this.MinWidth = oldMinWidth > 0 ? oldMinWidth : 20d;
                }
            }

            if (SyncColumn != null)
            {
                SyncColumn.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
            }

            // Для групп дети уже обработаны выше (в блоке else).
            // Для листовых этот блок не имеет эффекта (нет детей).
            // Оставляем для случая, когда IsVisible устанавливается не через системную колонку,
            // а напрямую (тогда дети ещё не скрыты и их нужно скрыть рекурсивно).
            if (HasChildren && isVisible == false)
            {
                foreach (var child in Children)
                {
                    if (child.IsVisible)
                        child.IsVisible = false;
                }
            }

            // Синхронизируем MinWidth/MaxWidth родителя (группы) без изменения Width
            ParentItem?.UpdateMinMaxWidths();

            OwnerGrid?.SyncColumnsWithHeaders();
        }

        /// <summary>
        /// Проверяет, можно ли скрыть указанную колонку (останется ли хотя бы одна видимая).
        /// </summary>
        internal bool CanHideColumn(ColumnHeaderItem columnToHide)
        {
            // Получаем корневой элемент
            var root = GetRootItem();

            // Собираем все листовые колонки
            var allLeafColumns = GetAllLeafColumns(root);

            // Считаем видимые колонки (исключая ту, которую хотим скрыть)
            int visibleCount = allLeafColumns.Count(col => col != columnToHide && col.IsVisible);

            // Если после скрытия останется хотя бы одна видимая колонка
            return visibleCount >= 1;
        }

        /// <summary>
        /// Возвращает все листовые колонки в иерархии.
        /// </summary>
        public List<ColumnHeaderItem> GetAllLeafColumns(ColumnHeaderItem item)
        {
            var leaves = new List<ColumnHeaderItem>();

            if (!item.HasChildren)
            {
                leaves.Add(item);
            }
            else
            {
                foreach (var child in item.Children)
                {
                    leaves.AddRange(GetAllLeafColumns(child));
                }
            }

            return leaves;
        }
    }
}
