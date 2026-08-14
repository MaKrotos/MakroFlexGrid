using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MakroFlexGrid.Scroll
{
    /// <summary>
    /// Единственный источник истины (Single Source of Truth) для горизонтального offset.
    /// Все изменения горизонтального скролла проходят через этот класс.
    /// </summary>
    public class ScrollManager : INotifyPropertyChanged
    {
        private double _horizontalOffset;
        private double _maxHorizontalOffset;
        private readonly object _lockObject = new object();

        public double HorizontalOffset
        {
            get => _horizontalOffset;
            set
            {
                double newValue;
                lock (_lockObject)
                {
                    if (Math.Abs(_horizontalOffset - value) <= 0.0001) return;
                    _horizontalOffset = value;
                    newValue = value;
                }

                OnPropertyChanged();
                OnHorizontalOffsetChanged(newValue);
            }
        }

        public double MaxHorizontalOffset
        {
            get => _maxHorizontalOffset;
            set
            {
                bool wasCorrected = false;
                lock (_lockObject)
                {
                    if (Math.Abs(_maxHorizontalOffset - value) <= 0.0001) return;
                    _maxHorizontalOffset = value;

                    // Если текущий HorizontalOffset превышает новый максимум — корректируем
                    if (_horizontalOffset > _maxHorizontalOffset)
                    {
                        _horizontalOffset = _maxHorizontalOffset;
                        wasCorrected = true;
                    }
                }
                OnPropertyChanged();

                // Если была корректировка HorizontalOffset — уведомляем подписчиков
                // (уведомление вне блокировки, чтобы избежать deadlock)
                if (wasCorrected)
                {
                    OnHorizontalOffsetChanged(_horizontalOffset);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event Action<double> HorizontalOffsetChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnHorizontalOffsetChanged(double offset)
        {
            HorizontalOffsetChanged?.Invoke(offset);
        }

        public void Reset()
        {
            HorizontalOffset = 0;
            MaxHorizontalOffset = 0;
        }
    }
}
