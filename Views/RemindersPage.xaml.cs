using StudyMateProject.Models;
using StudyMateProject.ViewModels;

namespace StudyMateProject.Views;

public partial class RemindersPage : ContentPage
{
    private RemindersViewModel _viewModel;

    public RemindersPage(RemindersViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadRemindersCommand.Execute(null);
    }

    private void OnReminderCompletedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (sender is CheckBox checkBox && checkBox.BindingContext is Reminder reminder)
        {
            _viewModel.ToggleCompletedCommand.Execute(reminder.Id);
        }
    }
}