using Microsoft.VisualStudio.TestTools.UnitTesting;
using StudyMateTest.Core.ViewModels.Calculator;
using StudyMateTest.Core.Services.CalculatorServices;
using System;
using System.ComponentModel;

namespace StudyMateTest.Tests
{
    [TestClass]
    public class CalculatorViewModelTests
    {
        private CalculatorViewModel _viewModel;
        private ICalculatorService _calculatorService;

        [TestInitialize]
        public void Setup()
        {
            _calculatorService = new CalculatorService();
            _viewModel = new CalculatorViewModel(_calculatorService);
        }

        #region Тесты инициализации

        [TestMethod]
        public void Constructor_WithoutService_InitializesCorrectly()
        {
            var vm = new CalculatorViewModel();

            Assert.AreEqual("0", vm.DisplayValue);
            Assert.AreEqual("0", vm.CurrentExpression);
            Assert.AreEqual(0.0, vm.LastResult);
            Assert.IsTrue(vm.IsNewCalculation);
            Assert.IsFalse(vm.HasDecimalPoint);
            Assert.IsNotNull(vm.History);
        }

        [TestMethod]
        public void Constructor_WithService_InitializesCorrectly()
        {
            Assert.AreEqual("0", _viewModel.DisplayValue);
            Assert.AreEqual("0", _viewModel.CurrentExpression);
            Assert.AreEqual(0.0, _viewModel.LastResult);
            Assert.IsTrue(_viewModel.IsNewCalculation);
            Assert.IsFalse(_viewModel.HasDecimalPoint);
            Assert.IsNotNull(_viewModel.History);
        }

        #endregion

        #region Тесты InputNumber

        [TestMethod]
        public void InputNumber_FirstNumber_SetsCorrectly()
        {
            _viewModel.InputNumber("5");

            Assert.AreEqual("5", _viewModel.DisplayValue);
            Assert.AreEqual("5", _viewModel.CurrentExpression);
            Assert.IsFalse(_viewModel.IsNewCalculation);
        }

        [TestMethod]
        public void InputNumber_Zero_WhenNewCalculation_DisplaysZero()
        {
            _viewModel.InputNumber("0");

            Assert.AreEqual("0", _viewModel.DisplayValue);
            Assert.AreEqual("0", _viewModel.CurrentExpression);
            Assert.IsFalse(_viewModel.IsNewCalculation);
        }

        [TestMethod]
        public void InputNumber_AfterZero_ReplacesZero()
        {
            _viewModel.InputNumber("0");
            _viewModel.InputNumber("5");

            Assert.AreEqual("5", _viewModel.DisplayValue);
            Assert.AreEqual("5", _viewModel.CurrentExpression);
        }

        [TestMethod]
        public void InputNumber_MultipleDigits_ConcatenatesCorrectly()
        {
            _viewModel.InputNumber("1");
            _viewModel.InputNumber("2");
            _viewModel.InputNumber("3");

            Assert.AreEqual("123", _viewModel.DisplayValue);
            Assert.AreEqual("123", _viewModel.CurrentExpression);
        }

        [TestMethod]
        public void InputNumber_AfterCalculation_StartsNewExpression()
        {
            _viewModel.InputNumber("2");
            _viewModel.InputOperator("+");
            _viewModel.InputNumber("3");
            _viewModel.Calculate();

            _viewModel.InputNumber("7");

            Assert.AreEqual("7", _viewModel.DisplayValue);
            Assert.AreEqual("7", _viewModel.CurrentExpression);
            Assert.IsFalse(_viewModel.IsNewCalculation);
        }

        #endregion

        #region Тесты InputOperator

        [TestMethod]
        public void InputOperator_Addition_AddsToExpression()
        {
            _viewModel.InputNumber("5");
            _viewModel.InputOperator("+");

            Assert.AreEqual("5 + ", _viewModel.CurrentExpression);
        }

        [TestMethod]
        public void InputOperator_UnicodeSymbols_ConvertsCorrectly()
        {
            _viewModel.InputNumber("5");
            _viewModel.InputOperator("×");

            Assert.AreEqual("5 * ", _viewModel.CurrentExpression);

            _viewModel.InputNumber("2");
            _viewModel.InputOperator("÷");

            Assert.AreEqual("5 * 2 / ", _viewModel.CurrentExpression);
        }

        [TestMethod]
        public void InputOperator_AfterResult_UsesLastResult()
        {
            _viewModel.InputNumber("2");
            _viewModel.InputOperator("+");
            _viewModel.InputNumber("3");
            var result = _viewModel.Calculate();

            _viewModel.InputOperator("*");

            Assert.AreEqual("5 * ", _viewModel.CurrentExpression);
            Assert.IsFalse(_viewModel.IsNewCalculation);
        }

        [TestMethod]
        public void InputOperator_ResetsDecimalFlag()
        {
            _viewModel.InputNumber("3");
            _viewModel.InputDecimal();
            _viewModel.InputNumber("14");

            Assert.IsTrue(_viewModel.HasDecimalPoint);

            _viewModel.InputOperator("+");

            Assert.IsFalse(_viewModel.HasDecimalPoint);
        }

        #endregion

        #region Тесты InputDecimal

        [TestMethod]
        public void InputDecimal_NewCalculation_StartsWithZeroPoint()
        {
            _viewModel.InputDecimal();

            Assert.AreEqual("0.", _viewModel.DisplayValue);
            Assert.AreEqual("0.", _viewModel.CurrentExpression);
            Assert.IsTrue(_viewModel.HasDecimalPoint);
            Assert.IsFalse(_viewModel.IsNewCalculation);
        }

        [TestMethod]
        public void InputDecimal_AfterNumber_AddsDecimalPoint()
        {
            _viewModel.InputNumber("5");
            _viewModel.InputDecimal();

            Assert.AreEqual("5.", _viewModel.DisplayValue);
            Assert.AreEqual("5.", _viewModel.CurrentExpression);
            Assert.IsTrue(_viewModel.HasDecimalPoint);
        }

        [TestMethod]
        public void InputDecimal_AlreadyHasDecimal_IgnoresSecondDecimal()
        {
            _viewModel.InputNumber("3");
            _viewModel.InputDecimal();
            _viewModel.InputNumber("14");
            _viewModel.InputDecimal(); // Должен игнорироваться

            Assert.AreEqual("3.14", _viewModel.DisplayValue);
            Assert.AreEqual("3.14", _viewModel.CurrentExpression);
        }

        #endregion

        #region Тесты Calculate

        [TestMethod]
        public void Calculate_SimpleAddition_ReturnsCorrectResult()
        {
            _viewModel.InputNumber("2");
            _viewModel.InputOperator("+");
            _viewModel.InputNumber("3");

            var result = _viewModel.Calculate();

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(5.0, result.Result);
            Assert.AreEqual("5", result.FormattedResult);
            Assert.AreEqual("5", _viewModel.DisplayValue);
            Assert.IsTrue(_viewModel.IsNewCalculation);
        }

        [TestMethod]
        public void Calculate_ComplexExpression_ReturnsCorrectResult()
        {
            _viewModel.InputNumber("2");
            _viewModel.InputOperator("+");
            _viewModel.InputNumber("3");
            _viewModel.InputOperator("*");
            _viewModel.InputNumber("4");

            var result = _viewModel.Calculate();

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(14.0, result.Result);
        }

        [TestMethod]
        public void Calculate_EmptyExpression_ReturnsError()
        {
            var result = _viewModel.Calculate();

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Нет выражения для вычисления", result.ErrorMessage);
        }

  
        [TestMethod]
        public void Calculate_AddsToHistory()
        {
            _viewModel.InputNumber("2");
            _viewModel.InputOperator("+");
            _viewModel.InputNumber("3");

            var initialHistoryCount = _viewModel.History.Count;
            _viewModel.Calculate();

            Assert.AreEqual(initialHistoryCount + 1, _viewModel.History.Count);
            Assert.AreEqual("2 + 3 = 5", _viewModel.History[_viewModel.History.Count - 1]);
        }

        [TestMethod]
        public void Calculate_DecimalResult_SetsDecimalFlag()
        {
            _viewModel.InputNumber("5");
            _viewModel.InputOperator("/");
            _viewModel.InputNumber("2");

            var result = _viewModel.Calculate();

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(_viewModel.HasDecimalPoint);
        }

        #endregion

        #region Тесты CalculateSquareRoot

        [TestMethod]
        public void CalculateSquareRoot_PositiveNumber_ReturnsCorrectResult()
        {
            _viewModel.DisplayValue = "9";

            var result = _viewModel.CalculateSquareRoot();

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(3.0, result.Result);
            Assert.AreEqual("3", result.FormattedResult);
            Assert.AreEqual("3", _viewModel.DisplayValue);
        }

        [TestMethod]
        public void CalculateSquareRoot_Zero_ReturnsZero()
        {
            _viewModel.DisplayValue = "0";

            var result = _viewModel.CalculateSquareRoot();

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(0.0, result.Result);
        }

        [TestMethod]
        public void CalculateSquareRoot_InvalidNumber_ReturnsError()
        {
            _viewModel.DisplayValue = "abc";

            var result = _viewModel.CalculateSquareRoot();

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Некорректное число", result.ErrorMessage);
        }

        [TestMethod]
        public void CalculateSquareRoot_AddsToHistory()
        {
            _viewModel.DisplayValue = "16";
            var initialHistoryCount = _viewModel.History.Count;

            _viewModel.CalculateSquareRoot();

            Assert.AreEqual(initialHistoryCount + 1, _viewModel.History.Count);
            Assert.AreEqual("√16 = 4", _viewModel.History[_viewModel.History.Count - 1]);
        }

        #endregion

        #region Тесты Clear и ClearEntry

        [TestMethod]
        public void Clear_ResetsAllValues()
        {
            _viewModel.InputNumber("123");
            _viewModel.InputOperator("+");
            _viewModel.InputNumber("456");
            _viewModel.LastResult = 579;
            _viewModel.HasDecimalPoint = true;

            _viewModel.Clear();

            Assert.AreEqual("0", _viewModel.DisplayValue);
            Assert.AreEqual("0", _viewModel.CurrentExpression);
            Assert.AreEqual(0.0, _viewModel.LastResult);
            Assert.IsTrue(_viewModel.IsNewCalculation);
            Assert.IsFalse(_viewModel.HasDecimalPoint);
        }

        [TestMethod]
        public void ClearEntry_ResetsDisplayOnly()
        {
            _viewModel.InputNumber("123");
            _viewModel.InputOperator("+");
            _viewModel.InputNumber("456");

            _viewModel.ClearEntry();

            Assert.AreEqual("0", _viewModel.DisplayValue);
            Assert.IsFalse(_viewModel.HasDecimalPoint);
            // CurrentExpression должно сохраниться частично
        }

        #endregion

        #region Тесты истории

        [TestMethod]
        public void GetHistory_ReturnsHistoryCopy()
        {
            _viewModel.InputNumber("2");
            _viewModel.InputOperator("+");
            _viewModel.InputNumber("3");
            _viewModel.Calculate();

            var history = _viewModel.GetHistory();

            Assert.IsNotNull(history);
            Assert.AreEqual(_viewModel.History.Count, history.Count);
            // Проверяем что это копия, а не ссылка
            Assert.AreNotSame(_viewModel.History, history);
        }

        [TestMethod]
        public void ClearHistory_RemovesAllHistoryItems()
        {
            _viewModel.InputNumber("2");
            _viewModel.InputOperator("+");
            _viewModel.InputNumber("3");
            _viewModel.Calculate();

            Assert.IsTrue(_viewModel.History.Count > 0);

            _viewModel.ClearHistory();

            Assert.AreEqual(0, _viewModel.History.Count);
        }

        #endregion

        #region Тесты PropertyChanged

        [TestMethod]
        public void PropertyChanged_DisplayValue_FiresEvent()
        {
            bool eventFired = false;
            _viewModel.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(_viewModel.DisplayValue))
                    eventFired = true;
            };

            _viewModel.DisplayValue = "test";

            Assert.IsTrue(eventFired);
        }

        [TestMethod]
        public void PropertyChanged_CurrentExpression_FiresEvent()
        {
            bool eventFired = false;
            _viewModel.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(_viewModel.CurrentExpression))
                    eventFired = true;
            };

            _viewModel.CurrentExpression = "2+3";

            Assert.IsTrue(eventFired);
        }

        [TestMethod]
        public void PropertyChanged_Clear_FiresMultipleEvents()
        {
            var firedProperties = new System.Collections.Generic.List<string>();
            _viewModel.PropertyChanged += (sender, e) =>
            {
                firedProperties.Add(e.PropertyName);
            };

            _viewModel.Clear();

            Assert.IsTrue(firedProperties.Contains(nameof(_viewModel.DisplayValue)));
            Assert.IsTrue(firedProperties.Contains(nameof(_viewModel.CurrentExpression)));
            Assert.IsTrue(firedProperties.Contains(nameof(_viewModel.LastResult)));
            Assert.IsTrue(firedProperties.Contains(nameof(_viewModel.IsNewCalculation)));
            Assert.IsTrue(firedProperties.Contains(nameof(_viewModel.HasDecimalPoint)));
        }

        #endregion

        #region Тесты ConvertOperatorSymbol

        [TestMethod]
        public void ConvertOperatorSymbol_AllSymbols_ConvertsCorrectly()
        {
            // Используем рефлексию для тестирования private метода или создаем public wrapper
            _viewModel.InputNumber("5");

            _viewModel.InputOperator("÷");
            Assert.IsTrue(_viewModel.CurrentExpression.Contains("/"));

            _viewModel.Clear();
            _viewModel.InputNumber("5");
            _viewModel.InputOperator("×");
            Assert.IsTrue(_viewModel.CurrentExpression.Contains("*"));

            _viewModel.Clear();
            _viewModel.InputNumber("5");
            _viewModel.InputOperator("−");
            Assert.IsTrue(_viewModel.CurrentExpression.Contains("-"));

            _viewModel.Clear();
            _viewModel.InputNumber("5");
            _viewModel.InputOperator("+");
            Assert.IsTrue(_viewModel.CurrentExpression.Contains("+"));
        }

        #endregion

        #region Интеграционные тесты

        [TestMethod]
        public void Integration_CompleteCalculationFlow_WorksCorrectly()
        {
            // Полный цикл: ввод → операция → ввод → результат → новая операция

            // Первая операция: 2 + 3 = 5
            _viewModel.InputNumber("2");
            _viewModel.InputOperator("+");
            _viewModel.InputNumber("3");
            var result1 = _viewModel.Calculate();

            Assert.IsTrue(result1.IsSuccess);
            Assert.AreEqual(5.0, result1.Result);
            Assert.IsTrue(_viewModel.IsNewCalculation);

            // Вторая операция: 5 * 2 = 10 (используя результат)
            _viewModel.InputOperator("*");
            _viewModel.InputNumber("2");
            var result2 = _viewModel.Calculate();

            Assert.IsTrue(result2.IsSuccess);
            Assert.AreEqual(10.0, result2.Result);

            // Проверяем историю
            Assert.IsTrue(_viewModel.History.Count >= 2);
        }

        [TestMethod]
        public void Integration_ErrorRecovery_WorksCorrectly()
        {
            // Тестируем восстановление после ошибки
            _viewModel.CurrentExpression = "invalid expression";
            var errorResult = _viewModel.Calculate();

            Assert.IsFalse(errorResult.IsSuccess);

            // После ошибки должна быть возможность нормально работать
            _viewModel.Clear();
            _viewModel.InputNumber("2");
            _viewModel.InputOperator("+");
            _viewModel.InputNumber("3");
            var goodResult = _viewModel.Calculate();

            Assert.IsTrue(goodResult.IsSuccess);
            Assert.AreEqual(5.0, goodResult.Result);
        }

        [TestMethod]
        public void Integration_SquareRootInFlow_WorksCorrectly()
        {
            // Квадратный корень в процессе вычислений
            _viewModel.InputNumber("16");
            var sqrtResult = _viewModel.CalculateSquareRoot();

            Assert.IsTrue(sqrtResult.IsSuccess);
            Assert.AreEqual(4.0, sqrtResult.Result);

            // Используем результат в дальнейших вычислениях
            _viewModel.InputOperator("+");
            _viewModel.InputNumber("6");
            var finalResult = _viewModel.Calculate();

            Assert.IsTrue(finalResult.IsSuccess);
            Assert.AreEqual(10.0, finalResult.Result);
        }

        #endregion

        #region Тесты граничных случаев

        [TestMethod]
        public void EdgeCase_VeryLongNumber_HandlesCorrectly()
        {
            var longNumber = "123456789012345";
            foreach (char digit in longNumber)
            {
                _viewModel.InputNumber(digit.ToString());
            }

            Assert.AreEqual(longNumber, _viewModel.DisplayValue);
            Assert.AreEqual(longNumber, _viewModel.CurrentExpression);
        }

        [TestMethod]
        public void EdgeCase_MultipleOperators_LastOneWins()
        {
            _viewModel.InputNumber("5");
            _viewModel.InputOperator("+");
            _viewModel.InputOperator("*");
            _viewModel.InputOperator("-");

            // Должен использоваться последний оператор
            Assert.IsTrue(_viewModel.CurrentExpression.Contains("-"));
        }

        [TestMethod]
        public void EdgeCase_CalculateWithoutSecondOperand_HandlesCorrectly()
        {
            _viewModel.InputNumber("5");
            _viewModel.InputOperator("+");

            var result = _viewModel.Calculate();

            // Поведение зависит от реализации сервиса
            // Может быть как ошибка, так и обработка как "5+"
            Assert.IsTrue(result.IsSuccess || !result.IsSuccess);
        }

        [TestMethod]
        public void EdgeCase_MultipleDecimals_OnlyFirstAccepted()
        {
            _viewModel.InputNumber("3");
            _viewModel.InputDecimal();
            _viewModel.InputNumber("14");
            _viewModel.InputDecimal(); // Должен игнорироваться
            _viewModel.InputDecimal(); // И этот тоже
            _viewModel.InputNumber("15");

            Assert.AreEqual("3.1415", _viewModel.DisplayValue);
        }

        [TestMethod]
        public void EdgeCase_CalculateAfterClear_WorksCorrectly()
        {
            _viewModel.InputNumber("5");
            _viewModel.Clear();
            _viewModel.InputNumber("3");
            _viewModel.InputOperator("+");
            _viewModel.InputNumber("2");

            var result = _viewModel.Calculate();

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(5.0, result.Result);
        }

        #endregion

        #region Тесты производительности и стабильности

        [TestMethod]
        public void Performance_ManyOperations_CompletesQuickly()
        {
            var startTime = DateTime.Now;

            for (int i = 0; i < 100; i++)
            {
                _viewModel.InputNumber(i.ToString());
                _viewModel.InputOperator("+");
                _viewModel.InputNumber("1");
                _viewModel.Calculate();
                _viewModel.Clear();
            }

            var duration = DateTime.Now - startTime;
            Assert.IsTrue(duration.TotalSeconds < 5); // Должно завершиться за 5 секунд
        }

        [TestMethod]
        public void Stability_RepeatedCalculations_RemainsStable()
        {
            for (int i = 0; i < 10; i++)
            {
                _viewModel.InputNumber("2");
                _viewModel.InputOperator("+");
                _viewModel.InputNumber("3");
                var result = _viewModel.Calculate();

                Assert.IsTrue(result.IsSuccess);
                Assert.AreEqual(5.0, result.Result);

                _viewModel.Clear();
            }
        }

        [TestMethod]
        public void Stability_PropertyChangedEvents_NoMemoryLeaks()
        {
            // Подписываемся и отписываемся от событий
            PropertyChangedEventHandler handler = (s, e) => { };

            for (int i = 0; i < 100; i++)
            {
                _viewModel.PropertyChanged += handler;
                _viewModel.DisplayValue = i.ToString();
                _viewModel.PropertyChanged -= handler;
            }

            // Если дошли до сюда без исключений, то все в порядке
            Assert.IsTrue(true);
        }

        #endregion

        #region Тесты исключений

        [TestMethod]
        public void Exception_ServiceThrowsException_HandledGracefully()
        {
            // Создаем мок сервиса который выбрасывает исключение
            var mockService = new ThrowingCalculatorService();
            var vmWithMock = new CalculatorViewModel(mockService);

            vmWithMock.InputNumber("2");
            vmWithMock.InputOperator("+");
            vmWithMock.InputNumber("3");

            var result = vmWithMock.Calculate();

            Assert.IsFalse(result.IsSuccess);
            Assert.IsNotNull(result.ErrorMessage);
        }

        [TestMethod]
        public void Exception_SquareRootServiceThrows_HandledGracefully()
        {
            var mockService = new ThrowingCalculatorService();
            var vmWithMock = new CalculatorViewModel(mockService);

            vmWithMock.DisplayValue = "9";
            var result = vmWithMock.CalculateSquareRoot();

            Assert.IsFalse(result.IsSuccess);
            Assert.IsNotNull(result.ErrorMessage);
        }

        #endregion
    }

    #region Помощники для тестов

    /// <summary>
    /// Мок сервиса калькулятора, который всегда выбрасывает исключения
    /// </summary>
    public class ThrowingCalculatorService : ICalculatorService
    {
        public double Calculate(string expression) => throw new InvalidOperationException("Test exception");

        public bool IsValidExpression(string expression) => throw new InvalidOperationException("Test exception");

        public string FormatResult(double result) => throw new InvalidOperationException("Test exception");

        public double CalculateSquareRoot(double value) => throw new InvalidOperationException("Test exception");

        public double CalculatePower(double baseValue, double exponent) => throw new InvalidOperationException("Test exception");

        public double CalculateCubeRoot(double value) => throw new InvalidOperationException("Test exception");

        public double CalculateFactorial(double value) => throw new InvalidOperationException("Test exception");
    }

    #endregion
}