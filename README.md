> **🌍 Язык / Language:** [**English**](#english) · [**Русский**](#russian)

---

<h1 id="english">English</h1>

```
# MakroFlexGrid

High-performance WPF DataGrid with row virtualization, multi-level headers, filtering, sorting, aggregates and a wide range of cell types.

`MakroFlexGrid` is built on top of the standard `DataGrid` and is designed for displaying large amounts of data with support for complex hierarchical headers, frozen columns (left and right), multi-level filtering and automatic calculation of aggregates in the bottom panel.

<img width="1389" height="773" alt="image" src="https://github.com/user-attachments/assets/157404a7-947e-4fc6-a281-a4f24f6d34f8" />


## 🚀 Features

- **Row virtualization** — efficient work with large datasets (`UnifiedRowsPresenter`).
- **Hierarchical headers** — grouping columns into multiple levels (`ColumnHeaderGroup` → `ColumnHeaderLeaf`).
- **Column freezing** — independent zones: left (`FrozenColumnHeaders`), the main scrollable area, and right (`RightFrozenColumnHeaders`).
- **Filtering** — built-in filter system: text, numeric (ranges), by date and time.
- **Sorting** — click on the header with optimized typed comparators.
- **Bottom aggregate panel** — automatic calculation of `Sum`, `Average`, `Min`, `Max`, `Count`.
- **Column Drag-and-Drop** — column reordering with visual indicators.
- **Wide range of cell types** — text, numbers, dates, images, checkboxes, radio buttons, combo boxes, ratings, progress, hyperlinks and more.
- **Deep customization** — via `DataTemplate` for cells, headers and totals.
- **Localization** — support for multiple resource languages.

## 📦 Installation

Add the `MakroFlexGrid` package via NuGet Package Manager:

```
dotnet add package MakroFlexGrid
```

The target platform **Windows** and WPF usage are required: `net8.0-windows`, `net9.0-windows` or `net10.0-windows`.

## 🧩 Quick Start

Add the namespace and place the control in markup:

```xml
<Window xmlns:mfgrid="clr-namespace:MakroFlexGrid.Core"
        xmlns:headers="clr-namespace:MakroFlexGrid.Headers.Base">
    <mfgrid:CustomDataGrid
        ItemsSource="{Binding MyDataList}"
        GridLineBrush="LightGray"
        BottomPanelHeight="30">

        <mfgrid:CustomDataGrid.ScrollableColumnHeaders>
            <headers:ColumnHeaderGroup Header="User" Width="300">
                <headers:ColumnHeaderLeaf Header="Name"
                                          SortMemberPath="Name"
                                          SortDataType="Text"
                                          Width="150" />
                <headers:ColumnHeaderLeaf Header="Email"
                                          SortMemberPath="Email"
                                          SortDataType="Text"
                                          Width="150" />
            </headers:ColumnHeaderGroup>
        </mfgrid:CustomDataGrid.ScrollableColumnHeaders>
    </mfgrid:CustomDataGrid>
</Window>
```

### Example with a custom cell template

The cell's data context is a `CellViewModel`. To access the row data, use the path `RowViewModel.Item`:

```xml
<DataTemplate x:Key="BooleanCellTemplate">
    <CheckBox Margin="2"
              HorizontalAlignment="Center"
              VerticalAlignment="Center"
              IsChecked="{Binding RowViewModel.Item.IsActive, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" />
</DataTemplate>

<headers:ColumnHeaderLeaf Header="Active"
                          CellTemplate="{StaticResource BooleanCellTemplate}"
                          SortMemberPath="IsActive"
                          Width="80" />
```

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

## 🎨 Custom Cells: `CellViewModel` and `Config`

The cell's data context (`DataContext`) is a `CellViewModel`. Key members:

| Member | Type | Description |
| :--- | :--- | :--- |
| `RowViewModel` | `RowViewModel` | Row ViewModel (to access `Item`). |
| `Item` | `object` | Row data object (your model data). |
| `Value` | `string` | String representation of the value (for display). |
| `CellType` | `string` | Cell type (for example, `Numeric`, `ComboBox`). |
| `Config` | `Dictionary<string,object>` | Cell configuration by keys (from leaf properties). |
| `Column` | `DataGridColumn` | The `DataGrid` column. |
| `IsEditing` | `bool` | Editing mode flag. |
| `EditValue` | `string` | Value in editing mode. |
| `Width` | `double` | Cell width. |
| `IsCellSelected` | `bool` | The cell is selected. |
| `IsLeftmostInRightPanel` | `bool` | The leftmost cell of the right frozen panel. |

### Accessing `Config` in XAML and C#

In the cell template XAML, leaf parameters are available via `{Binding Config[Key]}`:

```xml
<DataTemplate x:Key="CustomNumericTemplate">
    <StackPanel Orientation="Horizontal">
        <TextBlock Text="{Binding Value}" />
        <TextBlock Text="{Binding Config[CurrencySymbol]}" />
    </StackPanel>
</DataTemplate>
```

In C# code (for example, in a behavior) use typed access:

```csharp
int decimalPlaces = cellVm.GetConfig("DecimalPlaces", 2);
string format = cellVm.GetConfig<string>("Format");
```

### Editing methods

For editable cells (`Editable`, `Numeric`, `Date`, `MultiLine`) the following methods are available:

- `CommitEdit()` — saves `EditValue` back to the data model (with conversion to the target type).
- `CancelEdit()` — cancels editing and restores the original value.
- `IsEditing` / `EditValue` — managing the editing mode.

### Cell factory (`CellViewModelFactory`)

You can register your own factory method to create a specialized `CellViewModel` for a specific leaf type:

```csharp
CellViewModelFactory.Register<MyHeaderLeaf>((rowVm, column) => new MyCellViewModel(rowVm, column));
```

## 🧰 Attached Cell Behaviors

Behaviors configure the controls inside cell templates and are applied in XAML:

```xml
<TextBox rows:CellBehaviorBase.IsEnabled="True" rows:CellBehaviorBase.CellType="Numeric" />
```

Available classes:

| Class | Purpose |
| :--- | :--- |
| `CellBehaviorBase` | Single base behavior (`IsEnabled`, `CellType`). |
| `EditableCellBehavior` | Text editing: click — enter, loss of focus/`Enter` — save, `Esc` — cancel. |
| `ComboBoxCellBehavior` | Configures the `ComboBox` from `Config` (ItemsSource, DisplayMemberPath, SelectedValuePath). |
| `NumericCellBehavior` | Numbers-only input, formatting by `DecimalPlaces`/`Format`/`CurrencySymbol`. |
| `RatingCellBehavior` | Generates stars by `MaxRating`, handles clicks. |

Registering custom handlers:

```csharp
CellBehaviorBase.RegisterSetupHandler<ComboBox>(SetupComboBox);
CellBehaviorBase.RegisterEventHandler<ComboBox, MouseButtonEventArgs>("MouseDown", OnComboMouseDown);
```

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

## 🧩 Creating a Custom Cell Type

1. Create a subclass of `ColumnHeaderLeaf` with the `[CellTemplate("UnifiedCellTemplate")]` attribute and the required DependencyProperties:

```csharp
[CellTemplate("UnifiedCellTemplate")]
public class MyHeaderLeaf : ColumnHeaderLeaf
{
    public static readonly DependencyProperty MyOptionProperty =
        DependencyProperty.Register("MyOption", typeof(int), typeof(MyHeaderLeaf),
            new FrameworkPropertyMetadata(1));

    public int MyOption
    {
        get => (int)GetValue(MyOptionProperty);
        set => SetValue(MyOptionProperty, value);
    }
}
```

2. In `UnifiedCellTemplate.xaml` add a `DataTemplate` with the key `MyCellView` and a `DataTrigger` on `CellType = "My"`. Inside the template the parameter is available as `{Binding Config[MyOption]}`.

3. If necessary, register a behavior via `CellBehaviorBase` (for example, `CellBehaviorBase.RegisterSetupHandler<FrameworkElement>(...)`).

4. The `MyHeaderLeaf` column automatically uses the new template — the `CellType` property is formed from the class name without the `ColumnHeaderLeaf` suffix.

## 🛠 Customization Examples

### 1. Data model and ViewModel

```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
    public DateTime DateAdded { get; set; }
    public double Rating { get; set; }
    public double Progress { get; set; }
    public string Website { get; set; }
    public string AvatarUrl { get; set; }
    public string Description { get; set; }
    public int StatusId { get; set; }
    public string Option { get; set; }
}

public class MainViewModel
{
    public ObservableCollection<Product> Products { get; } = new ObservableCollection<Product>
    {
        new Product { Id = 1, Name = "Product A", Price = 1500.50m, IsActive = true,
                      DateAdded = DateTime.Today.AddDays(-3), Rating = 4.5, Progress = 78,
                      Website = "https://example.com/a", Description = "Long description\nwith line breaks" },
        // ... other products
    };

    public List<StatusOption> Statuses { get; } = new List<StatusOption>
    {
        new StatusOption { Id = 1, Name = "In stock" },
        new StatusOption { Id = 2, Name = "On order" },
        new StatusOption { Id = 3, Name = "Out of stock" }
    };

    public List<string> Options { get; } = new List<string> { "Option 1", "Option 2", "Option 3" };
}

public class StatusOption
{
    public int Id { get; set; }
    public string Name { get; set; }
}
```

### 2. Window markup with all column types

```xml
<Window xmlns:mfgrid="clr-namespace:MakroFlexGrid.Core"
        xmlns:headers="clr-namespace:MakroFlexGrid.Headers"
        xmlns:rows="clr-namespace:MakroFlexGrid.Rows"
        Title="MakroFlexGrid — customization example">
    <Grid>
        <mfgrid:CustomDataGrid
            ItemsSource="{Binding Products}"
            GridLineBrush="LightGray"
            RowSelectionMode="Multiple"
            IsSystemColumnEnabled="True"
            SeparatorWidth="2"
            SeparatorBrush="DarkGray"
            BottomPanelHeight="28"
            BottomPanelText="Total:"
            BottomPanelTextPosition="Top"
            PanelTextAlignment="Right"
            RowSelectedBackground="#CFE8FF">

            <!-- Columns on the left (frozen) -->
            <mfgrid:CustomDataGrid.FrozenColumnHeaders>
                <headers:ColumnHeaderLeaf Header="No"
                    SortMemberPath="Id" SortDataType="Number"
                    CanUserSort="True" Width="60" />
                <headers:ColumnHeaderLeaf Header="Product"
                    SortMemberPath="Name" SortDataType="Text"
                    CanUserSort="True" Width="200" />
            </mfgrid:CustomDataGrid.FrozenColumnHeaders>

            <!-- Main columns (scrollable) -->
            <mfgrid:CustomDataGrid.ScrollableColumnHeaders>
                <headers:ColumnHeaderGroup Header="Finance" Width="280">
                    <headers:NumericColumnHeaderLeaf Header="Price"
                        SortMemberPath="Price" SortDataType="Number"
                        CanUserSort="True" AggregateType="Sum"
                        DecimalPlaces="2" CurrencySymbol="$"
                        AllowNegative="False" Width="140" />
                    <headers:ProgressColumnHeaderLeaf Header="Progress"
                        SortMemberPath="Progress" SortDataType="Number"
                        Minimum="0" Maximum="100" ShowPercentage="True"
                        Width="160" />
                </headers:ColumnHeaderGroup>

                <headers:CheckBoxColumnHeaderLeaf Header="Active"
                    SortMemberPath="IsActive" SortDataType="Boolean"
                    CanUserSort="True" IsThreeState="False" Width="80" />

                <headers:DateColumnHeaderLeaf Header="Date"
                    SortMemberPath="DateAdded" SortDataType="Date"
                    CanUserSort="True" Format="dd.MM.yyyy" Width="120" />

                <headers:RatingColumnHeaderLeaf Header="Rating"
                    SortMemberPath="Rating" SortDataType="Number"
                    MaxRating="5" Width="120" />

                <headers:HyperlinkColumnHeaderLeaf Header="Website"
                    SortMemberPath="Website" UrlBinding="Website" Width="180" />

                <headers:ImageColumnHeaderLeaf Header="Photo"
                    SortMemberPath="AvatarUrl" Stretch="UniformToFill"
                    Width="70" />

                <headers:ComboBoxColumnHeaderLeaf Header="Status"
                    SortMemberPath="StatusId" SortDataType="Text"
                    ItemsSource="{Binding Statuses}"
                    SelectedValuePath="Id"
                    SelectedValueBinding="StatusId" Width="140" />

                <headers:MultiLineColumnHeaderLeaf Header="Description"
                    SortMemberPath="Description" MaxLines="3" Width="220" />
            </mfgrid:CustomDataGrid.ScrollableColumnHeaders>

            <!-- Columns on the right (frozen) -->
            <mfgrid:CustomDataGrid.RightFrozenColumnHeaders>
                <headers:RadioButtonColumnHeaderLeaf Header="Option"
                    SortMemberPath="Option" GroupName="Opt"
                    Options="{Binding Options}" Width="180" />
            </mfgrid:CustomDataGrid.RightFrozenColumnHeaders>
        </mfgrid:CustomDataGrid>
    </Grid>
</Window>
```

### 3. Custom cell template with `Config`

Suppose you need a cell that shows the product name in color and with a status icon.

```xml
<Window.Resources>
    <!-- Cell template. DataContext = CellViewModel. Row data via RowViewModel.Item -->
    <DataTemplate x:Key="ProductNameCellTemplate">
        <StackPanel Orientation="Horizontal" VerticalAlignment="Center" Margin="4,0">
            <Ellipse Width="10" Height="10" Margin="0,0,6,0">
                <Ellipse.Style>
                    <Style TargetType="Ellipse">
                        <Setter Property="Fill" Value="Gray" />
                        <Style.Triggers>
                            <DataTrigger Binding="{Binding RowViewModel.Item.IsActive}" Value="True">
                                <Setter Property="Fill" Value="Green" />
                            </DataTrigger>
                        </Style.Triggers>
                    </Style>
                </Ellipse.Style>
            </Ellipse>
            <TextBlock Text="{Binding RowViewModel.Item.Name}"
                       FontWeight="Bold" VerticalAlignment="Center" />
        </StackPanel>
    </DataTemplate>

    <!-- Total cell template (bottom panel). DataContext = BottomCellViewModel, property Value -->
    <DataTemplate x:Key="SumBottomCellTemplate">
        <StackPanel Orientation="Horizontal" HorizontalAlignment="Center">
            <TextBlock Text="Σ " FontWeight="Bold" />
            <TextBlock Text="{Binding Value}" FontWeight="Bold" />
        </StackPanel>
    </DataTemplate>
</Window.Resources>
```

Applying templates to columns:

```xml
<headers:ColumnHeaderLeaf Header="Product"
    SortMemberPath="Name"
    CellTemplate="{StaticResource ProductNameCellTemplate}"
    Width="200" />

<headers:NumericColumnHeaderLeaf Header="Price"
    SortMemberPath="Price" AggregateType="Sum"
    BottomCellTemplate="{StaticResource SumBottomCellTemplate}"
    Width="140" />
```

### 4. Custom header template with `HeaderTemplate`

```xml
<Window.Resources>
    <DataTemplate x:Key="HeaderWithIconTemplate">
        <StackPanel Orientation="Horizontal" VerticalAlignment="Center">
            <TextBlock Text="🔍 " VerticalAlignment="Center" />
            <TextBlock Text="{Binding}" VerticalAlignment="Center" FontWeight="Bold" />
        </StackPanel>
    </DataTemplate>
</Window.Resources>

<!-- Application -->
<headers:ColumnHeaderGroup Header="Search and filters"
    HeaderTemplate="{StaticResource HeaderWithIconTemplate}" Width="300">
    <headers:ColumnHeaderLeaf Header="Query" SortMemberPath="Query" Width="150" />
</headers:ColumnHeaderGroup>
```

### 5. Configuring filters via `FilterService`

```csharp
// Get the leaf of the column to filter by
var priceHeader = grid.ScrollableColumnHeaders.GetBottomItems()
    .OfType<NumericColumnHeaderLeaf>()
    .FirstOrDefault(h => h.SortMemberPath == "Price");

if (priceHeader != null)
{
    // Create a numeric range filter
    var filter = new NumberColumnFilter(priceHeader.SortMemberPath, priceHeader.SortDataType)
    {
        Operator = FilterOperator.Between,
        FromValue = 100,
        ToValue = 1000
    };
    filter.Activate();

    // Set the filter via the service
    grid.FilterService.SetFilter(priceHeader, filter);
}

// Reset a single column filter
grid.FilterService.ClearFilter(priceHeader);

// Reset all filters
grid.FilterService.ClearAllFilters();
```

### 6. Handling events

```csharp
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();

        dataGrid.RowSelected += OnRowSelected;
        dataGrid.RowDoubleClicked += OnRowDoubleClicked;
        dataGrid.CellRightClicked += OnCellRightClicked;
        dataGrid.SelectedItemsChanged += OnSelectedItemsChanged;
    }

    private void OnRowSelected(object sender, object item)
    {
        if (item is Product product)
            Title = $"Selected: {product.Name}";
    }

    private void OnRowDoubleClicked(object sender, object item)
    {
        if (item is Product product)
            MessageBox.Show($"Opening product card «{product.Name}»");
    }

    private void OnCellRightClicked(object sender, CellClickEventArgs e)
    {
        var menu = new ContextMenu();
        if (e.Item is Product product)
        {
            menu.Items.Add(new MenuItem { Header = $"Product: {product.Name}" });
            menu.Items.Add(new MenuItem { Header = "Action" });
        }
        menu.IsOpen = true;
    }

    private void OnSelectedItemsChanged(object sender, IReadOnlyCollection<object> items)
    {
        StatusBarText.Text = $"Selected: {items.Count}";
    }
}
```

### 7. Custom cell behavior

Let's create a behavior that shows the value when a cell is clicked.

```csharp
public static class ShowValueCellBehavior
{
    // At startup register a setup handler for TextBlock
    static ShowValueCellBehavior()
    {
        CellBehaviorBase.RegisterSetupHandler<TextBlock>(SetupTextBlock);
    }

    private static void SetupTextBlock(TextBlock textBlock)
    {
        textBlock.MouseLeftButtonUp += OnTextBlockClick;
        textBlock.Unloaded += (s, e) => textBlock.MouseLeftButtonUp -= OnTextBlockClick;
    }

    private static void OnTextBlockClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is TextBlock tb && tb.DataContext is CellViewModel cell)
        {
            MessageBox.Show($"Value: {cell.Value}");
        }
    }
}
```

Application in the template:

```xml
<DataTemplate x:Key="ClickableCellTemplate">
    <TextBlock Text="{Binding Value}" VerticalAlignment="Center"
               Cursor="Hand" TextDecorations="Underline" Foreground="Blue" />
</DataTemplate>

<!-- Column whose cells open a message on click -->
<headers:ColumnHeaderLeaf Header="Click me"
    SortMemberPath="Name" CellTemplate="{StaticResource ClickableCellTemplate}" Width="150" />
```

### 8. Hiding columns and configuring visibility

```csharp
// Hide a column programmatically
var header = grid.ScrollableColumnHeaders.GetBottomItems()
    .FirstOrDefault(h => h.SortMemberPath == "Description");
if (header != null) header.IsVisible = false;

// Refresh headers and rows after the change
grid.RefreshHeaders();
grid.RefreshRows();
```

```xml
<!-- Prohibit hiding via the context menu -->
<headers:ColumnHeaderLeaf Header="Product" SortMemberPath="Name"
    CanUserHide="False" Width="200" />

<!-- Prohibit filtering -->
<headers:ColumnHeaderLeaf Header="Code" SortMemberPath="Code"
    CanUserFilter="False" Width="100" />

<!-- Prohibit dragging a specific column -->
<headers:ColumnHeaderLeaf Header="No" SortMemberPath="Id"
    AllowDrag="False" Width="60" />
```

### 9. Customizing the bottom totals panel

```xml
<Window.Resources>
    <!-- Custom panel text template -->
    <DataTemplate x:Key="PanelHeaderTemplate">
        <StackPanel Orientation="Horizontal">
            <TextBlock Text="📊 " FontSize="12" />
            <TextBlock Text="{Binding}" FontWeight="Bold" />
        </StackPanel>
    </DataTemplate>
</Window.Resources>

<mfgrid:CustomDataGrid
    BottomPanelHeight="30"
    BottomPanelBackground="#F0F0F0"
    ShowBottomCellBorders="True"
    BottomPanelText="Table summary"
    BottomPanelTextPosition="Top"
    PanelTextAlignment="Center"
    PanelTextPadding="6,2,6,2"
    PanelTextTemplate="{StaticResource PanelHeaderTemplate}">
    <!-- ... columns ... -->
</mfgrid:CustomDataGrid>
```

### 10. Additional row below the main one (`BottomRowTemplate`)

`BottomRowTemplate` displays additional content below the main row. DataContext — the row data.

```xml
<Window.Resources>
    <DataTemplate x:Key="ProductBottomRowTemplate">
        <Border Background="#FAFAFA" BorderBrush="LightGray" BorderThickness="0,0,0,1" Padding="8,4">
            <TextBlock Text="{Binding Description}"
                       TextWrapping="Wrap" FontStyle="Italic" />
        </Border>
    </DataTemplate>
</Window.Resources>

<mfgrid:CustomDataGrid
    ItemsSource="{Binding Products}"
    BottomRowTemplate="{StaticResource ProductBottomRowTemplate}">
    <!-- ... columns ... -->
</mfgrid:CustomDataGrid>
```

### 11. Creating and using a custom cell type

Let's create a "Star rating with header" cell type.

**Step 1.** Column leaf class:

```csharp
using System.Windows;
using MakroFlexGrid.Headers;

[CellTemplate("UnifiedCellTemplate")]
public class StarRatingColumnHeaderLeaf : ColumnHeaderLeaf
{
    public static readonly DependencyProperty MaxRatingProperty =
        DependencyProperty.Register("MaxRating", typeof(int), typeof(StarRatingColumnHeaderLeaf),
            new FrameworkPropertyMetadata(5));

    public int MaxRating
    {
        get => (int)GetValue(MaxRatingProperty);
        set => SetValue(MaxRatingProperty, value);
    }
}
```

**Step 2.** Add a `DataTemplate` with the key `StarRatingCellView` in `Themes/RowTemplates/UnifiedCellTemplate.xaml`:

```xml
<DataTemplate x:Key="StarRatingCellView">
    <TextBlock VerticalAlignment="Center" HorizontalAlignment="Center"
               Text="{Binding Value}" FontSize="14" />
</DataTemplate>
```

**Step 3.** Add a `DataTrigger` to `UnifiedCellTemplate` (in `DataTemplate.Triggers`):

```xml
<DataTemplate>
    <!-- ... existing triggers ... -->
    <DataTrigger Binding="{Binding CellType}" Value="StarRating">
        <Setter Property="ContentTemplate" Value="{StaticResource StarRatingCellView}" />
    </DataTrigger>
</DataTemplate>
```

**Step 4.** Use the column:

```xml
<headers:StarRatingColumnHeaderLeaf Header="Stars"
    SortMemberPath="Rating" MaxRating="5" Width="100" />
```

**Mechanism description:** the `CellType` property is automatically formed from the class name without the `ColumnHeaderLeaf` suffix (i.e. `StarRating`). The universal template selects the corresponding `DataTemplate` with the key `{CellType}CellView` via a `DataTrigger`. Leaf parameters (for example, `MaxRating`) are copied into `CellViewModel.Config` and are available in the template as `{Binding Config[MaxRating]}`.

### 12. Cell selection

Managing the border of the selected cell.

```xml
<mfgrid:CustomDataGrid
    ItemsSource="{Binding Products}"
    IsCellSelectionEnabled="True"
    CellSelectedBorderBrush="#FF8C00"
    CellSelectedBorderThickness="2">
    <!-- ... columns ... -->
</mfgrid:CustomDataGrid>
```

```csharp
// Disable cell selection programmatically (only the row highlight remains)
dataGrid.IsCellSelectionEnabled = false;

// Change the color and thickness of the selected cell border
dataGrid.CellSelectedBorderBrush = new SolidColorBrush(Colors.Orange);
dataGrid.CellSelectedBorderThickness = new Thickness(3);
```

### 13. Frozen zone separators and the scrollbar

Configuring separators between the frozen and scrollable zones, as well as spacers below the scrollbar.

```xml
<mfgrid:CustomDataGrid
    SeparatorWidth="3"
    SeparatorBrush="#2F4F4F"
    ShowScrollBarSpacers="True">
    <!-- Frozen columns on the left -->
    <mfgrid:CustomDataGrid.FrozenColumnHeaders>
        <headers:ColumnHeaderLeaf Header="No" SortMemberPath="Id" Width="60" />
    </mfgrid:CustomDataGrid.FrozenColumnHeaders>

    <mfgrid:CustomDataGrid.ScrollableColumnHeaders>
        <headers:ColumnHeaderLeaf Header="Product" SortMemberPath="Name" Width="200" />
    </mfgrid:CustomDataGrid.ScrollableColumnHeaders>

    <!-- Frozen columns on the right -->
    <mfgrid:CustomDataGrid.RightFrozenColumnHeaders>
        <headers:ColumnHeaderLeaf Header="Actions" SortMemberPath="Id" Width="100" />
    </mfgrid:CustomDataGrid.RightFrozenColumnHeaders>
</mfgrid:CustomDataGrid>
```

### 14. Deferred column resize

`IsDeferredResizeEnabled="True"` allows changing the column width with a delay (the value is applied to the column when the gripper is released). Useful for large tables.

```xml
<mfgrid:CustomDataGrid
    ItemsSource="{Binding Products}"
    IsDeferredResizeEnabled="True">
    <!-- ... columns ... -->
</mfgrid:CustomDataGrid>
```

### 15. Row details (`RowDetailsTemplate`)

`RowDetailsTemplate` is inherited from the standard `DataGrid` and displays additional content below the selected row.

```xml
<Window.Resources>
    <DataTemplate x:Key="ProductDetailsTemplate">
        <StackPanel Margin="16,8">
            <TextBlock Text="{Binding Description}" TextWrapping="Wrap" />
            <TextBlock Text="{Binding Website}" Foreground="Blue" Margin="0,4,0,0" />
        </StackPanel>
    </DataTemplate>
</Window.Resources>

<mfgrid:CustomDataGrid
    ItemsSource="{Binding Products}"
    RowDetailsTemplate="{StaticResource ProductDetailsTemplate}">
    <!-- ... columns ... -->
</mfgrid:CustomDataGrid>
```

### 16. Programmatic sorting and direction

```csharp
// Find the column by the header leaf
var priceHeader = grid.ScrollableColumnHeaders.GetBottomItems()
    .OfType<NumericColumnHeaderLeaf>()
    .FirstOrDefault(h => h.SortMemberPath == "Price");

if (priceHeader != null)
{
    // Set the sort direction
    priceHeader.SortDirection = ListSortDirection.Ascending;
    priceHeader.CanUserSort = true;

    // Start sorting by the synchronized DataGrid column
    if (priceHeader.SyncColumn != null)
        grid.PerformSort(priceHeader.SyncColumn);
}
```

> **Note:** `SyncColumn` is an internal `DataGrid` column created from the header leaf. It can be obtained via the public method `grid.GetColumnHeaderItem(column)` (reverse lookup) or via `SyncColumn` on the leaf.

### 17. Configuring the system column and header zones

```xml
<mfgrid:CustomDataGrid
    IsSystemColumnEnabled="True"
    RowSelectionMode="Multiple">
    <!-- The system column (selection triangle) is shown on the left -->
</mfgrid:CustomDataGrid>
```

```csharp
// Check the number of frozen columns
int leftCount = grid.LeftFrozenColumnsCount;
int rightCount = grid.RightFrozenColumnsCount;

// Hide/show the system column
grid.IsSystemColumnEnabled = false;
```

### 18. Rebuilding headers and rows after changes

After programmatic changes to the header collections (adding/removing/hiding columns) call the synchronization:

```csharp
// Added a new column to the scrollable zone
var newHeader = new ColumnHeaderLeaf
{
    Header = "New",
    SortMemberPath = "Name",
    Width = 150
};
grid.ScrollableColumnHeaders.Add(newHeader);

// Synchronize headers with DataGrid.Columns
grid.SyncColumnsWithHeaders();

// Refresh rows (recreate cells)
grid.RefreshRows();
```

### 19. Creating frozen columns

`MakroFlexGrid` splits columns into **three functional zones**. Frozen columns do not scroll horizontally and remain always visible.

**Header zones:**
- `FrozenColumnHeaders` — columns **pinned on the left**.
- `ScrollableColumnHeaders` — the main **scrollable** area.
- `RightFrozenColumnHeaders` — columns **pinned on the right**.

The number of frozen columns is determined automatically by the number of items in the corresponding collections.

```xml
<mfgrid:CustomDataGrid ItemsSource="{Binding Products}">

    <!-- 1) Frozen columns on the left (always visible) -->
    <mfgrid:CustomDataGrid.FrozenColumnHeaders>
        <headers:ColumnHeaderLeaf Header="No"
            SortMemberPath="Id" Width="60" />
        <headers:ColumnHeaderLeaf Header="Product"
            SortMemberPath="Name" Width="200" />
    </mfgrid:CustomDataGrid.FrozenColumnHeaders>

    <!-- 2) Scrollable columns (horizontal scroll) -->
    <mfgrid:CustomDataGrid.ScrollableColumnHeaders>
        <headers:ColumnHeaderLeaf Header="Price"
            SortMemberPath="Price" Width="120" />
        <headers:ColumnHeaderLeaf Header="Date"
            SortMemberPath="DateAdded" Width="120" />
        <headers:ColumnHeaderLeaf Header="Description"
            SortMemberPath="Description" Width="220" />
    </mfgrid:CustomDataGrid.ScrollableColumnHeaders>

    <!-- 3) Frozen columns on the right (always visible) -->
    <mfgrid:CustomDataGrid.RightFrozenColumnHeaders>
        <headers:ColumnHeaderLeaf Header="Actions"
            SortMemberPath="Id" Width="100" />
    </mfgrid:CustomDataGrid.RightFrozenColumnHeaders>

</mfgrid:CustomDataGrid>
```

**Programmatic addition of a frozen column:**

```csharp
// Add a column to the left frozen zone
var leftHeader = new ColumnHeaderLeaf
{
    Header = "Code",
    SortMemberPath = "Code",
    Width = 80
};
grid.FrozenColumnHeaders.Add(leftHeader);

// Add a column to the right frozen zone
var rightHeader = new ColumnHeaderLeaf
{
    Header = "Status",
    SortMemberPath = "Status",
    Width = 100
};
grid.RightFrozenColumnHeaders.Add(rightHeader);

// Synchronize with DataGrid.Columns and refresh rows
grid.SyncColumnsWithHeaders();
grid.RefreshRows();

// Get the current number of frozen columns
int leftCount = grid.LeftFrozenColumnsCount;   // from FrozenColumnHeaders
int rightCount = grid.RightFrozenColumnsCount; // from RightFrozenColumnHeaders
```

**Tips:**
- Freezing columns **on the left** is usually used for key fields (ID, name) so they remain visible during horizontal scrolling.
- Freezing **on the right** is convenient for action columns (buttons, links) that should always be at hand.
- The separator between zones is configured via `SeparatorWidth` and `SeparatorBrush` (see example 13).
- Columns can be moved between zones via Drag-and-Drop (the `AllowCrossSectionDrag` property on the leaf, `true` by default).

## 📖 Documentation

Detailed documentation is located in [`docs/`](MakroFlexGrid/docs):

- [`UserGuide.md`](MakroFlexGrid/docs/UserGuide.md) — user guide, quick start and examples.
- [`Architecture.md`](MakroFlexGrid/docs/Architecture.md) — high-level architecture and interaction flows.
- [`TechnicalDoc.md`](MakroFlexGrid/docs/TechnicalDoc.md) — technical description of components.

## 🛠 Requirements

- Windows
- .NET 8, .NET 9 or .NET 10 (with WPF support)
- Visual Studio 2022 or a current version of the `dotnet` CLI

## 📄 License

The project is distributed under the **MIT** license. See the LICENSE file of the package for details.
```

<h1 id="russian">Russian</h1>

---

# MakroFlexGrid

Высокопроизводительный WPF DataGrid с виртуализацией строк, многоуровневыми заголовками, фильтрацией, сортировкой, агрегатами и широким набором типов ячеек.

`MakroFlexGrid` построен поверх стандартного `DataGrid` и предназначен для отображения больших объемов данных с поддержкой сложных иерархических заголовков, замороженных колонок (слева и справа), многоуровневой фильтрации и автоматического расчета агрегатов в нижней панели.

<img width="1389" height="773" alt="image" src="https://github.com/user-attachments/assets/157404a7-947e-4fc6-a281-a4f24f6d34f8" />


## 🚀 Возможности

- **Виртуализация строк** — эффективная работа с большими наборами данных (`UnifiedRowsPresenter`).
- **Иерархические заголовки** — группировка колонок в несколько уровней (`ColumnHeaderGroup` → `ColumnHeaderLeaf`).
- **Заморозка колонок** — независимые зоны: слева (`FrozenColumnHeaders`), основная прокручиваемая область и справа (`RightFrozenColumnHeaders`).
- **Фильтрация** — встроенная система фильтров: текстовые, числовые (диапазоны), по дате и времени.
- **Сортировка** — клик по заголовку с оптимизированными типизированными компараторами.
- **Нижняя панель агрегатов** — автоматический расчет `Sum`, `Average`, `Min`, `Max`, `Count`.
- **Drag-and-Drop колонок** — перестановка колонок с визуальными индикаторами.
- **Широкий набор типов ячеек** — текст, числа, даты, изображения, чекбоксы, радиокнопки, комбобоксы, рейтинги, прогресс, гиперссылки и др.
- **Глубокая кастомизация** — через `DataTemplate` для ячеек, заголовков и итогов.
- **Локализация** — поддержка нескольких языков ресурсов.

## 📦 Установка

Добавьте пакет `MakroFlexGrid` через NuGet Package Manager:

```
dotnet add package MakroFlexGrid
```

Требуется целевая платформа **Windows** и использование WPF: `net8.0-windows`, `net9.0-windows` или `net10.0-windows`.

## 🧩 Быстрый старт

Подключите пространство имен и разместите контрол в разметке:

```xml
<Window xmlns:mfgrid="clr-namespace:MakroFlexGrid.Core"
        xmlns:headers="clr-namespace:MakroFlexGrid.Headers.Base">
    <mfgrid:CustomDataGrid 
        ItemsSource="{Binding MyDataList}"
        GridLineBrush="LightGray"
        BottomPanelHeight="30">

        <mfgrid:CustomDataGrid.ScrollableColumnHeaders>
            <headers:ColumnHeaderGroup Header="Пользователь" Width="300">
                <headers:ColumnHeaderLeaf Header="Имя" 
                                          SortMemberPath="Name" 
                                          SortDataType="Text" 
                                          Width="150" />
                <headers:ColumnHeaderLeaf Header="Email" 
                                          SortMemberPath="Email" 
                                          SortDataType="Text" 
                                          Width="150" />
            </headers:ColumnHeaderGroup>
        </mfgrid:CustomDataGrid.ScrollableColumnHeaders>
    </mfgrid:CustomDataGrid>
</Window>
```

### Пример с кастомным шаблоном ячейки

Контекстом данных ячейки является `CellViewModel`. Чтобы добраться до данных строки, используйте путь `RowViewModel.Item`:

```xml
<DataTemplate x:Key="BooleanCellTemplate">
    <CheckBox Margin="2"
              HorizontalAlignment="Center"
              VerticalAlignment="Center"
              IsChecked="{Binding RowViewModel.Item.IsActive, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" />
</DataTemplate>

<headers:ColumnHeaderLeaf Header="Активен" 
                          CellTemplate="{StaticResource BooleanCellTemplate}" 
                          SortMemberPath="IsActive" 
                          Width="80" />
```

## 📌 События

`CustomDataGrid` предоставляет следующие публичные события:

| Событие | Сигнатура | Описание |
| :--- | :--- | :--- |
| `RowSelected` | `EventHandler<object>` | Выбор строки (клик). Аргумент — объект данных строки (`Item`). |
| `RowDoubleClicked` | `EventHandler<object>` | Двойной клик по строке. Аргумент — объект данных строки. |
| `SelectedItemsChanged` | `EventHandler<IReadOnlyCollection<object>>` | Изменение коллекции выбранных элементов (только в режиме `Multiple`). |
| `CellRightClicked` | `EventHandler<CellClickEventArgs>` | Правый клик по ячейке. `CellClickEventArgs` содержит `Item`, `ColumnHeader`, `RowViewModel`. |

Пример подписки на события:

```csharp
dataGrid.RowSelected += (s, item) => Console.WriteLine($"Выбрана строка: {item}");
dataGrid.RowDoubleClicked += (s, item) => Console.WriteLine($"Двойной клик: {item}");

dataGrid.CellRightClicked += (s, e) =>
{
    // e.Item — объект данных строки
    // e.ColumnHeader — ColumnHeaderItem колонки
    // e.RowViewModel — RowViewModel строки
    Console.WriteLine($"ПКМ по колонке {e.ColumnHeader.Header}, строка {e.Item}");
};

dataGrid.SelectedItemsChanged += (s, items) =>
{
    Console.WriteLine($"Выбрано элементов: {items.Count}");
};
```

## 🗂 Режимы выбора строк (`RowSelectionMode`)

Свойство `RowSelectionMode` (`CustomDataGrid`) принимает значения:

| Значение | Описание |
| :--- | :--- |
| `None` | Выбор строк запрещён. Клик не изменяет выделение. |
| `Single` | Можно выбрать только одну строку (по умолчанию). |
| `Multiple` | Множественный выбор: клик — сброс+выбор, `Ctrl+Click` — переключение, `Shift+Click` — диапазон. |

```xml
<mfgrid:CustomDataGrid RowSelectionMode="Multiple" ... />
```

В режиме `Multiple` доступна коллекция `SelectedItems` и событие `SelectedItemsChanged`.

## ⚙️ Справочник свойств `CustomDataGrid`

### Оформление панелей и строк

| Свойство | Тип | По умолчанию | Описание |
| :--- | :--- | :--- | :--- |
| `GridLineBrush` | `Brush` | `LightGray` | Цвет линий сетки. |
| `LeftFrozenPanelBackground` | `Brush` | `Transparent` | Фон левой frozen-панели. |
| `RightFrozenPanelBackground` | `Brush` | `Transparent` | Фон правой frozen-панели. |
| `CenterPanelBackground` | `Brush` | `Transparent` | Фон центральной (scrollable) панели. |
| `RowBackground` | `Brush` | `Transparent` | Фон строк. |
| `RowSelectedBackground` | `Brush` | `#afedfa` | Фон выбранной строки. |
| `BottomPanelBackground` | `Brush` | `Transparent` | Фон нижней панели итогов. |
| `LeftMargin` / `RightMargin` | `Thickness` | `0` | Внешние отступы (синхронизируются со скроллом). |
| `LeftBottomMargin` / `RightBottomMargin` | `Thickness` | `0` | Отступы нижней панели (под скроллбар). |

### Выделение ячеек

| Свойство | Тип | По умолчанию | Описание |
| :--- | :--- | :--- | :--- |
| `IsCellSelectionEnabled` | `bool` | `true` | Включает/отключает обводку выбранных ячеек. |
| `CellSelectedBorderBrush` | `Brush` | `DodgerBlue` | Кисть обводки выбранной ячейки. |
| `CellSelectedBorderThickness` | `Thickness` | `1` | Толщина обводки выбранной ячейки. |

### Разделители frozen-зон

| Свойство | Тип | По умолчанию | Описание |
| :--- | :--- | :--- | :--- |
| `SeparatorWidth` | `double` | `0` | Ширина разделителя между frozen-зонами. |
| `SeparatorBrush` | `Brush` | `Gray` | Цвет разделителя. |
| `ShowScrollBarSpacers` | `bool` | `false` | Показывать пустые зоны над скроллбаром (чтобы он не перекрывал frozen-зоны). |

### Нижняя панель итогов

| Свойство | Тип | По умолчанию | Описание |
| :--- | :--- | :--- | :--- |
| `BottomPanelHeight` | `double` | `20` | Высота панели итогов. |
| `ShowBottomCellBorders` | `bool` | `true` | Показывать бордюры ячеек в панели итогов. |
| `BottomPanelText` | `string` | `` | Текст на всю ширину панели (например, «Итого:»). |
| `PanelTextAlignment` | `HorizontalAlignment` | `Center` | Выравнивание текста панели. |
| `BottomPanelTextPosition` | `PanelTextPosition` | `Top` | Позиция текста (`Top` / `Bottom`) относительно ячеек итогов. |
| `PanelTextPadding` | `Thickness` | `0,2,0,2` | Отступы текста панели. |
| `PanelTextTemplate` | `DataTemplate` | — | Кастомный шаблон текста панели. |
| `BottomRowTemplate` | `DataTemplate` | — | Шаблон дополнительной строки снизу от основной (DataContext — данные строки). |

### Поведение

| Свойство | Тип | По умолчанию | Описание |
| :--- | :--- | :--- | :--- |
| `IsSystemColumnEnabled` | `bool` | `true` | Показывать системную колонку (с треугольником выделения). |
| `IsDeferredResizeEnabled` | `bool` | `false` | Отложенный ресайз колонок (для производительности). |
| `AllowDrag` | `bool` | `true` | Разрешить перетаскивание заголовков колонок. |
| `RowSelectionMode` | `RowSelectionMode` | `Single` | Режим выбора строк. |

### Зоны заголовков

| Свойство | Тип | Описание |
| :--- | :--- | :--- |
| `FrozenColumnHeaders` | `ColumnHeaderCollection` | Колонки, замороженные слева. |
| `ScrollableColumnHeaders` | `ColumnHeaderCollection` | Основная прокручиваемая область. |
| `RightFrozenColumnHeaders` | `ColumnHeaderCollection` | Колонки, замороженные справа. |

### Публичные методы и сервисы

| Член | Тип | Описание |
| :--- | :--- | :--- |
| `RefreshAggregates()` | метод | Пересчитывает итоговые значения в нижней панели. |
| `RefreshHeaders()` | метод | Перестраивает элементы заголовков. |
| `SyncColumnsWithHeaders()` | метод | Синхронизирует `DataGrid.Columns` с иерархией заголовков. |
| `PerformSort(column)` | метод | Запускает сортировку по указанной колонке. |
| `FilterService` | `FilterService` | Сервис фильтрации колонок. |
| `SelectedItems` | `IReadOnlyCollection<object>` | Коллекция выбранных элементов. |
| `BottomPanel` | `BottomPanelViewModel` | ViewModel нижней панели. |

## 🧪 Настройки колонок (`ColumnHeaderItem`)

Свойства доступны на элементах `ColumnHeaderGroup` и `ColumnHeaderLeaf`:

| Свойство | Тип | По умолчанию | Описание |
| :--- | :--- | :--- | :--- |
| `Header` | `object` | — | Текст/контент заголовка. |
| `Width` / `MinWidth` / `MaxWidth` | `double` | `20` | Размеры колонки. |
| `IsVisible` | `bool` | `true` | Видимость колонки. |
| `HeaderTemplate` | `DataTemplate` | — | Кастомный шаблон заголовка. |
| `HeaderStyle` | `Style` | — | Стиль заголовка. |
| `HorizontalHeaderAlignment` | `HorizontalAlignment` | `Center` | Горизонтальное выравнивание заголовка. |
| `VerticalHeaderAlignment` | `VerticalAlignment` | `Center` | Вертикальное выравнивание заголовка. |
| `CellTemplate` | `DataTemplate` | — | Шаблон ячейки. |
| `BottomCellTemplate` | `DataTemplate` | — | Шаблон ячейки итога (нижней панели). |
| `CanUserSort` | `bool` | `false` | Разрешить сортировку по колонке. |
| `SortMemberPath` | `string` | — | Путь к свойству объекта данных для сортировки/привязки. |
| `SortDataType` | `SortDataType` | `Text` | Тип данных для сортировки. |
| `AggregateType` | `AggregateType` | `None` | Агрегат для нижней панели. |
| `CanUserFilter` | `bool` | `true` | Разрешить фильтрацию колонки. |
| `CanUserHide` | `bool` | `true` | Разрешить скрытие колонки через контекстное меню. |
| `AllowDrag` | `bool` | `true` | Разрешить перетаскивание этой колонки. |
| `AllowCrossSectionDrag` | `bool` | `true` | Разрешить перенос между зонами (Frozen/Scrollable/RightFrozen). |
| `Filter` | `ColumnFilterBase` | — | Активный фильтр колонки. |
| `GripperPosition` | `GripperPositionType` | `Right` | Позиция gripper'а для ресайза (`Left`/`Right`). |

## 🧬 Типы ячеек

Каждая колонка (`ColumnHeaderLeaf` или наследник) определяет визуальное представление ячейки. Специализированные свойства листа автоматически копируются в `CellViewModel.Config` и используются шаблонами через `{Binding Config[Key]}`.

### Доступные типы

| Тип ячейки | Класс | Специальные свойства (`Config`) |
| :--- | :--- | :--- |
| `Editable` | `EditableColumnHeaderLeaf` | Редактируемый текст (Enter/потеря фокуса — сохранить, `Esc` — отмена). |
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
| (обычный) | `ColumnHeaderLeaf` | Текстовое отображение через `TextBlock`. |

### Примеры использования типов

```xml
<!-- Числовая колонка -->
<headers:NumericColumnHeaderLeaf Header="Цена"
    SortMemberPath="Price" SortDataType="Number"
    DecimalPlaces="2" CurrencySymbol="₽" AllowNegative="False" Width="120" />

<!-- Чекбокс -->
<headers:CheckBoxColumnHeaderLeaf Header="Активен"
    SortMemberPath="IsActive" IsThreeState="False" Width="80" />

<!-- Рейтинг -->
<headers:RatingColumnHeaderLeaf Header="Оценка"
    SortMemberPath="Rating" MaxRating="10" Width="100" />

<!-- Прогресс -->
<headers:ProgressColumnHeaderLeaf Header="Готовность"
    SortMemberPath="Progress" Minimum="0" Maximum="100" ShowPercentage="True" Width="140" />

<!-- Дата -->
<headers:DateColumnHeaderLeaf Header="Дата"
    SortMemberPath="Date" Format="dd.MM.yyyy" Width="110" />

<!-- Изображение -->
<headers:ImageColumnHeaderLeaf Header="Аватар"
    SortMemberPath="AvatarUrl" Stretch="UniformToFill" Width="60" />

<!-- Гиперссылка -->
<headers:HyperlinkColumnHeaderLeaf Header="Сайт"
    SortMemberPath="Website" UrlBinding="Website" Width="160" />

<!-- ComboBox -->
<headers:ComboBoxColumnHeaderLeaf Header="Статус"
    SortMemberPath="Status" ItemsSource="{Binding Statuses}"
    SelectedValuePath="Id" SelectedValueBinding="StatusId" Width="140" />

<!-- Многострочный текст -->
<headers:MultiLineColumnHeaderLeaf Header="Описание"
    SortMemberPath="Description" MaxLines="3" Width="200" />

<!-- Радиокнопки -->
<headers:RadioButtonColumnHeaderLeaf Header="Вариант"
    SortMemberPath="Option" GroupName="Opt" Options="{Binding Options}" Width="150" />
```

## 🎨 Кастомные ячейки: `CellViewModel` и `Config`

Контекстом данных (`DataContext`) ячейки является `CellViewModel`. Ключевые члены:

| Член | Тип | Описание |
| :--- | :--- | :--- |
| `RowViewModel` | `RowViewModel` | ViewModel строки (для доступа к `Item`). |
| `Item` | `object` | Объект данных строки (данные вашей модели). |
| `Value` | `string` | Строковое представление значения (для отображения). |
| `CellType` | `string` | Тип ячейки (например, `Numeric`, `ComboBox`). |
| `Config` | `Dictionary<string,object>` | Конфигурация ячейки по ключам (из свойств листа). |
| `Column` | `DataGridColumn` | Колонка `DataGrid`. |
| `IsEditing` | `bool` | Флаг режима редактирования. |
| `EditValue` | `string` | Значение в режиме редактирования. |
| `Width` | `double` | Ширина ячейки. |
| `IsCellSelected` | `bool` | Ячейка выбрана. |
| `IsLeftmostInRightPanel` | `bool` | Крайняя левая ячейка правой frozen-панели. |

### Доступ к `Config` в XAML и C#

В XAML шаблона ячейки параметры листа доступны через `{Binding Config[Key]}`:

```xml
<DataTemplate x:Key="CustomNumericTemplate">
    <StackPanel Orientation="Horizontal">
        <TextBlock Text="{Binding Value}" />
        <TextBlock Text="{Binding Config[CurrencySymbol]}" />
    </StackPanel>
</DataTemplate>
```

В C#-коде (например, в поведении) используйте типизированный доступ:

```csharp
int decimalPlaces = cellVm.GetConfig("DecimalPlaces", 2);
string format = cellVm.GetConfig<string>("Format");
```

### Методы редактирования

Для редактируемых ячеек (`Editable`, `Numeric`, `Date`, `MultiLine`) доступны методы:

- `CommitEdit()` — сохраняет `EditValue` обратно в модель данных (с конвертацией в целевой тип).
- `CancelEdit()` — отменяет редактирование и восстанавливает исходное значение.
- `IsEditing` / `EditValue` — управление режимом редактирования.

### Фабрика ячеек (`CellViewModelFactory`)

Можно зарегистрировать собственный фабричный метод для создания специализированного `CellViewModel` для конкретного типа листа:

```csharp
CellViewModelFactory.Register<MyHeaderLeaf>((rowVm, column) => new MyCellViewModel(rowVm, column));
```

## 🧰 Attached-поведения ячеек

Поведения настраивают элементы управления внутри шаблонов ячеек и применяются в XAML:

```xml
<TextBox rows:CellBehaviorBase.IsEnabled="True" rows:CellBehaviorBase.CellType="Numeric" />
```

Доступные классы:

| Класс | Назначение |
| :--- | :--- |
| `CellBehaviorBase` | Единый базовый behavior (`IsEnabled`, `CellType`). |
| `EditableCellBehavior` | Редактирование текста: клик — вход, потеря фокуса/`Enter` — сохранить, `Esc` — отмена. |
| `ComboBoxCellBehavior` | Настройка `ComboBox` из `Config` (ItemsSource, DisplayMemberPath, SelectedValuePath). |
| `NumericCellBehavior` | Ввод только чисел, форматирование по `DecimalPlaces`/`Format`/`CurrencySymbol`. |
| `RatingCellBehavior` | Генерация звёзд по `MaxRating`, обработка кликов. |

Регистрация собственных обработчиков:

```csharp
CellBehaviorBase.RegisterSetupHandler<ComboBox>(SetupComboBox);
CellBehaviorBase.RegisterEventHandler<ComboBox, MouseButtonEventArgs>("MouseDown", OnComboMouseDown);
```

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

## 🧩 Создание собственного типа ячейки

1. Создайте наследника `ColumnHeaderLeaf` с атрибутом `[CellTemplate("UnifiedCellTemplate")]` и нужными DependencyProperty:

```csharp
[CellTemplate("UnifiedCellTemplate")]
public class MyHeaderLeaf : ColumnHeaderLeaf
{
    public static readonly DependencyProperty MyOptionProperty =
        DependencyProperty.Register("MyOption", typeof(int), typeof(MyHeaderLeaf),
            new FrameworkPropertyMetadata(1));

    public int MyOption
    {
        get => (int)GetValue(MyOptionProperty);
        set => SetValue(MyOptionProperty, value);
    }
}
```

2. В `UnifiedCellTemplate.xaml` добавьте `DataTemplate` с ключом `MyCellView` и `DataTrigger` на `CellType = "My"`. Внутри шаблона параметр доступен как `{Binding Config[MyOption]}`.

3. При необходимости зарегистрируйте поведение через `CellBehaviorBase` (например, `CellBehaviorBase.RegisterSetupHandler<FrameworkElement>(...)`).

4. Колонка `MyHeaderLeaf` автоматически использует новый шаблон — свойство `CellType` формируется из имени класса без суффикса `ColumnHeaderLeaf`.

## 🛠 Примеры кастомизации

### 1. Модель данных и ViewModel

```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
    public DateTime DateAdded { get; set; }
    public double Rating { get; set; }
    public double Progress { get; set; }
    public string Website { get; set; }
    public string AvatarUrl { get; set; }
    public string Description { get; set; }
    public int StatusId { get; set; }
    public string Option { get; set; }
}

public class MainViewModel
{
    public ObservableCollection<Product> Products { get; } = new ObservableCollection<Product>
    {
        new Product { Id = 1, Name = "Товар А", Price = 1500.50m, IsActive = true,
                      DateAdded = DateTime.Today.AddDays(-3), Rating = 4.5, Progress = 78,
                      Website = "https://example.com/a", Description = "Длинное описание\nс переносом строк" },
        // ... остальные товары
    };

    public List<StatusOption> Statuses { get; } = new List<StatusOption>
    {
        new StatusOption { Id = 1, Name = "В наличии" },
        new StatusOption { Id = 2, Name = "Под заказ" },
        new StatusOption { Id = 3, Name = "Нет в наличии" }
    };

    public List<string> Options { get; } = new List<string> { "Вариант 1", "Вариант 2", "Вариант 3" };
}

public class StatusOption
{
    public int Id { get; set; }
    public string Name { get; set; }
}
```

### 2. Разметка окна со всеми типами колонок

```xml
<Window xmlns:mfgrid="clr-namespace:MakroFlexGrid.Core"
        xmlns:headers="clr-namespace:MakroFlexGrid.Headers"
        xmlns:rows="clr-namespace:MakroFlexGrid.Rows"
        Title="MakroFlexGrid — пример кастомизации">
    <Grid>
        <mfgrid:CustomDataGrid
            ItemsSource="{Binding Products}"
            GridLineBrush="LightGray"
            RowSelectionMode="Multiple"
            IsSystemColumnEnabled="True"
            SeparatorWidth="2"
            SeparatorBrush="DarkGray"
            BottomPanelHeight="28"
            BottomPanelText="Итого:"
            BottomPanelTextPosition="Top"
            PanelTextAlignment="Right"
            RowSelectedBackground="#CFE8FF">

            <!-- Колонки слева (замороженные) -->
            <mfgrid:CustomDataGrid.FrozenColumnHeaders>
                <headers:ColumnHeaderLeaf Header="№"
                    SortMemberPath="Id" SortDataType="Number"
                    CanUserSort="True" Width="60" />
                <headers:ColumnHeaderLeaf Header="Товар"
                    SortMemberPath="Name" SortDataType="Text"
                    CanUserSort="True" Width="200" />
            </mfgrid:CustomDataGrid.FrozenColumnHeaders>

            <!-- Основные колонки (прокручиваются) -->
            <mfgrid:CustomDataGrid.ScrollableColumnHeaders>
                <headers:ColumnHeaderGroup Header="Финансы" Width="280">
                    <headers:NumericColumnHeaderLeaf Header="Цена"
                        SortMemberPath="Price" SortDataType="Number"
                        CanUserSort="True" AggregateType="Sum"
                        DecimalPlaces="2" CurrencySymbol="₽"
                        AllowNegative="False" Width="140" />
                    <headers:ProgressColumnHeaderLeaf Header="Прогресс"
                        SortMemberPath="Progress" SortDataType="Number"
                        Minimum="0" Maximum="100" ShowPercentage="True"
                        Width="160" />
                </headers:ColumnHeaderGroup>

                <headers:CheckBoxColumnHeaderLeaf Header="Активен"
                    SortMemberPath="IsActive" SortDataType="Boolean"
                    CanUserSort="True" IsThreeState="False" Width="80" />

                <headers:DateColumnHeaderLeaf Header="Дата"
                    SortMemberPath="DateAdded" SortDataType="Date"
                    CanUserSort="True" Format="dd.MM.yyyy" Width="120" />

                <headers:RatingColumnHeaderLeaf Header="Оценка"
                    SortMemberPath="Rating" SortDataType="Number"
                    MaxRating="5" Width="120" />

                <headers:HyperlinkColumnHeaderLeaf Header="Сайт"
                    SortMemberPath="Website" UrlBinding="Website" Width="180" />

                <headers:ImageColumnHeaderLeaf Header="Фото"
                    SortMemberPath="AvatarUrl" Stretch="UniformToFill"
                    Width="70" />

                <headers:ComboBoxColumnHeaderLeaf Header="Статус"
                    SortMemberPath="StatusId" SortDataType="Text"
                    ItemsSource="{Binding Statuses}"
                    SelectedValuePath="Id"
                    SelectedValueBinding="StatusId" Width="140" />

                <headers:MultiLineColumnHeaderLeaf Header="Описание"
                    SortMemberPath="Description" MaxLines="3" Width="220" />
            </mfgrid:CustomDataGrid.ScrollableColumnHeaders>

            <!-- Колонки справа (замороженные) -->
            <mfgrid:CustomDataGrid.RightFrozenColumnHeaders>
                <headers:RadioButtonColumnHeaderLeaf Header="Вариант"
                    SortMemberPath="Option" GroupName="Opt"
                    Options="{Binding Options}" Width="180" />
            </mfgrid:CustomDataGrid.RightFrozenColumnHeaders>
        </mfgrid:CustomDataGrid>
    </Grid>
</Window>
```

### 3. Кастомный шаблон ячейки с `Config`

Допустим, нужна ячейка, которая показывает название товара цветным и с иконкой статуса.

```xml
<Window.Resources>
    <!-- Шаблон ячейки. DataContext = CellViewModel. Данные строки через RowViewModel.Item -->
    <DataTemplate x:Key="ProductNameCellTemplate">
        <StackPanel Orientation="Horizontal" VerticalAlignment="Center" Margin="4,0">
            <Ellipse Width="10" Height="10" Margin="0,0,6,0">
                <Ellipse.Style>
                    <Style TargetType="Ellipse">
                        <Setter Property="Fill" Value="Gray" />
                        <Style.Triggers>
                            <DataTrigger Binding="{Binding RowViewModel.Item.IsActive}" Value="True">
                                <Setter Property="Fill" Value="Green" />
                            </DataTrigger>
                        </Style.Triggers>
                    </Style>
                </Ellipse.Style>
            </Ellipse>
            <TextBlock Text="{Binding RowViewModel.Item.Name}"
                       FontWeight="Bold" VerticalAlignment="Center" />
        </StackPanel>
    </DataTemplate>

    <!-- Шаблон ячейки итога (нижняя панель). DataContext = BottomCellViewModel, свойство Value -->
    <DataTemplate x:Key="SumBottomCellTemplate">
        <StackPanel Orientation="Horizontal" HorizontalAlignment="Center">
            <TextBlock Text="Σ " FontWeight="Bold" />
            <TextBlock Text="{Binding Value}" FontWeight="Bold" />
        </StackPanel>
    </DataTemplate>
</Window.Resources>
```

Применение шаблонов к колонкам:

```xml
<headers:ColumnHeaderLeaf Header="Товар"
    SortMemberPath="Name"
    CellTemplate="{StaticResource ProductNameCellTemplate}"
    Width="200" />

<headers:NumericColumnHeaderLeaf Header="Цена"
    SortMemberPath="Price" AggregateType="Sum"
    BottomCellTemplate="{StaticResource SumBottomCellTemplate}"
    Width="140" />
```

### 4. Кастомный шаблон заголовка с `HeaderTemplate`

```xml
<Window.Resources>
    <DataTemplate x:Key="HeaderWithIconTemplate">
        <StackPanel Orientation="Horizontal" VerticalAlignment="Center">
            <TextBlock Text="🔍 " VerticalAlignment="Center" />
            <TextBlock Text="{Binding}" VerticalAlignment="Center" FontWeight="Bold" />
        </StackPanel>
    </DataTemplate>
</Window.Resources>

<!-- Применение -->
<headers:ColumnHeaderGroup Header="Поиск и фильтры"
    HeaderTemplate="{StaticResource HeaderWithIconTemplate}" Width="300">
    <headers:ColumnHeaderLeaf Header="Запрос" SortMemberPath="Query" Width="150" />
</headers:ColumnHeaderGroup>
```

### 5. Настройка фильтров через `FilterService`

```csharp
// Получаем лист колонки, по которой будем фильтровать
var priceHeader = grid.ScrollableColumnHeaders.GetBottomItems()
    .OfType<NumericColumnHeaderLeaf>()
    .FirstOrDefault(h => h.SortMemberPath == "Price");

if (priceHeader != null)
{
    // Создаём числовой фильтр диапазона
    var filter = new NumberColumnFilter(priceHeader.SortMemberPath, priceHeader.SortDataType)
    {
        Operator = FilterOperator.Between,
        FromValue = 100,
        ToValue = 1000
    };
    filter.Activate();

    // Устанавливаем фильтр через сервис
    grid.FilterService.SetFilter(priceHeader, filter);
}

// Сбросить фильтр одной колонки
grid.FilterService.ClearFilter(priceHeader);

// Сбросить все фильтры
grid.FilterService.ClearAllFilters();
```

### 6. Обработка событий

```csharp
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();

        dataGrid.RowSelected += OnRowSelected;
        dataGrid.RowDoubleClicked += OnRowDoubleClicked;
        dataGrid.CellRightClicked += OnCellRightClicked;
        dataGrid.SelectedItemsChanged += OnSelectedItemsChanged;
    }

    private void OnRowSelected(object sender, object item)
    {
        if (item is Product product)
            Title = $"Выбрано: {product.Name}";
    }

    private void OnRowDoubleClicked(object sender, object item)
    {
        if (item is Product product)
            MessageBox.Show($"Открываю карточку товара «{product.Name}»");
    }

    private void OnCellRightClicked(object sender, CellClickEventArgs e)
    {
        var menu = new ContextMenu();
        if (e.Item is Product product)
        {
            menu.Items.Add(new MenuItem { Header = $"Товар: {product.Name}" });
            menu.Items.Add(new MenuItem { Header = "Действие" });
        }
        menu.IsOpen = true;
    }

    private void OnSelectedItemsChanged(object sender, IReadOnlyCollection<object> items)
    {
        StatusBarText.Text = $"Выбрано: {items.Count}";
    }
}
```

### 7. Кастомное поведение ячейки

Создадим поведение, которое при клике по ячейке показывает значение.

```csharp
public static class ShowValueCellBehavior
{
    // При старте регистрируем обработчик настройки для TextBlock
    static ShowValueCellBehavior()
    {
        CellBehaviorBase.RegisterSetupHandler<TextBlock>(SetupTextBlock);
    }

    private static void SetupTextBlock(TextBlock textBlock)
    {
        textBlock.MouseLeftButtonUp += OnTextBlockClick;
        textBlock.Unloaded += (s, e) => textBlock.MouseLeftButtonUp -= OnTextBlockClick;
    }

    private static void OnTextBlockClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is TextBlock tb && tb.DataContext is CellViewModel cell)
        {
            MessageBox.Show($"Значение: {cell.Value}");
        }
    }
}
```

Применение в шаблоне:

```xml
<DataTemplate x:Key="ClickableCellTemplate">
    <TextBlock Text="{Binding Value}" VerticalAlignment="Center"
               Cursor="Hand" TextDecorations="Underline" Foreground="Blue" />
</DataTemplate>

<!-- Колонка, ячейки которой открывают сообщение по клику -->
<headers:ColumnHeaderLeaf Header="Кликни меня"
    SortMemberPath="Name" CellTemplate="{StaticResource ClickableCellTemplate}" Width="150" />
```

### 8. Скрытие колонок и настройка видимости

```csharp
// Скрыть колонку программно
var header = grid.ScrollableColumnHeaders.GetBottomItems()
    .FirstOrDefault(h => h.SortMemberPath == "Description");
if (header != null) header.IsVisible = false;

// Обновить заголовки и строки после изменения
grid.RefreshHeaders();
grid.RefreshRows();
```

```xml
<!-- Запрет скрытия через контекстное меню -->
<headers:ColumnHeaderLeaf Header="Товар" SortMemberPath="Name"
    CanUserHide="False" Width="200" />

<!-- Запрет фильтрации -->
<headers:ColumnHeaderLeaf Header="Код" SortMemberPath="Code"
    CanUserFilter="False" Width="100" />

<!-- Запрет перетаскивания конкретной колонки -->
<headers:ColumnHeaderLeaf Header="№" SortMemberPath="Id"
    AllowDrag="False" Width="60" />
```

### 9. Кастомизация нижней панели итогов

```xml
<Window.Resources>
    <!-- Кастомный шаблон текста панели -->
    <DataTemplate x:Key="PanelHeaderTemplate">
        <StackPanel Orientation="Horizontal">
            <TextBlock Text="📊 " FontSize="12" />
            <TextBlock Text="{Binding}" FontWeight="Bold" />
        </StackPanel>
    </DataTemplate>
</Window.Resources>

<mfgrid:CustomDataGrid
    BottomPanelHeight="30"
    BottomPanelBackground="#F0F0F0"
    ShowBottomCellBorders="True"
    BottomPanelText="Сводка по таблице"
    BottomPanelTextPosition="Top"
    PanelTextAlignment="Center"
    PanelTextPadding="6,2,6,2"
    PanelTextTemplate="{StaticResource PanelHeaderTemplate}">
    <!-- ... колонки ... -->
</mfgrid:CustomDataGrid>
```

### 10. Дополнительная строка под основной (`BottomRowTemplate`)

`BottomRowTemplate` отображает дополнительный контент снизу от основной строки. DataContext — данные строки.

```xml
<Window.Resources>
    <DataTemplate x:Key="ProductBottomRowTemplate">
        <Border Background="#FAFAFA" BorderBrush="LightGray" BorderThickness="0,0,0,1" Padding="8,4">
            <TextBlock Text="{Binding Description}"
                       TextWrapping="Wrap" FontStyle="Italic" />
        </Border>
    </DataTemplate>
</Window.Resources>

<mfgrid:CustomDataGrid
    ItemsSource="{Binding Products}"
    BottomRowTemplate="{StaticResource ProductBottomRowTemplate}">
    <!-- ... колонки ... -->
</mfgrid:CustomDataGrid>
```

### 11. Создание и использование собственного типа ячейки

Создадим тип ячейки «Рейтинг звёздами с заголовком».

**Шаг 1.** Класс листа колонки:

```csharp
using System.Windows;
using MakroFlexGrid.Headers;

[CellTemplate("UnifiedCellTemplate")]
public class StarRatingColumnHeaderLeaf : ColumnHeaderLeaf
{
    public static readonly DependencyProperty MaxRatingProperty =
        DependencyProperty.Register("MaxRating", typeof(int), typeof(StarRatingColumnHeaderLeaf),
            new FrameworkPropertyMetadata(5));

    public int MaxRating
    {
        get => (int)GetValue(MaxRatingProperty);
        set => SetValue(MaxRatingProperty, value);
    }
}
```

**Шаг 2.** Добавляем `DataTemplate` с ключом `StarRatingCellView` в `Themes/RowTemplates/UnifiedCellTemplate.xaml`:

```xml
<DataTemplate x:Key="StarRatingCellView">
    <TextBlock VerticalAlignment="Center" HorizontalAlignment="Center"
               Text="{Binding Value}" FontSize="14" />
</DataTemplate>
```

**Шаг 3.** Добавляем `DataTrigger` в `UnifiedCellTemplate` (в `DataTemplate.Triggers`):

```xml
<DataTemplate>
    <!-- ... существующие триггеры ... -->
    <DataTrigger Binding="{Binding CellType}" Value="StarRating">
        <Setter Property="ContentTemplate" Value="{StaticResource StarRatingCellView}" />
    </DataTrigger>
</DataTemplate>
```

**Шаг 4.** Используем колонку:

```xml
<headers:StarRatingColumnHeaderLeaf Header="Звёзды"
    SortMemberPath="Rating" MaxRating="5" Width="100" />
```

**Описание механизма:** свойство `CellType` автоматически формируется из имени класса без суффикса `ColumnHeaderLeaf` (т.е. `StarRating`). Универсальный шаблон по этому значению выбирает соответствующий `DataTemplate` с ключом `{CellType}CellView` через `DataTrigger`. Параметры листа (например, `MaxRating`) копируются в `CellViewModel.Config` и доступны в шаблоне как `{Binding Config[MaxRating]}`.

### 12. Выделение ячеек

Управление обводкой выбранной ячейки.

```xml
<mfgrid:CustomDataGrid
    ItemsSource="{Binding Products}"
    IsCellSelectionEnabled="True"
    CellSelectedBorderBrush="#FF8C00"
    CellSelectedBorderThickness="2">
    <!-- ... колонки ... -->
</mfgrid:CustomDataGrid>
```

```csharp
// Отключить выделение ячеек программно (останется только подсветка строки)
dataGrid.IsCellSelectionEnabled = false;

// Изменить цвет и толщину обводки выбранной ячейки
dataGrid.CellSelectedBorderBrush = new SolidColorBrush(Colors.Orange);
dataGrid.CellSelectedBorderThickness = new Thickness(3);
```

### 13. Разделители frozen-зон и скроллбар

Настройка разделителей между замороженными и прокручиваемой зонами, а также spacer'ов под скроллбаром.

```xml
<mfgrid:CustomDataGrid
    SeparatorWidth="3"
    SeparatorBrush="#2F4F4F"
    ShowScrollBarSpacers="True">
    <!-- Замороженные колонки слева -->
    <mfgrid:CustomDataGrid.FrozenColumnHeaders>
        <headers:ColumnHeaderLeaf Header="№" SortMemberPath="Id" Width="60" />
    </mfgrid:CustomDataGrid.FrozenColumnHeaders>

    <mfgrid:CustomDataGrid.ScrollableColumnHeaders>
        <headers:ColumnHeaderLeaf Header="Товар" SortMemberPath="Name" Width="200" />
    </mfgrid:CustomDataGrid.ScrollableColumnHeaders>

    <!-- Замороженные колонки справа -->
    <mfgrid:CustomDataGrid.RightFrozenColumnHeaders>
        <headers:ColumnHeaderLeaf Header="Действия" SortMemberPath="Id" Width="100" />
    </mfgrid:CustomDataGrid.RightFrozenColumnHeaders>
</mfgrid:CustomDataGrid>
```

### 14. Отложенный ресайз колонок

`IsDeferredResizeEnabled="True"` позволяет изменять ширину колонки с задержкой (значение применяется к колонке при отпускании gripper'а). Полезно для больших таблиц.

```xml
<mfgrid:CustomDataGrid
    ItemsSource="{Binding Products}"
    IsDeferredResizeEnabled="True">
    <!-- ... колонки ... -->
</mfgrid:CustomDataGrid>
```

### 15. Детали строки (`RowDetailsTemplate`)

`RowDetailsTemplate` наследуется от стандартного `DataGrid` и отображает дополнительный контент под выбранной строкой.

```xml
<Window.Resources>
    <DataTemplate x:Key="ProductDetailsTemplate">
        <StackPanel Margin="16,8">
            <TextBlock Text="{Binding Description}" TextWrapping="Wrap" />
            <TextBlock Text="{Binding Website}" Foreground="Blue" Margin="0,4,0,0" />
        </StackPanel>
    </DataTemplate>
</Window.Resources>

<mfgrid:CustomDataGrid
    ItemsSource="{Binding Products}"
    RowDetailsTemplate="{StaticResource ProductDetailsTemplate}">
    <!-- ... колонки ... -->
</mfgrid:CustomDataGrid>
```

### 16. Программная сортировка и направление

```csharp
// Найти колонку по листу заголовка
var priceHeader = grid.ScrollableColumnHeaders.GetBottomItems()
    .OfType<NumericColumnHeaderLeaf>()
    .FirstOrDefault(h => h.SortMemberPath == "Price");

if (priceHeader != null)
{
    // Задать направление сортировки
    priceHeader.SortDirection = ListSortDirection.Ascending;
    priceHeader.CanUserSort = true;

    // Запустить сортировку по синхронизированной колонке DataGrid
    if (priceHeader.SyncColumn != null)
        grid.PerformSort(priceHeader.SyncColumn);
}
```

> **Примечание:** `SyncColumn` — внутренняя колонка `DataGrid`, созданная из листа заголовка. Её можно получить через публичный метод `grid.GetColumnHeaderItem(column)` (обратная связь) или через `SyncColumn` у листа.

### 17. Настройка системной колонки и зон заголовков

```xml
<mfgrid:CustomDataGrid
    IsSystemColumnEnabled="True"
    RowSelectionMode="Multiple">
    <!-- Системная колонка (треугольник выделения) показывается слева -->
</mfgrid:CustomDataGrid>
```

```csharp
// Проверить количество замороженных колонок
int leftCount = grid.LeftFrozenColumnsCount;
int rightCount = grid.RightFrozenColumnsCount;

// Скрыть/показать системную колонку
grid.IsSystemColumnEnabled = false;
```

### 18. Перестроение заголовков и строк после изменений

После программного изменения коллекций заголовков (добавление/удаление/скрытие колонок) вызовите синхронизацию:

```csharp
// Добавили новую колонку в скроллируемую зону
var newHeader = new ColumnHeaderLeaf
{
    Header = "Новая",
    SortMemberPath = "Name",
    Width = 150
};
grid.ScrollableColumnHeaders.Add(newHeader);

// Синхронизируем заголовки с DataGrid.Columns
grid.SyncColumnsWithHeaders();

// Обновить строки (пересоздание ячеек)
grid.RefreshRows();
```

### 19. Создание замороженных колонок

`MakroFlexGrid` разделяет колонки на **три функциональные зоны**. Замороженные колонки не прокручиваются по горизонтали и остаются видимыми всегда.

**Зоны заголовков:**
- `FrozenColumnHeaders` — колонки, **закреплённые слева**.
- `ScrollableColumnHeaders` — основная **прокручиваемая** область.
- `RightFrozenColumnHeaders` — колонки, **закреплённые справа**.

Количество замороженных колонок определяется автоматически по количеству элементов в соответствующих коллекциях.

```xml
<mfgrid:CustomDataGrid ItemsSource="{Binding Products}">

    <!-- 1) Замороженные колонки слева (всегда видны) -->
    <mfgrid:CustomDataGrid.FrozenColumnHeaders>
        <headers:ColumnHeaderLeaf Header="№"
            SortMemberPath="Id" Width="60" />
        <headers:ColumnHeaderLeaf Header="Товар"
            SortMemberPath="Name" Width="200" />
    </mfgrid:CustomDataGrid.FrozenColumnHeaders>

    <!-- 2) Прокручиваемые колонки (горизонтальный скролл) -->
    <mfgrid:CustomDataGrid.ScrollableColumnHeaders>
        <headers:ColumnHeaderLeaf Header="Цена"
            SortMemberPath="Price" Width="120" />
        <headers:ColumnHeaderLeaf Header="Дата"
            SortMemberPath="DateAdded" Width="120" />
        <headers:ColumnHeaderLeaf Header="Описание"
            SortMemberPath="Description" Width="220" />
    </mfgrid:CustomDataGrid.ScrollableColumnHeaders>

    <!-- 3) Замороженные колонки справа (всегда видны) -->
    <mfgrid:CustomDataGrid.RightFrozenColumnHeaders>
        <headers:ColumnHeaderLeaf Header="Действия"
            SortMemberPath="Id" Width="100" />
    </mfgrid:CustomDataGrid.RightFrozenColumnHeaders>

</mfgrid:CustomDataGrid>
```

**Программное добавление замороженной колонки:**

```csharp
// Добавить колонку в левую frozen-зону
var leftHeader = new ColumnHeaderLeaf
{
    Header = "Код",
    SortMemberPath = "Code",
    Width = 80
};
grid.FrozenColumnHeaders.Add(leftHeader);

// Добавить колонку в правую frozen-зону
var rightHeader = new ColumnHeaderLeaf
{
    Header = "Статус",
    SortMemberPath = "Status",
    Width = 100
};
grid.RightFrozenColumnHeaders.Add(rightHeader);

// Синхронизировать с DataGrid.Columns и обновить строки
grid.SyncColumnsWithHeaders();
grid.RefreshRows();

// Узнать текущее количество замороженных колонок
int leftCount = grid.LeftFrozenColumnsCount;   // из FrozenColumnHeaders
int rightCount = grid.RightFrozenColumnsCount; // из RightFrozenColumnHeaders
```

**Советы:**
- Заморозка колонок **слева** обычно используется для ключевых полей (ID, наименование), чтобы они оставались видны при горизонтальной прокрутке.
- Заморозка **справа** удобна для колонок действий (кнопки, ссылки), которые должны быть всегда под рукой.
- Разделитель между зонами настраивается через `SeparatorWidth` и `SeparatorBrush` (см. пример 13).
- Колонки можно перемещать между зонами через Drag-and-Drop (свойство `AllowCrossSectionDrag` на листе, по умолчанию `true`).

## 📖 Документация

Подробная документация находится в [`docs/`](MakroFlexGrid/docs):

- [`UserGuide.md`](MakroFlexGrid/docs/UserGuide.md) — руководство пользователя, быстрый старт и примеры.
- [`Architecture.md`](MakroFlexGrid/docs/Architecture.md) — высокоуровневая архитектура и потоки взаимодействия.
- [`TechnicalDoc.md`](MakroFlexGrid/docs/TechnicalDoc.md) — техническое описание компонентов.

## 🛠 Требования

- Windows
- .NET 8, .NET 9 или .NET 10 (с поддержкой WPF)
- Visual Studio 2022 или актуальная версия `dotnet` CLI

## 📄 Лицензия

Проект распространяется под лицензией **MIT**. Подробности см. в LICENSE-файле пакета.
