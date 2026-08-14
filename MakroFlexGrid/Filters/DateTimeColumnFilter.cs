namespace MakroFlexGrid.Filters
{
    /// <summary>
    /// Фильтр для даты и времени.
    /// </summary>
    public class DateTimeColumnFilter : ColumnFilterBase
    {
        private DateTime? _fromDateTime;
        private DateTime? _toDateTime;

        public DateTime? FromDateTime
        {
            get => _fromDateTime;
            set
            {
                if (_fromDateTime != value)
                {
                    _fromDateTime = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime? ToDateTime
        {
            get => _toDateTime;
            set
            {
                if (_toDateTime != value)
                {
                    _toDateTime = value;
                    OnPropertyChanged();
                }
            }
        }

        public override void Clear()
        {
            FromDateTime = null;
            ToDateTime = null;
            Deactivate();
        }

        public override bool Passes(object value)
        {
            if (!IsActive)
                return true;

            if (value == null)
                return false;

            DateTime dateValue;
            if (value is DateTime dt)
                dateValue = dt;
            else if (value is DateTimeOffset dto)
                dateValue = dto.DateTime;
            else if (value is string str && DateTime.TryParse(str,
                System.Globalization.CultureInfo.CurrentCulture,
                System.Globalization.DateTimeStyles.None, out DateTime parsed))
                dateValue = parsed;
            else
                return false;

            bool fromCheck = true;
            bool toCheck = true;

            if (FromDateTime.HasValue)
            {
                fromCheck = dateValue >= FromDateTime.Value;
            }

            if (ToDateTime.HasValue)
            {
                toCheck = dateValue <= ToDateTime.Value;
            }

            return fromCheck && toCheck;
        }
    }
}
