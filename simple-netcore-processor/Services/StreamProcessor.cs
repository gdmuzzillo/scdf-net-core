using System;
using System.Threading;
using System.Threading.Tasks;
using com.avro.bean;
using Microsoft.Extensions.Configuration;
using Streamiz.Kafka.Net;
using Streamiz.Kafka.Net.SerDes;
using Streamiz.Kafka.Net.Stream;
using Streamiz.Kafka.Net.Table;
using Microsoft.Extensions.Hosting;
using Streamiz.Kafka.Net.SchemaRegistry.SerDes.Avro;
using Confluent.Kafka;
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
        public async Task process(IConfiguration config)
        {

            Console.WriteLine("Process");
            var sConfig = new StreamConfig<StringSerDes, StringSerDes>();


            sConfig.ApplicationId = config["SPRING_CLOUD_APPLICATION_GROUP"];
            sConfig.BootstrapServers = config["SPRING_CLOUD_STREAM_KAFKA_BINDER_BROKERS"];
            sConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
            sConfig.SchemaRegistryUrl = config["SchemaRegistryUrl"];
            sConfig.AutoRegisterSchemas = true;
            sConfig.NumStreamThreads = 1;
            sConfig.Acks = Acks.All;
            sConfig.Debug = "consumer,cgrp,topic,fetch";
            sConfig.AddConsumerConfig("allow.auto.create.topics", "true");
            sConfig.MaxTaskIdleMs = 50;
            sConfig.InnerExceptionHandler = (e) => ExceptionHandlerResponse.CONTINUE;


            var timeout = TimeSpan.FromSeconds(10);
            DateTime dt = DateTime.Now;

            OrderProduct op = new OrderProduct();

            var serializer = new SchemaAvroSerDes<OrderProduct>();

            StreamBuilder builder = new StreamBuilder();

            var table = builder.Table(config["simpleNetcoreProcessor.externaltopic"],
                                new Int32SerDes(),
                                new SchemaAvroSerDes<Product>(),
                                InMemory<int, Product>.As(config["simpleNetcoreProcessor.table"]));

            builder.Stream<int, Order, Int32SerDes, SchemaAvroSerDes<Order>>(config["spring.cloud.stream.bindings.input.destination"])
                    .Join(table, (order, product) =>
                    {
                        Console.WriteLine("Order: " + order?.order_id);
                        Console.WriteLine("Product: " + product?.product_id);

                        op = new OrderProduct
                        {
                            order_id = order.order_id,
                            price = order.price,
                            product_id = product.product_id,
                            product_name = product.name,
                            product_price = product.price
                        };
                        return op;
                    } )
            .To<Int32SerDes, SchemaAvroSerDes<OrderProduct>>(config["spring.cloud.stream.bindings.output.destination"]);
            Topology t = builder.Build();
            
            Console.WriteLine(t.Describe());

            

            KafkaStream stream = new KafkaStream(t, sConfig);

            bool isRunningState = false;

            stream.StateChanged += (old, @new) =>
            {
                if (@new.Equals(KafkaStream.State.RUNNING))
                {
                    isRunningState = true;
                }
            };

            await stream.StartAsync();

            while (!isRunningState)
            {
                Thread.Sleep(250);
                if (DateTime.Now > dt + timeout)
                {
                    break;
                }
            }

            if (isRunningState)
            {
                Console.WriteLine("Stream running state is " + isRunningState.ToString() );
            }
        }
    }
}