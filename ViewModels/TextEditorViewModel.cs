using StudyMateTest.Services.TextEditorServices;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using StudyMateTest.Models.TextEditor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyMateTest.ViewModels
{
    public class TextEditorViewModel : INotifyPropertyChanged
    {
        private readonly ITextEditorService _textEditorService;

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand NewDocumentCommand { get; private set; }
        public ICommand OpenDocumentCommand { get; private set; }
        public ICommand SaveDocumentCommand { get; private set; }
        public ICommand SaveAsDocumentCommand { get; private set; }
        public ICommand ToggleBoldCommand { get; private set; }
        public ICommand ToggleItalicCommand { get; private set; }
        public ICommand ToggleUnderlineCommand { get; private set; }
        public ICommand SetAlignmentCommand { get; private set; }
        public ICommand CreateListCommand { get; private set; }
        public ICommand IncreaseIndentCommand { get; private set; }
        public ICommand DecreaseIndentCommand { get; private set; }
        public ICommand IncreaseFontSizeCommand { get; private set; }
        public ICommand DecreaseFontSizeCommand { get; private set; }


        public TextDocument CurrentDocument => _textEditorService.CurrentDocument;

        public string DocumentContent 
        {
            get => CurrentDocument?.Content ?? "";
            set 
            {
                if (CurrentDocument != null && CurrentDocument.Content != value) 
                {
                    CurrentDocument.Content = value;
                    OnPropertyChanged();
                }
            }
        }

        public string DocumentTitle 
        {
            get => CurrentDocument?.Title ?? "Новый документ";
            set 
            {
                if (CurrentDocument != null && CurrentDocument.Title != value) 
                {
                    CurrentDocument.Title = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsDocumentModified => CurrentDocument?.IsModified ?? false;

        public TextFormatting CurrentFormatting => _textEditorService.CurrentFormatting;

        public bool IsBold 
        {
            get => CurrentFormatting?.IsBold ?? false;
            set 
            {
                _textEditorService.SetBold(value);
                OnPropertyChanged();
            }
        }

        public bool IsItalic 
        {
            get => CurrentFormatting?.IsItalic ?? false;
            set 
            {
                _textEditorService.SetItalic(value);
                OnPropertyChanged();
            }
        }

        public bool IsUnderlined 
        {
            get => CurrentFormatting?.IsUnderlined ?? false;
            set 
            {
                _textEditorService?.SetUnderlined(value);
                OnPropertyChanged();
            }
        }

        public double FontSize 
        {
            get => CurrentFormatting?.FontSize ?? 14;
            set 
            {
                _textEditorService.SetFontSize(value);
                OnPropertyChanged();
                OnPropertyChanged(nameof(FontSizeText));
            }
        }

        public string FontSizeText => $"{FontSize:F0}";

        public string FontFamily 
        {
            get => CurrentFormatting?.FontFamily ?? "Arial";
            set 
            {
                _textEditorService.SetFontFamily(value);
                OnPropertyChanged();
            }
        }

        public Models.TextEditor.TextAlignment TextAlignment 
        {
            get => CurrentFormatting?.Alignment ?? Models.TextEditor.TextAlignment.Left;
            set 
            {
                _textEditorService.SetAlignment(value);
                OnPropertyChanged();
            }
        }

        public List<string> AvailableFonts { get; } = new List<string>
        {
            "Arial", "Times New Roman", "Calibri", "Verdana", "Tahoma", "Courier New",
            "Georgia", "Comic Sans MS"
        };

        public List<double> FontSizes { get; } = new List<double>
        {
            8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72
        };

        public TextEditorViewModel(ITextEditorService textEditorService) 
        {
            _textEditorService = textEditorService;
            InitializeCommands();
            SubscribeToEvents();
        }

        public TextEditorViewModel() 
        {
            _textEditorService = App.Current.Handler.MauiContext.Services.GetService<ITextEditorService>();
            InitializeCommands();
            SubscribeToEvents();
        }

        private void InitializeCommands() 
        {
            NewDocumentCommand = new Command(async () => await NewDocumentAsync());
            OpenDocumentCommand = new Command(async () => await OpenDocumentAsync());
            SaveDocumentCommand = new Command(async () => await SaveDocumentAsync());
            SaveAsDocumentCommand = new Command(async () => await SaveAsDocumentAsync());

            ToggleBoldCommand = new Command(() => IsBold = !IsBold);
            ToggleItalicCommand = new Command(() => IsItalic = !IsItalic);
            ToggleUnderlineCommand = new Command(() => IsUnderlined = !IsUnderlined);

            SetAlignmentCommand = new Command<string>(SetAlignment);
            CreateListCommand = new Command<string>(CreateList);

            IncreaseIndentCommand = new Command(() => _textEditorService.IncreaseIndent());
            DecreaseIndentCommand = new Command(() => _textEditorService.DecreaseIndent());

            IncreaseFontSizeCommand = new Command(() => FontSize = Math.Min(72, FontSize + 2));
            DecreaseFontSizeCommand = new Command(() => FontSize = Math.Max(8, FontSize - 2));
        }

        private void SubscribeToEvents() 
        {
            _textEditorService.DocumentChanged += OnDocumentChanged;
            _textEditorService.FormattingChanged += OnFormattingChanged;
        }

        private async Task NewDocumentAsync() 
        {
            if (IsDocumentModified) 
            {
                bool save = await Application.Current.MainPage.DisplayAlert(
                    "Несохраненные изменения",
                    "Документ был изменен. Сохранить изменения?",
                    "Да", "Нет");

                if (save) 
                {
                    await SaveDocumentAsync();
                }
            }

            _textEditorService.NewDocument();
        }

        private async Task OpenDocumentAsync() 
        {
            if (IsDocumentModified) 
            {
                bool save = await Application.Current.MainPage.DisplayAlert(
                    "Несохраненные изменения",
                    "Текущий документ был изменен. Сохранить изменения?",
                    "Да", "Нет");

                if (save) 
                {
                    await SaveDocumentAsync();
                }
            }
            await _textEditorService.OpenDocumentAsync();
        }

        private async Task SaveDocumentAsync() 
        {
            await _textEditorService.SaveDocumentAsync();
        }

        private async Task SaveAsDocumentAsync() 
        {
            await _textEditorService.SaveDocumentAsAsync();
        }

        private void SetAlignment(string alignment) 
        {
            if (Enum.TryParse<Models.TextEditor.TextAlignment>(alignment, out var textAlignment)) 
            {
                TextAlignment = textAlignment;
            }
        }

        private void CreateList(string listType) 
        {
            if (Enum.TryParse<ListType>(listType, out var type)) 
            {
                _textEditorService.CreateList(type);
            }
        }

        private void OnDocumentChanged(object sender, EventArgs e) 
        {
            OnPropertyChanged(nameof(CurrentDocument));
            OnPropertyChanged(nameof(DocumentContent));
            OnPropertyChanged(nameof(DocumentTitle));
            OnPropertyChanged(nameof(IsDocumentModified));
        }

        private void OnFormattingChanged(object sender, EventArgs e) 
        {
            OnPropertyChanged(nameof(CurrentFormatting));
            OnPropertyChanged(nameof(IsBold));
            OnPropertyChanged(nameof(IsItalic));
            OnPropertyChanged(nameof(IsUnderlined));
            OnPropertyChanged(nameof(FontSize));
            OnPropertyChanged(nameof(FontSizeText));
            OnPropertyChanged(nameof(FontFamily));
            OnPropertyChanged(nameof(TextAlignment));
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) 
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
