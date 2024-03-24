using Microsoft.Extensions.DependencyInjection.Extensions;
using RabbitMQ.Client.Events;
using System.Collections.Frozen;
using System.Reflection;

namespace RabbitMQTest
{
    public interface IMessageRouter
    {
        Task RouteAsync(BasicDeliverEventArgs e, CancellationToken stoppingToken);
        IReadOnlySet<string> RoutedTypes { get; }
    }
    
    public class MessageRouter : IMessageRouter
    {
        public MessageRouter(IEnumerable<IMessageHandler> handlers)
        {
            routeMap = handlers.ToFrozenDictionary(h =>
            {
                var routeType = h.GetType().GetCustomAttribute<RouteTypeAttribute>() ?? throw new Exception($"MessageHandler {h.GetType().Name} is missing {nameof(RouteTypeAttribute)}");

                return routeType.RouteType;
            }, h => h);

            routedTypes = routeMap.Keys.ToFrozenSet();
        }

        private readonly FrozenDictionary<string, IMessageHandler> routeMap;
        private readonly FrozenSet<string> routedTypes;

        public IReadOnlySet<string> RoutedTypes => routedTypes;

        public Task RouteAsync(BasicDeliverEventArgs e, CancellationToken stoppingToken)
        {
            return routeMap[e.BasicProperties.Type].HandleAsync(e, stoppingToken);
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class RouteTypeAttribute(string routeType) : Attribute
    {
        public string RouteType { get; } = routeType;
    }

    public static class MessageRouterExtensions
    {
        public static IServiceCollection AddMessageRouting(this IServiceCollection services, Assembly assembly)
        {
            services.Scan(scan => scan.FromAssemblies(assembly)
                .AddClasses(classes => classes.AssignableTo<IMessageHandler>())
                .AsImplementedInterfaces());

            services.TryAddSingleton<IMessageRouter, MessageRouter>();

            return services;
        }

        public static IServiceCollection AddMessageRouting(this IServiceCollection services)
        {
            return services.AddMessageRouting(Assembly.GetCallingAssembly());
        }
    }
}
