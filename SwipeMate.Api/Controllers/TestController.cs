using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SwipeMate.Api.Controllers;



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


