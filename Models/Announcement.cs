using _Mafia_API.Helpers;
using _Mafia_API.Repositories;
using System.Text;
using System.Text.Json.Serialization;
using Macross.Json.Extensions;

namespace _Mafia_API.Models
{

    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum AnnouncementLanguage
    {
        [JsonPropertyName("en")]
        en = 0,

        [JsonPropertyName("ru")]
        ru,

        [JsonPropertyName("he")]
        he,
    }

    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum AnnouncementType
    {
        [JsonPropertyName("game_start")]
        game_start = 0,

        [JsonPropertyName("intro_start")]
        intro_start,

        [JsonPropertyName("user_intro")]
        user_intro,


        [JsonPropertyName("city_down")]
        city_down,
        [JsonPropertyName("city_up")]
        city_up,

        [JsonPropertyName("slut_on")]
        slut_on,
        [JsonPropertyName("slut_off")]
        slut_off,

        [JsonPropertyName("mafia_on")]
        mafia_on,
        [JsonPropertyName("mafia_off")]
        mafia_off,

        [JsonPropertyName("doctor_on")]
        doctor_on,
        [JsonPropertyName("doctor_off")]
        doctor_off,


        [JsonPropertyName("sheriff_on")]
        sheriff_on,
        [JsonPropertyName("sheriff_off")]
        sheriff_off,



        [JsonPropertyName("player_killed")]
        player_killed,
        [JsonPropertyName("player_vote_killed")]
        player_vote_killed,


        [JsonPropertyName("mafia_win")]
        mafia_win,
        [JsonPropertyName("peaceful_win")]
        peaceful_win,

        [JsonPropertyName("not_enough_players")]
        not_enough_players,

    }


    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum TextAnnouncementType
    {
        [JsonPropertyName("sheriff_is_correct")]
        sheriff_is_correct = 0,

        [JsonPropertyName("sheriff_is_wrong")]
        sheriff_is_wrong,

        [JsonPropertyName("sheriff_is_blocked")]
        sheriff_is_blocked,

    }


    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum AnnouncementPEGIRating
    {
        [JsonPropertyName("PEGI_7")]
        PEGI_07 = 0,

        [JsonPropertyName("PEGI_16")]
        PEGI_16 = 1,

        [JsonPropertyName("PEGI_18")]
        PEGI_18 = 2,
    }


    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum PersonalizedAnnouncementType
    {
        [JsonPropertyName("city_down")]
        city_down = 0,
        [JsonPropertyName("city_up")]
        city_up,

        [JsonPropertyName("slut_on")]
        slut_on,
        [JsonPropertyName("slut_off")]
        slut_off,

        [JsonPropertyName("mafia_on")]
        mafia_on,
        [JsonPropertyName("mafia_off")]
        mafia_off,

        [JsonPropertyName("doctor_on")]
        doctor_on,
        [JsonPropertyName("doctor_off")]
        doctor_off,


        [JsonPropertyName("sheriff_on")]
        sheriff_on,
        [JsonPropertyName("sheriff_off")]
        sheriff_off,



        [JsonPropertyName("player_killed")]
        player_killed,
        [JsonPropertyName("player_vote_killed")]
        player_vote_killed,


        [JsonPropertyName("mafia_win")]
        mafia_win,
        [JsonPropertyName("peacfull_win")]
        peacfull_win,
    }
    public class Announcement
    {
        public Announcement(
            AnnouncementType type,
            AnnouncementLanguage language,
            AnnouncementPEGIRating pegiRating,
            string text,
            string? postText = "",
            bool isPersonalized = false
            )
        {
            this.text = text;
            this.type = type;
            this.language = language;
            this.pegiRating = pegiRating;
            this.postText = postText;
            this.isPersonalized = isPersonalized;
        }

        public string text { get; set; }
        public AnnouncementType type { get; set; }
        public AnnouncementLanguage language { get; set; }
        public AnnouncementPEGIRating pegiRating { get; set; }
        public string? postText { get; set; }
        public bool isPersonalized { get; set; } = false;

        public string key
        {
            get
            {
                return CryptoHelper.GenerateMD5(
                    text +
                    type.ToString() +
                    language.ToString() +
                    postText
                    );
            }
        }

        public bool AvailableOnDisk
        {
            get
            {
                return File.Exists(Path.Combine("voice_static", key + ".mp3"));
            }
        }

        public async Task<string> GetFileAsync(User user)
        {
            if (isPersonalized)
            {
                var finalPath = Path.Combine("voice_dynamic", $"{key}_{user.Id}"+ ".mp3");

                if (File.Exists(finalPath))
                {
                    Console.WriteLine($"Using dynamic file for {key} for user {user.Id}");
                    return $"dynamic/{key}_{user.Id}";
                }

                Console.WriteLine($"Generating dynamic file for {key} for user {user.Id}");

                VoiceHelper.ConcatenateMp3Files(
                    new string[]
                    {
                        Path.Combine("voice_static", key + ".mp3"),
                        Path.Combine("voice_dynamic", $"{user.Id}" + ".mp3"),
                        Path.Combine("voice_static", key + "_post" + ".mp3"),
                    },
                    finalPath
                );

                return $"dynamic/{key}_{user.Id}";
            }
            else
            {
                Console.WriteLine($"Using static file for {key}");

                if (File.Exists(Path.Combine("voice_static", key + ".mp3")))
                    return $"static/{key}";

                await PreGenerateFileAsync();
                return $"static/{key}";
            }
        }

        public async Task PreGenerateFileAsync()
        {
            await Task.Run(() =>
            {

                if (string.IsNullOrEmpty(text) == false && !File.Exists(Path.Combine("voice_static", key + ".mp3")))
                    VoiceHelper.GenerateStaticText(text, key + ".mp3", language);
                else
                    Console.WriteLine($"Skipping generation of static file for {key} because text is empty or file already exists.");
                if (string.IsNullOrEmpty(postText) == false && !File.Exists(Path.Combine("voice_static", key + "_post" + ".mp3")))
                    VoiceHelper.GenerateStaticText(postText, key + "_post" + ".mp3", language);
                else if(string.IsNullOrEmpty(postText) == false)
                    Console.WriteLine($"Skipping generation of static post file for {key} because text is empty or file already exists.");
            });
        }

    }
}