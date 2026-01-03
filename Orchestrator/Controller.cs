using Microsoft.AspNetCore.Mvc;

namespace Orchestrator;

[ApiController]
[Route("[controller]")]
public class Controller(ISagaRepository sagaRepository) : ControllerBase
{
    [HttpPost]
    [Route("/start-saga")]
    public async Task<ActionResult> StartSaga()
    {
        await sagaRepository.Create();
        return Ok();
    }
}