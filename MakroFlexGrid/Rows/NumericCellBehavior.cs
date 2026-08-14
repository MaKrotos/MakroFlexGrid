using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MakroFlexGrid.Rows
{
    /// <summary>
    /// Attached behavior для числовой ячейки (NumericCell).
    /// Обрабатывает ввод только цифр, точки и минуса.
    /// Форматирует число при потере фокуса.
    /// Регистрирует обработчики через CellBehaviorBase.
    /// </summary>
    public static class NumericCellBehavior
    {
        /// <summary>
        /// Статический конструктор — регистрирует обработчики через CellBehaviorBase.
        /// </summary>
        static NumericCellBehavior()
        {
            CellBehaviorBase.RegisterSetupHandler<TextBox>(OnNumericTextBoxSetup);
        }

        private static void OnNumericTextBoxSetup(TextBox textBox)
        {
            // Подписываемся на события
            textBox.PreviewTextInput += OnNumericPreviewTextInput;
            textBox.PreviewKeyDown += OnNumericPreviewKeyDown;
            textBox.LostFocus += OnNumericLostFocus;
            textBox.KeyDown += OnNumericKeyDown;

            // Отписываемся при выгрузке
            textBox.Unloaded += (s, e) =>
            {
                textBox.PreviewTextInput -= OnNumericPreviewTextInput;
                textBox.PreviewKeyDown -= OnNumericPreviewKeyDown;
                textBox.LostFocus -= OnNumericLostFocus;
                textBox.KeyDown -= OnNumericKeyDown;
            };
        }

        /// <summary>
        /// Разрешаем ввод только цифр, точки и минуса.
        /// </summary>
        private static void OnNumericPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;

            // Проверяем AllowNegative из Config
            bool allowNegative = true;
            if (textBox.DataContext is CellViewModel cellVm)
            {
                allowNegative = cellVm.GetConfig("AllowNegative", true);
            }

            foreach (char c in e.Text)
            {
                if (!char.IsDigit(c) && c != '.' && c != ',')
                {
                    if (c == '-' && allowNegative)
                    {
                        // Минус разрешён только в начале и если его ещё нет
                        if (textBox.SelectionStart > 0 || textBox.Text.Contains("-"))
                        {
                            e.Handled = true;
                            return;
                        }
                        continue;
                    }
                    e.Handled = true;
                    return;
                }
            }

            // Точка/запятая — только одна
            if ((e.Text.Contains(".") || e.Text.Contains(",")) && textBox.Text.Contains(".") || textBox.Text.Contains(","))
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Разрешаем навигационные клавиши (Backspace, Delete, стрелки).
        /// </summary>
        private static void OnNumericPreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Разрешаем навигационные клавиши
            if (e.Key == Key.Back || e.Key == Key.Delete || e.Key == Key.Tab ||
                e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Home || e.Key == Key.End)
            {
                return;
            }

            // Блокируем всё, кроме цифр
            if (e.Key < Key.D0 || e.Key > Key.D9)
            {
                if (e.Key < Key.NumPad0 || e.Key > Key.NumPad9)
                {
                    // Разрешаем Enter и Escape (обрабатываются в EditableCellBehavior)
                    if (e.Key != Key.Enter && e.Key != Key.Escape)
                    {
                        e.Handled = true;
                    }
                }
            }
        }

        /// <summary>
        /// При потере фокуса форматируем число.
        /// </summary>
        private static void OnNumericLostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && !string.IsNullOrEmpty(textBox.Text))
            {
                // Пытаемся распарсить и отформатировать
                if (decimal.TryParse(textBox.Text,
                    NumberStyles.Any,
                    CultureInfo.CurrentCulture,
                    out decimal value))
                {
                    // Получаем параметры из Config
                    int decimalPlaces = 2;
                    string format = null;
                    string currencySymbol = null;

                    if (textBox.DataContext is CellViewModel cellVm)
                    {
                        decimalPlaces = cellVm.GetConfig("DecimalPlaces", 2);
                        format = cellVm.GetConfig<string>("Format");
                        currencySymbol = cellVm.GetConfig<string>("CurrencySymbol");
                    }

                    string formattedValue;
                    if (!string.IsNullOrEmpty(format))
                    {
                        formattedValue = value.ToString(format);
                    }
                    else
                    {
                        formattedValue = value.ToString($"F{decimalPlaces}");
                    }

                    // Добавляем символ валюты, если задан
                    if (!string.IsNullOrEmpty(currencySymbol))
                    {
                        formattedValue = currencySymbol + " " + formattedValue;
                    }

                    textBox.Text = formattedValue;
                }
            }
        }

        /// <summary>
        /// Enter — сохраняем, Escape — отменяем.
        /// </summary>
        private static void OnNumericKeyDown(object sender, KeyEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                var dataContext = textBox.DataContext;
                if (dataContext == null) return;

                if (e.Key == Key.Enter)
                {
                    // Форматируем перед сохранением
                    OnNumericLostFocus(sender, null);

                    // Вызываем CommitEdit через рефлексию
                    var commitMethod = dataContext.GetType().GetMethod("CommitEdit");
                    commitMethod?.Invoke(dataContext, null);
                    e.Handled = true;
                }
                else if (e.Key == Key.Escape)
                {
                    var cancelMethod = dataContext.GetType().GetMethod("CancelEdit");
                    cancelMethod?.Invoke(dataContext, null);
                    e.Handled = true;
                }
            }
        }
    }
}