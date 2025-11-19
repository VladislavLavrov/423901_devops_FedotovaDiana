using Calculator.Data;
using Calculator.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Calculator.Services;
using Confluent.Kafka;

namespace Calculator.Controllers
{

    public class CalculatorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly KafkaProducerService<Null, string> _producer;

        public CalculatorController(ApplicationDbContext context, KafkaProducerService<Null, string> producer)
        {

            _context = context;
            _producer = producer;
        }

        public IActionResult Index()
        {
            var data = _context.DataInputVariants
                        .OrderByDescending(x => x.ID_DataInputVariant)
                        .ToList();
            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> Calculate(double num1, double num2, string operation)
        {
            try
            {
                // Преобразуем строку операции в enum
                if (!Enum.TryParse<Operation>(operation, true, out var op))
                    return Json(new { message = "Неверная операция" });

                var dataInputVariant = new DataInputVariant
                {
                    Operand_1 = num1,
                    Operand_2 = num2,
                    Type_operation = op
                };

                // Отправка в Kafka
                await SendDataToKafka(dataInputVariant);

                // Расчёт результата сразу для фронтенда
                var result = CalculatorLibrary.CalculateOperation(num1, num2, op);

                return Json(new { result = result.ToString() });
            }
            catch (Exception ex)
            {
                // Логируем ошибку в консоль (или через ILogger, если есть)
                Console.WriteLine($"Ошибка в Calculate: {ex.Message}\n{ex.StackTrace}");
                return Json(new { message = "Произошла ошибка на сервере" });
            }
        }



        [HttpPost]
        public IActionResult Callback([FromBody] DataInputVariant inputData)
        {
            // Сохранение результата в базу
            _context.DataInputVariants.Add(inputData);
            _context.SaveChanges();
            return Ok();
        }

        private async Task SendDataToKafka(DataInputVariant dataInputVariant)
        {
            var json = JsonSerializer.Serialize(dataInputVariant);
            await _producer.ProduceAsync("Fedotova", new Message<Null, string> { Value = json });
        }
    }
}

