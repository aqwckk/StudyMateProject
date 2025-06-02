using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyMateTest.Models.Drawing.DrawingElements
{
    public interface IDrawingElement
    {
        void Draw(SKCanvas canvas);
        SKRect Bounds { get; }
    }
}
