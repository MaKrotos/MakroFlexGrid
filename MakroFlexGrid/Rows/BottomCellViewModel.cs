using MakroFlexGrid.Utilities;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MakroFlexGrid.Rows
{
    /// <summary>
    /// ViewModel для ячейки итогового значения в нижней панели (BottomPanel).
    /// </summary>
    public class BottomCellViewModel : INotifyPropertyChanged
    {
        private double _width;
        private string _value;
        private Brush _gridLineBrush;
        private bool _isLeftmostInRightPanel;
        private DataTemplate _template;
        private DataGridColumn _column;
        private WeakDependencyPropertyListener _columnWidthListener;
        private bool _isSubscribedToWidth;

        /// <summary>
        /// Ширина ячейки (соответствует ширине колонки).
        /// </summary>
        public double Width
        {
            get => _width;
            set
            {
                if (System.Math.Abs(_width - value) > 0.01)
                {
                    _width = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Итоговое значение (строка).
        /// </summary>
        public string Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
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
        /// True, если эта ячейка — самая левая в правой frozen-панели.
        /// </summary>
        public bool IsLeftmostInRightPanel
        {
            get => _isLeftmostInRightPanel;
            set
            {
                if (_isLeftmostInRightPanel != value)
                {
                    _isLeftmostInRightPanel = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Кастомный DataTemplate для визуального отображения ячейки.
        /// Если не задан, используется DefaultCellTemplate (TextBlock с привязкой к Value).
        /// </summary>
        public DataTemplate Template
        {
            get => _template;
            set
            {
                if (_template != value)
                {
                    _template = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Устанавливает колонку и подписывается на изменения её ширины.
        /// </summary>
        public void SetColumn(DataGridColumn column)
        {
            UnsubscribeFromColumnWidth();
            _column = column;
            SubscribeToColumnWidth();
        }

        /// <summary>
        /// Отписывается от событий и очищает ресурсы.
        /// </summary>
        public void Cleanup()
        {
            UnsubscribeFromColumnWidth();
        }

        /// <summary>
        /// Кэшированный DependencyPropertyDescriptor для DataGridColumn.ActualWidthProperty.
        /// </summary>
        private static readonly DependencyPropertyDescriptor ActualWidthDescriptor =
            DependencyPropertyDescriptor.FromProperty(
                DataGridColumn.ActualWidthProperty, typeof(DataGridColumn));

        private void SubscribeToColumnWidth()
        {
            if (_isSubscribedToWidth || _column == null) return;
            _isSubscribedToWidth = true;

            if (ActualWidthDescriptor != null)
            {
                _columnWidthListener = new WeakDependencyPropertyListener(
                    ActualWidthDescriptor, _column, this, OnColumnWidthChanged);
            }
        }

        private void UnsubscribeFromColumnWidth()
        {
            if (!_isSubscribedToWidth) return;
            _isSubscribedToWidth = false;

            if (_columnWidthListener != null)
            {
                _columnWidthListener.Dispose();
                _columnWidthListener = null;
            }
        }

        private void OnColumnWidthChanged(object sender, EventArgs e)
        {
            if (_column != null)
            {
                Width = _column.ActualWidth;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}