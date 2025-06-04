using Microsoft.Extensions.Logging;
using StudyMateTest.Services;
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

            // Регистрируем простой сервис уведомлений БЕЗ Shiny
            builder.Services.AddSingleton<INotificationService, SimpleNotificationService>();

            // Регистрируем страницы
            builder.Services.AddTransient<ReminderPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}