using System.Reflection;
using EasyNetQ;
using EasyNetQ.AutoSubscribe;

namespace OrderService;

public class RabbitMqAutoSubscriberHostedService(IBus bus, IServiceProvider serviceProvider) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        var subscriptionId = "hello-world";
        var autoSubscriber = new AutoSubscriber(bus, serviceProvider, subscriptionId);
        autoSubscriber.SubscribeAsync(Assembly.GetExecutingAssembly().GetTypes(), cancellationToken);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}