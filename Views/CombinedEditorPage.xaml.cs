using Microsoft.Maui.Controls;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using StudyMateTest.Services;
using StudyMateTest.Services.DrawingServices;
using StudyMateTest.Services.TextEditorServices;
using StudyMateTest.ViewModels;
using StudyMateTest.Views.Enums;
namespace StudyMateTest.Views;

public partial class CombinedEditorPage : ContentPage
{
    private DateTime _lastInvalidateTime = DateTime.MinValue;
    private const int MinInvalidateIntervalMs = 16;
    private CombinedEditorViewModel _viewModel;
    private bool _isCtrlPressed = false;

    private double _splitterPosition = 0.5;

    public CombinedEditorPage(IDrawingService drawingService, ITextEditorService textEditorService) 
    {
        InitializeComponent();
        _viewModel = new CombinedEditorViewModel(drawingService, textEditorService);
        BindingContext = _viewModel;

        drawingService.DrawingChanged += OnDrawingChanged;

        UpdateUIForMode(EditorMode.Graphics);
    }
    public CombinedEditorPage()
	{
		InitializeComponent();

        var drawingService = App.Current.Handler.MauiContext.Services.GetService<IDrawingService>();
        var textEditorService = App.Current.Handler.MauiContext.Services.GetService<ITextEditorService>();

        _viewModel = new CombinedEditorViewModel(drawingService, textEditorService);
        BindingContext = _viewModel;

        drawingService.DrawingChanged += OnDrawingChanged;
        UpdateUIForMode(EditorMode.Graphics);
	}

    private void OnGraphicsModeClicked(object sender, EventArgs e) 
    {
        _viewModel.CurrentMode = EditorMode.Graphics;
        UpdateUIForMode(EditorMode.Graphics);
    }
    private void OnTextModeClicked(object sender, EventArgs e)
    {
        _viewModel.CurrentMode = EditorMode.Text;
        UpdateUIForMode(EditorMode.Text);
    }
    private void OnSplitModeClicked(object sender, EventArgs e)
    {
        _viewModel.CurrentMode = EditorMode.Split;
        UpdateUIForMode(EditorMode.Split);
    }

    private void UpdateUIForMode(EditorMode mode) 
    {
        GraphicsButton.BackgroundColor = Colors.LightGray;
        TextButton.BackgroundColor = Colors.LightGray;
        SplitButton.BackgroundColor = Colors.LightGray;

        GraphicsOnlyMode.IsVisible = false;
        TextOnlyMode.IsVisible = false;
        SplitMode.IsVisible = false;

        GraphicsToolbar.IsVisible = false;
        TextToolbar.IsVisible = false;

        switch (mode) 
        {
            case EditorMode.Graphics:
                GraphicsButton.BackgroundColor = Colors.LightBlue;
                GraphicsOnlyMode.IsVisible = true;
                GraphicsToolbar.IsVisible = true;

                GraphicsControls.IsVisible = true;
                BrushSettings.IsVisible = true;
                ZoomSettings.IsVisible = true;
                SaveDrawingButton.IsVisible = true;

                DocumentStatus.IsVisible = false;
                SaveTextButton.IsVisible = false;
                break;
            case EditorMode.Text:
                TextButton.BackgroundColor = Colors.LightBlue;
                TextOnlyMode.IsVisible = true;
                TextToolbar.IsVisible = true;

                DocumentStatus.IsVisible = true;
                SaveTextButton.IsVisible = true;

                GraphicsControls.IsVisible = false;
                BrushSettings.IsVisible = false;
                ZoomSettings.IsVisible = false;
                SaveDrawingButton.IsVisible = false;
                break;

            case EditorMode.Split:
                SplitButton.BackgroundColor = Colors.LightBlue;
                SplitMode.IsVisible = true;
                GraphicsToolbar.IsVisible = true;
                TextToolbar.IsVisible = true;

                GraphicsControls.IsVisible = true;
                DocumentStatus.IsVisible = true;
                BrushSettings.IsVisible = true;
                ZoomSettings.IsVisible = true;
                SaveDrawingButton.IsVisible = true;
                SaveTextButton.IsVisible = true;

                UpdateSplitterLayout();
                break;
        }
    }

    private void UpdateSplitterLayout()
    {
        if (SplitMode.IsVisible)
        {
            var columnDefinitions = SplitMode.ColumnDefinitions;
            columnDefinitions[0].Width = new GridLength(_splitterPosition, GridUnitType.Star);
            columnDefinitions[2].Width = new GridLength(1 - _splitterPosition, GridUnitType.Star);
        }
    }

    private void OnSplitterPanUpdated(object sender, PanUpdatedEventArgs e)
    {
        if (e.StatusType == GestureStatus.Running)
        {
            var totalWidth = SplitMode.Width;
            if (totalWidth > 0)
            {
                var deltaRatio = e.TotalX / totalWidth;
                _splitterPosition = Math.Max(0.1, Math.Min(0.9, _splitterPosition + deltaRatio));
                UpdateSplitterLayout();
            }
        }
    }

    private void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        _viewModel.DrawingViewModel.SetViewSize(e.Info.Size);
        _viewModel.DrawingViewModel.Draw(e.Surface.Canvas);
    }

    private void OnDrawingChanged(object sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            canvasView?.InvalidateSurface();
            canvasViewSplit?.InvalidateSurface();
        });
    }

    private void OnCanvasViewTouch(object sender, SKTouchEventArgs e)
    {
        switch (e.ActionType)
        {
            case SKTouchAction.Pressed:
                _viewModel.DrawingViewModel.HandleTouchStart(e.Location);
                break;
            case SKTouchAction.Moved:
                _viewModel.DrawingViewModel.HandleTouchMove(e.Location);
                break;
            case SKTouchAction.Released:
                _viewModel.DrawingViewModel.HandleTouchEnd(e.Location);
                break;
        }

        DateTime now = DateTime.Now;
        if ((now - _lastInvalidateTime).TotalMilliseconds >= MinInvalidateIntervalMs)
        {
            ((SKCanvasView)sender).InvalidateSurface();
            _lastInvalidateTime = now;
        }

        e.Handled = true;
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        if (Handler?.PlatformView != null)
        {
            try
            {
#if WINDOWS
                    if (Handler.PlatformView is Microsoft.UI.Xaml.FrameworkElement frameworkElement)
                    {
                        frameworkElement.KeyDown += (s, e) =>
                        {
                            _isCtrlPressed = e.Key == Windows.System.VirtualKey.Control;
                        };
                        frameworkElement.KeyUp += (s, e) =>
                        {
                            if (e.Key == Windows.System.VirtualKey.Control)
                                _isCtrlPressed = false;
                        };
                        frameworkElement.PointerWheelChanged += (s, e) =>
                        {
                            if (_isCtrlPressed)
                            {
                                var delta = e.GetCurrentPoint(frameworkElement).Properties.MouseWheelDelta;
                                var location = e.GetCurrentPoint(frameworkElement).Position;
                                _viewModel.DrawingViewModel.HandleWheelZoom(delta, new SKPoint((float)location.X, (float)location.Y));
                                e.Handled = true;
                            }
                        };
                    }
#endif
            }
            catch
            {
                
            }
        }
    }

    protected override void OnDisappearing()
    {
        if (_viewModel?.DrawingViewModel != null && App.Current?.Handler?.MauiContext?.Services != null)
        {
            var drawingService = App.Current.Handler.MauiContext.Services.GetService<IDrawingService>();
            if (drawingService != null)
            {
                drawingService.DrawingChanged -= OnDrawingChanged;
            }
        }
        base.OnDisappearing();
    }
}