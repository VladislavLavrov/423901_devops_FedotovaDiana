namespace Calculator.Models
{
    // Модель данных для хранения операций калькулятора
    public class DataInputVariant
    {
        public int ID_DataInputVariant { get; set; }   // Первичный ключ
        public double Operand_1 { get; set; }          // Первый операнд
        public double Operand_2 { get; set; }          // Второй операнд
        public string Type_operation { get; set; } = string.Empty;  // Тип операции (+, -, *, /)
        public double Result { get; set; }             // Результат вычисления
    }
}
