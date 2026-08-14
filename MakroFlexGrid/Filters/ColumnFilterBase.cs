using MakroFlexGrid.Sorting;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MakroFlexGrid.Filters
{
    /// <summary>
    /// Базовый абстрактный класс для фильтрации колонки.
    /// </summary>
    public abstract class ColumnFilterBase : INotifyPropertyChanged
    {
        #region Private Variables

        private string _sortMemberPath;
        private SortDataType _dataType;
        private bool _isActive;

        #endregion

        #region Public Properties

        /// <summary>
        /// Путь к свойству объекта данных, по которому применяется фильтр.
        /// </summary>
        public string SortMemberPath
        {
            get => _sortMemberPath;
            set
            {
                if (_sortMemberPath != value)
                {
                    _sortMemberPath = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Тип данных колонки.
        /// </summary>
        public SortDataType DataType
        {
            get => _dataType;
            set
            {
                if (_dataType != value)
                {
                    _dataType = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// True, если фильтр активен.
        /// </summary>
        public bool IsActive
        {
            get => _isActive;
            protected set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        #region Public Methods

        public virtual void Activate()
        {
            IsActive = true;
        }

        public virtual void Deactivate()
        {
            IsActive = false;
        }

        /// <summary>
        /// Полностью сбрасывает все условия фильтра.
        /// </summary>
        public abstract void Clear();

        /// <summary>
        /// Проверяет, проходит ли указанное значение через фильтр.
        /// </summary>
        /// <param name="value">Значение для проверки.</param>
        /// <returns>True, если значение проходит фильтр.</returns>
        public abstract bool Passes(object value);

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
