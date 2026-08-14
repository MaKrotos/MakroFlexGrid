using System;
using System.Collections.Concurrent;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MakroFlexGrid.Headers
{
    /// <summary>
    /// Листовая колонка — конечный элемент заголовка, не содержащий дочерних элементов.
    /// Представляет конкретную колонку DataGrid.
    /// </summary>
    public class ColumnHeaderLeaf : ColumnHeaderItem
    {
        /// <summary>
        /// Кэш загруженных ResourceDictionary по SourcePath.
        /// Позволяет не загружать один и тот же словарь несколько раз.
        /// </summary>
        private static readonly ConcurrentDictionary<string, ResourceDictionary> _templateCache
            = new ConcurrentDictionary<string, ResourceDictionary>();

        static ColumnHeaderLeaf()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ColumnHeaderLeaf),
                new FrameworkPropertyMetadata(typeof(ColumnHeaderLeaf)));
        }

        public ColumnHeaderLeaf()
        {
            // Если CellTemplate уже установлен (например, в конструкторе наследника),
            // не перезаписываем его.
            if (CellTemplate != null)
                return;

            // Пытаемся загрузить шаблон через атрибут [CellTemplate]
            var template = LoadCellTemplateFromAttribute();
            if (template != null)
            {
                CellTemplate = template;
                return;
            }

            // Fallback: стандартный шаблон с TextBlock
            CellTemplate = CreateDefaultTextBlockTemplate();
        }

        /// <summary>
        /// Загружает DataTemplate из атрибута [CellTemplate], если он задан.
        /// </summary>
        private DataTemplate LoadCellTemplateFromAttribute()
        {
            var attr = (CellTemplateAttribute)Attribute.GetCustomAttribute(
                GetType(), typeof(CellTemplateAttribute));

            if (attr == null)
                return null;

            try
            {
                var dictionary = _templateCache.GetOrAdd(attr.SourcePath, path =>
                {
                    return new ResourceDictionary
                    {
                        Source = new Uri(path, UriKind.Absolute)
                    };
                });

                return dictionary[attr.ResourceKey] as DataTemplate;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[ColumnHeaderLeaf] Failed to load template '{attr.ResourceKey}' from '{attr.SourcePath}': {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Создаёт стандартный шаблон с TextBlock для DefaultCellTemplate.
        /// </summary>
        private static DataTemplate CreateDefaultTextBlockTemplate()
        {
            var factory = new FrameworkElementFactory(typeof(TextBlock));

            factory.SetBinding(TextBlock.TextProperty, new Binding("Value"));
            factory.SetValue(TextBlock.TextWrappingProperty, TextWrapping.Wrap);
            factory.SetValue(TextBlock.TextTrimmingProperty, TextTrimming.None);
            factory.SetValue(TextBlock.PaddingProperty, new Thickness(8, 8, 8, 8));
            factory.SetValue(VerticalAlignmentProperty, VerticalAlignment.Center);

            return new DataTemplate
            {
                VisualTree = factory,
            };
        }
    }
}
