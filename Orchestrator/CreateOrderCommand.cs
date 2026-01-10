using System.Text.Json;

namespace Orchestrator;

public record CreateOrderCommand(Guid SagaId, JsonDocument JsonDocument);