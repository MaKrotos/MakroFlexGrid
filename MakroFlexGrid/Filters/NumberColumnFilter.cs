namespace MakroFlexGrid.Filters
{
    /// <summary>
    /// Фильтр для числовых данных.
    /// </summary>
    public class NumberColumnFilter : ColumnFilterBase
    {
        private double? _fromValue;
        private double? _toValue;
        private FilterOperator _textOperator = FilterOperator.Contains; // Used for 'In' operator
        private HashSet<object> _selectedValues = new HashSet<object>();

        public double? FromValue
        {
            get => _fromValue;
            set
            {
                if (_fromValue != value)
                {
                    _fromValue = value;
                    OnPropertyChanged();
                }
            }
        }

        public double? ToValue
        {
            get => _toValue;
            set
            {
                if (_toValue != value)
                {
                    _toValue = value;
                    OnPropertyChanged();
                }
            }
        }

        public FilterOperator TextOperator
        {
            get => _textOperator;
            set
            {
                if (_textOperator != value)
                {
                    _textOperator = value;
                    OnPropertyChanged();
                }
            }
        }

        public HashSet<object> SelectedValues
        {
            get => _selectedValues;
            set
            {
                if (_selectedValues != value)
                {
                    _selectedValues = value;
                    OnPropertyChanged();
                }
            }
        }

        public override void Clear()
        {
            FromValue = null;
            ToValue = null;
            SelectedValues.Clear();
            Deactivate();
        }

        public override bool Passes(object value)
        {
            if (!IsActive)
                return true;

            if (value == null)
                return false;

            double numValue;
            if (value is double d)
                numValue = d;
            else if (value is int i)
                numValue = i;
            else if (value is long l)
                numValue = l;
            else if (value is float f)
                numValue = f;
            else if (value is decimal m)
                numValue = (double)m;
            else if (value is short s)
                numValue = s;
            else if (value is byte b)
                numValue = b;
            else if (value is string str && double.TryParse(str, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double parsed))
                numValue = parsed;
            else
                return false;

            if (TextOperator == FilterOperator.In && SelectedValues.Count > 0)
            {
                return SelectedValues.Contains(numValue);
            }

            bool fromCheck = true;
            bool toCheck = true;

            if (FromValue.HasValue)
            {
                fromCheck = numValue >= FromValue.Value;
            }

            if (ToValue.HasValue)
            {
                toCheck = numValue <= ToValue.Value;
            }

            return fromCheck && toCheck;
        }
    }
}
