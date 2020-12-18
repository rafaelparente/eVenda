using System;
using System.Threading.Tasks;

using Azure.Messaging.ServiceBus;

using Utils;

namespace SalesService
{
    public static class ProductStringify
    {
        public static string Stringify(this ProductEvent productEvent)
        {
            var productString = "";
            switch (productEvent.EventType)
            {
                case ProductEventType.Created:
                    productString += "[PRODUTO CRIADO]";
                    break;
                case ProductEventType.Edited:
                    productString += "[PRODUTO EDITADO]";
                    break;
                default:
                    productString += "[?]";
                    break;
            }
            productString += "\n" + productEvent.Product.ToString();

            return productString;
        }
    }

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

            if (_isDisplayingList && productEvent.Product.Quantity > 0)
            {
                Console.WriteLine();
                Console.WriteLine(productEvent.Stringify());
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
                        Console.WriteLine();
                        Console.WriteLine("\nNome ou código do produto: ");
                        var codeOrName = Console.ReadLine();
                        var sellingProduct = ProductCrud.Get(p => p.Name == codeOrName || p.Code == codeOrName);
                        if (sellingProduct == null)
                        {
                            Console.WriteLine("Produto não foi encontrado.");
                        }
                        else
                        {
                            Console.WriteLine($"Quantidade a ser vendida (máx.: {sellingProduct.Quantity}): ");
                            var sellingQuantity = Convert.ToInt32(Console.ReadLine());
                            if (sellingQuantity > sellingProduct.Quantity)
                            {
                                Console.WriteLine("Erro: quantidade acima do disponível.");
                            }
                            else
                            {
                                sellingProduct.Quantity -= sellingQuantity;
                                ProductCrud.Update(sellingProduct);
                                var soldEvent = new ProductEvent(ProductEventType.Sold, sellingProduct);
                                await ServiceBusUtils.SendObjectAsync(soldEvent);
                            }
                        }
                        break;
                    case (ConsoleKey.D2):
                        Console.WriteLine();
                        foreach (var product in ProductCrud.GetAll(p => p.Quantity > 0))
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
