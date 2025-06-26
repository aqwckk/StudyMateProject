using System;
using System.ComponentModel;
using System.IO;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using SkiaSharp;
using StudyMateTest.Models.Drawing;
using StudyMateTest.Services.DrawingServices;

namespace StudyMateTest.ViewModels
{
    public class DrawingPageViewModel : INotifyPropertyChanged
    {
        private readonly IDrawingService _drawingService;
        private SKSize _currentViewSize;

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand SetToolCommand { get; private set; }
        public ICommand UndoCommand { get; private set; }
        public ICommand RedoCommand { get; private set; }
        public ICommand ClearCommand { get; private set; }
        public ICommand ToggleFillModeCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
        public ICommand SetColorCommand { get; private set; }
        public ICommand ZoomInCommand { get; private set; }
        public ICommand ZoomOutCommand { get; private set; }
        public ICommand ResetZoomCommand { get; private set; }

        private DrawingTool _selectedTool = DrawingTool.Pen;
        public DrawingTool SelectedTool
        {
            get => _selectedTool;
            set
            {
                if (_selectedTool != value)
                {
                    var oldTool = _selectedTool;
                    _selectedTool = value;
                    _drawingService.SetCurrentTool(value);

                    // При переключении на ластик скрываем выбор цвета и заливки
                    if (value == DrawingTool.Eraser)
                    {
                        // Ластик автоматически использует белый цвет в DrawingService
                        // Не меняем _selectedColor, чтобы сохранить выбранный пользователем цвет
                    }
                    // При переключении с ластика на другой инструмент восстанавливаем настройки
                    else if (oldTool == DrawingTool.Eraser)
                    {
                        _drawingService.SetStrokeColor(_selectedColor);
                        _drawingService.SetFillMode(_isFilled);
                    }
                    // Для обычного переключения между инструментами
                    else
                    {
                        _drawingService.SetStrokeColor(_selectedColor);
                        _drawingService.SetFillMode(_isFilled);
                    }

                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsFillSupported));
                    OnPropertyChanged(nameof(IsColorSelectionVisible));
                }
            }
        }

        private float _strokeWidth = 5;
        public float StrokeWidth
        {
            get => _strokeWidth;
            set
            {
                if (_strokeWidth != value)
                {
                    _strokeWidth = value;
                    _drawingService.SetStrokeWidth(value);
                    OnPropertyChanged();
                }
            }
        }

        private SKColor _selectedColor = SKColors.Black;
        public SKColor SelectedColor
        {
            get => _selectedColor;
            set
            {
                if (_selectedColor != value && _selectedTool != DrawingTool.Eraser)
                {
                    _selectedColor = value;
                    _drawingService.SetStrokeColor(value);
                    OnPropertyChanged();
                }
            }
        }

        private bool _isFilled = false;
        public bool IsFilled
        {
            get => _isFilled;
            set
            {
                if (_isFilled != value && _selectedTool != DrawingTool.Eraser)
                {
                    _isFilled = value;
                    _drawingService.SetFillMode(value);
                    OnPropertyChanged();
                }
            }
        }

        public float CanvasWidth
        {
            get => _drawingService.CanvasWidth;
            set
            {
                if (_drawingService.CanvasWidth != value)
                {
                    _drawingService.CanvasWidth = value;
                    OnPropertyChanged();
                }
            }
        }

        public float CanvasHeight
        {
            get => _drawingService.CanvasHeight;
            set
            {
                if (_drawingService.CanvasHeight != value)
                {
                    _drawingService.CanvasHeight = value;
                    OnPropertyChanged();
                }
            }
        }

        public float Zoom
        {
            get => _drawingService.Zoom;
            set
            {
                if (_drawingService.Zoom != value)
                {
                    _drawingService.Zoom = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ZoomPercentage));

                    _drawingService.InvalidateCanvas();
                }
            }
        }

        public string ZoomPercentage => $"{Zoom * 100:F0}%";

        public bool IsFillSupported => _selectedTool != DrawingTool.Eraser &&
                                       (_selectedTool == DrawingTool.Rectangle ||
                                        _selectedTool == DrawingTool.Square ||
                                        _selectedTool == DrawingTool.Ellipse ||
                                        _selectedTool == DrawingTool.Circle ||
                                        _selectedTool == DrawingTool.Triangle);

        // Новое свойство для скрытия выбора цвета при использовании ластика
        public bool IsColorSelectionVisible => _selectedTool != DrawingTool.Eraser;

        public bool CanUndo => _drawingService.CanUndo;
        public bool CanRedo => _drawingService.CanRedo;

        public DrawingPageViewModel(IDrawingService drawingService)
        {
            _drawingService = drawingService;

            InitializeCommands();
            SubscribeToEvents();
        }

        public DrawingPageViewModel()
        {
            _drawingService = App.Current.Handler.MauiContext.Services.GetService<IDrawingService>();

            InitializeCommands();
            SubscribeToEvents();
        }

        private void InitializeCommands()
        {
            SetToolCommand = new Command<DrawingTool>(tool => SelectedTool = tool);
            UndoCommand = new Command(Undo, () => CanUndo);
            RedoCommand = new Command(Redo, () => CanRedo);
            ClearCommand = new Command(Clear);
            ToggleFillModeCommand = new Command(() =>
            {
                if (_selectedTool != DrawingTool.Eraser)
                {
                    IsFilled = !IsFilled;
                }
            });
            SaveCommand = new Command(async () => await SaveDrawingAsync());
            SetColorCommand = new Command<string>(SetColor);

            ZoomInCommand = new Command(() => Zoom = Math.Min(5.0f, Zoom * 1.2f));
            ZoomOutCommand = new Command(() => Zoom = Math.Max(0.1f, Zoom / 1.2f));
            ResetZoomCommand = new Command(() => Zoom = 1.0f);
        }

        private void SubscribeToEvents()
        {
            _drawingService.CanUndoRedoChanged += OnCanUndoRedoChanged;
            _drawingService.DrawingChanged += OnDrawingChanged;
            _drawingService.CanvasSizeChanged += OnCanvasSizeChanged;
        }

        private void OnCanUndoRedoChanged(object sender, EventArgs e)
        {
            OnPropertyChanged(nameof(CanUndo));
            OnPropertyChanged(nameof(CanRedo));

            ((Command)UndoCommand).ChangeCanExecute();
            ((Command)RedoCommand).ChangeCanExecute();
        }

        private void OnDrawingChanged(object sender, EventArgs e)
        {
            OnPropertyChanged(nameof(CanUndo));
            OnPropertyChanged(nameof(CanRedo));
            ((Command)UndoCommand).ChangeCanExecute();
            ((Command)RedoCommand).ChangeCanExecute();
        }

        private void OnCanvasSizeChanged(object sender, CanvasSizeChangedEventArgs e)
        {
            OnPropertyChanged(nameof(CanvasWidth));
            OnPropertyChanged(nameof(CanvasHeight));
        }

        public void SetViewSize(SKSize viewSize)
        {
            _currentViewSize = viewSize;
        }

        public void Draw(SKCanvas canvas)
        {
            _drawingService.Draw(canvas, _currentViewSize);
        }

        public void HandleTouchStart(SKPoint point)
        {
            _drawingService.HandleTouchStart(point, _currentViewSize);
        }

        public void HandleTouchMove(SKPoint point)
        {
            _drawingService.HandleTouchMove(point, _currentViewSize);
        }

        public void HandleTouchEnd(SKPoint point)
        {
            _drawingService.HandleTouchEnd(point, _currentViewSize);
        }

        public void HandleWheelZoom(float delta, SKPoint center)
        {
            float zoomFactor = delta > 0 ? 1.1f : 0.9f;
            float newZoom = Math.Max(0.1f, Math.Min(5.0f, Zoom * zoomFactor));
            Zoom = newZoom;
        }

        private void Undo()
        {
            _drawingService.Undo();
        }

        private void Redo()
        {
            _drawingService.Redo();
        }

        private void Clear()
        {
            _drawingService.Clear();
        }

        private async Task SaveDrawingAsync()
        {
            try
            {
                byte[] pngData = await _drawingService.SaveAsPngAsync();

                string fileName = $"Drawing_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                string filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);

                await File.WriteAllBytesAsync(filePath, pngData);

                await Application.Current.MainPage.DisplayAlert(
                    "Успешно",
                    $"Рисунок сохранен: {fileName}",
                    "OK");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Ошибка",
                    $"Не удалось сохранить рисунок: {ex.Message}",
                    "OK");
            }
        }

        private void SetColor(string colorName)
        {
            if (_selectedTool == DrawingTool.Eraser)
                return; // Для ластика цвет не меняется

            SKColor color;
            switch (colorName.ToLower())
            {
                case "red":
                    color = SKColors.Red;
                    break;
                case "green":
                    color = SKColors.Green;
                    break;
                case "blue":
                    color = SKColors.Blue;
                    break;
                case "yellow":
                    color = SKColors.Yellow;
                    break;
                case "purple":
                    color = SKColors.Purple;
                    break;
                default:
                    color = SKColors.Black;
                    break;
            }
            SelectedColor = color;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}