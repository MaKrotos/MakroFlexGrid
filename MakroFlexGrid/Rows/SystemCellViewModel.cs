namespace MakroFlexGrid.Rows
{
    /// <summary>
    /// Специальная ячейка для системной колонки.
    /// </summary>
    public class SystemCellViewModel : CellViewModel
    {
        public SystemCellViewModel(RowViewModel rowViewModel)
            : base(rowViewModel, null)
        {
            // Для системной ячейки нет привязанной DataGridColumn
            Width = 15;
        }

        // Свойство Value в базовом классе не virtual, поэтому мы не можем его переопределить.
        // Однако, так как CellViewModel использует геттер, который возвращает пустую строку,
        // если _valueGetter == null, нам ничего не нужно переопределять.

        /// <summary>
        /// Возвращает пустую строку, чтобы ContentControl не показывал
        /// полное имя класса, когда ContentTemplate не задан.
        /// </summary>
        public override string ToString() => "";
    }
}
