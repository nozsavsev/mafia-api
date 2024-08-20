using _Mafia_API.Models;
using Microsoft.AspNetCore.SignalR;

namespace _Mafia_API.Hubs
{
    public class GameHub() : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();

            //var userId = Context?.Items["UserID"]?.ToString();
            //if (userId != null)
            //{
            //    await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            //}
            //else
            //{
            //    Console.Write("NOT adding user to group");
            //}
        }


       public async Task in_AuthAs(string userId)
       {
           await Groups.AddToGroupAsync(Context.ConnectionId, userId);
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