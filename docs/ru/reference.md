# MakroFlexGrid — Справочник

В этом документе описаны события, режимы выбора строк, справочник свойств `CustomDataGrid`, настройки колонок и доступные типы ячеек.

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

## ➡️ Следующие шаги

- [Обзор и быстрый старт](overview.md)
- [Кастомные ячейки, поведения и примеры кастомизации](customization.md)
- [Фильтрация, сортировка и агрегаты](data-operations.md)