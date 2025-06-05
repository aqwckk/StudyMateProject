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

        // настройки холста и масштабирования 
        private float _canvasWidth = 800;
        private float _canvasHeight = 600;
        private float _zoom = 1.0f;
        private SKPoint _panOffset = new SKPoint(0, 0);
        private bool _isPanning = false;
        private SKPoint _lastPanPoint;

        // якоря для изменения размера 
        private const float HANDLE_SIZE = 10;
        private bool _isResizing = false;
        private ResizeHandle _activeHandle = ResizeHandle.None;

        public enum ResizeHandle 
        {
            None,
            TopLeft,
            TopRight, 
            BottomLeft,
            BottomRight,
            Top,
            Bottom,
            Left,
            Right
        }

        // события
        public event EventHandler CanUndoRedoChanged;
        public event EventHandler DrawingChanged;
        public event EventHandler<CanvasSizeChangedEventArgs> CanvasSizeChanged;

        private bool _isBatchOperation = false;
        private bool _hasChangedDuringBatch = false;

        // свойства для проверки возможности отмены/повтора
        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;

        public float CanvasWidth 
        {
            get => _canvasWidth;
            set 
            {
                if (_canvasWidth != value) 
                {
                    _canvasWidth = Math.Max(100, value);
                    OnCanvasSizeChanged();
                    OnDrawingChanged();
                }
            }
        }

        public float CanvasHeight
        {
            get => _canvasHeight;
            set
            {
                if (_canvasHeight != value)
                {
                    _canvasHeight = Math.Max(100, value);
                    OnCanvasSizeChanged();
                    OnDrawingChanged();
                }
            }
        }

        public float Zoom 
        {
            get => _zoom;
            set 
            {
                _zoom = Math.Max(0.1f, Math.Min(5.0f, value));
                OnDrawingChanged();
            }
        }

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

        private SKPoint ScreenToCanvas(SKPoint screenPoint, SKSize viewSize) 
        {
            float canvasScreenWidth = _canvasWidth * _zoom;
            float canvasScreenHeight = _canvasHeight * _zoom;

            float offsetX = (viewSize.Width - canvasScreenWidth) / 2 + _panOffset.X;
            float offsetY = (viewSize.Height - canvasScreenHeight) / 2 + _panOffset.Y;

            return new SKPoint(
                (screenPoint.X - offsetX) / _zoom,
                (screenPoint.Y - offsetY) / _zoom
                );
        }

        private SKPoint CanvasToScreen(SKPoint canvasPoint, SKSize viewSize)
        {
            float canvasScreenWidth = _canvasWidth * _zoom;
            float canvasScreenHeight = _canvasHeight * _zoom;

            float offsetX = (viewSize.Width - canvasScreenWidth) / 2 + _panOffset.X;
            float offsetY = (viewSize.Height - canvasScreenHeight) / 2 + _panOffset.Y;

            return new SKPoint(
                canvasPoint.X * _zoom + offsetX,
                canvasPoint.Y * _zoom + offsetY
                );
        }

        private ResizeHandle GetResizeHandle(SKPoint screenPoint, SKSize viewSize)
        {
            var topLeft = CanvasToScreen(new SKPoint(0, 0), viewSize);
            var bottomRight = CanvasToScreen(new SKPoint(_canvasWidth, _canvasHeight), viewSize);

            float handleSize = HANDLE_SIZE;

            // углы
            if (IsPointInHandle(screenPoint, topLeft, handleSize))
                return ResizeHandle.TopLeft;
            if (IsPointInHandle(screenPoint, new SKPoint(bottomRight.X, topLeft.Y), handleSize))
                return ResizeHandle.TopRight;
            if (IsPointInHandle(screenPoint, new SKPoint(topLeft.X, bottomRight.Y), handleSize))
                return ResizeHandle.BottomLeft;
            if (IsPointInHandle(screenPoint, bottomRight, handleSize))
                return ResizeHandle.BottomRight;

            // стороны
            if (IsPointInHandle(screenPoint, new SKPoint((topLeft.X + bottomRight.X) / 2, topLeft.Y), handleSize))
                return ResizeHandle.Top;
            if (IsPointInHandle(screenPoint, new SKPoint((topLeft.X + bottomRight.X) / 2, bottomRight.Y), handleSize))
                return ResizeHandle.Bottom;
            if (IsPointInHandle(screenPoint, new SKPoint(topLeft.X, (topLeft.Y + bottomRight.Y) / 2), handleSize))
                return ResizeHandle.Left;
            if (IsPointInHandle(screenPoint, new SKPoint(bottomRight.X, (topLeft.Y + bottomRight.Y) / 2), handleSize))
                return ResizeHandle.Right;

            return ResizeHandle.None;
        }

        private bool IsPointInHandle(SKPoint point, SKPoint handleCenter, float handleSize) 
        {
            return Math.Abs(point.X - handleCenter.X) <= handleSize &&
                Math.Abs(point.Y - handleCenter.Y) <= handleSize;
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
        public void Draw(SKCanvas canvas, SKSize viewSize) 
        {
            canvas.Clear(SKColors.White);

            float canvasScreenWidth = _canvasWidth * _zoom;
            float canvasScreenHeight = _canvasHeight * _zoom;

            float offsetX = (viewSize.Width - canvasScreenWidth) / 2 + _panOffset.X;
            float offsetY = (viewSize.Height - canvasScreenHeight) / 2 + _panOffset.Y;

            canvas.Save();

            SKRect canvasRect = new SKRect(offsetX, offsetY, offsetX + canvasScreenWidth, offsetY + canvasScreenHeight);
            using (SKPaint bgPaint = new SKPaint { Color = SKColors.White, Style = SKPaintStyle.Fill }) 
            {
                canvas.DrawRect(canvasRect, bgPaint);
            }

            using (SKPaint borderPaint = new SKPaint { Color = SKColors.Black, Style = SKPaintStyle.Stroke, StrokeWidth = 1 }) 
            {
                canvas.DrawRect(canvasRect, borderPaint);
            }

            canvas.Translate(offsetX, offsetY);
            canvas.Scale(_zoom);

            canvas.ClipRect(new SKRect(0, 0, _canvasWidth, _canvasHeight));

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
            canvas.Restore();

            DrawResizeHandles(canvas, viewSize);
        }

        private void DrawResizeHandles(SKCanvas canvas, SKSize viewSize)
        {
            var topLeft = CanvasToScreen(new SKPoint(0, 0), viewSize);
            var bottomRight = CanvasToScreen(new SKPoint(_canvasWidth, _canvasHeight), viewSize);

            using (SKPaint handlePaint = new SKPaint { Color = SKColors.Blue, Style = SKPaintStyle.Fill })
            {
                float handleSize = HANDLE_SIZE;

                DrawHandle(canvas, topLeft, handleSize, handlePaint);
                DrawHandle(canvas, new SKPoint(bottomRight.X, topLeft.Y), handleSize, handlePaint);
                DrawHandle(canvas, new SKPoint(topLeft.X, bottomRight.Y), handleSize, handlePaint);
                DrawHandle(canvas, bottomRight, handleSize, handlePaint);

                DrawHandle(canvas, new SKPoint((topLeft.X + bottomRight.X) / 2, topLeft.Y), handleSize, handlePaint);
                DrawHandle(canvas, new SKPoint((topLeft.X + bottomRight.X) / 2, bottomRight.Y), handleSize, handlePaint);
                DrawHandle(canvas, new SKPoint(topLeft.X, (topLeft.Y + bottomRight.Y) / 2), handleSize, handlePaint);
                DrawHandle(canvas, new SKPoint(bottomRight.X, (topLeft.Y + bottomRight.Y) / 2), handleSize, handlePaint);
            }
        }

        private void DrawHandle(SKCanvas canvas, SKPoint center, float size, SKPaint paint) 
        {
            canvas.DrawRect(center.X - size / 2, center.Y - size / 2, size, size, paint);
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

        public void HandleTouchStart(SKPoint point, SKSize viewSize) 
        {
            _activeHandle = GetResizeHandle(point, viewSize);
            if (_activeHandle != ResizeHandle.None) 
            {
                _isResizing = true;
                return;
            }

            SKPoint canvasPoint = ScreenToCanvas(point, viewSize);

            if (canvasPoint.X < 0 || canvasPoint.X > _canvasWidth || canvasPoint.Y < 0 || canvasPoint.Y > _canvasHeight) 
            {
                _isPanning = true;
                _lastPanPoint = point;
                return;
            }

            _startPoint = point;
            _lastPoint = point;
            _isDragging = true;

            if (_currentTool == DrawingTool.Pen)
            {
                _currentPath = new SKPath();
                _currentPath.MoveTo(canvasPoint);
            }
            
            OnDrawingChanged();
        }

        public void HandleTouchMove(SKPoint point, SKSize viewSize) 
        {
            if (_isResizing) 
            {
                HandleResize(point, viewSize);
                return;
            }

            if (_isPanning) 
            {
                float deltaX = point.X - _lastPanPoint.X;
                float deltaY = point.Y - _lastPanPoint.Y;
                _panOffset = new SKPoint(_panOffset.X + deltaX, _panOffset.Y + deltaY);
                _lastPanPoint = point;
                OnDrawingChanged();
                return;
            }
            if (!_isDragging)
                return;

            SKPoint canvasPoint = ScreenToCanvas(point, viewSize);
            _lastPoint = canvasPoint;

            if (_currentTool == DrawingTool.Pen && _currentPath != null) 
                _currentPath.LineTo(canvasPoint);

            OnDrawingChanged();
        }

        public void HandleTouchEnd(SKPoint point, SKSize viewSize) 
        {
            if (_isResizing) 
            {
                _isResizing = false;
                _activeHandle = ResizeHandle.None;
                return;
            }

            if (_isPanning) 
            {
                _isPanning = false;
                return;
            }

            if (!_isDragging)
                return;
            SKPoint canvasPoint = ScreenToCanvas(point, viewSize);
            _lastPoint = canvasPoint;
            _isDragging = false;
            int index = _elements.Count;

            IDrawingElement newElement = null;
            SKPaint paint = CreatePaint();

            switch (_currentTool) 
            {
                case DrawingTool.Pen:
                    if (_currentPath != null) 
                    {
                        _currentPath.LineTo(canvasPoint);
                        newElement = new PathElement(_currentPath, paint);
                        _currentPath = null;
                    }
                    break;

                case DrawingTool.Line:
                    newElement = new LineElement(_startPoint, canvasPoint, paint);
                    break;

                case DrawingTool.Rectangle:
                case DrawingTool.Square:
                    SKRect rectRect = CalculateRect(_startPoint, point, _currentTool == DrawingTool.Square);
                    newElement = new RectangleElement(rectRect, paint);
                    break;
                case DrawingTool.Ellipse:
                case DrawingTool.Circle:
                    SKRect ellipseRect = CalculateRect(_startPoint, canvasPoint, _currentTool == DrawingTool.Circle);
                    newElement = new EllipseElement(ellipseRect, paint);
                    break;
                case DrawingTool.Triangle:
                    SKPoint[] trianglePoints = CalculateTrianglePoints(_startPoint, canvasPoint);
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

        private void HandleResize(SKPoint point, SKSize viewSize) 
        {
            var topLeft = CanvasToScreen(new SKPoint(0, 0), viewSize);
            var bottomRight = CanvasToScreen(new SKPoint(_canvasWidth, _canvasHeight), viewSize);

            switch (_activeHandle)
            {
                case ResizeHandle.TopLeft:
                    var newTopLeft = point;
                    _canvasWidth = (bottomRight.X - newTopLeft.X) / _zoom;
                    _canvasHeight = (bottomRight.Y - newTopLeft.Y) / _zoom;
                    break;
                case ResizeHandle.TopRight:
                    _canvasWidth = (point.X - topLeft.X) / _zoom;
                    _canvasHeight = (bottomRight.Y - point.Y) / _zoom;
                    break;
                case ResizeHandle.BottomLeft:
                    _canvasWidth = (bottomRight.X - point.X) / _zoom;
                    _canvasHeight = (point.Y - topLeft.Y) / _zoom;
                    break;
                case ResizeHandle.BottomRight:
                    _canvasWidth = (point.X - topLeft.X) / _zoom;
                    _canvasHeight = (point.Y - topLeft.Y) / _zoom;
                    break;
                case ResizeHandle.Top:
                    _canvasHeight = (bottomRight.Y - point.Y) / _zoom;
                    break;
                case ResizeHandle.Bottom:
                    _canvasHeight = (point.Y - topLeft.Y) / _zoom;
                    break;
                case ResizeHandle.Left:
                    _canvasWidth = (bottomRight.X - point.X) / _zoom;
                    break;
                case ResizeHandle.Right:
                    _canvasWidth = (point.X - topLeft.X) / _zoom;
                    break;
            }

            _canvasWidth = Math.Max(100, _canvasWidth);
            _canvasHeight = Math.Max(100, _canvasHeight);

            OnCanvasSizeChanged();
            OnDrawingChanged();
        }

        public void SetZoom(float zoom) 
        {
            Zoom = zoom;
        }

        public void SetCanvasSize(float width, float height) 
        {
            CanvasWidth = width;
            CanvasHeight = height;
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
            SKImageInfo info = new SKImageInfo((int)_canvasWidth, (int)_canvasHeight);
            using (SKSurface surface = SKSurface.Create(info)) 
            {
                SKCanvas canvas = surface.Canvas;
                canvas.Clear(SKColors.White);

                int count = _elements.Count;
                for (int i = 0; i < count; i++) 
                {
                    _elements[i].Draw(canvas);
                }

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

        protected virtual void OnCanvasSizeChanged() 
        {
            CanvasSizeChanged?.Invoke(this, new CanvasSizeChangedEventArgs(_canvasWidth, _canvasHeight));
        }
    }
}
