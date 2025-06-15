using StudyMateTest.Views;

namespace StudyMateTest
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            // Регистрируем маршруты для навигации
            RegisterRoutes();
        }

        private void RegisterRoutes()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("AppShell: Registering routes");

                // Основные страницы
                Routing.RegisterRoute("MainPage", typeof(MainPage));

                // Страницы заметок и редактора
                Routing.RegisterRoute("CombinedEditorPage", typeof(CombinedEditorPage));
                Routing.RegisterRoute("DrawingPage", typeof(DrawingPage));

                // Страницы напоминаний
                Routing.RegisterRoute("ReminderPage", typeof(ReminderPage));
                Routing.RegisterRoute("AddReminderPage", typeof(AddReminderPage));
                Routing.RegisterRoute("EditReminderPage", typeof(EditReminderPage));

                // Калькулятор
                Routing.RegisterRoute("CalculatorPage", typeof(CalculatorPage));

                // Дополнительные маршруты для внутренней навигации напоминаний
                Routing.RegisterRoute("ReminderPage/AddReminder", typeof(AddReminderPage));
                Routing.RegisterRoute("ReminderPage/EditReminder", typeof(EditReminderPage));

                // Маршруты для управления заметками
                Routing.RegisterRoute("NotesListPage/CreateNote", typeof(CreateNoteDialog));
                Routing.RegisterRoute("NotesListPage/EditNote", typeof(CombinedEditorPage));

                // Маршруты для управления заметками
                Routing.RegisterRoute("NotesListPage", typeof(NotesListPage));
                Routing.RegisterRoute("CreateNoteDialog", typeof(CreateNoteDialog));

                System.Diagnostics.Debug.WriteLine("AppShell: All routes registered successfully");
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AppShell ERROR registering routes: {ex.Message}");
                throw;
            }
        }
    }
}