using MakroFlexGrid.Core;
using MakroFlexGrid.Utilities;
using System.Windows;
using System.Windows.Controls;

namespace MakroFlexGrid.Headers
{
    /// <summary>
    /// Специальный элемент заголовка для системной колонки.
    /// При клике открывает контекстное меню с CheckBox для скрытия/показа корневых заголовков.
    /// </summary>
    public class SystemColumnHeaderItem : ColumnHeaderItem
    {
        public SystemColumnHeaderItem(CustomDataGrid ownerGrid)
        {
            Header = string.Empty;
            Width = 15;
            MinWidth = 15;
            MaxWidth = 15;

            // Явно устанавливаем OwnerGrid, т.к. этот элемент не добавляется
            // в ColumnHeaderCollection, а создаётся напрямую в GenerateHeaderElements()
            OwnerGrid = ownerGrid;

            // Создаем системного дочернего наследника
            var child = new SystemLeafHeaderItem(ownerGrid);
            Children.Add(child);
        }

        protected override void OnOwnerGridChanged(CustomDataGrid oldOwner, CustomDataGrid newOwner)
        {
            base.OnOwnerGridChanged(oldOwner, newOwner);
            SetupContextMenu();
        }

        private void SetupContextMenu()
        {
            var menu = new ContextMenu();

            // Перестраиваем содержимое меню при каждом открытии,
            // чтобы учесть актуальное состояние коллекций заголовков
            menu.Opened += (s, e) => RebuildMenuItems(menu);

            HeaderElement.ContextMenu = menu;
        }

        private void RebuildMenuItems(ContextMenu menu)
        {
            menu.Items.Clear();

            menu.Items.Add(new MenuItem
            {
                Header = LocalizationManager.GetString("ColumnVisibility", "Column Visibility"),
                IsEnabled = false,
                FontWeight = FontWeights.Bold
            });
            menu.Items.Add(new Separator());

            if (OwnerGrid != null)
            {
                AddHeaderItemsToMenu(menu.Items, OwnerGrid.FrozenColumnHeaders, LocalizationManager.GetString("FrozenLeft", "Frozen (Left):"));
                AddHeaderItemsToMenu(menu.Items, OwnerGrid.ScrollableColumnHeaders, LocalizationManager.GetString("MainColumns", "Main:"));
                AddHeaderItemsToMenu(menu.Items, OwnerGrid.RightFrozenColumnHeaders, LocalizationManager.GetString("FrozenRight", "Frozen (Right):"));
            }
        }

        internal static void AddHeaderItemsToMenu(ItemCollection menuItems, ColumnHeaderCollection headers, string sectionName)
        {
            bool hasVisibleItems = false;
            foreach (var header in headers)
            {
                if (header is SystemColumnHeaderItem || header is SystemLeafHeaderItem)
                    continue;
                hasVisibleItems = true;
                break;
            }

            if (!hasVisibleItems)
                return;

            menuItems.Add(new MenuItem
            {
                Header = sectionName,
                IsEnabled = false,
                FontStyle = FontStyles.Italic
            });

            foreach (var header in headers)
            {
                if (header is SystemColumnHeaderItem || header is SystemLeafHeaderItem)
                    continue;

                var headerItem = header; // захват для замыкания

                // Пропускаем колонки, для которых скрытие запрещено
                if (!headerItem.CanUserHide)
                    continue;

                var checkBox = new CheckBox
                {
                    Content = headerItem.Header?.ToString() ?? LocalizationManager.GetString("NoName", "(no name)"),
                    IsChecked = headerItem.IsVisible,
                    Margin = new Thickness(16, 0, 0, 0),
                    Tag = headerItem
                };
                checkBox.Checked += (s, e) => ToggleHeaderVisibility(headerItem, true);
                checkBox.Unchecked += (s, e) => ToggleHeaderVisibility(headerItem, false);

                menuItems.Add(new MenuItem
                {
                    Header = checkBox,
                    StaysOpenOnClick = true
                });
            }
        }

        private static void ToggleHeaderVisibility(ColumnHeaderItem header, bool isVisible)
        {
            if (header.IsVisible == isVisible)
                return;

            header.IsVisible = isVisible;

            // При показе корневого заголовка восстанавливаем видимость всех дочерних заголовков
            if (isVisible && header.HasChildren)
            {
                foreach (var child in header.Children)
                {
                    if (!child.IsVisible)
                        child.IsVisible = true;
                }
            }
        }
    }

    public class SystemLeafHeaderItem : ColumnHeaderItem
    {
        public SystemLeafHeaderItem(CustomDataGrid ownerGrid)
        {
            Header = string.Empty;
            Width = 15;
            MinWidth = 15;
            MaxWidth = 15;
        }

        protected override void OnOwnerGridChanged(CustomDataGrid oldOwner, CustomDataGrid newOwner)
        {
            base.OnOwnerGridChanged(oldOwner, newOwner);
            SetupContextMenu();
        }

        private void SetupContextMenu()
        {
            var menu = new ContextMenu();

            // Перестраиваем содержимое меню при каждом открытии,
            // чтобы учесть актуальное состояние коллекций заголовков
            menu.Opened += (s, e) => RebuildMenuItems(menu);

            HeaderElement.ContextMenu = menu;
        }

        private void RebuildMenuItems(ContextMenu menu)
        {
            menu.Items.Clear();

            menu.Items.Add(new MenuItem
            {
                Header = LocalizationManager.GetString("ColumnVisibility", "Column Visibility"),
                IsEnabled = false,
                FontWeight = FontWeights.Bold
            });
            menu.Items.Add(new Separator());

            if (OwnerGrid != null)
            {
                SystemColumnHeaderItem.AddHeaderItemsToMenu(menu.Items, OwnerGrid.FrozenColumnHeaders, LocalizationManager.GetString("FrozenLeft", "Frozen (Left):"));
                SystemColumnHeaderItem.AddHeaderItemsToMenu(menu.Items, OwnerGrid.ScrollableColumnHeaders, LocalizationManager.GetString("MainColumns", "Main:"));
                SystemColumnHeaderItem.AddHeaderItemsToMenu(menu.Items, OwnerGrid.RightFrozenColumnHeaders, LocalizationManager.GetString("FrozenRight", "Frozen (Right):"));
            }
        }
    }
}
