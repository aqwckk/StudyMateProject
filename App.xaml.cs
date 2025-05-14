using StudyMateProject.Services;

namespace StudyMateProject;

public partial class App : Application
{
    public IServiceProvider Services { get; }

    public App()
    {
        try
        {
            InitializeComponent();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in App constructor: {ex.Message}\n{ex.StackTrace}");
        }
    }

    public App(IServiceProvider services) : this()
    {
        try
        {
            // Добавляем обработчики исключений
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                var ex = e.ExceptionObject as Exception;
                System.Diagnostics.Debug.WriteLine($"CRITICAL ERROR: {ex?.Message}\n{ex?.StackTrace}");
            };

            Services = services;

            // Создаем главную страницу
            System.Diagnostics.Debug.WriteLine("Creating MainPage...");
            MainPage = new AppShell();
            System.Diagnostics.Debug.WriteLine("MainPage created successfully");

            // Инициализация уведомлений
            if (DeviceInfo.Platform != DevicePlatform.WinUI)
            {
                NotificationService.Initialize();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Notifications disabled on Windows");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in App(IServiceProvider) constructor: {ex.Message}\n{ex.StackTrace}");

            // В случае ошибки создаем простую страницу для диагностики
            MainPage = new ContentPage
            {
                BackgroundColor = Colors.White,
                Content = new VerticalStackLayout
                {
                    Spacing = 20,
                    Padding = new Thickness(30),
                    VerticalOptions = LayoutOptions.Center,
                    Children =
                    {
                        new Label
                        {
                            Text = "Произошла ошибка при инициализации приложения",
                            TextColor = Colors.Red,
                            FontSize = 20,
                            HorizontalOptions = LayoutOptions.Center
                        },
                        new Label
                        {
                            Text = ex.Message,
                            TextColor = Colors.Black,
                            FontSize = 16,
                            HorizontalOptions = LayoutOptions.Center
                        }
                    }
                }
            };
        }
    }

    protected override void OnStart()
    {
        base.OnStart();
        System.Diagnostics.Debug.WriteLine("App.OnStart called");
    }

    protected override void OnSleep()
    {
        base.OnSleep();
        System.Diagnostics.Debug.WriteLine("App.OnSleep called");
    }

    protected override void OnResume()
    {
        base.OnResume();
        System.Diagnostics.Debug.WriteLine("App.OnResume called");
    }

    protected override Window CreateWindow(IActivationState activationState)
    {
        System.Diagnostics.Debug.WriteLine("App.CreateWindow called");

        try
        {
            var window = base.CreateWindow(activationState);

            window.Created += (s, e) => System.Diagnostics.Debug.WriteLine("Window.Created");
            window.Activated += (s, e) => System.Diagnostics.Debug.WriteLine("Window.Activated");
            window.Deactivated += (s, e) => System.Diagnostics.Debug.WriteLine("Window.Deactivated");
            window.Stopped += (s, e) => System.Diagnostics.Debug.WriteLine("Window.Stopped");
            window.Resumed += (s, e) => System.Diagnostics.Debug.WriteLine("Window.Resumed");
            window.Destroying += (s, e) => System.Diagnostics.Debug.WriteLine("Window.Destroying");

            return window;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in CreateWindow: {ex.Message}\n{ex.StackTrace}");
            throw;
        }
    }
}