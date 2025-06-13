using Microsoft.Maui.Controls;
using StudyMateTest.Models;
using StudyMateTest.Services.NotificationServices;

namespace StudyMateTest.Views
{
    public partial class AddReminderPage : ContentPage
    {
        private readonly INotificationService _notificationService;

        public AddReminderPage()
        {
            InitializeComponent();

            // Получаем сервис уведомлений
            _notificationService = GetNotificationService();

            // Инициализируем значения по умолчанию
            InitializeDefaults();

            // Подписываемся на изменения для предпросмотра
            SetupPreviewUpdates();
        }

        private INotificationService GetNotificationService()
        {
            try
            {
                if (Application.Current?.MainPage?.Handler?.MauiContext?.Services != null)
                {
                    return Application.Current.MainPage.Handler.MauiContext.Services.GetService<INotificationService>();
                }

                return new StudyMateTest.Services.NotificationServices.DefaultNotificationService();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting NotificationService: {ex.Message}");
                return new StudyMateTest.Services.NotificationServices.DefaultNotificationService();
            }
        }

        private void InitializeDefaults()
        {
            try
            {
                // Устанавливаем дату на сегодня
                ReminderDatePicker.Date = DateTime.Today;

                // Устанавливаем время на текущее + 1 час (округленное)
                var currentTime = DateTime.Now.AddHours(1);
                ReminderTimePicker.Time = new TimeSpan(currentTime.Hour, 0, 0);

                System.Diagnostics.Debug.WriteLine("AddReminderPage initialized with defaults");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing defaults: {ex.Message}");
            }
        }

        private void SetupPreviewUpdates()
        {
            try
            {
                // Подписываемся на изменения полей для обновления предпросмотра
                TitleEntry.TextChanged += (s, e) => UpdatePreview();
                MessageEditor.TextChanged += (s, e) => UpdatePreview();
                ReminderDatePicker.DateSelected += (s, e) => UpdatePreview();
                ReminderTimePicker.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(TimePicker.Time))
                        UpdatePreview();
                };

                // Обновляем предпросмотр сразу
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
                var title = string.IsNullOrWhiteSpace(TitleEntry.Text) ? "Без названия" : TitleEntry.Text;
                var scheduledDateTime = ReminderDatePicker.Date.Add(ReminderTimePicker.Time);

                var timeFromNow = scheduledDateTime - DateTime.Now;
                string timeDescription;

                if (timeFromNow.TotalMinutes < 0)
                {
                    timeDescription = "⚠️ Время в прошлом!";
                }
                else if (timeFromNow.TotalMinutes < 60)
                {
                    timeDescription = $"через {Math.Round(timeFromNow.TotalMinutes)} мин";
                }
                else if (timeFromNow.TotalHours < 24)
                {
                    timeDescription = $"через {Math.Round(timeFromNow.TotalHours, 1)} ч";
                }
                else
                {
                    timeDescription = $"через {Math.Round(timeFromNow.TotalDays, 1)} дн";
                }

                PreviewLabel.Text = $"'{title}' - {scheduledDateTime:dd.MM.yyyy HH:mm} ({timeDescription})";

                // Обновляем доступность кнопки создания
                CreateButton.IsEnabled = !string.IsNullOrWhiteSpace(TitleEntry.Text) && timeFromNow.TotalMinutes > 0;
                CreateButton.BackgroundColor = CreateButton.IsEnabled ? Color.FromArgb("#10B981") : Color.FromArgb("#9CA3AF");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating preview: {ex.Message}");
                PreviewLabel.Text = "Ошибка предпросмотра";
            }
        }

        private async void OnQuickTime5MinClicked(object sender, EventArgs e)
        {
            try
            {
                var quickTime = DateTime.Now.AddMinutes(5);
                ReminderDatePicker.Date = quickTime.Date;
                ReminderTimePicker.Time = quickTime.TimeOfDay;

                await DisplayAlert("Время установлено", "Напоминание через 5 минут", "OK");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting quick time 5min: {ex.Message}");
            }
        }

        private async void OnQuickTime1HourClicked(object sender, EventArgs e)
        {
            try
            {
                var quickTime = DateTime.Now.AddHours(1);
                ReminderDatePicker.Date = quickTime.Date;
                ReminderTimePicker.Time = quickTime.TimeOfDay;

                await DisplayAlert("Время установлено", "Напоминание через 1 час", "OK");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting quick time 1hour: {ex.Message}");
            }
        }

        private async void OnQuickTimeTomorrowClicked(object sender, EventArgs e)
        {
            try
            {
                var quickTime = DateTime.Now.AddDays(1);
                ReminderDatePicker.Date = quickTime.Date;
                // Оставляем текущее время

                await DisplayAlert("Время установлено", "Напоминание завтра в это время", "OK");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting quick time tomorrow: {ex.Message}");
            }
        }

        private async void OnCreateReminderClicked(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Create reminder button clicked");

                // Валидация
                if (string.IsNullOrWhiteSpace(TitleEntry.Text))
                {
                    await DisplayAlert("Ошибка", "Название напоминания обязательно", "OK");
                    return;
                }

                var scheduledDateTime = ReminderDatePicker.Date.Add(ReminderTimePicker.Time);

                if (scheduledDateTime <= DateTime.Now)
                {
                    await DisplayAlert("Ошибка", "Время напоминания должно быть в будущем", "OK");
                    return;
                }

                // Проверяем разрешения
                if (!await _notificationService.IsPermissionGranted())
                {
                    var permissionGranted = await _notificationService.RequestPermission();
                    if (!permissionGranted)
                    {
                        await DisplayAlert("Ошибка", "Разрешение на уведомления не предоставлено", "OK");
                        return;
                    }
                }

                // Показываем индикатор загрузки
                CreateButton.Text = "⏳ Создаем...";
                CreateButton.IsEnabled = false;

                // Создаем напоминание (БЕЗ метаданных приоритета и категории)
                var reminder = new Reminder
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = TitleEntry.Text.Trim(),
                    Message = MessageEditor.Text?.Trim() ?? "",
                    ScheduledTime = scheduledDateTime,
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    Metadata = new Dictionary<string, string>()
                };

                // Планируем уведомление
                var notificationId = await _notificationService.ScheduleNotification(
                    reminder.Title,
                    string.IsNullOrWhiteSpace(reminder.Message) ? $"Напоминание на {scheduledDateTime:HH:mm}" : reminder.Message,
                    reminder.ScheduledTime,
                    reminder.Metadata
                );

                reminder.Metadata["NotificationId"] = notificationId;

                System.Diagnostics.Debug.WriteLine($"Reminder created: {reminder.Title} for {reminder.ScheduledTime}");

                // Возвращаем результат на предыдущую страницу
                if (Navigation.NavigationStack.Count > 1)
                {
                    MessagingCenter.Send(this, "ReminderCreated", reminder);
                    await Navigation.PopAsync();
                }

                await DisplayAlert("Успех", $"Напоминание '{reminder.Title}' создано!\nВремя: {scheduledDateTime:HH:mm dd.MM.yyyy}", "OK");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating reminder: {ex.Message}");
                await DisplayAlert("Ошибка", $"Не удалось создать напоминание: {ex.Message}", "OK");
            }
            finally
            {
                // Восстанавливаем кнопку
                CreateButton.Text = "✅ Создать напоминание";
                CreateButton.IsEnabled = true;
            }
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            try
            {
                // Проверяем есть ли несохраненные изменения
                if (!string.IsNullOrWhiteSpace(TitleEntry.Text) || !string.IsNullOrWhiteSpace(MessageEditor.Text))
                {
                    bool confirm = await DisplayAlert("Подтверждение", "У вас есть несохраненные изменения. Вы уверены что хотите выйти?", "Да, выйти", "Остаться");
                    if (!confirm)
                        return;
                }

                if (Navigation.NavigationStack.Count > 1)
                {
                    await Navigation.PopAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error canceling: {ex.Message}");
            }
        }
    }
}