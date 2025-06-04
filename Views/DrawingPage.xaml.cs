using Microsoft.Maui.Controls;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using StudyMateTest.Services;
using StudyMateTest.ViewModels;

namespace StudyMateTest.Views;


public partial class DrawingPage : ContentPage
{
	private DateTime _lastInvalidateTime = DateTime.MinValue;
	private const int MinInvalidateIntervalMs = 16;
	private DrawingPageViewModel _viewModel;
	public DrawingPage(IDrawingService drawingService)
	{
		InitializeComponent();

		_viewModel = new DrawingPageViewModel(drawingService);
		BindingContext = _viewModel;

		drawingService.DrawingChanged += OnDrawingChanged;
	}

    public DrawingPage()
    {
        InitializeComponent();

        var drawingService = App.Current.Handler.MauiContext.Services.GetService<IDrawingService>();
        _viewModel = new DrawingPageViewModel();
        BindingContext = _viewModel;

		drawingService.DrawingChanged += OnDrawingChanged;
    }

    private void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs e) 
	{
		_viewModel.Draw(e.Surface.Canvas);
	}

	private void OnDrawingChanged(object sender, EventArgs e) 
	{
		MainThread.BeginInvokeOnMainThread(() =>
		{
			canvasView.InvalidateSurface();
		});
	}

	private void OnCanvasViewTouch(object sender, SKTouchEventArgs e) 
	{
		switch (e.ActionType) 
		{
			case SKTouchAction.Pressed:
				_viewModel.HandleTouchStart(e.Location);
				break;
			case SKTouchAction.Moved:
				_viewModel.HandleTouchMove(e.Location);
				break;
			case SKTouchAction.Released:
				_viewModel.HandleTouchEnd(e.Location);
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

    protected override void OnDisappearing()
    {
		if (_viewModel != null && App.Current?.Handler?.MauiContext?.Services != null) 
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