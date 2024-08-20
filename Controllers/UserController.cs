using _Mafia_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace _Mafia_API.Controllers
{

    [ApiController]
    [Route("/api/[controller]")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public class UserController : ControllerBase
    {
        private readonly UserService userService;

        public UserController(UserService UserService)
        {
            userService = UserService;
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("getNewUser")]
        public ActionResult<ResponseWrapper<string>> ObtainSession()
        {
            var session = HttpContext.MafiaUser();
            return BadRequest(new ResponseWrapper<string>(WrResponseStatus.NotFound, session));
        }

    }
}
