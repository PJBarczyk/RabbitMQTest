namespace RabbitMQTest
{
    public record class StockQuote(string Symbol, decimal Price, DateTime TimeStamp)
    {
        public const string RabbitMqType = "stockQuote";
    }

    public enum CoinFlipResult
    {
        Heads,
        Tails
    }

    public record class CoinFlip(CoinFlipResult Result)
    {
        public const string RabbitMqType = "coinFlip";
    }
}
