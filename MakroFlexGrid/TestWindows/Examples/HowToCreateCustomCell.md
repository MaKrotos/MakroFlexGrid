# Как создать кастомный тип ячейки

## Быстрый старт (3 шага)

### Шаг 1: Создать `ColumnHeaderLeaf` с атрибутом `[CellTemplate]`

```csharp
// Headers/MyColumnHeaderLeaf.cs
using System.Windows;
using System.Windows.Controls;

namespace MakroFlexGrid.Headers
{
    /// <summary>
    /// Атрибут [CellTemplate] автоматически загрузит шаблон
    /// из Themes/RowTemplates/MyCellTemplate.xaml
    /// </summary>
    [CellTemplate("MyCellTemplate")]
    public class MyColumnHeaderLeaf : ColumnHeaderLeaf
    {
        static MyColumnHeaderLeaf()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(MyColumnHeaderLeaf),
                new FrameworkPropertyMetadata(typeof(MyColumnHeaderLeaf)));
        }

        // Dependency Properties автоматически попадут в CellViewModel.Config
        public static readonly DependencyProperty MyOptionProperty =
            DependencyProperty.Register("MyOption", typeof(string),
                typeof(MyColumnHeaderLeaf), new FrameworkPropertyMetadata("default"));

        public string MyOption
        {
            get => (string)GetValue(MyOptionProperty);
            set => SetValue(MyOptionProperty, value);
        }
    }
}
```

### Шаг 2: Создать XAML-шаблон

```xml
<!-- Themes/RowTemplates/MyCellTemplate.xaml -->
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:rows="clr-namespace:MakroFlexGrid.Rows">

    <!-- x:Key должен совпадать с параметром атрибута [CellTemplate] -->
    <DataTemplate x:Key="MyCellTemplate">
        <Grid VerticalAlignment="Stretch" Background="Transparent">
            
            <!-- Режим просмотра -->
            <TextBlock Padding="4,0,4,0" VerticalAlignment="Center"
                       Text="{Binding Value}" TextWrapping="NoWrap">
                <TextBlock.Style>
                    <Style TargetType="TextBlock">
                        <Style.Triggers>
                            <DataTrigger Binding="{Binding IsEditing}" Value="True">
                                <Setter Property="Visibility" Value="Collapsed" />
                            </DataTrigger>
                        </Style.Triggers>
                    </Style>
                </TextBlock.Style>
            </TextBlock>

            <!-- Режим редактирования с CellBehaviorBase -->
            <TextBox Margin="4" VerticalAlignment="Stretch"
                     VerticalContentAlignment="Center" BorderThickness="1"
                     Text="{Binding EditValue, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
                     rows:CellBehaviorBase.IsEnabled="True"
                     rows:CellBehaviorBase.CellType="MyCell">
                <TextBox.Style>
                    <Style TargetType="TextBox">
                        <Setter Property="Visibility" Value="Collapsed" />
                        <Style.Triggers>
                            <DataTrigger Binding="{Binding IsEditing}" Value="True">
                                <Setter Property="Visibility" Value="Visible" />
                            </DataTrigger>
                        </Style.Triggers>
                    </Style>
                </TextBox.Style>
            </TextBox>
        </Grid>
    </DataTemplate>
</ResourceDictionary>
```

### Шаг 3: Зарегистрировать обработчики через `CellBehaviorBase`

```csharp
// App.xaml.cs или отдельный класс
public partial class App : Application
{
    public App()
    {
        // Регистрируем обработчик для TextBox с CellType="MyCell"
        CellBehaviorBase.RegisterSetupHandler<TextBox>(textBox =>
        {
            textBox.PreviewTextInput += (s, e) => {
                // Кастомная логика
            };
            textBox.LostFocus += (s, e) => {
                // Кастомная логика
            };
        });
    }
}
```

## Итого: что нужно для нового типа ячейки

| Компонент | Обязательно | Описание |
|---|---|---|
| `XxxColumnHeaderLeaf.cs` | ✅ Да | Класс с атрибутом `[CellTemplate]` и Dependency Properties |
| `XxxCellTemplate.xaml` | ✅ Да | DataTemplate с визуальным деревом |
| Регистрация в `CellBehaviorBase` | 🔶 Если нужно поведение | Обработчики для элементов управления |
| Изменение `RowTemplate.xaml` | ❌ Нет | Автоматически через `[CellTemplate]` |
| Изменение `CellViewModel` | ❌ Нет | Config заполняется автоматически |
| Изменение `RowViewModel` | ❌ Нет | Фабрика используется автоматически |

## Примеры готовых типов

Смотрите файлы в этой папке:
- [`NumericColumnHeaderLeaf.cs`](NumericColumnHeaderLeaf.cs) — пример колонки
- [`NumericCellTemplate.xaml`](NumericCellTemplate.xaml) — пример шаблона
- [`NumericCellBehaviorRegistration.cs`](NumericCellBehaviorRegistration.cs) — пример регистрации поведения

## Использование в коде

```csharp
// В гриде:
var grid = new CustomDataGrid();

// Создаём кастомную колонку
var myColumn = new MyColumnHeaderLeaf
{
    Header = "Моя колонка",
    Width = 150,
    MyOption = "кастомное значение"
};

// Добавляем в грид
grid.Columns.Add(myColumn);
```

## Как это работает (архитектура)

```
MyColumnHeaderLeaf ([CellTemplate])
    │
    ├── Атрибут → ColumnHeaderLeaf.LoadCellTemplateFromAttribute()
    │              загружает XAML и устанавливает CellTemplate
    │
    ├── Dependency Properties → CellViewModel.ApplyHeaderConfig()
    │                           копирует в CellViewModel.Config
    │
    └── CellBehaviorBase.IsEnabled="True" в XAML
        → CellBehaviorBase.SetupElement()
          → вызывает зарегистрированные обработчики