using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Streamiz.Kafka.Net;
using Streamiz.Kafka.Net.SerDes;
using simple_netcore_source.Helpers;
using Confluent.Kafka;
using com.avro.bean;
using Streamiz.Kafka.Net.SchemaRegistry.SerDes.Avro;
namespace simple_netcore_source.Services
{
    public class NStreamSource : BackgroundService, INStreamSource 
    {

        private readonly IConfiguration _config;
        private IServiceProvider _services;

        private IDataService _dataService;
        public NStreamSource(IConfiguration config, IServiceProvider services)
        {
            _config = config;
            _services = services;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await exec(_config, _services);
        }

        public async Task exec(IConfiguration config, IServiceProvider services){

            Console.WriteLine("Process");

            using (var scope = services.CreateScope())
            {
                this._dataService = scope.ServiceProvider
                        .GetRequiredService<IDataService>();

                bool isRunningState = false;
                 
                var timeout = TimeSpan.FromSeconds(10);
                DateTime dt = DateTime.Now;


                String[] capture = this._dataService.readData();

                // Inyectamos los datos obtenidos al Stream

                var sConfig = new StreamConfig<StringSerDes, StringSerDes>();
                sConfig.ApplicationId = config["SPRING_CLOUD_APPLICATION_GUID"];
                sConfig.BootstrapServers = config["SPRING_CLOUD_STREAM_KAFKA_BINDER_BROKERS"];
                sConfig.SchemaRegistryUrl = config["SchemaRegistryUrl"];
                sConfig.AutoRegisterSchemas = true;


                var supplier = new SyncKafkaSupplier();
                var producer = supplier.GetProducer(sConfig.ToProducerConfig());
                StreamBuilder builder = new StreamBuilder();



                builder.Table<String, Order, StringSerDes,  SchemaAvroSerDes<Order>>(config["spring.cloud.stream.bindings.output.destination"],null, config["table"] );

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
                    var serdes = new SchemaAvroSerDes<Order>();
                    var keySerdes = new StringSerDes();
                    producer.Produce(config["spring.cloud.stream.bindings.output.destination"],
                      new Confluent.Kafka.Message<byte[], byte[]>
                    {
                        Key = keySerdes.Serialize("key1", new SerializationContext()),
                        Value = serdes.Serialize(new Order
                    {
                        order_id = 1,
                        price = 123.5F,
                        product_id = 1
                    
                    }, new SerializationContext())
                    });
                    
                Thread.Sleep(50);
            }


            }
        }
    }
}