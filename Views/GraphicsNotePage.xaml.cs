using Microsoft.Maui.Graphics;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using StudyMateProject.Models;
using StudyMateProject.ViewModels;

namespace StudyMateProject.Views;

[QueryProperty(nameof(NoteId), "NoteId")]
public partial class GraphicNotePage : ContentPage
{
    private GraphicNoteViewModel _viewModel;
    private SKPath _currentPath;
    private DrawingStroke _currentStroke;
    private bool _isDrawing;

    public string NoteId
    {
        set
        {
            if (!string.IsNullOrEmpty(value))
            {
                _viewModel.NoteId = value;
            }
        }
    }

    public GraphicNotePage(GraphicNoteViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
        _currentPath = new SKPath();
        _isDrawing = false;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        // Auto-save when navigating away
        _viewModel.SaveNoteCommand.Execute(null);
    }

    private void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        SKCanvas canvas = e.Surface.Canvas;
        SKImageInfo info = e.Info;

        // Clear the canvas
        canvas.Clear(SKColors.White);

        // Draw all saved strokes
        foreach (var stroke in _viewModel.Strokes)
        {
            if (stroke.Points.Count < 2)
                continue;

            using (var paint = new SKPaint())
            {
                // Set paint properties based on the stroke
                paint.Style = SKPaintStyle.Stroke;
                paint.StrokeWidth = stroke.StrokeWidth;
                paint.StrokeJoin = SKStrokeJoin.Round;
                paint.StrokeCap = SKStrokeCap.Round;

                // Безопасное преобразование строки цвета в SKColor
                try
                {
                    paint.Color = SKColor.Parse(stroke.Color);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error parsing color: {stroke.Color}, {ex.Message}");
                    paint.Color = SKColors.Black; // Используем черный цвет по умолчанию при ошибке
                }

                // Apply special settings for highlighter
                if (stroke.StrokeType == "Highlighter")
                {
                    paint.IsAntialias = true;
                    paint.BlendMode = SKBlendMode.SrcOver;
                    paint.Color = paint.Color.WithAlpha(128); // Semi-transparent
                }
                // Apply settings for eraser
                else if (stroke.StrokeType == "Eraser")
                {
                    // Eraser is drawing white
                    paint.Color = SKColors.White;
                    paint.BlendMode = SKBlendMode.Src;
                }
                else
                {
                    paint.IsAntialias = true;
                }

                using (var path = new SKPath())
                {
                    // Start at the first point
                    if (stroke.Points.Count > 0)
                    {
                        path.MoveTo(stroke.Points[0].X, stroke.Points[0].Y);

                        // Add lines to all other points
                        for (int i = 1; i < stroke.Points.Count; i++)
                        {
                            path.LineTo(stroke.Points[i].X, stroke.Points[i].Y);
                        }

                        // Draw the path
                        canvas.DrawPath(path, paint);
                    }
                }
            }
        }

        // Draw the current path
        if (_isDrawing && _currentStroke != null && _currentStroke.Points.Count > 0)
        {
            using (var paint = new SKPaint())
            {
                // Set paint properties based on the current stroke
                paint.Style = SKPaintStyle.Stroke;
                paint.StrokeWidth = _currentStroke.StrokeWidth;
                paint.StrokeJoin = SKStrokeJoin.Round;
                paint.StrokeCap = SKStrokeCap.Round;

                // Безопасное преобразование строки цвета в SKColor
                try
                {
                    paint.Color = SKColor.Parse(_currentStroke.Color);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error parsing current stroke color: {_currentStroke.Color}, {ex.Message}");
                    paint.Color = SKColors.Black; // Используем черный цвет по умолчанию при ошибке
                }

                // Apply special settings for highlighter
                if (_currentStroke.StrokeType == "Highlighter")
                {
                    paint.IsAntialias = true;
                    paint.BlendMode = SKBlendMode.SrcOver;
                    paint.Color = paint.Color.WithAlpha(128); // Semi-transparent
                }
                // Apply settings for eraser
                else if (_currentStroke.StrokeType == "Eraser")
                {
                    // Eraser is drawing white
                    paint.Color = SKColors.White;
                    paint.BlendMode = SKBlendMode.Src;
                }
                else
                {
                    paint.IsAntialias = true;
                }

                // Draw the current path
                canvas.DrawPath(_currentPath, paint);
            }
        }
    }

    private void OnCanvasViewTouch(object sender, SKTouchEventArgs e)
    {
        try
        {
            switch (e.ActionType)
            {
                case SKTouchAction.Pressed:
                    // Start a new path
                    _currentPath = new SKPath();
                    _currentPath.MoveTo(e.Location);

                    // Create a new stroke
                    _currentStroke = new DrawingStroke
                    {
                        StrokeWidth = _viewModel.SelectedStrokeWidth,
                        Color = _viewModel.SelectedColor,
                        StrokeType = _viewModel.SelectedTool
                    };
                    _currentStroke.Points.Add(new SKPointWrapper { X = e.Location.X, Y = e.Location.Y });

                    _isDrawing = true;
                    break;

                case SKTouchAction.Moved:
                    if (_isDrawing)
                    {
                        // Add point to the path
                        _currentPath.LineTo(e.Location);

                        // Add point to the stroke
                        _currentStroke.Points.Add(new SKPointWrapper { X = e.Location.X, Y = e.Location.Y });

                        // Invalidate to redraw
                        canvasView.InvalidateSurface();
                    }
                    break;

                case SKTouchAction.Released:
                    if (_isDrawing)
                    {
                        // Finish the path
                        _currentPath.LineTo(e.Location);

                        // Add the last point to the stroke
                        _currentStroke.Points.Add(new SKPointWrapper { X = e.Location.X, Y = e.Location.Y });

                        // Add the stroke to the collection
                        _viewModel.AddStrokeCommand.Execute(_currentStroke);

                        _isDrawing = false;

                        // Invalidate to redraw
                        canvasView.InvalidateSurface();
                    }
                    break;

                case SKTouchAction.Cancelled:
                    _isDrawing = false;
                    break;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in OnCanvasViewTouch: {ex.Message}");
            _isDrawing = false;
        }

        e.Handled = true;
    }
}