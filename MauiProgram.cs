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
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseSkiaSharp() // Add SkiaSharp support
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Register services
        builder.Services.AddSingleton<INoteService, NoteService>();
        builder.Services.AddSingleton<IReminderService, ReminderService>();
        builder.Services.AddSingleton<IFileService, FileService>();

        // Register views and view models
        builder.Services.AddTransient<NotesPage>();
        builder.Services.AddTransient<NotesViewModel>();

        builder.Services.AddTransient<TextNotePage>();
        builder.Services.AddTransient<TextNoteViewModel>();

        builder.Services.AddTransient<GraphicNotePage>();
        builder.Services.AddTransient<GraphicNoteViewModel>();

        builder.Services.AddTransient<RemindersPage>();
        builder.Services.AddTransient<RemindersViewModel>();

        builder.Services.AddTransient<CalculatorPage>();
        builder.Services.AddTransient<CalculatorViewModel>();

        builder.Services.AddTransient<SettingsPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}