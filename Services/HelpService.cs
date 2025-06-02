using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Maui.Controls;

namespace StudyMateProject.Services
{
    /// <summary>
    /// Сервис для работы со справочной системой (только для Windows)
    /// </summary>
    public class HelpService
    {
        private const string HelpFileName = "StudyMateHelp.chm";

        // Соответствие страниц приложения темам справки
        private readonly Dictionary<string, int> _contextMap = new Dictionary<string, int>
        {
            { "NotesPage", 100 },
            { "TextNotePage", 200 },
            { "GraphicNotePage", 300 },
            { "RemindersPage", 400 },
            { "CalculatorPage", 500 },
            { "SettingsPage", 600 },
            { "HelpPage", 700 }
        };

        /// <summary>
        /// Показать общую справку по приложению
        /// </summary>
        public void ShowHelp()
        {
            try
            {
                // Получаем путь к файлу справки
                string appDir = AppDomain.CurrentDomain.BaseDirectory;
                var helpFilePath = Path.Combine(appDir, HelpFileName);

                // Проверяем существование файла
                if (File.Exists(helpFilePath))
                {
                    // Открываем файл справки
                    if (DeviceInfo.Platform == DevicePlatform.WinUI)
                    {
                        // Для Windows используем Process.Start
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = helpFilePath,
                            UseShellExecute = true
                        });
                    }
                    else
                    {
                        // Для других платформ показываем сообщение
                        MainThread.BeginInvokeOnMainThread(async () =>
                        {
                            await Application.Current.MainPage.DisplayAlert(
                                "Информация",
                                "Справка доступна только в Windows-версии приложения",
                                "OK");
                        });
                    }
                }
                else
                {
                    // Файл не найден
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await Application.Current.MainPage.DisplayAlert(
                            "Ошибка",
                            $"Файл справки не найден: {helpFilePath}",
                            "OK");
                    });
                }
            }
            catch (Exception ex)
            {
                // Обработка ошибок
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Ошибка",
                        $"Не удалось открыть справку: {ex.Message}",
                        "OK");
                });
            }
        }

        /// <summary>
        /// Показать контекстную справку для указанной страницы
        /// </summary>
        /// <param name="context">Имя класса страницы</param>
        public void ShowContextHelp(string context)
        {
            try
            {
                string appDir = AppDomain.CurrentDomain.BaseDirectory;
                var helpFilePath = Path.Combine(appDir, HelpFileName);

                if (File.Exists(helpFilePath))
                {
                    if (DeviceInfo.Platform == DevicePlatform.WinUI)
                    {
                        // Для Windows
                        if (_contextMap.ContainsKey(context))
                        {
                            // Если для страницы есть ID темы
                            int contextId = _contextMap[context];

                            try
                            {
                                // Пытаемся вызвать справку с контекстным ID
                                Process.Start(new ProcessStartInfo
                                {
                                    FileName = helpFilePath,
                                    UseShellExecute = true
                                });
                            }
                            catch
                            {
                                // Если не получилось, просто открываем файл справки
                                Process.Start(new ProcessStartInfo
                                {
                                    FileName = helpFilePath,
                                    UseShellExecute = true
                                });
                            }
                        }
                        else
                        {
                            // Если для страницы нет ID темы, открываем общую справку
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = helpFilePath,
                                UseShellExecute = true
                            });
                        }
                    }
                    else
                    {
                        // Для других платформ
                        MainThread.BeginInvokeOnMainThread(async () =>
                        {
                            await Application.Current.MainPage.DisplayAlert(
                                "Информация",
                                "Справка доступна только в Windows-версии приложения",
                                "OK");
                        });
                    }
                }
                else
                {
                    // Файл не найден
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await Application.Current.MainPage.DisplayAlert(
                            "Ошибка",
                            $"Файл справки не найден: {helpFilePath}",
                            "OK");
                    });
                }
            }
            catch (Exception ex)
            {
                // Обработка ошибок
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Ошибка",
                        $"Не удалось открыть справку: {ex.Message}",
                        "OK");
                });
            }
        }
    }
}