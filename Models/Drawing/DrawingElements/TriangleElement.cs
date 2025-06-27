using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace StudyMateTest.Models.Drawing.DrawingElements
{
    // класс для описания треугольника
    public class TriangleElement : DrawingElement // наследуемся от DrawingElement
    {
        public SKPoint Point1 { get; } // свойство только для чтения для первой вершины треугольника
        public SKPoint Point2 { get; } // свойство только для чтения для второй вершины треугольника
        public SKPoint Point3 { get; } // свойство только для чтения для третьей вершины треугольника

        public TriangleElement(SKPoint p1, SKPoint p2, SKPoint p3, SKPaint paint) : base(paint) // конструктор для создания треугольника с тремя вершинами и краской
        {
            Point1 = p1; // первая вершина - переданная p1
            Point2 = p2; // вторая вершина - переданная p2
            Point3 = p3; // третья вершина - переданная p3
        }

        public override void Draw(SKCanvas canvas) // переопределяем метод рисования на холсте
        {
            using (SKPath path = new SKPath()) // создаем временный путь (using для автоматического освобождения ресурсов)
            {
                path.MoveTo(Point1); // перемещаемся к первой вершине
                path.LineTo(Point2); // рисуем линию ко второй вершине
                path.LineTo(Point3); // рисуем линию к третьей вершине
                path.Close(); // замыкаем путь (рисуем линию обратно к первой вершине)
                canvas.DrawPath(path, Paint); // рисуем замкнутый путь (треугольник) заданной краской
            }
        }

        public override SKRect Bounds // переопределяем свойство для границ элемента
        {
            get
            {
                // вычисляем описывающий прямоугольник для треугольника
                float minX = Math.Min(Math.Min(Point1.X, Point2.X), Point3.X); // минимальная X координата из всех вершин
                float minY = Math.Min(Math.Min(Point1.Y, Point2.Y), Point3.Y); // минимальная Y координата из всех вершин
                float maxX = Math.Max(Math.Max(Point1.X, Point2.X), Point3.X); // максимальная X координата из всех вершин
                float maxY = Math.Max(Math.Max(Point1.Y, Point2.Y), Point3.Y); // максимальная Y координата из всех вершин

                return new SKRect(minX, minY, maxX, maxY); // создаем прямоугольник, описывающий треугольник
            }
        }
    }
}
