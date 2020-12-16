﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Azure.Messaging.ServiceBus;

using Utils;

namespace InventoryService
{
    class Program
    {
        private static readonly string ConnectionString = ConfigUtils.GetConnectionString();
        private static readonly string QueueName = ConfigUtils.GetQueueName();

        static async Task Main()
        {
            // send a message to the queue
            // await SendMessageAsync();

            // send a batch of messages to the queue
            // await SendMessageBatchAsync();

            await SendObjectAsync();
        }

        internal class PagamentoFeito
        {
            public string NumeroCartao { get; set; }
            public decimal Valor { get; set; }

            public override string ToString()
            {
                return $"Numero Cartao{NumeroCartao}, Valor{Valor}";

            }
        }

        static async Task SendObjectAsync()
        {
            var pagamentofeito = new PagamentoFeito { NumeroCartao = "123456789012", Valor = 1502.32M };

            await using var client = new ServiceBusClient(ConnectionString);
            // create a Service Bus client 
            ServiceBusSender sender = client.CreateSender(QueueName);

            var message = new ServiceBusMessage(pagamentofeito.ToJsonBytes())
            {
                ContentType = "application/json",
                CorrelationId = Guid.NewGuid().ToString()
            };

            // send the message
            await sender.SendMessageAsync(message);
        }
        
        static async Task SendMessageAsync()
        {
            await using ServiceBusClient client = new ServiceBusClient(ConnectionString);
            // create a Service Bus client 
            ServiceBusSender sender = client.CreateSender(QueueName);

            // create a message that we can send
            ServiceBusMessage message = new ServiceBusMessage("Hello world!");

            // send the message
            await sender.SendMessageAsync(message);
            Console.WriteLine($"Sent a single message to the queue: {QueueName}");
        }

        static Queue<ServiceBusMessage> CreateMessages()
        {
            // create a queue containing the messages and return it to the caller
            Queue<ServiceBusMessage> messages = new Queue<ServiceBusMessage>();
            messages.Enqueue(new ServiceBusMessage("First message in the batch"));
            messages.Enqueue(new ServiceBusMessage("Second message in the batch"));
            messages.Enqueue(new ServiceBusMessage("Third message in the batch"));
            return messages;
        }

        static async Task SendMessageBatchAsync()
        {
            // create a Service Bus client 
            await using ServiceBusClient client = new ServiceBusClient(ConnectionString);
            // create a sender for the queue 
            ServiceBusSender sender = client.CreateSender(QueueName);

            // get the messages to be sent to the Service Bus queue
            Queue<ServiceBusMessage> messages = CreateMessages();

            // total number of messages to be sent to the Service Bus queue
            int messageCount = messages.Count;

            // while all messages are not sent to the Service Bus queue
            while (messages.Count > 0)
            {
                // start a new batch 
                using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();

                // add the first message to the batch
                if (messageBatch.TryAddMessage(messages.Peek()))
                {
                    // dequeue the message from the .NET queue once the message is added to the batch
                    messages.Dequeue();
                }
                else
                {
                    // if the first message can't fit, then it is too large for the batch
                    throw new Exception($"Message {messageCount - messages.Count} is too large and cannot be sent.");
                }

                // add as many messages as possible to the current batch
                while (messages.Count > 0 && messageBatch.TryAddMessage(messages.Peek()))
                {
                    // dequeue the message from the .NET queue as it has been added to the batch
                    messages.Dequeue();
                }

                // now, send the batch
                await sender.SendMessagesAsync(messageBatch);

                // if there are any remaining messages in the .NET queue, the while loop repeats 
            }

            Console.WriteLine($"Sent a batch of {messageCount} messages to the topic: {QueueName}");
        }
    }
}
