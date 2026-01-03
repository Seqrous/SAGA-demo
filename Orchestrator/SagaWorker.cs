using System.Threading.Channels;

namespace Orchestrator;

public class SagaWorker(
    ChannelReader<Guid> sagaChannelReader
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var sagaId in sagaChannelReader.ReadAllAsync(stoppingToken))
        {
            Console.WriteLine($"Received {sagaId} :)");
        }
    }
}