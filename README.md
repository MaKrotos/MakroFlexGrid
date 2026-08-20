> **🌍 Language:** [**English**](#english) · [**Русский**](#русский)

---

# English

# MakroFlexGrid

High-performance WPF DataGrid with row virtualization, multi-level headers, filtering, sorting, aggregates and a wide range of cell types.

`MakroFlexGrid` is built on top of the standard `DataGrid` and is designed for displaying large amounts of data with support for complex hierarchical headers, frozen columns (left and right), multi-level filtering and automatic calculation of aggregates in the bottom panel.

![MakroFlexGrid](https://raw.githubusercontent.com/MaKrotos/MakroFlexGrid/master/preview/image.png)

## 🚀 Features

- **Row virtualization** — efficient work with large datasets (`UnifiedRowsPresenter`).
- **Hierarchical headers** — grouping columns into multiple levels (`ColumnHeaderGroup` → `ColumnHeaderLeaf`).
- **Column freezing** — independent zones: left, the main scrollable area, and right.
- **Filtering** — built-in filter system: text, numeric (ranges), by date and time.
- **Sorting** — click on the header with optimized typed comparators.
- **Bottom aggregate panel** — automatic calculation of `Sum`, `Average`, `Min`, `Max`, `Count`.
- **Column Drag-and-Drop** — column reordering with visual indicators.
- **Wide range of cell types** — text, numbers, dates, images, checkboxes, radio buttons, combo boxes, ratings, progress, hyperlinks and more.
- **Deep customization** — via `DataTemplate` for cells, headers and totals.
- **Localization** — support for multiple resource languages.

## 📦 Installation

```bash
dotnet add package MakroFlexGrid
```

Target platform **Windows** and WPF are required: `net8.0-windows`, `net9.0-windows` or `net10.0-windows`.

## 📖 Documentation

The full documentation is organized in [`docs/`](https://github.com/MaKrotos/MakroFlexGrid/tree/master/docs):

- [**Overview & Quick Start**](https://github.com/MaKrotos/MakroFlexGrid/tree/master/docs/en/overview.md) — features, installation and first steps.
- [**Reference**](https://github.com/MaKrotos/MakroFlexGrid/tree/master/docs/en/reference.md) — events, selection modes, `CustomDataGrid` property reference, column settings and cell types.
- [**Customization**](https://github.com/MaKrotos/MakroFlexGrid/tree/master/docs/en/customization.md) — `CellViewModel`/`Config`, behaviors, custom cell types and 19 examples.
- [**Filtering, Sorting & Aggregates**](https://github.com/MaKrotos/MakroFlexGrid/tree/master/docs/en/data-operations.md) — data operations reference.
- [**Requirements & License**](https://github.com/MaKrotos/MakroFlexGrid/tree/master/docs/en/requirements.md) — system requirements and license.

## 🛠 Requirements

- Windows
- .NET 8, .NET 9 or .NET 10 (with WPF support)
- Visual Studio 2022 or a current version of the `dotnet` CLI

## 📄 License

The project is distributed under the **MIT** license. See the LICENSE file of the package for details.

# Русский

---

# MakroFlexGrid

Высокопроизводительный WPF DataGrid с виртуализацией строк, многоуровневыми заголовками, фильтрацией, сортировкой, агрегатами и широким набором типов ячеек.

`MakroFlexGrid` построен поверх стандартного `DataGrid` и предназначен для отображения больших объемов данных с поддержкой сложных иерархических заголовков, замороженных колонок (слева и справа), многоуровневой фильтрации и автоматического расчета агрегатов в нижней панели.

![MakroFlexGrid](https://raw.githubusercontent.com/MaKrotos/MakroFlexGrid/master/preview/image.png)

## 🚀 Возможности

- **Виртуализация строк** — эффективная работа с большими наборами данных (`UnifiedRowsPresenter`).
- **Иерархические заголовки** — группировка колонок в несколько уровней (`ColumnHeaderGroup` → `ColumnHeaderLeaf`).
- **Заморозка колонок** — независимые зоны: слева, основная прокручиваемая область и справа.
- **Фильтрация** — встроенная система фильтров: текстовые, числовые (диапазоны), по дате и времени.
- **Сортировка** — клик по заголовку с оптимизированными типизированными компараторами.
- **Нижняя панель агрегатов** — автоматический расчет `Sum`, `Average`, `Min`, `Max`, `Count`.
- **Drag-and-Drop колонок** — перестановка колонок с визуальными индикаторами.
- **Широкий набор типов ячеек** — текст, числа, даты, изображения, чекбоксы, радиокнопки, комбобоксы, рейтинги, прогресс, гиперссылки и др.
- **Глубокая кастомизация** — через `DataTemplate` для ячеек, заголовков и итогов.
- **Локализация** — поддержка нескольких языков ресурсов.

## 📦 Установка

```bash
dotnet add package MakroFlexGrid
```

Требуется целевая платформа **Windows** и использование WPF: `net8.0-windows`, `net9.0-windows` или `net10.0-windows`.

## 📖 Документация

Полная документация организована в [`docs/`](https://github.com/MaKrotos/MakroFlexGrid/tree/master/docs):

- [**Обзор и быстрый старт**](https://github.com/MaKrotos/MakroFlexGrid/tree/master/docs/ru/overview.md) — возможности, установка и первые шаги.
- [**Справочник**](https://github.com/MaKrotos/MakroFlexGrid/tree/master/docs/ru/reference.md) — события, режимы выбора, справочник свойств `CustomDataGrid`, настройки колонок и типы ячеек.
- [**Кастомизация**](https://github.com/MaKrotos/MakroFlexGrid/tree/master/docs/ru/customization.md) — `CellViewModel`/`Config`, поведения, собственные типы ячеек и 19 примеров.
- [**Фильтрация, сортировка и агрегаты**](https://github.com/MaKrotos/MakroFlexGrid/tree/master/docs/ru/data-operations.md) — справочник операций с данными.
- [**Требования и лицензия**](https://github.com/MaKrotos/MakroFlexGrid/tree/master/docs/ru/requirements.md) — системные требования и лицензия.

## 🛠 Требования

- Windows
- .NET 8, .NET 9 или .NET 10 (с поддержкой WPF)
- Visual Studio 2022 или актуальная версия `dotnet` CLI

## 📄 Лицензия

Проект распространяется под лицензией **MIT**. Подробности см. в LICENSE-файле пакета.
