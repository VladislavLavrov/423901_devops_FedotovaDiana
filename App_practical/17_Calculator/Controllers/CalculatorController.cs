using Calculator.Data;
using Calculator.Models;
using Calculator.Services;
using Confluent.Kafka;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Calculator.Controllers
{
    [Route("Calculator")]
    public class CalculatorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly KafkaProducerService<Null, string> _producer;

        public CalculatorController(ApplicationDbContext context, KafkaProducerService<Null, string> producer)
        {
            _context = context;
            _producer = producer;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            var history = _context.DataInputVariants
                .OrderByDescending(x => x.ID_DataInputVariant)
                .ToList();

            return View(history);
        }

        [HttpPost("Calculate")]
        public async Task<IActionResult> Calculate([FromBody] CalculationRequest request)
        {
            var variant = new DataInputVariant
            {
                Operand_1 = request.Num1,
                Operand_2 = request.Num2,
                Type_operation = request.Operation
            };

            // Сохраняем временно с Result = null
            _context.DataInputVariants.Add(variant);
            await _context.SaveChangesAsync();

            // Отправка в Kafka
            await SendDataToKafka(variant);

            // Возвращаем ID созданного объекта
            return Ok(new { id = variant.ID_DataInputVariant });
        }
        [HttpGet("Result/{id}")]
        public IActionResult GetResult(int id)
        {
            var variant = _context.DataInputVariants
                .FirstOrDefault(x => x.ID_DataInputVariant == id);

            if (variant == null)
                return NotFound();

            return Ok(new
            {
                id = variant.ID_DataInputVariant,
                num1 = variant.Operand_1,
                num2 = variant.Operand_2,
                operation = variant.Type_operation,
                result = variant.Result
            });
        }

        private Task SendDataToKafka(DataInputVariant data)
        {
            var json = JsonSerializer.Serialize(data);

            return _producer.ProduceAsync(
                "Fedotova",                                 
                new Message<Null, string> { Value = json }
            );
        }

        // callback вызывается consumer-ом после расчёта
        [HttpPost("Callback")]
        public IActionResult Callback([FromBody] DataInputVariant model)
        {
            var variant = _context.DataInputVariants
                .FirstOrDefault(x => x.ID_DataInputVariant == model.ID_DataInputVariant);

            if (variant != null)
            {
                variant.Result = model.Result;
                _context.SaveChanges();
            }

            return Ok();
        }

    }
}


