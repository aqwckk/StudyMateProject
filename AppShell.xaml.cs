using StudyMateProject.Views;

namespace StudyMateProject;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Register routes for navigation
        Routing.RegisterRoute("textNote", typeof(TextNotePage));
        Routing.RegisterRoute("graphicNote", typeof(GraphicNotePage));
        Routing.RegisterRoute("editReminder", typeof(EditReminderPage));
    }
}