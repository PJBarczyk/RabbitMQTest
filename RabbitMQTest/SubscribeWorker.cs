using R3;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMQTest
{
    public class SubscribeWorker(ILogger<SubscribeWorker> logger, IMessageRouter router) : BackgroundService
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

            var consumer = new EventingBasicConsumer(channel);

            channel.BasicConsume(queue: "hello",
                autoAck: true,
                consumer: consumer);

            using var subscription = consumer.RecievedAsObservable()
                .SubscribeAwait(async (e, c) =>
                {
                    try
                    {
                        await router.RouteAsync(e, c);
                    }
                    catch (KeyNotFoundException)
                    {
                        logger.LogWarning("No handler found for message type {Type}", e.BasicProperties.Type);
                    }
                }, AwaitOperation.Parallel);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
