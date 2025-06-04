using Android.App;
using Android.Content;
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
            // Создаем канал уведомлений при запуске
            CreateNotificationChannel();
        }

        // Упрощенный метод создания канала уведомлений
        private void CreateNotificationChannel()
        {
            // Проверяем, что версия Android поддерживает каналы уведомлений (Android 8.0+)
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channelId = "default_channel"; // ID канала (должен совпадать с NotificationService)
                var channelName = "Default Notifications"; // Название канала, которое видит пользователь
                var description = "Default notification channel"; // Описание канала
                var importance = NotificationImportance.High; // Высокая важность уведомлений

                // Создаем канал уведомлений
                var channel = new NotificationChannel(channelId, channelName, importance);

                // Устанавливаем описание канала
                channel.Description = description;

                // Включаем вибрацию при получении уведомления
                channel.EnableVibration(true);
                // Устанавливаем паттерн вибрации: пауза 0мс, вибрация 250мс, пауза 250мс, вибрация 250мс
                channel.SetVibrationPattern(new long[] { 0, 250, 250, 250 });

                // Включаем светодиод уведомлений (если поддерживается устройством)
                channel.EnableLights(true);
                // Устанавливаем цвет светодиода (правильный метод для .NET MAUI)
                channel.LightColor = Android.Graphics.Color.Blue;

                // Получаем системный сервис для управления уведомлениями
                var notificationManager = (NotificationManager)GetSystemService(NotificationService);

                // Регистрируем канал в системе Android
                notificationManager?.CreateNotificationChannel(channel);
            }
        }

        // Обработка результатов запроса разрешений от пользователя
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            // В Shiny 3.3.4 обработка разрешений происходит автоматически через сервис
        }
    }
}