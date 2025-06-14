using System.Collections.Generic;

namespace StudyMateTest.Models
{
    public class CalculatorModel
    {
        public string CurrentExpression { get; set; } = "0";
        public string DisplayValue { get; set; } = "0";
        public double LastResult { get; set; } = 0;
        public bool IsNewCalculation { get; set; } = true;
        public bool HasDecimalPoint { get; set; } = false;
        public List<string> History { get; set; } = new List<string>();

        // Для отслеживания последней операции
        public string LastOperation { get; set; } = "";
        public bool WaitingForOperand { get; set; } = false;

        public void Reset()
        {
            CurrentExpression = "0";
            DisplayValue = "0";
            LastResult = 0;
            IsNewCalculation = true;
            HasDecimalPoint = false;
            LastOperation = "";
            WaitingForOperand = false;
        }

        public void AddToHistory(string calculation)
        {
            History.Add(calculation);
            // Ограничиваем историю 50 записями
            if (History.Count > 50)
            {
                History.RemoveAt(0);
            }
        }
    }
}