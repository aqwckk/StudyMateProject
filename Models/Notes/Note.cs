using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StudyMateTest.Models
{
    public class Note : INotifyPropertyChanged
    {
        private string _id = Guid.NewGuid().ToString();
        private string _title = string.Empty;
        private string _description = string.Empty;
        private string _textContent = string.Empty;
        private byte[] _graphicsData;
        private DateTime _createdAt = DateTime.Now;
        private DateTime _lastModified = DateTime.Now;
        private bool _isModified = false;

        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public string Title
        {
            get => _title;
            set
            {
                if (SetProperty(ref _title, value))
                {
                    MarkAsModified();
                    OnPropertyChanged(nameof(DisplayTitle));
                }
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                if (SetProperty(ref _description, value))
                {
                    MarkAsModified();
                    OnPropertyChanged(nameof(DisplayDescription));
                }
            }
        }

        public string TextContent
        {
            get => _textContent;
            set
            {
                if (SetProperty(ref _textContent, value))
                {
                    MarkAsModified();
                    OnPropertyChanged(nameof(HasTextContent));
                    OnPropertyChanged(nameof(TextPreview));
                }
            }
        }

        public byte[] GraphicsData
        {
            get => _graphicsData;
            set
            {
                if (SetProperty(ref _graphicsData, value))
                {
                    MarkAsModified();
                    OnPropertyChanged(nameof(HasGraphicsContent));
                }
            }
        }

        public DateTime CreatedAt
        {
            get => _createdAt;
            set => SetProperty(ref _createdAt, value);
        }

        public DateTime LastModified
        {
            get => _lastModified;
            set => SetProperty(ref _lastModified, value);
        }

        public bool IsModified
        {
            get => _isModified;
            set => SetProperty(ref _isModified, value);
        }

        public string DisplayTitle => string.IsNullOrWhiteSpace(Title) ? "Новая заметка" : Title;

        public string DisplayDescription => string.IsNullOrWhiteSpace(Description) ? "Без описания" : Description;

        public bool HasTextContent => !string.IsNullOrWhiteSpace(TextContent);

        public bool HasGraphicsContent => GraphicsData != null && GraphicsData.Length > 0;

        public string TextPreview
        {
            get
            {
                if (string.IsNullOrWhiteSpace(TextContent))
                    return "Нет текста";

                var plainText = System.Text.RegularExpressions.Regex.Replace(
                    TextContent, "<[^>]+>", "")
                    .Replace("&nbsp;", " ")
                    .Replace("&lt;", "<")
                    .Replace("&gt;", ">")
                    .Replace("&amp;", "&");

                if (plainText.Length > 100)
                    return plainText.Substring(0, 100) + "...";

                return plainText;
            }
        }

        public string ContentSummary
        {
            get
            {
                var parts = new List<string>();

                if (HasTextContent)
                    parts.Add("📝 Текст");
                if (HasGraphicsContent)
                    parts.Add("🎨 Рисунок");

                return parts.Count > 0 ? string.Join(" • ", parts) : "Пустая заметка";
            }
        }

        public string LastModifiedText
        {
            get
            {
                var timeDiff = DateTime.Now - LastModified;

                if (timeDiff.TotalMinutes < 1)
                    return "Только что";
                else if (timeDiff.TotalMinutes < 60)
                    return $"{(int)timeDiff.TotalMinutes} мин назад";
                else if (timeDiff.TotalHours < 24)
                    return $"{(int)timeDiff.TotalHours} ч назад";
                else if (timeDiff.TotalDays < 7)
                    return $"{(int)timeDiff.TotalDays} дн назад";
                else
                    return LastModified.ToString("dd.MM.yyyy");
            }
        }

        public string StatusColor => IsModified ? "#F59E0B" : "#10B981";

        public string StatusText => IsModified ? "Изменено" : "Сохранено";

        public void MarkAsModified()
        {
            IsModified = true;
            LastModified = DateTime.Now;
            OnPropertyChanged(nameof(LastModifiedText));
            OnPropertyChanged(nameof(StatusColor));
            OnPropertyChanged(nameof(StatusText));
        }

        public void MarkAsSaved()
        {
            IsModified = false;
            OnPropertyChanged(nameof(StatusColor));
            OnPropertyChanged(nameof(StatusText));
        }

        public Note CreateCopy()
        {
            return new Note
            {
                Id = Guid.NewGuid().ToString(),
                Title = $"{Title} (копия)",
                Description = Description,
                TextContent = TextContent,
                GraphicsData = GraphicsData?.ToArray(),
                CreatedAt = DateTime.Now,
                LastModified = DateTime.Now,
                IsModified = true
            };
        }

        public bool IsEmpty()
        {
            return string.IsNullOrWhiteSpace(Title) &&
                   string.IsNullOrWhiteSpace(Description) &&
                   string.IsNullOrWhiteSpace(TextContent) &&
                   (GraphicsData == null || GraphicsData.Length == 0);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}