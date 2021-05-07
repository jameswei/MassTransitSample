using GreenPipes;
using MassTransit;
using MassTransit.Azure.ServiceBus.Core.Contexts;
using MassTransit.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading;

namespace MassTransitSample
{
    // 扩展 MassTransit 的 IMessageDeserializer 来实现自定义消息的反序列化器
    public class CustomMessageDeserializer : IMessageDeserializer
    {
        private string _contentType;

        public CustomMessageDeserializer(string contentType)
        {
            _contentType = contentType;
        }

        // lambda 写法的属性声明，等同于下面的单独 getter 写法
        // public ContentType ContentType => new ContentType(_contentType);

        public ContentType ContentType
        {
            get
            {
                return new ContentType(_contentType);
            }
        }

        // 实现 IMessageDeserializer 接口中的 Deserialize() 方法，用来反序列化 dispatch 来的 ReceiveContext 
        public ConsumeContext Deserialize(ReceiveContext receiveContext)
        {
            // 从 byte[] 中以 utf8 编码读 message body
            var body = Encoding.UTF8.GetString(receiveContext.GetBody());
            // json 反序列化成指定的 message type
            var customMessage = JsonConvert.DeserializeObject<CustomMessage>(body);
            var serviceBusSendContext = new AzureServiceBusSendContext<CustomMessage>(customMessage, CancellationToken.None);

            // 通过 urn 指定 message type 的 full-qualified name, urn:message:{namespace}:{message_type}
            string[] messageTypes = { "urn:message:MassTransitSample:CustomMessage" };
            var serviceBusContext = receiveContext as ServiceBusReceiveContext;
            serviceBusSendContext.ContentType = new ContentType(JsonMessageSerializer.JsonContentType.ToString());
            serviceBusSendContext.SourceAddress = serviceBusContext.InputAddress;
            serviceBusSendContext.SessionId = serviceBusContext.SessionId;

            // sending JToken because we are using default Newtonsoft deserializer/serializer
            var messageEnv = new JsonMessageEnvelope(serviceBusSendContext, JObject.Parse(body), messageTypes);
            return new JsonConsumeContext(JsonSerializer.CreateDefault(), receiveContext, messageEnv);
        }

        public void Probe(ProbeContext context) { }
    }
}

