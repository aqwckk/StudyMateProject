using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace StudyMateTest.Models.TextEditor
{
    public class TextDocument : INotifyPropertyChanged // модель текстового документа (реализует INotifyPropertyChanged для уведомления UI об изменениях)
    {
        private string _content = ""; // строка для хранения текстового содержимого (по умолчанию пустая)
        private string _title = "Новый документ"; // строка для заголовка документа (по умолчанию "Новый документ")
        private bool _isModified = false; // булево переменная для отслеживания изменения текстового документа (по умолчанию false - не изменен)

        public string Content // свойство для содержимого документа
        {
            get => _content; // получаем содержимое
            set
            {
                if (_content != value) // если текущее содержимое не равно переданному значению
                {
                    _content = value; // обновляем содержимое на новое значение
                    IsModified = true; // помечаем документ как измененный
                    OnPropertyChanged(); // вызываем метод для уведомления подписчиков об изменении свойства (CallerMemberName автоматически передаст "Content")
                }
            }
        }

        // свойство для заголовка документа
        public string Title
        {
            get => _title; // получаем заголовок
            set
            {
                if (_title != value) // если текущий заголовок не равен переданному значению
                {
                    _title = value; // обновляем заголовок на новое значение
                    OnPropertyChanged(); // уведомляем подписчиков об изменении свойства Title
                }
            }
        }

        // свойство для флага изменений документа
        public bool IsModified
        {
            get => _isModified; // получаем флаг изменений
            set
            {
                if (_isModified != value) // если текущий флаг не равен переданному значению
                {
                    _isModified = value; // обновляем флаг изменений
                    OnPropertyChanged(); // уведомляем подписчиков об изменении свойства IsModified
                }
            }
        }

        // событие для уведомления подписчиков об изменении свойств (часть интерфейса INotifyPropertyChanged)
        public event PropertyChangedEventHandler PropertyChanged;

        // защищенный виртуальный метод для вызова события PropertyChanged
        // [CallerMemberName] автоматически подставляет имя вызывающего свойства на этапе компиляции
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); // вызываем событие если есть подписчики (null-conditional оператор ?.)
        }

        // метод для пометки документа как сохраненного (сбрасывает флаг изменений)
        public void MarkAsSaved()
        {
            IsModified = false; // устанавливаем флаг изменений в false (документ сохранен)
        }
    }
}
