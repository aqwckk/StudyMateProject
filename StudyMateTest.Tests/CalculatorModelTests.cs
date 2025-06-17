using Microsoft.VisualStudio.TestTools.UnitTesting;
using StudyMateTest.Core.Models.Calculator;
using System.Linq;

namespace StudyMateTest.Tests
{
    [TestClass]
    public class CalculatorModelTests
    {
        private CalculatorModel _calculatorModel;

        [TestInitialize]
        public void Setup()
        {
            _calculatorModel = new CalculatorModel();
        }

        #region Тесты инициализации

        [TestMethod]
        public void Constructor_InitializesWithDefaultValues()
        {
            var model = new CalculatorModel();

            Assert.AreEqual("0", model.CurrentExpression);
            Assert.AreEqual("0", model.DisplayValue);
            Assert.AreEqual(0.0, model.LastResult);
            Assert.IsTrue(model.IsNewCalculation);
            Assert.IsFalse(model.HasDecimalPoint);
            Assert.AreEqual("", model.LastOperation);
            Assert.IsFalse(model.WaitingForOperand);
            Assert.IsNotNull(model.History);
            Assert.AreEqual(0, model.History.Count);
        }

        #endregion

        #region Тесты свойств

        [TestMethod]
        public void CurrentExpression_CanSetAndGet()
        {
            _calculatorModel.CurrentExpression = "2+3";
            Assert.AreEqual("2+3", _calculatorModel.CurrentExpression);
        }

        [TestMethod]
        public void DisplayValue_CanSetAndGet()
        {
            _calculatorModel.DisplayValue = "5";
            Assert.AreEqual("5", _calculatorModel.DisplayValue);
        }

        [TestMethod]
        public void LastResult_CanSetAndGet()
        {
            _calculatorModel.LastResult = 42.5;
            Assert.AreEqual(42.5, _calculatorModel.LastResult);
        }

        [TestMethod]
        public void IsNewCalculation_CanSetAndGet()
        {
            _calculatorModel.IsNewCalculation = false;
            Assert.IsFalse(_calculatorModel.IsNewCalculation);
        }

        [TestMethod]
        public void HasDecimalPoint_CanSetAndGet()
        {
            _calculatorModel.HasDecimalPoint = true;
            Assert.IsTrue(_calculatorModel.HasDecimalPoint);
        }

        [TestMethod]
        public void LastOperation_CanSetAndGet()
        {
            _calculatorModel.LastOperation = "+";
            Assert.AreEqual("+", _calculatorModel.LastOperation);
        }

        [TestMethod]
        public void WaitingForOperand_CanSetAndGet()
        {
            _calculatorModel.WaitingForOperand = true;
            Assert.IsTrue(_calculatorModel.WaitingForOperand);
        }

        [TestMethod]
        public void History_CanSetAndGet()
        {
            var newHistory = new System.Collections.Generic.List<string> { "2+2=4" };
            _calculatorModel.History = newHistory;
            Assert.AreEqual(newHistory, _calculatorModel.History);
        }

        #endregion

        #region Тесты метода Reset

        [TestMethod]
        public void Reset_ResetsAllPropertiesToDefault()
        {
            // Устанавливаем некоторые значения
            _calculatorModel.CurrentExpression = "2+3*4";
            _calculatorModel.DisplayValue = "14";
            _calculatorModel.LastResult = 14.0;
            _calculatorModel.IsNewCalculation = false;
            _calculatorModel.HasDecimalPoint = true;
            _calculatorModel.LastOperation = "*";
            _calculatorModel.WaitingForOperand = true;
            _calculatorModel.History.Add("2+2=4");

            // Вызываем Reset
            _calculatorModel.Reset();

            // Проверяем что все сброшено кроме истории
            Assert.AreEqual("0", _calculatorModel.CurrentExpression);
            Assert.AreEqual("0", _calculatorModel.DisplayValue);
            Assert.AreEqual(0.0, _calculatorModel.LastResult);
            Assert.IsTrue(_calculatorModel.IsNewCalculation);
            Assert.IsFalse(_calculatorModel.HasDecimalPoint);
            Assert.AreEqual("", _calculatorModel.LastOperation);
            Assert.IsFalse(_calculatorModel.WaitingForOperand);

            // История НЕ должна очищаться
            Assert.AreEqual(1, _calculatorModel.History.Count);
            Assert.AreEqual("2+2=4", _calculatorModel.History[0]);
        }

        [TestMethod]
        public void Reset_CalledMultipleTimes_RemainsStable()
        {
            _calculatorModel.Reset();
            _calculatorModel.Reset();
            _calculatorModel.Reset();

            Assert.AreEqual("0", _calculatorModel.CurrentExpression);
            Assert.AreEqual("0", _calculatorModel.DisplayValue);
            Assert.AreEqual(0.0, _calculatorModel.LastResult);
        }

        [TestMethod]
        public void Reset_DoesNotClearHistory()
        {
            _calculatorModel.AddToHistory("5+5=10");
            _calculatorModel.AddToHistory("3*3=9");

            _calculatorModel.Reset();

            Assert.AreEqual(2, _calculatorModel.History.Count);
            Assert.AreEqual("5+5=10", _calculatorModel.History[0]);
            Assert.AreEqual("3*3=9", _calculatorModel.History[1]);
        }

        #endregion

        #region Тесты метода AddToHistory

        [TestMethod]
        public void AddToHistory_AddsItemToHistory()
        {
            _calculatorModel.AddToHistory("2+2=4");

            Assert.AreEqual(1, _calculatorModel.History.Count);
            Assert.AreEqual("2+2=4", _calculatorModel.History[0]);
        }

        [TestMethod]
        public void AddToHistory_AddsMultipleItems_MaintainsOrder()
        {
            _calculatorModel.AddToHistory("2+2=4");
            _calculatorModel.AddToHistory("3*3=9");
            _calculatorModel.AddToHistory("5-1=4");

            Assert.AreEqual(3, _calculatorModel.History.Count);
            Assert.AreEqual("2+2=4", _calculatorModel.History[0]);
            Assert.AreEqual("3*3=9", _calculatorModel.History[1]);
            Assert.AreEqual("5-1=4", _calculatorModel.History[2]);
        }

        [TestMethod]
        public void AddToHistory_EmptyString_AddsEmptyString()
        {
            _calculatorModel.AddToHistory("");

            Assert.AreEqual(1, _calculatorModel.History.Count);
            Assert.AreEqual("", _calculatorModel.History[0]);
        }

        [TestMethod]
        public void AddToHistory_NullString_AddsNull()
        {
            _calculatorModel.AddToHistory(null);

            Assert.AreEqual(1, _calculatorModel.History.Count);
            Assert.IsNull(_calculatorModel.History[0]);
        }

        [TestMethod]
        public void AddToHistory_ExceedsLimit_RemovesOldestItem()
        {
            // Добавляем 52 элемента (больше лимита в 50)
            for (int i = 1; i <= 52; i++)
            {
                _calculatorModel.AddToHistory($"calculation{i}");
            }

            // Должно остаться только 50 элементов
            Assert.AreEqual(50, _calculatorModel.History.Count);

            // Первые два элемента должны быть удалены
            Assert.AreEqual("calculation3", _calculatorModel.History[0]);
            Assert.AreEqual("calculation52", _calculatorModel.History[49]);
        }

        [TestMethod]
        public void AddToHistory_ExactlyFiftyItems_DoesNotRemove()
        {
            for (int i = 1; i <= 50; i++)
            {
                _calculatorModel.AddToHistory($"calculation{i}");
            }

            Assert.AreEqual(50, _calculatorModel.History.Count);
            Assert.AreEqual("calculation1", _calculatorModel.History[0]);
            Assert.AreEqual("calculation50", _calculatorModel.History[49]);
        }

        [TestMethod]
        public void AddToHistory_FiftyOneItems_RemovesFirst()
        {
            for (int i = 1; i <= 51; i++)
            {
                _calculatorModel.AddToHistory($"calculation{i}");
            }

            Assert.AreEqual(50, _calculatorModel.History.Count);
            Assert.AreEqual("calculation2", _calculatorModel.History[0]);
            Assert.AreEqual("calculation51", _calculatorModel.History[49]);
        }

        [TestMethod]
        public void AddToHistory_ContinuousAdding_MaintainsCorrectOrder()
        {
            // Добавляем элементы в несколько этапов
            for (int i = 1; i <= 25; i++)
            {
                _calculatorModel.AddToHistory($"batch1_{i}");
            }

            for (int i = 1; i <= 30; i++)
            {
                _calculatorModel.AddToHistory($"batch2_{i}");
            }

            Assert.AreEqual(50, _calculatorModel.History.Count);

            // Первые 5 элементов первой партии должны быть удалены
            Assert.AreEqual("batch1_6", _calculatorModel.History[0]);
            Assert.AreEqual("batch2_30", _calculatorModel.History[49]);
        }

        #endregion

        #region Интеграционные тесты

        [TestMethod]
        public void Integration_TypicalCalculatorUsage_WorksCorrectly()
        {
            // Симулируем типичное использование калькулятора
            _calculatorModel.CurrentExpression = "2";
            _calculatorModel.DisplayValue = "2";
            _calculatorModel.IsNewCalculation = false;

            _calculatorModel.LastOperation = "+";
            _calculatorModel.WaitingForOperand = true;

            _calculatorModel.CurrentExpression = "2+3";
            _calculatorModel.DisplayValue = "3";
            _calculatorModel.WaitingForOperand = false;

            _calculatorModel.LastResult = 5.0;
            _calculatorModel.AddToHistory("2+3=5");

            Assert.AreEqual("2+3", _calculatorModel.CurrentExpression);
            Assert.AreEqual(5.0, _calculatorModel.LastResult);
            Assert.AreEqual(1, _calculatorModel.History.Count);
            Assert.AreEqual("2+3=5", _calculatorModel.History[0]);
        }

        [TestMethod]
        public void Integration_DecimalCalculation_HandlesCorrectly()
        {
            _calculatorModel.HasDecimalPoint = true;
            _calculatorModel.CurrentExpression = "3.14";
            _calculatorModel.DisplayValue = "3.14";
            _calculatorModel.LastResult = 3.14;

            _calculatorModel.Reset();

            Assert.IsFalse(_calculatorModel.HasDecimalPoint);
            Assert.AreEqual("0", _calculatorModel.CurrentExpression);
        }

        [TestMethod]
        public void Integration_HistoryUnderLoad_PerformsWell()
        {
            // Тестируем производительность с большим количеством операций
            var startTime = System.DateTime.Now;

            for (int i = 0; i < 100; i++)
            {
                _calculatorModel.AddToHistory($"operation_{i}=result_{i}");
            }

            var endTime = System.DateTime.Now;
            var duration = endTime - startTime;

            // Операция должна завершиться быстро (менее 100мс)
            Assert.IsTrue(duration.TotalMilliseconds < 100);
            Assert.AreEqual(50, _calculatorModel.History.Count);
            Assert.AreEqual("operation_50=result_50", _calculatorModel.History[0]);
            Assert.AreEqual("operation_99=result_99", _calculatorModel.History[49]);
        }

        [TestMethod]
        public void Integration_StateTransitions_WorkCorrectly()
        {
            // Начальное состояние
            Assert.IsTrue(_calculatorModel.IsNewCalculation);
            Assert.IsFalse(_calculatorModel.WaitingForOperand);

            // Ввод числа
            _calculatorModel.IsNewCalculation = false;
            _calculatorModel.CurrentExpression = "5";

            // Ввод оператора
            _calculatorModel.LastOperation = "*";
            _calculatorModel.WaitingForOperand = true;

            // Ввод второго числа
            _calculatorModel.CurrentExpression = "5*3";
            _calculatorModel.WaitingForOperand = false;

            // Получение результата
            _calculatorModel.LastResult = 15.0;
            _calculatorModel.IsNewCalculation = true;
            _calculatorModel.AddToHistory("5*3=15");

            Assert.IsTrue(_calculatorModel.IsNewCalculation);
            Assert.IsFalse(_calculatorModel.WaitingForOperand);
            Assert.AreEqual(15.0, _calculatorModel.LastResult);
            Assert.AreEqual(1, _calculatorModel.History.Count);
        }

        #endregion

        #region Граничные случаи

        [TestMethod]
        public void EdgeCase_ExtremeNumbers_HandlesCorrectly()
        {
            _calculatorModel.LastResult = double.MaxValue;
            Assert.AreEqual(double.MaxValue, _calculatorModel.LastResult);

            _calculatorModel.LastResult = double.MinValue;
            Assert.AreEqual(double.MinValue, _calculatorModel.LastResult);

            _calculatorModel.LastResult = double.NaN;
            Assert.IsTrue(double.IsNaN(_calculatorModel.LastResult));

            _calculatorModel.LastResult = double.PositiveInfinity;
            Assert.IsTrue(double.IsPositiveInfinity(_calculatorModel.LastResult));
        }

        [TestMethod]
        public void EdgeCase_VeryLongStrings_HandlesCorrectly()
        {
            var longExpression = new string('1', 1000) + "+" + new string('2', 1000);
            _calculatorModel.CurrentExpression = longExpression;
            Assert.AreEqual(longExpression, _calculatorModel.CurrentExpression);

            var longHistory = "very_long_calculation_" + new string('x', 500);
            _calculatorModel.AddToHistory(longHistory);
            Assert.AreEqual(longHistory, _calculatorModel.History[0]);
        }

        [TestMethod]
        public void EdgeCase_SpecialCharacters_HandlesCorrectly()
        {
            _calculatorModel.CurrentExpression = "sin(π/2)+cos(0)×√(16)÷2−1";
            _calculatorModel.LastOperation = "≠≤≥∞";

            Assert.AreEqual("sin(π/2)+cos(0)×√(16)÷2−1", _calculatorModel.CurrentExpression);
            Assert.AreEqual("≠≤≥∞", _calculatorModel.LastOperation);
        }

        [TestMethod]
        public void EdgeCase_MassiveHistoryAddition_PerformsCorrectly()
        {
            // Добавляем 1000 элементов истории
            for (int i = 0; i < 1000; i++)
            {
                _calculatorModel.AddToHistory($"massive_operation_{i}");
            }

            // Должно остаться только 50
            Assert.AreEqual(50, _calculatorModel.History.Count);
            Assert.AreEqual("massive_operation_950", _calculatorModel.History[0]);
            Assert.AreEqual("massive_operation_999", _calculatorModel.History[49]);
        }

        #endregion

        #region Тесты состояния объекта

        [TestMethod]
        public void ObjectState_RemainsConsistent_AfterOperations()
        {
            // Выполняем серию операций
            _calculatorModel.CurrentExpression = "test";
            _calculatorModel.DisplayValue = "display";
            _calculatorModel.LastResult = 42.0;
            _calculatorModel.IsNewCalculation = false;
            _calculatorModel.HasDecimalPoint = true;
            _calculatorModel.LastOperation = "+";
            _calculatorModel.WaitingForOperand = true;
            _calculatorModel.AddToHistory("operation1");
            _calculatorModel.AddToHistory("operation2");

            // Проверяем что состояние согласовано
            Assert.AreEqual("test", _calculatorModel.CurrentExpression);
            Assert.AreEqual("display", _calculatorModel.DisplayValue);
            Assert.AreEqual(42.0, _calculatorModel.LastResult);
            Assert.IsFalse(_calculatorModel.IsNewCalculation);
            Assert.IsTrue(_calculatorModel.HasDecimalPoint);
            Assert.AreEqual("+", _calculatorModel.LastOperation);
            Assert.IsTrue(_calculatorModel.WaitingForOperand);
            Assert.AreEqual(2, _calculatorModel.History.Count);

            // Reset и проверка согласованности после сброса
            _calculatorModel.Reset();

            Assert.AreEqual("0", _calculatorModel.CurrentExpression);
            Assert.AreEqual("0", _calculatorModel.DisplayValue);
            Assert.AreEqual(0.0, _calculatorModel.LastResult);
            Assert.IsTrue(_calculatorModel.IsNewCalculation);
            Assert.IsFalse(_calculatorModel.HasDecimalPoint);
            Assert.AreEqual("", _calculatorModel.LastOperation);
            Assert.IsFalse(_calculatorModel.WaitingForOperand);
            Assert.AreEqual(2, _calculatorModel.History.Count); // История не сбрасывается
        }

        #endregion
    }
}