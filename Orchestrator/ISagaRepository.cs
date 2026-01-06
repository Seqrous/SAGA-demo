using System.Text.Json;

namespace Orchestrator;

public interface ISagaRepository
{
    Task<Guid> StartSaga(JsonDocument payloadJson);
    Task CreateSagaStepAndOutboxMessage(Guid sagaId, string stepName, JsonDocument payloadJson);
}