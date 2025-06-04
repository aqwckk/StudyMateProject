namespace StudyMateTest.Models
{
    public class Reminder
    {
        public string Id { get; set; } = Guid.NewGuid().ToString(); // Создаем уникальный Id для каждого уведомления. При создании объекта автоматически генериуется новый уникальный Id

        public string Title { get; set; } // Заголовок напоминания

        public string Message { get; set; } // Подробное описание напоминания

        public DateTime ScheduledTime { get; set; } // Дата и время, когда должно появится уведомление

        public bool IsActive { get; set; } // Свойство, которое показывает активно уведомление или нет

        public Dictionary<string, string> Metadata { get; set; } = new(); // Словарь для хранения дополнительных данных
    }
}