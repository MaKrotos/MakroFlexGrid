# MakroFlexGrid

Высокопроизводительный WPF DataGrid с виртуализацией строк, многоуровневыми заголовками, фильтрацией, сортировкой, агрегатами и широким набором типов ячеек.

`MakroFlexGrid` построен поверх стандартного `DataGrid` и предназначен для отображения больших объемов данных с поддержкой сложных иерархических заголовков, замороженных колонок (слева и справа), многоуровневой фильтрации и автоматического расчета агрегатов в нижней панели.

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