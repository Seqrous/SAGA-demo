using System.Text.Json;
using System.Threading.Channels;
using EasyNetQ;

namespace Orchestrator;

public class SagaWorker(
    ChannelReader<Guid> sagaChannelReader,
    ISagaRepository sagaRepository,
    IBus bus
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var sagaId in sagaChannelReader.ReadAllAsync(stoppingToken))
        {
            // TODO: read the saga payload from the DB
            var payload = JsonDocument.Parse("\":)\"");
            var stepName = "Order";
            
            await sagaRepository.CreateSagaStepAndOutboxMessage(sagaId, stepName, payload);
            var command = new CreateOrderCommand(sagaId, payload);
            await bus.PubSub.PublishAsync(command, stoppingToken);
            await sagaRepository.SetOutboxMessageSent(sagaId, stepName);
        }
    }
}