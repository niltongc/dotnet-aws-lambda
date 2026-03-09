using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Model;

namespace DotnetLambda.Controllers;

[Route("[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetUser()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Nano2",
            Email = "nano@nano.com" 
        };
        return Ok(user);
    }
}

