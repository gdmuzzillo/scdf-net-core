using Confluent.Kafka;
using Streamiz.Kafka.Net.Errors;
using Streamiz.Kafka.Net.Kafka;
using Helpers;
namespace simple_netcore_source.Helpers
{
    public class SyncKafkaSupplier : IKafkaSupplier
    {
          SyncProducer producer = null;

        public virtual IAdminClient GetAdmin(AdminClientConfig config) => new SyncAdminClient();

        public virtual IConsumer<byte[], byte[]> GetConsumer(ConsumerConfig config, IConsumerRebalanceListener rebalanceListener)
        {
            var consumer = new SyncConsumer(config, producer);
            consumer.SetRebalanceListener(rebalanceListener);
            return consumer;
        }

        public virtual IConsumer<byte[], byte[]> GetGlobalConsumer(ConsumerConfig config)
            => GetConsumer(config, null);

        public virtual IProducer<byte[], byte[]> GetProducer(ProducerConfig config)
        {
            if (producer == null)
                producer = new SyncProducer(config);
            return producer;
        }

public virtual IConsumer<byte[], byte[]> GetRestoreConsumer(ConsumerConfig config)
            => GetConsumer(config, null);
    }
}