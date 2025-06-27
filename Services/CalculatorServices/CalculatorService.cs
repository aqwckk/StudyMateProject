using System;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;

namespace StudyMateTest.Services.CalculatorServices
{
    public class CalculatorService : ICalculatorService
    {
        public double Calculate(string expression)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(expression))
                    throw new InvalidOperationException("Пустое выражение");

                // Подготавливаем выражение
                expression = PrepareExpression(expression);

                // Обрабатываем научные функции (включая cbrt!)
                expression = ProcessScientificFunctions(expression);
                expression = ProcessSqrtFunction(expression);
                expression = ProcessFactorialFunction(expression);
                expression = ProcessPowerOperations(expression);

                // Проверяем на ошибки обработки
                if (expression.Contains("ERROR_VALUE"))
                    return double.NaN;

                // Используем DataTable для вычисления базовых операций
                var table = new DataTable();
                var result = table.Compute(expression, null);

                if (result == DBNull.Value)
                    return double.NaN;

                double calculatedResult = Convert.ToDouble(result);

                if (double.IsNaN(calculatedResult) || double.IsInfinity(calculatedResult))
                    return double.NaN;

                return calculatedResult;
            }
            catch
            {
                return double.NaN;
            }
        }

        public bool IsValidExpression(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return false;

            try
            {
                // Проверяем на множественные минусы подряд
                if (HasMultipleMinuses(expression))
                    return false;

                string prepared = PrepareExpression(expression);

                // Проверяем балансировку скобок
                int openParens = 0;
                foreach (char c in prepared)
                {
                    if (c == '(') openParens++;
                    if (c == ')') openParens--;
                    if (openParens < 0) return false;
                }
                if (openParens != 0) return false;

                // Проверяем на недопустимые символы
                if (Regex.IsMatch(prepared, @"[^0-9+\-*/.() \^√²³sincostanlgbqrtfac]"))
                    return false;

                // Проверяем окончания
                string trimmed = prepared.Trim();
                if (Regex.IsMatch(trimmed, @"[+\-*/.^]$"))
                    return false;

                if (Regex.IsMatch(trimmed, @"^[+*/.^]"))
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool HasMultipleMinuses(string expression)
        {
            // Проверяем на два или более минуса подряд
            // Разрешаем только отрицательные числа в скобках: (-5)

            // Временно заменяем корректные отрицательные числа в скобках
            string temp = Regex.Replace(expression, @"\(\s*−\s*\d+(?:\.\d+)?\s*\)", "NEGATIVE_NUMBER");

            // Теперь ищем множественные минусы
            if (Regex.IsMatch(temp, @"−\s*−"))
                return true;

            // Проверяем на другие множественные операторы
            if (Regex.IsMatch(temp, @"[+×÷]\s*[+×÷]"))
                return true;

            return false;
        }

        public string FormatResult(double result)
        {
            if (double.IsNaN(result))
                return "Ошибка";
            if (double.IsPositiveInfinity(result))
                return "Ошибка";
            if (double.IsNegativeInfinity(result))
                return "Ошибка";
            if (result == 0)
                return "0";

            // Для очень маленьких чисел
            if (Math.Abs(result) > 0 && Math.Abs(result) < 0.000001)
                return result.ToString("E6", CultureInfo.InvariantCulture);

            // Для очень больших чисел
            if (Math.Abs(result) >= 1000000000000)
                return result.ToString("E6", CultureInfo.InvariantCulture);

            // Для целых чисел
            if (result == Math.Floor(result) && Math.Abs(result) < 1000000000)
                return result.ToString("F0", CultureInfo.InvariantCulture);

            // Для обычных чисел с ограничением десятичных знаков
            string formatted = result.ToString("F10", CultureInfo.InvariantCulture)
                                    .TrimEnd('0')
                                    .TrimEnd('.');

            return string.IsNullOrEmpty(formatted) ? "0" : formatted;
        }

        #region Basic Mathematical Functions

        public double CalculateSquareRoot(double value)
        {
            if (value < 0)
                return double.NaN;
            return Math.Sqrt(value);
        }

        public double CalculatePower(double baseValue, double exponent)
        {
            var result = Math.Pow(baseValue, exponent);
            if (double.IsNaN(result) || double.IsInfinity(result))
                return double.NaN;
            return result;
        }

        public double CalculateCubeRoot(double value)
        {
            try
            {
                if (value == 0)
                    return 0;

                if (value > 0)
                {
                    return Math.Pow(value, 1.0 / 3.0);
                }
                else
                {
                    // Для отрицательных чисел: ∛(-8) = -∛(8) = -2
                    return -Math.Pow(-value, 1.0 / 3.0);
                }
            }
            catch
            {
                return double.NaN;
            }
        }

        public double CalculateFactorial(double value)
        {
            // Проверки на все виды ошибок
            if (value < 0) return double.NaN; // Отрицательное число
            if (value != Math.Floor(value)) return double.NaN; // Не целое число
            if (value > 170) return double.NaN; // Слишком большое число

            if (value == 0 || value == 1)
                return 1;

            double result = 1;
            for (int i = 2; i <= value; i++)
            {
                result *= i;
                if (double.IsInfinity(result))
                    return double.NaN;
            }
            return result;
        }

        #endregion

        #region Expression Processing

        private string PrepareExpression(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return "";

            // ЗАМЕНЯЕМ ∛( на cbrt( - делаем кубический корень обычной функцией
            expression = expression.Replace("∛(", "cbrt(");

            // Заменяем символы на числовые значения ТОЛЬКО при вычислении
            expression = expression.Replace("π", Math.PI.ToString("G17", CultureInfo.InvariantCulture))
                                 .Replace("e", Math.E.ToString("G17", CultureInfo.InvariantCulture));

            // Заменяем отображаемые символы на математические операторы
            expression = expression.Replace("×", "*")
                                 .Replace("÷", "/")
                                 .Replace("−", "-")
                                 .Replace(",", ".")
                                 .Replace("²", "^2")
                                 .Replace("³", "^3");

            // Убираем лишние пробелы
            expression = Regex.Replace(expression, @"\s+", "");

            return expression;
        }

        private string ProcessScientificFunctions(string expression)
        {
            try
            {
                // Обрабатываем тригонометрические функции с ПРАВИЛЬНЫМИ проверками
                expression = ProcessFunction(expression, "sin", x => {
                    double result = Math.Sin(x);
                    if (double.IsNaN(result) || double.IsInfinity(result))
                        return double.NaN;
                    return result;
                });

                expression = ProcessFunction(expression, "cos", x => {
                    double result = Math.Cos(x);
                    if (double.IsNaN(result) || double.IsInfinity(result))
                        return double.NaN;
                    return result;
                });

                // TAN - ПРОВЕРЯЕМ НА tan(π/2), tan(3π/2) и т.д.
                expression = ProcessFunction(expression, "tan", x => {
                    // Нормализуем угол к диапазону [0, 2π]
                    double normalizedX = (x % (2 * Math.PI) + 2 * Math.PI) % (2 * Math.PI);

                    // Проверяем проблемные точки: π/2, 3π/2
                    double piHalf = Math.PI / 2;
                    double threePiHalf = 3 * Math.PI / 2;

                    if (Math.Abs(normalizedX - piHalf) < 1e-10 ||
                        Math.Abs(normalizedX - threePiHalf) < 1e-10)
                    {
                        return double.NaN; // tan не определен
                    }

                    double result = Math.Tan(x);

                    // Дополнительная проверка на очень большие значения (близко к π/2)
                    if (Math.Abs(result) > 1e15)
                        return double.NaN;

                    if (double.IsNaN(result) || double.IsInfinity(result))
                        return double.NaN;

                    return result;
                });

                // COT - ПРОВЕРЯЕМ НА cot(0), cot(π), cot(2π) И cot(π/2)
                expression = ProcessFunction(expression, "cot", x => {
                    // Нормализуем угол к диапазону [0, 2π]
                    double normalizedX = (x % (2 * Math.PI) + 2 * Math.PI) % (2 * Math.PI);

                    // Проверяем проблемные точки: 0, π, 2π (где sin=0)
                    if (Math.Abs(normalizedX) < 1e-10 ||
                        Math.Abs(normalizedX - Math.PI) < 1e-10 ||
                        Math.Abs(normalizedX - 2 * Math.PI) < 1e-10)
                    {
                        return double.NaN; // cot не определен когда sin=0
                    }

                    // Особый случай: cot(π/2) = 0, cot(3π/2) = 0
                    double piHalf = Math.PI / 2;
                    double threePiHalf = 3 * Math.PI / 2;

                    if (Math.Abs(normalizedX - piHalf) < 1e-10 ||
                        Math.Abs(normalizedX - threePiHalf) < 1e-10)
                    {
                        return 0; // cot(π/2) = 0
                    }

                    // Вычисляем через cos/sin для точности
                    double sinValue = Math.Sin(x);
                    double cosValue = Math.Cos(x);

                    if (Math.Abs(sinValue) < 1e-15)
                        return double.NaN;

                    double result = cosValue / sinValue;

                    if (double.IsNaN(result) || double.IsInfinity(result))
                        return double.NaN;

                    return result;
                });

                // Логарифмические функции
                expression = ProcessFunction(expression, "ln", x => {
                    if (x <= 0) return double.NaN;
                    double result = Math.Log(x);
                    if (double.IsNaN(result) || double.IsInfinity(result))
                        return double.NaN;
                    return result;
                });

                expression = ProcessFunction(expression, "log", x => {
                    if (x <= 0) return double.NaN;
                    double result = Math.Log10(x);
                    if (double.IsNaN(result) || double.IsInfinity(result))
                        return double.NaN;
                    return result;
                });

                // КУБИЧЕСКИЙ КОРЕНЬ КАК ОБЫЧНАЯ ФУНКЦИЯ!
                expression = ProcessFunction(expression, "cbrt", x => {
                    if (x == 0) return 0;

                    if (x > 0)
                    {
                        return Math.Pow(x, 1.0 / 3.0);
                    }
                    else
                    {
                        // Для отрицательных: ∛(-8) = -∛(8) = -2
                        return -Math.Pow(-x, 1.0 / 3.0);
                    }
                });

                return expression;
            }
            catch
            {
                return expression;
            }
        }

        private string ProcessFunction(string expression, string functionName, Func<double, double> function)
        {
            int iterations = 0;
            while (expression.Contains($"{functionName}(") && iterations < 20)
            {
                // Ищем самое внутреннее выражение функции (справа налево)
                int funcIndex = expression.LastIndexOf($"{functionName}(");
                if (funcIndex == -1) break;

                // Находим соответствующую закрывающую скобку
                int openParens = 0;
                int closeIndex = -1;

                for (int i = funcIndex + functionName.Length + 1; i < expression.Length; i++)
                {
                    if (expression[i] == '(')
                        openParens++;
                    else if (expression[i] == ')')
                    {
                        if (openParens == 0)
                        {
                            closeIndex = i;
                            break;
                        }
                        openParens--;
                    }
                }

                if (closeIndex == -1) break;

                // Извлекаем выражение внутри функции
                string innerExpression = expression.Substring(funcIndex + functionName.Length + 1,
                                                             closeIndex - funcIndex - functionName.Length - 1);

                try
                {
                    double value;

                    // Рекурсивно обрабатываем сложные выражения
                    if (innerExpression.Contains("sin") || innerExpression.Contains("cos") ||
                        innerExpression.Contains("tan") || innerExpression.Contains("cot") ||
                        innerExpression.Contains("ln") || innerExpression.Contains("log") ||
                        innerExpression.Contains("√") || innerExpression.Contains("cbrt") ||
                        innerExpression.Contains("fact") || innerExpression.Contains("^"))
                    {
                        value = Calculate(innerExpression);
                    }
                    else
                    {
                        // Простое числовое выражение
                        var table = new DataTable();
                        var innerResult = table.Compute(innerExpression, null);
                        value = Convert.ToDouble(innerResult);
                    }

                    // Проверяем входное значение
                    if (double.IsNaN(value) || double.IsInfinity(value))
                    {
                        expression = expression.Substring(0, funcIndex) + "ERROR_VALUE" + expression.Substring(closeIndex + 1);
                        break;
                    }

                    double result = function(value);

                    // Проверяем результат функции
                    if (double.IsNaN(result) || double.IsInfinity(result))
                    {
                        expression = expression.Substring(0, funcIndex) + "ERROR_VALUE" + expression.Substring(closeIndex + 1);
                        break;
                    }

                    string resultStr = result.ToString("G15", CultureInfo.InvariantCulture);

                    // Заменяем функция(выражение) на результат
                    expression = expression.Substring(0, funcIndex) + resultStr + expression.Substring(closeIndex + 1);
                }
                catch
                {
                    expression = expression.Substring(0, funcIndex) + "ERROR_VALUE" + expression.Substring(closeIndex + 1);
                    break;
                }

                iterations++;
            }

            return expression;
        }

        private string ProcessSqrtFunction(string expression)
        {
            int iterations = 0;
            while (expression.Contains("√(") && iterations < 20)
            {
                int sqrtIndex = expression.LastIndexOf("√(");
                if (sqrtIndex == -1) break;

                // Находим соответствующую закрывающую скобку
                int openParens = 0;
                int closeIndex = -1;

                for (int i = sqrtIndex + 2; i < expression.Length; i++)
                {
                    if (expression[i] == '(')
                        openParens++;
                    else if (expression[i] == ')')
                    {
                        if (openParens == 0)
                        {
                            closeIndex = i;
                            break;
                        }
                        openParens--;
                    }
                }

                if (closeIndex == -1) break;

                string innerExpression = expression.Substring(sqrtIndex + 2, closeIndex - sqrtIndex - 2);

                try
                {
                    double value;

                    if (innerExpression.Contains("sin") || innerExpression.Contains("cos") ||
                        innerExpression.Contains("tan") || innerExpression.Contains("cot") ||
                        innerExpression.Contains("ln") || innerExpression.Contains("log") ||
                        innerExpression.Contains("√") || innerExpression.Contains("cbrt") ||
                        innerExpression.Contains("fact") || innerExpression.Contains("^"))
                    {
                        value = Calculate(innerExpression);
                    }
                    else
                    {
                        var table = new DataTable();
                        var innerResult = table.Compute(innerExpression, null);
                        value = Convert.ToDouble(innerResult);
                    }

                    // ПРОВЕРКА НА КОРЕНЬ ИЗ ОТРИЦАТЕЛЬНОГО
                    if (value < 0 || double.IsNaN(value) || double.IsInfinity(value))
                    {
                        expression = expression.Substring(0, sqrtIndex) + "ERROR_VALUE" + expression.Substring(closeIndex + 1);
                        break;
                    }

                    double sqrtResult = Math.Sqrt(value);

                    if (double.IsNaN(sqrtResult) || double.IsInfinity(sqrtResult))
                    {
                        expression = expression.Substring(0, sqrtIndex) + "ERROR_VALUE" + expression.Substring(closeIndex + 1);
                        break;
                    }

                    string resultStr = sqrtResult.ToString("G15", CultureInfo.InvariantCulture);

                    expression = expression.Substring(0, sqrtIndex) + resultStr + expression.Substring(closeIndex + 1);
                }
                catch
                {
                    expression = expression.Substring(0, sqrtIndex) + "ERROR_VALUE" + expression.Substring(closeIndex + 1);
                    break;
                }

                iterations++;
            }

            return expression;
        }

        private string ProcessFactorialFunction(string expression)
        {
            int iterations = 0;
            while (expression.Contains("fact(") && iterations < 20)
            {
                int factIndex = expression.LastIndexOf("fact(");
                if (factIndex == -1) break;

                // Находим соответствующую закрывающую скобку
                int openParens = 0;
                int closeIndex = -1;

                for (int i = factIndex + 5; i < expression.Length; i++)
                {
                    if (expression[i] == '(')
                        openParens++;
                    else if (expression[i] == ')')
                    {
                        if (openParens == 0)
                        {
                            closeIndex = i;
                            break;
                        }
                        openParens--;
                    }
                }

                if (closeIndex == -1) break;

                string innerExpression = expression.Substring(factIndex + 5, closeIndex - factIndex - 5);

                try
                {
                    double value;

                    if (innerExpression.Contains("sin") || innerExpression.Contains("cos") ||
                        innerExpression.Contains("tan") || innerExpression.Contains("cot") ||
                        innerExpression.Contains("ln") || innerExpression.Contains("log") ||
                        innerExpression.Contains("√") || innerExpression.Contains("cbrt") ||
                        innerExpression.Contains("fact") || innerExpression.Contains("^"))
                    {
                        value = Calculate(innerExpression);
                    }
                    else
                    {
                        var table = new DataTable();
                        var innerResult = table.Compute(innerExpression, null);
                        value = Convert.ToDouble(innerResult);
                    }

                    if (double.IsNaN(value) || double.IsInfinity(value))
                    {
                        expression = expression.Substring(0, factIndex) + "ERROR_VALUE" + expression.Substring(closeIndex + 1);
                        break;
                    }

                    double factResult = CalculateFactorial(value);

                    // ПРОВЕРЯЕМ РЕЗУЛЬТАТ ФАКТОРИАЛА
                    if (double.IsNaN(factResult) || double.IsInfinity(factResult))
                    {
                        expression = expression.Substring(0, factIndex) + "ERROR_VALUE" + expression.Substring(closeIndex + 1);
                        break;
                    }

                    string resultStr = factResult.ToString("G15", CultureInfo.InvariantCulture);

                    expression = expression.Substring(0, factIndex) + resultStr + expression.Substring(closeIndex + 1);
                }
                catch
                {
                    expression = expression.Substring(0, factIndex) + "ERROR_VALUE" + expression.Substring(closeIndex + 1);
                    break;
                }

                iterations++;
            }

            return expression;
        }

        private string ProcessPowerOperations(string expression)
        {
            // Обрабатываем операции возведения в степень справа налево
            string pattern = @"(\d+(?:\.\d+)?(?:[Ee][+-]?\d+)?)\^(\d+(?:\.\d+)?(?:[Ee][+-]?\d+)?)";

            int iterations = 0;
            while (Regex.IsMatch(expression, pattern) && iterations < 20)
            {
                var matches = Regex.Matches(expression, pattern);
                var lastMatch = matches[matches.Count - 1];

                string baseValue = lastMatch.Groups[1].Value;
                string exponent = lastMatch.Groups[2].Value;

                if (double.TryParse(baseValue, NumberStyles.Float, CultureInfo.InvariantCulture, out double baseNum) &&
                    double.TryParse(exponent, NumberStyles.Float, CultureInfo.InvariantCulture, out double expNum))
                {
                    try
                    {
                        double result = Math.Pow(baseNum, expNum);

                        // ПРОВЕРЯЕМ РЕЗУЛЬТАТ СТЕПЕНИ НА ВСЕ ВИДЫ ОШИБОК
                        if (double.IsNaN(result) || double.IsInfinity(result))
                        {
                            expression = expression.Substring(0, lastMatch.Index) + "ERROR_VALUE" +
                                       expression.Substring(lastMatch.Index + lastMatch.Length);
                            break;
                        }

                        string resultStr = result.ToString("G15", CultureInfo.InvariantCulture);
                        expression = expression.Substring(0, lastMatch.Index) + resultStr +
                                   expression.Substring(lastMatch.Index + lastMatch.Length);
                    }
                    catch
                    {
                        expression = expression.Substring(0, lastMatch.Index) + "ERROR_VALUE" +
                                   expression.Substring(lastMatch.Index + lastMatch.Length);
                        break;
                    }
                }
                else
                {
                    break;
                }

                iterations++;
            }

            return expression;
        }

        #endregion
    }
}