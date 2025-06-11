using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace StudyMateTest.Models.TextEditor
{
    public class TextDocument : INotifyPropertyChanged
    {
        private string _content = "";
        private string _title = "Новый документ";
        private bool _isModified = false;

        public string Content 
        {
            get => _content;
            set 
            {
                if (_content != value) 
                {
                    _content = value;
                    IsModified = true;
                    OnPropertyChanged();
                }
            }
        }

        public string Title 
        {
            get => _title;
            set 
            {
                if (_title != value) 
                {
                    _title = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsModified
        {
            get => _isModified;
            set 
            {
                if (_isModified != value) 
                {
                    _isModified = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) 
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void MarkAsSaved() 
        {
            IsModified = false;
        }
    }
}
