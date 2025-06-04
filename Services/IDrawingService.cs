using SkiaSharp;
using StudyMateTest.Models.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyMateTest.Services
{
    public interface IDrawingService 
    {
        void Draw(SKCanvas canvas);
        void HandleTouchStart(SKPoint point);
        void HandleTouchMove(SKPoint point);
        void HandleTouchEnd(SKPoint point);

        void SetCurrentTool(DrawingTool tool);
        void SetStrokeWidth(float width);
        void SetStrokeColor(SKColor color);
        bool SetFillMode(bool isFilled);

        bool CanUndo { get; }
        bool CanRedo { get; }
        void Undo();
        void Redo();
        void Clear();

        Task<byte[]> SaveAsPngAsync();

        event EventHandler CanUndoRedoChanged;
        event EventHandler DrawingChanged;
    }
}
