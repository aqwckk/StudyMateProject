using Microsoft.Maui.Controls;
using System.ComponentModel;

namespace StudyMateTest.Controls
{
    public class RichTextEditor : ContentView
    {
        public static readonly BindableProperty TextProperty = BindableProperty.Create(
            nameof(Text),
            typeof(string),
            typeof(RichTextEditor),
            string.Empty,
            BindingMode.TwoWay,
            propertyChanged: OnTextPropertyChanged);

        public static readonly BindableProperty FontSizeProperty = BindableProperty.Create(
            nameof(FontSize),
            typeof(double),
            typeof(RichTextEditor),
            14.0,
            propertyChanged: OnFontSizePropertyChanged);

        public static readonly BindableProperty FontFamilyProperty = BindableProperty.Create(
            nameof(FontFamily),
            typeof(string),
            typeof(RichTextEditor),
            "Arial",
            propertyChanged: OnFontFamilyPropertyChanged);

        public static readonly BindableProperty IsBoldProperty = BindableProperty.Create(
            nameof(IsBold),
            typeof(bool),
            typeof(RichTextEditor),
            false,
            BindingMode.TwoWay,
            propertyChanged: OnFormattingPropertyChanged);

        public static readonly BindableProperty IsItalicProperty = BindableProperty.Create(
            nameof(IsItalic),
            typeof(bool),
            typeof(RichTextEditor),
            false,
            BindingMode.TwoWay,
            propertyChanged: OnFormattingPropertyChanged);

        public static readonly BindableProperty TextAlignmentProperty = BindableProperty.Create(
            nameof(TextAlignment),
            typeof(TextAlignment),
            typeof(RichTextEditor),
            TextAlignment.Start,
            BindingMode.TwoWay,
            propertyChanged: OnFormattingPropertyChanged);

        private Editor _editor;
        private ScrollView _scrollView;
        private FormattingInfo _currentFormatting = new FormattingInfo();

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public double FontSize
        {
            get => (double)GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

        public string FontFamily
        {
            get => (string)GetValue(FontFamilyProperty);
            set => SetValue(FontFamilyProperty, value);
        }

        public bool IsBold
        {
            get => (bool)GetValue(IsBoldProperty);
            set => SetValue(IsBoldProperty, value);
        }

        public bool IsItalic
        {
            get => (bool)GetValue(IsItalicProperty);
            set => SetValue(IsItalicProperty, value);
        }

        public TextAlignment TextAlignment
        {
            get => (TextAlignment)GetValue(TextAlignmentProperty);
            set => SetValue(TextAlignmentProperty, value);
        }

        public RichTextEditor()
        {
            CreateContent();
        }

        private void CreateContent()
        {
            _editor = new Editor
            {
                BackgroundColor = Colors.White,
                TextColor = Colors.Black,
                AutoSize = EditorAutoSizeOption.TextChanges,
                Placeholder = "Введите текст...",
                PlaceholderColor = Colors.Gray,
                MinimumHeightRequest = 300
            };

            _editor.TextChanged += OnEditorTextChanged;
            _editor.Focused += OnEditorFocused;
            _editor.Unfocused += OnEditorUnfocused;

            _scrollView = new ScrollView
            {
                Content = _editor
            };

            Content = _scrollView;
            UpdateEditorProperties();
        }

        private void OnEditorTextChanged(object sender, TextChangedEventArgs e)
        {
            SetValue(TextProperty, e.NewTextValue);
        }

        private void OnEditorFocused(object sender, FocusEventArgs e)
        {
            // При получении фокуса обновляем текущее форматирование
            UpdateCurrentFormattingFromCursor();
        }

        private void OnEditorUnfocused(object sender, FocusEventArgs e)
        {
            // При потере фокуса можно сохранить текущее состояние
        }

        private void UpdateCurrentFormattingFromCursor()
        {
            _currentFormatting.IsBold = IsBold;
            _currentFormatting.IsItalic = IsItalic;
            _currentFormatting.FontSize = FontSize;
            _currentFormatting.FontFamily = FontFamily;
            _currentFormatting.TextAlignment = TextAlignment;
        }

        private static void OnTextPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is RichTextEditor control && newValue is string text)
            {
                if (control._editor != null && control._editor.Text != text)
                {
                    control._editor.Text = text;
                }
            }
        }

        private static void OnFontSizePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is RichTextEditor control)
            {
                control._currentFormatting.FontSize = (double)newValue;
                control.ApplyFormattingToSelection();
            }
        }

        private static void OnFontFamilyPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is RichTextEditor control)
            {
                control._currentFormatting.FontFamily = (string)newValue;
                control.ApplyFormattingToSelection();
            }
        }

        private static void OnFormattingPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is RichTextEditor control)
            {
                control.UpdateCurrentFormattingFromCursor();
                control.ApplyFormattingToSelection();
            }
        }

        private void UpdateEditorProperties()
        {
            if (_editor != null)
            {
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

        private void ApplyFormattingToSelection()
        {
            if (_editor?.Text == null) return;

            UpdateEditorProperties();
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