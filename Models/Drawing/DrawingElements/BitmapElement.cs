using SkiaSharp;
using System;

namespace StudyMateTest.Models.Drawing.DrawingElements
{
    // модель для хранения битовой карты (png) для сохранений
    public class BitmapElement : IDrawingElement
    {
        private readonly SKBitmap _bitmap; // переменная только для чтения для стандартной битовой карты SkiaSharp
        private readonly SKRect _destinationRect; // переменная только для чтения для прямоугольника назначения (область сохранения)
        private bool _isDisposed = false; // логическая переменная для отслеживания был ли ресурс удален после использования (disposed)

        public BitmapElement(SKBitmap bitmap, SKRect destinationRect) // конструктор с параметрами для создания битовой карты
        {
            _bitmap = bitmap?.Copy() ?? throw new ArgumentNullException(nameof(bitmap)); // null-coalescing операция: если копия переданной битовой карты не null, то записываем ее в _bitmap
                                                                                         // иначе выбрасываем исключение ArgumentNullException
            _destinationRect = destinationRect; // прямоугольник назначения - переданный destinationRect
        }

        public SKRect Bounds => _destinationRect; // геттер-свойство для границ графического файла

        public void Draw(SKCanvas canvas) // метод для реализации интерфейса IDrawingElement
        {
            if (_bitmap != null && !_isDisposed) // если битовая карта не пустая и не была disposed
            {
                using (var paint = new SKPaint()) // using - использование ресурсов, которые не управляются стандартным garbage collector в C#, а затем их удаление/отключение (dispose)
                {
                    paint.FilterQuality = SKFilterQuality.High; // качество фильтра - высокое
                    paint.IsAntialias = true; // включаем антиалиасинг для краски (уменьшение эффекта лесенки для более плавной и приятной картинки)
                    canvas.DrawBitmap(_bitmap, _destinationRect, paint); // рисуем битовую карту на текущем холсте
                }
            }
        }

        public void Dispose() // метод для удаления ресурса после использования
        {
            if (!_isDisposed) // если не был disposed
            {
                _bitmap?.Dispose(); // выполняем dispose, если не null
                _isDisposed = true; // был удален - истина
            }
        }
    }
}