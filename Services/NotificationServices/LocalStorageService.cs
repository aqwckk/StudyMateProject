using StudyMateTest.Models;
using System.Text.Json;

namespace StudyMateTest.Services
{
    public class LocalStorageService : ILocalStorageService
    {
        private readonly string _fileName = "reminders.json";
        private string FilePath => Path.Combine(FileSystem.AppDataDirectory, _fileName);

        public async Task<List<Reminder>> LoadRemindersAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Loading reminders from: {FilePath}");

                if (!File.Exists(FilePath))
                {
                    System.Diagnostics.Debug.WriteLine("Reminders file doesn't exist, returning empty list");
                    return new List<Reminder>();
                }

                var json = await File.ReadAllTextAsync(FilePath);

                if (string.IsNullOrWhiteSpace(json))
                {
                    System.Diagnostics.Debug.WriteLine("Reminders file is empty, returning empty list");
                    return new List<Reminder>();
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = true
                };

                var reminders = JsonSerializer.Deserialize<List<Reminder>>(json, options) ?? new List<Reminder>();

                System.Diagnostics.Debug.WriteLine($"Successfully loaded {reminders.Count} reminders");
                return reminders;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading reminders: {ex.Message}");
                return new List<Reminder>();
            }
        }

        public async Task SaveRemindersAsync(List<Reminder> reminders)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Saving {reminders.Count} reminders to: {FilePath}");

                // Создаем директорию если не существует
                var directory = Path.GetDirectoryName(FilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = true
                };

                var json = JsonSerializer.Serialize(reminders, options);
                await File.WriteAllTextAsync(FilePath, json);

                System.Diagnostics.Debug.WriteLine("Reminders saved successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving reminders: {ex.Message}");
                throw; // Пробрасываем ошибку чтобы UI мог её обработать
            }
        }

        public async Task DeleteAllRemindersAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Deleting reminders file: {FilePath}");

                if (File.Exists(FilePath))
                {
                    File.Delete(FilePath);
                    System.Diagnostics.Debug.WriteLine("Reminders file deleted successfully");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Reminders file doesn't exist, nothing to delete");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting reminders file: {ex.Message}");
                throw;
            }
        }
    }
}