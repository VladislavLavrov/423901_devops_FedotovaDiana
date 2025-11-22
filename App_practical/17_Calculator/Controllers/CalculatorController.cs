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
            _logger = logger;
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
                // Преобразуем символ операции в enum
                Operation op = operation switch
                {
                    "+" => Operation.Add,
                    "-" => Operation.Subtract,
                    "*" => Operation.Multiply,
                    "/" => Operation.Divide,
                    _ => throw new ArgumentException("Неверная операция")
                };

                var dataInputVariant = new DataInputVariant
                {
                    Operand_1 = num1,
                    Operand_2 = num2,
                    Type_operation = op
                };

                // --- Отправка в Kafka (если доступна) ---
                try
                {
                    await SendDataToKafka(dataInputVariant);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Kafka недоступна — пропускаем отправку");
                }

                // --- Мгновенный расчёт для UI ---
                var result = CalculatorLibrary.CalculateOperation(num1, num2, op);

                return Json(new { result = result.ToString() });
            }
            catch (ArgumentException ex)
            {
                return Json(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в Calculate()");
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
                _logger.LogError(ex, "Ошибка при сохранении результата");
                return StatusCode(500, "Ошибка сохранения");
            }
        }

        private async Task SendDataToKafka(DataInputVariant dataInputVariant)
        {
            var json = JsonSerializer.Serialize(dataInputVariant);
            try
            {
                var deliveryResult = await _producer.ProduceAsync("Fedotova", new Message<Null, string> { Value = json });
                _logger.LogInformation("Сообщение отправлено: {Topic} / Partition {Partition} / Offset {Offset}",
                    deliveryResult.Topic, deliveryResult.Partition, deliveryResult.Offset);
            }
            catch (ProduceException<Null, string> ex)
            {
                _logger.LogError(ex, "Ошибка при отправке сообщения в Kafka");
            }
        }

    }
}

