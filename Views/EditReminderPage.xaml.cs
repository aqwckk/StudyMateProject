using StudyMateProject.Models;
using StudyMateProject.Services;

namespace StudyMateProject.Views;

[QueryProperty(nameof(Reminder), "Reminder")]
public partial class EditReminderPage : ContentPage
{
    private Reminder _reminder;
    private readonly INoteService _noteService;
    private readonly IReminderService _reminderService;
    private List<NoteBase> _notes;

    public Reminder Reminder
    {
        get => _reminder;
        set
        {
            _reminder = value;
            LoadReminderData();
        }
    }

    public EditReminderPage(INoteService noteService, IReminderService reminderService)
    {
        InitializeComponent();
        _noteService = noteService;
        _reminderService = reminderService;
        _notes = new List<NoteBase>();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadNotes();
    }

    private void LoadReminderData()
    {
        if (_reminder != null)
        {
            TitleEntry.Text = _reminder.Title;
            DescriptionEditor.Text = _reminder.Description;
            DueDatePicker.Date = _reminder.DueDate.Date;
            DueTimePicker.Time = _reminder.DueDate.TimeOfDay;
            CompletedCheckBox.IsChecked = _reminder.IsCompleted;

            // Select the related note in the picker
            if (!string.IsNullOrEmpty(_reminder.RelatedNoteId))
            {
                int index = _notes.FindIndex(n => n.Id == _reminder.RelatedNoteId);
                if (index >= 0)
                {
                    RelatedNotePicker.SelectedIndex = index;
                }
            }
        }
    }

    private async Task LoadNotes()
    {
        try
        {
            _notes = (await _noteService.GetAllNotesAsync()).ToList();
            RelatedNotePicker.ItemsSource = _notes;

            // Select the related note in the picker
            if (_reminder != null && !string.IsNullOrEmpty(_reminder.RelatedNoteId))
            {
                int index = _notes.FindIndex(n => n.Id == _reminder.RelatedNoteId);
                if (index >= 0)
                {
                    RelatedNotePicker.SelectedIndex = index;
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось загрузить заметки: {ex.Message}", "OK");
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TitleEntry.Text))
        {
            await DisplayAlert("Ошибка", "Заголовок обязателен", "OK");
            return;
        }

        try
        {
            // Update reminder properties
            _reminder.Title = TitleEntry.Text;
            _reminder.Description = DescriptionEditor.Text ?? string.Empty;

            // Combine date and time
            DateTime dueDate = DueDatePicker.Date;
            TimeSpan dueTime = DueTimePicker.Time;
            _reminder.DueDate = dueDate.Add(dueTime);

            _reminder.IsCompleted = CompletedCheckBox.IsChecked;

            // Set related note
            if (RelatedNotePicker.SelectedItem is NoteBase selectedNote)
            {
                _reminder.RelatedNoteId = selectedNote.Id;
            }
            else
            {
                _reminder.RelatedNoteId = string.Empty;
            }

            // Save the reminder
            await _reminderService.SaveReminderAsync(_reminder);

            // Navigate back
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось сохранить напоминание: {ex.Message}", "OK");
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}