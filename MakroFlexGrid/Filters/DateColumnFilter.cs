namespace MakroFlexGrid.Filters
{
    /// <summary>
    /// Фильтр для дат.
    /// </summary>
    public class DateColumnFilter : ColumnFilterBase
    {
        private DateTime? _fromDate;
        private DateTime? _toDate;

        public DateTime? FromDate
        {
            get => _fromDate;
            set
            {
                if (_fromDate != value)
                {
                    _fromDate = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime? ToDate
        {
            get => _toDate;
            set
            {
                if (_toDate != value)
                {
                    _toDate = value;
                    OnPropertyChanged();
                }
            }
        }

        public override void Clear()
        {
            FromDate = null;
            ToDate = null;
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

            if (FromDate.HasValue)
            {
                fromCheck = dateValue >= FromDate.Value;
            }

            if (ToDate.HasValue)
            {
                toCheck = dateValue <= ToDate.Value;
            }

            return fromCheck && toCheck;
        }
    }
}
