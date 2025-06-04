using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyMateTest.Models.Drawing.DrawingElements
{
    public class RectangleElement : DrawingElement 
    {
        public SKRect Rect { get; }

        public RectangleElement(SKRect rect, SKPaint paint) : base(paint)
        {
            Rect = rect;
        }

        public override void Draw(SKCanvas canvas)
        {
            canvas.DrawRect(Rect, Paint);
        }

        public override SKRect Bounds => Rect;
    }
}
