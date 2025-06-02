using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyMateTest.Models.Drawing.DrawingElements
{
    public abstract class DrawingElement : IDrawingElement 
    {
        public SKPaint Paint { get; }
        protected DrawingElement(SKPaint paint)
        {
            Paint = new SKPaint();
            Paint.Color = paint.Color;
            Paint.StrokeWidth = paint.StrokeWidth;
            Paint.IsAntialias = paint.IsAntialias;
            Paint.Style = paint.Style;
            Paint.StrokeCap = paint.StrokeCap;
            Paint.StrokeJoin = paint.StrokeJoin;
        }

        public abstract void Draw(SKCanvas canvas);
        public abstract SKRect Bounds { get; }
    }
}
