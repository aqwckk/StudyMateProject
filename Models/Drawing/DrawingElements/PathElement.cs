using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyMateTest.Models.Drawing.DrawingElements
{
    // класс для описания произвольного пути (кривой линии)
    public class PathElement : DrawingElement // наследуемся от DrawingElement
    {
        public SKPath Path { get; } // свойство только для чтения для хранения пути

        public PathElement(SKPath path, SKPaint paint) : base(paint) // конструктор для создания элемента пути с заданным путем и краской
        {
            Path = SimplifyPath(path); // упрощаем переданный путь для оптимизации производительности
        }

        // метод для упрощения пути путем уменьшения количества точек
        private SKPath SimplifyPath(SKPath originalPath, float tolerance = 2f)
        {
            SKPath simplifiedPath = new SKPath(); // создаем новый упрощенный путь

            if (originalPath.PointCount <= 10) // если точек мало (10 или меньше)
            {
                simplifiedPath.AddPath(originalPath); // просто копируем оригинальный путь
                return simplifiedPath;
            }

            using (SKPathMeasure measure = new SKPathMeasure(originalPath)) // создаем измеритель пути для работы с длиной и позициями
            {
                float length = measure.Length; // получаем общую длину пути
                int numPoints = Math.Max(2, (int)(length / (tolerance * 5))); // вычисляем оптимальное количество точек на основе длины и допуска

                if (numPoints > originalPath.PointCount) // если вычисленное количество больше исходного
                {
                    simplifiedPath.AddPath(originalPath); // используем оригинальный путь
                    return simplifiedPath;
                }

                bool isFirst = true; // флаг для отслеживания первой точки
                for (int i = 0; i < numPoints; i++) // проходим по оптимальному количеству точек
                {
                    float distance = i * length / (numPoints - 1); // вычисляем расстояние вдоль пути для текущей точки
                    if (measure.GetPosition(distance, out SKPoint point)) // получаем координаты точки на заданном расстоянии
                    {
                        if (isFirst) // если это первая точка
                        {
                            simplifiedPath.MoveTo(point); // перемещаемся к точке (начало пути)
                            isFirst = false;
                        }
                        else
                        {
                            simplifiedPath.LineTo(point); // рисуем линию к точке
                        }
                    }
                }
            }

            return simplifiedPath; // возвращаем упрощенный путь
        }

        public override void Draw(SKCanvas canvas) // переопределяем метод рисования на холсте
        {
            canvas.DrawPath(Path, Paint); // рисуем путь на холсте заданной краской
        }

        public override SKRect Bounds => Path.Bounds; // переопределяем свойство границ - используем встроенное свойство Bounds пути
    }
}
