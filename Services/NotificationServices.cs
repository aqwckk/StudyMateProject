using Shiny;
using Shiny.Notifications;
using StudyMateTest.Models;

namespace StudyMateTest.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationManager _notificationManager; // Приватное поле для хранения менеджера уведомлений от Shiny
        private readonly HashSet<int> _trackedNotifications = new();

        // Конструктор класса
        public NotificationService(INotificationManager notificationManager)
        {
            _notificationManager = notificationManager; // Сохраняет переданный менеджер в приватное поле
        }

        // Реализация метода запроса разрешения на уведомление
        public async Task<bool> RequestPermission()
        {
            // RequestAccess() показывает системный диалог запроса разрешения
            var result = await _notificationManager.RequestAccess(); // ожидаем выполнение асинхронной операции

            return result == AccessState.Available; // Проверяем, получено ли разрешение
        }

        // Реализация метода проверки наличия разрешения
        public async Task<bool> IsPermissionGranted()
        {
            // В Shiny 3.3.4 нет отдельного метода проверки, используем RequestAccess
            var result = await _notificationManager.RequestAccess(); // Запрашиваем статус доступа

            return result == AccessState.Available; // Возвращаем true, если разрешение есть
        }

        // Реализация метода планирования уведомления
        public async Task<string> ScheduleNotification(string title, string message, DateTime scheduledTime, Dictionary<string, string> metadata = null)
        {
            // Генерируем уникальный Id для уведомления
            var notificationId = Guid.NewGuid().ToString();
            var numericId = Math.Abs(notificationId.GetHashCode());

            // Создаем объект запроса уведомления с настройками
            var notification = new Notification
            {
                // Id должен быть числом, поэтому берем хеш-код от строки и делаем его положительным
                Id = Math.Abs(notificationId.GetHashCode()),

                // Заголовок уведомления
                Title = title,

                // Текст уведомления
                Message = message,

                // Время, когда показать уведомление
                ScheduleDate = scheduledTime,

                // Указываем канал уведомлений для Android
                Channel = "default_channel",

                // Устанавливаем метаданные, если они переданы, иначе пустой словарь
                Payload = metadata ?? new Dictionary<string, string>()
            };

            // Убеждаемся, что канал уведомлений создан (особенно важно для Android)
            await EnsureNotificationChannel();
            _trackedNotifications.Add(numericId);

            await _notificationManager.Send(notification); // Отправляем уведомление (планируем его показ на указанное время)
            return notificationId; // Возвращаем Id уведомления для последующего управления
        }

        // Метод для отмены уведомления по его идентификатору
        public async Task<bool> CancelNotification(string notificationId)
        {
            try // Пытаемся отменить уведомление
            {
                var numericId = Math.Abs(notificationId.GetHashCode());
                await _notificationManager.Cancel(Math.Abs(notificationId.GetHashCode())); // Отменяем уведомление по его Id
                _trackedNotifications.Remove(numericId);
                return true; // Возвращаем true, если отмена произошла успешно
            }
            catch
            {
                return false; // Возвращаем false, если произошла ошибка при отмене
            }
        }

        // Метод для отмены всех запланированных уведомлений
        public async Task CancelAllNotifications()
        {
            foreach (var id in _trackedNotifications.ToList())
            {
                try
                {
                    await _notificationManager.Cancel(id);
                    _trackedNotifications.Remove(id);
                }
                catch { }
            }
        }

        // Приватный метод для создания канала уведомлений (особенно важно для Android)
        private Task EnsureNotificationChannel()
        {
            #if ANDROID
            var channel = new Channel
            {
                Identifier = "default_channel",
                Description = "Default notifications channel",
                Importance = ChannelImportance.High
            };
    
            _notificationManager.AddChannel(channel);
            #endif

            return Task.CompletedTask;
        }
    }
}