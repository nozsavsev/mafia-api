using _Mafia_API.Helpers;
using System.Text.Json.Serialization;

namespace _Mafia_API.Models
{

    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum CurrentState
    {
        [JsonPropertyName("day")]
        day = 0,

        [JsonPropertyName("mafia")]
        mafia,

        [JsonPropertyName("slut")]
        slut,

        [JsonPropertyName("sherif")]
        sherif,

        [JsonPropertyName("doctor")]
        doctor,
    }

    public class Room
    {
        public string Id { get; set; } = SnowflakeGlobal.Generate();
        public List<User> Users { get; set; }
        public string roomCode { get; set; } = RoomNumber.Generate();
        public int nightCount { get; set; } = 0;
    }
}
