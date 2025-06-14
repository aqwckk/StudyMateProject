namespace StudyMateTest
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Регистрируем маршруты для навигации
            Routing.RegisterRoute(nameof(Views.DrawingPage), typeof(Views.DrawingPage));
            Routing.RegisterRoute(nameof(Views.CombinedEditorPage), typeof(Views.CombinedEditorPage));
            Routing.RegisterRoute(nameof(Views.ReminderPage), typeof(Views.ReminderPage));
            Routing.RegisterRoute(nameof(Views.AddReminderPage), typeof(Views.AddReminderPage));
            Routing.RegisterRoute(nameof(Views.EditReminderPage), typeof(Views.EditReminderPage));
        }
    }
}