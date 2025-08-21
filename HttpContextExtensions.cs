using _Mafia_API.Models;
using _Mafia_API.Repositories;
using _Mafia_API.Services;

namespace _Mafia_API
{
    public static class HttpContextExtensions
    {
        public static User? MafiaUser(this HttpContext? context)
        {
            var userId = context?.Items["UserID"] as string;
            return UserRepository.GetUserById(userId ?? "");
        }

        public static Room? MafiaRoom(this HttpContext? context)
        {
            return context.MafiaUser()?.Room;
        }
    }
}
