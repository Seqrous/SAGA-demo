using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace OrdersService;

[ApiController]
[Route("[controller]")]
public class OrderController(NpgsqlDataSource dataSource) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult> Create()
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand("SELECT 1", connection);
        await using var reader = await command.ExecuteReaderAsync();

        if (!reader.HasRows)
            return NotFound();
        
        return Ok();
    }
}