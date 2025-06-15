namespace StudyMateTest.Services.CalculatorServices
{
    public interface ICalculatorService
    {
        // Основные методы для калькулятора
        double Calculate(string expression);
        bool IsValidExpression(string expression);
        string FormatResult(double result);

        // Базовые математические функции (соответствуют кнопкам)
        double CalculateSquareRoot(double value);
        double CalculatePower(double baseValue, double exponent);
        double CalculateCubeRoot(double value);
        double CalculateFactorial(double value);
    }
}