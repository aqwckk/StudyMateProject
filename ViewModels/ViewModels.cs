using System.Collections.ObjectModel;
using System.Windows.Input;
using StudyMateProject.Models;
using StudyMateProject.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace StudyMateProject.ViewModels
{
    /// <summary>
    /// Базовая ViewModel с общей функциональностью
    /// </summary>
    public abstract class BaseViewModel : ObservableObject
    {
        private bool _isBusy;
        private string _title = string.Empty;

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }
    }

    /// <summary>
    /// ViewModel для страницы списка заметок
    /// </summary>
    public class NotesViewModel : BaseViewModel
    {
        private readonly INoteService _noteService;
        private ObservableCollection<NoteBase> _notes;
        private NoteBase _selectedNote;
        private string _searchText;

        public ObservableCollection<NoteBase> Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
        }

        public NoteBase SelectedNote
        {
            get => _selectedNote;
            set => SetProperty(ref _selectedNote, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    SearchNotesCommand.Execute(null);
                }
            }
        }

        public ICommand LoadNotesCommand { get; }
        public ICommand SearchNotesCommand { get; }
        public ICommand AddTextNoteCommand { get; }
        public ICommand AddGraphicNoteCommand { get; }
        public ICommand DeleteNoteCommand { get; }
        public ICommand NoteSelectedCommand { get; }

        public NotesViewModel(INoteService noteService)
        {
            _noteService = noteService;
            _notes = new ObservableCollection<NoteBase>();
            _searchText = string.Empty;

            Title = "Мои заметки";

            LoadNotesCommand = new AsyncRelayCommand(LoadNotesAsync);
            SearchNotesCommand = new AsyncRelayCommand(SearchNotesAsync);
            AddTextNoteCommand = new AsyncRelayCommand(AddTextNoteAsync);
            AddGraphicNoteCommand = new AsyncRelayCommand(AddGraphicNoteAsync);
            DeleteNoteCommand = new AsyncRelayCommand<string>(DeleteNoteAsync);
            NoteSelectedCommand = new AsyncRelayCommand<NoteBase>(NoteSelectedAsync);
        }

        private async Task LoadNotesAsync()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                Notes.Clear();
                var notes = await _noteService.GetAllNotesAsync();
                foreach (var note in notes.OrderByDescending(n => n.ModifiedDate))
                {
                    Notes.Add(note);
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", $"Не удалось загрузить заметки: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task SearchNotesAsync()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                Notes.Clear();
                var notes = await _noteService.SearchNotesAsync(SearchText);
                foreach (var note in notes.OrderByDescending(n => n.ModifiedDate))
                {
                    Notes.Add(note);
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", $"Не удалось выполнить поиск: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task AddTextNoteAsync()
        {
            var textNote = new TextNote
            {
                Title = "Новая текстовая заметка",
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now
            };

            await _noteService.SaveTextNoteAsync(textNote);
            Notes.Insert(0, textNote);
            await NoteSelectedAsync(textNote);
        }

        private async Task AddGraphicNoteAsync()
        {
            var graphicNote = new GraphicNote
            {
                Title = "Новая графическая заметка",
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now
            };

            await _noteService.SaveGraphicNoteAsync(graphicNote);
            Notes.Insert(0, graphicNote);
            await NoteSelectedAsync(graphicNote);
        }

        private async Task DeleteNoteAsync(string noteId)
        {
            if (string.IsNullOrEmpty(noteId))
                return;

            bool confirm = await Shell.Current.DisplayAlert(
                "Удаление заметки",
                "Вы уверены, что хотите удалить эту заметку?",
                "Да", "Нет");

            if (confirm)
            {
                var noteToDelete = Notes.FirstOrDefault(n => n.Id == noteId);
                if (noteToDelete != null)
                {
                    await _noteService.DeleteNoteAsync(noteId);
                    Notes.Remove(noteToDelete);
                }
            }
        }

        private async Task NoteSelectedAsync(NoteBase note)
        {
            if (note == null)
                return;

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "NoteId", note.Id }
            };

            // Navigate to the appropriate page based on note type
            if (note is TextNote)
            {
                await Shell.Current.GoToAsync($"textNote", parameters);
            }
            else if (note is GraphicNote)
            {
                await Shell.Current.GoToAsync($"graphicNote", parameters);
            }
        }
    }

    /// <summary>
    /// ViewModel для страницы текстовой заметки
    /// </summary>
    public class TextNoteViewModel : BaseViewModel
    {
        private readonly INoteService _noteService;
        private readonly IFileService _fileService;
        private TextNote _note;
        private string _content;
        private string _noteId;

        public TextNote Note
        {
            get => _note;
            set
            {
                if (SetProperty(ref _note, value))
                {
                    Title = value?.Title ?? "Новая заметка";
                    Content = value?.Content ?? string.Empty;
                }
            }
        }

        public string NoteId
        {
            get => _noteId;
            set
            {
                if (SetProperty(ref _noteId, value) && !string.IsNullOrEmpty(value))
                {
                    LoadNoteCommand.Execute(null);
                }
            }
        }

        public string Content
        {
            get => _content;
            set => SetProperty(ref _content, value);
        }

        public ICommand LoadNoteCommand { get; }
        public ICommand SaveNoteCommand { get; }
        public ICommand ExportNoteCommand { get; }

        public TextNoteViewModel(INoteService noteService, IFileService fileService)
        {
            _noteService = noteService;
            _fileService = fileService;
            _note = new TextNote();
            _content = string.Empty;
            _noteId = string.Empty;

            LoadNoteCommand = new AsyncRelayCommand(LoadNoteAsync);
            SaveNoteCommand = new AsyncRelayCommand(SaveNoteAsync);
            ExportNoteCommand = new AsyncRelayCommand<string>(ExportNoteAsync);
        }

        private async Task LoadNoteAsync()
        {
            if (IsBusy || string.IsNullOrEmpty(NoteId))
                return;

            IsBusy = true;

            try
            {
                Note = await _noteService.GetTextNoteAsync(NoteId);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", $"Не удалось загрузить заметку: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task SaveNoteAsync()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                Note.Content = Content;
                Note.ModifiedDate = DateTime.Now;

                await _noteService.SaveTextNoteAsync(Note);
                await Shell.Current.DisplayAlert("Успех", "Заметка сохранена успешно!", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", $"Не удалось сохранить заметку: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ExportNoteAsync(string format)
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                // Save before exporting to ensure latest content is exported
                Note.Content = Content;
                Note.ModifiedDate = DateTime.Now;
                await _noteService.SaveTextNoteAsync(Note);

                // Export using the file service
                await _fileService.ExportNoteAsync(Note, format);
                await Shell.Current.DisplayAlert("Успех", $"Заметка экспортирована в формате {format}", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", $"Не удалось экспортировать заметку: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }

    /// <summary>
    /// ViewModel для страницы графической заметки
    /// </summary>
    public partial class GraphicNoteViewModel : BaseViewModel
    {
        private readonly INoteService _noteService;
        private readonly IFileService _fileService;
        private GraphicNote _note;
        private string _noteId;
        private ObservableCollection<DrawingStroke> _strokes;
        private float _selectedStrokeWidth;
        private string _selectedColor;
        private string _selectedTool;

        public GraphicNote Note
        {
            get => _note;
            set
            {
                if (SetProperty(ref _note, value))
                {
                    Title = value?.Title ?? "Новая графическая заметка";
                    LoadStrokesFromNote();
                }
            }
        }

        public string NoteId
        {
            get => _noteId;
            set
            {
                if (SetProperty(ref _noteId, value) && !string.IsNullOrEmpty(value))
                {
                    LoadNoteCommand.Execute(null);
                }
            }
        }

        public ObservableCollection<DrawingStroke> Strokes
        {
            get => _strokes;
            set => SetProperty(ref _strokes, value);
        }

        public float SelectedStrokeWidth
        {
            get => _selectedStrokeWidth;
            set => SetProperty(ref _selectedStrokeWidth, value);
        }

        public string SelectedColor
        {
            get => _selectedColor;
            set => SetProperty(ref _selectedColor, value);
        }

        public string SelectedTool
        {
            get => _selectedTool;
            set => SetProperty(ref _selectedTool, value);
        }

        public ICommand LoadNoteCommand { get; }
        public ICommand SaveNoteCommand { get; }
        public ICommand AddStrokeCommand { get; }
        public ICommand ClearStrokesCommand { get; }
        public ICommand UndoCommand { get; }

        public GraphicNoteViewModel(INoteService noteService, IFileService fileService)
        {
            _noteService = noteService;
            _fileService = fileService;
            _note = new GraphicNote();
            _noteId = string.Empty;
            _strokes = new ObservableCollection<DrawingStroke>();
            _selectedStrokeWidth = 5;
            _selectedColor = "#000000";
            _selectedTool = "Pen";

            LoadNoteCommand = new AsyncRelayCommand(LoadNoteAsync);
            SaveNoteCommand = new AsyncRelayCommand(SaveNoteAsync);
            AddStrokeCommand = new RelayCommand<DrawingStroke>(AddStroke);
            ClearStrokesCommand = new RelayCommand(ClearStrokes);
            UndoCommand = new RelayCommand(UndoLastStroke);
        }

        private async Task LoadNoteAsync()
        {
            if (IsBusy || string.IsNullOrEmpty(NoteId))
                return;

            IsBusy = true;

            try
            {
                Note = await _noteService.GetGraphicNoteAsync(NoteId);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", $"Не удалось загрузить заметку: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void LoadStrokesFromNote()
        {
            Strokes.Clear();
            if (string.IsNullOrEmpty(Note.SkiaSharpData))
                return;

            try
            {
                var loadedStrokes = System.Text.Json.JsonSerializer.Deserialize<List<DrawingStroke>>(Note.SkiaSharpData);
                if (loadedStrokes != null)
                {
                    foreach (var stroke in loadedStrokes)
                    {
                        // Проверка валидности цвета
                        if (string.IsNullOrEmpty(stroke.Color) || !stroke.Color.StartsWith("#"))
                        {
                            stroke.Color = "#000000"; // По умолчанию черный
                        }

                        // Проверка валидности StrokeType
                        if (string.IsNullOrEmpty(stroke.StrokeType))
                        {
                            stroke.StrokeType = "Pen"; // По умолчанию ручка
                        }

                        Strokes.Add(stroke);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading strokes: {ex.Message}");
                // В случае ошибки десериализации, очищаем строки
                Strokes.Clear();
            }
        }

        private async Task SaveNoteAsync()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                // Serialize drawing strokes
                Note.SkiaSharpData = System.Text.Json.JsonSerializer.Serialize(Strokes);
                Note.ModifiedDate = DateTime.Now;

                await _noteService.SaveGraphicNoteAsync(Note);
                await Shell.Current.DisplayAlert("Успех", "Заметка сохранена успешно!", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", $"Не удалось сохранить заметку: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void AddStroke(DrawingStroke stroke)
        {
            if (stroke != null)
            {
                Strokes.Add(stroke);
            }
        }

        private void ClearStrokes()
        {
            Strokes.Clear();
        }

        private void UndoLastStroke()
        {
            if (Strokes.Count > 0)
            {
                Strokes.RemoveAt(Strokes.Count - 1);
            }
        }

        [RelayCommand]
        private async Task ExportNote(string format)
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                if (string.IsNullOrEmpty(format))
                {
                    await Shell.Current.DisplayAlert("Ошибка", "Формат экспорта не указан", "OK");
                    return;
                }

                // Save before exporting to ensure latest content is exported
                Note.SkiaSharpData = System.Text.Json.JsonSerializer.Serialize(Strokes);
                Note.ModifiedDate = DateTime.Now;
                await _noteService.SaveGraphicNoteAsync(Note);

                // Export using the file service
                await _fileService.ExportNoteAsync(Note, format);
                await Shell.Current.DisplayAlert("Успех", $"Заметка экспортирована в формате {format}", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", $"Не удалось экспортировать заметку: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }

    /// <summary>
    /// ViewModel для страницы напоминаний
    /// </summary>
    public class RemindersViewModel : BaseViewModel
    {
        private readonly IReminderService _reminderService;
        private ObservableCollection<Reminder> _reminders;
        private Reminder _selectedReminder;

        public ObservableCollection<Reminder> Reminders
        {
            get => _reminders;
            set => SetProperty(ref _reminders, value);
        }

        public Reminder SelectedReminder
        {
            get => _selectedReminder;
            set => SetProperty(ref _selectedReminder, value);
        }

        public ICommand LoadRemindersCommand { get; }
        public ICommand AddReminderCommand { get; }
        public ICommand DeleteReminderCommand { get; }
        public ICommand ToggleCompletedCommand { get; }
        public ICommand EditReminderCommand { get; }

        public RemindersViewModel(IReminderService reminderService)
        {
            _reminderService = reminderService;
            _reminders = new ObservableCollection<Reminder>();

            Title = "Напоминания";

            LoadRemindersCommand = new AsyncRelayCommand(LoadRemindersAsync);
            AddReminderCommand = new AsyncRelayCommand(AddReminderAsync);
            DeleteReminderCommand = new AsyncRelayCommand<string>(DeleteReminderAsync);
            ToggleCompletedCommand = new AsyncRelayCommand<string>(ToggleCompletedAsync);
            EditReminderCommand = new AsyncRelayCommand<Reminder>(EditReminderAsync);
        }

        private async Task LoadRemindersAsync()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                Reminders.Clear();
                var reminders = await _reminderService.GetAllRemindersAsync();
                foreach (var reminder in reminders.OrderBy(r => r.DueDate))
                {
                    Reminders.Add(reminder);
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", $"Не удалось загрузить напоминания: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task AddReminderAsync()
        {
            // Create a new reminder with default values
            var reminder = new Reminder
            {
                Title = "Новое напоминание",
                DueDate = DateTime.Now.AddDays(1),
                IsCompleted = false
            };

            // Show dialog to edit the reminder
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "Reminder", reminder }
            };

            await Shell.Current.GoToAsync("editReminder", parameters);
        }

        private async Task DeleteReminderAsync(string reminderId)
        {
            if (string.IsNullOrEmpty(reminderId))
                return;

            bool confirm = await Shell.Current.DisplayAlert(
                "Удаление напоминания",
                "Вы уверены, что хотите удалить это напоминание?",
                "Да", "Нет");

            if (confirm)
            {
                var reminderToDelete = Reminders.FirstOrDefault(r => r.Id == reminderId);
                if (reminderToDelete != null)
                {
                    await _reminderService.DeleteReminderAsync(reminderId);
                    Reminders.Remove(reminderToDelete);
                }
            }
        }

        private async Task ToggleCompletedAsync(string reminderId)
        {
            if (string.IsNullOrEmpty(reminderId))
                return;

            var reminder = Reminders.FirstOrDefault(r => r.Id == reminderId);
            if (reminder != null)
            {
                reminder.IsCompleted = !reminder.IsCompleted;
                await _reminderService.SetReminderCompletedAsync(reminderId, reminder.IsCompleted);

                // Resort the list
                ReorderReminders();
            }
        }

        private async Task EditReminderAsync(Reminder reminder)
        {
            if (reminder == null)
                return;

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "Reminder", reminder }
            };

            await Shell.Current.GoToAsync("editReminder", parameters);
        }

        private void ReorderReminders()
        {
            var ordered = Reminders.OrderBy(r => r.DueDate).ToList();
            Reminders.Clear();
            foreach (var reminder in ordered)
            {
                Reminders.Add(reminder);
            }
        }
    }

    /// <summary>
    /// ViewModel для страницы калькулятора
    /// </summary>
    public class CalculatorViewModel : BaseViewModel
    {
        private string _expression;
        private string _result;
        private ObservableCollection<string> _history;

        public string Expression
        {
            get => _expression;
            set => SetProperty(ref _expression, value);
        }

        public string Result
        {
            get => _result;
            set => SetProperty(ref _result, value);
        }

        public ObservableCollection<string> History
        {
            get => _history;
            set => SetProperty(ref _history, value);
        }

        public ICommand AppendCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand BackspaceCommand { get; }
        public ICommand CalculateCommand { get; }
        public ICommand ClearHistoryCommand { get; }
        public ICommand UseResultCommand { get; }

        public CalculatorViewModel()
        {
            _expression = string.Empty;
            _result = string.Empty;
            _history = new ObservableCollection<string>();

            Title = "Калькулятор";

            AppendCommand = new RelayCommand<string>(Append);
            ClearCommand = new RelayCommand(Clear);
            BackspaceCommand = new RelayCommand(Backspace);
            CalculateCommand = new RelayCommand(Calculate);
            ClearHistoryCommand = new RelayCommand(ClearHistory);
            UseResultCommand = new RelayCommand(UseResult);
        }

        private void Append(string value)
        {
            Expression += value;
        }

        private void Clear()
        {
            Expression = string.Empty;
            Result = string.Empty;
        }

        private void Backspace()
        {
            if (Expression.Length > 0)
            {
                Expression = Expression.Substring(0, Expression.Length - 1);
            }
        }

        private void Calculate()
        {
            if (string.IsNullOrEmpty(Expression))
                return;

            try
            {
                // Create an instance of System.Data.DataTable
                System.Data.DataTable table = new System.Data.DataTable();

                // Use Compute method to evaluate the expression
                var result = table.Compute(Expression, "");

                // Convert the result to string
                Result = result.ToString();

                // Add to history
                History.Add($"{Expression} = {Result}");
            }
            catch (Exception ex)
            {
                Result = "Ошибка";
            }
        }

        private void ClearHistory()
        {
            History.Clear();
        }

        private void UseResult()
        {
            if (!string.IsNullOrEmpty(Result) && Result != "Ошибка")
            {
                Expression = Result;
                Result = string.Empty;
            }
        }
    }
}