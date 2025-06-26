using StudyMateTest.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace StudyMateTest.Services
{
    public class NoteService : INoteService
    {
        private readonly string _notesFileName = "notes.json";
        private readonly string _backupFileName = "notes_backup.json";
        private string NotesFilePath => Path.Combine(FileSystem.AppDataDirectory, _notesFileName);
        private string BackupFilePath => Path.Combine(FileSystem.AppDataDirectory, _backupFileName);

        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        public async Task<List<Note>> LoadNotesAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== LoadNotesAsync START ===");
                System.Diagnostics.Debug.WriteLine($"Loading notes from: {NotesFilePath}");

                if (!File.Exists(NotesFilePath))
                {
                    System.Diagnostics.Debug.WriteLine("Notes file doesn't exist, returning empty list");
                    return new List<Note>();
                }

                var json = await File.ReadAllTextAsync(NotesFilePath);
                System.Diagnostics.Debug.WriteLine($"Raw JSON length: {json?.Length ?? 0}");
                System.Diagnostics.Debug.WriteLine($"First 500 chars of JSON: {(json?.Length > 500 ? json.Substring(0, 500) : json)}");

                if (string.IsNullOrWhiteSpace(json))
                {
                    System.Diagnostics.Debug.WriteLine("Notes file is empty, returning empty list");
                    return new List<Note>();
                }

                var notes = JsonSerializer.Deserialize<List<Note>>(json, _jsonOptions) ?? new List<Note>();

                System.Diagnostics.Debug.WriteLine($"Deserialized {notes.Count} notes");

                foreach (var note in notes)
                {
                    note.MarkAsSaved();

                    System.Diagnostics.Debug.WriteLine($"Note ID: {note.Id}");
                    System.Diagnostics.Debug.WriteLine($"  Title: '{note.Title}'");
                    System.Diagnostics.Debug.WriteLine($"  Description: '{note.Description}'");
                    System.Diagnostics.Debug.WriteLine($"  TextContent: '{(string.IsNullOrEmpty(note.TextContent) ? "EMPTY" : $"Length: {note.TextContent.Length}")}'");
                    System.Diagnostics.Debug.WriteLine($"  GraphicsData: {(note.GraphicsData == null ? "NULL" : $"Length: {note.GraphicsData.Length} bytes")}");
                    System.Diagnostics.Debug.WriteLine($"  CreatedAt: {note.CreatedAt}");
                    System.Diagnostics.Debug.WriteLine($"  LastModified: {note.LastModified}");
                    System.Diagnostics.Debug.WriteLine($"  IsModified: {note.IsModified}");
                    System.Diagnostics.Debug.WriteLine($"---");
                }

                System.Diagnostics.Debug.WriteLine($"=== LoadNotesAsync END - Successfully loaded {notes.Count} notes ===");
                return notes;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR loading notes: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return new List<Note>();
            }
        }

        public async Task SaveNotesAsync(List<Note> notes)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== SaveNotesAsync START ===");
                System.Diagnostics.Debug.WriteLine($"Saving {notes.Count} notes to: {NotesFilePath}");

                var directory = Path.GetDirectoryName(NotesFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                foreach (var note in notes)
                {
                    System.Diagnostics.Debug.WriteLine($"BEFORE SAVE - Note ID: {note.Id}");
                    System.Diagnostics.Debug.WriteLine($"  Title: '{note.Title}'");
                    System.Diagnostics.Debug.WriteLine($"  Description: '{note.Description}'");
                    System.Diagnostics.Debug.WriteLine($"  TextContent: '{(string.IsNullOrEmpty(note.TextContent) ? "EMPTY" : $"Length: {note.TextContent.Length}")}'");
                    System.Diagnostics.Debug.WriteLine($"  GraphicsData: {(note.GraphicsData == null ? "NULL" : $"Length: {note.GraphicsData.Length} bytes")}");
                }

                var json = JsonSerializer.Serialize(notes, _jsonOptions);
                System.Diagnostics.Debug.WriteLine($"Serialized JSON length: {json.Length}");
                System.Diagnostics.Debug.WriteLine($"First 1000 chars of serialized JSON: {(json.Length > 1000 ? json.Substring(0, 1000) : json)}");

                await File.WriteAllTextAsync(NotesFilePath, json);

                if (File.Exists(NotesFilePath))
                {
                    var fileInfo = new FileInfo(NotesFilePath);
                    System.Diagnostics.Debug.WriteLine($"File saved successfully. Size: {fileInfo.Length} bytes");
                }

                foreach (var note in notes)
                {
                    note.MarkAsSaved();
                }

                System.Diagnostics.Debug.WriteLine($"=== SaveNotesAsync END - Notes saved successfully ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR saving notes: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task SaveNoteAsync(Note note)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== SaveNoteAsync START ===");
                System.Diagnostics.Debug.WriteLine($"Saving individual note: {note.Title}");

                System.Diagnostics.Debug.WriteLine($"Note details before save:");
                System.Diagnostics.Debug.WriteLine($"  ID: {note.Id}");
                System.Diagnostics.Debug.WriteLine($"  Title: '{note.Title}'");
                System.Diagnostics.Debug.WriteLine($"  Description: '{note.Description}'");
                System.Diagnostics.Debug.WriteLine($"  TextContent: '{(string.IsNullOrEmpty(note.TextContent) ? "EMPTY" : $"Length: {note.TextContent.Length}")}'");
                System.Diagnostics.Debug.WriteLine($"  GraphicsData: {(note.GraphicsData == null ? "NULL" : $"Length: {note.GraphicsData.Length} bytes")}");
                System.Diagnostics.Debug.WriteLine($"  CreatedAt: {note.CreatedAt}");
                System.Diagnostics.Debug.WriteLine($"  LastModified: {note.LastModified}");
                System.Diagnostics.Debug.WriteLine($"  IsModified: {note.IsModified}");

                var notes = await LoadNotesAsync();
                var existingIndex = notes.FindIndex(n => n.Id == note.Id);

                if (existingIndex >= 0)
                {
                    System.Diagnostics.Debug.WriteLine($"Updating existing note at index {existingIndex}");
                    notes[existingIndex] = note;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Adding new note to collection");
                    notes.Add(note);
                }

                await SaveNotesAsync(notes);
                note.MarkAsSaved();

                System.Diagnostics.Debug.WriteLine($"=== SaveNoteAsync END - Note saved: {note.Title} ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR saving note: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task DeleteNoteAsync(string noteId)
        {
            try
            {
                var notes = await LoadNotesAsync();
                var noteToRemove = notes.FirstOrDefault(n => n.Id == noteId);

                if (noteToRemove != null)
                {
                    notes.Remove(noteToRemove);
                    await SaveNotesAsync(notes);
                    System.Diagnostics.Debug.WriteLine($"Note deleted: {noteId}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting note: {ex.Message}");
                throw;
            }
        }

        public async Task<Note> GetNoteByIdAsync(string noteId)
        {
            try
            {
                var notes = await LoadNotesAsync();
                return notes.FirstOrDefault(n => n.Id == noteId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting note by ID: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> ExportNoteToPngAsync(Note note, string fileName = null)
        {
            try
            {
                if (note.GraphicsData == null || note.GraphicsData.Length == 0)
                {
                    await Application.Current.MainPage.DisplayAlert("Ошибка", "В заметке нет графического содержимого", "OK");
                    return false;
                }

                string defaultFileName = SanitizeFileName(note.Title);
                string userFileName = await Application.Current.MainPage.DisplayPromptAsync(
                    "Экспорт рисунка",
                    "Введите имя файла:",
                    initialValue: defaultFileName,
                    placeholder: "Имя файла без расширения");

                if (string.IsNullOrWhiteSpace(userFileName))
                    return false;

                fileName = $"{SanitizeFileName(userFileName)}_{DateTime.Now:yyyyMMdd_HHmmss}.png";

                string filePath;

#if WINDOWS
        try
        {
            var picker = new Windows.Storage.Pickers.FileSavePicker();
            picker.SuggestedFileName = fileName;
            picker.FileTypeChoices.Add("PNG Image", new[] { ".png" });
            picker.DefaultFileExtension = ".png";
            
            var hwnd = ((MauiWinUIWindow)Application.Current.Windows[0].Handler.PlatformView).WindowHandle;
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
            
            var file = await picker.PickSaveFileAsync();
            if (file == null) return false;
            
            filePath = file.Path;
        }
        catch
        {
            filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), fileName);
        }
#else
                filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);
#endif

                await File.WriteAllBytesAsync(filePath, note.GraphicsData);

                await Application.Current.MainPage.DisplayAlert("Успешно", $"Рисунок экспортирован:\n{filePath}", "OK");
                System.Diagnostics.Debug.WriteLine($"Note graphics exported to: {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error exporting note to PNG: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Ошибка", $"Не удалось экспортировать рисунок: {ex.Message}", "OK");
                return false;
            }
        }

        public async Task<bool> ExportNoteToTxtAsync(Note note, string fileName = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(note.TextContent))
                {
                    await Application.Current.MainPage.DisplayAlert("Ошибка", "В заметке нет текстового содержимого", "OK");
                    return false;
                }

                string defaultFileName = SanitizeFileName(note.Title);
                string userFileName = await Application.Current.MainPage.DisplayPromptAsync(
                    "Экспорт текста",
                    "Введите имя файла:",
                    initialValue: defaultFileName,
                    placeholder: "Имя файла без расширения");

                if (string.IsNullOrWhiteSpace(userFileName))
                    return false;

                fileName = $"{SanitizeFileName(userFileName)}_{DateTime.Now:yyyyMMdd_HHmmss}.txt";

                string filePath;

#if WINDOWS
        try
        {
            var picker = new Windows.Storage.Pickers.FileSavePicker();
            picker.SuggestedFileName = fileName;
            picker.FileTypeChoices.Add("Text Document", new[] { ".txt" });
            picker.DefaultFileExtension = ".txt";
            
            var hwnd = ((MauiWinUIWindow)Application.Current.Windows[0].Handler.PlatformView).WindowHandle;
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
            
            var file = await picker.PickSaveFileAsync();
            if (file == null) return false;
            
            filePath = file.Path;
        }
        catch
        {
            filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);
        }
#else
                filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);
#endif

                var plainText = System.Text.RegularExpressions.Regex.Replace(
                    note.TextContent, "<[^>]+>", "")
                    .Replace("&nbsp;", " ")
                    .Replace("&lt;", "<")
                    .Replace("&gt;", ">")
                    .Replace("&amp;", "&");

                var content = new StringBuilder();
                content.AppendLine($"Заметка: {note.Title}");
                content.AppendLine($"Описание: {note.Description}");
                content.AppendLine($"Создано: {note.CreatedAt:dd.MM.yyyy HH:mm}");
                content.AppendLine($"Изменено: {note.LastModified:dd.MM.yyyy HH:mm}");
                content.AppendLine();
                content.AppendLine("--- СОДЕРЖИМОЕ ---");
                content.AppendLine(plainText);

                await File.WriteAllTextAsync(filePath, content.ToString(), Encoding.UTF8);

                await Application.Current.MainPage.DisplayAlert("Успешно", $"Текст экспортирован:\n{filePath}", "OK");
                System.Diagnostics.Debug.WriteLine($"Note text exported to: {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error exporting note to TXT: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Ошибка", $"Не удалось экспортировать текст: {ex.Message}", "OK");
                return false;
            }
        }

        public async Task<bool> ExportNoteToJsonAsync(Note note, string fileName = null)
        {
            try
            {
                string defaultFileName = SanitizeFileName(note.Title);
                string userFileName = await Application.Current.MainPage.DisplayPromptAsync(
                    "Экспорт заметки",
                    "Введите имя файла:",
                    initialValue: defaultFileName,
                    placeholder: "Имя файла без расширения");

                if (string.IsNullOrWhiteSpace(userFileName))
                    return false;

                fileName = $"{SanitizeFileName(userFileName)}_{DateTime.Now:yyyyMMdd_HHmmss}.json";

                string filePath;

#if WINDOWS
        try
        {
            var picker = new Windows.Storage.Pickers.FileSavePicker();
            picker.SuggestedFileName = fileName;
            picker.FileTypeChoices.Add("JSON Document", new[] { ".json" });
            picker.DefaultFileExtension = ".json";
            
            var hwnd = ((MauiWinUIWindow)Application.Current.Windows[0].Handler.PlatformView).WindowHandle;
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
            
            var file = await picker.PickSaveFileAsync();
            if (file == null) return false;
            
            filePath = file.Path;
        }
        catch
        {
            filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);
        }
#else
                filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);
#endif

                var json = JsonSerializer.Serialize(note, _jsonOptions);
                await File.WriteAllTextAsync(filePath, json, Encoding.UTF8);

                await Application.Current.MainPage.DisplayAlert("Успешно", $"Заметка экспортирована:\n{filePath}", "OK");
                System.Diagnostics.Debug.WriteLine($"Note exported to JSON: {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error exporting note to JSON: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Ошибка", $"Не удалось экспортировать заметку: {ex.Message}", "OK");
                return false;
            }
        }

        public async Task<Note> ImportNoteFromJsonAsync()
        {
            try
            {
                var result = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = "Выберите JSON файл заметки",
                    FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        { DevicePlatform.iOS, new[] { "public.json" } },
                        { DevicePlatform.Android, new[] { "application/json" } },
                        { DevicePlatform.WinUI, new[] { ".json" } },
                        { DevicePlatform.macOS, new[] { "json" } }
                    })
                });

                if (result != null)
                {
                    using var stream = await result.OpenReadAsync();
                    using var reader = new StreamReader(stream);
                    var json = await reader.ReadToEndAsync();

                    var note = JsonSerializer.Deserialize<Note>(json, _jsonOptions);

                    if (note != null)
                    {
                        note.Id = Guid.NewGuid().ToString();
                        note.Title = $"{note.Title} (импорт)";
                        note.MarkAsModified();

                        System.Diagnostics.Debug.WriteLine($"Note imported: {note.Title}");
                        return note;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error importing note from JSON: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Ошибка", $"Не удалось импортировать заметку: {ex.Message}", "OK");
                return null;
            }
        }

        public async Task DeleteAllNotesAsync()
        {
            try
            {
                if (File.Exists(NotesFilePath))
                {
                    File.Delete(NotesFilePath);
                    System.Diagnostics.Debug.WriteLine("All notes deleted");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting all notes: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> CreateBackupAsync()
        {
            try
            {
                if (File.Exists(NotesFilePath))
                {
                    File.Copy(NotesFilePath, BackupFilePath, true);
                    System.Diagnostics.Debug.WriteLine("Backup created successfully");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating backup: {ex.Message}");
                return false;
            }
        }

        public async Task<List<Note>> RestoreFromBackupAsync()
        {
            try
            {
                if (File.Exists(BackupFilePath))
                {
                    File.Copy(BackupFilePath, NotesFilePath, true);
                    System.Diagnostics.Debug.WriteLine("Backup restored successfully");
                    return await LoadNotesAsync();
                }
                return new List<Note>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error restoring backup: {ex.Message}");
                return new List<Note>();
            }
        }

        private string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return "note";

            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));

            if (sanitized.Length > 50)
                sanitized = sanitized.Substring(0, 50);

            return string.IsNullOrWhiteSpace(sanitized) ? "note" : sanitized;
        }
    }
}