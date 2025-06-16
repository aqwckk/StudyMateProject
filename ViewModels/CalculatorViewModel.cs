using System.ComponentModel;
using System.Runtime.CompilerServices;
using StudyMateTest.Models.Calculator;
using StudyMateTest.Services.CalculatorServices;

namespace StudyMateTest.ViewModels
{
    partial class CalculatorViewModel : INotifyPropertyChanged
    {
        private readonly ICalculatorService _calculatorService;
        private readonly CalculatorModel _calculatorModel;

        public CalculatorViewModel()
        {
            _calculatorService = new CalculatorService();
            _calculatorModel = new CalculatorModel();
        }

        public CalculatorViewModel(ICalculatorService calculatorService)
        {
            _calculatorService = calculatorService;
            _calculatorModel = new CalculatorModel();
        }

        #region Properties

        public string DisplayValue
        {
            get => _calculatorModel.DisplayValue;
            set
            {
                _calculatorModel.DisplayValue = value;
                OnPropertyChanged();
            }
        }

        public string CurrentExpression
        {
            get => _calculatorModel.CurrentExpression;
            set
            {
                _calculatorModel.CurrentExpression = value;
                OnPropertyChanged();
            }
        }

        public double LastResult
        {
            get => _calculatorModel.LastResult;
            set
            {
                _calculatorModel.LastResult = value;
                OnPropertyChanged();
            }
        }

        public bool IsNewCalculation
        {
            get => _calculatorModel.IsNewCalculation;
            set
            {
                _calculatorModel.IsNewCalculation = value;
                OnPropertyChanged();
            }
        }

        public bool HasDecimalPoint
        {
            get => _calculatorModel.HasDecimalPoint;
            set
            {
                _calculatorModel.HasDecimalPoint = value;
                OnPropertyChanged();
            }
        }

        public List<string> History => _calculatorModel.History;

        #endregion

        #region Public Methods

        /// Обработка ввода цифры
        /// <param name="number">Цифра в виде строки</param>
        public void InputNumber(string number)
        {
            if (IsNewCalculation)
            {
                DisplayValue = number == "0" ? "0" : number;
                CurrentExpression = number;
                IsNewCalculation = false;
            }
            else
            {
                if (DisplayValue == "0")
                {
                    DisplayValue = number;
                    CurrentExpression = number;
                }
                else
                {
                    DisplayValue += number;
                    CurrentExpression += number;
                }
            }
        }

        
        /// Обработка ввода оператора

        /// <param name="operatorSymbol">Символ оператора</param>
        public void InputOperator(string operatorSymbol)
        {
            // Конвертируем отображаемые символы в математические
            string mathOperator = ConvertOperatorSymbol(operatorSymbol);

            if (!IsNewCalculation)
            {
                CurrentExpression += $" {mathOperator} ";
                _calculatorModel.LastOperation = mathOperator;
                _calculatorModel.WaitingForOperand = true;
                HasDecimalPoint = false;
            }
            else if (LastResult != 0)
            {
                CurrentExpression = $"{LastResult} {mathOperator} ";
                IsNewCalculation = false;
                _calculatorModel.LastOperation = mathOperator;
                _calculatorModel.WaitingForOperand = true;
            }
        }

        
        /// Обработка ввода десятичной точки

        public void InputDecimal()
        {
            if (HasDecimalPoint)
                return;

            if (IsNewCalculation)
            {
                DisplayValue = "0.";
                CurrentExpression = "0.";
                IsNewCalculation = false;
            }
            else
            {
                DisplayValue += ".";
                CurrentExpression += ".";
            }

            HasDecimalPoint = true;
        }

        
        /// Выполнение вычисления

        /// <returns>Результат вычисления или сообщение об ошибке</returns>
        public CalculationResult Calculate()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(CurrentExpression) || CurrentExpression == "0")
                    return new CalculationResult { IsSuccess = false, ErrorMessage = "Нет выражения для вычисления" };

                if (!_calculatorService.IsValidExpression(CurrentExpression))
                    return new CalculationResult { IsSuccess = false, ErrorMessage = "Некорректное выражение" };

                double result = _calculatorService.Calculate(CurrentExpression);
                string formattedResult = _calculatorService.FormatResult(result);

                // Добавляем в историю
                string historyEntry = $"{CurrentExpression} = {formattedResult}";
                _calculatorModel.AddToHistory(historyEntry);

                // Обновляем состояние
                LastResult = result;
                DisplayValue = formattedResult;
                IsNewCalculation = true;
                HasDecimalPoint = formattedResult.Contains(".");
                _calculatorModel.WaitingForOperand = false;

                return new CalculationResult
                {
                    IsSuccess = true,
                    Result = result,
                    FormattedResult = formattedResult
                };
            }
            catch (Exception ex)
            {
                return new CalculationResult
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        
        /// Вычисление квадратного корня

        /// <returns>Результат вычисления или сообщение об ошибке</returns>
        public CalculationResult CalculateSquareRoot()
        {
            try
            {
                if (!double.TryParse(DisplayValue, out double value))
                    return new CalculationResult { IsSuccess = false, ErrorMessage = "Некорректное число" };

                double result = _calculatorService.CalculateSquareRoot(value);
                string formattedResult = _calculatorService.FormatResult(result);

                // Добавляем в историю
                string historyEntry = $"√{value} = {formattedResult}";
                _calculatorModel.AddToHistory(historyEntry);

                // Обновляем состояние
                LastResult = result;
                DisplayValue = formattedResult;
                CurrentExpression = formattedResult;
                IsNewCalculation = true;
                HasDecimalPoint = formattedResult.Contains(".");

                return new CalculationResult
                {
                    IsSuccess = true,
                    Result = result,
                    FormattedResult = formattedResult
                };
            }
            catch (Exception ex)
            {
                return new CalculationResult
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        
        /// Полная очистка калькулятора

        public void Clear()
        {
            _calculatorModel.Reset();
            OnPropertyChanged(nameof(DisplayValue));
            OnPropertyChanged(nameof(CurrentExpression));
            OnPropertyChanged(nameof(LastResult));
            OnPropertyChanged(nameof(IsNewCalculation));
            OnPropertyChanged(nameof(HasDecimalPoint));
        }

        
        /// Очистка последнего ввода

        public void ClearEntry()
        {
            DisplayValue = "0";
            HasDecimalPoint = false;

            // Если мы в середине выражения, удаляем последний операнд
            if (!IsNewCalculation && _calculatorModel.WaitingForOperand)
            {
                int lastOperatorIndex = Math.Max(
                    Math.Max(CurrentExpression.LastIndexOf('+'),
                            CurrentExpression.LastIndexOf('-')),
                    Math.Max(CurrentExpression.LastIndexOf('*'),
                            CurrentExpression.LastIndexOf('/'))
                );

                if (lastOperatorIndex > 0)
                {
                    CurrentExpression = CurrentExpression.Substring(0, lastOperatorIndex + 2);
                }
            }
        }

        
        /// Получить историю вычислений

        /// <returns>Список строк с историей</returns>
        public List<string> GetHistory()
        {
            return new List<string>(History);
        }

        
        /// Очистить историю вычислений

        public void ClearHistory()
        {
            History.Clear();
        }

        #endregion

        #region Private Methods

        private string ConvertOperatorSymbol(string displaySymbol)
        {
            return displaySymbol switch
            {
                "÷" => "/",
                "×" => "*",
                "−" => "-",
                "+" => "+",
                "%" => "%",
                _ => displaySymbol
            };
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    /// Результат вычисления
    public class CalculationResult
    {
        public bool IsSuccess { get; set; }
        public double Result { get; set; }
        public string FormattedResult { get; set; }
        public string ErrorMessage { get; set; }
    }
}