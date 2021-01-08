using System;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Streamiz.Kafka.Net;
using Streamiz.Kafka.Net.SerDes;
using Streamiz.Kafka.Net.Stream;
using Streamiz.Kafka.Net.Table;
using Microsoft.Extensions.Hosting;

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
            var sConfig = new StreamConfig<SchemaAvroSerDes, SchemaAvroSerDes>();

            sConfig.ApplicationId = config["SPRING_CLOUD_APPLICATION_GUID"];
            sConfig.BootstrapServers = config["SPRING_CLOUD_STREAM_KAFKA_BINDER_BROKERS"];

            sConfig.AutoRegisterSchemas = true;
            sConfig.SchemaRegistryUrl = config["schemaRegistry"];
            


            StreamBuilder builder = new StreamBuilder();

            var table = builder.Table(config["simpleNetcoreProcessor.externaltopic"],
                                new Int32SerDes(),
                                new SchemaAvroSerDes<Product>(),
                                InMemory<String,String>.As(config["simpleNetcoreProcessor.table"]));

            builder.Stream<String, String, StringSerDes, StringSerDes>(config["spring.cloud.stream.bindings.input.destination"])
                    .Join(table, (order, product) => order + product)
            .To(config["spring.cloud.stream.bindings.output.destination"]);

            Topology t = builder.Build();
            KafkaStream stream = new KafkaStream(t, sConfig);

            await stream.StartAsync();
        }
    }
}