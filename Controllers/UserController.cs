using _Mafia_API.Models;
using _Mafia_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace _Mafia_API.Controllers
{

    [ApiController]
    [AllowAnonymous]
    [Route("/api/[controller]")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public class UserController : ControllerBase
    {
        private readonly UserService userService;
        private readonly RoomService roomService;

        public UserController(UserService UserService, RoomService RoomService)
        {
            userService = UserService;
            roomService = RoomService;
        }

        [HttpGet]
        [Route("getUser")]
        public ActionResult<ResponseWrapper<User>> ObtainSession()
        {
            var user = HttpContext.MafiaUser();

            if (user == null)
            {
                user = userService.GetNewUser();
            }

            if (user == null)
                return BadRequest(new ResponseWrapper<User>(WrResponseStatus.InternalError));

            var cooptions = new CookieOptions
            {
                Path = "/",
                IsEssential = true,
                HttpOnly = false,
                Expires = DateTime.Now.AddDays(700),
                Domain = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development" ? "localhost" : ".nozsa.com",
                Secure = false,
            };

            HttpContext.Response.Cookies.Append("user", user.id, cooptions);

            return Ok(new ResponseWrapper<User>(WrResponseStatus.Ok, user));
        }

        [HttpPut]
        [Route("updateUser")]
        public ActionResult<ResponseWrapper<User>> UpdateUser(User? user)
        {

            user = userService.UpdateUser(user);

            if (user == null)
                return BadRequest(new ResponseWrapper<User>(WrResponseStatus.InternalError));

            var cooptions = new CookieOptions
            {
                Path = "/",
                IsEssential = true,
                HttpOnly = false,
                Expires = DateTime.Now.AddDays(700),
                Domain = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development" ? "localhost" : ".nozsa.com",
                Secure = false,
            };

            HttpContext.Response.Cookies.Append("user", user.id, cooptions);

            return Ok(new ResponseWrapper<User>(WrResponseStatus.Ok, user));
        }

        [HttpGet]
        [Route("fetchAnnouncement")]
        public ActionResult<ResponseWrapper<string>> FetchAnnouncement(string? announcement)
        {
            var filePath = Path.Combine("voice_dynamic", announcement);

            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var fileBytes = System.IO.File.ReadAllBytesAsync(filePath).Result;
            var base64FileContent = Convert.ToBase64String(fileBytes);

            var rsp = new ResponseWrapper<string>(WrResponseStatus.Ok, base64FileContent);

            return Ok(rsp);
        }
    }
}
