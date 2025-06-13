using Microsoft.Extensions.Logging;
using StudyMateTest.Services;
using StudyMateTest.Services.NotificationServices;
using StudyMateTest.Views;
using SkiaSharp.Views.Maui.Controls.Hosting;
using StudyMateTest.Services.DrawingServices;
using StudyMateTest.Services.TextEditorServices;

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

            // Регистрируем сервисы локального хранилища
            builder.Services.AddSingleton<ILocalStorageService, LocalStorageService>();

            // Регистрируем сервисы уведомлений в зависимости от платформы
#if WINDOWS
            builder.Services.AddSingleton<INotificationService, WindowsNotificationService>();
            System.Diagnostics.Debug.WriteLine("Registered WindowsNotificationService");
#elif ANDROID
            builder.Services.AddSingleton<INotificationService, AndroidNotificationService>();
            System.Diagnostics.Debug.WriteLine("Registered AndroidNotificationService");
#elif IOS
            builder.Services.AddSingleton<INotificationService, IOSNotificationService>();
            System.Diagnostics.Debug.WriteLine("Registered IOSNotificationService");
#else
            builder.Services.AddSingleton<INotificationService, DefaultNotificationService>();
            System.Diagnostics.Debug.WriteLine("Registered DefaultNotificationService");
#endif

            // Регистрируем страницы как Transient (создается новый экземпляр каждый раз)
            builder.Services.AddTransient<ReminderPage>();
            builder.Services.AddTransient<AddReminderPage>();
            builder.Services.AddTransient<EditReminderPage>();

            // Регистрируем MainPage если необходимо
            builder.Services.AddTransient<MainPage>();

            builder.Services.AddSingleton<IDrawingService, DrawingService>();
            builder.Services.AddSingleton<ITextEditorService, TextEditorService>();
#if DEBUG
            builder.Logging.AddDebug();
            // Включаем детальное логирование для отладки
            builder.Logging.SetMinimumLevel(LogLevel.Debug);
#endif

            var app = builder.Build();

            // Логируем успешную инициализацию
            System.Diagnostics.Debug.WriteLine("MauiProgram: Application configured successfully");

            return app;
        }
    }
}