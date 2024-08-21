using _Mafia_API.Models;
using _Mafia_API.Services;

namespace _Mafia_API
{
    public static class HttpContextExtensions
    {

        public static User? MafiaUser(this HttpContext context)
        {
            return UserService.UserStore.Find(u => u.id == (context.Items["UserID"] as string ?? ""));
        }
    }
}
