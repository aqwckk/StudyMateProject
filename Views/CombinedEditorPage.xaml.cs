using Microsoft.Maui.Controls;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using StudyMateTest.Services.DrawingServices;
using StudyMateTest.Services.TextEditorServices;
using StudyMateTest.ViewModels;

namespace StudyMateTest.Views;

public partial class CombinedEditorPage : ContentPage
{
    private DateTime _lastInvalidateTime = DateTime.MinValue;
    private const int MinInvalidateIntervalMs = 16;
    private CombinedEditorViewModel _viewModel;
    private bool _isCtrlPressed = false;

    private double _splitterPosition = 0.5;
    private bool _isDraggingSplitter = false;
    private double _totalWidth = 0;

    public CombinedEditorPage(IDrawingService drawingService, ITextEditorService textEditorService)
    {
        InitializeComponent();
        _viewModel = new CombinedEditorViewModel(drawingService, textEditorService);
        BindingContext = _viewModel;

        drawingService.DrawingChanged += OnDrawingChanged;
        drawingService.CanUndoRedoChanged += OnDrawingChanged; // Добавляем принудительную перерисовку

        // Подписываемся на изменения форматирования для обновления RichTextEditor
        textEditorService.FormattingChanged += OnTextFormattingChanged;

        UpdateSplitterLayout();
    }

    public CombinedEditorPage()
    {
        InitializeComponent();

        var drawingService = App.Current.Handler.MauiContext.Services.GetService<IDrawingService>();
        var textEditorService = App.Current.Handler.MauiContext.Services.GetService<ITextEditorService>();

        _viewModel = new CombinedEditorViewModel(drawingService, textEditorService);
        BindingContext = _viewModel;

        drawingService.DrawingChanged += OnDrawingChanged;
        drawingService.CanUndoRedoChanged += OnDrawingChanged; // Добавляем принудительную перерисовку

        // Подписываемся на изменения форматирования для обновления RichTextEditor
        textEditorService.FormattingChanged += OnTextFormattingChanged;

        UpdateSplitterLayout();
    }

    private void UpdateSplitterLayout()
    {
        if (SplitMode is Grid splitGrid)
        {
            LeftColumn.Width = new GridLength(_splitterPosition, GridUnitType.Star);
            RightColumn.Width = new GridLength(1 - _splitterPosition, GridUnitType.Star);
        }
    }

    private void OnSplitterPanUpdated(object sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _isDraggingSplitter = true;
                _totalWidth = SplitMode.Width;
                Splitter.BackgroundColor = Colors.Blue;
                StatusLabel.Text = "Перетаскивание разделителя...";
                break;

            case GestureStatus.Running:
                if (_isDraggingSplitter && _totalWidth > 0)
                {
                    // Исправленный расчет движения разделителя
                    double currentX = e.TotalX;
                    double deltaRatio = currentX / _totalWidth;
                    double newPosition = Math.Max(0.1, Math.Min(0.9, 0.5 + deltaRatio));

                    _splitterPosition = newPosition;
                    UpdateSplitterLayout();

                    StatusLabel.Text = $"Разделение: {_splitterPosition:P0} | {(1 - _splitterPosition):P0}";
                }
                break;

            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                _isDraggingSplitter = false;
                Splitter.BackgroundColor = Colors.DarkGray;
                StatusLabel.Text = "Готов";
                break;
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
        });
    }

    private void OnBulletListClicked(object sender, EventArgs e)
    {
        richTextEditor?.CreateBulletList();
    }

    private void OnNumberedListClicked(object sender, EventArgs e)
    {
        richTextEditor?.CreateNumberedList();
    }

    private void OnUnderlineClicked(object sender, EventArgs e)
    {
        richTextEditor?.ApplyUnderline();
    }

    private void OnTextFormattingChanged(object sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            // Обновляем RichTextEditor при изменении форматирования
            if (richTextEditor != null && _viewModel?.TextViewModel != null)
            {
                richTextEditor.IsBold = _viewModel.TextViewModel.IsBold;
                richTextEditor.IsItalic = _viewModel.TextViewModel.IsItalic;
                richTextEditor.FontSize = _viewModel.TextViewModel.FontSize;
                richTextEditor.FontFamily = _viewModel.TextViewModel.FontFamily;

                // Обработка списков
                if (_viewModel.TextViewModel.CurrentFormatting != null)
                {
                    // Здесь можно добавить логику для обновления списков
                }
            }
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
                drawingService.CanUndoRedoChanged -= OnDrawingChanged;
            }
        }
        base.OnDisappearing();
    }
}