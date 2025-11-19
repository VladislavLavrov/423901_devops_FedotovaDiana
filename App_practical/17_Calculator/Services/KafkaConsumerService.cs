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
        private readonly string _topic;

        public KafkaConsumerService(IConfiguration config, IServiceProvider serviceProvider, IHttpClientFactory clientFactory)
        {
            var consumerConfig = new ConsumerConfig();
            config.GetSection("Kafka:ConsumerSettings").Bind(consumerConfig);
            _topic = config.GetValue<string>("Kafka:TopicName");
            _consumer = new ConsumerBuilder<Null, string>(consumerConfig).Build();
            _serviceProvider = serviceProvider;
            _clientFactory = clientFactory;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(() =>
            {
                _consumer.Subscribe(_topic);
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var cr = _consumer.Consume(stoppingToken);
                        var inputData = JsonSerializer.Deserialize<DataInputVariant>(cr.Message.Value);

                        // Расчет результата
                        inputData.Result = CalculatorLibrary.CalculateOperation(inputData.Operand_1, inputData.Operand_2, inputData.Type_operation).ToString();

                        // Отправка на Callback
                        var httpClient = _clientFactory.CreateClient();
                        httpClient.PostAsJsonAsync($"http://app:5017/Calculator/Callback", inputData).Wait();
                    }
                    catch (OperationCanceledException) { break; }
                    catch (ConsumeException) { break; }
                }
            }, stoppingToken);
        }

        public override void Dispose()
        {
            _consumer.Close();
            _consumer.Dispose();
            base.Dispose();
        }
    }
}

