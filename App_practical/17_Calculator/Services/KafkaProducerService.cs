using Confluent.Kafka;
using System;
using System.Threading.Tasks;

namespace Calculator.Services
{
    public class KafkaProducerService<K, V>
    {
        IProducer<K, V> kafkaHandle;

        public KafkaProducerService(KafkaProducerHandler handle)
        {
            // В методичке используется DependentProducerBuilder для создания продюсера
            kafkaHandle = new DependentProducerBuilder<K, V>(handle.Handle).Build();
        }

        /// <summary>
        /// Асинхронная отправка сообщения (ожидаем подтверждения).
        /// </summary>
        public Task ProduceAsync(string topic, Message<K, V> message)
            => kafkaHandle.ProduceAsync(topic, message);

        /// <summary>
        /// Отправка сообщения без ожидания результата (callback).
        /// </summary>
        public void Produce(string topic, Message<K, V> message, Action<DeliveryReport<K, V>> deliveryHandler = null)
            => kafkaHandle.Produce(topic, message, deliveryHandler);

        public void Flush(TimeSpan timeout)
            => kafkaHandle.Flush(timeout);
    }
}

