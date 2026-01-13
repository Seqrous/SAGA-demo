using System.Text.Json;

namespace Shared.Commands;

public record CreateOrderCommand(Guid SagaId, JsonDocument JsonDocument);