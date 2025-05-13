using StudyMateProject.Services;

namespace StudyMateProject;

public partial class App : Application
{
    public IServiceProvider Services { get; }

    public App(IServiceProvider services)
    {
        InitializeComponent();

        Services = services;
        MainPage = new AppShell();

        // Initialize notification service
        NotificationService.Initialize();
    }

    protected override void OnStart()
    {
        base.OnStart();
    }

    protected override void OnSleep()
    {
        base.OnSleep();
        // TODO: Save any unsaved changes
    }

    protected override void OnResume()
    {
        base.OnResume();
    }
}