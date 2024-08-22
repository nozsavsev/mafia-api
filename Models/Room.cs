using _Mafia_API.Helpers;
using System.Text.Json.Serialization;

namespace _Mafia_API.Models
{

    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum CurrentState
    {
        [JsonPropertyName("day")]
        day = 0,

        [JsonPropertyName("allSleeping")]
        allSleeping,

        [JsonPropertyName("mafia")]
        mafia,

        [JsonPropertyName("slut")]
        slut,

        [JsonPropertyName("sherif")]
        sherif,

        [JsonPropertyName("doctor")]
        doctor,

        [JsonPropertyName("lobbie")]
        lobbie,
    }

    public class Room
    {
        public string Id { get; set; } = SnowflakeGlobal.Generate();
        public string roomCode { get; set; } = RoomNumber.Generate();
        public int nightCount { get; set; } = 0;
        public CurrentState state { get; set; } = CurrentState.lobbie;
        public bool slutEnabled { get; set; } = false;
        public int mafiaCount { get; set; } = 2;
        public bool doctorEnabled { get; set; } = true;
        public bool sherifEnabled { get; set; } = true;
        public bool sayHealedPlayer { get; set; } = false;
    }
}
