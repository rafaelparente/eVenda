using System;
using System.Threading.Tasks;

using Azure.Messaging.ServiceBus;

using Utils;

namespace SalesService
{
    class Program
    {
        private static readonly string ConnectionString = ConfigUtils.GetConnectionString();
        private static readonly string QueueName = ConfigUtils.GetQueueName();
        private static readonly ProductCrud ProductCrud = new();

        static async Task Main()
        {
            // receive message from the queue
            await ReceiveObjectsAsync();
        }
        
        static async Task ObjectHandler(ProcessMessageEventArgs args)
        {
            var createdProduct = args.Message.Body.ToArray().ParseJson<Product>();
            if (!ProductCrud.Create(createdProduct)) return;

            Console.WriteLine(createdProduct.ToString());

            // complete the message. messages is deleted from the queue. 
            await args.CompleteMessageAsync(args.Message);
        }

        // handle any errors when receiving messages
        static Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }

        static async Task ReceiveObjectsAsync()
        {
            await using ServiceBusClient client = new ServiceBusClient(ConnectionString);
            // create a processor that we can use to process the messages
            ServiceBusProcessor processor = client.CreateProcessor(QueueName, new ServiceBusProcessorOptions());
            
            // add handler to process messages
            processor.ProcessMessageAsync += ObjectHandler;

            // add handler to process any errors
            processor.ProcessErrorAsync += ErrorHandler;

            // start processing 
            await processor.StartProcessingAsync();

            Console.WriteLine("Wait for a minute and then press any key to end the processing");
            Console.ReadKey();

            // stop processing 
            Console.WriteLine("\nStopping the receiver...");
            await processor.StopProcessingAsync();
            Console.WriteLine("Stopped receiving messages");
        }
    }
}
