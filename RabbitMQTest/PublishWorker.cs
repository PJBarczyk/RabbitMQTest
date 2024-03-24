using R3;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;

namespace RabbitMQTest
{
    public class PublishWorker(ILogger<PublishWorker> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory { HostName = "localhost" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: "hello",
                     durable: false,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);

            while (!stoppingToken.IsCancellationRequested)
            {
                var (data, type) = GetMessage();
                
                var basicProperties = channel.CreateBasicProperties();
                basicProperties.Type = type;

                channel.BasicPublish(exchange: string.Empty,
                    routingKey: "hello",
                    basicProperties: basicProperties,
                    body: data);

                await Task.Delay(1000, stoppingToken);
            }
        }

        private static (byte[] Data, string Type) GetMessage()
        {
            if (Random.Shared.NextDouble() > 0.5)
            {
                var code = string.Concat(Random.Shared.GetItems("ABCDEFGHIJKLMNOPQRSTUVWXYZ".AsSpan(), 3));
                var price = (decimal)Math.Round(Random.Shared.NextDouble() * 100, 2);
                var quote = new StockQuote(code, price, DateTime.UtcNow);
                var data = JsonSerializer.SerializeToUtf8Bytes(quote);
                return (data, StockQuote.RabbitMqType);
            }
            else
            {
                var flip = new CoinFlip(Random.Shared.NextDouble() > 0.5 ? CoinFlipResult.Heads : CoinFlipResult.Tails);
                var data = JsonSerializer.SerializeToUtf8Bytes(flip);
                return (data, CoinFlip.RabbitMqType);
            }
        }
    }
}
