using System.Globalization;
using System.Reflection;
using System.Resources;

namespace MakroFlexGrid.Utilities
{
    /// <summary>
    /// Менеджер локализации, обеспечивающий доступ к ресурсам в зависимости от системных настроек.
    /// </summary>
    public static class LocalizationManager
    {
        private static readonly ResourceManager _resourceManager;

        static LocalizationManager()
        {
            // Инициализируем ResourceManager для файла ресурсов Resources.resx
            // Предполагается, что файлы ресурсов будут находиться в корне или специальной папке
            _resourceManager = new ResourceManager("MakroFlexGrid.Properties.Resources", Assembly.GetExecutingAssembly());
        }

        /// <summary>
        /// Возвращает переведенную строку по ключу.
        /// </summary>
        /// <param name="key">Ключ ресурса.</param>
        /// <param name="defaultValue">Значение по умолчанию, если ресурс не найден.</param>
        /// <returns>Переведенная строка.</returns>
        public static string GetString(string key, string defaultValue = "")
        {
            try
            {
                return _resourceManager.GetString(key) ?? defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Метод для принудительного обновления культуры (если потребуется поддержка смены языка без перезапуска).
        /// </summary>
        public static void SetCulture(string cultureName)
        {
            var culture = new CultureInfo(cultureName);
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
        }
    }
}
