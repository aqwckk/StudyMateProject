using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StudyMateTest.Models.TextEditor;


namespace StudyMateTest.Services.TextEditorServices
{
    public interface ITextEditorService
    {
        TextDocument CurrentDocument { get; }
        TextFormatting CurrentFormatting { get; }

        void NewDocument();
        Task<bool> OpenDocumentAsync();
        Task<bool> SaveDocumentAsync();
        Task<bool> SaveDocumentAsAsync();

        void ApplyFormatting(TextFormatting formatting);
        void SetBold(bool isBold);
        void SetItalic(bool isItalic);
        void SetFontSize(double fontSize);
        void SetFontFamily(string fontFamily);
        void SetTextColor(string color);
        void SetAlignment(Models.TextEditor.TextAlignment alignment);
        void CreateList(ListType listType);
        void IncreaseIndent();
        void DecreaseIndent();


        event EventHandler DocumentChanged;
        event EventHandler FormattingChanged;
    }
}
