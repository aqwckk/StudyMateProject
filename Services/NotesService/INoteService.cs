using StudyMateTest.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudyMateTest.Services
{
    public interface INoteService
    {
        Task<List<Note>> LoadNotesAsync();
        Task SaveNotesAsync(List<Note> notes);
        Task SaveNoteAsync(Note note);
        Task DeleteNoteAsync(string noteId);
        Task<Note> GetNoteByIdAsync(string noteId);
        Task<bool> ExportNoteToPngAsync(Note note, string fileName = null);
        Task<bool> ExportNoteToTxtAsync(Note note, string fileName = null);
        Task<bool> ExportNoteToJsonAsync(Note note, string fileName = null);
        Task<Note> ImportNoteFromJsonAsync();
        Task DeleteAllNotesAsync();
        Task<bool> CreateBackupAsync();
        Task<List<Note>> RestoreFromBackupAsync();
    }
}