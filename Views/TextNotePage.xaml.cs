using StudyMateProject.ViewModels;

namespace StudyMateProject.Views;

[QueryProperty(nameof(NoteId), "NoteId")]
public partial class TextNotePage : ContentPage
{
    private TextNoteViewModel _viewModel;

    public string NoteId
    {
        set
        {
            if (!string.IsNullOrEmpty(value))
            {
                _viewModel.NoteId = value;
            }
        }
    }

    public TextNotePage(TextNoteViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        // Auto-save when navigating away
        _viewModel.SaveNoteCommand.Execute(null);
    }
}