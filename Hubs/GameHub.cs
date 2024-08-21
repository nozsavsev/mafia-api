using _Mafia_API.Models;
using _Mafia_API.Services;
using Microsoft.AspNetCore.SignalR;
using static _Mafia_API.Helpers.VoiceHelper;

namespace _Mafia_API.Hubs
{
    public class GameHub(UserService userService, RoomService roomService) : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public async Task In_AuthAs(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        }

        public async Task In_JoinRoom(string roomCode, string userId)
        {
            var user = userService.GetUser(userId);
            var room = roomService.GetRoom(roomCode);

            if (user != null && room != null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);

                if (userService.GetUsersOfRoom(roomCode).Count == 0)
                    user.isGameMaster = true;

                user.currentRoom = roomCode;

                userService.UpdateUser(user);
                roomService.UpdateRoom(room);
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }

        public static void PushUserUpdate(IHubContext<GameHub> hubContext, User? user)
        {
            if (hubContext != null && user != null)
            {
                hubContext.Clients.Group(user.id).SendAsync("userUpdated", user).Wait();
            }
        }

        public static void PushAnounencment(IHubContext<GameHub> hubContext, AnnouncementType type, string roomCode, string userId)
        {
            var announcement = GenerateAnnouncement(type, userId);

            if (announcement != null)
            {
                hubContext.Clients.Group(roomCode).SendAsync("announcement", announcement).Wait();
            }
        }

        public static void PushRoomUpdate(IHubContext<GameHub> hubContext, Room room, List<User>? users)
        {
            if (hubContext != null && users.Count != null)
            {
                hubContext.Clients.Group(room.roomCode).SendAsync("roomUpdate", room, users).Wait();
            }
        }
    }
}