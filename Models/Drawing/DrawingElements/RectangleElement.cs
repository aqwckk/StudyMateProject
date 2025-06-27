using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyMateTest.Models.Drawing.DrawingElements
{
    // класс для описания прямоугольника
    public class RectangleElement : DrawingElement // наследуемся от DrawingElement
    {
        public SKRect Rect { get; } // свойство только для чтения для хранения прямоугольника

        public RectangleElement(SKRect rect, SKPaint paint) : base(paint) // конструктор для создания прямоугольника с заданными размерами и краской
        {
            Rect = rect; // прямоугольник - переданный rect
        }

        public override void Draw(SKCanvas canvas) // переопределяем метод рисования на холсте
        {
            canvas.DrawRect(Rect, Paint); // рисуем прямоугольник на холсте заданной краской
        }

        public override SKRect Bounds => Rect; // переопределяем свойство границ - возвращаем сам прямоугольник
    }
}
