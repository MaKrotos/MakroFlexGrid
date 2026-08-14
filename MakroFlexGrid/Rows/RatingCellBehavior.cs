using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MakroFlexGrid.Rows
{
    /// <summary>
    /// Attached behavior для ячейки рейтинга (RatingCell).
    /// Генерирует список звёзд на основе MaxRating и обрабатывает клики.
    /// Регистрирует обработчики через CellBehaviorBase.
    /// </summary>
    public static class RatingCellBehavior
    {
        /// <summary>
        /// Статический конструктор — регистрирует обработчики через CellBehaviorBase.
        /// </summary>
        static RatingCellBehavior()
        {
            CellBehaviorBase.RegisterSetupHandler<ItemsControl>(OnRatingItemsControlSetup);
        }

        private static void OnRatingItemsControlSetup(ItemsControl itemsControl)
        {
            var dataContext = itemsControl.DataContext;
            if (dataContext == null) return;

            var dataContextType = dataContext.GetType();

            // Получаем MaxRating из Config
            int maxRating = 5;
            var configProp = dataContextType.GetProperty("Config");
            if (configProp != null)
            {
                var config = configProp.GetValue(dataContext) as System.Collections.Generic.Dictionary<string, object>;
                if (config != null && config.TryGetValue("MaxRating", out var maxRatingObj))
                {
                    maxRating = (int)maxRatingObj;
                }
            }

            // Генерируем список звёзд
            var ratingItems = new ObservableCollection<int>();
            for (int i = 1; i <= maxRating; i++)
            {
                ratingItems.Add(i);
            }

            // Устанавливаем ItemsSource
            itemsControl.ItemsSource = ratingItems;

            // Создаём команду для клика по звезде
            var ratingCommand = new RelayCommand(parameter =>
            {
                if (parameter is int rating && dataContext != null)
                {
                    // Устанавливаем значение через EditValue
                    var editValueProp = dataContextType.GetProperty("EditValue");
                    if (editValueProp != null && editValueProp.CanWrite)
                    {
                        editValueProp.SetValue(dataContext, rating.ToString());

                        // Сохраняем
                        var commitMethod = dataContextType.GetMethod("CommitEdit");
                        commitMethod?.Invoke(dataContext, null);
                    }
                }
            });

            // Сохраняем команду в Config
            if (configProp != null)
            {
                var config = configProp.GetValue(dataContext) as System.Collections.Generic.Dictionary<string, object>;
                if (config != null)
                {
                    config["RatingCommand"] = ratingCommand;
                }
            }
        }
    }

    /// <summary>
    /// Простая реализация ICommand для использования внутри behavior.
    /// </summary>
    internal class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;

        public RelayCommand(Action<object> execute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter) => _execute(parameter);
    }
}