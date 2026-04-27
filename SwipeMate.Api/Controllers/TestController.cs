using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SwipeMate.Api.Controllers;

/*[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    [HttpGet("ping")]
    public IActionResult Ping() => Ok("pong");
}*/

[ApiController]
[Route("api/test")]
public class TestController : ControllerBase
{
    [HttpGet("public")]
    public IActionResult Public() => Ok("public ok");

    [Authorize]
    [HttpGet("private")]
    public IActionResult Private() => Ok("private ok");
}

