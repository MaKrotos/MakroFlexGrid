# MakroFlexGrid — Фильтрация, сортировка и агрегаты

В этом документе описаны операции с данными, поддерживаемые гридом: фильтрация, сортировка и автоматический расчет агрегатов в нижней панели.

## 🕸 Фильтрация

### API `FilterService`

Сервис доступен через свойство `CustomDataGrid.FilterService`:

| Метод | Описание |
| :--- | :--- |
| `SetFilter(headerItem, filter)` | Устанавливает фильтр для колонки. |
| `ClearFilter(headerItem)` | Сбрасывает фильтр колонки. |
| `ClearAllFilters()` | Сбрасывает все фильтры. |
| `GetFilter(headerItem)` | Возвращает активный фильтр колонки (или `null`). |
| `GetUniqueValues(headerItem)` | Уникальные значения колонки из `ItemsSource`. |
| `ApplyFilters()` | Применяет все активные фильтры к `ICollectionView`. |
| `HasActiveFilters` | `true`, если есть хотя бы один активный фильтр. |
| `ActiveFilterCount` | Количество активных фильтров. |

Событие `FilterChanged` уведомляет об изменении фильтра.

### Операторы (`FilterOperator`)

`None`, `Equals`, `NotEquals`, `Contains`, `StartsWith`, `EndsWith`, `GreaterThan`, `LessThan`, `GreaterThanOrEqual`, `LessThanOrEqual`, `Between`, `In`.

### Базовый класс `ColumnFilterBase`

Абстрактный класс для создания собственных фильтров:

| Член | Описание |
| :--- | :--- |
| `SortMemberPath` | Путь к свойству объекта данных. |
| `DataType` | Тип данных (`SortDataType`). |
| `IsActive` | Активен ли фильтр. |
| `Activate()` / `Deactivate()` | Активация/деактивация. |
| `Clear()` | Полный сброс условий (абстрактный). |
| `Passes(value)` | Проверка, проходит ли значение фильтр (абстрактный). |

### Выбор UI фильтра (`FilterUIFactory`)

`FilterUIFactory.CreateFilterControl(headerItem, filterService)` выбирает элемент управления по `SortDataType`:
`Text` → текстовый фильтр, `Number` → диапазон, `Date`/`DateTime` → диапазон дат.

Для создания собственного фильтра: унаследуйте `ColumnFilterBase`, реализуйте `Clear()` и `Passes()`, затем зарегистрируйте UI-контрол в `FilterUIFactory`.

## 🔀 Сортировка

Свойства колонки: `CanUserSort`, `SortMemberPath`, `SortDataType`, `SortDirection`.

Типы сортировки (`SortDataType`): `Text`, `Number`, `Date`, `DateTime`, `Boolean`.

Для нетекстовых типов используются оптимизированные типизированные компараторы (с кешированием), что обеспечивает корректную сортировку чисел, дат и логических значений.

## 📊 Агрегаты (нижняя панель)

Типы агрегатов (`AggregateType`): `None`, `Sum`, `Average`, `Count`, `Min`, `Max`.

Задайте `AggregateType` на колонке, чтобы рассчитать итог в нижней панели:

```xml
<headers:ColumnHeaderLeaf Header="Сумма" SortMemberPath="Total" AggregateType="Sum" Width="120" />
```

Для ручного пересчёта итогов вызывайте `RefreshAggregates()`.

Для оформления ячейки итога используйте `BottomCellTemplate` (DataContext — `BottomCellViewModel`, свойство `Value` содержит результат):

```xml
<DataTemplate x:Key="SumCellTemplate">
    <TextBlock Text="{Binding Value}" FontWeight="Bold" HorizontalAlignment="Center" />
</DataTemplate>

<headers:ColumnHeaderLeaf Header="Сумма" AggregateType="Sum"
    BottomCellTemplate="{StaticResource SumCellTemplate}" Width="120" />
```

## ➡️ Следующие шаги

- [Обзор и быстрый старт](overview.md)
- [События, режимы выбора и справочник свойств](reference.md)
- [Кастомные ячейки, поведения и примеры кастомизации](customization.md)