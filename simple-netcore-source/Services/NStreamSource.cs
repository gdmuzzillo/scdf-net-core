using Microsoft.Extensions.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Streamiz.Kafka.Net;
using Streamiz.Kafka.Net.SerDes;
using Streamiz.Kafka.Net.Stream;


namespace simple_netcore_source.Services
{
    public class NStreamSource : BackgroundService, INStreamSource 
    {

        private readonly IConfiguration _config;
        private IServiceProvider _services;
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


            

            // Inyectamos los datos obtenidos al Stream
            
            var sConfig = new StreamConfig<StringSerDes, StringSerDes>();
            sConfig.ApplicationId = config["SPRING_CLOUD_APPLICATION_GUID"];
            sConfig.BootstrapServers = config["SPRING_CLOUD_STREAM_KAFKA_BINDER_BROKERS"];

            StreamBuilder builder = new StreamBuilder();


            builder.Stream<String, String, StringSerDes, StringSerDes>(config["spring.cloud.stream.bindings.output.destination"])
            .To(config["spring.cloud.stream.bindings.output.destination"]);

            Topology t = builder.Build();
            KafkaStream stream = new KafkaStream(t, sConfig);

            await stream.StartAsync();

        }
    }
}