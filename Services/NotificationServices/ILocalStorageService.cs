using StudyMateTest.Models;

namespace StudyMateTest.Services
{
    public interface ILocalStorageService
    {
        Task<List<Reminder>> LoadRemindersAsync();
        Task SaveRemindersAsync(List<Reminder> reminders);
        Task DeleteAllRemindersAsync();
    }
}