using System.Globalization;

namespace StudyMateProject.Converters
{
    /// <summary>
    /// Конвертер для проверки, находится ли дата в прошлом
    /// </summary>
    public class DatePastConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dateTime)
            {
                return dateTime < DateTime.Now;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Конвертер для проверки, не пуста ли строка
    /// </summary>
    public class StringNotEmptyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                return !string.IsNullOrEmpty(str);
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Конвертер для определения типа заметки
    /// </summary>
    public class TypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Models.TextNote)
            {
                return "TextNote";
            }
            else if (value is Models.GraphicNote)
            {
                return "GraphicNote";
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Конвертер для работы с цветами
    /// </summary>
    public class ColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string currentColor && parameter is string parameterColor)
            {
                return currentColor.Equals(parameterColor, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isChecked && isChecked && parameter is string parameterColor)
            {
                return parameterColor;
            }

            return string.Empty;
        }
    }

    /// <summary>
    /// Конвертер для выбора инструментов
    /// </summary>
    public class ToolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string currentTool && parameter is string parameterTool)
            {
                return currentTool.Equals(parameterTool, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isChecked && isChecked && parameter is string parameterTool)
            {
                return parameterTool;
            }

            return string.Empty;
        }
    }

    /// <summary>
    /// Конвертер для работы с командой экспорта с параметрами
    /// </summary>
    public class ExportCommandConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 && values[0] is ICommand command && values[1] is string format)
            {
                return new Command(() => command.Execute(format));
            }

            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Конвертер для создания кисти рамки в зависимости от статуса выполнения
    /// </summary>
    public class CompletionStatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isCompleted)
            {
                return isCompleted ? new SolidColorBrush(Colors.Gray) : new SolidColorBrush(Colors.DodgerBlue);
            }

            return new SolidColorBrush(Colors.DodgerBlue);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Селектор шаблона на основе типа заметки
    /// </summary>
    public class NoteTemplateSelector : DataTemplateSelector
    {
        public DataTemplate TextNoteTemplate { get; set; }
        public DataTemplate GraphicNoteTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (item is Models.TextNote)
            {
                return TextNoteTemplate;
            }
            else if (item is Models.GraphicNote)
            {
                return GraphicNoteTemplate;
            }

            return null;
        }
    }
}