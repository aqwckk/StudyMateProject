using StudyMateProject.ViewModels;

namespace StudyMateProject.Views;

public partial class NotesPage : ContentPage
{
    private NotesViewModel _viewModel;

    public NotesPage(NotesViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadNotesCommand.Execute(null);
    }
}