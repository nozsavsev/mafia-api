using _Mafia_API.Helpers;
using _Mafia_API.Hubs;
using _Mafia_API.Models;
using _Mafia_API.Models.DTOs;
using _Mafia_API.Services;
using Google.Api.Gax;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using static _Mafia_API.Helpers.VoiceHelper;

namespace _Mafia_API.Controllers
{

    [ApiController]
    [AllowAnonymous]
    [Route("/api/[controller]")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
#pragma warning disable CS9113 // Parameter is unread.
#pragma warning disable CS9113 // Parameter is unread.
    public class UserController(UserService userService, GameHub mafiaRealtime, Scheduler scheduler, AnnouncementService announcementService, IHubContext<GameHub> hubContext) : ControllerBase
#pragma warning restore CS9113 // Parameter is unread.
#pragma warning restore CS9113 // Parameter is unread.
    {

        [HttpGet]
        [Route("getUser")]
        public ActionResult<ResponseWrapper<UserDTO>> ObtainSession()
        {
            var user = HttpContext.MafiaUser();

            if (user == null)
            {
                user = userService.CreateUser();
            }

            if (user == null)
                return BadRequest(new ResponseWrapper<UserDTO>(WrResponseStatus.InternalError));

            var cookieOptions = new CookieOptions
            {
                Path = "/",
                IsEssential = true,
                HttpOnly = false,
                Expires = DateTime.Now.AddDays(700),
                Domain = HttpContext.Request.Host.Host,
                Secure = false,
                SameSite = SameSiteMode.Lax
            };

            HttpContext.Response.Cookies.Append("user", user.Id, cookieOptions);

            return Ok(new ResponseWrapper<UserDTO>(WrResponseStatus.Ok, UserDTO.FromUser(user)));
        }

        [HttpPost]
        [Route("testAnnouncement")]
        public async Task<ActionResult<ResponseWrapper<UserDTO>>> TestAnnouncement()
        {
            var user = HttpContext.MafiaUser();
            if (user == null || user.Room == null)
            {
                return BadRequest(new ResponseWrapper<UserDTO>(WrResponseStatus.InternalError));
            }

            var announcementType = (AnnouncementType)Random.Shared.Next((int)AnnouncementType.game_start,  (int)AnnouncementType.peaceful_win +1);

            announcementType = AnnouncementType.player_killed;

            scheduler.ScheduleAnnouncement(TimeSpan.FromSeconds(1), announcementService.GetRandomAnnouncement(
                    announcementType,
                    user.Room.announcementLanguage,
                    user.Room.announcementPEGIRating),
                user.Room,
                user);

            return Ok(new ResponseWrapper<UserDTO>(WrResponseStatus.Ok));
        }


        [HttpGet]
        [Route("logOut")]
        public ActionResult<ResponseWrapper<UserDTO>> LogOut()
        {
            var user = HttpContext.MafiaUser();
            if (user != null)
            {
                userService.DeleteUser(user.Id);
            }

            HttpContext.Response.Cookies.Delete("user");

            return Ok(new ResponseWrapper<UserDTO>(WrResponseStatus.Ok));
        }


        [HttpGet]
        [Route("currentRoom")]
        public ActionResult<ResponseWrapper<RoomDTO?>> GetCurrentRoom()
        {
            var room = HttpContext?.MafiaUser()?.Room;
            var rsp = new ResponseWrapper<RoomDTO?>(WrResponseStatus.Ok, room != null ? RoomDTO.FromRoom(room) : null);
            return Ok(rsp);
        }


        public class LetsMakeFrontendTyped
        {
            public RoomDTO room { get; set; } = new();
            public UserDTO user { get; set; } = new();
            public Announcement announcement { get; set; }
            public AnnouncementLanguage announcementLanguage { get; set; }
            public AnnouncementPEGIRating announcementPEGIRating { get; set; }
            public AnnouncementType announcementType { get; set; }
        }

        [HttpPost]
        [Route("unused")]
        public ActionResult Unused(
            LetsMakeFrontendTyped args
            )//simply for openapi generator to puck up classes and enums
        {
            return Ok();
        }




        [HttpGet]
        [Route("fetchAnnouncement")]
        public ActionResult<ResponseWrapper<string>> FetchAnnouncement(string announcement)
        {
            bool isStatic = announcement.StartsWith("static/");
            string sanitizedAnnouncmentPath = announcement.Split('/')[1].Replace("/", "").Replace("\\", "").Replace(".", "");

            string path = $"{(isStatic ? "voice_static" : "voice_dynamic")}/{sanitizedAnnouncmentPath}.mp3";

            if (!System.IO.File.Exists(path))
                return NotFound();

            var fileBytes = System.IO.File.ReadAllBytesAsync(path).Result;
            var base64FileContent = Convert.ToBase64String(fileBytes);

            var rsp = new ResponseWrapper<string>(WrResponseStatus.Ok, base64FileContent);

            return Ok(rsp);
        }
    }
}
