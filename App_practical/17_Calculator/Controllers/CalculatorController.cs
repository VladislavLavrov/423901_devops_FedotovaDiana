using Calculator.Data;
using Calculator.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Calculator.Services;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace Calculator.Controllers
{
    public class CalculatorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly KafkaProducerService<Null, string> _producer;
        private readonly ILogger<CalculatorController> _logger;

        public CalculatorController(
            ApplicationDbContext context,
            KafkaProducerService<Null, string> producer,
            ILogger<CalculatorController> logger)
        {
            _context = context;
            _producer = producer;
            _logger = logger; // теперь правильно
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

                try
                {
                    await SendDataToKafka(dataInputVariant);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при отправке данных в Kafka");
                    return StatusCode(500, "Ошибка Kafka");
                }

                // Расчёт результата сразу для фронтенда
                var result = CalculatorLibrary.CalculateOperation(num1, num2, op);

                return Json(new { result = result.ToString() });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в методе Calculate");
                return Json(new { message = "Произошла ошибка на сервере" });
            }
        }

        [HttpPost]
        public IActionResult Callback([FromBody] DataInputVariant inputData)
        {
            try
            {
                _context.DataInputVariants.Add(inputData);
                _context.SaveChanges();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при сохранении результата в базу");
                return StatusCode(500, "Ошибка сохранения");
            }
        }

        private async Task SendDataToKafka(DataInputVariant dataInputVariant)
        {
            var json = JsonSerializer.Serialize(dataInputVariant);
            await _producer.ProduceAsync("Fedotova", new Message<Null, string> { Value = json });
        }
    }
}

