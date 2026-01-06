using System.Text.Json;
using System.Threading.Channels;

namespace Orchestrator;

public class SagaWorker(
    ChannelReader<Guid> sagaChannelReader,
    ISagaRepository sagaRepository
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var sagaId in sagaChannelReader.ReadAllAsync(stoppingToken))
        {
            // TODO: read the saga payload
            // processing the payload
            // TODO: save & dispatch & update
            await sagaRepository.CreateSagaStepAndOutboxMessage(sagaId, "Order", JsonDocument.Parse("\":)\""));
        }
    }
}