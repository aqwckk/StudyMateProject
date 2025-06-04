using Microsoft.Maui.Controls.Shapes;
using StudyMateTest.Models;
using StudyMateTest.Services;
using System.Collections.ObjectModel;

namespace StudyMateTest.Views;

public partial class ReminderPage : ContentPage
{
    // Сервис для работы с уведомлениями
    private readonly INotificationService _notificationService;

    // Коллекция напоминаний для отображения в UI
    private readonly ObservableCollection<Reminder> _reminders = new();

    // Словарь для отслеживания ID уведомлений (строковый ID → системный числовой ID)
    private readonly Dictionary<string, string> _notificationIds = new();

    public ReminderPage()
    {
        InitializeComponent();

        // Получаем сервис уведомлений через Dependency Injection
        _notificationService = Application.Current.MainPage.Handler.MauiContext.Services.GetService<INotificationService>();

        // Подписываемся на изменения в коллекции напоминаний
        _reminders.CollectionChanged += OnRemindersCollectionChanged;
    }

    // Метод вызывается когда страница появляется на экране
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Запрашиваем разрешение на уведомления при первом появлении
        await RequestNotificationPermission();

        // Загружаем существующие напоминания
        await LoadReminders();

        // Обновляем UI
        UpdateUI();
    }

    // Запрос разрешения на уведомления
    private async Task RequestNotificationPermission()
    {
        try
        {
            var hasPermission = await _notificationService.IsPermissionGranted();

            if (!hasPermission)
            {
                var granted = await _notificationService.RequestPermission();

                if (!granted)
                {
                    await DisplayAlert("Разрешение отклонено",
                        "Для работы напоминаний необходимо разрешение на уведомления. Вы можете включить их в настройках приложения.",
                        "OK");
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось запросить разрешение: {ex.Message}", "OK");
        }
    }

    // Загрузка сохраненных напоминаний (пока что создаем примеры)
    private async Task LoadReminders()
    {
        try
        {
            // TODO: Здесь должна быть загрузка из базы данных или локального хранилища
            // Пока что создаем примеры для демонстрации

            // Очищаем коллекцию
            _reminders.Clear();

            // Можно добавить логику загрузки из файла или базы данных
            // Например: var savedReminders = await LoadFromLocalStorage();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось загрузить напоминания: {ex.Message}", "OK");
        }
    }

    // Обновление пользовательского интерфейса
    private void UpdateUI()
    {
        // Обновляем счетчик напоминаний
        var activeCount = _reminders.Count(r => r.IsActive && r.ScheduledTime > DateTime.Now);
        CountLabel.Text = $"{activeCount} активных напоминаний";

        // Показываем/скрываем empty state
        EmptyState.IsVisible = _reminders.Count == 0;
        RemindersList.IsVisible = _reminders.Count > 0;

        // Очищаем список карточек
        RemindersList.Children.Clear();

        // Создаем карточки для каждого напоминания
        foreach (var reminder in _reminders.OrderBy(r => r.ScheduledTime))
        {
            var card = CreateReminderCard(reminder);
            RemindersList.Children.Add(card);
        }
    }

    // Создание карточки напоминания
    private Border CreateReminderCard(Reminder reminder)
    {
        // Определяем цвет карточки в зависимости от времени
        var isExpired = reminder.ScheduledTime < DateTime.Now;
        var isUpcoming = reminder.ScheduledTime <= DateTime.Now.AddHours(1);

        var cardColor = isExpired ? Color.FromArgb("#FFEBEE") :
                       isUpcoming ? Color.FromArgb("#FFF3E0") :
                       Color.FromArgb("#F5F5F5");

        var borderColor = isExpired ? Color.FromArgb("#F44336") :
                         isUpcoming ? Color.FromArgb("#FF9800") :
                         Color.FromArgb("#E0E0E0");

        // Создаем карточку
        var card = new Border
        {
            BackgroundColor = cardColor,
            Stroke = borderColor,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 12 }, // Исправленный способ для .NET MAUI
            Margin = new Thickness(0, 5),
            Padding = new Thickness(15)
        };

        // Создаем содержимое карточки
        var mainGrid = new Grid();
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        // Заголовок
        var titleLabel = new Label
        {
            Text = reminder.Title,
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.Black
        };
        Grid.SetRow(titleLabel, 0);
        Grid.SetColumn(titleLabel, 0);
        mainGrid.Children.Add(titleLabel);

        // Описание
        if (!string.IsNullOrEmpty(reminder.Message))
        {
            var messageLabel = new Label
            {
                Text = reminder.Message,
                FontSize = 14,
                TextColor = Color.FromArgb("#666666"),
                Margin = new Thickness(0, 5, 0, 0)
            };
            Grid.SetRow(messageLabel, 1);
            Grid.SetColumn(messageLabel, 0);
            mainGrid.Children.Add(messageLabel);
        }

        // Время
        var timeText = GetTimeDisplayText(reminder.ScheduledTime);
        var timeLabel = new Label
        {
            Text = timeText,
            FontSize = 12,
            TextColor = isExpired ? Color.FromArgb("#F44336") : Color.FromArgb("#888888"),
            FontAttributes = isExpired ? FontAttributes.Bold : FontAttributes.None,
            Margin = new Thickness(0, 10, 0, 0)
        };
        Grid.SetRow(timeLabel, 2);
        Grid.SetColumn(timeLabel, 0);
        mainGrid.Children.Add(timeLabel);

        // Кнопки действий
        var actionsStack = new StackLayout
        {
            Orientation = StackOrientation.Horizontal,
            Spacing = 10,
            VerticalOptions = LayoutOptions.Center
        };

        // Кнопка редактирования
        var editButton = new Button
        {
            Text = "✏️",
            FontSize = 16,
            BackgroundColor = Colors.Transparent,
            TextColor = Color.FromArgb("#2196F3"),
            WidthRequest = 40,
            HeightRequest = 40,
            BorderColor = Color.FromArgb("#2196F3"),
            BorderWidth = 1
        };
        editButton.Clicked += (s, e) => OnEditReminderClicked(reminder);
        actionsStack.Children.Add(editButton);

        // Кнопка удаления
        var deleteButton = new Button
        {
            Text = "🗑️",
            FontSize = 16,
            BackgroundColor = Colors.Transparent,
            TextColor = Color.FromArgb("#F44336"),
            WidthRequest = 40,
            HeightRequest = 40,
            BorderColor = Color.FromArgb("#F44336"),
            BorderWidth = 1
        };
        deleteButton.Clicked += (s, e) => OnDeleteReminderClicked(reminder);
        actionsStack.Children.Add(deleteButton);

        Grid.SetRow(actionsStack, 0);
        Grid.SetColumn(actionsStack, 1);
        Grid.SetRowSpan(actionsStack, 3);
        mainGrid.Children.Add(actionsStack);

        card.Content = mainGrid;
        return card;
    }

    // Получение читаемого текста времени
    private string GetTimeDisplayText(DateTime scheduledTime)
    {
        var now = DateTime.Now;
        var timeSpan = scheduledTime - now;

        if (scheduledTime < now)
        {
            var pastTimeSpan = now - scheduledTime;
            if (pastTimeSpan.TotalMinutes < 60)
                return $"Просрочено на {(int)pastTimeSpan.TotalMinutes} мин";
            else if (pastTimeSpan.TotalHours < 24)
                return $"Просрочено на {(int)pastTimeSpan.TotalHours} ч";
            else
                return $"Просрочено на {(int)pastTimeSpan.TotalDays} дн";
        }
        else
        {
            if (timeSpan.TotalMinutes < 60)
                return $"Через {(int)timeSpan.TotalMinutes} мин ({scheduledTime:HH:mm})";
            else if (timeSpan.TotalHours < 24)
                return $"Через {(int)timeSpan.TotalHours} ч ({scheduledTime:HH:mm})";
            else if (timeSpan.TotalDays < 7)
                return $"Через {(int)timeSpan.TotalDays} дн ({scheduledTime:dd.MM HH:mm})";
            else
                return scheduledTime.ToString("dd.MM.yyyy HH:mm");
        }
    }

    // Обработчик изменения коллекции напоминаний
    private void OnRemindersCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        // Обновляем UI при изменении коллекции
        MainThread.BeginInvokeOnMainThread(() => UpdateUI());
    }

    // Обработчик нажатия кнопки добавления напоминания
    private async void OnAddReminderClicked(object sender, EventArgs e)
    {
        await ShowAddReminderDialog();
    }

    // Обработчик нажатия кнопки обновления
    private async void OnRefreshClicked(object sender, EventArgs e)
    {
        await LoadReminders();
        UpdateUI();
        await DisplayAlert("Обновлено", "Список напоминаний обновлен", "OK");
    }

    // Обработчик нажатия кнопки очистки всех напоминаний
    private async void OnClearAllClicked(object sender, EventArgs e)
    {
        var confirm = await DisplayAlert("Подтверждение",
            "Вы уверены, что хотите удалить все напоминания?",
            "Да", "Отмена");

        if (confirm)
        {
            await ClearAllReminders();
        }
    }

    // Обработчик редактирования напоминания
    private async void OnEditReminderClicked(Reminder reminder)
    {
        await ShowEditReminderDialog(reminder);
    }

    // Обработчик удаления напоминания
    private async void OnDeleteReminderClicked(Reminder reminder)
    {
        var confirm = await DisplayAlert("Подтверждение",
            $"Удалить напоминание \"{reminder.Title}\"?",
            "Да", "Отмена");

        if (confirm)
        {
            await DeleteReminder(reminder);
        }
    }

    // Диалог добавления нового напоминания
    private async Task ShowAddReminderDialog()
    {
        try
        {
            // Создаем новое напоминание с данными по умолчанию
            var newReminder = new Reminder
            {
                Title = "",
                Message = "",
                ScheduledTime = DateTime.Now.AddMinutes(30), // По умолчанию через 30 минут
                IsActive = true
            };

            // Показываем диалог редактирования
            var success = await ShowReminderEditDialog(newReminder, isNew: true);

            if (success)
            {
                // Создаем уведомление в системе
                var notificationId = await _notificationService.ScheduleNotification(
                    newReminder.Title,
                    newReminder.Message,
                    newReminder.ScheduledTime,
                    newReminder.Metadata
                );

                // Сохраняем ID уведомления
                _notificationIds[newReminder.Id] = notificationId;

                // Добавляем в коллекцию
                _reminders.Add(newReminder);

                // TODO: Сохранить в базу данных или локальное хранилище
                // await SaveReminder(newReminder);

                await DisplayAlert("Успех", "Напоминание создано!", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось создать напоминание: {ex.Message}", "OK");
        }
    }

    // Диалог редактирования существующего напоминания
    private async Task ShowEditReminderDialog(Reminder reminder)
    {
        try
        {
            // Создаем копию для редактирования
            var editedReminder = new Reminder
            {
                Id = reminder.Id,
                Title = reminder.Title,
                Message = reminder.Message,
                ScheduledTime = reminder.ScheduledTime,
                IsActive = reminder.IsActive,
                Metadata = new Dictionary<string, string>(reminder.Metadata)
            };

            // Показываем диалог редактирования
            var success = await ShowReminderEditDialog(editedReminder, isNew: false);

            if (success)
            {
                // Отменяем старое уведомление
                if (_notificationIds.ContainsKey(reminder.Id))
                {
                    await _notificationService.CancelNotification(_notificationIds[reminder.Id]);
                }

                // Создаем новое уведомление
                var notificationId = await _notificationService.ScheduleNotification(
                    editedReminder.Title,
                    editedReminder.Message,
                    editedReminder.ScheduledTime,
                    editedReminder.Metadata
                );

                // Обновляем ID уведомления
                _notificationIds[editedReminder.Id] = notificationId;

                // Обновляем данные в коллекции
                var index = _reminders.IndexOf(reminder);
                if (index >= 0)
                {
                    _reminders[index] = editedReminder;
                }

                // TODO: Обновить в базе данных
                // await UpdateReminder(editedReminder);

                await DisplayAlert("Успех", "Напоминание обновлено!", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось обновить напоминание: {ex.Message}", "OK");
        }
    }

    // Универсальный диалог редактирования напоминания
    private async Task<bool> ShowReminderEditDialog(Reminder reminder, bool isNew)
    {
        // Создаем простую страницу редактирования
        var editPage = new ContentPage
        {
            Title = isNew ? "Новое напоминание" : "Редактирование"
        };

        var titleEntry = new Entry
        {
            Placeholder = "Заголовок напоминания",
            Text = reminder.Title,
            FontSize = 16
        };

        var messageEditor = new Editor
        {
            Placeholder = "Описание (необязательно)",
            Text = reminder.Message,
            FontSize = 14,
            HeightRequest = 100
        };

        var datePicker = new DatePicker
        {
            Date = reminder.ScheduledTime.Date,
            Format = "dd.MM.yyyy"
        };

        var timePicker = new TimePicker
        {
            Time = reminder.ScheduledTime.TimeOfDay,
            Format = "HH:mm"
        };

        var saveButton = new Button
        {
            Text = isNew ? "Создать" : "Сохранить",
            BackgroundColor = Color.FromArgb("#2196F3"),
            TextColor = Colors.White
        };

        var cancelButton = new Button
        {
            Text = "Отмена",
            BackgroundColor = Colors.Transparent,
            TextColor = Color.FromArgb("#666666"),
            BorderColor = Color.FromArgb("#CCCCCC"),
            BorderWidth = 1
        };

        var buttonStack = new StackLayout
        {
            Orientation = StackOrientation.Horizontal,
            Spacing = 10
        };
        buttonStack.Children.Add(cancelButton);
        buttonStack.Children.Add(saveButton);

        var mainStack = new StackLayout
        {
            Padding = 20,
            Spacing = 15
        };
        mainStack.Children.Add(new Label { Text = "Заголовок:", FontAttributes = FontAttributes.Bold });
        mainStack.Children.Add(titleEntry);
        mainStack.Children.Add(new Label { Text = "Описание:", FontAttributes = FontAttributes.Bold });
        mainStack.Children.Add(messageEditor);
        mainStack.Children.Add(new Label { Text = "Дата:", FontAttributes = FontAttributes.Bold });
        mainStack.Children.Add(datePicker);
        mainStack.Children.Add(new Label { Text = "Время:", FontAttributes = FontAttributes.Bold });
        mainStack.Children.Add(timePicker);
        mainStack.Children.Add(buttonStack);

        editPage.Content = new ScrollView { Content = mainStack };

        bool result = false;

        saveButton.Clicked += (s, e) =>
        {
            if (string.IsNullOrWhiteSpace(titleEntry.Text))
            {
                DisplayAlert("Ошибка", "Введите заголовок напоминания", "OK");
                return;
            }

            var scheduledDateTime = datePicker.Date.Add(timePicker.Time);
            if (scheduledDateTime <= DateTime.Now)
            {
                DisplayAlert("Ошибка", "Время напоминания должно быть в будущем", "OK");
                return;
            }

            reminder.Title = titleEntry.Text.Trim();
            reminder.Message = messageEditor.Text?.Trim() ?? "";
            reminder.ScheduledTime = scheduledDateTime;

            result = true;
            Navigation.PopModalAsync();
        };

        cancelButton.Clicked += (s, e) => Navigation.PopModalAsync();

        await Navigation.PushModalAsync(editPage);
        return result;
    }

    // Удаление напоминания
    private async Task DeleteReminder(Reminder reminder)
    {
        try
        {
            // Отменяем уведомление в системе
            if (_notificationIds.ContainsKey(reminder.Id))
            {
                await _notificationService.CancelNotification(_notificationIds[reminder.Id]);
                _notificationIds.Remove(reminder.Id);
            }

            // Удаляем из коллекции
            _reminders.Remove(reminder);

            // TODO: Удалить из базы данных
            // await DeleteReminderFromStorage(reminder.Id);

            await DisplayAlert("Успех", "Напоминание удалено", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось удалить напоминание: {ex.Message}", "OK");
        }
    }

    // Очистка всех напоминаний
    private async Task ClearAllReminders()
    {
        try
        {
            // Отменяем все уведомления
            await _notificationService.CancelAllNotifications();

            // Очищаем коллекции
            _reminders.Clear();
            _notificationIds.Clear();

            // TODO: Очистить базу данных
            // await ClearAllRemindersFromStorage();

            await DisplayAlert("Успех", "Все напоминания удалены", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось очистить напоминания: {ex.Message}", "OK");
        }
    }
}