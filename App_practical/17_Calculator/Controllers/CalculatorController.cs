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

        [HttpPost("Calculate")]
        public JsonResult Calculate([FromBody] CalculationRequest request)
        {
            double result = 0;
            string message = "";

            switch (request.Operation)
            {
                case "+": result = request.Num1 + request.Num2; break;
                case "-": result = request.Num1 - request.Num2; break;
                case "*": result = request.Num1 * request.Num2; break;
                case "/":
                    if (request.Num2 == 0)
                        message = "Ошибка: деление на ноль!";
                    else
                        result = request.Num1 / request.Num2;
                    break;
                default:
                    message = "Неизвестная операция!";
                    break;
            }

            // Сохраняем в базу, если нет ошибки
            if (string.IsNullOrEmpty(message))
            {
                var record = new DataInputVariant
                {
                    Operand_1 = request.Num1,
                    Operand_2 = request.Num2,
                    Type_operation = request.Operation,
                    Result = result
                };

                _context.DataInputVariants.Add(record);
                _context.SaveChanges();
            }

            return Json(new { result, message });
        }
    }
}


