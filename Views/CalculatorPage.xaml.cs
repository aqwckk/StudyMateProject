using Microsoft.Maui.Controls;
using System;
using StudyMateTest.Models.Calculator;
using StudyMateTest.Services.CalculatorServices;

namespace StudyMateTest.Views
{
    public partial class CalculatorPage : ContentPage
    {
        private readonly ICalculatorService _calculatorService;
        private readonly CalculatorModel _calculatorModel;
        private string _currentExpression = "";
        private bool _justCalculated = false;
        private bool _isScientificMode = false;
        private bool _isMatrixMode = false;

        public CalculatorPage()
        {
            InitializeComponent();
            _calculatorService = new CalculatorService();
            _calculatorModel = new CalculatorModel();

            // Подписываемся на изменение размера Label
            DisplayLabel.SizeChanged += OnDisplayLabelSizeChanged;

            UpdateDisplay();
        }

        private void OnDisplayLabelSizeChanged(object sender, EventArgs e)
        {
            try
            {
                // Прокручиваем в конец при изменении размера
                Dispatcher.Dispatch(async () =>
                {
                    try
                    {
                        await Task.Delay(50);
                        await DisplayScrollView.ScrollToAsync(DisplayLabel, ScrollToPosition.End, false);
                    }
                    catch { /* Игнорируем ошибки прокрутки */ }
                });
            }
            catch { /* Игнорируем все ошибки */ }
        }

        #region Mode Switching
        private void OnToggleCalculatorModeClicked(object sender, EventArgs e)
        {
            try
            {
                CalculatorSelectorOverlay.IsVisible = true;
                CalculatorSelectorOverlay.FadeTo(1, 250, Easing.CubicOut);
            }
            catch { /* Игнорируем ошибки */ }
        }

        private async void OnBasicCalculatorSelected(object sender, EventArgs e)
        {
            try
            {
                await CalculatorSelectorOverlay.FadeTo(0, 200, Easing.CubicIn);
                CalculatorSelectorOverlay.IsVisible = false;
                SwitchToBasicMode();
            }
            catch { /* Игнорируем ошибки */ }
        }

        private async void OnScientificCalculatorSelected(object sender, EventArgs e)
        {
            try
            {
                await CalculatorSelectorOverlay.FadeTo(0, 200, Easing.CubicIn);
                CalculatorSelectorOverlay.IsVisible = false;
                SwitchToScientificMode();
            }
            catch { /* Игнорируем ошибки */ }
        }

        private async void OnCancelSelection(object sender, EventArgs e)
        {
            try
            {
                await CalculatorSelectorOverlay.FadeTo(0, 200, Easing.CubicIn);
                CalculatorSelectorOverlay.IsVisible = false;
            }
            catch { /* Игнорируем ошибки */ }
        }

        private void SwitchToBasicMode()
        {
            try
            {
                BasicCalculatorPanel.IsVisible = true;
                ScientificCalculatorPanel.IsVisible = false;
                MatrixCalculatorPanel.IsVisible = false;
                _isScientificMode = false;
                _isMatrixMode = false;
            }
            catch { /* Игнорируем ошибки */ }
        }

        private void SwitchToScientificMode()
        {
            try
            {
                BasicCalculatorPanel.IsVisible = false;
                ScientificCalculatorPanel.IsVisible = true;
                MatrixCalculatorPanel.IsVisible = false;
                _isScientificMode = true;
                _isMatrixMode = false;
            }
            catch { /* Игнорируем ошибки */ }
        }

        private async void OnMatrixCalculatorSelected(object sender, EventArgs e)
        {
            try
            {
                await CalculatorSelectorOverlay.FadeTo(0, 200, Easing.CubicIn);
                CalculatorSelectorOverlay.IsVisible = false;
                SwitchToMatrixMode();
            }
            catch { /* Игнорируем ошибки */ }
        }

        private void SwitchToMatrixMode()
        {
            try
            {
                BasicCalculatorPanel.IsVisible = false;
                ScientificCalculatorPanel.IsVisible = false;
                MatrixCalculatorPanel.IsVisible = true;
                _isScientificMode = false;
                _isMatrixMode = true;
            }
            catch { /* Игнорируем ошибки */ }
        }
        #endregion

        #region Basic Operations
        private void OnNumberClicked(object sender, EventArgs e)
        {
            try
            {
                if (sender is not Button button) return;
                string number = button.Text;

                if (_justCalculated)
                {
                    _currentExpression = number;
                    _justCalculated = false;
                }
                else
                {
                    _currentExpression += number;
                }

                UpdateDisplay();
            }
            catch
            {
                ShowError();
            }
        }

        private void OnOperatorClicked(object sender, EventArgs e)
        {
            try
            {
                if (sender is not Button button) return;
                string operatorSymbol = button.Text;

                string mathOperator = operatorSymbol switch
                {
                    "÷" => " ÷ ",
                    "×" => " × ",
                    "−" => " − ",
                    "+" => " + ",
                    _ => " " + operatorSymbol + " "
                };

                if (_justCalculated)
                {
                    _currentExpression = _calculatorService.FormatResult(_calculatorModel.LastResult) + mathOperator;
                    _justCalculated = false;
                }
                else if (!string.IsNullOrEmpty(_currentExpression))
                {
                    if (_currentExpression.TrimEnd().EndsWith(" "))
                    {
                        var trimmed = _currentExpression.TrimEnd();
                        var lastSpaceIndex = trimmed.LastIndexOf(' ');
                        if (lastSpaceIndex > 0)
                        {
                            _currentExpression = trimmed.Substring(0, lastSpaceIndex) + mathOperator;
                        }
                    }
                    else
                    {
                        _currentExpression += mathOperator;
                    }
                }
                else if (_calculatorModel.LastResult != 0)
                {
                    _currentExpression = _calculatorService.FormatResult(_calculatorModel.LastResult) + mathOperator;
                }

                UpdateDisplay();
            }
            catch
            {
                ShowError();
            }
        }

        private void OnDecimalClicked(object sender, EventArgs e)
        {
            try
            {
                if (_justCalculated)
                {
                    _currentExpression = "0.";
                    _justCalculated = false;
                }
                else
                {
                    var parts = _currentExpression.Split(new[] { " + ", " − ", " × ", " ÷ " }, StringSplitOptions.None);
                    string lastPart = parts[^1].Trim();

                    if (!lastPart.Contains("."))
                    {
                        if (string.IsNullOrEmpty(lastPart) || lastPart.EndsWith("("))
                        {
                            _currentExpression += "0.";
                        }
                        else
                        {
                            _currentExpression += ".";
                        }
                    }
                }

                UpdateDisplay();
            }
            catch
            {
                ShowError();
            }
        }

        private void OnPercentClicked(object sender, EventArgs e)
        {
            try
            {
                if (_justCalculated)
                {
                    double percentValue = _calculatorModel.LastResult / 100;
                    _currentExpression = _calculatorService.FormatResult(percentValue);
                    _justCalculated = false;
                }
                else if (!string.IsNullOrEmpty(_currentExpression))
                {
                    var parts = _currentExpression.Split(new[] { " + ", " − ", " × ", " ÷ " }, StringSplitOptions.None);
                    string lastPart = parts[^1].Trim();

                    if (double.TryParse(lastPart, out double number))
                    {
                        double percentValue = number / 100;
                        string formattedPercent = _calculatorService.FormatResult(percentValue);

                        if (parts.Length > 1)
                        {
                            parts[^1] = formattedPercent;
                            _currentExpression = string.Join(" ", parts).Replace("  ", " ÷ ").Replace("  ", " × ").Replace("  ", " − ").Replace("  ", " + ");
                        }
                        else
                        {
                            _currentExpression = formattedPercent;
                        }
                    }
                }

                UpdateDisplay();
            }
            catch
            {
                ShowError();
            }
        }

        private void OnEqualsClicked(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_currentExpression))
                    return;

                string expression = _currentExpression.Trim();

                // Убираем оператор в конце, если есть
                while (expression.EndsWith(" + ") || expression.EndsWith(" − ") ||
                       expression.EndsWith(" × ") || expression.EndsWith(" ÷ "))
                {
                    var lastSpaceIndex = expression.TrimEnd().LastIndexOf(' ');
                    if (lastSpaceIndex > 0)
                    {
                        expression = expression.Substring(0, lastSpaceIndex);
                    }
                    else
                    {
                        break;
                    }
                }

                if (!_calculatorService.IsValidExpression(expression))
                {
                    ShowError();
                    return;
                }

                double result = _calculatorService.Calculate(expression);

                if (double.IsNaN(result) || double.IsInfinity(result))
                {
                    ShowError();
                    return;
                }

                string formattedResult = _calculatorService.FormatResult(result);

                string historyEntry = $"{_currentExpression.Trim()} = {formattedResult}";
                _calculatorModel.AddToHistory(historyEntry);

                _calculatorModel.LastResult = result;
                _currentExpression = formattedResult;
                _justCalculated = true;

                UpdateDisplay();
            }
            catch
            {
                ShowError();
            }
        }

        private void OnSquareRootClicked(object sender, EventArgs e)
        {
            try
            {
                if (_justCalculated)
                {
                    _currentExpression = "√(";
                    _justCalculated = false;
                }
                else
                {
                    _currentExpression += "√(";
                }

                UpdateDisplay();
            }
            catch
            {
                ShowError();
            }
        }

        private void OnOpenParenthesisClicked(object sender, EventArgs e)
        {
            try
            {
                if (_justCalculated)
                {
                    _currentExpression = "(";
                    _justCalculated = false;
                }
                else
                {
                    _currentExpression += "(";
                }

                UpdateDisplay();
            }
            catch
            {
                ShowError();
            }
        }

        private void OnCloseParenthesisClicked(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(_currentExpression) &&
                    !_currentExpression.TrimEnd().EndsWith(" ") &&
                    !_currentExpression.EndsWith("(") &&
                    CountOpenParentheses() > CountCloseParentheses())
                {
                    _currentExpression += ")";
                    UpdateDisplay();
                }
            }
            catch
            {
                ShowError();
            }
        }

        private void OnClearClicked(object sender, EventArgs e)
        {
            try
            {
                _currentExpression = "";
                _justCalculated = false;
                _calculatorModel.Reset();
                UpdateDisplay();
            }
            catch
            {
                ShowError();
            }
        }

        private void OnClearEntryClicked(object sender, EventArgs e)
        {
            try
            {
                if (_justCalculated)
                {
                    OnClearClicked(sender, e);
                }
                else
                {
                    if (_currentExpression.TrimEnd().EndsWith(" "))
                    {
                        var trimmed = _currentExpression.TrimEnd();
                        var lastSpaceIndex = trimmed.LastIndexOf(' ');
                        if (lastSpaceIndex > 0)
                        {
                            _currentExpression = trimmed.Substring(0, lastSpaceIndex + 1);
                        }
                        else
                        {
                            _currentExpression = "";
                        }
                    }
                    else
                    {
                        var parts = _currentExpression.Split(new[] { " + ", " − ", " × ", " ÷ " }, StringSplitOptions.None);
                        if (parts.Length > 1)
                        {
                            parts[^1] = "";
                            _currentExpression = string.Join(" ", parts);
                            _currentExpression = _currentExpression.TrimEnd() + " ";
                        }
                        else
                        {
                            _currentExpression = "";
                        }
                    }

                    UpdateDisplay();
                }
            }
            catch
            {
                ShowError();
            }
        }

        private void OnBackspaceClicked(object sender, EventArgs e)
        {
            try
            {
                if (_justCalculated)
                {
                    _justCalculated = false;
                    if (_currentExpression.Length > 1)
                    {
                        _currentExpression = _currentExpression[..^1];
                    }
                    else
                    {
                        _currentExpression = "";
                    }
                }
                else if (!string.IsNullOrEmpty(_currentExpression))
                {
                    // Умное удаление для научных функций
                    if (_currentExpression.EndsWith("sin(") || _currentExpression.EndsWith("cos(") ||
                        _currentExpression.EndsWith("tan(") || _currentExpression.EndsWith("cot(") ||
                        _currentExpression.EndsWith("log("))
                    {
                        _currentExpression = _currentExpression[..^4];
                    }
                    else if (_currentExpression.EndsWith("ln("))
                    {
                        _currentExpression = _currentExpression[..^3];
                    }
                    else if (_currentExpression.EndsWith("√(") || _currentExpression.EndsWith("∛("))
                    {
                        _currentExpression = _currentExpression[..^2];
                    }
                    else if (_currentExpression.EndsWith(" + ") || _currentExpression.EndsWith(" − ") ||
                             _currentExpression.EndsWith(" × ") || _currentExpression.EndsWith(" ÷ "))
                    {
                        _currentExpression = _currentExpression[..^3];
                    }
                    else
                    {
                        _currentExpression = _currentExpression[..^1];
                    }
                }

                UpdateDisplay();
            }
            catch
            {
                ShowError();
            }
        }
        #endregion

        #region Scientific Functions

        // Тригонометрические функции
        private void OnScientificFunctionClicked(object sender, EventArgs e)
        {
            try
            {
                if (sender is not Button button) return;
                string function = button.Text;

                if (_justCalculated)
                {
                    _currentExpression = $"{function}(";
                    _justCalculated = false;
                }
                else
                {
                    _currentExpression += $"{function}(";
                }

                UpdateDisplay();
            }
            catch
            {
                ShowError();
            }
        }

        // Степенные функции
        private void OnSquareClicked(object sender, EventArgs e)
        {
            try
            {
                if (_justCalculated)
                {
                    _currentExpression = $"{_calculatorService.FormatResult(_calculatorModel.LastResult)}²";
                    _justCalculated = false;
                }
                else if (!string.IsNullOrEmpty(_currentExpression))
                {
                    if (_currentExpression.EndsWith(")"))
                    {
                        _currentExpression += "²";
                    }
                    else
                    {
                        var parts = _currentExpression.Split(new[] { " + ", " − ", " × ", " ÷ " }, StringSplitOptions.None);
                        string lastPart = parts[^1].Trim();

                        if (double.TryParse(lastPart, out _) || lastPart == "π" || lastPart == "e")
                        {
                            parts[^1] = lastPart + "²";
                            _currentExpression = string.Join(" ", parts).Replace("  ", " ÷ ").Replace("  ", " × ").Replace("  ", " − ").Replace("  ", " + ");
                        }
                        else
                        {
                            _currentExpression += "²";
                        }
                    }
                }
                UpdateDisplay();
            }
            catch
            {
                ShowError();
            }
        }

        private void OnCubeClicked(object sender, EventArgs e)
        {
            try
            {
                if (_justCalculated)
                {
                    _currentExpression = $"{_calculatorService.FormatResult(_calculatorModel.LastResult)}³";
                    _justCalculated = false;
                }
                else if (!string.IsNullOrEmpty(_currentExpression))
                {
                    if (_currentExpression.EndsWith(")"))
                    {
                        _currentExpression += "³";
                    }
                    else
                    {
                        var parts = _currentExpression.Split(new[] { " + ", " − ", " × ", " ÷ " }, StringSplitOptions.None);
                        string lastPart = parts[^1].Trim();

                        if (double.TryParse(lastPart, out _) || lastPart == "π" || lastPart == "e")
                        {
                            parts[^1] = lastPart + "³";
                            _currentExpression = string.Join(" ", parts).Replace("  ", " ÷ ").Replace("  ", " × ").Replace("  ", " − ").Replace("  ", " + ");
                        }
                        else
                        {
                            _currentExpression += "³";
                        }
                    }
                }
                UpdateDisplay();
            }
            catch
            {
                ShowError();
            }
        }

        private void OnPowerYClicked(object sender, EventArgs e)
        {
            try
            {
                if (_justCalculated)
                {
                    _currentExpression = _calculatorService.FormatResult(_calculatorModel.LastResult) + "^";
                    _justCalculated = false;
                }
                else if (!string.IsNullOrEmpty(_currentExpression) &&
                         !_currentExpression.TrimEnd().EndsWith("^"))
                {
                    _currentExpression += "^";
                }
                UpdateDisplay();
            }
            catch
            {
                ShowError();
            }
        }

        private void OnFactorialClicked(object sender, EventArgs e)
        {
            try
            {
                if (_justCalculated)
                {
                    _currentExpression = "fact(";
                    _justCalculated = false;
                }
                else
                {
                    _currentExpression += "fact(";
                }
                UpdateDisplay();
            }
            catch
            {
                ShowError();
            }
        }

        // Корни
        private void OnCubeRootClicked(object sender, EventArgs e)
        {
            try
            {
                if (_justCalculated)
                {
                    _currentExpression = "∛(";
                    _justCalculated = false;
                }
                else
                {
                    _currentExpression += "∛(";
                }
                UpdateDisplay();
            }
            catch
            {
                ShowError();
            }
        }

        // Константы
        private void OnPiClicked(object sender, EventArgs e)
        {
            try
            {
                if (_justCalculated)
                {
                    _currentExpression = "π";
                    _justCalculated = false;
                }
                else
                {
                    _currentExpression += "π";
                }
                UpdateDisplay();
            }
            catch
            {
                ShowError();
            }
        }

        private void OnEClicked(object sender, EventArgs e)
        {
            try
            {
                if (_justCalculated)
                {
                    _currentExpression = "e";
                    _justCalculated = false;
                }
                else
                {
                    _currentExpression += "e";
                }
                UpdateDisplay();
            }
            catch
            {
                ShowError();
            }
        }

        #endregion

        #region Helper Methods
        private int CountOpenParentheses()
        {
            try
            {
                int count = 0;
                foreach (char c in _currentExpression)
                {
                    if (c == '(') count++;
                }
                return count;
            }
            catch
            {
                return 0;
            }
        }

        private int CountCloseParentheses()
        {
            try
            {
                int count = 0;
                foreach (char c in _currentExpression)
                {
                    if (c == ')') count++;
                }
                return count;
            }
            catch
            {
                return 0;
            }
        }

        private void UpdateDisplay()
        {
            try
            {
                string text = string.IsNullOrEmpty(_currentExpression) ? "0" : _currentExpression;
                DisplayLabel.Text = text;

                // Автоматическое уменьшение шрифта
                if (text.Length > 20)
                    DisplayLabel.FontSize = 24;
                else if (text.Length > 15)
                    DisplayLabel.FontSize = 32;
                else if (text.Length > 10)
                    DisplayLabel.FontSize = 36;
                else
                    DisplayLabel.FontSize = 42;

                // Прокручиваем к правому краю
                Dispatcher.Dispatch(async () =>
                {
                    try
                    {
                        await Task.Delay(50);
                        await DisplayScrollView.ScrollToAsync(2000, 0, false); // Прокрутка к концу
                        await ExpressionScrollView.ScrollToAsync(2000, 0, false);
                    }
                    catch { /* Игнорируем ошибки прокрутки */ }
                });

                if (_justCalculated)
                {
                    ExpressionLabel.Text = "Результат";
                }
                else
                {
                    ExpressionLabel.Text = string.IsNullOrEmpty(_currentExpression) ? "Введите выражение" : "Ввод...";
                }
            }
            catch
            {
                ShowError();
            }
        }

        private void ShowError()
        {
            try
            {
                DisplayLabel.Text = "Ошибка";
                ExpressionLabel.Text = "Ошибка";
                _currentExpression = "";
                _justCalculated = false;
                _calculatorModel.Reset();
            }
            catch { /* Даже обработка ошибок может упасть */ }
        }
        #endregion
    }
}