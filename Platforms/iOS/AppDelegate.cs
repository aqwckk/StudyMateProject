using Foundation;
using UIKit;
using UserNotifications;

namespace StudyMateTest
{
    [Register("AppDelegate")]
    public class AppDelegate : MauiUIApplicationDelegate
    {
        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            // Настраиваем уведомления при запуске
            ConfigureNotifications();

            // Настраиваем внешний вид приложения
            ConfigureAppearance();

            return base.FinishedLaunching(app, options);
        }

        // Метод вызывается когда приложение становится активным
        public override void OnActivated(UIApplication uiApplication)
        {
            base.OnActivated(uiApplication);

            // Сбрасываем счетчик уведомлений на иконке приложения
            UIApplication.SharedApplication.ApplicationIconBadgeNumber = 0;

            System.Diagnostics.Debug.WriteLine("App activated - badge cleared");
        }

        // Метод вызывается когда приложение уходит в фон
        public override void DidEnterBackground(UIApplication uiApplication)
        {
            base.DidEnterBackground(uiApplication);
            System.Diagnostics.Debug.WriteLine("App entered background");
        }

        // Метод вызывается когда приложение возвращается из фона
        public override void WillEnterForeground(UIApplication uiApplication)
        {
            base.WillEnterForeground(uiApplication);
            System.Diagnostics.Debug.WriteLine("App will enter foreground");

            // Сбрасываем счетчик уведомлений
            UIApplication.SharedApplication.ApplicationIconBadgeNumber = 0;
        }

        // Приватный метод для настройки уведомлений
        private async void ConfigureNotifications()
        {
            var center = UNUserNotificationCenter.Current;
            center.Delegate = new ExtendedNotificationDelegate();

            try
            {
                // Запрашиваем разрешения на уведомления
                var authorizationResult = await center.RequestAuthorizationAsync(
                    UNAuthorizationOptions.Alert |
                    UNAuthorizationOptions.Sound |
                    UNAuthorizationOptions.Badge |
                    UNAuthorizationOptions.CriticalAlert); // Критические уведомления (для важных напоминаний)

                System.Diagnostics.Debug.WriteLine($"iOS Notification permission granted: {authorizationResult.Item1}");

                if (authorizationResult.Item1)
                {
                    // Создаем категории уведомлений с действиями
                    await CreateNotificationCategories(center);

                    System.Diagnostics.Debug.WriteLine("Notification categories created");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Notification permission denied");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error configuring notifications: {ex.Message}");
            }
        }

        // Создание категорий уведомлений с интерактивными действиями
        private async Task CreateNotificationCategories(UNUserNotificationCenter center)
        {
            // Действие "Открыть" - открывает приложение
            var openAction = UNNotificationAction.FromIdentifier(
                "OPEN_ACTION",
                "Открыть",
                UNNotificationActionOptions.Foreground); // Открывает приложение

            // Действие "Отложить" - откладывает напоминание на 10 минут
            var snoozeAction = UNNotificationAction.FromIdentifier(
                "SNOOZE_ACTION",
                "Отложить на 10 мин",
                UNNotificationActionOptions.None); // Выполняется в фоне

            // Действие "Отметить как выполненное" - помечает задачу выполненной
            var completeAction = UNNotificationAction.FromIdentifier(
                "COMPLETE_ACTION",
                "Выполнено",
                UNNotificationActionOptions.Destructive); // Красная кнопка

            // Категория для напоминаний о заметках
            var reminderCategory = UNNotificationCategory.FromIdentifier(
                "REMINDER_CATEGORY",
                new UNNotificationAction[] { openAction, snoozeAction, completeAction },
                new string[] { }, // Placeholder для текста
                UNNotificationCategoryOptions.CustomDismissAction);

            // Действие только для открытия (для простых уведомлений)
            var simpleCategory = UNNotificationCategory.FromIdentifier(
                "SIMPLE_CATEGORY",
                new UNNotificationAction[] { openAction },
                new string[] { },
                UNNotificationCategoryOptions.None);

            // Регистрируем категории в системе
            var categories = new NSSet<UNNotificationCategory>(reminderCategory, simpleCategory);
            center.SetNotificationCategories(categories);
        }

        // Настройка внешнего вида приложения
        private void ConfigureAppearance()
        {
            // Настройка цвета статус бара
            if (UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
            {
                // iOS 13+ использует другой способ настройки статус бара
                UIApplication.SharedApplication.SetStatusBarStyle(UIStatusBarStyle.Default, false);
            }
        }
    }

    // Расширенный делегат для обработки уведомлений
    public class ExtendedNotificationDelegate : UNUserNotificationCenterDelegate
    {
        // Метод вызывается когда уведомление приходит в открытом приложении
        public override void WillPresentNotification(UNUserNotificationCenter center, UNNotification notification, Action<UNNotificationPresentationOptions> completionHandler)
        {
            var content = notification.Request.Content;
            System.Diagnostics.Debug.WriteLine($"Will present notification: {content.Title} - {content.Body}");

            // Показываем уведомление даже в открытом приложении
            completionHandler(UNNotificationPresentationOptions.Alert |
                            UNNotificationPresentationOptions.Badge |
                            UNNotificationPresentationOptions.Sound |
                            UNNotificationPresentationOptions.Banner); // iOS 14+ баннер
        }

        // Метод вызывается при нажатии на уведомление или его действия
        public override void DidReceiveNotificationResponse(UNUserNotificationCenter center, UNNotificationResponse response, Action completionHandler)
        {
            var content = response.Notification.Request.Content;
            var userInfo = content.UserInfo;
            var actionIdentifier = response.ActionIdentifier;

            System.Diagnostics.Debug.WriteLine($"User interacted with notification: {content.Title}");
            System.Diagnostics.Debug.WriteLine($"Action: {actionIdentifier}");

            // Обрабатываем разные действия пользователя
            switch (actionIdentifier)
            {
                case "OPEN_ACTION":
                case "com.apple.UNNotificationDefaultActionIdentifier": // Исправленная константа для нажатия на уведомление
                    HandleOpenAction(userInfo);
                    break;

                case "SNOOZE_ACTION":
                    HandleSnoozeAction(userInfo);
                    break;

                case "COMPLETE_ACTION":
                    HandleCompleteAction(userInfo);
                    break;

                case "com.apple.UNNotificationDismissActionIdentifier": // Исправленная константа для смахивания
                    HandleDismissAction(userInfo);
                    break;

                default:
                    // Если действие неизвестно, но это нажатие на уведомление (пустая строка или null)
                    if (string.IsNullOrEmpty(actionIdentifier) || actionIdentifier == "default")
                    {
                        HandleOpenAction(userInfo);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Unknown action: {actionIdentifier}");
                        // По умолчанию открываем приложение
                        HandleOpenAction(userInfo);
                    }
                    break;
            }

            // Обрабатываем метаданные уведомления
            ProcessNotificationMetadata(userInfo);

            completionHandler();
        }

        // Обработка действия "Открыть"
        private void HandleOpenAction(NSDictionary userInfo)
        {
            System.Diagnostics.Debug.WriteLine("User chose to open the app");

            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    // Получаем ID заметки из метаданных
                    if (userInfo?.ContainsKey(new NSString("noteId")) == true)
                    {
                        var noteId = userInfo["noteId"]?.ToString();
                        NavigateToNote(noteId);
                    }
                    else
                    {
                        // Открываем главную страницу
                        NavigateToMainPage();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in HandleOpenAction: {ex.Message}");
                }
            });
        }

        // Обработка действия "Отложить"
        private async void HandleSnoozeAction(NSDictionary userInfo)
        {
            System.Diagnostics.Debug.WriteLine("User chose to snooze notification");

            try
            {
                // Создаем новое уведомление через 10 минут
                var center = UNUserNotificationCenter.Current;

                var content = new UNMutableNotificationContent();
                content.Title = "Отложенное напоминание";
                content.Body = "Не забудьте проверить вашу заметку!";
                content.Sound = UNNotificationSound.Default;
                content.CategoryIdentifier = "REMINDER_CATEGORY";

                // Копируем метаданные из оригинального уведомления
                if (userInfo != null)
                {
                    content.UserInfo = userInfo;
                }

                // Устанавливаем время через 10 минут
                var trigger = UNTimeIntervalNotificationTrigger.CreateTrigger(600, false); // 600 секунд = 10 минут

                var request = UNNotificationRequest.FromIdentifier(
                    $"snooze_{Guid.NewGuid()}",
                    content,
                    trigger);

                await center.AddNotificationRequestAsync(request);
                System.Diagnostics.Debug.WriteLine("Snooze notification scheduled for 10 minutes");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating snooze notification: {ex.Message}");
            }
        }

        // Обработка действия "Выполнено"
        private void HandleCompleteAction(NSDictionary userInfo)
        {
            System.Diagnostics.Debug.WriteLine("User marked task as complete");

            try
            {
                // Здесь можно отметить задачу как выполненную в базе данных
                if (userInfo?.ContainsKey(new NSString("noteId")) == true)
                {
                    var noteId = userInfo["noteId"]?.ToString();
                    MarkNoteAsCompleted(noteId);
                }

                // Показываем подтверждение
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ShowCompletionConfirmation();
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in HandleCompleteAction: {ex.Message}");
            }
        }

        // Обработка смахивания уведомления
        private void HandleDismissAction(NSDictionary userInfo)
        {
            System.Diagnostics.Debug.WriteLine("User dismissed notification");
            // Здесь можно логировать отклоненные уведомления
        }

        // Обработка метаданных уведомления
        private void ProcessNotificationMetadata(NSDictionary userInfo)
        {
            if (userInfo != null && userInfo.Count > 0)
            {
                foreach (var item in userInfo)
                {
                    var key = item.Key.ToString();
                    var value = item.Value.ToString();

                    System.Diagnostics.Debug.WriteLine($"Notification metadata: {key} = {value}");

                    // Обработка специфичных метаданных
                    switch (key)
                    {
                        case "noteId":
                            System.Diagnostics.Debug.WriteLine($"Note ID: {value}");
                            break;
                        case "priority":
                            System.Diagnostics.Debug.WriteLine($"Priority level: {value}");
                            break;
                        case "category":
                            System.Diagnostics.Debug.WriteLine($"Category: {value}");
                            break;
                    }
                }
            }
        }

        // Навигация к конкретной заметке
        private async void NavigateToNote(string noteId)
        {
            try
            {
                if (!string.IsNullOrEmpty(noteId))
                {
                    // Пример навигации через Shell (адаптируйте под вашу структуру)
                    await Shell.Current.GoToAsync($"//notes/{noteId}");
                    System.Diagnostics.Debug.WriteLine($"Navigated to note: {noteId}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error navigating to note: {ex.Message}");
                // Если навигация не удалась, открываем главную страницу
                NavigateToMainPage();
            }
        }

        // Навигация к главной странице
        private async void NavigateToMainPage()
        {
            try
            {
                await Shell.Current.GoToAsync("//main");
                System.Diagnostics.Debug.WriteLine("Navigated to main page");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error navigating to main page: {ex.Message}");
            }
        }

        // Отметка заметки как выполненной
        private void MarkNoteAsCompleted(string noteId)
        {
            try
            {
                // Здесь добавьте логику для обновления статуса заметки в вашей базе данных
                // Например, через ваш сервис заметок:
                // var noteService = Application.Current.MainPage.Handler.MauiContext.Services.GetService<INoteService>();
                // await noteService.MarkAsCompleted(noteId);

                System.Diagnostics.Debug.WriteLine($"Note {noteId} marked as completed");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error marking note as completed: {ex.Message}");
            }
        }

        // Показ подтверждения выполнения
        private async void ShowCompletionConfirmation()
        {
            try
            {
                var viewController = Platform.GetCurrentUIViewController();

                if (viewController != null)
                {
                    var alert = UIAlertController.Create(
                        "Выполнено",
                        "Задача отмечена как выполненная",
                        UIAlertControllerStyle.Alert);

                    alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));

                    viewController.PresentViewController(alert, true, null);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing completion confirmation: {ex.Message}");
            }
        }
    }
}