﻿using GreenPipes;
using MassTransit;
using MassTransit.Azure.ServiceBus.Core.Contexts;
using MassTransit.Serialization;
using Microsoft.Azure.EventGrid.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading;

namespace MassTransitSample
{
    // 扩展 IMessageDeserializer 实现 Azure Event 的反序列化器
    public class EventGridMessgeDeserializer : IMessageDeserializer
    {
        private string _contentType;

        public EventGridMessgeDeserializer(string contentType)
        {
            _contentType = contentType;
        }

        public ContentType ContentType
        {
            get
            {
                return new ContentType(_contentType);
            }
        }

        public ConsumeContext Deserialize(ReceiveContext receiveContext)
        {
            var body = Encoding.UTF8.GetString(receiveContext.GetBody());
            var customMessage = JsonConvert.DeserializeObject<EventGridEvent>(body);
            var serviceBusSendContext = new AzureServiceBusSendContext<EventGridEvent>(customMessage, CancellationToken.None);

            // this is the default scheme, that has to match in order messages to be processed
            // EventGrid messages type of EventGridEvent within namespace Microsoft.Azure.EventGrid.Models
            string[] messageTypes = { "urn:message:Microsoft.Azure.EventGrid.Models:EventGridEvent" };
            var serviceBusContext = receiveContext as ServiceBusReceiveContext;
            serviceBusSendContext.ContentType = new ContentType(JsonMessageSerializer.JsonContentType.ToString());
            serviceBusSendContext.SourceAddress = serviceBusContext.InputAddress;
            serviceBusSendContext.SessionId = serviceBusContext.SessionId;

            // sending JToken because we are using default Newtonsoft deserializer/serializer
            var messageEnv = new JsonMessageEnvelope(serviceBusSendContext, JObject.Parse(body), messageTypes);
            return new JsonConsumeContext(JsonSerializer.CreateDefault(), receiveContext, messageEnv);
        }

        public void Probe(ProbeContext context)
        {
        }
    }
}
