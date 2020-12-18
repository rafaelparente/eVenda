using System;
using System.Threading.Tasks;

using Azure.Messaging.ServiceBus;

using Utils;

namespace InventoryService
{
    public static class ProductStringify
    {
        public static string Stringify(this ProductEvent productEvent)
        {
            var productString = "";
            switch (productEvent.EventType)
            {
                case ProductEventType.Sold:
                    productString += "[PRODUTO VENDIDO]";
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
            if (productEvent.EventType != ProductEventType.Sold || !ProductCrud.Update(productEvent.Product))
            {
                await args.AbandonMessageAsync(args.Message);
                return;
            }

            if (_isDisplayingList)
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
                Console.WriteLine("1: Incluir produto");
                Console.WriteLine("2: Editar produto");
                Console.WriteLine("3: Exibir produtos");
                Console.WriteLine("ou qualquer outra tecla para sair.");

                switch (Console.ReadKey().Key)
                {
                    case ConsoleKey.D1:
                        Console.WriteLine();
                        var createdEvent = MakeNewProductEvent();
                        if (createdEvent != null)
                        {
                            await ServiceBusUtils.SendObjectAsync(createdEvent);
                        }
                        break;
                    case ConsoleKey.D2:
                        Console.WriteLine();
                        Console.WriteLine("\nNome ou código do produto: ");
                        var codeOrName = Console.ReadLine();
                        var editProduct = ProductCrud.Get(p => p.Name == codeOrName || p.Code == codeOrName);
                        if (editProduct == null)
                        {
                            Console.WriteLine("Produto não foi encontrado.");
                        }
                        else
                        {
                            Console.WriteLine("\n1: Editar código");
                            Console.WriteLine("2: Editar nome");
                            Console.WriteLine("3: Editar preço");
                            Console.WriteLine("4: Editar quantidade");
                            switch (Console.ReadKey().Key)
                            {
                                case ConsoleKey.D1:
                                    Console.Write("\n\nNovo código: ");
                                    editProduct.Code = Console.ReadLine();
                                    break;
                                case ConsoleKey.D2:
                                    Console.Write("\n\nNovo nome: ");
                                    editProduct.Name = Console.ReadLine();
                                    break;
                                case ConsoleKey.D3:
                                    Console.Write("\n\nNovo preço: ");
                                    editProduct.Price = Convert.ToDouble(Console.ReadLine());
                                    break;
                                case ConsoleKey.D4:
                                    Console.Write("\n\nNova quantidade: ");
                                    editProduct.Quantity = Convert.ToInt32(Console.ReadLine());
                                    break;
                                default:
                                    break;
                            }
                            if (ProductCrud.Update(editProduct))
                            {
                                var editEvent = new ProductEvent(ProductEventType.Edited, editProduct);
                                await ServiceBusUtils.SendObjectAsync(editEvent);
                            }
                        }
                        break;
                    case ConsoleKey.D3:
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
