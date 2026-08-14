using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace MakroFlexGrid.Rows
{
    /// <summary>
    /// ViewModel для нижней панели с итоговыми значениями (BottomPanel).
    /// Содержит коллекции ячеек для левой frozen, центральной scrollable и правой frozen частей.
    /// </summary>
    public class BottomPanelViewModel : INotifyPropertyChanged
    {
        private double _horizontalOffset;
        private Brush _leftPanelBackground;
        private Brush _centerPanelBackground;
        private Brush _rightPanelBackground;
        private Brush _bottomPanelBackground;
        private Brush _gridLineBrush;
        private double _separatorWidth;
        private Brush _separatorBrush;
        private double _rowWidth;
        private bool _hasAggregates;
        private bool _showBottomCellBorders = true;
        private string _panelText;
        private System.Windows.HorizontalAlignment _panelTextAlignment = System.Windows.HorizontalAlignment.Center;
        private PanelTextPosition _panelTextPosition = PanelTextPosition.Top;
        private System.Windows.Thickness _panelTextPadding = new System.Windows.Thickness(0, 2, 0, 2);
        private System.Windows.DataTemplate _panelTextTemplate;
        private int _leftFrozenColumnsCount;
        private int _rightFrozenColumnsCount;

        public enum PanelTextPosition
        {
            Top,
            Bottom
        }

        /// <summary>
        /// Ячейки левой frozen панели.
        /// </summary>
        public ObservableCollection<BottomCellViewModel> LeftCells { get; } = new ObservableCollection<BottomCellViewModel>();

        /// <summary>
        /// Ячейки центральной scrollable панели.
        /// </summary>
        public ObservableCollection<BottomCellViewModel> CenterCells { get; } = new ObservableCollection<BottomCellViewModel>();

        /// <summary>
        /// Ячейки правой frozen панели.
        /// </summary>
        public ObservableCollection<BottomCellViewModel> RightCells { get; } = new ObservableCollection<BottomCellViewModel>();

        /// <summary>
        /// Горизонтальный offset для синхронизации скролла.
        /// </summary>
        public double HorizontalOffset
        {
            get => _horizontalOffset;
            set
            {
                if (System.Math.Abs(_horizontalOffset - value) > 0.01)
                {
                    _horizontalOffset = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Фон левой frozen панели.
        /// </summary>
        public Brush LeftPanelBackground
        {
            get => _leftPanelBackground;
            set
            {
                if (_leftPanelBackground != value)
                {
                    _leftPanelBackground = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Фон центральной панели.
        /// </summary>
        public Brush CenterPanelBackground
        {
            get => _centerPanelBackground;
            set
            {
                if (_centerPanelBackground != value)
                {
                    _centerPanelBackground = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Фон правой frozen панели.
        /// </summary>
        public Brush RightPanelBackground
        {
            get => _rightPanelBackground;
            set
            {
                if (_rightPanelBackground != value)
                {
                    _rightPanelBackground = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Единый фон для всей нижней панели итогов.
        /// </summary>
        public Brush BottomPanelBackground
        {
            get => _bottomPanelBackground;
            set
            {
                if (_bottomPanelBackground != value)
                {
                    _bottomPanelBackground = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Кисть для линий сетки.
        /// </summary>
        public Brush GridLineBrush
        {
            get => _gridLineBrush;
            set
            {
                if (_gridLineBrush != value)
                {
                    _gridLineBrush = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Ширина разделителей между frozen и scrollable частями.
        /// </summary>
        public double SeparatorWidth
        {
            get => _separatorWidth;
            set
            {
                if (System.Math.Abs(_separatorWidth - value) > 0.01)
                {
                    _separatorWidth = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Кисть для разделителей.
        /// </summary>
        public Brush SeparatorBrush
        {
            get => _separatorBrush;
            set
            {
                if (_separatorBrush != value)
                {
                    _separatorBrush = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Ширина строки (вся панель).
        /// </summary>
        public double RowWidth
        {
            get => _rowWidth;
            set
            {
                if (System.Math.Abs(_rowWidth - value) > 0.01)
                {
                    _rowWidth = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// True, если есть хотя бы одна ячейка с непустым итоговым значением.
        /// Используется для скрытия панели, когда итоги не нужны.
        /// </summary>
        public bool HasAggregates
        {
            get => _hasAggregates;
            set
            {
                if (_hasAggregates != value)
                {
                    _hasAggregates = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Текст, который отображается во всю ширину панели над итоговыми ячейками.
        /// </summary>
        public string PanelText
        {
            get => _panelText;
            set
            {
                if (_panelText != value)
                {
                    _panelText = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Выравнивание текста в нижней панели.
        /// </summary>
        public System.Windows.HorizontalAlignment PanelTextAlignment
        {
            get => _panelTextAlignment;
            set
            {
                if (_panelTextAlignment != value)
                {
                    _panelTextAlignment = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Позиция текста относительно ячеек итогов (над или под).
        /// </summary>
        public PanelTextPosition TextPosition
        {
            get => _panelTextPosition;
            set
            {
                if (_panelTextPosition != value)
                {
                    _panelTextPosition = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Внутренние отступы для текста в нижней панели.
        /// </summary>
        public System.Windows.Thickness PanelTextPadding
        {
            get => _panelTextPadding;
            set
            {
                if (_panelTextPadding != value)
                {
                    _panelTextPadding = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Кастомный шаблон для отображения текста нижней панели.
        /// Если не задан, используется стандартный TextBlock.
        /// </summary>
        public DataTemplate PanelTextTemplate
        {
            get => _panelTextTemplate;
            set
            {
                if (_panelTextTemplate != value)
                {
                    _panelTextTemplate = value;
                    OnPropertyChanged();
                }
            }
        }
        /// <summary>
        /// Показывать ли бордюры ячеек в нижней панели итогов.
        /// </summary>
        public bool ShowBottomCellBorders
        {
            get => _showBottomCellBorders;
            set
            {
                if (_showBottomCellBorders != value)
                {
                    _showBottomCellBorders = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Количество замороженных колонок слева.
        /// Используется для управления видимостью левого разделителя.
        /// </summary>
        public int LeftFrozenColumnsCount
        {
            get => _leftFrozenColumnsCount;
            set
            {
                if (_leftFrozenColumnsCount != value)
                {
                    _leftFrozenColumnsCount = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Количество замороженных колонок справа.
        /// Используется для управления видимостью правого разделителя.
        /// </summary>
        public int RightFrozenColumnsCount
        {
            get => _rightFrozenColumnsCount;
            set
            {
                if (_rightFrozenColumnsCount != value)
                {
                    _rightFrozenColumnsCount = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}