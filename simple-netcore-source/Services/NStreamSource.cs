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
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
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

            Console.WriteLine(destTopic);




            using (var scope = services.CreateScope())
            {
                this._dataService = scope.ServiceProvider
                    .GetRequiredService<IDataService>();

                bool isRunningState = false;

                var timeout = TimeSpan.FromSeconds(10);
                DateTime dt = DateTime.Now;

                Order[] capture = this._dataService.readData();

                // Inyectamos los datos obtenidos al Stream


                var sConfig = new StreamConfig<StringSerDes, StringSerDes>();
                sConfig.ApplicationId = config["SPRING_CLOUD_APPLICATION_GUID"];
                sConfig.BootstrapServers = config["SPRING_CLOUD_STREAM_KAFKA_BINDER_BROKERS"];
                sConfig.SchemaRegistryUrl = config["SchemaRegistryUrl"];
                sConfig.AutoRegisterSchemas = true;
                sConfig.NumStreamThreads = 10;
                sConfig.Acks = Acks.All;
                sConfig.AddConsumerConfig("allow.auto.create.topics", "true");
                sConfig.InnerExceptionHandler = (e) => ExceptionHandlerResponse.CONTINUE;

                var schemaRegistryClient = new CachedSchemaRegistryClient
                (new SchemaRegistryConfig
                {
                    Url = sConfig.SchemaRegistryUrl
                });

                var supplier = new SyncKafkaSupplier(new KafkaLoggerAdapter(sConfig));

                var producerConfig = sConfig.ToProducerConfig();
                var adminConfig = sConfig.ToAdminConfig(sConfig.ApplicationId);

                var admin = supplier.GetAdmin(adminConfig);

                // try
                // {

                //     var topic = new TopicSpecification
                //     {
                //         Name = destTopic,
                //         NumPartitions = 1,
                //         ReplicationFactor = 3
                //     };
                //     var topicProduct = new TopicSpecification
                //     {
                //         Name = "product-external",
                //         NumPartitions = 1,
                //         ReplicationFactor = 3
                //     };


                //     IList<TopicSpecification> topics = new List<TopicSpecification>();

                //     topics.Add(topic);
                //     topics.Add(topicProduct);

                //     await admin.CreateTopicsAsync(topics);
                // }
                // catch (Exception topicExists)
                // {
                //     Console.WriteLine("Topic alreade exists");
                //     Console.Write(topicExists);
                // }

                var producer = supplier.GetProducer(producerConfig);

                StreamBuilder builder = new StreamBuilder();

                var serdes = new SchemaAvroSerDes<Order>();
                var keySerdes = new Int32SerDes();

                builder.Table(destTopic, keySerdes, serdes, InMemory<int, Order>.As(config["table"]));

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


                    //   //create a well formatted Endpoint in external topic
                    var endpProducer = new ProducerBuilder<byte[], Endpoint>(producerConfig)
                    .SetValueSerializer(new AvroSerializer<Endpoint>(schemaRegistryClient, new AvroSerializerConfig { AutoRegisterSchemas = true }).AsSyncOverAsync()).Build();
                        endpProducer.Produce("api-endpoints",
                            new Message<byte[], Endpoint>
                            {
                                Key = new Int32SerDes().Serialize(1, new SerializationContext()),
                                Value = new Endpoint
                                {
                                    endpoint_id = ("endpoint1"),
                                    endpoint_url = ("http://endpoint" + keySerdes + "/"),
                                    http_method = "POST"
                                }
                            }, (d) =>
                            {
                                if (d.Status == PersistenceStatus.Persisted)
                                {
                                    Console.WriteLine("Endpoint Message sent !");
                                }
                            });

                    


                    //create a well formatted Product in external topic
                    var productProducer = new ProducerBuilder<byte[], Product>(producerConfig)
                    .SetValueSerializer(new AvroSerializer<Product>(schemaRegistryClient, new AvroSerializerConfig { AutoRegisterSchemas = true }).AsSyncOverAsync()).Build();

                    productProducer.Produce("product-external",
                        new Message<byte[], Product>
                        {
                            Key = new Int32SerDes().Serialize(1, new SerializationContext()),
                            Value = new Product
                            {
                                name = "Producto de Software",
                                price = 1234.5F,
                                product_id = 3
                            }
                        }, (d) =>
                        {
                            if (d.Status == PersistenceStatus.Persisted)
                            {
                                Console.WriteLine("Product Message sent !");
                            }
                        });


                    Thread.Sleep(10);

                    producer.Produce(destTopic,
                        new Confluent.Kafka.Message<byte[], byte[]>
                        {
                            Key = keySerdes.Serialize(1, new SerializationContext()),
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
                                Console.WriteLine("Order Message sent !");
                            }
                        });

         

                    Thread.Sleep(50);
                }

            }
        }
    }
}