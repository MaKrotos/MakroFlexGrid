using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MakroFlexGrid.Headers
{
    /// <summary>
    /// Листовая колонка для отображения изображений.
    /// Поддерживает URI, ресурсы и byte[].
    /// Параметры автоматически копируются в CellViewModel.Config через ApplyHeaderConfig().
    /// </summary>
    [CellTemplate("UnifiedCellTemplate")]
    public class ImageColumnHeaderLeaf : ColumnHeaderLeaf
    {
        static ImageColumnHeaderLeaf()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ImageColumnHeaderLeaf),
                new FrameworkPropertyMetadata(typeof(ImageColumnHeaderLeaf)));
        }

        #region Dependency Properties

        /// <summary>
        /// Режим растяжения изображения. По умолчанию Uniform.
        /// </summary>
        public static readonly DependencyProperty StretchProperty =
            DependencyProperty.Register(
                "Stretch",
                typeof(Stretch),
                typeof(ImageColumnHeaderLeaf),
                new FrameworkPropertyMetadata(Stretch.Uniform));

        public Stretch Stretch
        {
            get => (Stretch)GetValue(StretchProperty);
            set => SetValue(StretchProperty, value);
        }

        /// <summary>
        /// Максимальная ширина изображения.
        /// </summary>
        public static readonly DependencyProperty MaxWidthProperty =
            DependencyProperty.Register(
                "MaxWidth",
                typeof(double),
                typeof(ImageColumnHeaderLeaf),
                new FrameworkPropertyMetadata(double.PositiveInfinity));

        public double MaxWidth
        {
            get => (double)GetValue(MaxWidthProperty);
            set => SetValue(MaxWidthProperty, value);
        }

        /// <summary>
        /// Максимальная высота изображения.
        /// </summary>
        public static readonly DependencyProperty MaxHeightProperty =
            DependencyProperty.Register(
                "MaxHeight",
                typeof(double),
                typeof(ImageColumnHeaderLeaf),
                new FrameworkPropertyMetadata(double.PositiveInfinity));

        public double MaxHeight
        {
            get => (double)GetValue(MaxHeightProperty);
            set => SetValue(MaxHeightProperty, value);
        }

        /// <summary>
        /// Изображение по умолчанию (показывается, когда основное изображение недоступно).
        /// </summary>
        public static readonly DependencyProperty DefaultImageProperty =
            DependencyProperty.Register(
                "DefaultImage",
                typeof(ImageSource),
                typeof(ImageColumnHeaderLeaf),
                new FrameworkPropertyMetadata(null));

        public ImageSource DefaultImage
        {
            get => (ImageSource)GetValue(DefaultImageProperty);
            set => SetValue(DefaultImageProperty, value);
        }

        #endregion
    }
}