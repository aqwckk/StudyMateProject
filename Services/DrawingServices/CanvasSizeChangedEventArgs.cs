using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyMateTest.Services.DrawingServices
{
    // класс для передачи данных о новых размерах холста в событиях (наследуется от EventArgs)
    public class CanvasSizeChangedEventArgs : EventArgs
    {
        public float Width { get; } // свойство только для чтения для новой ширины холста
        public float Height { get; } // свойство только для чтения для новой высоты холста

        // конструктор для создания аргументов события с новыми размерами холста
        public CanvasSizeChangedEventArgs(float width, float height)
        {
            Width = width; // устанавливаем ширину
            Height = height; // устанавливаем высоту
        }
    }
}
