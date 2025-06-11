using Microsoft.Maui.Controls;
using System.Text;

namespace StudyMateTest.Controls
{
    public class RichTextEditor : ContentView
    {
        private WebView _webView;
        private string _htmlContent = "";

        public static readonly BindableProperty TextProperty = BindableProperty.Create(
            nameof(Text),
            typeof(string),
            typeof(RichTextEditor),
            string.Empty,
            BindingMode.TwoWay,
            propertyChanged: OnTextChanged);

        public static readonly BindableProperty FontSizeProperty = BindableProperty.Create(
            nameof(FontSize),
            typeof(double),
            typeof(RichTextEditor),
            14.0,
            propertyChanged: OnFontSizeChanged);

        public static readonly BindableProperty FontFamilyProperty = BindableProperty.Create(
            nameof(FontFamily),
            typeof(string),
            typeof(RichTextEditor),
            "Arial",
            propertyChanged: OnFontFamilyChanged);

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

        public RichTextEditor()
        {
            CreateWebView();
            LoadEditor();
        }

        private void CreateWebView()
        {
            _webView = new WebView
            {
                BackgroundColor = Colors.White
            };
            Content = _webView;
        }

        private void LoadEditor()
        {
            var html = GenerateHtml();
            var htmlSource = new HtmlWebViewSource
            {
                Html = html
            };
            _webView.Source = htmlSource;
        }

        private string GenerateHtml()
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta charset='UTF-8'>");
            sb.AppendLine("<meta name='viewport' content='width=device-width, initial-scale=1.0'>");
            sb.AppendLine("<style>");
            sb.AppendLine("body { margin: 10px; font-family: Arial, sans-serif; }");
            sb.AppendLine("#editor { min-height: 300px; border: 1px solid #ccc; padding: 10px; outline: none; }");
            sb.AppendLine(".toolbar { margin-bottom: 10px; padding: 5px; border-bottom: 1px solid #ccc; }");
            sb.AppendLine(".toolbar button { margin-right: 5px; padding: 5px 10px; border: 1px solid #ccc; background: #f9f9f9; }");
            sb.AppendLine(".toolbar button:hover { background: #e9e9e9; }");
            sb.AppendLine(".toolbar button.active { background: #007acc; color: white; }");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");

            // Панель инструментов
            sb.AppendLine("<div class='toolbar'>");
            sb.AppendLine("<button onclick='execCmd(\"bold\")' title='Жирный'><b>B</b></button>");
            sb.AppendLine("<button onclick='execCmd(\"italic\")' title='Курсив'><i>I</i></button>");
            sb.AppendLine("<button onclick='execCmd(\"underline\")' title='Подчеркнутый'><u>U</u></button>");
            sb.AppendLine("<button onclick='execCmd(\"insertUnorderedList\")' title='Маркированный список'>•</button>");
            sb.AppendLine("<button onclick='execCmd(\"insertOrderedList\")' title='Нумерованный список'>1.</button>");
            sb.AppendLine("<button onclick='execCmd(\"justifyLeft\")' title='По левому краю'>◀</button>");
            sb.AppendLine("<button onclick='execCmd(\"justifyCenter\")' title='По центру'>■</button>");
            sb.AppendLine("<button onclick='execCmd(\"justifyRight\")' title='По правому краю'>▶</button>");
            sb.AppendLine("<button onclick='execCmd(\"indent\")' title='Увеличить отступ'>↱</button>");
            sb.AppendLine("<button onclick='execCmd(\"outdent\")' title='Уменьшить отступ'>↰</button>");

            // Селектор размера шрифта
            sb.AppendLine("<select onchange='changeFontSize(this.value)'>");
            sb.AppendLine("<option value='1'>8pt</option>");
            sb.AppendLine("<option value='2'>10pt</option>");
            sb.AppendLine("<option value='3' selected>12pt</option>");
            sb.AppendLine("<option value='4'>14pt</option>");
            sb.AppendLine("<option value='5'>18pt</option>");
            sb.AppendLine("<option value='6'>24pt</option>");
            sb.AppendLine("<option value='7'>36pt</option>");
            sb.AppendLine("</select>");

            // Селектор шрифта
            sb.AppendLine("<select onchange='changeFontFamily(this.value)' style='margin-left: 5px;'>");
            sb.AppendLine("<option value='Arial'>Arial</option>");
            sb.AppendLine("<option value='Times New Roman'>Times New Roman</option>");
            sb.AppendLine("<option value='Calibri'>Calibri</option>");
            sb.AppendLine("<option value='Verdana'>Verdana</option>");
            sb.AppendLine("<option value='Courier New'>Courier New</option>");
            sb.AppendLine("</select>");

            sb.AppendLine("</div>");

            // Область редактирования
            sb.AppendLine($"<div id='editor' contenteditable='true' style='font-family: {FontFamily}; font-size: {FontSize}px;'>");
            sb.AppendLine(Text?.Replace("\n", "<br>") ?? "");
            sb.AppendLine("</div>");

            // JavaScript для функциональности
            sb.AppendLine("<script>");
            sb.AppendLine("function execCmd(command, value = null) {");
            sb.AppendLine("    document.execCommand(command, false, value);");
            sb.AppendLine("    updateContent();");
            sb.AppendLine("}");

            sb.AppendLine("function changeFontSize(size) {");
            sb.AppendLine("    document.execCommand('fontSize', false, size);");
            sb.AppendLine("    updateContent();");
            sb.AppendLine("}");

            sb.AppendLine("function changeFontFamily(font) {");
            sb.AppendLine("    document.execCommand('fontName', false, font);");
            sb.AppendLine("    updateContent();");
            sb.AppendLine("}");

            sb.AppendLine("function updateContent() {");
            sb.AppendLine("    var content = document.getElementById('editor').innerHTML;");
            sb.AppendLine("    window.location.href = 'maui://content-changed/' + encodeURIComponent(content);");
            sb.AppendLine("}");

            sb.AppendLine("document.getElementById('editor').addEventListener('input', updateContent);");
            sb.AppendLine("document.getElementById('editor').addEventListener('blur', updateContent);");
            sb.AppendLine("</script>");

            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }

        private static void OnTextChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is RichTextEditor editor)
            {
                editor.UpdateEditorContent();
            }
        }

        private static void OnFontSizeChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is RichTextEditor editor)
            {
                editor.LoadEditor();
            }
        }

        private static void OnFontFamilyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is RichTextEditor editor)
            {
                editor.LoadEditor();
            }
        }

        private void UpdateEditorContent()
        {
            if (_webView != null)
            {
                LoadEditor();
            }
        }
    }
}
