using System.Text.Json;
using System.Threading.Channels;
using Microsoft.AspNetCore.Mvc;

namespace Orchestrator;

[ApiController]
[Route("[controller]")]
public class Controller(
    ISagaRepository sagaRepository,
    ChannelWriter<Guid> sagaChannelWriter
    ) : ControllerBase
{
    [HttpPost]
    [Route("/start-saga")]
    public async Task<ActionResult> StartSaga()
    {
        var payload = await JsonDocument.ParseAsync(HttpContext.Request.Body);
        var sagaId = await sagaRepository.Create(payload);
        
        if (!sagaChannelWriter.TryWrite(sagaId))
            Console.WriteLine("Queue full :(");
            
        return Ok();
    }
}