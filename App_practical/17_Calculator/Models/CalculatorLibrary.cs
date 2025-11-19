using Calculator.Data;

namespace Calculator.Models
{
    public static class CalculatorLibrary
    {
        public static double CalculateOperation(double num1, double num2, Operation operation)
        {
            return operation switch
            {
                Operation.Add => num1 + num2,
                Operation.Subtract => num1 - num2,
                Operation.Multiply => num1 * num2,
                Operation.Divide => num2 != 0 ? num1 / num2 : throw new DivideByZeroException(),
                _ => throw new ArgumentOutOfRangeException(nameof(operation), "Invalid operation")
            };
        }
    }
}

