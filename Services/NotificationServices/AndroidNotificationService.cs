#if ANDROID
using AndroidX.Core.App;
using Android.App;
using Android.Content;
using AndroidX.Core.Content;
using Android.OS;
using StudyMateTest.Services;
using System.Timers;
using StudyMateTest.Services.NotificationServices;

namespace StudyMateTest.Services
{
    public class AndroidNotificationService : INotificationService
    {
        private readonly Dictionary<string, System.Timers.Timer> _scheduledTimers = new();
        private readonly Dictionary<string, int> _notificationIds = new();
        private int _nextNotificationId = 1000;

        public async Task<bool> IsPermissionGranted()
        {
            var context = Platform.CurrentActivity ?? Android.App.Application.Context;

            if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu) // Android 13+
            {
                return ContextCompat.CheckSelfPermission(context, Android.Manifest.Permission.PostNotifications)
                    == Android.Content.PM.Permission.Granted;
            }

            return true; // Для Android < 13 разрешения не требуются
        }

        public async Task<bool> RequestPermission()
        {
            var context = Platform.CurrentActivity ?? Android.App.Application.Context;

            if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu) // Android 13+
            {
                if (Platform.CurrentActivity is AndroidX.Activity.ComponentActivity activity)
                {
                    if (ContextCompat.CheckSelfPermission(context, Android.Manifest.Permission.PostNotifications)
                        != Android.Content.PM.Permission.Granted)
                    {
                        ActivityCompat.RequestPermissions(activity,
                            new[] { Android.Manifest.Permission.PostNotifications }, 1001);

                        // Ждем результат (упрощенно возвращаем true)
                        return true;
                    }
                }
                return true;
            }

            return true; // Для Android < 13
        }

        public async Task<string> ScheduleNotification(string title, string message, DateTime scheduledTime, Dictionary<string, string> metadata = null)
        {
            var notificationId = Guid.NewGuid().ToString();
            var androidNotificationId = _nextNotificationId++;
            _notificationIds[notificationId] = androidNotificationId;

            try
            {
                var timeUntilNotification = scheduledTime - DateTime.Now;

                if (timeUntilNotification.TotalMilliseconds <= 0)
                {
                    // Показываем уведомление сразу
                    ShowNotification(androidNotificationId, title, message, metadata);
                }
                else
                {
                    // Создаем таймер для отложенного уведомления
                    var timer = new System.Timers.Timer(timeUntilNotification.TotalMilliseconds);
                    timer.Elapsed += (sender, e) =>
                    {
                        ShowNotification(androidNotificationId, title, message, metadata);
                        timer.Stop();
                        timer.Dispose();
                        _scheduledTimers.Remove(notificationId);
                    };
                    timer.AutoReset = false;
                    timer.Start();

                    _scheduledTimers[notificationId] = timer;
                    System.Diagnostics.Debug.WriteLine($"Scheduled Android notification '{title}' for {scheduledTime}");
                }

                return notificationId;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error scheduling Android notification: {ex.Message}");
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

                // Отменяем показанное уведомление
                if (_notificationIds.ContainsKey(notificationId))
                {
                    var context = Platform.CurrentActivity ?? Android.App.Application.Context;
                    var notificationManager = NotificationManagerCompat.From(context);
                    notificationManager.Cancel(_notificationIds[notificationId]);
                    _notificationIds.Remove(notificationId);
                }

                System.Diagnostics.Debug.WriteLine($"Cancelled Android notification: {notificationId}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error canceling Android notification: {ex.Message}");
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

                // Отменяем все показанные уведомления
                var context = Platform.CurrentActivity ?? Android.App.Application.Context;
                var notificationManager = NotificationManagerCompat.From(context);
                notificationManager.CancelAll();
                _notificationIds.Clear();

                System.Diagnostics.Debug.WriteLine("Cancelled all Android notifications");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error canceling all Android notifications: {ex.Message}");
            }
        }

        private void ShowNotification(int notificationId, string title, string message, Dictionary<string, string> metadata)
        {
            try
            {
                var context = Platform.CurrentActivity ?? Android.App.Application.Context;

                // Используем новый ID канала
                var builder = new NotificationCompat.Builder(context, "StudyMateReminders_v2")
                    .SetContentTitle(title)
                    .SetContentText(message)
                    .SetSmallIcon(Android.Resource.Drawable.IcDialogAlert) // Другая иконка
                    .SetAutoCancel(true)
                    .SetPriority(NotificationCompat.PriorityMax) // МАКСИМАЛЬНЫЙ приоритет
                    .SetDefaults(NotificationCompat.DefaultAll)
                    .SetStyle(new NotificationCompat.BigTextStyle().BigText(message))
                    .SetCategory(NotificationCompat.CategoryCall) // Категория ЗВОНКА - самая важная!
                    .SetVisibility(NotificationCompat.VisibilityPublic)
                    .SetOngoing(false)
                    .SetWhen(Java.Lang.JavaSystem.CurrentTimeMillis())
                    .SetShowWhen(true)
                    .SetSound(Android.Media.RingtoneManager.GetDefaultUri(Android.Media.RingtoneType.Notification))
                    .SetVibrate(new long[] { 0, 250, 250, 250 }); // Принудительная вибрация

                var notification = builder.Build();
                
                // Принудительно устанавливаем флаги
                notification.Flags |= NotificationFlags.NoClear;
                notification.Flags |= NotificationFlags.HighPriority;

                var notificationManager = NotificationManagerCompat.From(context);
        
                if (NotificationManagerCompat.From(context).AreNotificationsEnabled())
                {
                    notificationManager.Notify(notificationId, notification);
                    System.Diagnostics.Debug.WriteLine($"FORCED MAX PRIORITY notification: {title}");
                    
                    // Дополнительная отладка
                    System.Diagnostics.Debug.WriteLine($"Notification ID: {notificationId}");
                    System.Diagnostics.Debug.WriteLine($"Channel ID: StudyMateReminders_v2");
                    System.Diagnostics.Debug.WriteLine($"Priority: MAX");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("❌ Notifications are DISABLED for this app!");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error showing notification: {ex.Message}");
            }
        }
    }
}
#endif