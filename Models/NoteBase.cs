using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace StudyMateProject.Models
{
    /// <summary>
    /// Базовый класс для всех типов заметок
    /// </summary>
    public abstract class NoteBase
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime ModifiedDate { get; set; } = DateTime.Now;
        public List<string> Tags { get; set; } = new();
    }

    /// <summary>
    /// Представляет текстовую заметку
    /// </summary>
    public class TextNote : NoteBase
    {
        public string Content { get; set; } = string.Empty;
        public bool IsFormatted { get; set; } = false;
        public string FormattedContent { get; set; } = string.Empty;
    }

    /// <summary>
    /// Представляет графическую заметку с данными рисунка
    /// </summary>
    public class GraphicNote : NoteBase
    {
        public string SkiaSharpData { get; set; } = string.Empty;
        public List<byte[]> ImageData { get; set; } = new();
    }

    /// <summary>
    /// Представляет напоминание
    /// </summary>
    public class Reminder
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public bool IsCompleted { get; set; }
        public string RelatedNoteId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Настройки приложения
    /// </summary>
    public class AppSettings
    {
        public string Language { get; set; } = "ru-RU";
        public string Theme { get; set; } = "Light";
        public string LastOpenedNoteId { get; set; } = string.Empty;
        public bool AutosaveEnabled { get; set; } = true;
        public int AutosaveIntervalSeconds { get; set; } = 60;
    }

    /// <summary>
    /// Класс для хранения данных штриха для графических заметок
    /// </summary>
    public class DrawingStroke
    {
        public List<SKPointWrapper> Points { get; set; } = new();
        public float StrokeWidth { get; set; } = 5;
        private string _color = "#000000";
        public string Color
        {
            get => _color;
            set
            {
                // Проверяем, что значение является валидным цветом
                if (!string.IsNullOrEmpty(value) && value.StartsWith("#"))
                {
                    _color = value;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Invalid color value: {value}, using default");
                    _color = "#000000"; // Черный цвет по умолчанию
                }
            }
        }
        private string _strokeType = "Pen";
        public string StrokeType
        {
            get => _strokeType;
            set
            {
                // Проверяем, что значение является валидным типом штриха
                if (value == "Pen" || value == "Highlighter" || value == "Eraser")
                {
                    _strokeType = value;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Invalid stroke type: {value}, using default");
                    _strokeType = "Pen"; // Ручка по умолчанию
                }
            }
        }
    }

    /// <summary>
    /// Обертка для SkiaSharp SKPoint для возможности сериализации
    /// </summary>
    public class SKPointWrapper
    {
        public float X { get; set; }
        public float Y { get; set; }
    }
}