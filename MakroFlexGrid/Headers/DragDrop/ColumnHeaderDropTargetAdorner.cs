using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace MakroFlexGrid.Headers
{
    /// <summary>
    /// Adorner для отображения стилизованного индикатора позиции вставки при Drag & Drop заголовков.
    /// Рисует вертикальную линию с точками на концах и полупрозрачной подсветкой,
    /// указывающую место вставки колонки.
    /// </summary>
    internal sealed class ColumnHeaderDropTargetAdorner : Adorner
    {
        private double _xPosition;
        private double _height;
        private readonly Pen _linePen;
        private readonly Pen _dotPen;
        private readonly Brush _highlightBrush;
        private readonly double _lineThickness;
        private readonly double _dotRadius;

        /// <summary>
        /// Создаёт Adorner со стилизованным индикатором вставки.
        /// </summary>
        /// <param name="adornedElement">Элемент, к которому прикрепляется Adorner.</param>
        /// <param name="lineColor">Цвет линии (по умолчанию #1E90FF — DodgerBlue).</param>
        /// <param name="lineThickness">Толщина линии (по умолчанию 3).</param>
        /// <param name="highlightColor">Цвет полупрозрачной подсветки (по умолчанию #1A1E90FF).</param>
        /// <param name="dotRadius">Радиус точек на концах линии (по умолчанию 4).</param>
        public ColumnHeaderDropTargetAdorner(
            UIElement adornedElement,
            Color? lineColor = null,
            double lineThickness = 3,
            Color? highlightColor = null,
            double dotRadius = 4)
            : base(adornedElement)
        {
            var color = lineColor ?? Color.FromRgb(0x1E, 0x90, 0xFF);
            _lineThickness = lineThickness;
            _dotRadius = dotRadius;

            // Основная линия
            _linePen = new Pen(new SolidColorBrush(color), lineThickness);
            _linePen.Freeze();

            // Кисть для точек на концах
            _dotPen = new Pen(new SolidColorBrush(color), 1);
            _dotPen.Freeze();

            // Полупрозрачная подсветка
            var hlColor = highlightColor ?? Color.FromArgb(0x1A, 0x1E, 0x90, 0xFF);
            _highlightBrush = new SolidColorBrush(hlColor);
            _highlightBrush.Freeze();

            IsHitTestVisible = false;
        }

        /// <summary>
        /// Обновляет позицию индикатора вставки.
        /// </summary>
        /// <param name="xPosition">X-координата линии относительно adornedElement.</param>
        /// <param name="height">Высота линии.</param>
        public void SetPosition(double xPosition, double height)
        {
            _xPosition = xPosition;
            _height = height;
            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (_height <= 0)
                return;

            double halfThickness = _lineThickness / 2;

            // Полупрозрачная подсветка слева от линии (ширина ~20px)
            var highlightRect = new Rect(
                _xPosition - 10,
                0,
                20,
                _height);
            drawingContext.DrawRectangle(_highlightBrush, null, highlightRect);

            // Основная вертикальная линия
            drawingContext.DrawLine(
                _linePen,
                new Point(_xPosition, _dotRadius),
                new Point(_xPosition, _height - _dotRadius));

            // Верхняя точка
            drawingContext.DrawEllipse(
                _linePen.Brush,
                _dotPen,
                new Point(_xPosition, _dotRadius),
                _dotRadius, _dotRadius);

            // Нижняя точка
            drawingContext.DrawEllipse(
                _linePen.Brush,
                _dotPen,
                new Point(_xPosition, _height - _dotRadius),
                _dotRadius, _dotRadius);
        }
    }
}