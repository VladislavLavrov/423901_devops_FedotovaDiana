using Microsoft.AspNetCore.Mvc;

namespace Calculator.Controllers
{
    public class CalculatorController : Controller
    {
        // Отображение страницы
        public IActionResult Index()
        {
            return View();
        }

        // Серверный метод для вычислений (AJAX)
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

            return Json(new { result, message });
        }
    }
}


