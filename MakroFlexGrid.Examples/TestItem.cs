using System.Collections.ObjectModel;

namespace MakroFlexGrid.TestWindows
{
    /// <summary>
    /// Модель данных для демонстрации всех типов ячеек MakroFlexGrid.
    /// </summary>
    public class TestItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Value { get; set; }
        public string Status { get; set; }
        public string Category { get; set; }
        public DateTime Date { get; set; }
        public string LastColumn { get; set; }
        public bool IsActive { get; set; }

        // Новые свойства для демонстрации всех типов ячеек
        public decimal Price { get; set; }
        public bool IsChecked { get; set; }
        public DateTime? NullableDate { get; set; }
        public string Url { get; set; }
        public double Progress { get; set; }
        public string ImageUrl { get; set; }
        public int Rating { get; set; }
        public string Color { get; set; }
        public string MultiLineText { get; set; }
        public string RadioOption { get; set; }

        /// <summary>
        /// Статический список категорий для ComboBoxColumnHeaderLeaf.
        /// </summary>
        public static ObservableCollection<string> Categories { get; } =
            new ObservableCollection<string>
            {
                "Электроника",
                "Одежда",
                "Продукты",
                "Книги",
                "Игрушки",
                "Мебель",
                "Спорт",
                "Авто"
            };

        /// <summary>
        /// Статический список опций для RadioButtonCell.
        /// </summary>
        public static ObservableCollection<string> RadioOptions { get; } =
            new ObservableCollection<string>
            {
                "Вариант А",
                "Вариант Б",
                "Вариант В"
            };
    }
}