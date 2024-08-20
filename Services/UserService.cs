
using _Mafia_API.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace _Mafia_API.Services
{
    public class UserService
    {
        IHubContext<GameHub> hubContext;

        public UserService(IHubContext<GameHub> hubContext)
        {
            this.hubContext = hubContext;
        }

    }
}
