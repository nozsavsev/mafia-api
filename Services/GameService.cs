using _Mafia_API.Helpers;
using _Mafia_API.Hubs;
using _Mafia_API.Models;
using Microsoft.AspNetCore.SignalR;
using System.Numerics;

namespace _Mafia_API.Services
{
    class IStageProcessor
    {

        protected UserService userService;
        protected RoomService roomService;
        protected IHubContext<GameHub> hubContext;
        protected AnnouncementService announcementService;

        public IStageProcessor(
        UserService UserService,
        RoomService RoomService,
        IHubContext<GameHub> HubContext,
        AnnouncementService AnnouncementService
        )
        {
            userService = UserService;
            roomService = RoomService;
            hubContext = HubContext;
            announcementService = AnnouncementService;


        }



        public virtual async Task<bool> StageInAsync(Room room) { return false; }
        public virtual async Task<bool> OnVoteConfirmed(Room room) { return false; }
        public virtual async Task StageOutAsync(Room room) { }
    }



    class MafiaStageProcessor(
        UserService userService,
        RoomService roomService,
        IHubContext<GameHub> hubContext,
        AnnouncementService announcementService) : IStageProcessor(userService, roomService, hubContext, announcementService)
    {
        public override async Task<bool> StageInAsync(Room room)
        {
            // Mafia stage entering logic here

            await room.AnnouncementMutex.WaitAsync();
            try
            {
                await GameHub.PushAnnouncementAsync(
                    announcementService.GetRandomAnnouncement(
                        AnnouncementType.mafia_on,
                        room.announcementLanguage,
                        room.announcementPEGIRating),
                    room,
                    null!,
                    hubContext);
            }
            finally
            {
                room.AnnouncementMutex.Release();
            }

            await roomService.SetCurrentStageAsync(room.Id, CurrentStage.mafia);
            // Clear votes and confirmations for all mafias (both alive and dead)
            var allMafias = room.users.Where(u => u.role == UserRole.mafia).ToList();
            foreach (var mafia in allMafias)
            {
                await userService.SetVoteAsync(mafia.Id, null);
                await userService.SetVoteConfirmedAsync(mafia.Id, false);
            }

            // Check if we have any alive mafias
            var aliveMafias = room.users.Where(u => u.role == UserRole.mafia && u.status == UserStatus.alive).ToList();
            if (aliveMafias.Count == 0)
            {
                // No alive mafias - skip this stage
                return true;
            }

            return false;
        }

        public override async Task<bool> OnVoteConfirmed(Room room)
        {
            // Mafia stage logic here
            // All ALIVE mafias must confirm on one vote
            var aliveMafias = room.users.Where(u => u.role == UserRole.mafia && u.status == UserStatus.alive).ToList();

            if (aliveMafias.Count == 0)
            {
                // No alive mafias - skip this stage
                return true;
            }

            // Check if all alive mafias have confirmed their votes
            var allConfirmed = aliveMafias.All(u => u.voteConfirmed);
            if (!allConfirmed)
            {
                return false; // Not all votes are confirmed
            }

            // On first night, no actual killing happens
            if (room.isFirstNight)
            {
                // Clear votes and confirmations for next stage
                foreach (var mafia in aliveMafias)
                {
                    await userService.SetVoteAsync(mafia.Id, null);
                    await userService.SetVoteConfirmedAsync(mafia.Id, false);
                }
                return true;
            }
            var firstVote = aliveMafias.First().vote;
            var areVotesTheSame = aliveMafias.All(u => u.vote == firstVote);
            if (!areVotesTheSame)
            {
                return false; // Votes are not unanimous
            }
            // Check if all alive mafias voted for the same target
            
            if (firstVote == null)
            {
                return true; // No vote is still valid
            }



            // Check if target is valid (not dead)
            var target = userService.GetUser(firstVote);
            if (target == null || target.status == UserStatus.dead)
            {
                // Invalid target - clear votes and try again
                foreach (var mafia in aliveMafias)
                {
                    await userService.SetVoteAsync(mafia.Id, null);
                    await userService.SetVoteConfirmedAsync(mafia.Id, false);
                }
                return false;
            }

            // Set the final vote and clear individual votes
            room.mafiaFinalVote = firstVote;
            
            // Clear votes and confirmations for next stage
            foreach (var mafia in aliveMafias)
            {
                await userService.SetVoteAsync(mafia.Id, null);
                await userService.SetVoteConfirmedAsync(mafia.Id, false);
            }

            return true; // Go to next stage
        }

        public override async Task StageOutAsync(Room room)
        {
            // Logic for when the mafia stage ends

            await room.AnnouncementMutex.WaitAsync();
            try
            {
                await GameHub.PushAnnouncementAsync(
                    announcementService.GetRandomAnnouncement(
                        AnnouncementType.mafia_off,
                        room.announcementLanguage,
                        room.announcementPEGIRating),
                    room,
                    null!,
                    hubContext);
            }
            finally
            {
                room.AnnouncementMutex.Release();
            }
            await roomService.SetCurrentStageAsync(room.Id, CurrentStage.night);

            // Ensure all mafias have cleared votes (both alive and dead)
            var allMafias = room.users.Where(u => u.role == UserRole.mafia).ToList();
            foreach (var mafia in allMafias)
            {
                await userService.SetVoteAsync(mafia.Id, null);
                await userService.SetVoteConfirmedAsync(mafia.Id, false);
            }
        }
    }


    class SlutStageProcessor(
    UserService userService,
    RoomService roomService,
    IHubContext<GameHub> hubContext,
    AnnouncementService announcementService) : IStageProcessor(userService, roomService, hubContext, announcementService)
    {
        public override async Task<bool> StageInAsync(Room room)
        {
            // Slut stage entering logic here

            await room.AnnouncementMutex.WaitAsync();
            try
            {
                await GameHub.PushAnnouncementAsync(
                    announcementService.GetRandomAnnouncement(
                        AnnouncementType.slut_on,
                        room.announcementLanguage,
                        room.announcementPEGIRating),
                    room,
                    null!,
                    hubContext);
            }
            finally
            {
                room.AnnouncementMutex.Release();
            }

            await roomService.SetCurrentStageAsync(room.Id, CurrentStage.slut);
            // Clear votes and confirmations for all sluts (both alive and dead)
            var allSluts = room.users.Where(u => u.role == UserRole.slut).ToList();
            foreach (var slut in allSluts)
            {
                await userService.SetVoteAsync(slut.Id, null);
                await userService.SetVoteConfirmedAsync(slut.Id, false);
            }

            // Check if we have any alive sluts
            var aliveSlut = room.users.Where(u => u.role == UserRole.slut && u.status == UserStatus.alive).FirstOrDefault();
            if (aliveSlut == null)
            {
                // No alive slut - skip this stage
                return true;
            }

            return false;
        }

        public override async Task<bool> OnVoteConfirmed(Room room)
        {
            // Slut stage logic here
            // Only ALIVE slut can vote and confirm
            var aliveSlut = room.users.Where(u => u.role == UserRole.slut && u.status == UserStatus.alive).FirstOrDefault();

            if (aliveSlut == null)
            {
                // Slut is dead - skip this stage
                return true;
            }

            if (!aliveSlut.voteConfirmed)
            {
                return false; // Vote not confirmed
            }

            // On first night, no actual blocking happens
            if (room.isFirstNight)
            {
                // Clear votes and confirmations for next stage
                await userService.SetVoteAsync(aliveSlut.Id, null);
                await userService.SetVoteConfirmedAsync(aliveSlut.Id, false);
                return true;
            }

            if (aliveSlut.vote == null)
            {
                return true; 
            }

            var target = userService.GetUser(aliveSlut.vote);
            if (target != null && target.status == UserStatus.dead)
            {
                // Invalid target - clear votes and try again
                await userService.SetVoteAsync(aliveSlut.Id, null);
                await userService.SetVoteConfirmedAsync(aliveSlut.Id, false);
                return false;
            }

            // Set the final vote and clear individual vote
            room.slutFinalVote = aliveSlut.vote;

            // Clear votes and confirmations for next stage
            await userService.SetVoteAsync(aliveSlut.Id, null);
            await userService.SetVoteConfirmedAsync(aliveSlut.Id, false);

            return true;
        }

        public override async Task StageOutAsync(Room room)
        {
            // Logic for when the slut stage ends

            await room.AnnouncementMutex.WaitAsync();
            try
            {
                await GameHub.PushAnnouncementAsync(
                    announcementService.GetRandomAnnouncement(
                        AnnouncementType.slut_off,
                        room.announcementLanguage,
                        room.announcementPEGIRating),
                    room,
                    null!,
                    hubContext);
            }
            finally
            {
                room.AnnouncementMutex.Release();
            }

            await roomService.SetCurrentStageAsync(room.Id, CurrentStage.night);
            // Ensure all sluts have cleared votes (both alive and dead)
            var allSluts = room.users.Where(u => u.role == UserRole.slut).ToList();
            foreach (var slut in allSluts)
            {
                await userService.SetVoteAsync(slut.Id, null);
                await userService.SetVoteConfirmedAsync(slut.Id, false);
            }
        }
    }


    class SheriffStageProcessor(
UserService userService,
RoomService roomService,
IHubContext<GameHub> hubContext,
AnnouncementService announcementService) : IStageProcessor(userService, roomService, hubContext, announcementService)
    {
        public override async Task<bool> StageInAsync(Room room)
        {
            // Sheriff stage entering logic here

            await room.AnnouncementMutex.WaitAsync();
            try
            {
                await GameHub.PushAnnouncementAsync(
                    announcementService.GetRandomAnnouncement(
                        AnnouncementType.sheriff_on,
                        room.announcementLanguage,
                        room.announcementPEGIRating),
                    room,
                    null!,
                    hubContext);
            }
            finally
            {
                room.AnnouncementMutex.Release();
            }
            await roomService.SetCurrentStageAsync(room.Id, CurrentStage.sheriff);

            // Clear votes and confirmations for all sheriffs (both alive and dead)
            var allSheriffs = room.users.Where(u => u.role == UserRole.sheriff).ToList();
            foreach (var sheriff in allSheriffs)
            {
                await userService.SetVoteAsync(sheriff.Id, null);
                await userService.SetVoteConfirmedAsync(sheriff.Id, false);
            }

            // Check if we have any alive sheriffs
            var aliveSheriff = room.users.Where(u => u.role == UserRole.sheriff && u.status == UserStatus.alive).FirstOrDefault();
            if (aliveSheriff == null)
            {
                // No alive sheriff - skip this stage
                return true;
            }

            return false;
        }

        public override async Task<bool> OnVoteConfirmed(Room room)
        {
            // Sheriff stage logic here
            // Only ALIVE sheriff can vote and confirm
            var aliveSheriff = room.users.Where(u => u.role == UserRole.sheriff && u.status == UserStatus.alive).FirstOrDefault();

            if (aliveSheriff == null)
            {
                // Sheriff is dead - skip this stage
                return true;
            }

            if (!aliveSheriff.voteConfirmed)
            {
                return false; // Vote not confirmed
            }

            // On first night, no actual investigation happens
            if (room.isFirstNight)
            {
                // Clear votes and confirmations for next stage
                await userService.SetVoteAsync(aliveSheriff.Id, null);
                await userService.SetVoteConfirmedAsync(aliveSheriff.Id, false);
                return true;
            }

            // Check if target is valid
            if (aliveSheriff.vote == null)
            {
                return true; // No vote set
            }

            var target = userService.GetUser(aliveSheriff.vote);
            if (target != null && target.status == UserStatus.dead)
            {
                // Invalid target - clear votes and try again
                await userService.SetVoteAsync(aliveSheriff.Id, null);
                await userService.SetVoteConfirmedAsync(aliveSheriff.Id, false);
                return false;
            }

            // Check if sheriff was blocked by slut
            if (room.slutFinalVote == aliveSheriff.Id)
            {
                // Sheriff has been disabled
                await GameHub.PushTextAnnouncementAsync(TextAnnouncementType.sheriff_is_blocked, aliveSheriff, null!, hubContext);
            }
            else if(target != null)
            {
                // Sheriff can investigate
                var isCorrect = target.role == UserRole.mafia;

                await GameHub.PushTextAnnouncementAsync(
                    isCorrect ?
                        TextAnnouncementType.sheriff_is_correct :
                        TextAnnouncementType.sheriff_is_wrong,
                    aliveSheriff, target, hubContext);
            }

            // Finally reset sheriff and continue
            await userService.SetVoteAsync(aliveSheriff.Id, null);
            await userService.SetVoteConfirmedAsync(aliveSheriff.Id, false);

            return true;
        }

        public override async Task StageOutAsync(Room room)
        {
            // Logic for when the sheriff stage ends

            await room.AnnouncementMutex.WaitAsync();
            try
            {
                await GameHub.PushAnnouncementAsync(
                    announcementService.GetRandomAnnouncement(
                        AnnouncementType.sheriff_off,
                        room.announcementLanguage,
                        room.announcementPEGIRating),
                    room,
                    null!,
                    hubContext);
            }
            finally
            {
                room.AnnouncementMutex.Release();
            }
            await roomService.SetCurrentStageAsync(room.Id, CurrentStage.night);

            // Ensure all sheriffs have cleared votes (both alive and dead)
            var allSheriffs = room.users.Where(u => u.role == UserRole.sheriff).ToList();
            foreach (var sheriff in allSheriffs)
            {
                await userService.SetVoteAsync(sheriff.Id, null);
                await userService.SetVoteConfirmedAsync(sheriff.Id, false);
            }
        }
    }


    class DoctorStageProcessor(
UserService userService,
RoomService roomService,
IHubContext<GameHub> hubContext,
AnnouncementService announcementService) : IStageProcessor(userService, roomService, hubContext, announcementService)
    {
        public override async Task<bool> StageInAsync(Room room)
        {
            // Doctor stage entering logic here

            await room.AnnouncementMutex.WaitAsync();
            try
            {
                await GameHub.PushAnnouncementAsync(
                    announcementService.GetRandomAnnouncement(
                        AnnouncementType.doctor_on,
                        room.announcementLanguage,
                        room.announcementPEGIRating),
                    room,
                    null!,
                    hubContext);
            }
            finally
            {
                room.AnnouncementMutex.Release();
            }

            await roomService.SetCurrentStageAsync(room.Id, CurrentStage.doctor);
            // Clear votes and confirmations for all doctors (both alive and dead)
            var allDoctors = room.users.Where(u => u.role == UserRole.doctor).ToList();
            foreach (var doctor in allDoctors)
            {
                await userService.SetVoteAsync(doctor.Id, null);
                await userService.SetVoteConfirmedAsync(doctor.Id, false);
            }

            // Check if we have any alive doctors
            var aliveDoctor = room.users.Where(u => u.role == UserRole.doctor && u.status == UserStatus.alive).FirstOrDefault();
            if (aliveDoctor == null)
            {
                // No alive doctor - skip this stage
                return true;
            }

            return false;
        }

        public override async Task<bool> OnVoteConfirmed(Room room)
        {
            // Doctor stage logic here
            // Only ALIVE doctor can vote and confirm
            var aliveDoctor = room.users.Where(u => u.role == UserRole.doctor && u.status == UserStatus.alive).FirstOrDefault();

            if (aliveDoctor == null)
            {
                // Doctor is dead - skip this stage
                return true;
            }

            if (!aliveDoctor.voteConfirmed)
            {
                return false; // Vote not confirmed
            }

            // On first night, no actual healing happens
            if (room.isFirstNight)
            {
                // Clear votes and confirmations for next stage
                await userService.SetVoteAsync(aliveDoctor.Id, null);
                await userService.SetVoteConfirmedAsync(aliveDoctor.Id, false);
                return true;
            }

            // Check if target is valid (not dead)
            if (aliveDoctor.vote == null)
            {
                return true; // No vote set
            }

            var target = userService.GetUser(aliveDoctor.vote);
            if (target == null || target.status == UserStatus.dead)
            {
                // Invalid target - clear votes and try again
                await userService.SetVoteAsync(aliveDoctor.Id, null);
                await userService.SetVoteConfirmedAsync(aliveDoctor.Id, false);
                return false;
            }

            // Set the final vote and clear individual vote
            room.doctorFinalVote = aliveDoctor.vote;
            await GameHub.PushRoomUpdateAsync(room, hubContext);

            // Finally reset doctor and continue
            await userService.SetVoteAsync(aliveDoctor.Id, null);
            await userService.SetVoteConfirmedAsync(aliveDoctor.Id, false);

            return true;
        }

        public override async Task StageOutAsync(Room room)
        {
            // Logic for when the doctor stage ends

            await room.AnnouncementMutex.WaitAsync();
            try
            {
                await GameHub.PushAnnouncementAsync(
                    announcementService.GetRandomAnnouncement(
                        AnnouncementType.doctor_off,
                        room.announcementLanguage,
                        room.announcementPEGIRating),
                    room,
                    null!,
                    hubContext);
            }
            finally
            {
                room.AnnouncementMutex.Release();
            }
            await roomService.SetCurrentStageAsync(room.Id, CurrentStage.night);

            // Ensure all doctors have cleared votes (both alive and dead)
            var allDoctors = room.users.Where(u => u.role == UserRole.doctor).ToList();
            foreach (var doctor in allDoctors)
            {
                await userService.SetVoteAsync(doctor.Id, null);
                await userService.SetVoteConfirmedAsync(doctor.Id, false);
            }
        }
    }

    class DayStageProcessor(
UserService userService,
RoomService roomService,
IHubContext<GameHub> hubContext,
AnnouncementService announcementService) : IStageProcessor(userService, roomService, hubContext, announcementService)
    {
        public override async Task<bool> StageInAsync(Room room)
        {
            // Day stage entering logic here

            await room.AnnouncementMutex.WaitAsync();
            try
            {
                await GameHub.PushAnnouncementAsync(
                    announcementService.GetRandomAnnouncement(
                        AnnouncementType.city_up,
                        room.announcementLanguage,
                        room.announcementPEGIRating),
                    room,
                    null!,
                    hubContext);
            }
            finally
            {
                room.AnnouncementMutex.Release();
            }
            await roomService.SetCurrentStageAsync(room.Id, CurrentStage.day);

            // Process night actions
            var mafiaKilled = userService.GetUser(room.mafiaFinalVote);
            var doctorProtected = userService.GetUser(room.doctorFinalVote);
            var slutBlocked = userService.GetUser(room.slutFinalVote);

            // Handle slut blocking effects
            if (slutBlocked != null && slutBlocked.role == UserRole.doctor)
            {
                doctorProtected = null; // Slut blocked doctor
            }

            if (slutBlocked != null && slutBlocked.role == UserRole.mafia)
            {
                mafiaKilled = null; // Slut blocked mafia
            }

            // If slut blocked sheriff, we already dealt with it in sheriff stage

            // Handle doctor healing
            if (doctorProtected != null && mafiaKilled != null && doctorProtected.Id == mafiaKilled.Id)
            {
                // Doctor successfully healed
                mafiaKilled = null;
            }

            // Kill the target if mafia wasn't blocked
            if (mafiaKilled != null)
            {
                await room.AnnouncementMutex.WaitAsync();
                try
                {
                    await GameHub.PushAnnouncementAsync(
                        announcementService.GetRandomAnnouncement(
                            AnnouncementType.player_killed,
                            room.announcementLanguage,
                            room.announcementPEGIRating),
                        room,
                        mafiaKilled,
                        hubContext);
                }
                finally
                {
                    room.AnnouncementMutex.Release();
                }

                // Kill user
                await userService.SetStatusAsync(mafiaKilled.Id, UserStatus.dead);
            }

            // Clear all night stage final votes
            await roomService.SetSlutFinalVoteAsync(room.Id, null);
            await roomService.SetDoctorFinalVoteAsync(room.Id, null);
            await roomService.SetMafiaFinalVoteAsync(room.Id, null);

            // Clear votes and confirmations for ALL users (both alive and dead)
            var allUsers = room.users.ToList();
            foreach (var user in allUsers)
            {
                await userService.SetVoteAsync(user.Id, null);
                await userService.SetVoteConfirmedAsync(user.Id, false);
            }

            return false;
        }

        public override async Task<bool> OnVoteConfirmed(Room room)
        {
            // Day stage logic here
            // All ALIVE users must confirm on one vote
            var aliveUsers = room.users.Where(u => u.status == UserStatus.alive).ToList();

            if (aliveUsers.Count == 0)
            {
                // No alive users - game should end
                return true;
            }
            var allConfirmed = aliveUsers.All(u => u.voteConfirmed);
            if (!allConfirmed)
            {
                return false; // Not all votes are confirmed
            }
            // Handle first night transition
            if (room.isFirstNight)
            {
                await roomService.SetIsFirstNightAsync(room.Id, false);
                return true; // No votes on first night
            }

            // Check if all alive users have confirmed their votes


            // Find the most popular vote
            var mostPopularVote = aliveUsers.Where(u => u.vote != null)
                .GroupBy(u => u.vote)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault();

            var secondPopularVote = aliveUsers.Where(u => u.vote != null)
                .GroupBy(u => u.vote)
                .OrderByDescending(g => g.Count())
                .Skip(1)
                .FirstOrDefault();

            // Check for tie
            if (secondPopularVote != null && secondPopularVote.Count() == mostPopularVote!.Count())
            {
                mostPopularVote = null; // Tie - no one gets killed
            }

            // Execute the vote if there's a clear winner
            if (mostPopularVote != null)
            {
                var targetUser = userService.GetUser(mostPopularVote.Key!);
                if (targetUser != null)
                {

                    await userService.SetStatusAsync(targetUser.Id, UserStatus.dead);
                    await room.AnnouncementMutex.WaitAsync();
                    try
                    {
                        await GameHub.PushAnnouncementAsync(
                            announcementService.GetRandomAnnouncement(
                                AnnouncementType.player_killed,
                                room.announcementLanguage,
                                room.announcementPEGIRating),
                            room,
                            targetUser,
                            hubContext);
                    }
                    finally
                    {
                        room.AnnouncementMutex.Release();
                    }
                }
            }

            // Clear votes and confirmations for ALL users (both alive and dead)
            var allUsers = room.users.ToList();
            foreach (var user in allUsers)
            {
                await userService.SetVoteAsync(user.Id, null);
                await userService.SetVoteConfirmedAsync(user.Id, false);
            }

            // Check if game should end after this vote execution
            var aliveMafias = room.users.Where(u => u.role == UserRole.mafia && u.status == UserStatus.alive).Count();
            var alivePeacefuls = room.users.Where(u => u.role != UserRole.mafia && u.status == UserStatus.alive).Count();
            
            // If all mafias are dead, civilians win - don't transition to next stage
            if (aliveMafias == 0)
            {
                return false; // Let ReEvaluateGame handle the game end
            }
            
            // If mafias can win by vote (more mafias than peacefuls), don't transition
            if (alivePeacefuls < aliveMafias)
            {
                return false; // Let ReEvaluateGame handle the game end
            }

            return true; // Go to next stage
        }

        public override async Task StageOutAsync(Room room)
        {
            // Logic for when the day stage ends
            await roomService.SetCurrentStageAsync(room.Id, CurrentStage.night);

            await room.AnnouncementMutex.WaitAsync();
            try
            {
                await GameHub.PushAnnouncementAsync(
                    announcementService.GetRandomAnnouncement(
                        AnnouncementType.city_down,
                        room.announcementLanguage,
                        room.announcementPEGIRating),
                    room,
                    null!,
                    hubContext);
            }
            finally
            {
                room.AnnouncementMutex.Release();
            }

            // Ensure all users have cleared votes (both alive and dead)
            var allUsers = room.users.ToList();
            foreach (var user in allUsers)
            {
                await userService.SetVoteAsync(user.Id, null);
                await userService.SetVoteConfirmedAsync(user.Id, false);
            }
        }
    }

    public class GameService
    {

        private Dictionary<CurrentStage, IStageProcessor> gameStageProcessors = new();

        UserService userService;
        RoomService roomService;
        IHubContext<GameHub> hubContext;
        AnnouncementService announcementService;

        public GameService(UserService UserService, RoomService RoomService, IHubContext<GameHub> HubContext, AnnouncementService AnnouncementService)
        {
            userService = UserService;
            roomService = RoomService;
            hubContext = HubContext;
            announcementService = AnnouncementService;

            gameStageProcessors[CurrentStage.mafia] = new MafiaStageProcessor(userService, roomService, hubContext, announcementService);
            gameStageProcessors[CurrentStage.slut] = new SlutStageProcessor(userService, roomService, hubContext, announcementService);
            gameStageProcessors[CurrentStage.sheriff] = new SheriffStageProcessor(userService, roomService, hubContext, announcementService);
            gameStageProcessors[CurrentStage.doctor] = new DoctorStageProcessor(userService, roomService, hubContext, announcementService);
            gameStageProcessors[CurrentStage.day] = new DayStageProcessor(userService, roomService, hubContext, announcementService);

        }

        public async Task StartGame(string roomId)
        {
            var room = roomService.GetRoom(roomId);
            if (room == null)
                return;

            if (room.currentStage != CurrentStage.lobby)
                return;

            if (!RoomService.CanStartGame(roomId))
                return;

            Console.WriteLine(room.currentStage);

            await AssignRoles(roomId);

            await roomService.SetCurrentStageAsync(roomId, CurrentStage.night);

            await room.AnnouncementMutex.WaitAsync();
            try
            {
                await GameHub.PushAnnouncementAsync(
                    announcementService.GetRandomAnnouncement(
                        AnnouncementType.game_start,
                        room.announcementLanguage,
                        room.announcementPEGIRating),
                    room,
                    null!,
                    hubContext);

                await GameHub.PushAnnouncementAsync(
                    announcementService.GetRandomAnnouncement(
                        AnnouncementType.city_down,
                        room.announcementLanguage,
                        room.announcementPEGIRating),
                    room,
                    null!,
                    hubContext);
            }
            finally
            {
                room.AnnouncementMutex.Release();
            }

            gameStageProcessors.TryGetValue(CurrentStage.mafia, out var stageProcessor);

            if (stageProcessor != null)
                await stageProcessor.StageInAsync(room);
        }

        public async Task onVoteConfirmed(string roomId)
        {
            var room = roomService.GetRoom(roomId);
            if (room == null)
                return;

            if (room.currentStage == CurrentStage.lobby || room.currentStage == CurrentStage.endGame || room.currentStage == CurrentStage.notEnoughPlayers)
                return; // Game not started yet or already ended

            if (gameStageProcessors.TryGetValue(room.currentStage, out var stageProcessor))
            {
                var canContinue = await stageProcessor.OnVoteConfirmed(room);
                if (canContinue)
                {
                    var nextStage = getNextGameStage(room);
                    await stageProcessor.StageOutAsync(room);

                    // Re-evaluate game state after stage completion
                    if (!await ReEvaluateGame(roomId))
                    {
                        if (gameStageProcessors.TryGetValue(nextStage, out var nextStageProcessor))
                        {
                            // Try to enter the next stage
                            var stageEntered = await nextStageProcessor.StageInAsync(room);
                            
                            if (stageEntered)
                            {
                                // Stage was skipped (e.g., no alive players of that role)
                                // Continue to the next stage automatically
                                await Task.Delay(TimeSpan.FromSeconds(Random.Shared.Next(3, 15)));
                                await onVoteConfirmed(roomId); // Continue to next stage
                            }
                            // If stageEntered is false, the stage is active and waiting for votes
                        }
                    }
                }
                else
                {
                    // Stage indicates game should end (e.g., day stage after civilian win)
                    // Re-evaluate game state to determine winner and end game
                    await ReEvaluateGame(roomId);
                }
            }
        }


        public async Task<Room> AssignRoles(string roomId)
        {
            var room = roomService.GetRoom(roomId);
            if (room == null || room.currentStage != CurrentStage.lobby || !RoomService.CanStartGame(roomId))
                return null!;

            var rolesToAssign = new Stack<UserRole>();

            for (int i = 0; i < room.mafiaCount; i++)
                rolesToAssign.Push(UserRole.mafia);

            if (room.doctorEnabled)
                rolesToAssign.Push(UserRole.doctor);

            if (room.sheriffEnabled)
                rolesToAssign.Push(UserRole.sheriff);

            if (room.slutEnabled)
                rolesToAssign.Push(UserRole.slut);

            while (rolesToAssign.Count < room.users.Count())
                rolesToAssign.Push(UserRole.peaceful);

            var users = room.users.Select(u => u.Id).ToList();

            while (users.Count > 0)
            {
                var index = Random.Shared.Next(0, users.Count);
                await userService.SetRoleAsync(users[index], rolesToAssign.Pop(), true);
                users.RemoveAt(index);
            }

            await GameHub.PushRoomUpdateAsync(room, hubContext);

            return room;
        }

        public async Task<bool> ReEvaluateGame(string roomId)
        {
            // Checks that game can continue when player leaves or gets killed
            var room = roomService.GetRoom(roomId);

            if (room == null)
                return false;

            // Check if we still have enough players to continue
            if (RoomService.CanStartGame(roomId) == false)
            {
                await roomService.SetCurrentStageAsync(roomId, CurrentStage.notEnoughPlayers);
                await room.AnnouncementMutex.WaitAsync();
                try
                {
                    await GameHub.PushAnnouncementAsync(
                        announcementService.GetRandomAnnouncement(
                            AnnouncementType.not_enough_players,
                            room.announcementLanguage,
                            room.announcementPEGIRating),
                        room,
                        null!,
                        hubContext);
                }
                finally
                {
                    room.AnnouncementMutex.Release();
                }
                return true;
            }

            // Check if all mafias are dead
            var aliveMafias = room.users.Where(u => u.role == UserRole.mafia && u.status == UserStatus.alive).Count();
            if (aliveMafias == 0)
            {
                // No mafia left, peaceful win
                await roomService.SetCurrentStageAsync(roomId, CurrentStage.endGame);
                await room.AnnouncementMutex.WaitAsync();
                try
                {
                    await GameHub.PushAnnouncementAsync(
                        announcementService.GetRandomAnnouncement(
                            AnnouncementType.peaceful_win,
                            room.announcementLanguage,
                            room.announcementPEGIRating),
                        room,
                        null!,
                        hubContext);
                }
                finally
                {
                    room.AnnouncementMutex.Release();
                }
                return true;
            }

            // Check if mafia can win by vote (more mafias than peacefuls)
            var alivePeacefuls = room.users.Where(u => u.role != UserRole.mafia && u.status == UserStatus.alive).Count();
            if (alivePeacefuls < aliveMafias)
            {
                // No peaceful left, mafia win
                await roomService.SetCurrentStageAsync(roomId, CurrentStage.endGame);
                await room.AnnouncementMutex.WaitAsync();
                try
                {
                    await GameHub.PushAnnouncementAsync(
                        announcementService.GetRandomAnnouncement(
                            AnnouncementType.mafia_win,
                            room.announcementLanguage,
                            room.announcementPEGIRating),
                        room,
                        null!,
                        hubContext);
                }
                finally
                {
                    room.AnnouncementMutex.Release();
                }
                return true;
            }

            // Game can continue
            return false;
        }

        public CurrentStage getNextGameStage(Room room)
        {

            var enabledRoles = new List<CurrentStage>();

            if (room.currentStage == CurrentStage.day) //day always goes to mafia
                return CurrentStage.mafia;

            if (room.currentStage == CurrentStage.mafia) //goes to next enabled role slut -> sheriff -> doctor 
            {
                if (room.isFirstNight)
                    return CurrentStage.day;

                //goes to next available stage in order slut, sheriff, doctor
                if (room.slutEnabled)
                    return CurrentStage.slut;

                else if (room.sheriffEnabled)
                    return CurrentStage.sheriff;

                else if (room.doctorEnabled)
                    return CurrentStage.doctor;
                else
                    return CurrentStage.day;
            }

            //goes to next enabled role sheriff -> doctor 
            if (room.currentStage == CurrentStage.slut)
            {
                //goes to next available stage or straight to day since slut is already worked
                if (room.sheriffEnabled)
                    return CurrentStage.sheriff;
                else if (room.doctorEnabled)
                    return CurrentStage.doctor;
                else
                    return CurrentStage.day;
            }

            //goes to next enabled role sheriff -> doctor 
            if (room.currentStage == CurrentStage.sheriff)
            {
                //goes to next available stage or straight to day since slut is already worked
                if (room.doctorEnabled)
                    return CurrentStage.doctor;
                else
                    return CurrentStage.day;
            }

            if (room.currentStage == CurrentStage.doctor)
            {
                return CurrentStage.day;
            }



            return room.currentStage;
        }








    }
}
