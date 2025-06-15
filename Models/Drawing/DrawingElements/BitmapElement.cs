using SkiaSharp;
using System;

namespace StudyMateTest.Models.Drawing.DrawingElements
{
    public class BitmapElement : IDrawingElement
    {
        private readonly SKBitmap _bitmap;
        private readonly SKRect _destinationRect;
        private bool _isDisposed = false;

        public BitmapElement(SKBitmap bitmap, SKRect destinationRect)
        {
            _bitmap = bitmap?.Copy() ?? throw new ArgumentNullException(nameof(bitmap));
            _destinationRect = destinationRect;
        }

        public SKRect Bounds => _destinationRect;

        public void Draw(SKCanvas canvas)
        {
            if (_bitmap != null && !_isDisposed)
            {
                using (var paint = new SKPaint())
                {
                    paint.FilterQuality = SKFilterQuality.High;
                    paint.IsAntialias = true;
                    canvas.DrawBitmap(_bitmap, _destinationRect, paint);
                }
            }
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _bitmap?.Dispose();
                _isDisposed = true;
            }
        }
    }
}