using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MakroFlexGrid.Headers
{
    /// <summary>
    /// Листовая колонка для отображения текста в виде гиперссылки.
    /// При клике открывает URL или выполняет команду.
    /// Параметры автоматически копируются в CellViewModel.Config через ApplyHeaderConfig().
    /// </summary>
    [CellTemplate("UnifiedCellTemplate")]
    public class HyperlinkColumnHeaderLeaf : ColumnHeaderLeaf
    {
        static HyperlinkColumnHeaderLeaf()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(HyperlinkColumnHeaderLeaf),
                new FrameworkPropertyMetadata(typeof(HyperlinkColumnHeaderLeaf)));
        }

        #region Dependency Properties

        /// <summary>
        /// Команда, выполняемая при клике на ссылку.
        /// </summary>
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(
                "Command",
                typeof(ICommand),
                typeof(HyperlinkColumnHeaderLeaf),
                new FrameworkPropertyMetadata(null));

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        /// <summary>
        /// Параметр команды.
        /// </summary>
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register(
                "CommandParameter",
                typeof(object),
                typeof(HyperlinkColumnHeaderLeaf),
                new FrameworkPropertyMetadata(null));

        public object CommandParameter
        {
            get => (object)GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        /// <summary>
        /// Путь к свойству в Item, содержащему URL для открытия.
        /// Если задан, URL будет извлекаться из данных строки.
        /// </summary>
        public static readonly DependencyProperty UrlBindingProperty =
            DependencyProperty.Register(
                "UrlBinding",
                typeof(string),
                typeof(HyperlinkColumnHeaderLeaf),
                new FrameworkPropertyMetadata(null));

        public string UrlBinding
        {
            get => (string)GetValue(UrlBindingProperty);
            set => SetValue(UrlBindingProperty, value);
        }

        #endregion
    }
}