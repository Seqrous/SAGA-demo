using System.Text.Json;

namespace Orchestrator;

public interface ISagaRepository
{
    Task<Guid> Create(JsonDocument payloadJson);
}