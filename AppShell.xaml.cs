namespace StudyMateTest
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(Views.DrawingPage), typeof(Views.DrawingPage));
            Routing.RegisterRoute(nameof(Views.CombinedEditorPage), typeof(Views.CombinedEditorPage));
        }
    }
}
