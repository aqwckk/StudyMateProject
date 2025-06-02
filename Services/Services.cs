using System.Collections.ObjectModel;
using System.Text.Json;
using StudyMateProject.Models;
using Plugin.LocalNotification;

namespace StudyMateProject.Services
{
    /// <summary>
    /// Интерфейс для сервиса заметок
    /// </summary>
    public interface INoteService
    {
        Task<IEnumerable<NoteBase>> GetAllNotesAsync();
        Task<TextNote> GetTextNoteAsync(string id);
        Task<GraphicNote> GetGraphicNoteAsync(string id);
        Task SaveTextNoteAsync(TextNote note);
        Task SaveGraphicNoteAsync(GraphicNote note);
        Task DeleteNoteAsync(string id);
        Task<IEnumerable<NoteBase>> SearchNotesAsync(string searchTerm);
    }

    /// <summary>
    /// Интерфейс для сервиса напоминаний
    /// </summary>
    public interface IReminderService
    {
        Task<IEnumerable<Reminder>> GetAllRemindersAsync();
        Task<Reminder> GetReminderAsync(string id);
        Task SaveReminderAsync(Reminder reminder);
        Task DeleteReminderAsync(string id);
        Task<IEnumerable<Reminder>> GetUpcomingRemindersAsync(int days = 7);
        Task SetReminderCompletedAsync(string id, bool isCompleted);
        Task ScheduleLocalNotificationAsync(Reminder reminder);
    }

    /// <summary>
    /// Интерфейс для сервиса работы с файлами
    /// </summary>
    public interface IFileService
    {
        Task<string> SaveFileAsync(string filename, byte[] data);
        Task<byte[]> ReadFileAsync(string filename);
        Task<bool> FileExistsAsync(string filename);
        Task DeleteFileAsync(string filename);
        Task<IEnumerable<string>> GetFilesAsync(string extension = "");
        Task<string> GetAppDataDirectoryAsync();
        Task ExportNoteAsync(NoteBase note, string format);
        Task<NoteBase> ImportNoteAsync(string filePath);
    }

    /// <summary>
    /// Реализация сервиса заметок
    /// </summary>
    public class NoteService : INoteService
    {
        private readonly IFileService _fileService;
        private const string TEXT_NOTES_DIRECTORY = "text_notes";
        private const string GRAPHIC_NOTES_DIRECTORY = "graphic_notes";
        private const string NOTE_INDEX_FILE = "note_index.json";

        private List<NoteBase> _noteIndex;

        public NoteService(IFileService fileService)
        {
            _fileService = fileService;
            _noteIndex = new List<NoteBase>();
            InitializeAsync().Wait();
        }

        private async Task InitializeAsync()
        {
            try
            {
                if (await _fileService.FileExistsAsync(NOTE_INDEX_FILE))
                {
                    var indexData = await _fileService.ReadFileAsync(NOTE_INDEX_FILE);
                    var indexJson = System.Text.Encoding.UTF8.GetString(indexData);
                    _noteIndex = JsonSerializer.Deserialize<List<NoteBase>>(indexJson) ?? new List<NoteBase>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing note service: {ex.Message}");
                _noteIndex = new List<NoteBase>();
            }
        }

        private async Task SaveIndexAsync()
        {
            var indexJson = JsonSerializer.Serialize(_noteIndex);
            var indexData = System.Text.Encoding.UTF8.GetBytes(indexJson);
            await _fileService.SaveFileAsync(NOTE_INDEX_FILE, indexData);
        }

        public async Task<IEnumerable<NoteBase>> GetAllNotesAsync()
        {
            return _noteIndex;
        }

        public async Task<TextNote> GetTextNoteAsync(string id)
        {
            var noteFileName = $"{TEXT_NOTES_DIRECTORY}/{id}.json";
            if (!await _fileService.FileExistsAsync(noteFileName))
                throw new FileNotFoundException($"Text note with id {id} not found");

            var noteData = await _fileService.ReadFileAsync(noteFileName);
            var noteJson = System.Text.Encoding.UTF8.GetString(noteData);
            return JsonSerializer.Deserialize<TextNote>(noteJson) ?? new TextNote();
        }

        public async Task<GraphicNote> GetGraphicNoteAsync(string id)
        {
            var noteFileName = $"{GRAPHIC_NOTES_DIRECTORY}/{id}.json";
            if (!await _fileService.FileExistsAsync(noteFileName))
                throw new FileNotFoundException($"Graphic note with id {id} not found");

            var noteData = await _fileService.ReadFileAsync(noteFileName);
            var noteJson = System.Text.Encoding.UTF8.GetString(noteData);
            return JsonSerializer.Deserialize<GraphicNote>(noteJson) ?? new GraphicNote();
        }

        public async Task SaveTextNoteAsync(TextNote note)
        {
            // Update modified date
            note.ModifiedDate = DateTime.Now;

            // Save the note data
            var noteJson = JsonSerializer.Serialize(note);
            var noteData = System.Text.Encoding.UTF8.GetBytes(noteJson);
            var noteFileName = $"{TEXT_NOTES_DIRECTORY}/{note.Id}.json";
            await _fileService.SaveFileAsync(noteFileName, noteData);

            // Update the index
            var existingNote = _noteIndex.FirstOrDefault(n => n.Id == note.Id);
            if (existingNote != null)
            {
                existingNote.Title = note.Title;
                existingNote.ModifiedDate = note.ModifiedDate;
                existingNote.Tags = note.Tags;
            }
            else
            {
                _noteIndex.Add(note);
            }

            await SaveIndexAsync();
        }

        public async Task SaveGraphicNoteAsync(GraphicNote note)
        {
            // Update modified date
            note.ModifiedDate = DateTime.Now;

            // Save the note data
            var noteJson = JsonSerializer.Serialize(note);
            var noteData = System.Text.Encoding.UTF8.GetBytes(noteJson);
            var noteFileName = $"{GRAPHIC_NOTES_DIRECTORY}/{note.Id}.json";
            await _fileService.SaveFileAsync(noteFileName, noteData);

            // Update the index
            var existingNote = _noteIndex.FirstOrDefault(n => n.Id == note.Id);
            if (existingNote != null)
            {
                existingNote.Title = note.Title;
                existingNote.ModifiedDate = note.ModifiedDate;
                existingNote.Tags = note.Tags;
            }
            else
            {
                _noteIndex.Add(note);
            }

            await SaveIndexAsync();
        }

        public async Task DeleteNoteAsync(string id)
        {
            var existingNote = _noteIndex.FirstOrDefault(n => n.Id == id);
            if (existingNote == null)
                return;

            // Remove from index
            _noteIndex.Remove(existingNote);
            await SaveIndexAsync();

            // Delete the note file
            if (existingNote is TextNote)
            {
                var noteFileName = $"{TEXT_NOTES_DIRECTORY}/{id}.json";
                if (await _fileService.FileExistsAsync(noteFileName))
                    await _fileService.DeleteFileAsync(noteFileName);
            }
            else if (existingNote is GraphicNote)
            {
                var noteFileName = $"{GRAPHIC_NOTES_DIRECTORY}/{id}.json";
                if (await _fileService.FileExistsAsync(noteFileName))
                    await _fileService.DeleteFileAsync(noteFileName);
            }
        }

        public async Task<IEnumerable<NoteBase>> SearchNotesAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return _noteIndex;

            searchTerm = searchTerm.ToLowerInvariant();

            var results = new List<NoteBase>();

            // Search in titles and tags from the index
            var indexResults = _noteIndex.Where(n =>
                n.Title.ToLowerInvariant().Contains(searchTerm) ||
                n.Tags.Any(t => t.ToLowerInvariant().Contains(searchTerm))
            ).ToList();

            results.AddRange(indexResults);

            // For text notes, we need to search in content too
            foreach (var note in _noteIndex.Where(n => n is TextNote))
            {
                if (results.Contains(note))
                    continue;

                try
                {
                    var textNote = await GetTextNoteAsync(note.Id);
                    if (textNote.Content.ToLowerInvariant().Contains(searchTerm))
                        results.Add(note);
                }
                catch (Exception)
                {
                    // Skip notes that can't be loaded
                }
            }

            return results;
        }
    }

    /// <summary>
    /// Реализация сервиса напоминаний
    /// </summary>
    public class ReminderService : IReminderService
    {
        private readonly IFileService _fileService;
        private const string REMINDERS_FILE = "reminders.json";
        private List<Reminder> _reminders;

        public ReminderService(IFileService fileService)
        {
            _fileService = fileService;
            _reminders = new List<Reminder>();
            InitializeAsync().Wait();
        }

        private async Task InitializeAsync()
        {
            try
            {
                if (await _fileService.FileExistsAsync(REMINDERS_FILE))
                {
                    var remindersData = await _fileService.ReadFileAsync(REMINDERS_FILE);
                    var remindersJson = System.Text.Encoding.UTF8.GetString(remindersData);
                    _reminders = JsonSerializer.Deserialize<List<Reminder>>(remindersJson) ?? new List<Reminder>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing reminder service: {ex.Message}");
                _reminders = new List<Reminder>();
            }
        }

        private async Task SaveRemindersAsync()
        {
            var remindersJson = JsonSerializer.Serialize(_reminders);
            var remindersData = System.Text.Encoding.UTF8.GetBytes(remindersJson);
            await _fileService.SaveFileAsync(REMINDERS_FILE, remindersData);
        }

        public async Task<IEnumerable<Reminder>> GetAllRemindersAsync()
        {
            return _reminders;
        }

        public async Task<Reminder> GetReminderAsync(string id)
        {
            var reminder = _reminders.FirstOrDefault(r => r.Id == id);
            if (reminder == null)
                throw new Exception($"Reminder with id {id} not found");

            return reminder;
        }

        public async Task SaveReminderAsync(Reminder reminder)
        {
            var existingReminder = _reminders.FirstOrDefault(r => r.Id == reminder.Id);
            if (existingReminder != null)
            {
                // Update existing reminder
                var index = _reminders.IndexOf(existingReminder);
                _reminders[index] = reminder;
            }
            else
            {
                // Add new reminder
                _reminders.Add(reminder);
            }

            await SaveRemindersAsync();
            await ScheduleLocalNotificationAsync(reminder);
        }

        public async Task DeleteReminderAsync(string id)
        {
            var reminder = _reminders.FirstOrDefault(r => r.Id == id);
            if (reminder != null)
            {
                _reminders.Remove(reminder);
                await SaveRemindersAsync();

                // Cancel notification if scheduled
                try
                {
                    // Исправлено: убран await т.к. метод возвращает bool, а не Task
                    LocalNotificationCenter.Current.Cancel(id.GetHashCode());
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to cancel notification: {ex.Message}");
                }
            }
        }

        public async Task<IEnumerable<Reminder>> GetUpcomingRemindersAsync(int days = 7)
        {
            var cutoffDate = DateTime.Now.AddDays(days);
            return _reminders.Where(r => !r.IsCompleted && r.DueDate <= cutoffDate)
                            .OrderBy(r => r.DueDate);
        }

        public async Task SetReminderCompletedAsync(string id, bool isCompleted)
        {
            var reminder = _reminders.FirstOrDefault(r => r.Id == id);
            if (reminder != null)
            {
                reminder.IsCompleted = isCompleted;
                await SaveRemindersAsync();

                if (isCompleted)
                {
                    // Cancel notification if completed
                    try
                    {
                        // Исправлено: убран await т.к. метод возвращает bool, а не Task
                        LocalNotificationCenter.Current.Cancel(id.GetHashCode());
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to cancel notification: {ex.Message}");
                    }
                }
                else
                {
                    // Reschedule if uncompleted and still in future
                    if (reminder.DueDate > DateTime.Now)
                    {
                        await ScheduleLocalNotificationAsync(reminder);
                    }
                }
            }
        }

        public async Task ScheduleLocalNotificationAsync(Reminder reminder)
        {
            // Skip scheduling if the reminder is completed or in the past
            if (reminder.IsCompleted || reminder.DueDate <= DateTime.Now)
                return;

            try
            {
                var notification = new NotificationRequest
                {
                    NotificationId = reminder.Id.GetHashCode(),
                    Title = reminder.Title,
                    Description = reminder.Description,
                    Schedule = new NotificationRequestSchedule
                    {
                        NotifyTime = reminder.DueDate
                    }
                };

                // Добавляем ReturningData для идентификации напоминания при нажатии
                notification.ReturningData = reminder.Id;

                await LocalNotificationCenter.Current.Show(notification);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to schedule notification: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Реализация сервиса работы с файлами
    /// </summary>
    public class FileService : IFileService
    {
        public async Task<string> SaveFileAsync(string filename, byte[] data)
        {
            try
            {
                string filePath = Path.Combine(await GetAppDataDirectoryAsync(), filename);

                // Create directory if it doesn't exist
                string directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                await File.WriteAllBytesAsync(filePath, data);
                return filePath;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving file: {ex.Message}");
                throw;
            }
        }

        public async Task<byte[]> ReadFileAsync(string filename)
        {
            try
            {
                string filePath = Path.Combine(await GetAppDataDirectoryAsync(), filename);
                return await File.ReadAllBytesAsync(filePath);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error reading file: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> FileExistsAsync(string filename)
        {
            try
            {
                string filePath = Path.Combine(await GetAppDataDirectoryAsync(), filename);
                return File.Exists(filePath);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking if file exists: {ex.Message}");
                return false;
            }
        }

        public async Task DeleteFileAsync(string filename)
        {
            try
            {
                string filePath = Path.Combine(await GetAppDataDirectoryAsync(), filename);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting file: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<string>> GetFilesAsync(string extension = "")
        {
            try
            {
                string directory = await GetAppDataDirectoryAsync();

                if (!Directory.Exists(directory))
                    return new List<string>();

                string searchPattern = string.IsNullOrEmpty(extension) ? "*" : $"*.{extension}";
                return Directory.GetFiles(directory, searchPattern, SearchOption.AllDirectories)
                    .Select(f => Path.GetRelativePath(directory, f));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting files: {ex.Message}");
                return new List<string>();
            }
        }

        public async Task<string> GetAppDataDirectoryAsync()
        {
            return FileSystem.AppDataDirectory;
        }

        public async Task ExportNoteAsync(NoteBase note, string format)
        {
            try
            {
                // Determine export format and create the appropriate file
                string fileName;
                byte[] fileData;

                if (note is TextNote textNote)
                {
                    switch (format.ToLowerInvariant())
                    {
                        case "txt":
                            fileName = $"{note.Title}.txt";
                            fileData = System.Text.Encoding.UTF8.GetBytes(textNote.Content);
                            break;

                        case "json":
                            fileName = $"{note.Title}.json";
                            fileData = System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(textNote));
                            break;

                        default:
                            throw new Exception($"Unsupported export format: {format}");
                    }
                }
                else if (note is GraphicNote graphicNote)
                {
                    switch (format.ToLowerInvariant())
                    {
                        case "json":
                            fileName = $"{note.Title}.json";
                            fileData = System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(graphicNote));
                            break;

                        default:
                            throw new Exception($"Unsupported export format: {format}");
                    }
                }
                else
                {
                    throw new Exception("Unknown note type");
                }

                // Use share functionality to let user save the file
                string tempFilePath = Path.Combine(FileSystem.CacheDirectory, fileName);
                await File.WriteAllBytesAsync(tempFilePath, fileData);

                await Share.RequestAsync(new ShareFileRequest
                {
                    Title = $"Export {note.Title}",
                    File = new ShareFile(tempFilePath)
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error exporting note: {ex.Message}");
                throw;
            }
        }

        public async Task<NoteBase> ImportNoteAsync(string filePath)
        {
            try
            {
                string fileExtension = Path.GetExtension(filePath).ToLowerInvariant();
                byte[] fileData = await File.ReadAllBytesAsync(filePath);

                switch (fileExtension)
                {
                    case ".txt":
                        var content = System.Text.Encoding.UTF8.GetString(fileData);
                        return new TextNote
                        {
                            Title = Path.GetFileNameWithoutExtension(filePath),
                            Content = content
                        };

                    case ".json":
                        var json = System.Text.Encoding.UTF8.GetString(fileData);

                        // Try to deserialize as TextNote first
                        try
                        {
                            var textNote = JsonSerializer.Deserialize<TextNote>(json);
                            if (textNote != null)
                                return textNote;
                        }
                        catch { }

                        // Try to deserialize as GraphicNote
                        try
                        {
                            var graphicNote = JsonSerializer.Deserialize<GraphicNote>(json);
                            if (graphicNote != null)
                                return graphicNote;
                        }
                        catch { }

                        throw new Exception("Could not deserialize the imported file");

                    default:
                        throw new Exception($"Unsupported file format: {fileExtension}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error importing note: {ex.Message}");
                throw;
            }
        }
    }
}