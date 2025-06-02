using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using SkiaSharp;
using StudyMateTest.Models.Drawing;
using StudyMateTest.ViewModels;

namespace StudyMateTest.Converters
{
    // конвертер для изменения цвета кнопки при включении заливки
    public class BoolToBackgroundColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Colors.LightBlue : Colors.LightGray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
