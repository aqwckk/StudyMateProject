#if WINDOWS
using StudyMateTest.Services;
using StudyMateTest.Services.NotificationServices;
using System.Timers;

namespace StudyMateTest.Services
{
    public class WindowsNotificationService : INotificationService
    {
        private readonly Dictionary<string, System.Timers.Timer> _scheduledTimers = new();

        public async Task<bool> IsPermissionGranted()
        {
            // Windows десктоп приложения обычно имеют разрешения на показ окон
            return true;
        }

        public async Task<bool> RequestPermission()
        {
            // Для Windows MAUI разрешения не требуются
            return true;
        }

        public async Task<string> ScheduleNotification(string title, string message, DateTime scheduledTime, Dictionary<string, string> metadata = null)
        {
            var notificationId = Guid.NewGuid().ToString();
            
            try
            {
                var timeUntilNotification = scheduledTime - DateTime.Now;
                
                if (timeUntilNotification.TotalMilliseconds <= 0)
                {
                    // Показываем уведомление сразу
                    await ShowNotificationAsync(title, message);
                }
                else
                {
                    // Создаем таймер для отложенного уведомления
                    var timer = new System.Timers.Timer(timeUntilNotification.TotalMilliseconds);
                    timer.Elapsed += async (sender, e) =>
                    {
                        await ShowNotificationAsync(title, message);
                        timer.Stop();
                        timer.Dispose();
                        _scheduledTimers.Remove(notificationId);
                    };
                    timer.AutoReset = false;
                    timer.Start();
                    
                    _scheduledTimers[notificationId] = timer;
                    System.Diagnostics.Debug.WriteLine($"Scheduled Windows notification '{title}' for {scheduledTime}");
                }
                
                return notificationId;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error scheduling Windows notification: {ex.Message}");
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
                    System.Diagnostics.Debug.WriteLine($"Cancelled Windows notification: {notificationId}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error canceling Windows notification: {ex.Message}");
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
                System.Diagnostics.Debug.WriteLine("Cancelled all Windows notifications");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error canceling all Windows notifications: {ex.Message}");
            }
        }

        private async Task ShowNotificationAsync(string title, string message)
        {
            try
            {
                // Показываем уведомление в UI потоке
                await Microsoft.Maui.Controls.Application.Current.Dispatcher.DispatchAsync(async () =>
                {
                    var page = Microsoft.Maui.Controls.Application.Current.MainPage;
                    
                    if (page != null)
                    {
                        await page.DisplayAlert(
                            $"📅 {title}",
                            $"{message}\n\n⏰ Время: {DateTime.Now:HH:mm}",
                            "OK"
                        );
                    }
                });
                
                System.Diagnostics.Debug.WriteLine($"Showed Windows notification: {title} - {message}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing Windows notification: {ex.Message}");
            }
        }
    }
}
#endif