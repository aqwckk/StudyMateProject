using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyMateTest.Models.Drawing.DrawingElements
{
    // абстрактный класс для элементов рисования
    public abstract class DrawingElement : IDrawingElement // реализуем интерфейс IDrawingElement
    {
        public SKPaint Paint { get; } // гет-свойство для краски SKPaint
        protected DrawingElement(SKPaint paint) // protected (доступен только для наследников) конструктор для создания элемента с заданной краской
        {
            Paint = new SKPaint(); // создаем новую краску
            // изменяем ее в соответствии с переданной краской
            Paint.Color = paint.Color;
            Paint.StrokeWidth = paint.StrokeWidth;
            Paint.IsAntialias = paint.IsAntialias;
            Paint.Style = paint.Style;
            Paint.StrokeCap = paint.StrokeCap;
            Paint.StrokeJoin = paint.StrokeJoin;
        }

        public abstract void Draw(SKCanvas canvas); // абстрактный метод для рисования на холсте
        public abstract SKRect Bounds { get; } // абстрактное свойство для границ элемента
    }
}
