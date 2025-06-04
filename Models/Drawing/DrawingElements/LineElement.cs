using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyMateTest.Models.Drawing.DrawingElements
{
    public class LineElement : DrawingElement
    {
        public SKPoint Start { get; }
        public SKPoint End { get; }

        public LineElement(SKPoint start, SKPoint end, SKPaint paint) : base(paint)
        {
            Start = start;
            End = end;
        }

        public override void Draw(SKCanvas canvas)
        {
            canvas.DrawLine(Start, End, Paint);
        }

        public override SKRect Bounds
        {
            get 
            {
                return new SKRect(
                    Math.Min(Start.X, End.X),
                    Math.Min(Start.Y, End.Y),
                    Math.Max(Start.X, End.X),
                    Math.Max(Start.Y, End.Y)
                    );
            }
        }

    }
}
