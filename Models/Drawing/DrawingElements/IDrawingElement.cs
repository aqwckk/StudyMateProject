using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyMateTest.Models.Drawing.DrawingElements
{
    public interface IDrawingElement // интерфейс для все графических элементов
    {
        void Draw(SKCanvas canvas); // метод для рисования на холсте
        SKRect Bounds { get; } // границы объекта
    }
}
