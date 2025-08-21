using _Mafia_API.Models;
using _Mafia_API.Repositories;
using _Mafia_API.Services;
using Google.Cloud.TextToSpeech.V1;
using System;
using System.Diagnostics;
using System.Text;
using System.Text.Json.Serialization;
using System.Linq; // Added for .Any()

namespace _Mafia_API.Helpers
{
    public class VoiceHelper
    {

        public static void GenerateStaticText(string text, string outputFile, AnnouncementLanguage? language)
        {
            GenerateText(text, Path.Combine("voice_static", outputFile), language);
        }
        public static void GenerateDynamicText(string text, string outputFile, AnnouncementLanguage? language)
        {
            GenerateText(text, Path.Combine("voice_dynamic", outputFile), language);
        }







        private static AnnouncementLanguage DetectLanguage(string text)
        {
            string russian = "абвгдеёжзийклмнопрстуфхцчшщъыьэюя";
            string english = "abcdefghijklmnopqrstuvwxyz";
            string hebrew = "אבגדהוזחטיכלמנסעפצקרשת";

            // Check if text contains any Russian characters
            if (text.Any(c => russian.Contains(c)))
                return AnnouncementLanguage.ru;

            // Check if text contains any Hebrew characters
            if (text.Any(c => hebrew.Contains(c)))
                return AnnouncementLanguage.he;

            // Check if text contains any English characters
            if (text.Any(c => english.Contains(c)))
                return AnnouncementLanguage.en;

            // Default to English if no specific language characters are found
            return AnnouncementLanguage.en;
        }



        private static void GenerateText(string text, string outputFile, AnnouncementLanguage? language)
        {
            GenerateTextWithGoogleCloud(text, outputFile, language);
        }

        private static void GenerateTextWithGoogleCloud(string text, string outputFile, AnnouncementLanguage? language)
        {
            TextToSpeechClient client = TextToSpeechClient.Create();

            SynthesisInput input = new SynthesisInput
            {
                Text = text
            };

            VoiceSelectionParams voiceSelection = new VoiceSelectionParams
            {
                LanguageCode = language switch
                {
                    AnnouncementLanguage.ru => "ru-RU",
                    AnnouncementLanguage.en => "en-US",
                    AnnouncementLanguage.he => "he-IL",
                    _ => "en-US"
                },
                SsmlGender = SsmlVoiceGender.Male,
                Name = language switch
                {
                    AnnouncementLanguage.ru => "ru-RU-Wavenet-D",
                    AnnouncementLanguage.en => "en-US-Wavenet-D",
                    AnnouncementLanguage.he => "he-IL-Wavenet-D",
                    _ => "en-US-Wavenet-D"
                }
            };

            AudioConfig audioConfig = new AudioConfig
            {
                AudioEncoding = AudioEncoding.Mp3,
                Pitch = 0.8f,
                SpeakingRate = 1.2f, // Slightly faster for the "late-night host" feel
                VolumeGainDb = 16,
            };

            SynthesizeSpeechResponse response = client.SynthesizeSpeech(input, voiceSelection, audioConfig);

            // Ensure output directory exists
            var outputDir = Path.GetDirectoryName(outputFile);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            using (Stream output = File.Create(outputFile))
            {
                response.AudioContent.WriteTo(output);
            }
        }
        public static void ConcatenateMp3Files(string[] mp3Files, string outputFile)
        {
            // ffmpeg concat needs a text file listing all inputs
            var tempListFile = Path.GetTempFileName();

            try
            {
                using (var writer = new StreamWriter(tempListFile))
                {
                    foreach (var file in mp3Files)
                    {
                        // paths must be quoted if they contain spaces
                        writer.WriteLine($"file '{Path.GetFullPath(file).Replace("'", "'\\''")}'");
                    }
                }

                var psi = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = $"-y -f concat -safe 0 -i \"{tempListFile}\" -c copy \"{outputFile}\"",
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(psi))
                {
                    process.WaitForExit();
                    if (process.ExitCode != 0)
                    {
                        throw new Exception("ffmpeg failed: " + process.StandardError.ReadToEnd());
                    }
                }
            }
            finally
            {
                if (File.Exists(tempListFile))
                    File.Delete(tempListFile);
            }
        }

    }
}
