using EasyNetQ.AutoSubscribe;
using Shared.Commands;

namespace OrderService;

public class MessageConsumer : IConsume<CreateOrderCommand>
{
    public void Consume(CreateOrderCommand message, CancellationToken cancellationToken = new())
    {
        Console.WriteLine($"Received {message}");
    }
}