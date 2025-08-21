using _Mafia_API.Models;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Macross.Json.Extensions;
using _Mafia_API.Services;

namespace _Mafia_API.Models.DTOs
{
    public class RoomDTO
    {
        public string Id { get; set; } = string.Empty;
        public string roomCode { get; set; } = string.Empty;
        public bool isFirstNight { get; set; } = true;
        public bool canStartGame { get; set; } = false;
        public CurrentStage currentStage { get; set; } = CurrentStage.lobby;
        public AnnouncementLanguage announcementLanguage { get; set; } = AnnouncementLanguage.en;
        public AnnouncementPEGIRating announcementPEGIRating { get; set; } = AnnouncementPEGIRating.PEGI_07;
        public int mafiaCount { get; set; } = 2;
        public string? mafiaFinalVote { get; set; } = null;
        public bool doctorEnabled { get; set; } = true;
        public string? doctorFinalVote { get; set; } = null;
        public bool sheriffEnabled { get; set; } = true;
        public bool slutEnabled { get; set; } = false;
        public string? slutFinalVote { get; set; } = null;
        public string? dayBanFinalVote { get; set; } = null;
        public string roomOwnerId { get; set; } = string.Empty;
        public IEnumerable<UserDTO> users { get; set; } = new List<UserDTO>();

        public static RoomDTO? FromRoom(Room? room)
        {
            if (room == null)
                return null;

            return new RoomDTO
            {
                Id = room?.Id,
                roomCode = room?.roomCode,
                isFirstNight = room.isFirstNight,
                currentStage = room.currentStage,
                announcementLanguage = room.announcementLanguage,
                announcementPEGIRating = room.announcementPEGIRating,
                mafiaCount = room.mafiaCount,
                mafiaFinalVote = room.mafiaFinalVote,
                doctorEnabled = room.doctorEnabled,
                doctorFinalVote = room.doctorFinalVote,
                sheriffEnabled = room.sheriffEnabled,
                slutEnabled = room.slutEnabled,
                slutFinalVote = room.slutFinalVote,
                dayBanFinalVote = room.dayBanFinalVote,
                roomOwnerId = room.roomOwnerId,
                users = room?.users.Select(u => UserDTO.FromUser(u, true)).ToList(),
                canStartGame = RoomService.CanStartGame(room.Id)
            };
        }
    }
}
