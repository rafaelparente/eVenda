using System;
using System.Threading.Tasks;

using Azure.Messaging.ServiceBus;

namespace Utils
{
    public class ServiceBusUtils
    {
        public static async Task SendObjectAsync(object obj)
        {
            await using var client = new ServiceBusClient(ConfigUtils.GetConnectionString());
            // create a Service Bus client 
            ServiceBusSender sender = client.CreateSender(ConfigUtils.GetQueueName());

            var message = new ServiceBusMessage(obj.ToJsonBytes())
            {
                ContentType = "application/json",
                CorrelationId = Guid.NewGuid().ToString()
            };

            // send the message
            await sender.SendMessageAsync(message);
        }
    }
}
