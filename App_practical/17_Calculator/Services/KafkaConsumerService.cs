using Confluent.Kafka;
using Calculator.Data;
using Calculator.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Net.Http;

namespace Calculator.Services
{
    public class KafkaConsumerService : BackgroundService
    {
        private readonly IConsumer<Null, string> _consumer;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<KafkaConsumerService> _logger;
        private readonly string _topic;

        public KafkaConsumerService(
            IConfiguration config,
            IServiceProvider serviceProvider,
            IHttpClientFactory clientFactory,
            ILogger<KafkaConsumerService> logger)
        {
            var consumerConfig = new ConsumerConfig();
            config.GetSection("Kafka:ConsumerSettings").Bind(consumerConfig);

            _topic = config.GetValue<string>("Kafka:TopicName");

            _consumer = new ConsumerBuilder<Null, string>(consumerConfig).Build();

            _serviceProvider = serviceProvider;
            _clientFactory = clientFactory;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(async () =>
            {
                try
                {
                    _consumer.Subscribe(_topic);
                    _logger.LogInformation("Kafka Consumer subscribed to topic {Topic}", _topic);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Kafka unavailable — Consumer will not start");
                    return; // НЕ ПАДАЕМ!
                }

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var cr = _consumer.Consume(stoppingToken);

                        var inputData = JsonSerializer.Deserialize<DataInputVariant>(cr.Message.Value);

                        // Расчёт результата
                        inputData.Result = CalculatorLibrary.CalculateOperation(inputData.Operand_1, inputData.Operand_2, inputData.Type_operation);


                        // Отправляем результат на Callback
                        var httpClient = _clientFactory.CreateClient();
                        await httpClient.PostAsJsonAsync("http://web-app-calculator-17:5017/Calculator/Callback", inputData);
                    }
                    catch (ConsumeException ex)
                    {
                        _logger.LogError(ex, "Ошибка Kafka Consume — продолжаем работу");
                        await Task.Delay(3000);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Ошибка обработки сообщения Kafka");
                    }
                }
            }, stoppingToken);
        }

        public override void Dispose()
        {
            try
            {
                _consumer.Close();
                _consumer.Dispose();
            }
            catch { }

            base.Dispose();
        }
    }
}
