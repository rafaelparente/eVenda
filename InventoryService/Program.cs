using System;
using System.Threading.Tasks;

using Azure.Messaging.ServiceBus;

using Utils;

namespace InventoryService
{
    class Program
    {
        private static readonly string ConnectionString = ConfigUtils.GetConnectionString();
        private static readonly string QueueName = ConfigUtils.GetQueueName();
        private static readonly ProductCrud ProductCrud = new ();

        static async Task Main()
        {
            // send a message to the queue
            await SendObjectAsync();
        }

        static async Task SendObjectAsync()
        {
            var createdProduct = new Product("P01", "Produto 1", 5.89, 3);
            if (!ProductCrud.Create(createdProduct)) return;

            await using var client = new ServiceBusClient(ConnectionString);
            // create a Service Bus client 
            ServiceBusSender sender = client.CreateSender(QueueName);

            var message = new ServiceBusMessage(createdProduct.ToJsonBytes())
            {
                ContentType = "application/json",
                CorrelationId = Guid.NewGuid().ToString()
            };

            // send the message
            await sender.SendMessageAsync(message);
        }
    }
}
