using System.Data;
using System.Text.Json;
using Npgsql;
using NpgsqlTypes;

namespace Orchestrator;

public class PgSagaRepository(NpgsqlDataSource pgDataSource, TimeProvider timeProvider) : ISagaRepository
{
    public async Task<Guid> StartSaga(JsonDocument payloadJson)
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

    public async Task<JsonDocument> GetSagaInitPayload(Guid sagaId)
    {
        const string sql = "SELECT payload FROM orchestrator.SagaLog WHERE saga_id = $1";

        await using var connection = await pgDataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddRange(new NpgsqlParameter[]
        {
            new() { Value = sagaId, NpgsqlDbType = NpgsqlDbType.Uuid },
        });
        
        var dbReader = await command.ExecuteReaderAsync();
        if (!await dbReader.ReadAsync())
            throw new InvalidOperationException("No SAGA found for ID {sagaId}");

        return JsonDocument.Parse(dbReader.GetString(0));
    }

    public async Task CreateSagaStepAndOutboxMessage(Guid sagaId, string stepName, JsonDocument payloadJson)
    {
        const string createStepSql = """
            INSERT INTO orchestrator.SagaStep (saga_id, step_name, status, idempotency_key, created_at, updated_at)
            VALUES ($1, $2, 'PENDING', $3, $4, $5);
        """;
        
        const string createOutboxEntrySql = """
            INSERT INTO orchestrator.SagaOutbox (saga_id, step_name, payload, sent, created_at)
            VALUES ($1, $2, $3, false, $4)
        """;

        await using var connection = await pgDataSource.OpenConnectionAsync();
        var tx = await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted);

        var timeNow = timeProvider.GetUtcNow().DateTime;
        await using var command1 = new NpgsqlCommand(createStepSql, connection, tx);
        command1.Parameters.AddRange(new NpgsqlParameter[]
        {
            new() { Value = sagaId, NpgsqlDbType = NpgsqlDbType.Uuid },
            new() { Value = stepName, NpgsqlDbType = NpgsqlDbType.Varchar },
            new() { Value = Guid.NewGuid(), NpgsqlDbType = NpgsqlDbType.Uuid },
            new() { Value = timeNow, NpgsqlDbType = NpgsqlDbType.Timestamp },
            new() { Value = timeNow, NpgsqlDbType = NpgsqlDbType.Timestamp },
        });
        
        await using var command2 = new NpgsqlCommand(createOutboxEntrySql, connection, tx);
        command2.Parameters.AddRange(new NpgsqlParameter[]
        {
            new() { Value = sagaId, NpgsqlDbType = NpgsqlDbType.Uuid },
            new() { Value = stepName, NpgsqlDbType = NpgsqlDbType.Varchar },
            new() { Value = payloadJson, NpgsqlDbType = NpgsqlDbType.Json },
            new() { Value = timeNow, NpgsqlDbType = NpgsqlDbType.Timestamp }
        });

        try
        {
            await command1.ExecuteNonQueryAsync();
            await command2.ExecuteNonQueryAsync();
            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task SetOutboxMessageSent(Guid sagaId, string stepName)
    {
        const string sql = """
            UPDATE orchestrator.SagaOutbox
            SET sent = true
            WHERE saga_id = $1 AND step_name = $2
        """;
        
        await using var connection = await pgDataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddRange(new NpgsqlParameter[]
        {
            new() { Value = sagaId, NpgsqlDbType = NpgsqlDbType.Uuid },
            new() { Value = stepName, NpgsqlDbType = NpgsqlDbType.Varchar },
        });
        
        var affectedRows = await command.ExecuteNonQueryAsync();
        switch (affectedRows)
        {
            case 0: throw new InvalidOperationException($"Now outbox row found for saga {sagaId}, step '{stepName}'");
            case >1: throw new InvalidOperationException("Data integrity violation");
        }
    }
}