using _Mafia_API.Helpers;
using _Mafia_API.Models;
using _Mafia_API.Repositories;
using System.Text.Json.Serialization;
using System;

namespace _Mafia_API.Services
{

    public class AnnouncementService
    {
        private readonly Announcement[] announcements = new Announcement[]
        {
            
            // GAME START
            new Announcement( AnnouncementType.game_start, AnnouncementLanguage.en, AnnouncementPEGIRating.PEGI_07,"The game has begun."),
            new Announcement( AnnouncementType.game_start, AnnouncementLanguage.ru, AnnouncementPEGIRating.PEGI_07,"Игра началась."),
            new Announcement( AnnouncementType.game_start, AnnouncementLanguage.he, AnnouncementPEGIRating.PEGI_07,"המשחק התחיל."),

            // INTRO START
            new Announcement( AnnouncementType.intro_start, AnnouncementLanguage.en, AnnouncementPEGIRating.PEGI_07,"Everyone, introduce yourself"),
            new Announcement( AnnouncementType.intro_start, AnnouncementLanguage.ru, AnnouncementPEGIRating.PEGI_07,"Всем, представьтесь"),
            new Announcement( AnnouncementType.intro_start, AnnouncementLanguage.he, AnnouncementPEGIRating.PEGI_07,"כולם, מצאו עצמכם"),

            // USER INTRO
            new Announcement( AnnouncementType.user_intro, AnnouncementLanguage.en, AnnouncementPEGIRating.PEGI_07,"Player ", " tell us who you are", true),
            new Announcement( AnnouncementType.user_intro, AnnouncementLanguage.ru, AnnouncementPEGIRating.PEGI_07,"Игрок ", " расскажите, кто вы", true),
            new Announcement( AnnouncementType.user_intro, AnnouncementLanguage.he, AnnouncementPEGIRating.PEGI_07,"השחקן ", " תספרו לנו מי אתם", true),

            // CITY DOWN
            new Announcement( AnnouncementType.city_down, AnnouncementLanguage.en, AnnouncementPEGIRating.PEGI_07,"The city goes to sleep."),
            new Announcement( AnnouncementType.city_down, AnnouncementLanguage.ru, AnnouncementPEGIRating.PEGI_07,"Город засыпает."),
            new Announcement( AnnouncementType.city_down, AnnouncementLanguage.he, AnnouncementPEGIRating.PEGI_07,"העיר הולכת לישון."),

            // CITY UP
            new Announcement( AnnouncementType.city_up, AnnouncementLanguage.en, AnnouncementPEGIRating.PEGI_07,"The city wakes up"),
            new Announcement( AnnouncementType.city_up, AnnouncementLanguage.ru, AnnouncementPEGIRating.PEGI_07,"Город просыпается"),
            new Announcement( AnnouncementType.city_up, AnnouncementLanguage.he, AnnouncementPEGIRating.PEGI_07,"העיר מתעוררת"),

            // SLUT ON
            new Announcement( AnnouncementType.slut_on, AnnouncementLanguage.en, AnnouncementPEGIRating.PEGI_07,"Love is in the air. Sort of"),
            new Announcement( AnnouncementType.slut_on, AnnouncementLanguage.ru, AnnouncementPEGIRating.PEGI_07,"Девушка выходит на охоту"),
            new Announcement( AnnouncementType.slut_on, AnnouncementLanguage.he, AnnouncementPEGIRating.PEGI_07,"הנערה יוצאת לעבודה"),

            // SLUT OFF
            new Announcement( AnnouncementType.slut_off, AnnouncementLanguage.en, AnnouncementPEGIRating.PEGI_07,"Her work is done for the night"),
            new Announcement( AnnouncementType.slut_off, AnnouncementLanguage.ru, AnnouncementPEGIRating.PEGI_07,"Похитительница сердец отдыхает"),
            new Announcement( AnnouncementType.slut_off, AnnouncementLanguage.he, AnnouncementPEGIRating.PEGI_07,"הנערה חוזרת הביתה"),

            // MAFIA ON
            new Announcement( AnnouncementType.mafia_on, AnnouncementLanguage.en, AnnouncementPEGIRating.PEGI_07,"The mafia wakes up"),
            new Announcement( AnnouncementType.mafia_on, AnnouncementLanguage.ru, AnnouncementPEGIRating.PEGI_07,"Мафия просыпается"),
            new Announcement( AnnouncementType.mafia_on, AnnouncementLanguage.he, AnnouncementPEGIRating.PEGI_07,"המאפיה מתעוררת"),

            // MAFIA OFF
            new Announcement( AnnouncementType.mafia_off, AnnouncementLanguage.en, AnnouncementPEGIRating.PEGI_07,"The mafia goes back to sleep"),
            new Announcement( AnnouncementType.mafia_off, AnnouncementLanguage.ru, AnnouncementPEGIRating.PEGI_07,"Мафия снова ложится спать"),
            new Announcement( AnnouncementType.mafia_off, AnnouncementLanguage.he, AnnouncementPEGIRating.PEGI_07,"המאפיה חוזרת לישון"),

            // DOCTOR ON
            new Announcement( AnnouncementType.doctor_on, AnnouncementLanguage.en, AnnouncementPEGIRating.PEGI_07,"Doctor is on duty"),
            new Announcement( AnnouncementType.doctor_on, AnnouncementLanguage.ru, AnnouncementPEGIRating.PEGI_07,"Доктор начал обход"),
            new Announcement( AnnouncementType.doctor_on, AnnouncementLanguage.he, AnnouncementPEGIRating.PEGI_07,"הרופא במשמרת"),

            // DOCTOR OFF
            new Announcement( AnnouncementType.doctor_off, AnnouncementLanguage.en, AnnouncementPEGIRating.PEGI_07,"Doctor goes back to bed"),
            new Announcement( AnnouncementType.doctor_off, AnnouncementLanguage.ru, AnnouncementPEGIRating.PEGI_07,"Доктор идет спать"),
            new Announcement( AnnouncementType.doctor_off, AnnouncementLanguage.he, AnnouncementPEGIRating.PEGI_07,"הרופא הולך לישון"),

            // SHERIFF ON
                new Announcement( AnnouncementType.sheriff_on, AnnouncementLanguage.en, AnnouncementPEGIRating.PEGI_07,"Sheriff is on patrol"),
            new Announcement( AnnouncementType.sheriff_on, AnnouncementLanguage.ru, AnnouncementPEGIRating.PEGI_07,"Шериф выходит на патруль"),
            new Announcement( AnnouncementType.sheriff_on, AnnouncementLanguage.he, AnnouncementPEGIRating.PEGI_07,"השריף יוצא לסיור"),

            // SHERIFF OFF
            new Announcement( AnnouncementType.sheriff_off, AnnouncementLanguage.en, AnnouncementPEGIRating.PEGI_07,"Sheriff goes back to rest"),
            new Announcement( AnnouncementType.sheriff_off, AnnouncementLanguage.ru, AnnouncementPEGIRating.PEGI_07,"Шериф идет отдыхать"),
            new Announcement( AnnouncementType.sheriff_off, AnnouncementLanguage.he, AnnouncementPEGIRating.PEGI_07,"השריף חוזר לנוח"),

            // PLAYER KILLED
            new Announcement( AnnouncementType.player_killed, AnnouncementLanguage.en, AnnouncementPEGIRating.PEGI_07, "Player ", " was eliminated tonight", true),
            new Announcement( AnnouncementType.player_killed, AnnouncementLanguage.ru, AnnouncementPEGIRating.PEGI_07, "Игрок ", " был убит этой ночью", true),
            new Announcement( AnnouncementType.player_killed, AnnouncementLanguage.he, AnnouncementPEGIRating.PEGI_07, "השחקן ", " נפל הלילה", true),

            // PLAYER VOTE KILLED
            new Announcement( AnnouncementType.player_vote_killed, AnnouncementLanguage.en, AnnouncementPEGIRating.PEGI_07, "By vote, ", " was removed", true),
            new Announcement( AnnouncementType.player_vote_killed, AnnouncementLanguage.ru, AnnouncementPEGIRating.PEGI_07, "Город решил, ", " должен уйти", true),
            new Announcement( AnnouncementType.player_vote_killed, AnnouncementLanguage.he, AnnouncementPEGIRating.PEGI_07, "בהצבעה, ", " הוסר", true),

            // MAFIA WIN    
            new Announcement( AnnouncementType.mafia_win, AnnouncementLanguage.en, AnnouncementPEGIRating.PEGI_07, "The mafia has won"),
            new Announcement( AnnouncementType.mafia_win, AnnouncementLanguage.ru, AnnouncementPEGIRating.PEGI_07, "Мафия победила"),
            new Announcement( AnnouncementType.mafia_win, AnnouncementLanguage.he, AnnouncementPEGIRating.PEGI_07, "המאפיה ניצחה"),

            // PEACEFUL WIN
            new Announcement( AnnouncementType.peaceful_win, AnnouncementLanguage.en, AnnouncementPEGIRating.PEGI_07, "The town wins. Peace restored"),
            new Announcement( AnnouncementType.peaceful_win, AnnouncementLanguage.ru, AnnouncementPEGIRating.PEGI_07, "Город победил. Мир восстановлен"),
            new Announcement( AnnouncementType.peaceful_win, AnnouncementLanguage.he, AnnouncementPEGIRating.PEGI_07, "העיר ניצחה. שלום חוזר"),

            // NOT ENOUGH PLAYERS
            new Announcement( AnnouncementType.not_enough_players, AnnouncementLanguage.en, AnnouncementPEGIRating.PEGI_07, "Not enough players to continue the game"),
            new Announcement( AnnouncementType.not_enough_players, AnnouncementLanguage.ru, AnnouncementPEGIRating.PEGI_07, "Недостаточно игроков для продолжения игры"),
            new Announcement( AnnouncementType.not_enough_players, AnnouncementLanguage.he, AnnouncementPEGIRating.PEGI_07, "אין מספיק שחקנים כדי להמשיך את המשחק"),


        };

        public async Task EnsureAllGeneratedAsync()
        {
            const int batchSize = 10;
            int totalBatches = (int)Math.Ceiling((double)announcements.Length / batchSize);

            for (int batch = 0; batch < totalBatches; batch++)
            {
                int startIndex = batch * batchSize;
                int endIndex = Math.Min(startIndex + batchSize, announcements.Length);
                int currentBatchSize = endIndex - startIndex;

                Console.WriteLine($"Processing batch {batch + 1}/{totalBatches} (announcements {startIndex + 1}-{endIndex})");

                var batchTasks = new List<Task>();
                for (int i = startIndex; i < endIndex; i++)
                {
                    batchTasks.Add(announcements[i].PreGenerateFileAsync());
                }

                await Task.WhenAll(batchTasks);
                Console.WriteLine($"Completed batch {batch + 1}/{totalBatches}");
            }

            Console.WriteLine("All announcements generated");
        }

        public Announcement GetRandomAnnouncement(AnnouncementType type, AnnouncementLanguage language, AnnouncementPEGIRating pegiRating)
        {
            // Try to find exact match first
            var suitableAnnouncements = announcements.Where(a => a.type == type && a.language == language && a.pegiRating == pegiRating).ToArray();

            if (suitableAnnouncements.Length > 0)
            {
                return suitableAnnouncements[Random.Shared.Next(0, suitableAnnouncements.Length)];
            }

            // If exact match not found, gradually reduce PEGI requirements (never go below PEGI 7+)
            // Start with the requested PEGI rating and work our way down
            for (int currentPegi = (int)pegiRating; currentPegi >= 0; currentPegi--)
            {
                var fallbackAnnouncements = announcements.Where(a => a.type == type && a.language == language && a.pegiRating == (AnnouncementPEGIRating)currentPegi).ToArray();

                if (fallbackAnnouncements.Length > 0)
                {
                    return fallbackAnnouncements[Random.Shared.Next(0, fallbackAnnouncements.Length)];
                }
            }

            // If no announcements found for the type and language at any PEGI rating, return null
            return null!; //not happening
        }

        /// <summary>
        /// Ensures proper timing between announcements for a room
        /// </summary>
        /// <param name="room">The room to check timing for</param>
        /// <returns>Task that completes when it's safe to send the next announcement</returns>
        public static async Task WaitForAnnouncementTimingAsync(Room room)
        {
            if (room.lastAnnouncement.HasValue)
            {
                var timeSinceLastAnnouncement = DateTime.UtcNow - room.lastAnnouncement.Value;
                var requiredDelay = TimeSpan.FromSeconds(2.5);
                
                if (timeSinceLastAnnouncement < requiredDelay)
                {
                    var remainingDelay = requiredDelay - timeSinceLastAnnouncement;
                    await Task.Delay(remainingDelay);
                }
            }
        }


    }

}