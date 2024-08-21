using _Mafia_API.Services;
using Google.Cloud.TextToSpeech.V1;
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
                Name = "ru-RU-Wavenet-D",
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
                        File.Copy(Path.Combine("voice_static", $"city_up_{variant}.mp3"), announcementPath);
                    }
                    break;


                case AnnouncementType.player_killed:
                    {
                        int variant = new Random().Next(3);
                        int variant_cmnt = new Random().Next(4);

                        ConcatenateMp3Files(new string[]
                        {
                            Path.Combine("voice_static",$"player_killed_pre_{variant}.mp3"),
                            userFile ,
                            Path.Combine("voice_static",$"player_killed_pst_{variant}.mp3"),
                            Path.Combine("voice_static",$"player_killed_cmnt_{variant_cmnt}.mp3")
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
            using (var outputStream = new FileStream(outputFile, FileMode.Create))
            {
                for (int i = 0; i < mp3Files.Length; i++)
                {
                    using (var inputStream = new FileStream(mp3Files[i], FileMode.Open))
                    {
                        // For the first file, copy everything
                        if (i == 0)
                        {
                            inputStream.CopyTo(outputStream);
                        }
                        else
                        {
                            // For subsequent files, skip ID3 tags and only copy MP3 frame data
                            SkipId3v2Tag(inputStream);

                            // Copy MP3 frames without the header from the second file onwards
                            byte[] buffer = new byte[1024];
                            int bytesRead;
                            while ((bytesRead = inputStream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                outputStream.Write(buffer, 0, bytesRead);
                            }
                        }
                    }
                }
            }
        }

        public static void SkipId3v2Tag(FileStream inputStream)
        {
            byte[] header = new byte[10];
            inputStream.Read(header, 0, 10);

            if (header[0] == 'I' && header[1] == 'D' && header[2] == '3')
            {
                // ID3v2 tag found, parse size
                int tagSize = (header[6] << 21) | (header[7] << 14) | (header[8] << 7) | header[9];

                // Skip the tag
                inputStream.Seek(tagSize, SeekOrigin.Current);
            }
            else
            {
                // No ID3v2 tag, reset stream position
                inputStream.Seek(0, SeekOrigin.Begin);
            }
        }

    }
}
