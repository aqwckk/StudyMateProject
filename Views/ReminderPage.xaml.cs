using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using StudyMateTest.Models;
using StudyMateTest.Services;
using StudyMateTest.Services.NotificationServices;

namespace StudyMateTest.Views
{
    public partial class ReminderPage : ContentPage
    {
        private readonly INotificationService _notificationService;
        private readonly ILocalStorageService _localStorageService;
        private readonly ObservableCollection<Reminder> _reminders;
        private readonly Dictionary<string, string> _notificationIds;

        public ReminderPage()
        {
            InitializeComponent();
            _reminders = new ObservableCollection<Reminder>();
            _notificationIds = new Dictionary<string, string>();
            _notificationService = GetNotificationService();
            _localStorageService = GetLocalStorageService();
            BindingContext = this;
            RemindersCollectionView.ItemsSource = _reminders;

            // Подписываемся на сообщения
            MessagingCenter.Subscribe<AddReminderPage, Reminder>(this, "ReminderCreated", OnReminderCreated);
            MessagingCenter.Subscribe<EditReminderPage, Reminder>(this, "ReminderEdited", OnReminderEdited);

            _ = Task.Run(LoadReminders);

            // Запускаем периодическое обновление статусов
            StartStatusUpdateTimer();

            UpdateReminderCount();
        }

        private INotificationService GetNotificationService()
        {
            try
            {
                if (Application.Current?.MainPage?.Handler?.MauiContext?.Services != null)
                {
                    return Application.Current.MainPage.Handler.MauiContext.Services.GetService<INotificationService>();
                }
                return new DefaultNotificationService();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting NotificationService: {ex.Message}");
                return new DefaultNotificationService();
            }
        }

        private ILocalStorageService GetLocalStorageService()
        {
            try
            {
                if (Application.Current?.MainPage?.Handler?.MauiContext?.Services != null)
                {
                    return Application.Current.MainPage.Handler.MauiContext.Services.GetService<ILocalStorageService>();
                }
                return new LocalStorageService();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting LocalStorageService: {ex.Message}");
                return new LocalStorageService();
            }
        }

        private async void OnReminderEdited(EditReminderPage sender, Reminder editedReminder)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Received edited reminder: {editedReminder.Title}");

                // Отменяем старое уведомление если оно есть
                if (_notificationIds.ContainsKey(editedReminder.Id))
                {
                    await _notificationService.CancelNotification(_notificationIds[editedReminder.Id]);
                    _notificationIds.Remove(editedReminder.Id);
                    System.Diagnostics.Debug.WriteLine($"Cancelled old notification for reminder: {editedReminder.Id}");
                }

                // Создаем новое уведомление если время в будущем
                if (editedReminder.ScheduledTime > DateTime.Now)
                {
                    var notificationId = await _notificationService.ScheduleNotification(
                        editedReminder.Title,
                        string.IsNullOrWhiteSpace(editedReminder.Message)
                            ? $"Напоминание на {editedReminder.ScheduledTime:HH:mm}"
                            : editedReminder.Message,
                        editedReminder.ScheduledTime,
                        editedReminder.Metadata
                    );

                    editedReminder.Metadata["NotificationId"] = notificationId;
                    _notificationIds[editedReminder.Id] = notificationId;

                    System.Diagnostics.Debug.WriteLine($"Created new notification for edited reminder: {notificationId}");
                }

                await Dispatcher.DispatchAsync(() =>
                {
                    var index = _reminders.ToList().FindIndex(r => r.Id == editedReminder.Id);
                    if (index >= 0)
                    {
                        _reminders[index] = editedReminder;
                        UpdateReminderCount();
                    }
                });

                await SaveReminders();

                System.Diagnostics.Debug.WriteLine($"Reminder edited and saved: {editedReminder.Title}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error handling edited reminder: {ex.Message}");
            }
        }

        private void StartStatusUpdateTimer()
        {
            try
            {
                var timer = new System.Timers.Timer(60000);
                timer.Elapsed += async (s, e) =>
                {
                    await UpdateReminderStatuses();
                };
                timer.Start();

                System.Diagnostics.Debug.WriteLine("Status update timer started");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error starting status timer: {ex.Message}");
            }
        }

        private async Task UpdateReminderStatuses()
        {
            try
            {
                bool hasChanges = false;

                await Dispatcher.DispatchAsync(() =>
                {
                    foreach (var reminder in _reminders)
                    {
                        var wasActive = reminder.IsActive;
                        reminder.UpdateStatus();

                        if (wasActive != reminder.IsActive)
                        {
                            hasChanges = true;
                        }
                    }

                    if (hasChanges)
                    {
                        UpdateReminderCount();
                    }
                });

                if (hasChanges)
                {
                    await SaveReminders();
                    System.Diagnostics.Debug.WriteLine("Reminder statuses updated and saved");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating reminder statuses: {ex.Message}");
            }
        }

        private async void OnAddReminderClicked(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Add reminder button clicked - opening new page");
                var addReminderPage = new AddReminderPage();
                await Navigation.PushAsync(addReminderPage);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in OnAddReminderClicked: {ex.Message}");
                await DisplayAlert("Ошибка", $"Произошла ошибка: {ex.Message}", "OK");
            }
        }

        private async void OnReminderCreated(AddReminderPage sender, Reminder reminder)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Received new reminder: {reminder.Title}");
                if (reminder.Metadata.ContainsKey("NotificationId"))
                {
                    _notificationIds[reminder.Id] = reminder.Metadata["NotificationId"];
                }
                await Dispatcher.DispatchAsync(() =>
                {
                    _reminders.Add(reminder);
                    UpdateReminderCount();
                });

                await SaveReminders();

                System.Diagnostics.Debug.WriteLine($"Reminder added and saved. Total reminders: {_reminders.Count}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding received reminder: {ex.Message}");
            }
        }

        private async void OnEditReminderClicked(object sender, EventArgs e)
        {
            try
            {
                if (sender is Button button && button.BindingContext is Reminder reminder)
                {
                    System.Diagnostics.Debug.WriteLine($"Opening edit page for reminder: {reminder.Title}");
                    var editPage = new EditReminderPage(reminder);
                    await Navigation.PushAsync(editPage);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error opening edit page: {ex.Message}");
                await DisplayAlert("Ошибка", $"Ошибка открытия редактора: {ex.Message}", "OK");
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
                        if (_notificationIds.ContainsKey(reminder.Id))
                        {
                            await _notificationService.CancelNotification(_notificationIds[reminder.Id]);
                            _notificationIds.Remove(reminder.Id);
                        }
                        await Dispatcher.DispatchAsync(() =>
                        {
                            _reminders.Remove(reminder);
                            UpdateReminderCount();
                        });

                        await SaveReminders();

                        System.Diagnostics.Debug.WriteLine($"Reminder deleted and saved. Remaining: {_reminders.Count}");
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
                System.Diagnostics.Debug.WriteLine("Refresh clicked - reloading from storage");
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
                    await _notificationService.CancelAllNotifications();
                    await Dispatcher.DispatchAsync(() =>
                    {
                        _reminders.Clear();
                        _notificationIds.Clear();
                        UpdateReminderCount();
                    });

                    await _localStorageService.DeleteAllRemindersAsync();

                    System.Diagnostics.Debug.WriteLine("All reminders cleared from memory and storage");
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
                System.Diagnostics.Debug.WriteLine("Loading reminders from local storage...");

                var savedReminders = await _localStorageService.LoadRemindersAsync();

                await Dispatcher.DispatchAsync(() =>
                {
                    _reminders.Clear();

                    foreach (var reminder in savedReminders)
                    {
                        _reminders.Add(reminder);

                        if (reminder.Metadata.ContainsKey("NotificationId"))
                        {
                            _notificationIds[reminder.Id] = reminder.Metadata["NotificationId"];
                        }
                    }

                    UpdateReminderCount();
                });

                System.Diagnostics.Debug.WriteLine($"Loaded {_reminders.Count} reminders from storage");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading reminders: {ex.Message}");
            }
        }

        private async Task SaveReminders()
        {
            try
            {
                var remindersList = _reminders.ToList();
                await _localStorageService.SaveRemindersAsync(remindersList);
                System.Diagnostics.Debug.WriteLine($"Saved {remindersList.Count} reminders to storage");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving reminders: {ex.Message}");
            }
        }

        private void UpdateReminderCount()
        {
            try
            {
                var activeCount = _reminders.Count(r => r.IsCurrentlyActive);
                ReminderCountLabel.Text = $"{activeCount} активных напоминаний";
                bool isEmpty = _reminders.Count == 0;
                EmptyStateLabel.IsVisible = isEmpty;
                RemindersCollectionView.IsVisible = !isEmpty;
                System.Diagnostics.Debug.WriteLine($"Updated reminder count: {activeCount}, isEmpty: {isEmpty}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating reminder count: {ex.Message}");
                ReminderCountLabel.Text = "0 активных напоминаний";
            }
        }

        public ObservableCollection<Reminder> Reminders => _reminders;
    }
}