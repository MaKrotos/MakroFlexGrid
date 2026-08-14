# User Guide: MaKroFlexGrid

`MaKroFlexGrid` — это продвинутый WPF-контрол на базе `DataGrid`, предназначенный для отображения больших объемов данных с поддержкой сложных иерархических заголовков, замороженных колонок (слева и справа), многоуровневой фильтрации и автоматического расчета агрегатов в нижней панели.

## 🚀 Быстрый старт

### Подключение в XAML
Добавьте пространство имен вашего проекта и разместите контрол в разметке:

```xml
<Window xmlns:mfgrid="clr-namespace:MakroFlexGrid.Core">
    <mfgrid:CustomDataGrid 
        ItemsSource="{Binding MyDataList}" 
        GridLineBrush="LightGray"
        BottomPanelHeight="30" />
</Window>
```

## 🛠 Основные возможности

### 1. Иерархические заголовки и Заморозка
В отличие от стандартного `DataGrid`, `MaKroFlexGrid` разделяет заголовки на три функциональные зоны. Это позволяет создавать сложные интерфейсы с фиксированными областями.

**Зоны заголовков:**
- **`FrozenColumnHeaders`**: Колонки, которые всегда остаются слева (заморожены).
- **`ScrollableColumnHeaders`**: Основная область с данными, которая прокручивается горизонтально.
- **`RightFrozenColumnHeaders`**: Колонки, которые всегда остаются справа (заморожены).

**Пример создания иерархии (Группа $\rightarrow$ Листья):**
```xml
<mfgrid:CustomDataGrid.ScrollableColumnHeaders>
    <headers:ColumnHeaderGroup Header="Информация о пользователе" Width="300">
        <headers:ColumnHeaderLeaf Header="Имя" SortMemberPath="Name" SortDataType="Text" Width="150" />
        <headers:ColumnHeaderLeaf Header="Email" SortMemberPath="Email" SortDataType="Text" Width="150" />
    </headers:ColumnHeaderGroup>
</mfgrid:CustomDataGrid.ScrollableColumnHeaders>
```

### 2. Фильтрация данных
Контрол поддерживает встроенную систему фильтрации. 

**Настройка колонки для фильтрации:**
- `CanUserFilter="True"`: Разрешает пользователю открывать интерфейс фильтра.
- `SortDataType`: Определяет тип данных, от которого зависит выбор UI-контрола фильтра (`Text`, `Number`, `Date`, `DateTime`).

**Типы фильтров:**
- **Текстовый**: Поиск по подстроке.
- **Числовой**: Диапазоны (от и до).
- **Дата/Время**: Выбор интервалов дат.

### 3. Сортировка
Сортировка осуществляется через клик по заголовку. 
- `CanUserSort="True"`: Включает возможность сортировки.
- `SortMemberPath`: Путь к свойству объекта данных для сортировки.

### 4. Нижняя панель (Агрегаты)
Нижняя панель позволяет отображать итоговые значения по колонкам.

**Расчет агрегатов:**
Установите свойство `AggregateType` в `ColumnHeaderLeaf`, чтобы выбрать тип расчета:
- `Sum`: Сумма значений.
- `Average`: Среднее значение.
- `Min` / `Max`: Минимальное или максимальное значение.
- `Count`: Количество записей.

**Настройка внешнего вида панели:**
- `BottomPanelBackground`: Цвет фона панели.
- `BottomPanelText`: Текст, отображаемый в левой части панели (например, "Итого:").
- `BottomPanelTextPosition`: Позиция текста (например, `Bottom`).
- `PanelTextAlignment`: Выравнивание текста.
- `PanelTextPadding`: Отступы текста.
- `ShowBottomCellBorders`: Отображение или скрытие границ ячеек в итогах.
- `PanelTextTemplate`: Позволяет полностью изменить вид текста итога.

## 🎨 Кастомизация внешнего вида

Контрол предоставляет широкие возможности для изменения визуального представления через `DataTemplate`. 

### 1. Как вставлять шаблоны в ячейки (`CellTemplate`)
Для каждой колонки (`ColumnHeaderLeaf`) вы можете определить свой `CellTemplate`. Это позволяет заменить стандартный текстовый вывод на любой WPF-элемент.

**Важно:** Внутри шаблона ячейки контекстом данных (`DataContext`) является объект `CellViewModel`. Чтобы добраться до данных вашей строки, используйте путь `RowViewModel.Item`.

**Пример: отображение Boolean-значения через CheckBox**
```xml
<!-- 1. Определяем шаблон в ресурсах -->
<DataTemplate x:Key="BooleanCellTemplate">
    <CheckBox 
        Margin="2"
        HorizontalAlignment="Center" 
        VerticalAlignment="Center" 
        IsChecked="{Binding RowViewModel.Item.IsActive, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" />
</DataTemplate>

<!-- 2. Применяем шаблон к конкретной колонке -->
<headers:ColumnHeaderLeaf 
    Header="Активен" 
    CellTemplate="{StaticResource BooleanCellTemplate}" 
    SortMemberPath="IsActive" 
    Width="80" />
```

### 2. Шаблоны итоговых ячеек (`BottomCellTemplate`)
Позволяют изменить вид ячейки в нижней панели (агрегатов). Контекстом данных здесь также является `BottomCellViewModel`, где свойство `Value` содержит результат расчета агрегата.
```xml
<DataTemplate x:Key="BooleanBottomCellTemplate">
    <TextBlock Text="{Binding Value}" HorizontalAlignment="Center" />
</DataTemplate>

<!-- Применение к колонке -->
<headers:ColumnHeaderLeaf Header="Активен" BottomCellTemplate="{StaticResource BooleanBottomCellTemplate}" ... />
```

### 3. Шаблоны заголовков (`HeaderTemplate`)
Позволяют добавить кнопки, иконки или сложное оформление прямо в шапку таблицы. Контекстом данных здесь является сам объект заголовка (обычно строка `Header`).
```xml
<headers:ColumnHeaderGroup Header="Сводка" HeaderTemplate="{StaticResource HeaderColoredTemplate}">
   ...
</headers:ColumnHeaderGroup>
```

### 4. Детали строки (`RowDetailsTemplate`)
Для отображения дополнительной информации о выбранной строке используйте `RowDetailsTemplate`. Контент появится под основной строкой при её активации.

## ⚙️ Справочник свойств CustomDataGrid

| Свойство | Описание | Значение по умолчанию |
| :--- | :--- | :--- |
| `GridLineBrush` | Цвет линий сетки | `LightGray` |
| `SeparatorWidth` | Ширина разделителя между frozen-зонами | `0` |
| `SeparatorBrush` | Цвет разделителя | `Transparent` |
| `IsDeferredResizeEnabled` | Отложенный ресайз колонок (для производительности) | `False` |
| `ShowScrollBarSpacers` | Показывать пустые зоны над скроллбаром (чтобы он не перекрывал frozen-зоны) | `False` |
| `AllowDrag` | Разрешить перетаскивание заголовков | `True` |
| `IsSystemColumnEnabled` | Включить системные колонки | `False` |
