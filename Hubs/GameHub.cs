using _Mafia_API.Models;
using Microsoft.AspNetCore.SignalR;
using static _Mafia_API.Helpers.VoiceHelper;

namespace _Mafia_API.Hubs
{
    public class GameHub() : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();

        }

        public async Task In_AuthAs(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        }

        public async Task In_Announce(AnnouncementType type, string userId)
        {
            var announcement = GenerateAnnouncement(type, userId);

            if (announcement != null)
            {
                await Clients.Group(userId).SendAsync("announcement", announcement);
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);

            if (Context?.Items["UserID"]?.ToString() != null)
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, Context?.Items["UserID"].ToString());
        }

        public static void PushUserUpdate(IHubContext<GameHub> hubContext, User? user)
        {

            if (hubContext != null && user != null)
            {
                Console.Write("Pushing user update");
                hubContext.Clients.Group(user.id).SendAsync("userUpdated", user).Wait();
            }
            else
            {
                Console.Write("NOT pushing user update");
            }

        }
    }
}