using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyMateTest.Models.Drawing.DrawingElements
{
    // класс для описания линии
    public class LineElement : DrawingElement // наследуемся от DrawingElement
    {
        public SKPoint Start { get; } // свойство только для чтения для начальной точки линии
        public SKPoint End { get; } // свойство только для чтения для конечной точки линии

        public LineElement(SKPoint start, SKPoint end, SKPaint paint) : base(paint) // конструктор для создания линии с заданными начальной и конечной точкой и краской
        {
            Start = start; // начальная точка - переданная start
            End = end; // конечная точка - переданная end
        }

        public override void Draw(SKCanvas canvas) // переопределяем метод рисования на холсте
        {
            canvas.DrawLine(Start, End, Paint); // рисуем линию от начальной до конечной точки заданной краской
        }

        public override SKRect Bounds // переопределяем свойство для границ элемента
        {
            get
            {
                return new SKRect( // создаем прямоугольник, описывающий линию
                    Math.Min(Start.X, End.X), // левая граница - минимальная X координата
                    Math.Min(Start.Y, End.Y), // верхняя граница - минимальная Y координата
                    Math.Max(Start.X, End.X), // правая граница - максимальная X координата
                    Math.Max(Start.Y, End.Y)  // нижняя граница - максимальная Y координата
                    );
            }
        }
    }
}
