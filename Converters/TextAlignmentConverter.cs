using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyMateTest.Converters
{
    // конвертер для конвертирования кастомного (нашего) выравнивания в стандартное выравнивание MAUI
    class TextAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Models.TextEditor.TextAlignment textAlignment)
            {
                return textAlignment switch
                {
                    Models.TextEditor.TextAlignment.Left => Microsoft.Maui.TextAlignment.Start,
                    Models.TextEditor.TextAlignment.Center => Microsoft.Maui.TextAlignment.Center,
                    Models.TextEditor.TextAlignment.Right => Microsoft.Maui.TextAlignment.End,
                    _ => Microsoft.Maui.TextAlignment.Start
                };
            }
            return Microsoft.Maui.TextAlignment.Start;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Microsoft.Maui.TextAlignment alignment)
            {
                return alignment switch
                {
                    Microsoft.Maui.TextAlignment.Start => Models.TextEditor.TextAlignment.Left,
                    Microsoft.Maui.TextAlignment.Center => Models.TextEditor.TextAlignment.Center,
                    Microsoft.Maui.TextAlignment.End => Models.TextEditor.TextAlignment.Right,
                    _ => Models.TextEditor.TextAlignment.Left
                };
            }
            return Models.TextEditor.TextAlignment.Left;
        }
    }
}
