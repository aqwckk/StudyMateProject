using StudyMateTest.Services.DrawingServices;
using StudyMateTest.Services.TextEditorServices;
using StudyMateTest.Views.Enums;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyMateTest.ViewModels
{
    public class CombinedEditorViewModel : INotifyPropertyChanged
    {
        private EditorMode _currentMode = EditorMode.Graphics;

        public DrawingPageViewModel DrawingViewModel { get; }
        public TextEditorViewModel TextViewModel { get; }

        public EditorMode CurrentMode 
        {
            get => _currentMode;
            set 
            {
                if (_currentMode != value) 
                {
                    _currentMode = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public CombinedEditorViewModel(IDrawingService drawingService, ITextEditorService textEditorService) 
        {
            DrawingViewModel = new DrawingPageViewModel(drawingService);
            TextViewModel = new TextEditorViewModel(textEditorService);
        }

        public CombinedEditorViewModel() 
        {
            DrawingViewModel = new DrawingPageViewModel();
            TextViewModel = new TextEditorViewModel();
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
