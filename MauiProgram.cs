using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;
using StudyMateTest.Services;
using StudyMateTest.Services.NotificationServices;
using StudyMateTest.Services.DrawingServices;
using StudyMateTest.Services.TextEditorServices;
using StudyMateTest.Views;

namespace StudyMateTest
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("MauiProgram: Starting CreateMauiApp");

                var builder = MauiApp.CreateBuilder();
                builder
                    .UseMauiApp<App>()
                    .UseSkiaSharp()  // Для графического редактора
                    .ConfigureFonts(fonts =>
                    {
                        fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                        fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    });

                System.Diagnostics.Debug.WriteLine("MauiProgram: Basic configuration completed");

                // ===== СЕРВИСЫ ЗАМЕТОК И НАПОМИНАНИЙ =====
                builder.Services.AddSingleton<ILocalStorageService, LocalStorageService>();

                // Платформо-специфичные уведомления
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

                // Сервисы для редакторов
                builder.Services.AddSingleton<IDrawingService, DrawingService>();
                builder.Services.AddSingleton<ITextEditorService, TextEditorService>();

                // ===== СЕРВИСЫ КАЛЬКУЛЯТОРА =====
                // Добавляем сервисы калькулятора
                // builder.Services.AddSingleton<ICalculatorService, CalculatorService>();

                System.Diagnostics.Debug.WriteLine("Services registered successfully");

                // ===== РЕГИСТРАЦИЯ ВСЕХ СТРАНИЦ =====

                // Основные страницы
                builder.Services.AddTransient<MainPage>();

                // Страницы напоминаний
                builder.Services.AddTransient<ReminderPage>();
                builder.Services.AddTransient<AddReminderPage>();
                builder.Services.AddTransient<EditReminderPage>();

                // Страницы заметок/редактора
                builder.Services.AddTransient<CombinedEditorPage>();
                builder.Services.AddTransient<DrawingPage>();

                // Страница калькулятора
                builder.Services.AddTransient<CalculatorPage>();

#if DEBUG
                builder.Logging.AddDebug();
                builder.Logging.SetMinimumLevel(LogLevel.Debug);
#endif

                System.Diagnostics.Debug.WriteLine("MauiProgram: All services and pages registered");

                var app = builder.Build();

                System.Diagnostics.Debug.WriteLine("MauiProgram: Application built successfully");

                return app;
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MauiProgram ERROR: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }
    }
}