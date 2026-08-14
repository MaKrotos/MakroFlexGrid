using System.Windows;
using System.Windows.Controls;

namespace MakroFlexGrid.Headers
{
    /// <summary>
    /// Листовая колонка с возможностью редактирования ячеек.
    /// При клике на ячейку TextBlock заменяется на TextBox для ввода значения.
    /// При потере фокуса или нажатии Enter значение сохраняется в модель данных.
    /// Шаблон загружается автоматически через атрибут [CellTemplate].
    /// </summary>
    [CellTemplate("EditableCellTemplate")]
    public class EditableColumnHeaderLeaf : ColumnHeaderLeaf
    {
        static EditableColumnHeaderLeaf()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(EditableColumnHeaderLeaf),
                new FrameworkPropertyMetadata(typeof(EditableColumnHeaderLeaf)));
        }

        // Конструктор не требуется — CellTemplate загружается автоматически
        // через ColumnHeaderLeaf.LoadCellTemplateFromAttribute()
    }
}