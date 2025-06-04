using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using SkiaSharp;
using StudyMateTest.Models.Drawing;
using StudyMateTest.ViewModels;

namespace StudyMateTest.Converters
{
    // конвертер для подсветки выбранного инструмента
    public class ToolToBackgroundColorConverter : IValueConverter 
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) 
        {
            if (value is DrawingTool selectedTool && parameter is DrawingTool buttonTool)
                return selectedTool == buttonTool ? Colors.LightBlue : Colors.LightGray;
            return Colors.LightGray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) 
        {
            throw new NotImplementedException();
        }
    }

}
