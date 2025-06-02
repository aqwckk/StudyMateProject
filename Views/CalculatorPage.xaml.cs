using StudyMateProject.ViewModels;

namespace StudyMateProject.Views;

public partial class CalculatorPage : ContentPage
{
    private CalculatorViewModel _viewModel;

    public CalculatorPage(CalculatorViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }
}