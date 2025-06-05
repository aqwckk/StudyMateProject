#if IOS
using UserNotifications;
using Foundation;
using StudyMateTest.Services;
using System.Timers;
using StudyMateTest.Services.NotificationServices;

namespace StudyMateTest.Services
{
    public class IOSNotificationService : INotificationService
    {
        private readonly Dictionary<string, System.Timers.Timer> _scheduledTimers = new();

        public async Task<bool> IsPermissionGranted()
        {
            try
            {
                var center = UNUserNotificationCenter.Current;
                var settings = await center.GetNotificationSettingsAsync();
                return settings.AuthorizationStatus == UNAuthorizationStatus.Authorized;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking iOS notification permission: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RequestPermission()
        {
            try
            {
                var center = UNUserNotificationCenter.Current;
                var result = await center.RequestAuthorizationAsync(
                    UNAuthorizationOptions.Alert | 
                    UNAuthorizationOptions.Sound | 
                    UNAuthorizationOptions.Badge);
                
                return result.Item1; // granted
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error requesting iOS notification permission: {ex.Message}");
                return false;
            }
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
                    await ShowNotificationAsync(notificationId, title, message, metadata);
                }
                else
                {
                    // Создаем таймер для отложенного уведомления
                    var timer = new System.Timers.Timer(timeUntilNotification.TotalMilliseconds);
                    timer.Elapsed += async (sender, e) =>
                    {
                        await ShowNotificationAsync(notificationId, title, message, metadata);
                        timer.Stop();
                        timer.Dispose();
                        _scheduledTimers.Remove(notificationId);
                    };
                    timer.AutoReset = false;
                    timer.Start();
                    
                    _scheduledTimers[notificationId] = timer;
                    System.Diagnostics.Debug.WriteLine($"Scheduled iOS notification '{title}' for {scheduledTime}");
                }
                
                return notificationId;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error scheduling iOS notification: {ex.Message}");
                return notificationId;
            }
        }

        public async Task CancelNotification(string notificationId)
        {
            try
            {
                // Останавливаем таймер если есть
                if (_scheduledTimers.ContainsKey(notificationId))
                {
                    _scheduledTimers[notificationId].Stop();
                    _scheduledTimers[notificationId].Dispose();
                    _scheduledTimers.Remove(notificationId);
                }
                
                // Отменяем запланированное уведомление
                var center = UNUserNotificationCenter.Current;
                center.RemovePendingNotificationRequests(new[] { notificationId });
                center.RemoveDeliveredNotifications(new[] { notificationId });
                
                System.Diagnostics.Debug.WriteLine($"Cancelled iOS notification: {notificationId}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error canceling iOS notification: {ex.Message}");
            }
        }

        public async Task CancelAllNotifications()
        {
            try
            {
                // Останавливаем все таймеры
                foreach (var timer in _scheduledTimers.Values)
                {
                    timer.Stop();
                    timer.Dispose();
                }
                _scheduledTimers.Clear();
                
                // Отменяем все уведомления
                var center = UNUserNotificationCenter.Current;
                center.RemoveAllPendingNotificationRequests();
                center.RemoveAllDeliveredNotifications();
                
                System.Diagnostics.Debug.WriteLine("Cancelled all iOS notifications");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error canceling all iOS notifications: {ex.Message}");
            }
        }

        private async Task ShowNotificationAsync(string notificationId, string title, string message, Dictionary<string, string> metadata)
        {
            try
            {
                var center = UNUserNotificationCenter.Current;

                var content = new UNMutableNotificationContent();
                content.Title = title;
                content.Body = message;
                content.Sound = UNNotificationSound.Default;
                content.Badge = 1;

                if (metadata != null)
                {
                    var userInfo = new NSMutableDictionary();
                    foreach (var item in metadata)
                    {
                        userInfo[new NSString(item.Key)] = new NSString(item.Value);
                    }
                    content.UserInfo = userInfo;
                }

                // Создаем запрос для немедленного показа (trigger = null)
                var request = UNNotificationRequest.FromIdentifier(notificationId, content, null);
                
                await center.AddNotificationRequestAsync(request);
                System.Diagnostics.Debug.WriteLine($"Showed iOS notification: {title} - {message}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing iOS notification: {ex.Message}");
            }
        }
    }
}
#endif