using _Mafia_API.Helpers;
using Bogus;
using System.Text.Json.Serialization;

namespace _Mafia_API.Models
{
    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum UserRole
    {
        [JsonPropertyName("peacfull")]
        peacfull = 0,

        [JsonPropertyName("slut")]
        slut,

        [JsonPropertyName("mafia")]
        mafia,

        [JsonPropertyName("sherif")]
        sherif,

        [JsonPropertyName("doctor")]
        doctor,
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
        public string? id { get; set; } = SnowflakeGlobal.Generate();
        public string? fullName { get; set; } = new Faker().Person.FullName;
        public bool? nameConfirmed { get; set; } = false;
        public UserRole? role { get; set; } = UserRole.peacfull;
        public UserStatus? status { get; set; } = UserStatus.alive;
        public int? positiveVotes { get; set; } = 0;
        public int? negativeVotes { get; set; } = 0;
        public bool? isGameMaster { get; set; } = false;
        public string? currentRoom { get; set; } = null;
    }
}
