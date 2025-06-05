using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using StudyMateTest.Models;
using StudyMateTest.Services.NotificationServices;

namespace StudyMateTest.Views
{
    public partial class ReminderPage : ContentPage
    {
        private readonly INotificationService _notificationService;
        private readonly ObservableCollection<Reminder> _reminders;
        private readonly Dictionary<string, string> _notificationIds;

        public ReminderPage()
        {
            InitializeComponent();

            // Инициализация коллекций
            _reminders = new ObservableCollection<Reminder>();
            _notificationIds = new Dictionary<string, string>();

            // Получаем сервис уведомлений
            _notificationService = GetNotificationService();

            // Привязываем данные
            BindingContext = this;
            RemindersCollectionView.ItemsSource = _reminders;

            // Загружаем существующие напоминания
            _ = Task.Run(LoadReminders);

            // Обновляем счетчик
            UpdateReminderCount();
        }

        private INotificationService GetNotificationService()
        {
            try
            {
                // Пытаемся получить сервис из DI контейнера
                if (Application.Current?.MainPage?.Handler?.MauiContext?.Services != null)
                {
                    return Application.Current.MainPage.Handler.MauiContext.Services.GetService<INotificationService>();
                }

                // Если не получилось, создаем fallback сервис
                System.Diagnostics.Debug.WriteLine("Warning: Could not get NotificationService from DI, creating fallback");
                return new DefaultNotificationService();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting NotificationService: {ex.Message}");
                return new DefaultNotificationService();
            }
        }

        public ObservableCollection<Reminder> Reminders => _reminders;

        private async void OnAddReminderClicked(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Add reminder button clicked");

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

                // Показываем форму создания напоминания
                await ShowCreateReminderDialog();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in OnAddReminderClicked: {ex.Message}");
                await DisplayAlert("Ошибка", $"Произошла ошибка: {ex.Message}", "OK");
            }
        }

        private async Task ShowCreateReminderDialog()
        {
            try
            {
                // Создаем простую форму ввода
                string title = await DisplayPromptAsync("Новое напоминание", "Введите заголовок:", "OK", "Отмена", "Напоминание", 50);

                if (string.IsNullOrWhiteSpace(title))
                {
                    System.Diagnostics.Debug.WriteLine("Title is empty, cancelling");
                    return;
                }

                string message = await DisplayPromptAsync("Описание", "Введите описание (необязательно):", "OK", "Отмена", "", 100);
                if (message == null) message = ""; // Пользователь нажал отмена

                // Запрашиваем время (упрощенно - на 1 минуту вперед для тестирования)
                var scheduledTime = DateTime.Now.AddMinutes(1);

                var timeChoice = await DisplayActionSheet(
                    "Когда напомнить?",
                    "Отмена",
                    null,
                    "Через 1 минуту (тест)",
                    "Через 5 минут",
                    "Через 1 час",
                    "Завтра в это время"
                );

                switch (timeChoice)
                {
                    case "Через 1 минуту (тест)":
                        scheduledTime = DateTime.Now.AddMinutes(1);
                        break;
                    case "Через 5 минут":
                        scheduledTime = DateTime.Now.AddMinutes(5);
                        break;
                    case "Через 1 час":
                        scheduledTime = DateTime.Now.AddHours(1);
                        break;
                    case "Завтра в это время":
                        scheduledTime = DateTime.Now.AddDays(1);
                        break;
                    case "Отмена":
                    case null:
                        System.Diagnostics.Debug.WriteLine("Time selection cancelled");
                        return;
                }

                // Создаем напоминание
                await CreateReminder(title, message, scheduledTime);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ShowCreateReminderDialog: {ex.Message}");
                await DisplayAlert("Ошибка", $"Ошибка создания формы: {ex.Message}", "OK");
            }
        }

        private async Task CreateReminder(string title, string message, DateTime scheduledTime)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Creating reminder: {title} for {scheduledTime}");

                // Создаем объект напоминания
                var reminder = new Reminder
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = title,
                    Message = message,
                    ScheduledTime = scheduledTime,
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    Metadata = new Dictionary<string, string>()
                };

                // Планируем уведомление
                var notificationId = await _notificationService.ScheduleNotification(
                    reminder.Title,
                    reminder.Message,
                    reminder.ScheduledTime,
                    reminder.Metadata
                );

                // Сохраняем ID уведомления
                _notificationIds[reminder.Id] = notificationId;

                // ВАЖНО: Добавляем в коллекцию в UI потоке
                await Dispatcher.DispatchAsync(() =>
                {
                    _reminders.Add(reminder);
                    UpdateReminderCount();
                });

                System.Diagnostics.Debug.WriteLine($"Reminder created successfully. Total reminders: {_reminders.Count}");

                await DisplayAlert("Успех", $"Напоминание создано!\nВремя: {scheduledTime:HH:mm dd.MM.yyyy}", "OK");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating reminder: {ex.Message}");
                await DisplayAlert("Ошибка", $"Не удалось создать напоминание: {ex.Message}", "OK");
            }
        }

        private async void OnEditReminderClicked(object sender, EventArgs e)
        {
            try
            {
                if (sender is Button button && button.BindingContext is Reminder reminder)
                {
                    System.Diagnostics.Debug.WriteLine($"Editing reminder: {reminder.Title}");

                    // Простое редактирование - изменяем только заголовок
                    string newTitle = await DisplayPromptAsync("Редактировать", "Новый заголовок:", "OK", "Отмена", reminder.Title);

                    if (!string.IsNullOrWhiteSpace(newTitle) && newTitle != reminder.Title)
                    {
                        await Dispatcher.DispatchAsync(() =>
                        {
                            reminder.Title = newTitle;
                            // Принудительно обновляем UI
                            var index = _reminders.IndexOf(reminder);
                            if (index >= 0)
                            {
                                _reminders[index] = reminder;
                            }
                        });

                        await DisplayAlert("Успех", "Напоминание обновлено", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error editing reminder: {ex.Message}");
                await DisplayAlert("Ошибка", $"Ошибка редактирования: {ex.Message}", "OK");
            }
        }

        private async void OnDeleteReminderClicked(object sender, EventArgs e)
        {
            try
            {
                if (sender is Button button && button.BindingContext is Reminder reminder)
                {
                    System.Diagnostics.Debug.WriteLine($"Deleting reminder: {reminder.Title}");

                    bool confirm = await DisplayAlert("Подтверждение", $"Удалить напоминание '{reminder.Title}'?", "Да", "Нет");

                    if (confirm)
                    {
                        // Отменяем уведомление
                        if (_notificationIds.ContainsKey(reminder.Id))
                        {
                            await _notificationService.CancelNotification(_notificationIds[reminder.Id]);
                            _notificationIds.Remove(reminder.Id);
                        }

                        // Удаляем из коллекции
                        await Dispatcher.DispatchAsync(() =>
                        {
                            _reminders.Remove(reminder);
                            UpdateReminderCount();
                        });

                        System.Diagnostics.Debug.WriteLine($"Reminder deleted. Remaining: {_reminders.Count}");
                        await DisplayAlert("Успех", "Напоминание удалено", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting reminder: {ex.Message}");
                await DisplayAlert("Ошибка", $"Ошибка удаления: {ex.Message}", "OK");
            }
        }

        private async void OnRefreshClicked(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Refresh clicked");
                await LoadReminders();
                await DisplayAlert("Обновлено", $"Загружено {_reminders.Count} напоминаний", "OK");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error refreshing: {ex.Message}");
                await DisplayAlert("Ошибка", $"Ошибка обновления: {ex.Message}", "OK");
            }
        }

        private async void OnClearAllClicked(object sender, EventArgs e)
        {
            try
            {
                if (_reminders.Count == 0)
                {
                    await DisplayAlert("Информация", "Нет напоминаний для удаления", "OK");
                    return;
                }

                bool confirm = await DisplayAlert("Подтверждение", $"Удалить все {_reminders.Count} напоминаний?", "Да", "Нет");

                if (confirm)
                {
                    // Отменяем все уведомления
                    await _notificationService.CancelAllNotifications();

                    // Очищаем коллекции
                    await Dispatcher.DispatchAsync(() =>
                    {
                        _reminders.Clear();
                        _notificationIds.Clear();
                        UpdateReminderCount();
                    });

                    System.Diagnostics.Debug.WriteLine("All reminders cleared");
                    await DisplayAlert("Успех", "Все напоминания удалены", "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error clearing all: {ex.Message}");
                await DisplayAlert("Ошибка", $"Ошибка очистки: {ex.Message}", "OK");
            }
        }

        private async Task LoadReminders()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Loading reminders...");

                // В этой версии у нас нет постоянного хранилища, 
                // но можно добавить позже

                await Dispatcher.DispatchAsync(() =>
                {
                    UpdateReminderCount();
                });

                System.Diagnostics.Debug.WriteLine($"Loaded {_reminders.Count} reminders");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading reminders: {ex.Message}");
            }
        }

        private void UpdateReminderCount()
        {
            try
            {
                var activeCount = _reminders.Count(r => r.IsActive);
                ReminderCountLabel.Text = $"{activeCount} активных напоминаний";

                // Показываем/скрываем EmptyState
                EmptyStateLabel.IsVisible = _reminders.Count == 0;

                System.Diagnostics.Debug.WriteLine($"Updated reminder count: {activeCount}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating reminder count: {ex.Message}");
                ReminderCountLabel.Text = "0 активных напоминаний";
            }
        }
    }
}