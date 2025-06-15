using Microsoft.Maui.Controls;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using StudyMateTest.Services.DrawingServices;
using StudyMateTest.Services.TextEditorServices;
using StudyMateTest.ViewModels;
using StudyMateTest.Models;
using StudyMateTest.Services;

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

    private Note _currentNote;
    private INoteService _noteService;

    private System.Timers.Timer _autoSaveTimer;
    private bool _hasUnsavedChanges = false;
    private readonly object _autoSaveLock = new object();

    public CombinedEditorPage(IDrawingService drawingService, ITextEditorService textEditorService)
    {
        InitializeComponent();
        _viewModel = new CombinedEditorViewModel(drawingService, textEditorService);
        BindingContext = _viewModel;

        _noteService = GetNoteService();

        drawingService.DrawingChanged += OnDrawingChanged;
        drawingService.CanUndoRedoChanged += OnDrawingChanged;

        textEditorService.FormattingChanged += OnTextFormattingChanged;

        InitializeAutoSave();

        UpdateSplitterLayout();
    }

    public CombinedEditorPage()
    {
        InitializeComponent();

        var drawingService = App.Current.Handler.MauiContext.Services.GetService<IDrawingService>();
        var textEditorService = App.Current.Handler.MauiContext.Services.GetService<ITextEditorService>();

        _viewModel = new CombinedEditorViewModel(drawingService, textEditorService);
        BindingContext = _viewModel;

        _noteService = GetNoteService();

        drawingService.DrawingChanged += OnDrawingChanged;
        drawingService.CanUndoRedoChanged += OnDrawingChanged;

        textEditorService.FormattingChanged += OnTextFormattingChanged;

        InitializeAutoSave();

        UpdateSplitterLayout();
    }

    private INoteService GetNoteService()
    {
        try
        {
            if (Application.Current?.MainPage?.Handler?.MauiContext?.Services != null)
            {
                var service = Application.Current.MainPage.Handler.MauiContext.Services.GetService<INoteService>();
                if (service != null)
                {
                    return service;
                }
            }
            return new NoteService();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting NoteService: {ex.Message}");
            return new NoteService();
        }
    }

    private void InitializeAutoSave()
    {
        _autoSaveTimer = new System.Timers.Timer(180000);
        _autoSaveTimer.Elapsed += OnAutoSaveTimer;
        _autoSaveTimer.AutoReset = true;
        _autoSaveTimer.Start();

        System.Diagnostics.Debug.WriteLine("Auto-save timer initialized (3 minutes)");
    }

    private async void OnAutoSaveTimer(object sender, System.Timers.ElapsedEventArgs e)
    {
        lock (_autoSaveLock)
        {
            if (!_hasUnsavedChanges || _currentNote == null)
                return;
        }

        try
        {
            System.Diagnostics.Debug.WriteLine($"=== AUTO-SAVE TRIGGERED ===");

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                try
                {
                    StatusIndicator.Text = "Автосохранение...";

                    await CollectNoteContent();
                    await _noteService.SaveNoteAsync(_currentNote);

                    MessagingCenter.Send(this, "NoteSaved", _currentNote);

                    lock (_autoSaveLock)
                    {
                        _hasUnsavedChanges = false;
                    }

                    StatusIndicator.Text = "Автосохранено";
                    System.Diagnostics.Debug.WriteLine($"Auto-saved note: {_currentNote.Title}");

                    _ = Task.Delay(2000).ContinueWith(_ =>
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            if (StatusIndicator.Text == "Автосохранено")
                                StatusIndicator.Text = "Готов";
                        });
                    });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in auto-save: {ex.Message}");
                    StatusIndicator.Text = "Ошибка автосохранения";
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in auto-save timer: {ex.Message}");
        }
    }

    private void MarkAsChanged()
    {
        lock (_autoSaveLock)
        {
            _hasUnsavedChanges = true;
        }

        if (_currentNote != null)
        {
            _currentNote.MarkAsModified();
            StatusIndicator.Text = "Изменено";
        }
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

            MarkAsChanged();
        });
    }

    private void OnTextFormattingChanged(object sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (richTextEditor != null && _viewModel?.TextViewModel != null)
            {
                richTextEditor.IsBold = _viewModel.TextViewModel.IsBold;
                richTextEditor.IsItalic = _viewModel.TextViewModel.IsItalic;
                richTextEditor.FontSize = _viewModel.TextViewModel.FontSize;
                richTextEditor.FontFamily = _viewModel.TextViewModel.FontFamily;
            }

            MarkAsChanged();
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

    public void LoadNote(Note note)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"=== LoadNote START ===");
            System.Diagnostics.Debug.WriteLine($"Loading note: {note.Title}");
            System.Diagnostics.Debug.WriteLine($"Note ID: {note.Id}");
            System.Diagnostics.Debug.WriteLine($"TextContent: {(string.IsNullOrEmpty(note.TextContent) ? "EMPTY" : $"Length: {note.TextContent.Length}")}");
            System.Diagnostics.Debug.WriteLine($"GraphicsData: {(note.GraphicsData == null ? "NULL" : $"Length: {note.GraphicsData.Length} bytes")}");

            _currentNote = note;

            if (!string.IsNullOrEmpty(note.TextContent))
            {
                _viewModel.TextViewModel.DocumentContent = note.TextContent;
                _viewModel.TextViewModel.DocumentTitle = note.Title;
                System.Diagnostics.Debug.WriteLine($"Text content loaded: {note.TextContent.Length} chars");
            }
            else
            {
                _viewModel.TextViewModel.DocumentContent = "";
                _viewModel.TextViewModel.DocumentTitle = note.Title;
                System.Diagnostics.Debug.WriteLine("No text content to load");
            }

            if (note.GraphicsData != null && note.GraphicsData.Length > 0)
            {
                System.Diagnostics.Debug.WriteLine($"Graphics data available: {note.GraphicsData.Length} bytes");

                try
                {
                    var drawingService = App.Current.Handler.MauiContext.Services.GetService<IDrawingService>();
                    if (drawingService != null)
                    {
                        System.Diagnostics.Debug.WriteLine("DrawingService found, loading graphics");
                        drawingService.LoadFromBytes(note.GraphicsData);

                        System.Diagnostics.Debug.WriteLine($"Graphics loaded, has content: {drawingService.HasGraphicsContent()}");

                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            canvasView?.InvalidateSurface();
                            System.Diagnostics.Debug.WriteLine("Canvas invalidated after loading graphics");
                        });
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("DrawingService not found");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading graphics: {ex.Message}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("No graphics data to load");
                try
                {
                    var drawingService = App.Current.Handler.MauiContext.Services.GetService<IDrawingService>();
                    drawingService?.Clear();
                    MainThread.BeginInvokeOnMainThread(() => canvasView?.InvalidateSurface());
                    System.Diagnostics.Debug.WriteLine("Canvas cleared - no graphics data");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error clearing canvas: {ex.Message}");
                }
            }

            Title = $"Редактор - {note.DisplayTitle}";
            StatusIndicator.Text = note.IsModified ? "Изменено" : "Сохранено";

            lock (_autoSaveLock)
            {
                _hasUnsavedChanges = false;
            }

            System.Diagnostics.Debug.WriteLine($"=== LoadNote END - Note loaded successfully: {note.Title} ===");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ERROR loading note: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            DisplayAlert("Ошибка", $"Не удалось загрузить заметку: {ex.Message}", "OK");
        }
    }

    public async void OnSaveNoteClicked(object sender, EventArgs e)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"=== OnSaveNoteClicked START ===");

            if (_currentNote == null)
            {
                await DisplayAlert("Ошибка", "Нет заметки для сохранения", "OK");
                return;
            }

            StatusIndicator.Text = "Сохранение...";

            await CollectNoteContent();

            System.Diagnostics.Debug.WriteLine($"Saving note: {_currentNote.Title}");
            System.Diagnostics.Debug.WriteLine($"TextContent: {(string.IsNullOrEmpty(_currentNote.TextContent) ? "EMPTY" : $"Length: {_currentNote.TextContent.Length}")}");
            System.Diagnostics.Debug.WriteLine($"GraphicsData: {(_currentNote.GraphicsData == null ? "NULL" : $"Length: {_currentNote.GraphicsData.Length} bytes")}");

            await _noteService.SaveNoteAsync(_currentNote);

            MessagingCenter.Send(this, "NoteSaved", _currentNote);

            lock (_autoSaveLock)
            {
                _hasUnsavedChanges = false;
            }

            StatusIndicator.Text = "Сохранено";
            await DisplayAlert("Сохранение", $"Заметка '{_currentNote.Title}' сохранена", "OK");

            System.Diagnostics.Debug.WriteLine($"=== OnSaveNoteClicked END - Note saved successfully ===");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ERROR saving note: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            StatusIndicator.Text = "Ошибка сохранения";
            await DisplayAlert("Ошибка", $"Не удалось сохранить заметку: {ex.Message}", "OK");
        }
    }

    public async void OnSaveAsNewNoteClicked(object sender, EventArgs e)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"=== OnSaveAsNewNoteClicked START ===");

            if (_currentNote == null)
            {
                await DisplayAlert("Ошибка", "Нет заметки для сохранения", "OK");
                return;
            }

            StatusIndicator.Text = "Создание копии...";

            await CollectNoteContent();

            var newNote = _currentNote.CreateCopy();

            System.Diagnostics.Debug.WriteLine($"Creating copy of note: {newNote.Title}");

            await _noteService.SaveNoteAsync(newNote);

            MessagingCenter.Send(this, "NoteSaved", newNote);

            _currentNote = newNote;
            Title = $"Редактор - {newNote.DisplayTitle}";

            lock (_autoSaveLock)
            {
                _hasUnsavedChanges = false;
            }

            StatusIndicator.Text = "Сохранено как новая";
            await DisplayAlert("Сохранение", $"Создана новая заметка: {newNote.Title}", "OK");

            System.Diagnostics.Debug.WriteLine($"=== OnSaveAsNewNoteClicked END - New note created ===");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ERROR saving as new note: {ex.Message}");
            StatusIndicator.Text = "Ошибка сохранения";
            await DisplayAlert("Ошибка", $"Не удалось сохранить как новую заметку: {ex.Message}", "OK");
        }
    }

    private async Task CollectNoteContent()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"=== CollectNoteContent START ===");

            if (_currentNote == null) return;

            var textContent = _viewModel?.TextViewModel?.DocumentContent ?? "";
            if (!string.IsNullOrEmpty(textContent))
            {
                _currentNote.TextContent = textContent;
                System.Diagnostics.Debug.WriteLine($"Collected text content: {textContent.Length} chars");
            }
            else
            {
                _currentNote.TextContent = "";
                System.Diagnostics.Debug.WriteLine("No text content to collect");
            }

            try
            {
                var drawingService = App.Current.Handler.MauiContext.Services.GetService<IDrawingService>();
                if (drawingService != null && drawingService.HasGraphicsContent())
                {
                    System.Diagnostics.Debug.WriteLine("Saving graphics data from DrawingService");
                    var graphicsData = await drawingService.SaveAsPngAsync();

                    if (graphicsData != null && graphicsData.Length > 0)
                    {
                        _currentNote.GraphicsData = graphicsData;
                        System.Diagnostics.Debug.WriteLine($"Collected graphics data: {_currentNote.GraphicsData.Length} bytes");
                    }
                    else
                    {
                        _currentNote.GraphicsData = null;
                        System.Diagnostics.Debug.WriteLine("No graphics data returned from DrawingService.SaveAsPngAsync");
                    }
                }
                else
                {
                    _currentNote.GraphicsData = null;
                    System.Diagnostics.Debug.WriteLine("DrawingService not found or has no graphics content");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error collecting graphics: {ex.Message}");
                _currentNote.GraphicsData = null;
            }

            _currentNote.LastModified = DateTime.Now;

            System.Diagnostics.Debug.WriteLine($"=== CollectNoteContent END ===");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ERROR in CollectNoteContent: {ex.Message}");
        }
    }

    public async void OnExportOptionsClicked(object sender, EventArgs e)
    {
        try
        {
            if (_currentNote == null)
            {
                await DisplayAlert("Ошибка", "Нет заметки для экспорта", "OK");
                return;
            }

            await CollectNoteContent();

            var action = await DisplayActionSheet(
                "Выберите формат экспорта",
                "Отмена",
                null,
                "💾 Сохранить заметку",
                "📄 Экспорт в JSON",
                "🖼️ Экспорт рисунка (PNG)",
                "📝 Экспорт текста (TXT)");

            switch (action)
            {
                case "💾 Сохранить заметку":
                    OnSaveNoteClicked(sender, e);
                    break;
                case "📄 Экспорт в JSON":
                    await _noteService.ExportNoteToJsonAsync(_currentNote);
                    break;
                case "🖼️ Экспорт рисунка (PNG)":
                    await _noteService.ExportNoteToPngAsync(_currentNote);
                    break;
                case "📝 Экспорт текста (TXT)":
                    await _noteService.ExportNoteToTxtAsync(_currentNote);
                    break;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in export options: {ex.Message}");
            await DisplayAlert("Ошибка", $"Ошибка экспорта: {ex.Message}", "OK");
        }
    }

    protected override void OnDisappearing()
    {
        System.Diagnostics.Debug.WriteLine($"=== CombinedEditorPage OnDisappearing ===");

        _autoSaveTimer?.Stop();
        _autoSaveTimer?.Dispose();

        if (_currentNote != null)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await CollectNoteContent();
                    await _noteService.SaveNoteAsync(_currentNote);
                    MessagingCenter.Send(this, "NoteSaved", _currentNote);
                    System.Diagnostics.Debug.WriteLine($"Auto-saved note on exit: {_currentNote.Title}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error auto-saving note: {ex.Message}");
                }
            });
        }

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