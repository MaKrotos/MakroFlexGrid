using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace MakroFlexGrid.Headers
{
    /// <summary>
    /// Adorner для отображения стилизованной полупрозрачной копии перетаскиваемого заголовка колонки.
    /// Следует за курсором мыши во время Drag & Drop.
    /// Поддерживает тень, скруглённые углы, рамку и настраиваемый фон.
    /// </summary>
    internal sealed class ColumnHeaderDragAdorner : Adorner
    {
        private readonly UIElement _child;
        private double _left;
        private double _top;

        /// <summary>
        /// Создаёт Adorner со стилизованной полупрозрачной копией визуального элемента.
        /// </summary>
        /// <param name="adornedElement">Элемент, к которому прикрепляется Adorner (обычно Grid-обёртка заголовков).</param>
        /// <param name="dragVisual">Визуальный элемент для отображения (копия заголовка).</param>
        /// <param name="opacity">Прозрачность копии (по умолчанию 0.85).</param>
        /// <param name="cornerRadius">Радиус скругления углов (по умолчанию 4).</param>
        /// <param name="borderBrush">Кисть рамки (по умолчанию полупрозрачный синий #991E90FF).</param>
        /// <param name="borderThickness">Толщина рамки (по умолчанию 1.5).</param>
        /// <param name="shadowDepth">Глубина тени (по умолчанию 5).</param>
        public ColumnHeaderDragAdorner(
            UIElement adornedElement,
            UIElement dragVisual,
            double opacity = 0.85,
            double cornerRadius = 4,
            Brush borderBrush = null,
            double borderThickness = 1.5,
            double shadowDepth = 5)
            : base(adornedElement)
        {
            // Оборачиваем dragVisual в Border с рамкой, скруглением и тенью
            var border = new Border
            {
                Child = dragVisual,
                Background = Brushes.White,
                BorderBrush = borderBrush ?? new SolidColorBrush(Color.FromArgb(0x99, 0x1E, 0x90, 0xFF)),
                BorderThickness = new Thickness(borderThickness),
                CornerRadius = new CornerRadius(cornerRadius),
                Effect = new DropShadowEffect
                {
                    BlurRadius = 8,
                    Opacity = 0.35,
                    ShadowDepth = shadowDepth,
                    Color = Colors.Black,
                    RenderingBias = RenderingBias.Performance
                }
            };

            _child = new ContentPresenter
            {
                Content = border,
                Opacity = opacity,
                IsHitTestVisible = false
            };

            IsHitTestVisible = false;
        }

        /// <summary>
        /// Обновляет позицию Adorner-а относительно adornedElement.
        /// </summary>
        public void SetPosition(double left, double top)
        {
            _left = left;
            _top = top;
            InvalidateArrange();
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            _child.Arrange(new Rect(_left, _top, _child.DesiredSize.Width, _child.DesiredSize.Height));
            return finalSize;
        }

        protected override Visual GetVisualChild(int index) => _child;

        protected override int VisualChildrenCount => 1;

        protected override Size MeasureOverride(Size constraint)
        {
            _child.Measure(constraint);
            return _child.DesiredSize;
        }
    }
}