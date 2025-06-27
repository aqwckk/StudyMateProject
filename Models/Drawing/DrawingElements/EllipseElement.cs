using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyMateTest.Models.Drawing.DrawingElements
{
    // класс для описания эллипса
    public class EllipseElement : DrawingElement // наследуемся от DrawingElement
    {
        public SKRect Rect { get; } // реализуем свойство для прямоугольника, внутри которого будем рисовать эллипс

        public EllipseElement(SKRect rect, SKPaint paint) : base(paint) // конструктор для создания эллипса с заданным описанным прямоугольником и краской
        {
            Rect = rect;
        }

        public override void Draw(SKCanvas canvas) // "рисуем" эллипс на холсте
        {
            canvas.DrawOval(Rect, Paint); // вписываем овал в заданный прямоугольник заданной краски
        }

        public override SKRect Bounds => Rect; // реализуем перегрузку базового абстрактного свойства Bounds
    }
}
