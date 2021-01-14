using Confluent.Kafka;
using Streamiz.Kafka.Net.Kafka;
using System;

namespace simple_netcore_source.Helpers
{
    public class SyncKafkaSupplier : IKafkaSupplier
    {
        private readonly KafkaLoggerAdapter loggerAdapter = null;

        public SyncKafkaSupplier(KafkaLoggerAdapter loggerAdapter)
        {
            if (loggerAdapter == null)
                throw new ArgumentNullException(nameof(loggerAdapter));

            this.loggerAdapter = loggerAdapter;
        }


        public virtual IAdminClient GetAdmin(AdminClientConfig config)
        {
            AdminClientBuilder builder = new AdminClientBuilder(config);
            builder.SetLogHandler(loggerAdapter.LogAdmin);
            builder.SetErrorHandler(loggerAdapter.ErrorAdmin);
            return builder.Build();
        }

        public virtual IConsumer<byte[], byte[]> GetConsumer(ConsumerConfig config, IConsumerRebalanceListener rebalanceListener)
        {
            ConsumerBuilder<byte[], byte[]> builder = new ConsumerBuilder<byte[], byte[]>(config);
            if (rebalanceListener != null)
            {
                builder.SetPartitionsAssignedHandler((c, p) => rebalanceListener.PartitionsAssigned(c, p));
                builder.SetPartitionsRevokedHandler((c, p) => rebalanceListener.PartitionsRevoked(c, p));
                builder.SetLogHandler(loggerAdapter.LogConsume);
                builder.SetErrorHandler(loggerAdapter.ErrorConsume);
            }
            return builder.Build();
        }
        public virtual IConsumer<byte[], byte[]> GetGlobalConsumer(ConsumerConfig config)
        {
            ConsumerBuilder<byte[], byte[]> builder = new ConsumerBuilder<byte[], byte[]>(config);
            // TOOD : Finish
            builder.SetLogHandler(loggerAdapter.LogConsume);
            builder.SetErrorHandler(loggerAdapter.ErrorConsume);
            return builder.Build();
        }
        public virtual IProducer<byte[], byte[]> GetProducer(ProducerConfig config)
        {
            ProducerBuilder<byte[], byte[]> builder = new ProducerBuilder<byte[], byte[]>(config);
            builder.SetLogHandler(loggerAdapter.LogProduce);
            builder.SetErrorHandler(loggerAdapter.ErrorProduce);
            return builder.Build();
        }
        public virtual IConsumer<byte[], byte[]> GetRestoreConsumer(ConsumerConfig config)
        {
            ConsumerBuilder<byte[], byte[]> builder = new ConsumerBuilder<byte[], byte[]>(config);
            // TOOD : Finish
            builder.SetLogHandler(loggerAdapter.LogConsume);
            builder.SetErrorHandler(loggerAdapter.ErrorConsume);
            return builder.Build();
        }
    }
}