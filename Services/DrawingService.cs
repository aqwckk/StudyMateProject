using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;
using StudyMateTest.Models.Drawing;
using StudyMateTest.Models.Drawing.DrawingElements;

namespace StudyMateTest.Services
{
    public class DrawingService : IDrawingService
    {
        // списки для хранения элементов в истории
        private List<IDrawingElement> _elements = new List<IDrawingElement>();
        private Stack<DrawingAction>_undoStack = new Stack<DrawingAction>();
        private Stack<DrawingAction> _redoStack = new Stack<DrawingAction>();

        // текущее состояние рисования
        private DrawingTool _currentTool = DrawingTool.Pen;
        private SKPoint _startPoint;
        private SKPoint _lastPoint;
        private SKPath _currentPath;
        private bool _isDragging = false;

        // текущие настройки кисти
        private SKColor _strokeColor = SKColors.Black;
        private float _strokeWidth = 5;
        private bool _isFilled = false;

        // события
        public event EventHandler CanUndoRedoChanged;
        public event EventHandler DrawingChanged;

        private bool _isBatchOperation = false;
        private bool _hasChangedDuringBatch = false;

        // свойства для проверки возможности отмены/повтора
        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;

        public void BeginBatchOperation() 
        {
            _isBatchOperation = true;
            _hasChangedDuringBatch = false;
        }

        public void EndBatchOperation() 
        {
            _isBatchOperation = false;
            if (_hasChangedDuringBatch) 
            {
                OnDrawingChanged();
                _hasChangedDuringBatch = false;
            }
        }

        // создание кисти с текущими настройками
        private SKPaint CreatePaint() 
        {
            bool supportsFill = _currentTool == DrawingTool.Rectangle ||
                                _currentTool == DrawingTool.Square ||
                                _currentTool == DrawingTool.Ellipse ||
                                _currentTool == DrawingTool.Circle ||
                                _currentTool == DrawingTool.Triangle;

            return new SKPaint()
            {
                Color = _strokeColor,
                StrokeWidth = _strokeWidth,
                IsAntialias = true,
                Style = (supportsFill && _isFilled) ? SKPaintStyle.Fill : SKPaintStyle.Stroke,
                StrokeCap = SKStrokeCap.Round,
                StrokeJoin = SKStrokeJoin.Round
            };
        }

        // метод рисования на холсте
        public void Draw(SKCanvas canvas) 
        {
            canvas.Clear(SKColors.White);

            int count = _elements.Count;
            for (int i = 0; i < count; i++) 
            {
                _elements[i].Draw(canvas);
            }

            // рисуем текущий элемент в процессе создания
            if (_isDragging) 
            {
                SKPaint paint = CreatePaint();

                switch (_currentTool) 
                {
                    case DrawingTool.Pen:
                        if (_currentPath != null) 
                        {
                            canvas.DrawPath(_currentPath, paint);
                        }
                        break;
                    case DrawingTool.Line:
                        if (_startPoint != null && _lastPoint != null) 
                        {
                            canvas.DrawLine(_startPoint, _lastPoint, paint);
                        }
                        break;
                    case DrawingTool.Rectangle:
                    case DrawingTool.Square:
                        if (_startPoint != null && _lastPoint != null) 
                        {
                            SKRect rect = CalculateRect(_startPoint, _lastPoint, _currentTool == DrawingTool.Square);
                            canvas.DrawRect(rect, paint);
                        }
                        break;
                    case DrawingTool.Ellipse:
                        case DrawingTool.Circle:
                        if (_startPoint != null && _lastPoint != null) 
                        {
                            SKRect rect = CalculateRect(_startPoint, _lastPoint, _currentTool == DrawingTool.Circle);
                            canvas.DrawOval(rect, paint);
                        }
                        break;
                        case DrawingTool.Triangle:
                        if (_startPoint != null && _lastPoint != null) 
                        {
                            SKPoint[] points = CalculateTrianglePoints(_startPoint, _lastPoint);
                            using (SKPath path = new SKPath()) 
                            {
                                path.MoveTo(points[0]);
                                path.LineTo(points[1]);
                                path.LineTo(points[2]);
                                path.Close();
                                canvas.DrawPath(path, paint);
                            }
                        }
                        break;
                }
            }
        }

        private SKRect CalculateRect(SKPoint start, SKPoint end, bool isSquareorCircle) 
        {
            float left = Math.Min(start.X, end.X);
            float top = Math.Min(start.Y, end.Y);
            float right = Math.Max(start.X, end.X);
            float bottom = Math.Max (start.Y, end.Y);

            if (isSquareorCircle) 
            {
                float size = Math.Max(right - left, bottom - top);
                if (end.X < start.X)
                    left = right - size;
                else
                    right = left + size;

                if(end.Y < start.Y) 
                    top = bottom - size;
                else 
                    bottom = top + size;
            }

            return new SKRect(left, top, right, bottom);
        }

        private SKPoint[] CalculateTrianglePoints(SKPoint start, SKPoint end) 
        {
            // создание равнобедренного треугольника
            float width = end.X - start.X;
            float height = end.Y - start.Y;

            SKPoint top = new SKPoint(start.X + width / 2, start.Y);
            SKPoint bottomLeft = new SKPoint(start.X, start.Y + height);
            SKPoint bottomRight = new SKPoint(end.X, end.Y);

            return new SKPoint[] { top, bottomLeft, bottomRight };
        }

        public void HandleTouchStart(SKPoint point) 
        {
            _startPoint = point;
            _lastPoint = point;
            _isDragging = true;

            if (_currentTool == DrawingTool.Pen)
            {
                _currentPath = new SKPath();
                _currentPath.MoveTo(point);
            }
            
            OnDrawingChanged();
        }

        public void HandleTouchMove(SKPoint point) 
        {
            if (!_isDragging)
                return;

            _lastPoint = point;

            if (_currentTool == DrawingTool.Pen && _currentPath != null) 
                _currentPath.LineTo(point);

            OnDrawingChanged();
        }

        public void HandleTouchEnd(SKPoint point) 
        {
            if (!_isDragging)
                return;
            _lastPoint = point;
            _isDragging = false;
            int index = _elements.Count;

            IDrawingElement newElement = null;
            SKPaint paint = CreatePaint();

            switch (_currentTool) 
            {
                case DrawingTool.Pen:
                    if (_currentPath != null) 
                    {
                        _currentPath.LineTo(point);
                        newElement = new PathElement(_currentPath, paint);
                        _currentPath = null;
                    }
                    break;

                case DrawingTool.Line:
                    newElement = new LineElement(_startPoint, point, paint);
                    break;

                case DrawingTool.Rectangle:
                case DrawingTool.Square:
                    SKRect rectRect = CalculateRect(_startPoint, point, _currentTool == DrawingTool.Square);
                    newElement = new RectangleElement(rectRect, paint);
                    break;
                case DrawingTool.Ellipse:
                case DrawingTool.Circle:
                    SKRect ellipseRect = CalculateRect(_startPoint, point, _currentTool == DrawingTool.Circle);
                    newElement = new EllipseElement(ellipseRect, paint);
                    break;
                case DrawingTool.Triangle:
                    SKPoint[] trianglePoints = CalculateTrianglePoints(_startPoint, point);
                    newElement = new TriangleElement(trianglePoints[0], trianglePoints[1], trianglePoints[2], paint);
                    break;
            }
            if (newElement != null) 
            {
                _undoStack.Push(new DrawingAction
                {
                    Type = DrawingAction.ActionType.Add,
                    Element = newElement,
                    Index = index
                });
                _elements.Add(newElement);
                _redoStack.Clear();
                OnCanUndoRedoChanged();
            }
            OnDrawingChanged();
        }

        

        private bool ColorMatch(SKColor color1, SKColor color2, int tolerance = 10) 
        {
            return Math.Abs(color1.Red - color2.Red) <= tolerance &&
                   Math.Abs(color1.Green - color2.Green) <= tolerance &&
                   Math.Abs(color1.Blue - color2.Blue) <= tolerance &&
                   Math.Abs(color1.Alpha - color2.Alpha) <= tolerance;
        }

        public void SetCurrentTool(DrawingTool tool) 
        {
            _currentTool = tool;
        }

        public void SetStrokeWidth(float width) 
        {
            _strokeWidth = Math.Max(1, width);
        }

        public void SetStrokeColor(SKColor color) 
        {
            _strokeColor = color;
        }

        public bool SetFillMode(bool isFilled) 
        {
            _isFilled = isFilled;
            return _isFilled;
        }

        public void Undo() 
        {
            if (!CanUndo)
                return;
            BeginBatchOperation();
            try
            {
                DrawingAction action = _undoStack.Pop();

                switch (action.Type)
                {
                    case DrawingAction.ActionType.Add:
                        _elements.RemoveAt(action.Index);

                        _redoStack.Push(new DrawingAction
                        {
                            Type = DrawingAction.ActionType.Add,
                            Element = action.Element,
                            Index = action.Index
                        });
                        break;
                    case DrawingAction.ActionType.Remove:
                        _elements.Insert(action.Index, action.Element);

                        _redoStack.Push(new DrawingAction
                        {
                            Type = DrawingAction.ActionType.Remove,
                            Element = action.Element,
                            Index = action.Index
                        });
                        break;

                    case DrawingAction.ActionType.Clear:
                        _elements.AddRange(action.ClearedElements);

                        _redoStack.Push(new DrawingAction
                        {
                            Type = DrawingAction.ActionType.Clear,
                            ClearedElements = new List<IDrawingElement>(action.ClearedElements)
                        });
                        break;
                }
            }
            finally 
            {
                EndBatchOperation();
                OnCanUndoRedoChanged();
                OnDrawingChanged();
            }        
        }

        public void Redo() 
        {
            if (!CanRedo) 
                return;

            BeginBatchOperation();
            try
            {
                DrawingAction action = _redoStack.Pop();

                switch (action.Type)
                {
                    case DrawingAction.ActionType.Add:
                        if (action.Index >= 0 && action.Index <= _elements.Count)
                            _elements.Insert(action.Index, action.Element);
                        else
                            _elements.Add(action.Element);

                            _undoStack.Push(new DrawingAction
                            {
                                Type = DrawingAction.ActionType.Add,
                                Element = action.Element,
                                Index = action.Index
                            });
                        break;

                    case DrawingAction.ActionType.Remove:
                        if(action.Index >= 0 && action.Index < _elements.Count)
                            _elements.RemoveAt(action.Index);

                        _undoStack.Push(new DrawingAction
                        {
                            Type = DrawingAction.ActionType.Remove,
                            Element = action.Element,
                            Index = action.Index
                        });
                        break;

                    case DrawingAction.ActionType.Clear:
                        List<IDrawingElement> elementsToSave = new List<IDrawingElement>(_elements);
                        _elements.Clear();

                        _undoStack.Push(new DrawingAction
                        {
                            Type = DrawingAction.ActionType.Clear,
                            ClearedElements = elementsToSave
                        });
                        break;
                }
            }
            finally 
            {
                EndBatchOperation();
                OnCanUndoRedoChanged();
                OnDrawingChanged();
            }
        }

        public void Clear() 
        {
            if (_elements.Count == 0)
                return;

            BeginBatchOperation();

            try
            {
                _undoStack.Push(new DrawingAction
                {
                    Type = DrawingAction.ActionType.Clear,
                    ClearedElements = new List<IDrawingElement>(_elements)
                });

                _elements.Clear();
                _redoStack.Clear();
            }
            finally 
            {
                EndBatchOperation();
                OnCanUndoRedoChanged();
                OnDrawingChanged();
            }
        }

        public async Task<byte[]> SaveAsPngAsync()
        {
            int width = 800;
            int height = 600;
            if (_elements.Count > 0)
            {
                SKRect bounds = SKRect.Empty;
                foreach (IDrawingElement element in _elements)
                {
                    if (bounds.IsEmpty)
                    {
                        bounds = element.Bounds;
                    }
                    else
                    {
                        float left = Math.Min(bounds.Left, element.Bounds.Left);
                        float top = Math.Min(bounds.Top, element.Bounds.Top);
                        float right = Math.Max(bounds.Right, element.Bounds.Right);
                        float bottom = Math.Max(bounds.Bottom, element.Bounds.Bottom);

                        bounds = new SKRect(left, top, right, bottom);
                    }
                }

                bounds = new SKRect(
                    bounds.Left - 10,
                    bounds.Top - 10,
                    bounds.Right + 10,
                    bounds.Bottom + 10
                );

                width = Math.Max(width, (int)bounds.Width);
                height = Math.Max(height, (int)bounds.Height);
            }

            SKImageInfo info = new SKImageInfo(width, height);
            using (SKSurface surface = SKSurface.Create(info))
            {
                Draw(surface.Canvas);
                using (SKImage image = surface.Snapshot())
                using (SKData data = image.Encode(SKEncodedImageFormat.Png, 100))
                {
                    return data.ToArray();
                }
            }
        }

        protected virtual void OnCanUndoRedoChanged() 
        {
            CanUndoRedoChanged?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnDrawingChanged() 
        {
            if (_isBatchOperation) 
            {
                _hasChangedDuringBatch = true;
                return;
            }

            DrawingChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
