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

            // отправка в Kafka
            await SendDataToKafka(variant);

            return Ok(new { message = "Данные отправлены в Kafka" });
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
            _context.DataInputVariants.Add(model);
            _context.SaveChanges();

            return Ok();
        }
    }
}


