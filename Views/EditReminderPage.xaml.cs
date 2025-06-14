using Microsoft.Maui.Controls;
using StudyMateTest.Models;

namespace StudyMateTest.Views
{
    public partial class EditReminderPage : ContentPage
    {
        private Reminder _originalReminder;
        private Reminder _editedReminder;

        public EditReminderPage(Reminder reminder)
        {
            InitializeComponent();
            _originalReminder = reminder;
            _editedReminder = new Reminder
            {
                Id = reminder.Id,
                Title = reminder.Title,
                Message = reminder.Message,
                ScheduledTime = reminder.ScheduledTime,
                IsActive = reminder.IsActive,
                CreatedAt = reminder.CreatedAt,
                Metadata = new Dictionary<string, string>(reminder.Metadata)
            };

            LoadReminderData();
            SetupPreviewUpdates();
            UpdatePreview();
        }

        private void LoadReminderData()
        {
            try
            {
                TitleEntry.Text = _editedReminder.Title;
                MessageEditor.Text = _editedReminder.Message;
                ReminderDatePicker.Date = _editedReminder.ScheduledTime.Date;
                ReminderTimePicker.Time = _editedReminder.ScheduledTime.TimeOfDay;

                System.Diagnostics.Debug.WriteLine($"Loaded reminder data: {_editedReminder.Title}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading reminder data: {ex.Message}");
            }
        }

        private void SetupPreviewUpdates()
        {
            try
            {
                TitleEntry.TextChanged += (s, e) => UpdatePreview();
                MessageEditor.TextChanged += (s, e) => UpdatePreview();
                ReminderDatePicker.DateSelected += (s, e) => UpdatePreview();
                ReminderTimePicker.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(TimePicker.Time))
                        UpdatePreview();
                };
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
                var title = string.IsNullOrWhiteSpace(TitleEntry?.Text) ? "Без названия" : TitleEntry.Text;
                var message = string.IsNullOrWhiteSpace(MessageEditor?.Text) ? "Без описания" : MessageEditor.Text;

                var selectedDate = ReminderDatePicker?.Date ?? DateTime.Today;
                var selectedTime = ReminderTimePicker?.Time ?? TimeSpan.Zero;
                var scheduledDateTime = selectedDate.Add(selectedTime);

                var timeText = scheduledDateTime.ToString("dd.MM.yyyy в HH:mm");

                var preview = $"📝 {title}\n📋 {message}\n⏰ {timeText}";

                if (PreviewLabel != null)
                {
                    PreviewLabel.Text = preview;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating preview: {ex.Message}");
            }
        }

        private void OnQuickTimeClicked(object sender, EventArgs e)
        {
            try
            {
                if (sender is Button button)
                {
                    var timeText = button.Text;
                    if (TimeSpan.TryParse(timeText, out var time))
                    {
                        ReminderTimePicker.Time = time;
                        System.Diagnostics.Debug.WriteLine($"Set quick time: {timeText}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting quick time: {ex.Message}");
            }
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            try
            {
                // Валидация
                if (string.IsNullOrWhiteSpace(TitleEntry.Text))
                {
                    await DisplayAlert("Ошибка", "Название напоминания не может быть пустым", "OK");
                    return;
                }

                var selectedDate = ReminderDatePicker.Date;
                var selectedTime = ReminderTimePicker.Time;
                var scheduledDateTime = selectedDate.Add(selectedTime);

                if (scheduledDateTime <= DateTime.Now)
                {
                    bool confirm = await DisplayAlert("Предупреждение",
                        "Выбранное время уже прошло. Напоминание будет неактивным. Продолжить?",
                        "Да", "Нет");
                    if (!confirm) return;
                }

                // Обновляем данные напоминания
                _editedReminder.Title = TitleEntry.Text.Trim();
                _editedReminder.Message = MessageEditor.Text?.Trim() ?? string.Empty;
                _editedReminder.ScheduledTime = scheduledDateTime;

                // Обновляем статус активности
                _editedReminder.IsActive = scheduledDateTime > DateTime.Now;

                System.Diagnostics.Debug.WriteLine($"Saving edited reminder: {_editedReminder.Title}");

                // Отправляем обновленное напоминание обратно
                MessagingCenter.Send(this, "ReminderEdited", _editedReminder);

                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving reminder: {ex.Message}");
                await DisplayAlert("Ошибка", $"Произошла ошибка при сохранении: {ex.Message}", "OK");
            }
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            try
            {
                bool hasChanges = HasChanges();

                if (hasChanges)
                {
                    bool confirm = await DisplayAlert("Подтверждение",
                        "У вас есть несохраненные изменения. Выйти без сохранения?",
                        "Да", "Нет");
                    if (!confirm) return;
                }

                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error canceling edit: {ex.Message}");
                await Navigation.PopAsync();
            }
        }

        private bool HasChanges()
        {
            try
            {
                var currentTitle = TitleEntry?.Text?.Trim() ?? string.Empty;
                var currentMessage = MessageEditor?.Text?.Trim() ?? string.Empty;
                var currentDate = ReminderDatePicker?.Date ?? DateTime.Today;
                var currentTime = ReminderTimePicker?.Time ?? TimeSpan.Zero;
                var currentDateTime = currentDate.Add(currentTime);

                return currentTitle != _originalReminder.Title ||
                       currentMessage != _originalReminder.Message ||
                       currentDateTime != _originalReminder.ScheduledTime;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking changes: {ex.Message}");
                return false;
            }
        }
    }
}