using Calculator.Data;
using Calculator.Models;
using Microsoft.AspNetCore.Mvc;

namespace Calculator.Controllers
{
    public class CalculatorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CalculatorController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Calculate(double num1, double num2, string operation)
        {
            double result = 0;
            string message = "";

            switch (operation)
            {
                case "+": result = num1 + num2; break;
                case "-": result = num1 - num2; break;
                case "*": result = num1 * num2; break;
                case "/":
                    if (num2 == 0)
                        message = "Ошибка: деление на ноль!";
                    else
                        result = num1 / num2;
                    break;
                default:
                    message = "Неизвестная операция!";
                    break;
            }

            // Сохраняем результат в базу данных
            if (string.IsNullOrEmpty(message))
            {
                var record = new DataInputVariant
                {
                    Operand_1 = num1,
                    Operand_2 = num2,
                    Type_operation = operation,
                    Result = result
                };

                _context.DataInputVariants.Add(record);
                _context.SaveChanges();
            }

            return Json(new { result, message });
        }
    }
}

