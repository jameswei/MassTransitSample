using MassTransit;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace MassTransitSample
{
    // 实现 IConsumer 接口来消费自定义的 message type
    public class MessageConsumer : IConsumer<CustomMessage>
    {
        public MessageConsumer() { }

        public Task Consume(ConsumeContext<CustomMessage> context)
        {
            // 显式得使用 Newton.JsonConverter 将收到的 CustomMessage 消息序列化成 JSON 字符串
            var jsonString = JsonConvert.SerializeObject(context.Message);
            Console.WriteLine($"Received {jsonString}");
            return Task.FromResult(true);
        }
    }
}
