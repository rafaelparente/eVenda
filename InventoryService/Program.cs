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
                        var product = MakeNewProduct();
                        if (product != null)
                        {
                            await SendObjectAsync(product);
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

        private static Product MakeNewProduct()
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
            return !ProductCrud.Create(product) ? null : product;
        }

        static async Task SendObjectAsync(object obj)
        {
            await using var client = new ServiceBusClient(ConnectionString);
            // create a Service Bus client 
            ServiceBusSender sender = client.CreateSender(QueueName);

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
