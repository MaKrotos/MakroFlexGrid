# MakroFlexGrid — Overview

High-performance WPF DataGrid with row virtualization, multi-level headers, filtering, sorting, aggregates and a wide range of cell types.

`MakroFlexGrid` is built on top of the standard `DataGrid` and is designed for displaying large amounts of data with support for complex hierarchical headers, frozen columns (left and right), multi-level filtering and automatic calculation of aggregates in the bottom panel.

![MakroFlexGrid](https://raw.githubusercontent.com/MaKrotos/MakroFlexGrid/master/preview/image.png)

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

## ➡️ Next steps

- [Events, selection modes and property reference](reference.md)
- [Custom cells, behaviors and customization examples](customization.md)
- [Filtering, sorting and aggregates](data-operations.md)
- [Requirements and license](requirements.md)