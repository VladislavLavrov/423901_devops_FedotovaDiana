using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Calculator.Models
{
    public class DataInputVariant
    {
        [Key]
        public int ID_DataInputVariant { get; set; }
        public double Operand_1 { get; set; }
        public double Operand_2 { get; set; }
        public Operation Type_operation { get; set; }

        [Column(TypeName = "varchar(128)")]
        public double? Result { get; set; }
    }

    public enum Operation
    {
        Add = 1,
        Subtract = 2,
        Multiply = 3,
        Divide = 4
    }
}
