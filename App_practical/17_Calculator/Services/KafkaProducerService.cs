using Confluent.Kafka;
using System.Threading.Tasks;

namespace Calculator.Services
{
    public class KafkaProducerService<K, V>
    {
        private readonly IProducer<K, V> _producer;

        public KafkaProducerService(IProducer<K, V> producer)
        {
            _producer = producer;
        }

        public Task ProduceAsync(string topic, Message<K, V> message)
            => _producer.ProduceAsync(topic, message);
    }
}


