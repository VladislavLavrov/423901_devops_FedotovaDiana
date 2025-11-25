using Confluent.Kafka;
using Calculator.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Calculator.Services
{
    public class KafkaConsumerService : BackgroundService
    {
        private readonly string _topic;
        private readonly IConsumer<Null, string> _kafkaConsumer;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHttpClientFactory _clientFactory;

        public KafkaConsumerService(IConfiguration config, IServiceProvider serviceProvider, IHttpClientFactory clientFactory)
        {
            // Конфигурирование настроек Kafka и инициализация компонентов
            var consumerConfig = new ConsumerConfig();
            config.GetSection("Kafka:ConsumerSettings").Bind(consumerConfig);

            // в приложении 4 имя topic берётся из Kafka:Topic-Name
            _topic = config.GetValue<string>("Kafka:TopicName");

            _kafkaConsumer = new ConsumerBuilder<Null, string>(consumerConfig).Build();
            _serviceProvider = serviceProvider;
            _clientFactory = clientFactory;
        }

        /// <summary>
        /// Выполнение работы Kafka Consumer’а.
        /// </summary>
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(() => StartConsumerLoop(stoppingToken), stoppingToken);
        }

        /// <summary>
        /// Цикл обработки сообщений из Kafka.
        /// </summary>
        private async Task StartConsumerLoop(CancellationToken cancellationToken)
        {
            _kafkaConsumer.Subscribe(_topic);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var cr = _kafkaConsumer.Consume(cancellationToken);

                    var inputData = JsonSerializer.Deserialize<DataInputVariant>(cr.Message.Value);

                    
                    var result = CalculatorLibrary.CalculateOperation(inputData.Operand_1, inputData.Operand_2, inputData.Type_operation);

                    inputData.Result = result.ToString();

                    var httpClient = _clientFactory.CreateClient();

                    
                    await httpClient.PostAsJsonAsync($"http://localhost:5017/Calculator/Callback", inputData);

                    // Логирование/дебаг
                    Console.WriteLine($"Message key: {cr.Message.Key}, value: {cr.Message.Value}");
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (ConsumeException e)
                {
                    if (e.Error.IsFatal)
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    // на случай других ошибок выходит из цикла по методичке
                    break;
                }
            }
        }

        /// <summary>
        /// Очистка ресурсов Consumer’а при завершении работы сервиса.
        /// </summary>
        public override void Dispose()
        {
            _kafkaConsumer.Close(); // фиксация оффсетов и корректный выход из группы
            _kafkaConsumer.Dispose();

            base.Dispose();
        }
    }
}
