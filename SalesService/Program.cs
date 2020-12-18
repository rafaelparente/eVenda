using System;
using System.Threading.Tasks;

using Azure.Messaging.ServiceBus;

using Utils;

namespace SalesService
{
    class Program
    {
        private static readonly ProductCrud ProductCrud = new();
        private static bool _isDisplayingList = false;

        static async Task Main()
        {
            await using var client = new ServiceBusClient(ConfigUtils.GetConnectionString());
            
            // create a processor that we can use to process the messages
            ServiceBusProcessor processor = client.CreateProcessor(ConfigUtils.GetQueueName(), new ServiceBusProcessorOptions());

            // add handler to process messages
            processor.ProcessMessageAsync += ObjectHandler;

            // add handler to process any errors
            processor.ProcessErrorAsync += ErrorHandler;

            // start processing 
            await processor.StartProcessingAsync();

            await Menu();
            
            Console.WriteLine();
            Console.WriteLine("Encerrando...");
            
            // stop processing 
            await processor.StopProcessingAsync();
        }
        
        private static async Task ObjectHandler(ProcessMessageEventArgs args)
        {
            var productEvent = args.Message.Body.ToArray().ParseJson<ProductEvent>();
            if (productEvent.EventType != ProductEventType.Created || !ProductCrud.Create(productEvent.Product))
            {
                await args.AbandonMessageAsync(args.Message);
                return;
            }

            if (_isDisplayingList)
            {
                Console.WriteLine();
                Console.WriteLine(productEvent.Product.ToString());
                Console.WriteLine();
                Console.WriteLine("Pressione qualquer tecla para voltar ao menu.");
            }

            // complete the message. messages is deleted from the queue.
            await args.CompleteMessageAsync(args.Message);
        }

        // handle any errors when receiving messages
        private static Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }
        
        private static async Task Menu()
        {
            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("1: Realizar venda de produto");
                Console.WriteLine("2: Exibir produtos à venda");
                Console.WriteLine("ou qualquer outra tecla para sair.");

                switch (Console.ReadKey().Key)
                {
                    case (ConsoleKey.D1):
                        break;
                    case (ConsoleKey.D2):
                        Console.WriteLine();
                        foreach (var product in ProductCrud.GetAll())
                        {
                            Console.WriteLine();
                            Console.WriteLine(product.ToString());
                        }
                        Console.WriteLine();
                        Console.WriteLine("Pressione qualquer tecla para voltar ao menu.");
                        _isDisplayingList = true;
                        Console.ReadKey();
                        _isDisplayingList = false;
                        break;
                    default:
                        return;
                }
            }
        }
    }
}
