using RabbitMQ.Client.Events;
using System.Text.Json;

namespace RabbitMQTest
{
    public interface IMessageHandler
    {
        Task HandleAsync(BasicDeliverEventArgs e, CancellationToken stoppingToken);
    }

    public abstract class MessageHandler<T> : IMessageHandler
    {
        public virtual JsonSerializerOptions Options { get; } = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        public Task HandleAsync(BasicDeliverEventArgs e, CancellationToken stoppingToken)
        {
            var body = JsonSerializer.Deserialize<T>(e.Body.Span, Options) ?? throw new Exception($"Failed to deserialize message of type {typeof(T).Name}");

            return HandleAsync(body, stoppingToken);
        }

        public abstract Task HandleAsync(T message, CancellationToken stoppingToken);
    }

    [RouteType(StockQuote.RabbitMqType)]
    public class StockQuoteHandler(ILogger<StockQuoteHandler> logger) : MessageHandler<StockQuote>
    {
        public override Task HandleAsync(StockQuote message, CancellationToken stoppingToken)
        {
            logger.LogInformation("Received stock quote {Symbol} at {Price} on {TimeStamp}", message.Symbol, message.Price, message.TimeStamp);

            return Task.CompletedTask;
        }
    }

    [RouteType(CoinFlip.RabbitMqType)]
    public class CoinFlipHandler(ILogger<CoinFlipHandler> logger) : MessageHandler<CoinFlip>
    {
        public override Task HandleAsync(CoinFlip message, CancellationToken stoppingToken)
        {
            logger.LogInformation("Received coin flip result {Result}", message.Result);

            return Task.CompletedTask;
        }
    }
}
