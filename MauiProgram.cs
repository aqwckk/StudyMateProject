using Microsoft.Extensions.Logging;
using StudyMateTest.Services;
using StudyMateTest.Services.NotificationServices;
using StudyMateTest.Views;

namespace StudyMateTest
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Регистрируем наши сервисы уведомлений (без Shiny)
#if WINDOWS
            builder.Services.AddSingleton<INotificationService, WindowsNotificationService>();
#elif ANDROID
            builder.Services.AddSingleton<INotificationService, AndroidNotificationService>();
#elif IOS
            builder.Services.AddSingleton<INotificationService, IOSNotificationService>();
#else
            builder.Services.AddSingleton<INotificationService, DefaultNotificationService>();
#endif

            // Регистрируем страницы
            builder.Services.AddTransient<ReminderPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}