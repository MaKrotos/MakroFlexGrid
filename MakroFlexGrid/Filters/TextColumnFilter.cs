namespace MakroFlexGrid.Filters
{
    /// <summary>
    /// Фильтр для текстовых данных.
    /// </summary>
    public class TextColumnFilter : ColumnFilterBase
    {
        private string _textValue;
        private FilterOperator _textOperator = FilterOperator.Contains;
        private HashSet<object> _selectedValues = new HashSet<object>();
        private List<object> _allUniqueValues = new List<object>();

        public string TextValue
        {
            get => _textValue;
            set
            {
                if (_textValue != value)
                {
                    _textValue = value;
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

        public List<object> AllUniqueValues
        {
            get => _allUniqueValues;
            set
            {
                if (_allUniqueValues != value)
                {
                    _allUniqueValues = value;
                    OnPropertyChanged();
                }
            }
        }

        public override void Clear()
        {
            TextValue = null;
            TextOperator = FilterOperator.Contains;
            SelectedValues.Clear();
            Deactivate();
        }

        public override bool Passes(object value)
        {
            if (!IsActive)
                return true;

            if (value == null)
                return false;

            string strValue = value.ToString();
            if (string.IsNullOrEmpty(strValue))
                return false;

            if (TextOperator == FilterOperator.In && SelectedValues.Count > 0)
            {
                return SelectedValues.Contains(value) || SelectedValues.Contains(strValue);
            }

            if (string.IsNullOrEmpty(TextValue))
                return true;

            switch (TextOperator)
            {
                case FilterOperator.Equals:
                    return string.Equals(strValue, TextValue, StringComparison.CurrentCultureIgnoreCase);
                case FilterOperator.NotEquals:
                    return !string.Equals(strValue, TextValue, StringComparison.CurrentCultureIgnoreCase);
                case FilterOperator.Contains:
                    return strValue.IndexOf(TextValue, StringComparison.CurrentCultureIgnoreCase) >= 0;
                case FilterOperator.StartsWith:
                    return strValue.StartsWith(TextValue, StringComparison.CurrentCultureIgnoreCase);
                case FilterOperator.EndsWith:
                    return strValue.EndsWith(TextValue, StringComparison.CurrentCultureIgnoreCase);
                default:
                    return true;
            }
        }
    }
}
