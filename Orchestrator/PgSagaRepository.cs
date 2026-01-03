using System.Text.Json;
using Npgsql;
using NpgsqlTypes;

namespace Orchestrator;

public class PgSagaRepository(NpgsqlDataSource pgDataSource) : ISagaRepository
{
    public async Task<Guid> Create(JsonDocument payloadJson)
    {
        const string sql = """
            INSERT INTO orchestrator.SagaLog (saga_id, status, current_step, payload)
            VALUES ($1, 'CREATED', NULL, $2);
        """;

        var sagaId = Guid.NewGuid();
        await using var connection = await pgDataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.Add(new NpgsqlParameter { Value = sagaId, NpgsqlDbType = NpgsqlDbType.Uuid });
        command.Parameters.Add(new NpgsqlParameter { Value = payloadJson, NpgsqlDbType = NpgsqlDbType.Json });
        
        _ = await command.ExecuteNonQueryAsync();
        return sagaId;
    }
}