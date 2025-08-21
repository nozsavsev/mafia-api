using _Mafia_API.Helpers;
using _Mafia_API.Repositories;
using Bogus;
using System.Text.Json.Serialization;
using Macross.Json.Extensions;

namespace _Mafia_API.Models
{
    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum UserRole
    {
        [JsonPropertyName("peaceful")]
        peaceful = 0,

        [JsonPropertyName("mafia")]
        mafia,

        [JsonPropertyName("doctor")]
        doctor,

        [JsonPropertyName("sheriff")]
        sheriff,

        [JsonPropertyName("slut")]
        slut,
    }

    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum UserStatus
    {
        [JsonPropertyName("alive")]
        alive = 0,

        [JsonPropertyName("dead")]
        dead,
    }

    public class User
    {
        public string Id { get; set; } = SnowflakeGlobal.Generate();
        protected string? _currentRoomId { get; set; } = null;
        public string? currentRoomId { get; set; } = null;
        public Room? Room { get { return RoomRepository.GetRoomById(currentRoomId); } }

        public string? fullName { get; set; } = null;
        public UserRole? role { get; set; } = UserRole.peaceful;
        public UserStatus status { get; set; } = UserStatus.alive;


        public string? vote { get; set; } = null;
        public bool voteConfirmed { get; set; } = false;

        public bool? isGameMaster { get { return RoomRepository.GetRoomById(currentRoomId)?.roomOwnerId == Id; } }


    }
}
