using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace Orchestrator;

[ApiController]
[Route("[controller]")]
public class Controller(NpgsqlDataSource dataSource) : ControllerBase
{
    [HttpPost]
    [Route("/start-saga")]
    public async Task<ActionResult> StartSaga()
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand("SELECT 1", connection);
        await using var reader = await command.ExecuteReaderAsync();
        
        if (!reader.HasRows)
            return NotFound();
        
        return Ok();
    }
}