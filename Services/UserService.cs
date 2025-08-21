
using _Mafia_API.Helpers;
using _Mafia_API.Hubs;
using _Mafia_API.Models;
using _Mafia_API.Repositories;
using Bogus.DataSets;
using Google.Api.Gax;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace _Mafia_API.Services
{
#pragma warning disable CS9113 // Parameter is unread.
    public class UserService(GameHub mafiaRealtime, IServiceProvider serviceProvider, IHubContext<GameHub> hubContext, Scheduler scheduler)
#pragma warning restore CS9113 // Parameter is unread.
    {
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

        public User? GetUser(string? id)
                 => UserRepository.GetUserById(id);
        public User? CreateUser()
             => UserRepository.NewUser();


        public void DeleteUser(string id)
        {
            KickUser_unguarded(id);
            UserRepository.DeleteUser(id);
        }

        //called by SignalR hub
        public async Task<bool> JoinRoom(string userId, string roomCode)
        {
            var user = GetUser(userId);
            var room = RoomRepository.GetRoomByCode(roomCode);

            if (user != null && room != null && room.currentStage == CurrentStage.lobby)
            {
                if (user.currentRoomId == room.Id)
                    KickUser_unguarded(userId);

                user.currentRoomId = room.Id;

                await GameHub.PushUserUpdateAsync(user, hubContext);
                await GameHub.PushRoomUpdateAsync(room, hubContext);

                return true;
            }

            return false;
        }

        public async Task KickUser_unguarded(string id)
        {
            var user = GetUser(id);
            var ownerChanged = false;
            if (user != null)
            {
                var room = RoomRepository.GetRoomById(user.currentRoomId);

                if (room != null)
                {
                    if (room.users.Count() == 1)
                    {
                        roomService.DeleteRoom(room.Id);
                    }
                    else if (room.roomOwnerId == user.Id)
                    {
                        room.roomOwnerId = room.users.First(u => u.Id != user.Id).Id;
                        ownerChanged = true;
                    }
                }

                user.currentRoomId = null;

                await GameHub.PushUserUpdateAsync(user, hubContext);
                await GameHub.PushRoomUpdateAsync(room, hubContext);

                if (ownerChanged)
                    await GameHub.PushUserUpdateAsync(GetUser(room.roomOwnerId), hubContext);

                if (room?.currentStage != CurrentStage.lobby && room != null)
                    await gameService.ReEvaluateGame(room.Id);
            }
        }

        public async Task<User?> SetNameAsync(string userId, string? fullname)
        {
            var user = GetUser(userId);

            if (user != null && fullname != user.fullName && !string.IsNullOrEmpty(fullname))
            {
                VoiceHelper.GenerateDynamicText(fullname, user.Id + ".mp3", null);

                user.fullName = fullname;
                Console.WriteLine($"name is set tp {GetUser(userId).fullName} : {fullname} (expected)");
            }

            await GameHub.PushUserUpdateAsync(user, hubContext);

            return user;
        }


        public async Task<User?> SetRoleAsync(string userId, UserRole? newRole, bool skipRoomUpdate = false)
        {
            var user = GetUser(userId);

            if (user == null)
                return null;

            user.role = newRole;

            await GameHub.PushUserUpdateAsync(user, hubContext, skipRoomUpdate);

            return user;
        }

        public async Task<User?> SetStatusAsync(string userId, UserStatus newStatus)
        {
            var user = GetUser(userId);

            if (user == null)
                return null;

            user.status = newStatus;

            await GameHub.PushUserUpdateAsync(user, hubContext);

            return user;
        }

        public async Task<User?> SetVoteAsync(string userId, string? vote)
        {
            var user = GetUser(userId);

            if (user == null)
                return null;

            user.vote = vote;

            await GameHub.PushUserUpdateAsync(user, hubContext);

            return user;
        }

        public async Task<User?> SetVoteConfirmedAsync(string userId, bool voteConfirmed)
        {
            var user = GetUser(userId);

            if (user == null)
                return null;

            user.voteConfirmed = voteConfirmed;

            await GameHub.PushUserUpdateAsync(user, hubContext);

            return user;
        }
    }
}
