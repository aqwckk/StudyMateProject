using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyMateTest.Models.TextEditor
{
    public class TextFormatting
    {
        public bool IsBold { get; set; }
        public bool IsItalic { get; set; }
        public string FontFamily { get; set; } = "Arial";
        public double FontSize { get; set; } = 14;
        public string TextColor { get; set; } = "#000000";
        public string BackgroundColor { get; set; } = "#FFFFFF";
        public TextAlignment Alignment { get; set; } = TextAlignment.Left;
        public double LineHeight { get; set; } = 1.5;
        public double ParagraphSpacing { get; set; } = 0;
        public double Indent { get; set; } = 0;

    }
}
