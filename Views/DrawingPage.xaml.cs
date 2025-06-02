using Microsoft.Maui.Controls;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using StudyMateTest.Services;
using StudyMateTest.ViewModels;
using System.Xml;

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
	}

    public DrawingPage()
    {
        InitializeComponent();

        var drawingService = App.Current.Handler.MauiContext.Services.GetService<IDrawingService>();
        _viewModel = new DrawingPageViewModel();
        BindingContext = _viewModel;
    }

    private void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs e) 
	{
		_viewModel.Draw(e.Surface.Canvas);
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
}