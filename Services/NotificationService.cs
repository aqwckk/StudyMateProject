using Plugin.LocalNotification;
using StudyMateProject.Models;

namespace StudyMateProject.Services
{
    public static class NotificationService
    {
        public static async void Initialize()
        {
            try
            {
                // Request permission with await, т.к. метод возвращает Task<bool>
                bool permissionGranted = await LocalNotificationCenter.Current.RequestNotificationPermission();
                if (!permissionGranted)
                {
                    System.Diagnostics.Debug.WriteLine("Notification permission not granted");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error requesting notification permission: {ex}");
            }

            // Set notification tap handling
            LocalNotificationCenter.Current.NotificationActionTapped += OnNotificationTapped;
        }

        public static void ScheduleReminderNotification(Reminder reminder)
        {
            // Skip scheduling if the reminder is completed or in the past
            if (reminder.IsCompleted || reminder.DueDate <= DateTime.Now)
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

                // Без await
                LocalNotificationCenter.Current.Show(notification);
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