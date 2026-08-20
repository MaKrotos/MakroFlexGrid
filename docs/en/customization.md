# MakroFlexGrid — Customization

This document explains the `CellViewModel`/`Config` model, attached cell behaviors, how to create a custom cell type, and provides 19 step-by-step customization examples.

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

## ➡️ Next steps

- [Overview and quick start](overview.md)
- [Events, selection modes and property reference](reference.md)
- [Filtering, sorting and aggregates](data-operations.md)
- [Requirements and license](requirements.md)