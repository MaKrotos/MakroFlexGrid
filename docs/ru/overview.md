# MakroFlexGrid — Обзор

Высокопроизводительный WPF DataGrid с виртуализацией строк, многоуровневыми заголовками, фильтрацией, сортировкой, агрегатами и широким набором типов ячеек.

`MakroFlexGrid` построен поверх стандартного `DataGrid` и предназначен для отображения больших объемов данных с поддержкой сложных иерархических заголовков, замороженных колонок (слева и справа), многоуровневой фильтрации и автоматического расчета агрегатов в нижней панели.

![MakroFlexGrid](https://raw.githubusercontent.com/MaKrotos/MakroFlexGrid/master/preview/image.png)

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

## ➡️ Следующие шаги

- [События, режимы выбора и справочник свойств](reference.md)
- [Кастомные ячейки, поведения и примеры кастомизации](customization.md)
- [Фильтрация, сортировка и агрегаты](data-operations.md)
- [Требования и лицензия](requirements.md)