using StudyMateTest.Models;

#if ANDROID
using AndroidX.Core.App;
using Android.App;
using Android.Content;
using AndroidX.Core.Content;
using Android.OS;
#elif IOS
using UserNotifications;
using Foundation;
#endif

namespace StudyMateTest.Services
{
    public class SimpleNotificationService : INotificationService
    {
        private readonly Dictionary<string, string> _scheduledNotifications = new();

        public async Task<bool> IsPermissionGranted()
        {
#if ANDROID
            var context = Platform.CurrentActivity ?? Android.App.Application.Context;
            return ContextCompat.CheckSelfPermission(context, Android.Manifest.Permission.PostNotifications) == Android.Content.PM.Permission.Granted;
#elif IOS
            var center = UNUserNotificationCenter.Current;
            var settings = await center.GetNotificationSettingsAsync();
            return settings.AuthorizationStatus == UNAuthorizationStatus.Authorized;
#else
            return false;
#endif
        }

        public async Task<bool> RequestPermission()
        {
#if ANDROID
            var context = Platform.CurrentActivity ?? Android.App.Application.Context;

            if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
            {
                if (ContextCompat.CheckSelfPermission(context, Android.Manifest.Permission.PostNotifications) != Android.Content.PM.Permission.Granted)
                {
                    // Для Android 13+ нужно запрашивать разрешение через Activity
                    if (Platform.CurrentActivity is AndroidX.Activity.ComponentActivity activity)
                    {
                        ActivityCompat.RequestPermissions(activity, new[] { Android.Manifest.Permission.PostNotifications }, 1);
                        return true; // Предполагаем, что пользователь даст разрешение
                    }
                }
                return true;
            }
            return true; // Для более старых версий Android разрешение не требуется
#elif IOS
            var center = UNUserNotificationCenter.Current;
            var result = await center.RequestAuthorizationAsync(UNAuthorizationOptions.Alert | UNAuthorizationOptions.Sound | UNAuthorizationOptions.Badge);
            return result.Item1;
#else
            return false;
#endif
        }

        public async Task<string> ScheduleNotification(string title, string message, DateTime scheduledTime, Dictionary<string, string> metadata = null)
        {
            var notificationId = Guid.NewGuid().ToString();

#if ANDROID
            await ScheduleAndroidNotification(notificationId, title, message, scheduledTime, metadata);
#elif IOS
            await ScheduleiOSNotification(notificationId, title, message, scheduledTime, metadata);
#endif

            _scheduledNotifications[notificationId] = notificationId;
            return notificationId;
        }

        public async Task<bool> CancelNotification(string notificationId)
        {
            try
            {
#if ANDROID
                var context = Platform.CurrentActivity ?? Android.App.Application.Context;
                var notificationManager = NotificationManagerCompat.From(context);

                if (int.TryParse(notificationId.GetHashCode().ToString(), out int id))
                {
                    notificationManager.Cancel(id);
                }
#elif IOS
        var center = UNUserNotificationCenter.Current;
        center.RemovePendingNotificationRequests(new[] { notificationId });
#endif

                _scheduledNotifications.Remove(notificationId);
                return true; // Успешное выполнение
            }
            catch
            {
                return false; // Ошибка при отмене
            }
        }

        public async Task CancelAllNotifications()
        {
#if ANDROID
            var context = Platform.CurrentActivity ?? Android.App.Application.Context;
            var notificationManager = NotificationManagerCompat.From(context);
            notificationManager.CancelAll();
#elif IOS
            var center = UNUserNotificationCenter.Current;
            center.RemoveAllPendingNotificationRequests();
#endif

            _scheduledNotifications.Clear();
        }

#if ANDROID
        private async Task ScheduleAndroidNotification(string notificationId, string title, string message, DateTime scheduledTime, Dictionary<string, string> metadata)
        {
            var context = Platform.CurrentActivity ?? Android.App.Application.Context;

            var intent = new Intent(context, typeof(MainActivity));
            intent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);

            if (metadata != null)
            {
                foreach (var item in metadata)
                {
                    intent.PutExtra(item.Key, item.Value);
                }
            }

            var pendingIntent = PendingIntent.GetActivity(context, 0, intent, PendingIntentFlags.Immutable);

            var builder = new NotificationCompat.Builder(context, "general")
                .SetContentTitle(title)
                .SetContentText(message)
                .SetSmallIcon(Resource.Drawable.ic_notification) // Убедитесь что иконка существует
                .SetContentIntent(pendingIntent)
                .SetAutoCancel(true)
                .SetPriority(NotificationCompat.PriorityDefault);

            var notification = builder.Build();
            var notificationManager = NotificationManagerCompat.From(context);

            // Для простоты показываем уведомление сразу
            // В реальном приложении нужно использовать AlarmManager для отложенных уведомлений
            var delay = (int)(scheduledTime - DateTime.Now).TotalMilliseconds;
            if (delay > 0)
            {
                await Task.Delay(Math.Min(delay, int.MaxValue));
            }

            notificationManager.Notify(notificationId.GetHashCode(), notification);
        }
#endif

#if IOS
        private async Task ScheduleiOSNotification(string notificationId, string title, string message, DateTime scheduledTime, Dictionary<string, string> metadata)
        {
            var center = UNUserNotificationCenter.Current;

            var content = new UNMutableNotificationContent();
            content.Title = title;
            content.Body = message;
            content.Sound = UNNotificationSound.Default;

            if (metadata != null)
            {
                var userInfo = new NSMutableDictionary();
                foreach (var item in metadata)
                {
                    userInfo[new NSString(item.Key)] = new NSString(item.Value);
                }
                content.UserInfo = userInfo;
            }

            var triggerDate = scheduledTime.ToNSDate();
            var trigger = UNCalendarNotificationTrigger.CreateTrigger(NSCalendar.CurrentCalendar.Components(
                NSCalendarUnit.Year | NSCalendarUnit.Month | NSCalendarUnit.Day |
                NSCalendarUnit.Hour | NSCalendarUnit.Minute | NSCalendarUnit.Second,
                triggerDate), false);

            var request = UNNotificationRequest.FromIdentifier(notificationId, content, trigger);
            
            try
            {
                await center.AddNotificationRequestAsync(request);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error scheduling iOS notification: {ex.Message}");
            }
        }
#endif
    }

#if IOS
    public static class DateTimeExtensions
    {
        public static NSDate ToNSDate(this DateTime dateTime)
        {
            var reference = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return NSDate.FromTimeIntervalSinceReferenceDate((dateTime.ToUniversalTime() - reference).TotalSeconds);
        }
    }
#endif
}