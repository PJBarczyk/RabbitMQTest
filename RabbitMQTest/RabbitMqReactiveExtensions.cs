using R3;
using RabbitMQ.Client.Events;

namespace RabbitMQTest
{
    public static class RabbitMqReactiveExtensions
    {
        public static Observable<BasicDeliverEventArgs> RecievedAsObservable(this EventingBasicConsumer consumer) => Observable.FromEventHandler<BasicDeliverEventArgs>(h => consumer.Received += h, h => consumer.Received -= h).Select(t => t.e);
    }
}
