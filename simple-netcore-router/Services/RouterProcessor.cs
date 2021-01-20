using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using com.avro.bean;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Hadoop.Avro;
using Streamiz.Kafka.Net;
using Streamiz.Kafka.Net.SchemaRegistry.SerDes.Avro;
using Streamiz.Kafka.Net.SerDes;
using Streamiz.Kafka.Net.Stream;
using Streamiz.Kafka.Net.Table;
namespace simple_netcore_router.Services {
    public class RouterProcessor : BackgroundService, IRouterProcessor {

        private readonly IConfiguration _config;
        public RouterProcessor (IConfiguration config) {
            _config = config;
        }
        protected override async Task ExecuteAsync (CancellationToken stoppingToken) {
            await process (_config);
        }
        public async Task process (IConfiguration config) {

            // creando AvroSchemas from clases
            AvroSerializerSettings settings = new AvroSerializerSettings ();
            settings.Resolver = new AvroPublicMemberContractResolver ();
            var endpontSchema = AvroSerializer.Create<Endpoint> (settings).WriterSchema.ToString ();

            var messageDSchema = AvroSerializer.Create<MessageDestination> (settings).WriterSchema.ToString ();

            Console.WriteLine ("Endpoint Schema: " + endpontSchema);
            Console.WriteLine ("Message Destination Schema: " + messageDSchema);

            Console.WriteLine ("RouterProcess");
            var sConfig = new StreamConfig<StringSerDes, StringSerDes> ();

            sConfig.ApplicationId = config["SPRING_CLOUD_APPLICATION_GROUP"];
            sConfig.BootstrapServers = config["SPRING_CLOUD_STREAM_KAFKA_BINDER_BROKERS"];
            sConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
            sConfig.SchemaRegistryUrl = config["SchemaRegistryUrl"];
            sConfig.AutoRegisterSchemas = true;
            sConfig.NumStreamThreads = 1;
            sConfig.Acks = Acks.All;
            //sConfig.Debug = "consumer,cgrp,topic,fetch";
            sConfig.AddConsumerConfig ("allow.auto.create.topics", "true");
            sConfig.MaxTaskIdleMs = 50;
            sConfig.InnerExceptionHandler = (e) => ExceptionHandlerResponse.CONTINUE;

            var timeout = TimeSpan.FromSeconds (10);
            DateTime dt = DateTime.Now;

            MessageDestination op = new MessageDestination ();

            var serializer = new SchemaAvroSerDes<OrderProduct> ();

            StreamBuilder builder = new StreamBuilder ();

            var table = builder.Table (config["endpoints"],
                new Int32SerDes (),
                new SchemaAvroSerDes<Endpoint> (),
                InMemory<int, Endpoint>.As (config["endpoints-table"]));

            builder.Stream<int, OrderProduct, Int32SerDes, SchemaAvroSerDes<OrderProduct>> (config["spring.cloud.stream.bindings.input.destination"])
                .Map<int, OrderProduct> ((k, v) => {
                    return (KeyValuePair.Create ( v.product_id,v));
                })
                .Join (table, (orderProduct, endpoint) => {
                    Console.WriteLine ("OrderProduct: " + orderProduct?.order_id);
                    Console.WriteLine ("Endpoint: " + endpoint?.endpoint_id);

                    op = new MessageDestination {
                        messageId = orderProduct.order_id,
                        endpoint = endpoint,
                        payload = orderProduct
                    };
                    return op;
                })
                .Peek ((k, v) => Console.WriteLine ($"Sending message {k}  to endpoint {v.endpoint.endpoint_url}"))
                .Print (Printed<int, MessageDestination>.ToOut ());

            Topology t = builder.Build ();

            Console.WriteLine (t.Describe ());

            KafkaStream stream = new KafkaStream (t, sConfig);

            bool isRunningState = false;

            stream.StateChanged += (old, @new) => {
                if (@new.Equals (KafkaStream.State.RUNNING)) {
                    isRunningState = true;
                }
            };

            await stream.StartAsync ();

            while (!isRunningState) {
                Thread.Sleep (250);
                if (DateTime.Now > dt + timeout) {
                    break;
                }
            }

            if (isRunningState) {
                Console.WriteLine ("Stream running state is " + isRunningState.ToString ());
            }
        }
    }
}