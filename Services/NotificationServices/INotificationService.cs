using StudyMateTest.Models;

namespace StudyMateTest.Services.NotificationServices
{
    public interface INotificationService
    {
        Task<bool> RequestPermission(); // Асинхронный метод для запроса разрешения на отправку уведомлений у пользователя

        Task<string> ScheduleNotification(string title, string message, DateTime scheduledTime, Dictionary<string, string> metadata = null); // Метод для планирования уведомлений, возвращает string с Id созданного уведомления

        Task CancelNotification(string notificationId); // Метод для отмены конкретного уведомления по его Id

        Task CancelAllNotifications(); // Метод для отмены всех запланированных уведомлений

        Task<bool> IsPermissionGranted(); // Метод для проверки, есть ли у приложения разрешение на уведомления
    }
}