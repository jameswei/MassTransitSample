using MassTransit;
using Microsoft.Azure.EventGrid.Models;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace MassTransitSample
{
    // 实现 IConsumer 来消费 Azure Event 消息
    public class EventGridMessageConsumer : IConsumer<EventGridEvent>
    {
        public EventGridMessageConsumer() { }

        public Task Consume(ConsumeContext<EventGridEvent> context)
        {
            // 显式得使用 Newton.JsonConverter 将收到的 Azure Event 序列化成 JSON 字符串
            var jsonString = JsonConvert.SerializeObject(context.Message);
            Console.WriteLine($"Received {jsonString}");
            return Task.FromResult(true);
        }
    }
}
