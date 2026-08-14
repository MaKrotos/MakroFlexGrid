namespace MakroFlexGrid.Headers
{
    /// <summary>
    /// Тип агрегации (итогового значения) для столбца.
    /// </summary>
    public enum AggregateType
    {
        /// <summary>
        /// Без агрегации.
        /// </summary>
        None,

        /// <summary>
        /// Сумма значений.
        /// </summary>
        Sum,

        /// <summary>
        /// Среднее арифметическое значений.
        /// </summary>
        Average,

        /// <summary>
        /// Количество значений.
        /// </summary>
        Count,

        /// <summary>
        /// Минимальное значение.
        /// </summary>
        Min,

        /// <summary>
        /// Максимальное значение.
        /// </summary>
        Max
    }
}