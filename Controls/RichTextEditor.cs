using Microsoft.Maui.Controls;
using System.ComponentModel;

namespace StudyMateTest.Controls
{
    public class RichTextEditor : ContentView
    {
        #region Привязываемые свойства
        // Свойство для текста
        public static readonly BindableProperty TextProperty = BindableProperty.Create(
            nameof(Text), // Имя свойства - Text
            typeof(string), // Тип данных свойства - string
            typeof(RichTextEditor), // Класс-владелец - текущий класс (RichTextEditor)
            string.Empty, // Значение по умолчанию - пустая строка
            BindingMode.TwoWay, // Режим привязки - двусторонний 
            propertyChanged: OnTextPropertyChanged); // callback (действие при изменении) - OnTextPropertyChanged

        // Свойство для размера шрифта
        public static readonly BindableProperty FontSizeProperty = BindableProperty.Create(
            nameof(FontSize), // Имя свойства - FontSize
            typeof(double), // Тип данных свойства - double (дробное число)
            typeof(RichTextEditor), // Класс-владелец - текущий класс (RichTextEditor)
            14.0, // Значение по умолчанию - 14.0 (стандартный размер шрифта)
            propertyChanged: OnFontSizePropertyChanged); // callback (действие при изменении) - OnFontSizePropertyChanged

        // Свойство для семейства шрифта
        public static readonly BindableProperty FontFamilyProperty = BindableProperty.Create(
            nameof(FontFamily), // Имя свойства - FontFamily
            typeof(string), // Тип данных свойства - string
            typeof(RichTextEditor), // Класс-владелец - текущий класс (RichTextEditor)
            "Arial", // Значение по умолчанию - "Arial" (стандартный шрифт)
            propertyChanged: OnFontFamilyPropertyChanged); // callback (действие при изменении) - OnFontFamilyPropertyChanged

        // Свойство для жирного начертания
        public static readonly BindableProperty IsBoldProperty = BindableProperty.Create(
            nameof(IsBold), // Имя свойства - IsBold
            typeof(bool), // Тип данных свойства - bool 
            typeof(RichTextEditor), // Класс-владелец - текущий класс (RichTextEditor)
            false, // Значение по умолчанию - false (не жирный)
            BindingMode.TwoWay, // Режим привязки - двусторонний
            propertyChanged: OnFormattingPropertyChanged); // callback (действие при изменении) - OnFormattingPropertyChanged

        // Свойство для курсивного начертания
        public static readonly BindableProperty IsItalicProperty = BindableProperty.Create(
            nameof(IsItalic), // Имя свойства - IsItalic
            typeof(bool), // Тип данных свойства - bool 
            typeof(RichTextEditor), // Класс-владелец - текущий класс (RichTextEditor)
            false, // Значение по умолчанию - false (не курсивный)
            BindingMode.TwoWay, // Режим привязки - двусторонний
            propertyChanged: OnFormattingPropertyChanged); // callback (действие при изменении) - OnFormattingPropertyChanged

        // Свойство для выравнивания текста
        public static readonly BindableProperty TextAlignmentProperty = BindableProperty.Create(
            nameof(TextAlignment), // Имя свойства - TextAlignment
            typeof(TextAlignment), // Тип данных свойства - TextAlignment (перечисление: Start, Center, End)
            typeof(RichTextEditor), // Класс-владелец - текущий класс (RichTextEditor)
            TextAlignment.Start, // Значение по умолчанию - TextAlignment.Start (выравнивание по левому краю)
            BindingMode.TwoWay, // Режим привязки - двусторонний
            propertyChanged: OnFormattingPropertyChanged); // callback (действие при изменении) - OnFormattingPropertyChanged
        #endregion

        private Editor _editor; // переменная для хранения базового редактора текста в MAUI
        private ScrollView _scrollView; // переменная для хранения прокручиваемой области (область отображения текста)
        private FormattingInfo _currentFormatting = new FormattingInfo(); // создаем объект для хранения информации о форматировании текста

        public string Text // свойство для текста (обертка)
        {
            get => (string)GetValue(TextProperty); // получаем значение из привязываемое свойства, объявленного выше
            set => SetValue(TextProperty, value); // устанавливаем значение с помощью привязываемого свойства, объявленного выше
        }

        public double FontSize // свойство для размера шрифта (обертка)
        {
            get => (double)GetValue(FontSizeProperty); // получаем значение из привязываемого свойства FontSizeProperty
            set => SetValue(FontSizeProperty, value); // устанавливаем значение с помощью привязываемого свойства FontSizeProperty
        }

        public string FontFamily // свойство для семейства шрифта (обертка)
        {
            get => (string)GetValue(FontFamilyProperty); // получаем значение из привязываемого свойства FontFamilyProperty
            set => SetValue(FontFamilyProperty, value); // устанавливаем значение с помощью привязываемого свойства FontFamilyProperty
        }

        public bool IsBold // свойство для жирного начертания текста (обертка)
        {
            get => (bool)GetValue(IsBoldProperty); // получаем значение из привязываемого свойства IsBoldProperty
            set => SetValue(IsBoldProperty, value); // устанавливаем значение с помощью привязываемого свойства IsBoldProperty
        }

        public bool IsItalic // свойство для курсивного начертания текста (обертка)
        {
            get => (bool)GetValue(IsItalicProperty); // получаем значение из привязываемого свойства IsItalicProperty
            set => SetValue(IsItalicProperty, value); // устанавливаем значение с помощью привязываемого свойства IsItalicProperty
        }

        public TextAlignment TextAlignment // свойство для выравнивания текста (обертка)
        {
            get => (TextAlignment)GetValue(TextAlignmentProperty); // получаем значение из привязываемого свойства TextAlignmentProperty
            set => SetValue(TextAlignmentProperty, value); // устанавливаем значение с помощью привязываемого свойства TextAlignmentProperty
        }

        // конструктор по умолчанию
        public RichTextEditor()
        {
            CreateContent(); // создаем содержимое облассти для редактирования текста
        }

        // функция для создания объекта текстового редактора
        private void CreateContent()
        {
            _editor = new Editor // создаем новый Editor
            {
                BackgroundColor = Colors.White, // фон - белый
                TextColor = Colors.Black, // текст - черный
                AutoSize = EditorAutoSizeOption.TextChanges, // автоматически меняем размер при изменении текста
                Placeholder = "Введите текст...", // приглашение-плейсхолдер
                PlaceholderColor = Colors.Gray, // цвет приглашения-плейсхолдера
                MinimumHeightRequest = 300 // минимальная высота
            };

            // подписываемся на события для слежения за изменением текста
            _editor.TextChanged += OnEditorTextChanged; // изменение текста в Editor
            _editor.Focused += OnEditorFocused; // фокусировка на editor
            _editor.Unfocused += OnEditorUnfocused; // расфокусировка с Editor

            _scrollView = new ScrollView // создаем новый контейнер для отображения editor
            {
                Content = _editor // содержимое - editor
            };

            Content = _scrollView; // содержимое области редатирования текста - ScrollView (контейнер для editor)
            UpdateEditorProperties(); // обновляем свойства editor
        }

        private void OnEditorTextChanged(object sender, TextChangedEventArgs e) // On-метод для отслеживания изменения текста
        {
            SetValue(TextProperty, e.NewTextValue); // устанавливаем новое значение через TextProperty
        }

        private void OnEditorFocused(object sender, FocusEventArgs e) // On-метод для отслеживания фокусировки на editor
        {
            // При получении фокуса обновляем текущее форматирование
            UpdateCurrentFormattingFromCursor();
        }

        private void OnEditorUnfocused(object sender, FocusEventArgs e) // On-метод для отслеживания расфокусировки на editor
        {
            // При потере фокуса можно сохранить текущее состояние
        }

        // метод для синхронизации текущего форматирования с внутренним
        private void UpdateCurrentFormattingFromCursor()
        {
            _currentFormatting.IsBold = IsBold; // сохраняем текущее состояние жирности
            _currentFormatting.IsItalic = IsItalic; // сохраняем текущее состояние курсива
            _currentFormatting.FontSize = FontSize; // сохраняем текущий размер шрифта
            _currentFormatting.FontFamily = FontFamily; // сохраняем текущее семейство шрифта
            _currentFormatting.TextAlignment = TextAlignment; // сохраняем текущее выравнивание
        }

        // on-метод для отслеживания изменения текста
        private static void OnTextPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            // если объект, у которого изменилось свойство (bindable) является этим контролом и новое значение - строка
            if (bindable is RichTextEditor control && newValue is string text)
            {
                // если текущий текст не равен null и передаваемый текст не равен текущему
                if (control._editor != null && control._editor.Text != text)
                {
                    control._editor.Text = text; // обновляем текст
                }
            }
        }

        // on-метод для отслеживания изменения размера шрифта
        private static void OnFontSizePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            // если объект, у которого изменилось свойство (bindable) является этим контролом
            if (bindable is RichTextEditor control)
            {
                control._currentFormatting.FontSize = (double)newValue; // обновляем размер шрифта в текущем форматировании
                control.ApplyFormattingToSelection(); // применяем форматирование к выделенному тексту
            }
        }

        // on-метод для отслеживания изменения семейства шрифта
        private static void OnFontFamilyPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            // если объект, у которого изменилось свойство (bindable) является этим контролом
            if (bindable is RichTextEditor control)
            {
                control._currentFormatting.FontFamily = (string)newValue; // обновляем семейство шрифта в текущем форматировании
                control.ApplyFormattingToSelection(); // применяем форматирование к выделенному тексту
            }
        }

        // on-метод для отслеживания изменения свойств форматирования (жирный, курсив, выравнивание)
        private static void OnFormattingPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            // если объект, у которого изменилось свойство (bindable) является этим контролом
            if (bindable is RichTextEditor control)
            {
                control.UpdateCurrentFormattingFromCursor(); // обновляем текущее форматирование из позиции курсора
                control.ApplyFormattingToSelection(); // применяем форматирование к выделенному тексту
            }
        }

        // меод для обноваления свойств Editor
        private void UpdateEditorProperties()
        {
            if (_editor != null) // если editor не null
            {
                // обновляем размер, семейство и выравнивание текста
                _editor.FontSize = FontSize;
                _editor.FontFamily = FontFamily;
                _editor.HorizontalTextAlignment = TextAlignment;

                // Применяем форматирование
                FontAttributes attributes = FontAttributes.None;
                if (IsBold) attributes |= FontAttributes.Bold;
                if (IsItalic) attributes |= FontAttributes.Italic;
                _editor.FontAttributes = attributes;
            }
        }

        // метод для применения форматирования к выбранному фрагменту
        private void ApplyFormattingToSelection()
        {
            if (_editor?.Text == null) return; // если текст пустой или Editor - null ничего не делаем, т.к. Editor не позволяет
                                               // использовать различные стили в одном документе, также как и блокнот
                                               // в Windows - форматирование применяется ко всему документу

            UpdateEditorProperties(); // обновляем свойства текста
        }

        // Методы для применения форматирования
        public void ApplyBold()
        {
            IsBold = !IsBold;
        }

        public void ApplyItalic()
        {
            IsItalic = !IsItalic;
        }


        public void ApplyAlignment(TextAlignment alignment)
        {
            TextAlignment = alignment;
        }

        public void Focus()
        {
            _editor?.Focus();
        }

        public void Unfocus()
        {
            _editor?.Unfocus();
        }
    }

    // Вспомогательный класс для хранения информации о форматировании
    public class FormattingInfo
    {
        public bool IsBold { get; set; }
        public bool IsItalic { get; set; }
        public double FontSize { get; set; } = 14.0;
        public string FontFamily { get; set; } = "Arial";
        public TextAlignment TextAlignment { get; set; } = TextAlignment.Start;
    }
}