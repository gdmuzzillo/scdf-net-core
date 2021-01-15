using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using com.avro.bean;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using simple_netcore_source.Helpers;
using Streamiz.Kafka.Net;
using Streamiz.Kafka.Net.SchemaRegistry.SerDes.Avro;
using Streamiz.Kafka.Net.SerDes;
using Streamiz.Kafka.Net.Table;

namespace simple_netcore_source.Services {
    public class NStreamSource : BackgroundService, INStreamSource {

        private readonly IConfiguration _config;
        private IServiceProvider _services;

        private IDataService _dataService;

        public NStreamSource (IConfiguration config, IServiceProvider services) {
            _config = config;
            _services = services;
        }
        protected override async Task ExecuteAsync (CancellationToken stoppingToken) {
            await exec (_config, _services);
        }

        public async Task exec (IConfiguration config, IServiceProvider services) {

            Console.WriteLine ("Process");

            var destTopic = config["spring.cloud.stream.bindings.output.destination"];

            Console.WriteLine (destTopic);

            using (var scope = services.CreateScope ()) {
                this._dataService = scope.ServiceProvider
                    .GetRequiredService<IDataService>();

                bool isRunningState = false;

                var timeout = TimeSpan.FromSeconds(10);
                DateTime dt = DateTime.Now;

                Order[] capture = this._dataService.readData();

                // Inyectamos los datos obtenidos al Stream

                var sConfig = new StreamConfig<StringSerDes, StringSerDes>();
                sConfig.ApplicationId = config["SPRING_CLOUD_APPLICATION_GROUP"];
                sConfig.BootstrapServers = config["SPRING_CLOUD_STREAM_KAFKA_BINDER_BROKERS"];
                sConfig.SchemaRegistryUrl = config["SchemaRegistryUrl"];
                sConfig.AutoRegisterSchemas = true;
                sConfig.NumStreamThreads = 10;
                sConfig.Acks = Acks.All;
              //  sConfig.AddConsumerConfig( "AllowAutoCreateTopics" , "true"  );


                var supplier = new SyncKafkaSupplier(new KafkaLoggerAdapter(sConfig));

                var producerConfig = sConfig.ToProducerConfig();
                var adminConfig = sConfig.ToAdminConfig(sConfig.ApplicationId);

                var admin = supplier.GetAdmin(adminConfig);
               
                try{

                    var topic = new TopicSpecification
                    {
                        Name = destTopic,
                        NumPartitions = 1,
                        ReplicationFactor = 3
                    };


                    IList<TopicSpecification> topics = new List<TopicSpecification>();

                    topics.Add(topic);

                    await admin.CreateTopicsAsync(topics);
                }
                catch (Exception topicExists)
                {
                    Console.WriteLine("Topic alreade exists");
                    Console.Write(topicExists);
                }

                var producer = supplier.GetProducer(producerConfig);

                StreamBuilder builder = new StreamBuilder();

                var serdes = new SchemaAvroSerDes<Order>();
                var keySerdes = new StringSerDes();

                builder.Table(destTopic, keySerdes, serdes, InMemory<string, Order>.As(config["table"]));

                var t = builder.Build();
                KafkaStream stream = new KafkaStream(t, sConfig, supplier);

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

                    producer.Produce(destTopic,
                        new Confluent.Kafka.Message<byte[], byte[]>
                        {
                            Key = keySerdes.Serialize("key1", new SerializationContext()),
                            Value = serdes.Serialize(new Order
                            {
                                order_id = 1,
                                price = 123.5F,
                                product_id = 1

                            }, new SerializationContext())
                        }, (d) =>
                        {
                            if (d.Status == PersistenceStatus.Persisted)
                            {
                                Console.WriteLine("Message sent !");
                            }
                        });

                    //create a well formatted Product in external topic
                    
                    producer.Produce("product-external",
                        new Confluent.Kafka.Message<byte[], byte[]>
                        {
                            Key = keySerdes.Serialize("key1", new SerializationContext()),
                            Value = new SchemaAvroSerDes<Product>().Serialize(new Product
                            {
                                product_id = 1,
                                price = 123.5F,
                                name = "NC Kafka Producer Software"

                            }, new SerializationContext())
                        }, (d) =>
                        {
                            if (d.Status == PersistenceStatus.Persisted)
                            {
                                Console.WriteLine("Message sent !");
                            }
                        });

                    Thread.Sleep(50);
                }

            }
        }
    }
}