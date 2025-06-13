using System.Timers;

namespace StudyMateTest.Services.NotificationServices
{
    public class DefaultNotificationService : INotificationService
    {
        private readonly Dictionary<string, System.Timers.Timer> _scheduledTimers = new();

        public async Task<bool> IsPermissionGranted()
        {
            return true; // Заглушка для неизвестных платформ
        }

        public async Task<bool> RequestPermission()
        {
            return true; // Заглушка для неизвестных платформ
        }

        public async Task<string> ScheduleNotification(string title, string message, DateTime scheduledTime, Dictionary<string, string> metadata = null)
        {
            var notificationId = Guid.NewGuid().ToString();

            try
            {
                var timeUntilNotification = scheduledTime - DateTime.Now;

                if (timeUntilNotification.TotalMilliseconds <= 0)
                {
                    // Показываем сообщение в консоли
                    System.Diagnostics.Debug.WriteLine($"NOTIFICATION NOW: {title} - {message}");
                }
                else
                {
                    // Создаем таймер для отложенного уведомления
                    var timer = new System.Timers.Timer(timeUntilNotification.TotalMilliseconds);
                    timer.Elapsed += (sender, e) =>
                    {
                        System.Diagnostics.Debug.WriteLine($"NOTIFICATION: {title} - {message}");
                        timer.Stop();
                        timer.Dispose();
                        _scheduledTimers.Remove(notificationId);
                    };
                    timer.AutoReset = false;
                    timer.Start();

                    _scheduledTimers[notificationId] = timer;
                    System.Diagnostics.Debug.WriteLine($"Scheduled notification '{title}' for {scheduledTime}");
                }

                return notificationId;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error scheduling notification: {ex.Message}");
                return notificationId;
            }
        }

        public async Task CancelNotification(string notificationId)
        {
            try
            {
                if (_scheduledTimers.ContainsKey(notificationId))
                {
                    _scheduledTimers[notificationId].Stop();
                    _scheduledTimers[notificationId].Dispose();
                    _scheduledTimers.Remove(notificationId);
                    System.Diagnostics.Debug.WriteLine($"Cancelled notification: {notificationId}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error canceling notification: {ex.Message}");
            }
        }

        public async Task CancelAllNotifications()
        {
            try
            {
                foreach (var timer in _scheduledTimers.Values)
                {
                    timer.Stop();
                    timer.Dispose();
                }
                _scheduledTimers.Clear();
                System.Diagnostics.Debug.WriteLine("Cancelled all notifications");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error canceling all notifications: {ex.Message}");
            }
        }
    }
}