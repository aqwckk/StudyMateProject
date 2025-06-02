using System;
using System.Diagnostics;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using StudyMateProject.Services;

namespace StudyMateProject.Views
{
    /// <summary>
    /// Базовый класс для всех страниц приложения
    /// Добавляет поддержку справки и кнопку "Помощь" в верхнюю панель
    /// </summary>
    public class BaseContentPage : ContentPage
    {
        protected HelpService _helpService;

        public BaseContentPage()
        {
            // Добавляем кнопку "Помощь" в тулбар каждой страницы
            ToolbarItems.Add(new ToolbarItem
            {
                Text = "Помощь",
                Command = new Command(ShowHelp)
            });

            // Получаем сервис справки
            Loaded += (s, e) => {
                if (_helpService == null && Application.Current != null &&
                    Application.Current.Handler != null &&
                    Application.Current.Handler.MauiContext != null)
                {
                    _helpService = Application.Current.Handler.MauiContext.Services.GetService<HelpService>();
                }
            };
        }

        /// <summary>
        /// Показать справку для текущей страницы
        /// </summary>
        protected void ShowHelp()
        {
            if (_helpService == null && Application.Current != null &&
                Application.Current.Handler != null &&
                Application.Current.Handler.MauiContext != null)
            {
                _helpService = Application.Current.Handler.MauiContext.Services.GetService<HelpService>();
            }

            if (_helpService != null)
            {
                string currentPage = GetType().Name;
                _helpService.ShowContextHelp(currentPage);
            }
            else
            {
                DisplayAlert("Ошибка", "Не удалось загрузить справку", "OK");
            }
        }
    }
}