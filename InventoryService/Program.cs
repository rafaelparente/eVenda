using System;
using System.Threading.Tasks;

using Azure.Messaging.ServiceBus;

using Utils;

namespace InventoryService
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
            if (productEvent.EventType != ProductEventType.Sold || !ProductCrud.Create(productEvent.Product))
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
                Console.WriteLine("1: Incluir produto");
                Console.WriteLine("2: Editar produto");
                Console.WriteLine("3: Exibir produtos");
                Console.WriteLine("ou qualquer outra tecla para sair.");

                switch (Console.ReadKey().Key)
                {
                    case (ConsoleKey.D1):
                        Console.WriteLine();
                        var productEvent = MakeNewProductEvent();
                        if (productEvent != null)
                        {
                            await ServiceBusUtils.SendObjectAsync(productEvent);
                        }
                        break;
                    case (ConsoleKey.D2):
                        break;
                    case (ConsoleKey.D3):
                        break;
                    default:
                        return;
                }
            }
        }

        private static ProductEvent MakeNewProductEvent()
        {
            Console.WriteLine();
            Console.Write("Código: ");
            var code = Console.ReadLine();
            Console.Write("Nome: ");
            var name = Console.ReadLine();
            Console.Write("Preço: ");
            var price = Convert.ToDouble(Console.ReadLine());
            Console.Write("Quantidade: ");
            var quantity = Convert.ToInt32(Console.ReadLine());
            
            var product = new Product(code, name, price, quantity);
            return !ProductCrud.Create(product) ? null : new ProductEvent(ProductEventType.Created, product);
        }
    }
}
