namespace StudyMateTest.Models
{
    public class Reminder
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime ScheduledTime { get; set; } = DateTime.Now.AddHours(1);
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public Dictionary<string, string> Metadata { get; set; } = new();

        // Автоматическое вычисление статуса на основе времени
        public bool IsCurrentlyActive => IsActive && ScheduledTime > DateTime.Now;

        // Проверка что напоминание уже сработало
        public bool HasTriggered => ScheduledTime <= DateTime.Now;

        // Отображение времени для UI
        public string DisplayTime
        {
            get
            {
                var now = DateTime.Now;
                var timeDiff = ScheduledTime - now;

                if (HasTriggered)
                {
                    // Напоминание уже прошло
                    var passedTime = now - ScheduledTime;
                    if (passedTime.TotalMinutes < 60)
                        return $"Сработало {(int)passedTime.TotalMinutes} мин назад";
                    else if (passedTime.TotalHours < 24)
                        return $"Сработало {(int)passedTime.TotalHours} ч назад";
                    else
                        return $"Сработало {(int)passedTime.TotalDays} дн назад";
                }
                else if (timeDiff.TotalMinutes < 60)
                {
                    return $"Через {(int)timeDiff.TotalMinutes} мин";
                }
                else if (timeDiff.TotalHours < 24)
                {
                    return $"Через {(int)timeDiff.TotalHours} ч";
                }
                else if (timeDiff.TotalDays < 7)
                {
                    return $"Через {(int)timeDiff.TotalDays} дн";
                }
                else
                {
                    return ScheduledTime.ToString("dd.MM.yyyy в HH:mm");
                }
            }
        }

        // Статус для отображения
        public string StatusText
        {
            get
            {
                if (HasTriggered)
                    return "Выполнено";
                else if (IsCurrentlyActive)
                    return "Активно";
                else
                    return "Неактивно";
            }
        }

        // Цвет статуса
        public string StatusColor
        {
            get
            {
                if (HasTriggered)
                    return "#6B7280"; // Серый - выполнено
                else if (IsCurrentlyActive)
                    return "#10B981"; // Зеленый - активно
                else
                    return "#F59E0B"; // Желтый - неактивно
            }
        }

        // Фоновый цвет статуса
        public string StatusBackgroundColor
        {
            get
            {
                if (HasTriggered)
                    return "#F3F4F6"; // Светло-серый
                else if (IsCurrentlyActive)
                    return "#ECFDF5"; // Светло-зеленый
                else
                    return "#FEF3C7"; // Светло-желтый
            }
        }

        // Метод для обновления статуса (вызывается периодически)
        public void UpdateStatus()
        {
            if (HasTriggered && IsActive)
            {
                IsActive = false; // Автоматически деактивируем сработавшие напоминания
            }
        }
    }
}