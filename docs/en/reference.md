# MakroFlexGrid — Reference

This document covers events, row selection modes, the `CustomDataGrid` property reference, column settings and available cell types.

## 📌 Events

`CustomDataGrid` provides the following public events:

| Event | Signature | Description |
| :--- | :--- | :--- |
| `RowSelected` | `EventHandler<object>` | Row selection (click). The argument is the row data object (`Item`). |
| `RowDoubleClicked` | `EventHandler<object>` | Double-click on a row. The argument is the row data object. |
| `SelectedItemsChanged` | `EventHandler<IReadOnlyCollection<object>>` | Change of the selected items collection (only in `Multiple` mode). |
| `CellRightClicked` | `EventHandler<CellClickEventArgs>` | Right-click on a cell. `CellClickEventArgs` contains `Item`, `ColumnHeader`, `RowViewModel`. |

Example of subscribing to events:

```csharp
dataGrid.RowSelected += (s, item) => Console.WriteLine($"Row selected: {item}");
dataGrid.RowDoubleClicked += (s, item) => Console.WriteLine($"Double click: {item}");

dataGrid.CellRightClicked += (s, e) =>
{
    // e.Item — row data object
    // e.ColumnHeader — ColumnHeaderItem of the column
    // e.RowViewModel — RowViewModel of the row
    Console.WriteLine($"Right click on column {e.ColumnHeader.Header}, row {e.Item}");
};

dataGrid.SelectedItemsChanged += (s, items) =>
{
    Console.WriteLine($"Selected items: {items.Count}");
};
```

## 🗂 Row Selection Modes (`RowSelectionMode`)

The `RowSelectionMode` property (`CustomDataGrid`) accepts the values:

| Value | Description |
| :--- | :--- |
| `None` | Row selection is disabled. Click does not change the selection. |
| `Single` | Only one row can be selected (default). |
| `Multiple` | Multiple selection: click — reset+select, `Ctrl+Click` — toggle, `Shift+Click` — range. |

```xml
<mfgrid:CustomDataGrid RowSelectionMode="Multiple" ... />
```

In `Multiple` mode the `SelectedItems` collection and the `SelectedItemsChanged` event are available.

## ⚙️ `CustomDataGrid` Property Reference

### Panels and rows styling

| Property | Type | Default | Description |
| :--- | :--- | :--- | :--- |
| `GridLineBrush` | `Brush` | `LightGray` | Grid line color. |
| `LeftFrozenPanelBackground` | `Brush` | `Transparent` | Background of the left frozen panel. |
| `RightFrozenPanelBackground` | `Brush` | `Transparent` | Background of the right frozen panel. |
| `CenterPanelBackground` | `Brush` | `Transparent` | Background of the central (scrollable) panel. |
| `RowBackground` | `Brush` | `Transparent` | Background of rows. |
| `RowSelectedBackground` | `Brush` | `#afedfa` | Background of the selected row. |
| `BottomPanelBackground` | `Brush` | `Transparent` | Background of the bottom totals panel. |
| `LeftMargin` / `RightMargin` | `Thickness` | `0` | Outer margins (synchronized with scroll). |
| `LeftBottomMargin` / `RightBottomMargin` | `Thickness` | `0` | Bottom panel margins (below the scrollbar). |

### Cell selection

| Property | Type | Default | Description |
| :--- | :--- | :--- | :--- |
| `IsCellSelectionEnabled` | `bool` | `true` | Enables/disables the border of selected cells. |
| `CellSelectedBorderBrush` | `Brush` | `DodgerBlue` | Border brush of the selected cell. |
| `CellSelectedBorderThickness` | `Thickness` | `1` | Border thickness of the selected cell. |

### Frozen zone separators

| Property | Type | Default | Description |
| :--- | :--- | :--- | :--- |
| `SeparatorWidth` | `double` | `0` | Width of the separator between frozen zones. |
| `SeparatorBrush` | `Brush` | `Gray` | Separator color. |
| `ShowScrollBarSpacers` | `bool` | `false` | Show empty zones above the scrollbar (so it does not overlap frozen zones). |

### Bottom totals panel

| Property | Type | Default | Description |
| :--- | :--- | :--- | :--- |
| `BottomPanelHeight` | `double` | `20` | Height of the totals panel. |
| `ShowBottomCellBorders` | `bool` | `true` | Show cell borders in the totals panel. |
| `BottomPanelText` | `string` | `` | Full-width panel text (for example, "Total:"). |
| `PanelTextAlignment` | `HorizontalAlignment` | `Center` | Alignment of the panel text. |
| `BottomPanelTextPosition` | `PanelTextPosition` | `Top` | Text position (`Top` / `Bottom`) relative to the total cells. |
| `PanelTextPadding` | `Thickness` | `0,2,0,2` | Padding of the panel text. |
| `PanelTextTemplate` | `DataTemplate` | — | Custom panel text template. |
| `BottomRowTemplate` | `DataTemplate` | — | Template of an additional row below the main one (DataContext — row data). |

### Behavior

| Property | Type | Default | Description |
| :--- | :--- | :--- | :--- |
| `IsSystemColumnEnabled` | `bool` | `true` | Show the system column (with the selection triangle). |
| `IsDeferredResizeEnabled` | `bool` | `false` | Deferred column resize (for performance). |
| `AllowDrag` | `bool` | `true` | Allow dragging column headers. |
| `RowSelectionMode` | `RowSelectionMode` | `Single` | Row selection mode. |

### Header zones

| Property | Type | Description |
| :--- | :--- | :--- |
| `FrozenColumnHeaders` | `ColumnHeaderCollection` | Columns frozen on the left. |
| `ScrollableColumnHeaders` | `ColumnHeaderCollection` | The main scrollable area. |
| `RightFrozenColumnHeaders` | `ColumnHeaderCollection` | Columns frozen on the right. |

### Public methods and services

| Member | Type | Description |
| :--- | :--- | :--- |
| `RefreshAggregates()` | method | Recalculates the total values in the bottom panel. |
| `RefreshHeaders()` | method | Rebuilds the header elements. |
| `SyncColumnsWithHeaders()` | method | Synchronizes `DataGrid.Columns` with the header hierarchy. |
| `PerformSort(column)` | method | Starts sorting by the specified column. |
| `FilterService` | `FilterService` | Column filtering service. |
| `SelectedItems` | `IReadOnlyCollection<object>` | Collection of selected items. |
| `BottomPanel` | `BottomPanelViewModel` | ViewModel of the bottom panel. |

## 🧪 Column Settings (`ColumnHeaderItem`)

Properties are available on `ColumnHeaderGroup` and `ColumnHeaderLeaf` elements:

| Property | Type | Default | Description |
| :--- | :--- | :--- | :--- |
| `Header` | `object` | — | Header text/content. |
| `Width` / `MinWidth` / `MaxWidth` | `double` | `20` | Column sizes. |
| `IsVisible` | `bool` | `true` | Column visibility. |
| `HeaderTemplate` | `DataTemplate` | — | Custom header template. |
| `HeaderStyle` | `Style` | — | Header style. |
| `HorizontalHeaderAlignment` | `HorizontalAlignment` | `Center` | Horizontal header alignment. |
| `VerticalHeaderAlignment` | `VerticalAlignment` | `Center` | Vertical header alignment. |
| `CellTemplate` | `DataTemplate` | — | Cell template. |
| `BottomCellTemplate` | `DataTemplate` | — | Total cell template (bottom panel). |
| `CanUserSort` | `bool` | `false` | Allow sorting by the column. |
| `SortMemberPath` | `string` | — | Path to the data object property for sorting/binding. |
| `SortDataType` | `SortDataType` | `Text` | Data type for sorting. |
| `AggregateType` | `AggregateType` | `None` | Aggregate for the bottom panel. |
| `CanUserFilter` | `bool` | `true` | Allow column filtering. |
| `CanUserHide` | `bool` | `true` | Allow hiding the column via the context menu. |
| `AllowDrag` | `bool` | `true` | Allow dragging this column. |
| `AllowCrossSectionDrag` | `bool` | `true` | Allow moving between zones (Frozen/Scrollable/RightFrozen). |
| `Filter` | `ColumnFilterBase` | — | Active column filter. |
| `GripperPosition` | `GripperPositionType` | `Right` | Resize gripper position (`Left`/`Right`). |

## 🧬 Cell Types

Each column (`ColumnHeaderLeaf` or a subclass) defines the visual representation of the cell. Specialized leaf properties are automatically copied into `CellViewModel.Config` and used by templates via `{Binding Config[Key]}`.

### Available types

| Cell type | Class | Special properties (`Config`) |
| :--- | :--- | :--- |
| `Editable` | `EditableColumnHeaderLeaf` | Editable text (Enter/loss of focus — save, `Esc` — cancel). |
| `Numeric` | `NumericColumnHeaderLeaf` | `DecimalPlaces` (`2`), `MinValue`, `MaxValue`, `Format`, `CurrencySymbol`, `AllowNegative` (`true`). |
| `ComboBox` | `ComboBoxColumnHeaderLeaf` | `ItemsSource`, `DisplayMemberPath`, `SelectedValuePath`, `SelectedValueBinding`. |
| `CheckBox` | `CheckBoxColumnHeaderLeaf` | `IsThreeState` (`false`). |
| `Date` | `DateColumnHeaderLeaf` | `Format` (`dd.MM.yyyy`), `FirstDayOfWeek` (`Monday`). |
| `Hyperlink` | `HyperlinkColumnHeaderLeaf` | `Command`, `CommandParameter`, `UrlBinding`. |
| `Image` | `ImageColumnHeaderLeaf` | `Stretch` (`Uniform`), `MaxWidth`, `MaxHeight`, `DefaultImage`. |
| `Progress` | `ProgressColumnHeaderLeaf` | `Minimum` (`0`), `Maximum` (`100`), `ShowPercentage` (`true`). |
| `Rating` | `RatingColumnHeaderLeaf` | `MaxRating` (`5`), `Icon` (`Star`), `IsReadOnly` (`false`). |
| `Color` | `ColorColumnHeaderLeaf` | `ShowColorName` (`true`), `Editable` (`false`). |
| `MultiLine` | `MultiLineColumnHeaderLeaf` | `MaxLines` (`5`), `IsReadOnly` (`false`). |
| `RadioButton` | `RadioButtonColumnHeaderLeaf` | `GroupName`, `Options`. |
| (regular) | `ColumnHeaderLeaf` | Text display via `TextBlock`. |

### Usage examples of types

```xml
<!-- Numeric column -->
<headers:NumericColumnHeaderLeaf Header="Price"
    SortMemberPath="Price" SortDataType="Number"
    DecimalPlaces="2" CurrencySymbol="$" AllowNegative="False" Width="120" />

<!-- Checkbox -->
<headers:CheckBoxColumnHeaderLeaf Header="Active"
    SortMemberPath="IsActive" IsThreeState="False" Width="80" />

<!-- Rating -->
<headers:RatingColumnHeaderLeaf Header="Rating"
    SortMemberPath="Rating" MaxRating="10" Width="100" />

<!-- Progress -->
<headers:ProgressColumnHeaderLeaf Header="Readiness"
    SortMemberPath="Progress" Minimum="0" Maximum="100" ShowPercentage="True" Width="140" />

<!-- Date -->
<headers:DateColumnHeaderLeaf Header="Date"
    SortMemberPath="Date" Format="dd.MM.yyyy" Width="110" />

<!-- Image -->
<headers:ImageColumnHeaderLeaf Header="Avatar"
    SortMemberPath="AvatarUrl" Stretch="UniformToFill" Width="60" />

<!-- Hyperlink -->
<headers:HyperlinkColumnHeaderLeaf Header="Website"
    SortMemberPath="Website" UrlBinding="Website" Width="160" />

<!-- ComboBox -->
<headers:ComboBoxColumnHeaderLeaf Header="Status"
    SortMemberPath="Status" ItemsSource="{Binding Statuses}"
    SelectedValuePath="Id" SelectedValueBinding="StatusId" Width="140" />

<!-- Multi-line text -->
<headers:MultiLineColumnHeaderLeaf Header="Description"
    SortMemberPath="Description" MaxLines="3" Width="200" />

<!-- Radio buttons -->
<headers:RadioButtonColumnHeaderLeaf Header="Option"
    SortMemberPath="Option" GroupName="Opt" Options="{Binding Options}" Width="150" />
```

## ➡️ Next steps

- [Overview and quick start](overview.md)
- [Custom cells, behaviors and customization examples](customization.md)
- [Filtering, sorting and aggregates](data-operations.md)