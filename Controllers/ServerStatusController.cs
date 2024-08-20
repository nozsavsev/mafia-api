using _Mafia_API;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[AllowAnonymous]
[Route("/api/[controller]")]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
public class ServerStatusController : ControllerBase
{
    public ServerStatusController()
    {
    }

    [HttpGet]
    [AllowAnonymous]
    public ActionResult<ResponseWrapper<string>> Status()
    {
        return Ok(new ResponseWrapper<string>(WrResponseStatus.Ok));
    }
}
