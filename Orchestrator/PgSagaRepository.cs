using Npgsql;

namespace Orchestrator;

public class PgSagaRepository(NpgsqlDataSource pgDataSource) : ISagaRepository
{
    public async Task<Guid> Create()
    {
        const string sql = """
            INSERT INTO orchestrator.SagaLog (saga_id, status, current_step)
            VALUES ($1, 'CREATED', NULL);
        """;

        var sagaId = Guid.NewGuid();
        await using var connection = await pgDataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.Add(new NpgsqlParameter { Value = sagaId });
        
        _ = await command.ExecuteNonQueryAsync();
        return sagaId;
    }
}