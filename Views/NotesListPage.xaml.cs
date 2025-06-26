using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using StudyMateTest.Models;
using StudyMateTest.Services;

namespace StudyMateTest.Views
{
    public partial class NotesListPage : ContentPage
    {
        private readonly INoteService _noteService;
        private readonly ObservableCollection<Note> _notes;

        public NotesListPage()
        {
            InitializeComponent();
            _notes = new ObservableCollection<Note>();
            _noteService = GetNoteService();
            BindingContext = this;
            NotesCollectionView.ItemsSource = _notes;

            System.Diagnostics.Debug.WriteLine("=== NotesListPage Constructor START ===");

            MessagingCenter.Subscribe<CreateNoteDialog, Note>(this, "NoteCreated", OnNoteCreated);
            MessagingCenter.Subscribe<CombinedEditorPage, Note>(this, "NoteSaved", OnNoteSaved);

            System.Diagnostics.Debug.WriteLine("MessagingCenter subscriptions set up");

            _ = Task.Run(LoadNotes);
            UpdateNotesCount();

            System.Diagnostics.Debug.WriteLine("=== NotesListPage Constructor END ===");
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            System.Diagnostics.Debug.WriteLine("=== NotesListPage OnAppearing ===");

            MessagingCenter.Unsubscribe<CreateNoteDialog, Note>(this, "NoteCreated");
            MessagingCenter.Unsubscribe<CombinedEditorPage, Note>(this, "NoteSaved");

            MessagingCenter.Subscribe<CreateNoteDialog, Note>(this, "NoteCreated", OnNoteCreated);
            MessagingCenter.Subscribe<CombinedEditorPage, Note>(this, "NoteSaved", OnNoteSaved);

            System.Diagnostics.Debug.WriteLine("MessagingCenter re-subscribed on page appearing");
        }

        private INoteService GetNoteService()
        {
            try
            {
                if (Application.Current?.MainPage?.Handler?.MauiContext?.Services != null)
                {
                    var service = Application.Current.MainPage.Handler.MauiContext.Services.GetService<INoteService>();
                    if (service != null)
                    {
                        System.Diagnostics.Debug.WriteLine("Got NoteService from DI");
                        return service;
                    }
                }

                System.Diagnostics.Debug.WriteLine("Creating NoteService directly");
                return new NoteService();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting NoteService: {ex.Message}");
                return new NoteService();
            }
        }

        private async void OnNoteCreated(CreateNoteDialog sender, Note note)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== OnNoteCreated RECEIVED ===");
                System.Diagnostics.Debug.WriteLine($"Received new note: {note.Title}");
                System.Diagnostics.Debug.WriteLine($"Note ID: {note.Id}");
                System.Diagnostics.Debug.WriteLine($"Current notes count before add: {_notes?.Count ?? 0}");

                await _noteService.SaveNoteAsync(note);
                System.Diagnostics.Debug.WriteLine($"Note saved to storage");

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    try
                    {
                        if (_notes != null)
                        {
                            _notes.Add(note);
                            System.Diagnostics.Debug.WriteLine($"Added note to collection. New count: {_notes.Count}");
                            UpdateNotesCount();
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("ERROR: _notes collection is null!");
                        }
                    }
                    catch (Exception innerEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"ERROR adding note to collection: {innerEx.Message}");
                        System.Diagnostics.Debug.WriteLine($"Stack trace: {innerEx.StackTrace}");
                    }
                });

                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await DisplayAlert("Успех", $"Заметка '{note.Title}' создана!", "OK");
                });

                System.Diagnostics.Debug.WriteLine($"=== OnNoteCreated COMPLETED ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in OnNoteCreated: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");

                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await DisplayAlert("Ошибка", $"Не удалось сохранить заметку: {ex.Message}", "OK");
                });
            }
        }

        private async void OnNoteSaved(CombinedEditorPage sender, Note note)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Received saved note: {note.Title}");

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    var existingIndex = _notes.ToList().FindIndex(n => n.Id == note.Id);
                    if (existingIndex >= 0)
                    {
                        _notes[existingIndex] = note;
                    }
                    else
                    {
                        _notes.Add(note);
                    }
                    UpdateNotesCount();
                });

                await _noteService.SaveNoteAsync(note);
                System.Diagnostics.Debug.WriteLine($"Note updated and saved: {note.Title}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating note: {ex.Message}");
            }
        }

        private async void OnCreateNoteClicked(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== Create note button clicked ===");

                var createDialog = new CreateNoteDialog();

                await Navigation.PushAsync(createDialog);
                System.Diagnostics.Debug.WriteLine("CreateNoteDialog pushed successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in OnCreateNoteClicked: {ex.Message}");
                await DisplayAlert("Ошибка", $"Произошла ошибка: {ex.Message}", "OK");
            }
        }

        private async void OnOpenNoteClicked(object sender, EventArgs e)
        {
            try
            {
                if (sender is Button button && button.BindingContext is Note note)
                {
                    System.Diagnostics.Debug.WriteLine($"Opening note: {note.Title}");

                    var editorPage = new CombinedEditorPage();
                    editorPage.LoadNote(note);
                    await Navigation.PushAsync(editorPage);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error opening note: {ex.Message}");
                await DisplayAlert("Ошибка", $"Ошибка открытия заметки: {ex.Message}", "OK");
            }
        }

        private async void OnExportNoteClicked(object sender, EventArgs e)
        {
            try
            {
                if (sender is Button button && button.BindingContext is Note note)
                {
                    System.Diagnostics.Debug.WriteLine($"Exporting note: {note.Title}");

                    var action = await DisplayActionSheet(
                        "Выберите формат экспорта",
                        "Отмена",
                        null,
                        "JSON (полная заметка)",
                        "PNG (только рисунок)",
                        "TXT (только текст)");

                    switch (action)
                    {
                        case "JSON (полная заметка)":
                            await _noteService.ExportNoteToJsonAsync(note);
                            break;
                        case "PNG (только рисунок)":
                            await _noteService.ExportNoteToPngAsync(note);
                            break;
                        case "TXT (только текст)":
                            await _noteService.ExportNoteToTxtAsync(note);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error exporting note: {ex.Message}");
                await DisplayAlert("Ошибка", $"Ошибка экспорта: {ex.Message}", "OK");
            }
        }

        private async void OnDuplicateNoteClicked(object sender, EventArgs e)
        {
            try
            {
                if (sender is Button button && button.BindingContext is Note originalNote)
                {
                    System.Diagnostics.Debug.WriteLine($"Duplicating note: {originalNote.Title}");

                    var duplicatedNote = originalNote.CreateCopy();

                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        _notes.Add(duplicatedNote);
                        UpdateNotesCount();
                    });

                    await _noteService.SaveNoteAsync(duplicatedNote);

                    await DisplayAlert("Успешно", $"Создана копия заметки: {duplicatedNote.Title}", "OK");
                    System.Diagnostics.Debug.WriteLine($"Note duplicated: {duplicatedNote.Title}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error duplicating note: {ex.Message}");
                await DisplayAlert("Ошибка", $"Ошибка копирования: {ex.Message}", "OK");
            }
        }

        private async void OnDeleteNoteClicked(object sender, EventArgs e)
        {
            try
            {
                if (sender is Button button && button.BindingContext is Note note)
                {
                    System.Diagnostics.Debug.WriteLine($"Deleting note: {note.Title}");

                    bool confirm = await DisplayAlert(
                        "Подтверждение",
                        $"Удалить заметку '{note.DisplayTitle}'?",
                        "Да", "Нет");

                    if (confirm)
                    {
                        await _noteService.DeleteNoteAsync(note.Id);

                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            _notes.Remove(note);
                            UpdateNotesCount();
                        });

                        await DisplayAlert("Успешно", "Заметка удалена", "OK");
                        System.Diagnostics.Debug.WriteLine($"Note deleted. Remaining: {_notes.Count}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting note: {ex.Message}");
                await DisplayAlert("Ошибка", $"Ошибка удаления: {ex.Message}", "OK");
            }
        }

        private async void OnRefreshClicked(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Refresh clicked - reloading from storage");
                await LoadNotes();
                await DisplayAlert("Обновлено", $"Загружено {_notes.Count} заметок", "OK");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error refreshing: {ex.Message}");
                await DisplayAlert("Ошибка", $"Ошибка обновления: {ex.Message}", "OK");
            }
        }

        private async void OnImportClicked(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Import clicked");

                var importedNote = await _noteService.ImportNoteFromJsonAsync();
                if (importedNote != null)
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        _notes.Add(importedNote);
                        UpdateNotesCount();
                    });

                    await _noteService.SaveNoteAsync(importedNote);
                    await DisplayAlert("Успешно", $"Заметка импортирована: {importedNote.Title}", "OK");
                    System.Diagnostics.Debug.WriteLine($"Note imported: {importedNote.Title}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error importing: {ex.Message}");
                await DisplayAlert("Ошибка", $"Ошибка импорта: {ex.Message}", "OK");
            }
        }

        private async void OnClearAllClicked(object sender, EventArgs e)
        {
            try
            {
                if (_notes.Count == 0)
                {
                    await DisplayAlert("Информация", "Нет заметок для удаления", "OK");
                    return;
                }

                bool confirm = await DisplayAlert(
                    "Подтверждение",
                    $"Удалить все {_notes.Count} заметок? Это действие нельзя отменить!",
                    "Да", "Нет");

                if (confirm)
                {
                    await _noteService.DeleteAllNotesAsync();

                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        _notes.Clear();
                        UpdateNotesCount();
                    });

                    await DisplayAlert("Успешно", "Все заметки удалены", "OK");
                    System.Diagnostics.Debug.WriteLine("All notes cleared");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error clearing all: {ex.Message}");
                await DisplayAlert("Ошибка", $"Ошибка очистки: {ex.Message}", "OK");
            }
        }

        private async Task LoadNotes()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== LoadNotes called ===");

                var savedNotes = await _noteService.LoadNotesAsync();
                System.Diagnostics.Debug.WriteLine($"Loaded {savedNotes.Count} notes from service");

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    _notes.Clear();
                    foreach (var note in savedNotes.OrderByDescending(n => n.LastModified))
                    {
                        _notes.Add(note);
                    }
                    UpdateNotesCount();
                });

                System.Diagnostics.Debug.WriteLine($"Notes loaded into UI. Total: {_notes.Count}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR loading notes: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        private void UpdateNotesCount()
        {
            try
            {
                var count = _notes.Count;
                var countText = count switch
                {
                    0 => "Нет заметок",
                    1 => "1 заметка",
                    2 or 3 or 4 => $"{count} заметки",
                    _ => $"{count} заметок"
                };

                NotesCountLabel.Text = countText;

                bool isEmpty = count == 0;
                EmptyStateStack.IsVisible = isEmpty;
                NotesCollectionView.IsVisible = !isEmpty;

                System.Diagnostics.Debug.WriteLine($"Updated notes count: {count}, isEmpty: {isEmpty}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating notes count: {ex.Message}");
                NotesCountLabel.Text = "Ошибка подсчета";
            }
        }

        public ObservableCollection<Note> Notes => _notes;

        protected override void OnDisappearing()
        {
            System.Diagnostics.Debug.WriteLine("=== NotesListPage OnDisappearing ===");
            base.OnDisappearing();
        }

        ~NotesListPage()
        {
            MessagingCenter.Unsubscribe<CreateNoteDialog, Note>(this, "NoteCreated");
            MessagingCenter.Unsubscribe<CombinedEditorPage, Note>(this, "NoteSaved");
        }
    }
}