using StudyMateProject.Models;
using StudyMateProject.Services;

namespace StudyMateProject.Views;

public partial class SettingsPage : ContentPage
{
    private readonly INoteService _noteService;
    private readonly IReminderService _reminderService;
    private readonly IFileService _fileService;
    private AppSettings _settings;

    public SettingsPage(INoteService noteService, IReminderService reminderService, IFileService fileService)
    {
        InitializeComponent();
        _noteService = noteService;
        _reminderService = reminderService;
        _fileService = fileService;
        _settings = new AppSettings();

        // Set default values
        LanguagePicker.SelectedIndex = 0; // Русский по умолчанию
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadSettings();
        await UpdateStorageInfo();
    }

    private async Task LoadSettings()
    {
        try
        {
            // Load settings from file if it exists
            string settingsFile = "app_settings.json";
            if (await _fileService.FileExistsAsync(settingsFile))
            {
                var settingsData = await _fileService.ReadFileAsync(settingsFile);
                var settingsJson = System.Text.Encoding.UTF8.GetString(settingsData);
                _settings = System.Text.Json.JsonSerializer.Deserialize<AppSettings>(settingsJson) ?? new AppSettings();
            }

            // Apply settings to UI
            switch (_settings.Theme)
            {
                case "Light":
                    LightThemeRadio.IsChecked = true;
                    break;
                case "Dark":
                    DarkThemeRadio.IsChecked = true;
                    break;
                case "System":
                    SystemThemeRadio.IsChecked = true;
                    break;
            }

            // Set language based on settings
            string[] languages = { "ru-RU", "en-US", "es-ES", "fr-FR", "de-DE" };
            int languageIndex = Array.IndexOf(languages, _settings.Language);
            if (languageIndex >= 0)
            {
                LanguagePicker.SelectedIndex = languageIndex;
            }

            // Set autosave settings
            AutosaveCheckBox.IsChecked = _settings.AutosaveEnabled;
            AutosaveIntervalEntry.Text = _settings.AutosaveIntervalSeconds.ToString();
            AutosaveIntervalEntry.IsEnabled = _settings.AutosaveEnabled;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось загрузить настройки: {ex.Message}", "OK");
        }
    }

    private async Task SaveSettings()
    {
        try
        {
            string settingsJson = System.Text.Json.JsonSerializer.Serialize(_settings);
            byte[] settingsData = System.Text.Encoding.UTF8.GetBytes(settingsJson);
            await _fileService.SaveFileAsync("app_settings.json", settingsData);

            // Apply theme
            ApplyTheme();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось сохранить настройки: {ex.Message}", "OK");
        }
    }

    private void ApplyTheme()
    {
        switch (_settings.Theme)
        {
            case "Light":
                Application.Current.UserAppTheme = AppTheme.Light;
                break;
            case "Dark":
                Application.Current.UserAppTheme = AppTheme.Dark;
                break;
            case "System":
                Application.Current.UserAppTheme = AppTheme.Unspecified;
                break;
        }
    }

    private async Task UpdateStorageInfo()
    {
        try
        {
            // Count notes
            var notes = await _noteService.GetAllNotesAsync();
            NotesCountLabel.Text = notes.Count().ToString();

            // Count reminders
            var reminders = await _reminderService.GetAllRemindersAsync();
            RemindersCountLabel.Text = reminders.Count().ToString();

            // Calculate storage used
            long storageUsed = 0;
            var files = await _fileService.GetFilesAsync();
            foreach (var file in files)
            {
                try
                {
                    var fileData = await _fileService.ReadFileAsync(file);
                    storageUsed += fileData.Length;
                }
                catch
                {
                    // Skip files that can't be read
                }
            }

            // Convert to human-readable format
            string storageText;
            if (storageUsed < 1024)
            {
                storageText = $"{storageUsed} Б";
            }
            else if (storageUsed < 1024 * 1024)
            {
                storageText = $"{storageUsed / 1024.0:F2} КБ";
            }
            else if (storageUsed < 1024 * 1024 * 1024)
            {
                storageText = $"{storageUsed / (1024.0 * 1024.0):F2} МБ";
            }
            else
            {
                storageText = $"{storageUsed / (1024.0 * 1024.0 * 1024.0):F2} ГБ";
            }

            StorageUsedLabel.Text = storageText;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось обновить информацию о хранилище: {ex.Message}", "OK");
        }
    }

    private void OnThemeChanged(object sender, CheckedChangedEventArgs e)
    {
        if (!e.Value) return; // Only process when a radio button is checked

        if (sender == LightThemeRadio)
        {
            _settings.Theme = "Light";
        }
        else if (sender == DarkThemeRadio)
        {
            _settings.Theme = "Dark";
        }
        else if (sender == SystemThemeRadio)
        {
            _settings.Theme = "System";
        }

        SaveSettings();
    }

    private void OnLanguageChanged(object sender, EventArgs e)
    {
        if (LanguagePicker.SelectedIndex < 0) return;

        string[] languages = { "ru-RU", "en-US", "es-ES", "fr-FR", "de-DE" };
        if (LanguagePicker.SelectedIndex < languages.Length)
        {
            _settings.Language = languages[LanguagePicker.SelectedIndex];
            SaveSettings();
        }
    }

    private void OnAutosaveChanged(object sender, CheckedChangedEventArgs e)
    {
        _settings.AutosaveEnabled = e.Value;
        AutosaveIntervalEntry.IsEnabled = e.Value;
        SaveSettings();
    }

    private void OnAutosaveIntervalChanged(object sender, TextChangedEventArgs e)
    {
        if (int.TryParse(e.NewTextValue, out int interval) && interval > 0)
        {
            _settings.AutosaveIntervalSeconds = interval;
            SaveSettings();
        }
    }

    private async void OnClearDataClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert(
            "Очистить все данные",
            "Вы уверены, что хотите очистить все данные? Это действие нельзя отменить.",
            "Да", "Нет");

        if (confirm)
        {
            try
            {
                // Get all notes and delete them
                var notes = await _noteService.GetAllNotesAsync();
                foreach (var note in notes)
                {
                    await _noteService.DeleteNoteAsync(note.Id);
                }

                // Get all reminders and delete them
                var reminders = await _reminderService.GetAllRemindersAsync();
                foreach (var reminder in reminders)
                {
                    await _reminderService.DeleteReminderAsync(reminder.Id);
                }

                // Reset settings
                _settings = new AppSettings();
                await SaveSettings();

                // Update UI
                await LoadSettings();
                await UpdateStorageInfo();

                await DisplayAlert("Успех", "Все данные были очищены.", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось очистить данные: {ex.Message}", "OK");
            }
        }
    }
}