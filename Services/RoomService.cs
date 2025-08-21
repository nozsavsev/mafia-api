using _Mafia_API.Hubs;
using _Mafia_API.Models;
using _Mafia_API.Repositories;
using Microsoft.AspNetCore.SignalR;

namespace _Mafia_API.Services
{
#pragma warning disable CS9113 // Parameter is unread.
#pragma warning disable CS9113 // Parameter is unread.
    public class RoomService(GameHub mafiaRealtime, UserService userService, IHubContext<GameHub> hubContext)
#pragma warning restore CS9113 // Parameter is unread.
#pragma warning restore CS9113 // Parameter is unread.
    {
        public Room createNewRoom(string roomOwnerId)
            => RoomRepository.NewRoom(roomOwnerId);
        public Room? GetRoom(string? IdOrCode)
        {
            return RoomRepository.GetRoomById(IdOrCode) ?? RoomRepository.GetRoomByCode(IdOrCode);
        }

        public async Task<bool> DeleteRoom(string? IdOrCode)
        {
            var room = GetRoom(IdOrCode);
            if (room == null)
                return false;

            foreach (var user in room.users)
            {
                user.currentRoomId = null;
                await GameHub.PushUserUpdateAsync(user, hubContext);
            }

            RoomRepository.DeleteRoom(room.Id);
            return true;
        }

        public async Task<Room?> SetIsFirstNightAsync(string IdOrCode, bool isFirstNight)
        {
            var room = GetRoom(IdOrCode);

            if (room == null)
                return null;

            room.isFirstNight = isFirstNight;

            await GameHub.PushRoomUpdateAsync(room, hubContext);

            return room;
        }

        public async Task<Room?> SetCurrentStageAsync(string IdOrCode, CurrentStage currentStage)
        {
            var room = GetRoom(IdOrCode);

            if (room == null)
                return null;

            room.currentStage = currentStage;

            await GameHub.PushRoomUpdateAsync(room, hubContext);

            return room;
        }

        public async Task<Room?> SetMafiaCountAsync(string IdOrCode, int mafiaCount)
        {
            var room = GetRoom(IdOrCode);

            if (room == null || room.currentStage != CurrentStage.lobby)
                return null;

            room.mafiaCount = mafiaCount;

            await GameHub.PushRoomUpdateAsync(room, hubContext);

            return room;
        }

        public async Task<Room?> SetMafiaFinalVoteAsync(string IdOrCode, string? mafiaFinalVote)
        {
            var room = GetRoom(IdOrCode);

            if (room == null)
                return null;

            room.mafiaFinalVote = mafiaFinalVote;

            await GameHub.PushRoomUpdateAsync(room, hubContext);

            return room;
        }

        public async Task<Room?> SetDoctorEnabledAsync(string IdOrCode, bool doctorEnabled)
        {
            var room = GetRoom(IdOrCode);

            if (room == null || room.currentStage != CurrentStage.lobby)
                return null;

            room.doctorEnabled = doctorEnabled;

            await GameHub.PushRoomUpdateAsync(room, hubContext);

            return room;
        }

        public async Task<Room?> SetDoctorFinalVoteAsync(string IdOrCode, string? doctorFinalVote)
        {
            var room = GetRoom(IdOrCode);

            if (room == null)
                return null;

            room.doctorFinalVote = doctorFinalVote;

            await GameHub.PushRoomUpdateAsync(room, hubContext);

            return room;
        }

        public async Task<Room?> SetSheriffEnabledAsync(string IdOrCode, bool sheriffEnabled)
        {
            var room = GetRoom(IdOrCode);

            if (room == null || room.currentStage != CurrentStage.lobby)
                return null;

            room.sheriffEnabled = sheriffEnabled;

            await GameHub.PushRoomUpdateAsync(room, hubContext);

            return room;
        }

        public async Task<Room?> SetSlutEnabledAsync(string IdOrCode, bool slutEnabled)
        {
            var room = GetRoom(IdOrCode);

            if (room == null || room.currentStage != CurrentStage.lobby)
                return null;

            room.slutEnabled = slutEnabled;

            await GameHub.PushRoomUpdateAsync(room, hubContext);

            return room;
        }

        public async Task<Room?> SetSlutFinalVoteAsync(string IdOrCode, string? slutFinalVote)
        {
            var room = GetRoom(IdOrCode);

            if (room == null)
                return null;

            room.slutFinalVote = slutFinalVote;

            await GameHub.PushRoomUpdateAsync(room, hubContext);

            return room;
        }

        public async Task<Room?> SetDayBanFinalVoteAsync(string IdOrCode, string? dayBanFinalVote)
        {
            var room = GetRoom(IdOrCode);

            if (room == null)
                return null;

            room.dayBanFinalVote = dayBanFinalVote;

            await GameHub.PushRoomUpdateAsync(room, hubContext);

            return room;
        }

        public async Task<Room?> SetRoomOwnerIdAsync(string IdOrCode, string roomOwnerId)
        {
            var room = GetRoom(IdOrCode);

            if (room == null)
                return null;

            room.roomOwnerId = roomOwnerId;

            await GameHub.PushRoomUpdateAsync(room, hubContext);

            return room;
        }

        public async Task<Room?> SetAnnouncementLanguageAsync(string IdOrCode, AnnouncementLanguage announcementLanguage)
        {
            var room = GetRoom(IdOrCode);

            if (room == null || room.currentStage != CurrentStage.lobby)
                return null;

            room.announcementLanguage = announcementLanguage;

            await GameHub.PushRoomUpdateAsync(room, hubContext);

            return room;
        }

        public async Task<Room?> SetAnnouncementPEGIRatingAsync(string IdOrCode, AnnouncementPEGIRating announcementPEGIRating)
        {
            var room = GetRoom(IdOrCode);

            if (room == null || room.currentStage != CurrentStage.lobby)
                return null;

            room.announcementPEGIRating = announcementPEGIRating;

            await GameHub.PushRoomUpdateAsync(room, hubContext);

            return room;
        }

        public static bool CanStartGame(string Id)
        {
            var room = RoomRepository.GetRoomById(Id);

            if (room == null || room.mafiaCount <= 0)
                return false;

            var playerCount = room.users.Count();
            var requiredRoles = room.mafiaCount;

            if (room.doctorEnabled)
                requiredRoles++;
            if (room.sheriffEnabled)
                requiredRoles++;
            if (room.slutEnabled)
                requiredRoles++;

            requiredRoles++; //atleast one civil

            if (requiredRoles < room.mafiaCount)//if user sets 3 mafia and disables all stuff, we need more civil then mafia to start the game
                requiredRoles = room.mafiaCount;

            return playerCount >= requiredRoles;
        }

        public static Room? UpdateLastAnnouncement(string Id, DateTime lastAnnouncement)
        {
            var room = RoomRepository.GetRoomById(Id);

            if (room == null)
                return null;

            room.lastAnnouncement = lastAnnouncement;
            return room;
        }


    }
}
