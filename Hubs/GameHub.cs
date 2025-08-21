using _Mafia_API.Helpers;
using _Mafia_API.Models;
using _Mafia_API.Models.DTOs;
using _Mafia_API.Repositories;
using _Mafia_API.Services;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace _Mafia_API.Hubs
{
#pragma warning disable CS9113 // Parameter is unread.
    public class GameHub(IServiceProvider serviceProvider, AnnouncementService announcementService, Scheduler scheduler) : Hub
#pragma warning restore CS9113 // Parameter is unread.
    {

        public static ConcurrentDictionary<string, string> ConnectionUserPairs = new();

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();

            await In_Authenticate();

        }
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }


        protected UserService userService
        {
            get
            {
                return serviceProvider.GetRequiredService<UserService>();
            }
        }

        protected RoomService roomService
        {
            get
            {
                return serviceProvider.GetRequiredService<RoomService>();
            }
        }

        protected GameService gameService
        {
            get
            {
                return serviceProvider.GetRequiredService<GameService>();
            }
        }



        public async Task<bool> In_Authenticate()
        {
            if (Context.GetHttpContext()?.MafiaUser()?.Id != null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, Context.GetHttpContext()?.MafiaUser()?.Id ?? "");

                if (Context.GetHttpContext()?.MafiaRoom()?.roomCode != null)
                    await Groups.AddToGroupAsync(Context.ConnectionId, Context.GetHttpContext()?.MafiaRoom()?.roomCode ?? "");

                ConnectionUserPairs[Context.ConnectionId] = Context.GetHttpContext()?.MafiaUser()?.Id;

                return true;
            }
            else
                Console.WriteLine("User is not yet registered or context is null");

            return false;
        }


        public async Task<RoomDTO?> In_createAndJoinRoom()
        {
            var user = Context.GetHttpContext()?.MafiaUser();
            if (user != null)
            {
                var room = roomService.createNewRoom(user.Id);
                await userService.JoinRoom(user.Id, room.roomCode);
                await Groups.AddToGroupAsync(Context.ConnectionId, room.roomCode);

                return RoomDTO.FromRoom(room);
            }

            var currentRoom = Context.GetHttpContext()?.MafiaRoom();
            return currentRoom != null ? RoomDTO.FromRoom(currentRoom) : null;
        }


        public async Task<RoomDTO?> In_JoinRoom(string roomCode)
        {
            var user = Context.GetHttpContext()?.MafiaUser();
            if (user != null)
            {
                if ((await userService.JoinRoom(user.Id, roomCode)) == true)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);
                }
            }

            var currentRoom = Context.GetHttpContext()?.MafiaRoom();
            return currentRoom != null ? RoomDTO.FromRoom(currentRoom) : null;
        }


        public void In_StartGame()
        {
            scheduler.Detached(async (u, r, game) =>
            {
                await game.StartGame(r!.Id);
            }, Context);
        }


        //let's impelent IN_{setVote/setVoteConfirmed/setName}
        public async Task<UserDTO?> IN_SetVote(string? vote)
        {
            var user = Context.GetHttpContext()?.MafiaUser();
            if (user != null)
            {
                var updatedUser = await userService.SetVoteAsync(user.Id, vote);
                return updatedUser != null ? UserDTO.FromUser(updatedUser) : null;
            }
            else
                return null;
        }

        public async Task<UserDTO?> IN_SetVoteConfirmed(bool voteConfirmed)
        {
            var user = Context.GetHttpContext()?.MafiaUser();
            if (user != null)
            {
                var updatedUser = await userService.SetVoteConfirmedAsync(user.Id, voteConfirmed);

                scheduler.Detached(async (u, r, game) =>
                {
                    if (game != null)
                    {
                        await game.onVoteConfirmed(r!.Id);
                    }
                }, Context);

                return updatedUser != null ? UserDTO.FromUser(updatedUser) : null;
            }
            else
                return null;
        }

        public async Task<UserDTO?> IN_SetName(string? fullname)
        {
            var user = Context.GetHttpContext()?.MafiaUser();
            if (user != null)
            {
                var updatedUser = await userService.SetNameAsync(user.Id, fullname);
                return updatedUser != null ? UserDTO.FromUser(updatedUser) : null;
            }
            else
                return null;
        }


        //let's impelement IN_ set mafia count doctor slut and sherif enabled and keep in mind that settings only can be changed by room master
        public async Task<RoomDTO?> IN_SetMafiaCount(int mafiaCount)
        {
            var room = Context.GetHttpContext()?.MafiaRoom();
            if (room != null && room.roomOwnerId == Context.GetHttpContext()?.MafiaUser()?.Id)
            {
                var updatedRoom = await roomService.SetMafiaCountAsync(room.roomCode, mafiaCount);
                return updatedRoom != null ? RoomDTO.FromRoom(updatedRoom) : null;
            }
            else
                return null;
        }

        public async Task<RoomDTO?> IN_SetDoctorEnabled(bool doctorEnabled)
        {
            var room = Context.GetHttpContext()?.MafiaRoom();
            if (room != null && room.roomOwnerId == Context.GetHttpContext()?.MafiaUser()?.Id)
            {
                var updatedRoom = await roomService.SetDoctorEnabledAsync(room.roomCode, doctorEnabled);
                return updatedRoom != null ? RoomDTO.FromRoom(updatedRoom) : null;
            }
            else
                return null;
        }

        public async Task<RoomDTO?> IN_SetSheriffEnabled(bool sheriffEnabled)
        {
            var room = Context.GetHttpContext()?.MafiaRoom();
            if (room != null && room.roomOwnerId == Context.GetHttpContext()?.MafiaUser()?.Id)
            {
                var updatedRoom = await roomService.SetSheriffEnabledAsync(room.roomCode, sheriffEnabled);
                return updatedRoom != null ? RoomDTO.FromRoom(updatedRoom) : null;
            }
            else
                return null;
        }

        public static async Task DeauthenticateConnection(string userId, string roomCode, IHubContext<GameHub> Context)
        {
            var connectionPairs = ConnectionUserPairs
                .Where(kp => kp.Value == userId)
                .ToList();

            foreach (var kp in connectionPairs)
            {
                await Context.Groups.RemoveFromGroupAsync(kp.Key, roomCode);
            }
        }

        public async Task<RoomDTO?> IN_KickUser(string userId)
        {
            var room = Context.GetHttpContext()?.MafiaRoom();
            var targetUser = userService.GetUser(userId);
            var currentUser = Context.GetHttpContext()?.MafiaUser();
            if (
                room != null &&
                room.roomOwnerId == currentUser?.Id &&
                targetUser != null &&
                targetUser.currentRoomId == room.Id)
            {

                IHubContext<GameHub> hubContext = Context.GetHttpContext()?.RequestServices.GetRequiredService<IHubContext<GameHub>>() ?? throw new InvalidOperationException("Hub context is not available.");

                await userService.KickUser_unguarded(userId);
                await DeauthenticateConnection(userId, room.roomCode, hubContext);
            }
            return room != null ? RoomDTO.FromRoom(room) : null;
        }

        public async Task<RoomDTO?> IN_SetSlutEnabled(bool slutEnabled)
        {
            var room = Context.GetHttpContext()?.MafiaRoom();
            if (room != null && room.roomOwnerId == Context.GetHttpContext()?.MafiaUser()?.Id)
            {
                var updatedRoom = await roomService.SetSlutEnabledAsync(room.roomCode, slutEnabled);
                return updatedRoom != null ? RoomDTO.FromRoom(updatedRoom) : null;
            }
            else
                return null;
        }

        public async Task<RoomDTO?> IN_SetAnnouncementLanguage(AnnouncementLanguage announcementLanguage)
        {
            var room = Context.GetHttpContext()?.MafiaRoom();
            if (room != null && room.roomOwnerId == Context.GetHttpContext()?.MafiaUser()?.Id)
            {
                var updatedRoom = await roomService.SetAnnouncementLanguageAsync(room.roomCode, announcementLanguage);
                return updatedRoom != null ? RoomDTO.FromRoom(updatedRoom) : null;
            }
            else
                return null;
        }

        public async Task<RoomDTO?> IN_AnnouncementPEGIRating(AnnouncementPEGIRating announcementPEGIRating)
        {
            var room = Context.GetHttpContext()?.MafiaRoom();
            if (room != null && room.roomOwnerId == Context.GetHttpContext()?.MafiaUser()?.Id)
            {
                var updatedRoom = await roomService.SetAnnouncementPEGIRatingAsync(room.roomCode, announcementPEGIRating);
                return updatedRoom != null ? RoomDTO.FromRoom(updatedRoom) : null;
            }
            else
                return null;
        }








        public static async Task PushUserUpdateAsync(User? user, IHubContext<GameHub> Context, bool skipRoomUpdate = false)
        {
            if (user != null)
            {
                await Context.Clients.Group(user.Id).SendAsync("userUpdated", UserDTO.FromUser(user));
                if (user.currentRoomId != null && !skipRoomUpdate)
                {
                    await GameHub.PushRoomUpdateAsync(RoomRepository.GetRoomById(user.currentRoomId), Context);
                }
            }
        }

        public static async Task PushRoomUpdateAsync(Room room, IHubContext<GameHub> Context)
        {
            if (room != null && room.users.Count() != 0)
                await Context.Clients.Group(room.roomCode).SendAsync("roomUpdate", RoomDTO.FromRoom(room));
        }

        public static async Task PushAnnouncementAsync(Announcement announcement, Room room, User user, IHubContext<GameHub> Context)
        {
            if (announcement != null)
            {
                // Get the announcement service to handle timing
                await AnnouncementService.WaitForAnnouncementTimingAsync(room);

                // Send the announcement
                await Context.Clients.Group(room.roomCode).SendAsync("announcement", await announcement.GetFileAsync(user));

                // Update the room's lastAnnouncement timestamp
                RoomService.UpdateLastAnnouncement(room.Id, DateTime.UtcNow);
            }
        }


        public static async Task PushTextAnnouncementAsync(TextAnnouncementType announcement, User user, User targetUser, IHubContext<GameHub> Context)
        {
            await Context.Clients.Group(user.Id).SendAsync("textAnnouncement", announcement, UserDTO.FromUser(targetUser));
        }

    }
}