namespace StudyMateTest.Core.ViewModels.Calculator
{
    
    /// Результат вычисления
    
    public class CalculationResult
    {      
        /// Успешно ли выполнено вычисление
        public bool IsSuccess { get; set; }
        
        /// Численный результат вычисления
        public double Result { get; set; }
        
        /// Отформатированный результат для отображения
        public string FormattedResult { get; set; }
        
        /// Сообщение об ошибке, если вычисление не удалось
        public string ErrorMessage { get; set; }
    }
}