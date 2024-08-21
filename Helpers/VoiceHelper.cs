using _Mafia_API.Models;
using _Mafia_API.Services;
using Google.Cloud.TextToSpeech.V1;
using NAudio.Wave;
using System.Text.Json.Serialization;

namespace _Mafia_API.Helpers
{
    public class VoiceHelper
    {
        public static void GenerateText(string text, string outputFile)
        {

            outputFile = Path.Combine("voice_dynamic", outputFile);

            TextToSpeechClient client = TextToSpeechClient.Create();

            SynthesisInput input = new SynthesisInput
            {
                Text = text
            };
            VoiceSelectionParams voiceSelection = new VoiceSelectionParams
            {
                LanguageCode = "ru-RU",
                SsmlGender = SsmlVoiceGender.Male,
                Name = "ru-RU-Wavenet-D"
            };

            AudioConfig audioConfig = new AudioConfig
            {
                AudioEncoding = AudioEncoding.Mp3,
                Pitch = 0.8f,
                SpeakingRate = 1.0,
                VolumeGainDb = 16,

            };
            SynthesizeSpeechResponse response = client.SynthesizeSpeech(input, voiceSelection, audioConfig);
            using (Stream output = File.Create(outputFile))
            {
                response.AudioContent.WriteTo(output);
            }



        }

        [JsonConverter(typeof(JsonStringEnumMemberConverter))]
        public enum AnnouncementType
        {
            [JsonPropertyName("city_down")]
            city_down = 0,

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

            [JsonPropertyName("sherif_on")]
            sherif_on,

            [JsonPropertyName("sherif_off")]
            sherif_off,

            [JsonPropertyName("city_up")]
            city_up,



            [JsonPropertyName("player_killed")]
            player_killed,

            [JsonPropertyName("player_healed")]
            player_healed,

            [JsonPropertyName("vote_killed")]
            vote_killed

        }

        public static string GenerateAnnouncement(AnnouncementType type, string? userId = null)
        {
            string announcement = "announcement_" + SnowflakeGlobal.Generate();
            string announcementPath = Path.Combine("voice_dynamic", announcement);
            string userFile = Path.Combine("voice_dynamic", userId);

            var user = UserService.st_GetUser(userId);

            switch (type)
            {
                case AnnouncementType.city_down:
                    {
                        int variant = new Random().Next(2);
                        File.Copy(Path.Combine("voice_static", $"city_down_{variant}.mp3"), announcementPath);
                    }
                    break;
                case AnnouncementType.slut_on:
                    {
                        int variant = new Random().Next(3);
                        File.Copy(Path.Combine("voice_static", $"slut_on_{variant}.mp3"), announcementPath);
                    }
                    break;
                case AnnouncementType.slut_off:
                    {
                        int variant = new Random().Next(3);
                        File.Copy(Path.Combine("voice_static", $"slut_off_{variant}.mp3"), announcementPath);
                    }
                    break;
                case AnnouncementType.mafia_on:
                    {
                        int variant = new Random().Next(3);
                        File.Copy(Path.Combine("voice_static", $"mafia_on_{variant}.mp3"), announcementPath);
                    }
                    break;
                case AnnouncementType.mafia_off:
                    {
                        int variant = new Random().Next(3);
                        File.Copy(Path.Combine("voice_static", $"mafia_off_{variant}.mp3"), announcementPath);
                    }
                    break;
                case AnnouncementType.doctor_on:
                    {
                        int variant = new Random().Next(3);
                        File.Copy(Path.Combine("voice_static", $"doctor_on_{variant}.mp3"), announcementPath);
                    }
                    break;

                case AnnouncementType.doctor_off:
                    {
                        int variant = new Random().Next(3);
                        File.Copy(Path.Combine("voice_static", $"doctor_off_{variant}.mp3"), announcementPath);
                    }
                    break;
                case AnnouncementType.sherif_on:
                    {
                        int variant = new Random().Next(3);
                        File.Copy(Path.Combine("voice_static", $"sherif_on_{variant}.mp3"), announcementPath);
                    }
                    break;
                case AnnouncementType.sherif_off:
                    {
                        int variant = new Random().Next(3);
                        File.Copy(Path.Combine("voice_static", $"sherif_off_{variant}.mp3"), announcementPath);
                    }
                    break;
                case AnnouncementType.city_up:
                    {
                        int variant = new Random().Next(3);
                        File.Copy(Path.Combine($"voice_static", "city_up_{variant}.mp3"), announcementPath);
                    }
                    break;


                case AnnouncementType.player_killed:
                    {
                        int variant = new Random().Next(3);
                        int variant_cmnt = new Random().Next(4);

                        ConcatenateMp3Files(new string[]
                        {
                            Path.Combine($"voice_static","player_killed_pre_{variant}.mp3"),
                            userFile ,
                            Path.Combine($"voice_static","player_killed_pst_{variant}.mp3"),
                            Path.Combine($"voice_static","player_killed_cmnt_{variant_cmnt}.mp3")
                        },
                        announcementPath);

                    }
                    break;
                case AnnouncementType.player_healed:
                    {
                        int variant = new Random().Next(0);
                        int variant_cmnt = new Random().Next(2);

                        ConcatenateMp3Files(new string[]
                        {
                            Path.Combine($"voice_static","player_healed_pre_{variant}.mp3"),
                            userFile ,
                            Path.Combine($"voice_static","player_healed_cmnt_{variant_cmnt}.mp3")
                        },
                        announcementPath);
                    }
                    break;
                case AnnouncementType.vote_killed:
                    {
                        int variant = new Random().Next(3);

                        ConcatenateMp3Files(new string[]
                        {
                            Path.Combine($"voice_static","vote_killed_{variant}.mp3"),
                            userFile ,
                        },
                        announcementPath);
                    }
                    break;

            }

            return announcement;
        }


        public static void ConcatenateMp3Files(string[] mp3Files, string outputFile)
        {
            using (var waveFileWriter = new WaveFileWriter(outputFile, new Mp3FileReader(mp3Files[0]).WaveFormat))
            {
                foreach (var mp3File in mp3Files)
                {
                    using (var reader = new Mp3FileReader(mp3File))
                    {
                        var buffer = new byte[reader.Length];
                        int read;
                        while ((read = reader.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            waveFileWriter.Write(buffer, 0, read);
                        }
                    }
                }
            }
        }

    }
}
