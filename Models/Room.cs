using _Mafia_API.Helpers;
using _Mafia_API.Repositories;
using System.Text.Json.Serialization;
using Macross.Json.Extensions;
using System;
using System.Threading;

namespace _Mafia_API.Models
{

    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum CurrentStage
    {
        [JsonPropertyName("lobby")]
        lobby,

        [JsonPropertyName("day")]
        day,

        [JsonPropertyName("night")]
        night,

        [JsonPropertyName("mafia")]
        mafia,

        [JsonPropertyName("doctor")]
        doctor,

        [JsonPropertyName("sheriff")]
        sheriff,

        [JsonPropertyName("slut")]
        slut,

        [JsonPropertyName("endGame")]
        endGame,

        [JsonPropertyName("notEnoughPlayers")]
        notEnoughPlayers,
    }

    public class Room
    {
        public string Id { get; set; } = SnowflakeGlobal.Generate();
        public string roomCode { get; set; } = RoomNumber.Generate();

        public bool isFirstNight { get; set; } = true;

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

        public string roomOwnerId { get; set; } = null!;
        
        public DateTime? lastAnnouncement { get; set; } = null;
        
        [JsonIgnore]
        public SemaphoreSlim AnnouncementMutex { get; set; } = new SemaphoreSlim(1, 1);
        
        public IEnumerable<User> users
        {
            get
            {
                return UserRepository.GetAllUsers().Where(u => u.currentRoomId == Id);
            }
        }
    }
}
