using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyMateTest.Models.Drawing.DrawingElements
{
    public class PathElement : DrawingElement 
    {
        public SKPath Path { get; }

        public PathElement(SKPath path, SKPaint paint) : base(paint)
        {
            Path = SimplifyPath(path);
        }

        private SKPath SimplifyPath(SKPath originalPath, float tolerance = 2f) 
        {
            SKPath simplifiedPath = new SKPath();

            if (originalPath.PointCount <= 10) 
            {
                simplifiedPath.AddPath(originalPath);
                return simplifiedPath;
            }

            using (SKPathMeasure measure = new SKPathMeasure(originalPath)) 
            {
                float length = measure.Length;
                int numPoints = Math.Max(2, (int)(length / (tolerance * 5)));

                if (numPoints > originalPath.PointCount) 
                {
                    simplifiedPath.AddPath(originalPath);
                    return simplifiedPath;
                }

                bool isFirst = true;
                for (int i = 0; i < numPoints; i++) 
                {
                    float distance = i * length / (numPoints - 1);
                    if (measure.GetPosition(distance, out SKPoint point)) 
                    {
                        if (isFirst)
                        {
                            simplifiedPath.MoveTo(point);
                            isFirst = false;
                        }
                        else 
                        {
                            simplifiedPath.LineTo(point);
                        }
                    }
                }
            }

            return simplifiedPath;
        }

        public override void Draw(SKCanvas canvas)
        {
            canvas.DrawPath(Path, Paint);
        }

        public override SKRect Bounds => Path.Bounds;
    }
}
