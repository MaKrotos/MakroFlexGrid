using System;

namespace MakroFlexGrid.Headers
{
    /// <summary>
    /// Атрибут для автоматической загрузки DataTemplate ячейки в ColumnHeaderLeaf.
    /// Позволяет избежать дублирования кода загрузки ResourceDictionary
    /// в конструкторе каждого наследника ColumnHeaderLeaf.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class CellTemplateAttribute : Attribute
    {
        /// <summary>
        /// Ключ ресурса в ResourceDictionary (x:Key).
        /// </summary>
        public string ResourceKey { get; }

        /// <summary>
        /// Путь к ResourceDictionary с шаблоном.
        /// Если не указан, используется "{ResourceKey}.xaml" в той же папке.
        /// </summary>
        public string SourcePath { get; }

        /// <summary>
        /// Создаёт атрибут с указанием ключа ресурса.
        /// SourcePath будет сформирован автоматически как "Themes/RowTemplates/{ResourceKey}.xaml".
        /// </summary>
        /// <param name="resourceKey">Ключ ресурса (x:Key) в ResourceDictionary.</param>
        public CellTemplateAttribute(string resourceKey)
        {
            if (string.IsNullOrWhiteSpace(resourceKey))
                throw new ArgumentException("ResourceKey cannot be null or empty", nameof(resourceKey));

            ResourceKey = resourceKey;
            SourcePath = $"pack://application:,,,/MakroFlexGrid;component/Themes/RowTemplates/{resourceKey}.xaml";
        }

        /// <summary>
        /// Создаёт атрибут с указанием ключа ресурса и пути к ResourceDictionary.
        /// </summary>
        /// <param name="resourceKey">Ключ ресурса (x:Key) в ResourceDictionary.</param>
        /// <param name="sourcePath">Путь к ResourceDictionary (pack URI).</param>
        public CellTemplateAttribute(string resourceKey, string sourcePath)
        {
            if (string.IsNullOrWhiteSpace(resourceKey))
                throw new ArgumentException("ResourceKey cannot be null or empty", nameof(resourceKey));
            if (string.IsNullOrWhiteSpace(sourcePath))
                throw new ArgumentException("SourcePath cannot be null or empty", nameof(sourcePath));

            ResourceKey = resourceKey;
            SourcePath = sourcePath;
        }
    }
}