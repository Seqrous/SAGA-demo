using Microsoft.AspNetCore.Mvc;

namespace OrdersService;

[ApiController]
[Route("[controller]")]
public class OrderController : ControllerBase
{
    [HttpPost]
    public ActionResult Create()
    {
        return Ok();
    }
}