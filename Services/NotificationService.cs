namespace StudyMateProject.Services
{
    public static class NotificationService
    {
        private static bool _isSupported = false;

        public static async void Initialize()
        {
            try
            {
                // Сначала проверяем доступность плагина для текущей платформы
                if (DeviceInfo.Platform == DevicePlatform.WinUI)
                {
                    System.Diagnostics.Debug.WriteLine("Local notifications are not fully supported on Windows platform");
                    _isSupported = false;
                    return; // Ранний выход для Windows
                }

                // Проверяем существование LocalNotificationCenter.Current
                // используя null-условный оператор
                var center = LocalNotificationCenter.Current;
                if (center == null)
                {
                    System.Diagnostics.Debug.WriteLine("LocalNotificationCenter.Current is null");
                    _isSupported = false;
                    return;
                }

                _isSupported = true;

                // Безопасная подписка на событие
                center.NotificationActionTapped += OnNotificationTapped;

                // Запрос разрешения
                bool permissionGranted = await center.RequestNotificationPermission();
                if (!permissionGranted)
                {
                    System.Diagnostics.Debug.WriteLine("Notification permission not granted");
                }
            }
            catch (NotImplementedException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Local notifications are not implemented for this platform: {ex.Message}");
                _isSupported = false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing notification service: {ex.Message}");
                _isSupported = false;
            }
        }

        public static void ScheduleReminderNotification(Reminder reminder)
        {
            // Skip scheduling if not supported, the reminder is completed, or in the past
            if (!_isSupported || reminder.IsCompleted || reminder.DueDate <= DateTime.Now)
                return;

            try
            {
                var notification = new NotificationRequest
                {
                    NotificationId = reminder.Id.GetHashCode(),
                    Title = "Напоминание: " + reminder.Title,
                    Description = reminder.Description,
                    Schedule = new NotificationRequestSchedule
                    {
                        NotifyTime = reminder.DueDate
                    }
                };

                // Add data to identify the reminder when tapped
                notification.ReturningData = reminder.Id;

                // Вызываем синхронно, чтобы избежать проблем с отложенным выполнением
                LocalNotificationCenter.Current?.Show(notification);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to schedule notification: {ex}");
            }
        }

        public static void CancelReminderNotification(string reminderId)
        {
            try
            {
                int notificationId = reminderId.GetHashCode();
                // Без await
                LocalNotificationCenter.Current.Cancel(notificationId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to cancel notification: {ex}");
            }
        }

        private static void OnNotificationTapped(Plugin.LocalNotification.EventArgs.NotificationActionEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Request.ReturningData))
                return;

            // Get the reminder ID from the notification data
            string reminderId = e.Request.ReturningData;

            // Navigate to the reminder details
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    // Prepare navigation parameters
                    Dictionary<string, object> parameters = new Dictionary<string, object>
                    {
                        { "ReminderId", reminderId }
                    };

                    // Navigate to the reminders page first
                    await Shell.Current.GoToAsync($"//reminders");

                    // Then navigate to edit the specific reminder
                    await Shell.Current.GoToAsync($"editReminder", parameters);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to navigate to reminder: {ex}");
                }
            });
        }
    }
}