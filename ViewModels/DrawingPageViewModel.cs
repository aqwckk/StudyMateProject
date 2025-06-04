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
using StudyMateTest.Services;

namespace StudyMateTest.ViewModels
{
    public class DrawingPageViewModel : INotifyPropertyChanged 
    {
        private readonly IDrawingService _drawingService;

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand SetToolCommand { get; private set; }
        public ICommand UndoCommand { get; private set; }
        public ICommand RedoCommand { get; private set; }
        public ICommand ClearCommand { get; private set; }
        public ICommand ToggleFillModeCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
        public ICommand SetColorCommand { get; private set; }

        private DrawingTool _selectedTool = DrawingTool.Pen;
        public DrawingTool SelectedTool 
        {
            get => _selectedTool;
            set 
            {
                if (_selectedTool != value) 
                {
                    _selectedTool = value;
                    _drawingService.SetCurrentTool(value);
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsFillSupported));
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
                if (_selectedColor != value) 
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
                if (_isFilled != value) 
                {
                    _isFilled = value;
                    _drawingService.SetFillMode(value);
                    OnPropertyChanged();
                }
            }
        }

        public bool IsFillSupported => _selectedTool == DrawingTool.Rectangle ||
                                       _selectedTool == DrawingTool.Square ||
                                       _selectedTool == DrawingTool.Ellipse ||
                                       _selectedTool == DrawingTool.Circle ||
                                       _selectedTool == DrawingTool.Triangle;

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
            ToggleFillModeCommand = new Command(() => IsFilled = !IsFilled);
            SaveCommand = new Command(async () => await SaveDrawingAsync());
            SetColorCommand = new Command<string>(SetColor);
        }

        private void SubscribeToEvents() 
        {
            _drawingService.CanUndoRedoChanged += OnCanUndoRedoChanged;
            _drawingService.DrawingChanged += OnDrawingChanged;
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

        public void Draw(SKCanvas canvas) 
        {
            _drawingService.Draw(canvas);
        }

        public void HandleTouchStart(SKPoint point) 
        {
            _drawingService.HandleTouchStart(point);
        }

        public void HandleTouchMove(SKPoint point) 
        {
            _drawingService.HandleTouchMove(point);
        }

        public void HandleTouchEnd(SKPoint point) 
        {
            _drawingService.HandleTouchEnd(point);
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
