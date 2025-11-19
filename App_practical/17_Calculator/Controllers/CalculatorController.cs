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
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Calculate(double num1, double num2, Operation operation)
        {
            var dataInputVariant = new DataInputVariant
            {
                Operand_1 = num1,
                Operand_2 = num2,
                Type_operation = operation
            };

            // Отправка данных в Kafka
            await SendDataToKafka(dataInputVariant);

            return RedirectToAction(nameof(Index));

            // Вычисляем результат сразу
            var result = CalculatorLibrary.CalculateOperation(num1, num2, operation);
            dataInputVariant.Result = result.ToString();

            // Сохраняем в базу
            _context.DataInputVariants.Add(dataInputVariant);
            _context.SaveChanges();

            // Отправка данных в Kafka
            await SendDataToKafka(dataInputVariant);

            // Возвращаем JSON для AJAX
            return Json(new { result = dataInputVariant.Result });


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

