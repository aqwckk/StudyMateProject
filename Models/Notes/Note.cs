using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StudyMateTest.Models
{
    // модель для хранения заметки (реализует INotifyPropertyChanged для уведомления UI об изменениях)
    public class Note : INotifyPropertyChanged
    {
        // поля заметки
        private string _id = Guid.NewGuid().ToString(); // уникальный идентификатор заметки (генерируется автоматически)
        private string _title = string.Empty; // заголовок заметки (по умолчанию пустая строка)
        private string _description = string.Empty; // описание заметки (по умолчанию пустая строка)
        private string _textContent = string.Empty; // текстовое содержимое заметки (может содержать HTML)
        private byte[] _graphicsData; // графические данные в виде массива байт (изображения, рисунки)
        private DateTime _createdAt = DateTime.Now; // время создания заметки (устанавливается при создании)
        private DateTime _lastModified = DateTime.Now; // время последнего изменения заметки
        private bool _isModified = false; // флаг для отслеживания наличия несохраненных изменений

        // свойства

        // свойство для уникального идентификатора
        public string Id
        {
            get => _id; // возвращаем значение поля _id
            set => SetProperty(ref _id, value); // устанавливаем значение через SetProperty для автоматического уведомления UI
        }
        // свойство для заголовка заметки
        public string Title
        {
            get => _title; // получаем заголовок
            set
            {
                if (SetProperty(ref _title, value)) // если значение действительно изменилось
                {
                    MarkAsModified(); // помечаем заметку как измененную
                    OnPropertyChanged(nameof(DisplayTitle)); // уведомляем об изменении связанного свойства DisplayTitle
                }
            }
        }

        // свойство для описания заметки
        public string Description
        {
            get => _description; // возвращаем описание
            set
            {
                if (SetProperty(ref _description, value)) // если значение действительно изменилось
                {
                    MarkAsModified(); // помечаем заметку как измененную
                    OnPropertyChanged(nameof(DisplayDescription)); // уведомляем об изменении связанного свойства DisplayDescription
                }
            }
        }

        // свойство для текстового содержимого
        public string TextContent
        {
            get => _textContent; // возвращаем текстовое содержимое
            set
            {
                if (SetProperty(ref _textContent, value)) // если значение действительно изменилось
                {
                    MarkAsModified(); // помечаем заметку как измененную
                    OnPropertyChanged(nameof(HasTextContent)); // уведомляем об изменении свойства HasTextContent
                    OnPropertyChanged(nameof(TextPreview)); // уведомляем об изменении свойства TextPreview
                }
            }
        }

        // свойство для графических данных
        public byte[] GraphicsData
        {
            get => _graphicsData; // возвращаем графические данные
            set
            {
                if (SetProperty(ref _graphicsData, value)) // если значение действительно изменилось
                {
                    MarkAsModified(); // помечаем заметку как измененную
                    OnPropertyChanged(nameof(HasGraphicsContent)); // уведомляем об изменении свойства HasGraphicsContent
                }
            }
        }

        // свойство для времени создания
        public DateTime CreatedAt
        {
            get => _createdAt; // возвращаем время создания
            set => SetProperty(ref _createdAt, value); // устанавливаем через SetProperty (обычно не изменяется после создания)
        }

        // свойство для времени последнего изменения
        public DateTime LastModified
        {
            get => _lastModified; // возвращаем время последнего изменения
            set => SetProperty(ref _lastModified, value); // устанавливаем через SetProperty
        }

        // свойство для флага изменений
        public bool IsModified
        {
            get => _isModified; // возвращаем флаг изменений
            set => SetProperty(ref _isModified, value); // устанавливаем через SetProperty
        }

        // отображаемый заголовок (с fallback на "Новая заметка" если пустой)
        public string DisplayTitle => string.IsNullOrWhiteSpace(Title) ? "Новая заметка" : Title;

        // отображаемое описание (с fallback на "Без описания" если пустое)
        public string DisplayDescription => string.IsNullOrWhiteSpace(Description) ? "Без описания" : Description;

        // проверка наличия текстового содержимого (возвращает true если текст не пустой)
        public bool HasTextContent => !string.IsNullOrWhiteSpace(TextContent);

        // проверка наличия графического содержимого (возвращает true если массив байт не null и не пустой)
        public bool HasGraphicsContent => GraphicsData != null && GraphicsData.Length > 0;

        // краткий предпросмотр текста (очищенный от HTML тегов, максимум 100 символов)
        public string TextPreview
        {
            get
            {
                if (string.IsNullOrWhiteSpace(TextContent)) // если текст пустой
                    return "Нет текста"; // возвращаем заглушку

                // очищаем текст от HTML тегов и HTML entities с помощью регулярного выражения
                var plainText = System.Text.RegularExpressions.Regex.Replace(
                    TextContent, "<[^>]+>", "") // удаляем все HTML теги (все что между < и >)
                    .Replace("&nbsp;", " ") // заменяем неразрывные пробелы на обычные
                    .Replace("&lt;", "<") // декодируем HTML entities для символа <
                    .Replace("&gt;", ">") // декодируем HTML entities для символа >
                    .Replace("&amp;", "&"); // декодируем HTML entities для символа &

                if (plainText.Length > 100) // если очищенный текст длиннее 100 символов
                    return plainText.Substring(0, 100) + "..."; // обрезаем до 100 символов и добавляем многоточие

                return plainText; // возвращаем полный текст если он короткий
            }
        }

        // сводка содержимого заметки с эмодзи (показывает что содержит заметка)
        public string ContentSummary
        {
            get
            {
                var parts = new List<string>(); // создаем список частей сводки

                if (HasTextContent) // если есть текстовое содержимое
                    parts.Add("📝 Текст"); // добавляем индикатор текста с эмодзи
                if (HasGraphicsContent) // если есть графическое содержимое
                    parts.Add("🎨 Рисунок"); // добавляем индикатор рисунка с эмодзи

                return parts.Count > 0 ? string.Join(" • ", parts) : "Пустая заметка"; // объединяем части через • или возвращаем "Пустая заметка"
            }
        }

        // человекопонятное отображение времени последнего изменения (относительное время)
        public string LastModifiedText
        {
            get
            {
                var timeDiff = DateTime.Now - LastModified; // вычисляем разность между текущим временем и временем изменения

                if (timeDiff.TotalMinutes < 1) // если прошло меньше минуты
                    return "Только что";
                else if (timeDiff.TotalMinutes < 60) // если прошло меньше часа
                    return $"{(int)timeDiff.TotalMinutes} мин назад";
                else if (timeDiff.TotalHours < 24) // если прошло меньше дня
                    return $"{(int)timeDiff.TotalHours} ч назад";
                else if (timeDiff.TotalDays < 7) // если прошло меньше недели
                    return $"{(int)timeDiff.TotalDays} дн назад";
                else // если прошло больше недели
                    return LastModified.ToString("dd.MM.yyyy"); // возвращаем дату в формате ДД.ММ.ГГГГ
            }
        }

        // цвет статуса в HEX формате (оранжевый #F59E0B для измененных, зеленый #10B981 для сохраненных)
        public string StatusColor => IsModified ? "#F59E0B" : "#10B981";

        // текст статуса заметки
        public string StatusText => IsModified ? "Изменено" : "Сохранено";

        // метод для пометки заметки как измененной (вызывается при любом изменении содержимого)
        public void MarkAsModified()
        {
            IsModified = true; // устанавливаем флаг изменений
            LastModified = DateTime.Now; // обновляем время последнего изменения
            OnPropertyChanged(nameof(LastModifiedText)); // уведомляем об изменении отображения времени
            OnPropertyChanged(nameof(StatusColor)); // уведомляем об изменении цвета статуса
            OnPropertyChanged(nameof(StatusText)); // уведомляем об изменении текста статуса
        }

        // метод для пометки заметки как сохраненной (вызывается после успешного сохранения)
        public void MarkAsSaved()
        {
            IsModified = false; // сбрасываем флаг изменений
            OnPropertyChanged(nameof(StatusColor)); // уведомляем об изменении цвета статуса
            OnPropertyChanged(nameof(StatusText)); // уведомляем об изменении текста статуса
        }

        // метод для создания копии заметки с новым ID
        public Note CreateCopy()
        {
            return new Note // создаем новую заметку
            {
                Id = Guid.NewGuid().ToString(), // генерируем новый уникальный ID для копии
                Title = $"{Title} (копия)", // добавляем "(копия)" к заголовку
                Description = Description, // копируем описание
                TextContent = TextContent, // копируем текстовое содержимое
                GraphicsData = GraphicsData?.ToArray(), // создаем копию массива графических данных (если не null)
                CreatedAt = DateTime.Now, // устанавливаем текущее время как время создания копии
                LastModified = DateTime.Now, // устанавливаем текущее время как время последнего изменения
                IsModified = true // помечаем копию как измененную (требует сохранения)
            };
        }

        // метод для проверки является ли заметка пустой (не содержит никакого содержимого)
        public bool IsEmpty()
        {
            return string.IsNullOrWhiteSpace(Title) && // заголовок пустой или содержит только пробелы
                   string.IsNullOrWhiteSpace(Description) && // описание пустое или содержит только пробелы
                   string.IsNullOrWhiteSpace(TextContent) && // текстовое содержимое пустое или содержит только пробелы
                   (GraphicsData == null || GraphicsData.Length == 0); // графические данные отсутствуют или массив пустой
        }

        // событие для уведомления подписчиков об изменении свойств (часть интерфейса INotifyPropertyChanged)
        public event PropertyChangedEventHandler PropertyChanged;

        // защищенный метод для вызова события PropertyChanged
        // [CallerMemberName] автоматически подставляет имя вызывающего свойства/метода на этапе компиляции
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); // вызываем событие если есть подписчики (null-conditional оператор ?.)
        }

        // универсальный метод для установки значения свойства с автоматическим уведомлением об изменении
        // ref T backingStore - ссылка на приватное поле которое нужно изменить
        // T value - новое значение для установки
        // [CallerMemberName] - автоматически получает имя вызывающего свойства на этапе компиляции
        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value)) // сравниваем текущее и новое значение используя стандартный компаратор для типа T
                return false; // если значения равны - возвращаем false (изменений не было)

            backingStore = value; // устанавливаем новое значение в поле по ссылке
            OnPropertyChanged(propertyName); // уведомляем подписчиков об изменении свойства
            return true; // возвращаем true - изменение произошло успешно
        }
    }
}