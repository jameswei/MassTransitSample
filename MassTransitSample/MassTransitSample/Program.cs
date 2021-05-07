
using System;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Serialization;
using MassTransit.Azure.ServiceBus.Core;
using System.Net.Mime;

namespace MassTransitSample
{
    class Program
    {
        private static string ContentTypeJson = "application/json";
        public static async Task Main(string[] args)
        {
            // 使用 ASB 创建 MassTransit service bus
            var bus = Bus.Factory.CreateUsingAzureServiceBus(cfg =>
            {
                // 配置 ASB
                var queueName = "Your ASB Queue Name";
                var connectionString = "Connection String with RooTManage policy";
                var host = cfg.Host(connectionString, h =>
                {
                    h.OperationTimeout = TimeSpan.FromSeconds(60);
                });

                // 配置 ASB queue 的 receive endpoint
                cfg.ReceiveEndpoint(queueName, e =>
                    {
                        // 配置自定义的反序列化器，指定要用于的消息类型，这里是 Azure Event
                        e.AddMessageDeserializer(contentType: new ContentType(ContentTypeJson), () =>
                        {
                            return new EventGridMessgeDeserializer(ContentTypeJson);
                        });
                        e.Consumer(() => new EventGridMessageConsumer());
                        // 配置用于消费自定义消息的反序列化器
                        e.AddMessageDeserializer(contentType: JsonMessageSerializer.JsonContentType, () =>
                        {
                            return new CustomMessageDeserializer(JsonMessageSerializer.JsonContentType.ToString());
                        });
                        e.Consumer(() => new MessageConsumer());
                    });
            });
            // bus.Start();
            await bus.StartAsync();

            await bus.Publish<CustomMessage>(new { Hello = "Hello, World." });

            Console.WriteLine("Press any key to exit");
            await Task.Run(() => Console.ReadKey());

            await bus.StopAsync();
        }
    }
}
