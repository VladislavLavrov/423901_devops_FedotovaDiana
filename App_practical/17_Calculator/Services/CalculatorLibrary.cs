namespace Calculator.Services
{
    public static class CalculatorLibrary
    {
        public static double CalculateOperation(double a, double b, string operation)
        {
            return operation switch
            {
                "+" or "Add" => a + b,
                "-" or "Subtract" => a - b,
                "*" or "Multiply" => a * b,
                "/" or "Divide" => b == 0 ? double.NaN : a / b,
                _ => double.NaN
            };
        }
    }
}
