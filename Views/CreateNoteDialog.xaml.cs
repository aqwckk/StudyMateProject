using Microsoft.Maui.Controls;
using StudyMateTest.Models;

namespace StudyMateTest.Views
{
    public partial class CreateNoteDialog : ContentPage
    {
        public CreateNoteDialog()
        {
            InitializeComponent();
            SetupPreviewUpdates();
            UpdatePreview();
        }

        protected override bool OnBackButtonPressed()
        {
            try
            {
                MainThread.BeginInvokeOnMainThread(() => OnCancelClicked(null, null));
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in OnBackButtonPressed: {ex.Message}");
                return false;
            }
        }

        private void SetupPreviewUpdates()
        {
            try
            {
                TitleEntry.TextChanged += (s, e) => UpdatePreview();
                DescriptionEditor.TextChanged += (s, e) => UpdatePreview();

                UpdatePreview();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting up preview updates: {ex.Message}");
            }
        }

        private void UpdatePreview()
        {
            try
            {
                var title = string.IsNullOrWhiteSpace(TitleEntry?.Text) ? "Новая заметка" : TitleEntry.Text;
                var description = string.IsNullOrWhiteSpace(DescriptionEditor?.Text) ? "Без описания" : DescriptionEditor.Text;

                var preview = $"📝 {title}\n📋 {description}\n🕐 Создано: {DateTime.Now:dd.MM.yyyy HH:mm}";

                if (PreviewLabel != null)
                {
                    PreviewLabel.Text = preview;
                }

                if (CreateButton != null)
                {
                    CreateButton.IsEnabled = !string.IsNullOrWhiteSpace(TitleEntry?.Text);
                    CreateButton.BackgroundColor = CreateButton.IsEnabled ? Color.FromArgb("#10B981") : Color.FromArgb("#9CA3AF");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating preview: {ex.Message}");
                if (PreviewLabel != null)
                {
                    PreviewLabel.Text = "Ошибка предпросмотра";
                }
            }
        }

        private void OnTemplateClicked(object sender, EventArgs e)
        {
            try
            {
                if (sender is Button button)
                {
                    var templateText = button.Text;
                    System.Diagnostics.Debug.WriteLine($"Template clicked: {templateText}");

                    if (templateText.Contains("Лекция"))
                    {
                        TitleEntry.Text = "Лекция";
                        DescriptionEditor.Text = "Конспект лекции";
                        System.Diagnostics.Debug.WriteLine("Applied Lecture template");
                    }
                    else if (templateText.Contains("Заметки"))
                    {
                        TitleEntry.Text = "Заметки";
                        DescriptionEditor.Text = "Важные заметки и идеи";
                        System.Diagnostics.Debug.WriteLine("Applied Notes template");
                    }
                    else if (templateText.Contains("Расчеты"))
                    {
                        TitleEntry.Text = "Расчеты";
                        DescriptionEditor.Text = "Математические расчеты и формулы";
                        System.Diagnostics.Debug.WriteLine("Applied Calculations template");
                    }
                    else if (templateText.Contains("Идеи"))
                    {
                        TitleEntry.Text = "Идеи";
                        DescriptionEditor.Text = "Креативные идеи и мысли";
                        System.Diagnostics.Debug.WriteLine("Applied Ideas template");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Unknown template: {templateText}");
                    }

                    UpdatePreview();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying template: {ex.Message}");
            }
        }

        private async void OnCreateNoteClicked(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== OnCreateNoteClicked START ===");

                if (string.IsNullOrWhiteSpace(TitleEntry.Text))
                {
                    await DisplayAlert("Ошибка", "Название заметки обязательно", "OK");
                    return;
                }

                CreateButton.Text = "⏳ Создаем...";
                CreateButton.IsEnabled = false;

                var note = new Note
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = TitleEntry.Text.Trim(),
                    Description = DescriptionEditor.Text?.Trim() ?? "",
                    TextContent = "",
                    GraphicsData = null,
                    CreatedAt = DateTime.Now,
                    LastModified = DateTime.Now,
                    IsModified = true
                };

                System.Diagnostics.Debug.WriteLine($"Note created with ID: {note.Id}");
                System.Diagnostics.Debug.WriteLine($"Note title: {note.Title}");
                System.Diagnostics.Debug.WriteLine($"Note description: {note.Description}");

                System.Diagnostics.Debug.WriteLine("Sending MessagingCenter message...");
                MessagingCenter.Send(this, "NoteCreated", note);
                System.Diagnostics.Debug.WriteLine("MessagingCenter message sent");

                await Task.Delay(100);

                System.Diagnostics.Debug.WriteLine("Closing modal...");
                await Navigation.PopAsync();

                System.Diagnostics.Debug.WriteLine("=== OnCreateNoteClicked END ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in OnCreateNoteClicked: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                await DisplayAlert("Ошибка", $"Не удалось создать заметку: {ex.Message}", "OK");
            }
            finally
            {
                CreateButton.Text = "✅ Создать заметку";
                CreateButton.IsEnabled = true;
            }
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(TitleEntry.Text) || !string.IsNullOrWhiteSpace(DescriptionEditor.Text))
                {
                    bool confirm = await DisplayAlert(
                        "Подтверждение",
                        "У вас есть несохраненные изменения. Вы уверены что хотите выйти?",
                        "Да, выйти", "Остаться");
                    if (!confirm)
                        return;
                }

                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error canceling: {ex.Message}");
                await Navigation.PopAsync();
            }
        }
    }
}