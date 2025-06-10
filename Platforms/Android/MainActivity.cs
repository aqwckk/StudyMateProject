using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace StudyMateTest
{
    [Activity(
        Theme = "@style/Maui.SplashTheme",
        MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density,
        LaunchMode = LaunchMode.SingleTop)]  // Добавили LaunchMode.SingleTop
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Создаем канал уведомлений для Android
            CreateNotificationChannel();

            // Обрабатываем intent если приложение открыто через уведомление
            HandleNotificationIntent(Intent);
        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);

            // Обновляем intent активности
            Intent = intent;

            // Обрабатываем новый intent (когда приложение уже запущено)
            HandleNotificationIntent(intent);
        }

        private void HandleNotificationIntent(Intent intent)
        {
            try
            {
                if (intent?.GetBooleanExtra("FromNotification", false) == true)
                {
                    var title = intent.GetStringExtra("NotificationTitle");
                    var message = intent.GetStringExtra("NotificationMessage");

                    System.Diagnostics.Debug.WriteLine($"App opened from notification: {title}");

                    Microsoft.Maui.Controls.Application.Current?.Dispatcher.Dispatch(() =>
                    {
                        MessagingCenter.Send<MainActivity, string>(this, "NotificationClicked", title ?? "");
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error handling notification intent: {ex.Message}");
            }
        }

        private void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                // Создаем оба канала для совместимости
                CreateChannel("general", "General Notifications", "General application notifications", NotificationImportance.Default);
                CreateChannel("studymate_reminders", "StudyMate Напоминания", "Уведомления о напоминаниях StudyMate", NotificationImportance.High);
            }
        }

        private void CreateChannel(string channelId, string channelName, string channelDescription, NotificationImportance importance)
        {
            try
            {
                var channel = new NotificationChannel(channelId, channelName, importance)
                {
                    Description = channelDescription
                };

                if (importance == NotificationImportance.High)
                {
                    channel.EnableLights(true);
                    channel.EnableVibration(true);
                    channel.SetVibrationPattern(new long[] { 100, 200, 300, 400, 500, 400, 300, 200, 400 });
                }

                var notificationManager = GetSystemService(NotificationService) as NotificationManager;
                notificationManager?.CreateNotificationChannel(channel);

                System.Diagnostics.Debug.WriteLine($"Notification channel created: {channelId}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating notification channel {channelId}: {ex.Message}");
            }
        }
    }
}