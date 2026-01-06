using System.Text.Json;
using Npgsql;
using NpgsqlTypes;

namespace Orchestrator;

public class PgSagaRepository(NpgsqlDataSource pgDataSource, TimeProvider timeProvider) : ISagaRepository
{
    public async Task<Guid> Create(JsonDocument payloadJson)
    {
        const string sql = """
            INSERT INTO orchestrator.SagaLog (saga_id, created_at, updated_at, status, current_step, payload)
            VALUES ($1, $2, $3, 'CREATED', NULL, $4);
        """;

        var sagaId = Guid.NewGuid();
        await using var connection = await pgDataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddRange(new NpgsqlParameter[]
        {
            new() { Value = sagaId, NpgsqlDbType = NpgsqlDbType.Uuid },
            new() { Value = timeProvider.GetUtcNow().DateTime, NpgsqlDbType = NpgsqlDbType.Timestamp },
            new() { Value = timeProvider.GetUtcNow().DateTime, NpgsqlDbType = NpgsqlDbType.Timestamp },
            new() { Value = payloadJson, NpgsqlDbType = NpgsqlDbType.Json }
        });
        _ = await command.ExecuteNonQueryAsync();
        return sagaId;
    }
}