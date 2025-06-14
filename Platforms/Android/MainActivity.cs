using Android.App;
using Android.Content.PM;
using Android.OS;

namespace StudyMateTest
{
    [Activity(
        Theme = "@style/Maui.SplashTheme",
        MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Создаем канал уведомлений для Android
            CreateNotificationChannel();
        }

        private void CreateNotificationChannel()
        {
            try
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    // Удаляем старый канал если существует
                    var notificationManager = GetSystemService(Android.Content.Context.NotificationService) as NotificationManager;
                    notificationManager?.DeleteNotificationChannel("StudyMateReminders");
                    notificationManager?.DeleteNotificationChannel("studymate_reminders");

                    var channelId = "StudyMateReminders_v2"; // Новый ID
                    var channelName = "Напоминания StudyMate";
                    var description = "Уведомления о напоминаниях";

                    // Создаем канал с МАКСИМАЛЬНЫМИ настройками
                    var channel = new NotificationChannel(channelId, channelName, NotificationImportance.Max)
                    {
                        Description = description,
                        LockscreenVisibility = NotificationVisibility.Public
                    };

                    // Форсируем все настройки
                    channel.EnableVibration(true);
                    channel.EnableLights(true);
                    channel.SetSound(Android.Media.RingtoneManager.GetDefaultUri(Android.Media.RingtoneType.Notification), null);
                    channel.SetShowBadge(true);

                    notificationManager?.CreateNotificationChannel(channel);

                    System.Diagnostics.Debug.WriteLine("Created FORCED notification channel with MAX settings");
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating notification channel: {ex.Message}");
            }
        }
    }
}