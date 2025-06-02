using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyMateTest.Models.Drawing.DrawingElements
{
    public class EllipseElement : DrawingElement 
    {
        public SKRect Rect { get; }

        public EllipseElement(SKRect rect, SKPaint paint) : base(paint)
        {
            Rect = rect;
        }

        public override void Draw(SKCanvas canvas)
        {
            canvas.DrawOval(Rect, Paint);
        }

        public override SKRect Bounds => Rect;
    }
}
