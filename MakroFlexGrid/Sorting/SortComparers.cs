using System.Collections;
using System.ComponentModel;
using System.Globalization;

namespace MakroFlexGrid.Sorting
{
    /// <summary>
    /// Фабрика компараторов для сортировки по типам данных с кешированием.
    /// </summary>
    internal static class SortComparerFactory
    {
        private static readonly Dictionary<(SortDataType, ListSortDirection), IComparer> _cache = new();
        private static readonly object _lock = new();

        public static IComparer GetComparer(SortDataType dataType, ListSortDirection direction)
        {
            var key = (dataType, direction);

            // Быстрая проверка без блокировки
            if (_cache.TryGetValue(key, out var cached))
                return cached;

            lock (_lock)
            {
                // Двойная проверка внутри блокировки
                if (_cache.TryGetValue(key, out var doubleChecked))
                    return doubleChecked;

                var comparer = CreateComparer(dataType, direction);
                _cache[key] = comparer;
                return comparer;
            }
        }

        private static IComparer CreateComparer(SortDataType dataType, ListSortDirection direction)
        {
            IComparer<object> baseComparer = dataType switch
            {
                SortDataType.Text => StringComparerAdapter.Instance,
                SortDataType.Number => NumberSorters.NumberComparer.Instance,
                SortDataType.Date => NumberSorters.DateComparer.Instance,
                SortDataType.DateTime => NumberSorters.DateComparer.Instance,
                SortDataType.Boolean => NumberSorters.BooleanComparer.Instance,
                _ => throw new ArgumentOutOfRangeException(nameof(dataType), dataType, null)
            };

            return new DirectionalComparer(baseComparer, direction);
        }

        /// <summary>
        /// Очистка кеша (вызывать при смене культуры или для освобождения памяти).
        /// </summary>
        public static void ClearCache()
        {
            lock (_lock)
            {
                _cache.Clear();
            }
        }
    }

    /// <summary>
    /// Универсальный компаратор, который добавляет поддержку направления сортировки.
    /// </summary>
    internal sealed class DirectionalComparer : IComparer
    {
        private readonly IComparer<object> _comparer;
        private readonly int _directionMultiplier;

        public DirectionalComparer(IComparer<object> comparer, ListSortDirection direction)
        {
            _comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
            _directionMultiplier = direction == ListSortDirection.Ascending ? 1 : -1;
        }

        public int Compare(object x, object y)
        {
            // Быстрая проверка на идентичность и null
            if (ReferenceEquals(x, y)) return 0;
            if (x is null) return -1 * _directionMultiplier;
            if (y is null) return 1 * _directionMultiplier;

            return _comparer.Compare(x, y) * _directionMultiplier;
        }
    }

    /// <summary>
    /// Вспомогательные классы для приведения типов перед сравнением.
    /// </summary>
    internal static class NumberSorters
    {
        /// <summary>
        /// Оптимизированный компаратор чисел с прямыми проверками типов.
        /// </summary>
        public sealed class NumberComparer : IComparer<object>
        {
            public static readonly NumberComparer Instance = new();

            private NumberComparer() { }

            public int Compare(object x, object y)
            {
                // Быстрая проверка на идентичность
                if (ReferenceEquals(x, y)) return 0;

                double? numX = TryParseNumberFast(x);
                double? numY = TryParseNumberFast(y);

                if (numX.HasValue && numY.HasValue)
                    return numX.Value.CompareTo(numY.Value);

                // Если оба не распарсились как числа - сравниваем как строки
                if (!numX.HasValue && !numY.HasValue)
                {
                    string strX = x?.ToString() ?? string.Empty;
                    string strY = y?.ToString() ?? string.Empty;
                    return string.Compare(strX, strY, StringComparison.CurrentCulture);
                }

                // Числа всегда меньше не-чисел при сортировке
                return numX.HasValue ? -1 : 1;
            }

            private static double? TryParseNumberFast(object value)
            {
                if (value is null) return null;

                // Прямые проверки типов для избежания boxing/unboxing и дорогих преобразований
                if (value is double d) return d;
                if (value is int i) return i;
                if (value is long l) return l;
                if (value is float f) return f;
                if (value is decimal m) return (double)m;
                if (value is short s) return s;
                if (value is byte b) return b;
                if (value is uint ui) return ui;
                if (value is ulong ul) return ul;
                if (value is ushort us) return us;
                if (value is sbyte sb) return sb;

                // Для строк и других типов, которые могут быть числами
                string str;

                if (value is string strValue)
                {
                    str = strValue;
                }
                else if (value is IConvertible convertible)
                {
                    // Попытка быстрого преобразования через TypeCode
                    try
                    {
                        return convertible.ToDouble(CultureInfo.InvariantCulture);
                    }
                    catch
                    {
                        return null;
                    }
                }
                else
                {
                    str = value.ToString();
                }

                if (string.IsNullOrEmpty(str)) return null;

                // Используем NumberStyles.Any для поддержки разных форматов
                if (double.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
                    return result;

                return null;
            }
        }

        /// <summary>
        /// Оптимизированный компаратор дат с прямыми проверками типов.
        /// </summary>
        public sealed class DateComparer : IComparer<object>
        {
            public static readonly DateComparer Instance = new();

            private DateComparer() { }

            public int Compare(object x, object y)
            {
                // Быстрая проверка на идентичность
                if (ReferenceEquals(x, y)) return 0;

                DateTime? dateX = TryParseDateFast(x);
                DateTime? dateY = TryParseDateFast(y);

                if (dateX.HasValue && dateY.HasValue)
                    return dateX.Value.CompareTo(dateY.Value);

                // Если оба не распарсились как даты - сравниваем как строки
                if (!dateX.HasValue && !dateY.HasValue)
                {
                    string strX = x?.ToString() ?? string.Empty;
                    string strY = y?.ToString() ?? string.Empty;
                    return string.Compare(strX, strY, StringComparison.CurrentCulture);
                }

                // Даты всегда меньше не-дат при сортировке
                return dateX.HasValue ? -1 : 1;
            }

            private static DateTime? TryParseDateFast(object value)
            {
                if (value is null) return null;

                // Прямые проверки для типов дат
                if (value is DateTime dateTime) return dateTime;
                if (value is DateTimeOffset dateTimeOffset) return dateTimeOffset.DateTime;

                // Для строк
                if (value is string str)
                {
                    if (string.IsNullOrEmpty(str)) return null;

                    if (DateTime.TryParse(str, CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime result))
                        return result;

                    return null;
                }

                // Для остальных типов
                string strValue = value.ToString();
                if (string.IsNullOrEmpty(strValue)) return null;

                if (DateTime.TryParse(strValue, CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime parsedResult))
                    return parsedResult;

                return null;
            }
        }

        /// <summary>
        /// Компаратор логических значений (false < true).
        /// </summary>
        public sealed class BooleanComparer : IComparer<object>
        {
            public static readonly BooleanComparer Instance = new();

            private BooleanComparer() { }

            public int Compare(object x, object y)
            {
                // Быстрая проверка на идентичность
                if (ReferenceEquals(x, y)) return 0;

                bool? boolX = TryParseBool(x);
                bool? boolY = TryParseBool(y);

                if (boolX.HasValue && boolY.HasValue)
                    return boolX.Value.CompareTo(boolY.Value);

                // Если оба не распарсились как bool - сравниваем как строки
                if (!boolX.HasValue && !boolY.HasValue)
                {
                    string strX = x?.ToString() ?? string.Empty;
                    string strY = y?.ToString() ?? string.Empty;
                    return string.Compare(strX, strY, StringComparison.CurrentCulture);
                }

                // bool всегда меньше не-bool при сортировке
                return boolX.HasValue ? -1 : 1;
            }

            private static bool? TryParseBool(object value)
            {
                if (value is null) return null;

                if (value is bool b) return b;

                if (value is string str)
                {
                    if (bool.TryParse(str, out bool parsed))
                        return parsed;

                    return null;
                }

                // Для остальных типов (например, byte 0/1) - попытка числового преобразования
                if (value is IConvertible convertible)
                {
                    try
                    {
                        int intValue = convertible.ToInt32(CultureInfo.InvariantCulture);
                        return intValue != 0;
                    }
                    catch
                    {
                        return null;
                    }
                }

                return null;
            }
        }
    }

    /// <summary>
    /// Адаптер для StringComparer, чтобы он соответствовал IComparer&lt;object&gt;.
    /// Использует синглтон для уменьшения аллокаций.
    /// </summary>
    internal sealed class StringComparerAdapter : IComparer<object>
    {
        public static readonly StringComparerAdapter Instance = new(StringComparer.CurrentCulture);

        private readonly StringComparer _comparer;

        public StringComparerAdapter(StringComparer comparer)
        {
            _comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
        }

        public int Compare(object x, object y)
        {
            // Быстрая проверка на идентичность и null
            if (ReferenceEquals(x, y)) return 0;

            string strX = x?.ToString() ?? string.Empty;
            string strY = y?.ToString() ?? string.Empty;

            return _comparer.Compare(strX, strY);
        }
    }
}