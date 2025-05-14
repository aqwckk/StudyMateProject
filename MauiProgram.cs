using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using StudyMateProject.Services;
using StudyMateProject.ViewModels;
using StudyMateProject.Views;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace StudyMateProject;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("Starting CreateMauiApp...");

            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseSkiaSharp()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if !WINDOWS
            // Инициализируем уведомления только на не-Windows платформах
            builder.UseLocalNotification();
#endif

            // Register services
            System.Diagnostics.Debug.WriteLine("Registering services...");
            builder.Services.AddSingleton<IFileService, FileService>();
            builder.Services.AddSingleton<INoteService, NoteService>();
            builder.Services.AddSingleton<IReminderService, ReminderService>();

            // Register views
            System.Diagnostics.Debug.WriteLine("Registering views...");
            builder.Services.AddTransient<NotesPage>();
            builder.Services.AddTransient<RemindersPage>();
            builder.Services.AddTransient<CalculatorPage>();
            builder.Services.AddTransient<SettingsPage>();
            builder.Services.AddTransient<TextNotePage>();
            builder.Services.AddTransient<GraphicNotePage>();
            builder.Services.AddTransient<EditReminderPage>();

            // Register view models
            System.Diagnostics.Debug.WriteLine("Registering view models...");
            builder.Services.AddTransient<NotesViewModel>();
            builder.Services.AddTransient<RemindersViewModel>();
            builder.Services.AddTransient<CalculatorViewModel>();

            // ViewModels with multiple dependencies
            builder.Services.AddTransient<TextNoteViewModel>(sp =>
                new TextNoteViewModel(
                    sp.GetRequiredService<INoteService>(),
                    sp.GetRequiredService<IFileService>()
                ));

            builder.Services.AddTransient<GraphicNoteViewModel>(sp =>
                new GraphicNoteViewModel(
                    sp.GetRequiredService<INoteService>(),
                    sp.GetRequiredService<IFileService>()
                ));

#if DEBUG
            builder.Logging.AddDebug();
#endif

            System.Diagnostics.Debug.WriteLine("MauiApp building completed");
            return builder.Build();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ERROR in CreateMauiApp: {ex.Message}\n{ex.StackTrace}");
            throw;
        }
    }
}