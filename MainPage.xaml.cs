using Microsoft.Maui.Controls;

namespace StudyMateTest
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnCalculatorClicked(object sender, EventArgs e)
        {
            try
            {
                await Shell.Current.GoToAsync("//CalculatorPage");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось открыть калькулятор: {ex.Message}", "ОК");
            }
        }

        private async void OnNotebooksClicked(object sender, EventArgs e)
        {
            try
            {
                await Shell.Current.GoToAsync("//CombinedEditorPage");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось открыть тетради: {ex.Message}", "ОК");
            }
        }

        private async void OnRemindersClicked(object sender, EventArgs e)
        {
            try
            {
                await Shell.Current.GoToAsync("//ReminderPage");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось открыть напоминания: {ex.Message}", "ОК");
            }
        }
    }
}