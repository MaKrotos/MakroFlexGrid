using MakroFlexGrid.Headers;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace MakroFlexGrid.Rows
{
    /// <summary>
    /// Фабрика для создания CellViewModel.
    /// Позволяет зарегистрировать фабричный метод для конкретного типа ColumnHeaderLeaf,
    /// чтобы создавать специализированные CellViewModel без модификации RowViewModel.
    /// </summary>
    public static class CellViewModelFactory
    {
        private static readonly Dictionary<Type, Func<RowViewModel, DataGridColumn, CellViewModel>> _factories
            = new Dictionary<Type, Func<RowViewModel, DataGridColumn, CellViewModel>>();

        /// <summary>
        /// Регистрирует фабричный метод для указанного типа заголовка колонки.
        /// </summary>
        /// <typeparam name="THeader">Тип ColumnHeaderLeaf.</typeparam>
        /// <param name="factory">Фабричный метод, создающий CellViewModel.</param>
        public static void Register<THeader>(Func<RowViewModel, DataGridColumn, CellViewModel> factory)
            where THeader : ColumnHeaderLeaf
        {
            _factories[typeof(THeader)] = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        /// <summary>
        /// Создаёт CellViewModel для указанной колонки.
        /// Если для типа заголовка зарегистрирован фабричный метод, использует его.
        /// Иначе создаёт стандартный CellViewModel.
        /// </summary>
        /// <param name="rowViewModel">RowViewModel строки.</param>
        /// <param name="column">Колонка DataGrid.</param>
        /// <param name="headerItem">ColumnHeaderItem для определения типа.</param>
        /// <returns>Созданный CellViewModel.</returns>
        public static CellViewModel Create(RowViewModel rowViewModel, DataGridColumn column, ColumnHeaderItem headerItem)
        {
            if (headerItem != null && _factories.TryGetValue(headerItem.GetType(), out var factory))
            {
                return factory(rowViewModel, column);
            }

            return new CellViewModel(rowViewModel, column);
        }
    }
}