using StudyMateTest.Models.TextEditor;
using StudyMateTest.Services;
using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyMateTest.Services.TextEditorServices
{
    public class TextEditorService : ITextEditorService
    {
        private TextDocument _currentDocument;
        private TextFormatting _currentFormatting;

        public TextDocument CurrentDocument 
        {
            get => _currentDocument;
            private set 
            {
                _currentDocument = value;
                OnDocumentChanged();
            }
        }

        public TextFormatting CurrentFormatting 
        {
            get => _currentFormatting;
            private set 
            {
                _currentFormatting = value;
                OnFormattingChanged();
            }
        }

        public event EventHandler DocumentChanged;
        public event EventHandler FormattingChanged;

        public TextEditorService() 
        {
            _currentDocument = new TextDocument();
            _currentFormatting = new TextFormatting();
        }

        public void NewDocument() 
        {
            CurrentDocument = new TextDocument();
        }

        public async Task<bool> OpenDocumentAsync() 
        {
            try
            {
                FileResult result = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = "Выберите текстовый файл",
                    FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>> 
                    {
                        { DevicePlatform.iOS, new[]{ "public.text", "public.rtf"} },
                        { DevicePlatform.Android, new[]{ "text/*", "application/rtf"} },
                        { DevicePlatform.WinUI, new[] { ".txt", ".rtf"} },
                        { DevicePlatform.macOS, new[] { "txt", "rtf"} }
                    })
                });

                if (result != null) 
                {
                    using Stream stream = await result.OpenReadAsync();
                    using StreamReader reader = new StreamReader(stream);
                    string content = await reader.ReadToEndAsync();

                    TextDocument newDocument = new TextDocument()
                    {
                        Content = content,
                        Title = Path.GetFileNameWithoutExtension(result.FileName)
                    };

                    newDocument.MarkAsSaved();
                    CurrentDocument = newDocument;
                    return true;
                }
            }
            catch (Exception exception)
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", $"Не удалось открыть файл: {exception.Message}", "OK");
            }
            return false;
        }

        public async Task<bool> SaveDocumentAsync() 
        {
            try
            {
                string fileName = $"{CurrentDocument.Title}.txt";
                string filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);

                await File.WriteAllTextAsync(filePath, CurrentDocument.Content, Encoding.UTF8);
                CurrentDocument.MarkAsSaved();

                await Application.Current.MainPage.DisplayAlert("Успешно", $"Документ сохранен: {fileName}", "OK");
                return true;
            }
            catch (Exception exception) 
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", $"Не удалось открыть файл: {exception.Message}", "OK");
                return false;
            }   
        }

        public async Task<bool> SaveDocumentAsAsync() 
        {
            try
            {
                string newTitle = await Application.Current.MainPage.DisplayPromptAsync("Сохранить как", "Введите название файла:", initialValue: CurrentDocument.Title);

                if (!string.IsNullOrWhiteSpace(newTitle))
                {
                    CurrentDocument.Title = newTitle;
                    return await SaveDocumentAsync();
                }
            }
            catch (Exception exception)
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка",
                    $"Не удалось сохранить файл: {exception.Message}", "OK");
            }
            return false;
        }

        public void ApplyFormatting(TextFormatting formatting) 
        {
            CurrentFormatting = formatting;
        }

        public void SetBold(bool isBold) 
        {
            CurrentFormatting.IsBold = isBold;
            OnFormattingChanged();
        }

        public void SetItalic(bool isItalic) 
        {
            CurrentFormatting.IsItalic = isItalic;
            OnFormattingChanged();
        }

        public void SetFontSize(double fontSize) 
        {
            CurrentFormatting.FontSize = Math.Max(8, Math.Min(72, fontSize));
            OnFormattingChanged();
        }

        public void SetFontFamily(string fontFamily) 
        {
            CurrentFormatting.FontFamily = fontFamily;
            OnFormattingChanged();
        }

        public void SetTextColor(string color) 
        {
            CurrentFormatting.TextColor = color;
            OnFormattingChanged();
        }

        public void SetAlignment(Models.TextEditor.TextAlignment alignment) 
        {
            CurrentFormatting.Alignment = alignment;
            OnFormattingChanged();
        }

        public void IncreaseIndent() 
        {
            CurrentFormatting.Indent += 20;
            OnFormattingChanged();
        }

        public void DecreaseIndent()
        {
            CurrentFormatting.Indent = Math.Max(0, CurrentFormatting.Indent - 20);
            OnFormattingChanged();
        }

        protected virtual void OnDocumentChanged() 
        {
            DocumentChanged?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnFormattingChanged() 
        {
            FormattingChanged?.Invoke(this, EventArgs.Empty);
        }

        public async Task<bool> SaveAsRtfAsync()
        {
            try
            {
                var fileName = $"{CurrentDocument.Title}.rtf";
                var filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);

                var rtfContent = ConvertHtmlToRtf(CurrentDocument.Content);
                await File.WriteAllTextAsync(filePath, rtfContent, Encoding.UTF8);

                CurrentDocument.MarkAsSaved();
                await Application.Current.MainPage.DisplayAlert("Успешно",
                    $"Документ сохранен как RTF: {fileName}", "OK");
                return true;
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка",
                    $"Не удалось сохранить RTF файл: {ex.Message}", "OK");
                return false;
            }
        }

        private string ConvertHtmlToRtf(string htmlContent)
        {
            var rtf = new StringBuilder();
            rtf.AppendLine(@"{\rtf1\ansi\deff0 {\fonttbl {\f0 Times New Roman;}}");

            var text = htmlContent
                .Replace("<b>", @"\b ")
                .Replace("</b>", @"\b0 ")
                .Replace("<i>", @"\i ")
                .Replace("</i>", @"\i0 ")
                .Replace("<u>", @"\ul ")
                .Replace("</u>", @"\ulnone ")
                .Replace("<br>", @"\par ")
                .Replace("<p>", "")
                .Replace("</p>", @"\par ")
                .Replace("&nbsp;", " ");

            text = System.Text.RegularExpressions.Regex.Replace(text, "<[^>]+>", "");

            rtf.AppendLine(@"\f0\fs24 " + text);
            rtf.AppendLine("}");

            return rtf.ToString();
        }

        public string GetPlainText()
        {
            if (string.IsNullOrEmpty(CurrentDocument.Content))
                return "";

            return System.Text.RegularExpressions.Regex.Replace(
                CurrentDocument.Content, "<[^>]+>", "")
                .Replace("&nbsp;", " ")
                .Replace("&lt;", "<")
                .Replace("&gt;", ">")
                .Replace("&amp;", "&");
        }

        public int GetWordCount()
        {
            var plainText = GetPlainText();
            if (string.IsNullOrWhiteSpace(plainText))
                return 0;

            return plainText.Split(new char[] { ' ', '\t', '\n', '\r' },
                StringSplitOptions.RemoveEmptyEntries).Length;
        }

        public int GetCharacterCount()
        {
            var plainText = GetPlainText();
            return plainText?.Length ?? 0;
        }
    }
}
