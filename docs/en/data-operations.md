# MakroFlexGrid — Filtering, Sorting and Aggregates

This document describes the data operations supported by the grid: filtering, sorting and automatic aggregate calculation in the bottom panel.

## 🕸 Filtering

### `FilterService` API

The service is available via the `CustomDataGrid.FilterService` property:

| Method | Description |
| :--- | :--- |
| `SetFilter(headerItem, filter)` | Sets the filter for a column. |
| `ClearFilter(headerItem)` | Resets the column filter. |
| `ClearAllFilters()` | Resets all filters. |
| `GetFilter(headerItem)` | Returns the active column filter (or `null`). |
| `GetUniqueValues(headerItem)` | Unique column values from `ItemsSource`. |
| `ApplyFilters()` | Applies all active filters to the `ICollectionView`. |
| `HasActiveFilters` | `true` if there is at least one active filter. |
| `ActiveFilterCount` | Number of active filters. |

The `FilterChanged` event notifies about a filter change.

### Operators (`FilterOperator`)

`None`, `Equals`, `NotEquals`, `Contains`, `StartsWith`, `EndsWith`, `GreaterThan`, `LessThan`, `GreaterThanOrEqual`, `LessThanOrEqual`, `Between`, `In`.

### Base class `ColumnFilterBase`

Abstract class for creating custom filters:

| Member | Description |
| :--- | :--- |
| `SortMemberPath` | Path to the data object property. |
| `DataType` | Data type (`SortDataType`). |
| `IsActive` | Whether the filter is active. |
| `Activate()` / `Deactivate()` | Activate/deactivate. |
| `Clear()` | Full reset of conditions (abstract). |
| `Passes(value)` | Checks whether a value passes the filter (abstract). |

### Selecting the filter UI (`FilterUIFactory`)

`FilterUIFactory.CreateFilterControl(headerItem, filterService)` selects a control by `SortDataType`:
`Text` → text filter, `Number` → range, `Date`/`DateTime` → date range.

To create a custom filter: inherit from `ColumnFilterBase`, implement `Clear()` and `Passes()`, then register the UI control in `FilterUIFactory`.

## 🔀 Sorting

Column properties: `CanUserSort`, `SortMemberPath`, `SortDataType`, `SortDirection`.

Sort types (`SortDataType`): `Text`, `Number`, `Date`, `DateTime`, `Boolean`.

For non-text types, optimized typed comparators (with caching) are used, which ensures correct sorting of numbers, dates and boolean values.

## 📊 Aggregates (bottom panel)

Aggregate types (`AggregateType`): `None`, `Sum`, `Average`, `Count`, `Min`, `Max`.

Set `AggregateType` on a column to calculate the total in the bottom panel:

```xml
<headers:ColumnHeaderLeaf Header="Total" SortMemberPath="Total" AggregateType="Sum" Width="120" />
```

Call `RefreshAggregates()` for manual recalculation of totals.

Use `BottomCellTemplate` to style the total cell (DataContext — `BottomCellViewModel`, the `Value` property contains the result):

```xml
<DataTemplate x:Key="SumCellTemplate">
    <TextBlock Text="{Binding Value}" FontWeight="Bold" HorizontalAlignment="Center" />
</DataTemplate>

<headers:ColumnHeaderLeaf Header="Total" AggregateType="Sum"
    BottomCellTemplate="{StaticResource SumCellTemplate}" Width="120" />
```

## ➡️ Next steps

- [Overview and quick start](overview.md)
- [Events, selection modes and property reference](reference.md)
- [Custom cells, behaviors and customization examples](customization.md)