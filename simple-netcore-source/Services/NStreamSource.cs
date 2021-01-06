using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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


                String[] capture = this._dataService.readData();

                // Inyectamos los datos obtenidos al Stream

                var sConfig = new StreamConfig<StringSerDes, StringSerDes>();
                sConfig.ApplicationId = config["SPRING_CLOUD_APPLICATION_GUID"];
                sConfig.BootstrapServers = config["SPRING_CLOUD_STREAM_KAFKA_BINDER_BROKERS"];

                StreamBuilder builder = new StreamBuilder();

                

                builder.Table<String, String, StringSerDes, StringSerDes>(config["spring.cloud.stream.bindings.output.destination"])
                .ToStream()
                .To(config["spring.cloud.stream.bindings.output.destination"]);

                Topology t = builder.Build();
                KafkaStream stream = new KafkaStream(t, sConfig);
                

                await stream.StartAsync();
            }
        }
    }
}