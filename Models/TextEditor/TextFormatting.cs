using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyMateTest.Models.TextEditor
{
    public class TextFormatting // класс для хранения информации о форматировании текста
    {
        public bool IsBold { get; set; } // булево для жирности
        public bool IsItalic { get; set; } // булево для курсива
        public string FontFamily { get; set; } = "Arial"; // строка для семейства шрифтов
        public double FontSize { get; set; } = 14; // double для размера шрифта
        public string TextColor { get; set; } = "#000000"; // строка для цвета текста
        public string BackgroundColor { get; set; } = "#FFFFFF"; // строка для цвета фона
        public TextAlignment Alignment { get; set; } = TextAlignment.Left; // выравнивание текста
        public double LineHeight { get; set; } = 1.5; // double для междустрочный интервал
        public double ParagraphSpacing { get; set; } = 0; // double для красной строки
        public double Indent { get; set; } = 0; // double для отступа

    }
}
