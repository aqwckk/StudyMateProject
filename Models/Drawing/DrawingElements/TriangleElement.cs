using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace StudyMateTest.Models.Drawing.DrawingElements
{
    public class TriangleElement : DrawingElement 
    {
        public SKPoint Point1 { get; }

        public SKPoint Point2 { get; }

        public SKPoint Point3 { get; }

        public TriangleElement(SKPoint p1, SKPoint p2, SKPoint p3, SKPaint paint) : base(paint)
        {
            Point1 = p1;
            Point2 = p2;
            Point3 = p3;
        }

        public override void Draw(SKCanvas canvas)
        {
            using (SKPath path = new SKPath()) 
            {
                path.MoveTo(Point1);
                path.LineTo(Point2);
                path.LineTo(Point3);
                path.Close();
                canvas.DrawPath(path, Paint);
            }
        }

        public override SKRect Bounds 
        {
            get 
            {
                float minX = Math.Min(Math.Min(Point1.X, Point2.X), Point3.X);
                float minY = Math.Min(Math.Min(Point1.Y, Point2.Y), Point3.Y);
                float maxX = Math.Max(Math.Max(Point1.X, Point2.X), Point3.X);
                float maxY = Math.Max(Math.Max(Point1.Y, Point2.Y), Point3.Y);

                return new SKRect(minX, minY, maxX, maxY);
            }
        }
    }
}
