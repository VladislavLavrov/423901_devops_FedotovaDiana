using Confluent.Kafka;
using Microsoft.Extensions.Configuration;

namespace Calculator.Services
{
    public class KafkaProducerHandler
    {
        public IProducer<Null, string> Producer { get; private set; }

        public KafkaProducerHandler(IConfiguration config)
        {
            var producerConfig = new ProducerConfig();
            config.GetSection("Kafka:ProducerSettings").Bind(producerConfig);

            Producer = new ProducerBuilder<Null, string>(producerConfig).Build();
        }
    }
}
