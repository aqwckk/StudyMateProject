using SkiaSharp;
using StudyMateTest.Models.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyMateTest.Services.DrawingServices
{
    public interface IDrawingService 
    {
        void Draw(SKCanvas canvas, SKSize viewSize);
        void HandleTouchStart(SKPoint point, SKSize viewSize);
        void HandleTouchMove(SKPoint point, SKSize viewSize);
        void HandleTouchEnd(SKPoint point, SKSize viewSize);

        void SetCurrentTool(DrawingTool tool);
        void SetStrokeWidth(float width);
        void SetStrokeColor(SKColor color);
        bool SetFillMode(bool isFilled);


        void SetZoom(float zoom);
        void SetCanvasSize(float width, float height);

        bool CanUndo { get; }
        bool CanRedo { get; }
        float CanvasWidth { get; set; }
        float CanvasHeight { get; set; }
        float Zoom { get; set; }

        void Undo();
        void Redo();
        void Clear();

        Task<byte[]> SaveAsPngAsync();

        event EventHandler CanUndoRedoChanged;
        event EventHandler DrawingChanged;
        event EventHandler<CanvasSizeChangedEventArgs> CanvasSizeChanged;
    }
}
