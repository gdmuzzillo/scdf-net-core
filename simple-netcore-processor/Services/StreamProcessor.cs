using System;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using com.avro.bean;
using Microsoft.Extensions.Configuration;
using Streamiz.Kafka.Net;
using Streamiz.Kafka.Net.SerDes;
using Streamiz.Kafka.Net.Stream;
using Streamiz.Kafka.Net.Table;
using Microsoft.Extensions.Hosting;
using Streamiz.Kafka.Net.SchemaRegistry.SerDes.Avro;

namespace simple_netcore_processor.Services {
    public class StreamProcessor : BackgroundService, IStreamProcessor {

        private readonly IConfiguration _config;
        public StreamProcessor(IConfiguration config)
        {
            _config = config;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await process(_config);
        }
        public async Task process (IConfiguration config) {

            Console.WriteLine("Process");
            var sConfig = new StreamConfig<StringSerDes, SchemaAvroSerDes<Order>>();

            sConfig.ApplicationId = config["SPRING_CLOUD_APPLICATION_GUID"];
            sConfig.BootstrapServers = config["SPRING_CLOUD_STREAM_KAFKA_BINDER_BROKERS"];

            sConfig.AutoRegisterSchemas = true;
            sConfig.SchemaRegistryUrl = config["SchemaRegistryUrl"];
            
            StreamBuilder builder = new StreamBuilder();

            var table = builder.Table(config["simpleNetcoreProcessor.externaltopic"],
                                new StringSerDes(),
                                new SchemaAvroSerDes<Product>(),
                                InMemory<String,Product>.As(config["simpleNetcoreProcessor.table"]));

            builder.Stream<string, Order, StringSerDes, SchemaAvroSerDes<Order>>(config["spring.cloud.stream.bindings.input.destination"])
                    .Join(table, (order, product) =>new OrderProduct
                    {
                        order_id = order.order_id,
                        price = order.price,
                        product_id = product.product_id,
                        product_name = product.name,
                        product_price = product.price
                    })
            .To<StringSerDes, SchemaAvroSerDes<OrderProduct>>(config["spring.cloud.stream.bindings.output.destination"]);

            Topology t = builder.Build();
            KafkaStream stream = new KafkaStream(t, sConfig);

            await stream.StartAsync();
        }
    }
}