using System.ComponentModel.DataAnnotations;

namespace Calculator.Models
{
    public class DataInputVariant
    {
        [Key]
        public int ID_DataInputVariant { get; set; }
        public double Operand_1 { get; set; }
        public double Operand_2 { get; set; }
        public string Type_operation { get; set; } = string.Empty;
        public double Result { get; set; }
    }
}
