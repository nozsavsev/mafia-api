using _Mafia_API.Helpers;
using _Mafia_API.Hubs;
using _Mafia_API.Models;
using Microsoft.AspNetCore.SignalR;

namespace _Mafia_API.Services
{
    public class GameService(UserService userService, RoomService roomService, IHubContext<GameHub> hubContext)
    {

        class UPNV
        {
            public string uid;
            public int votes = 0;
        }


        public void continueGame(string roomCode)
        {
            var room = roomService.GetRoom(roomCode);
            var users = userService.GetUsersOfRoom(roomCode).Where(u => u.status == UserStatus.alive);

            bool cityJustWokeUp = false;



            if (room.state == CurrentState.day)
            {

                if (users.Select(e => e.positiveVote).Count() > 0 || users.Select(e => e.negativeVote).Count() > 0)
                {

                    List<UPNV> vts = new List<UPNV>();

                    foreach (var user in users)
                    {

                        UPNV vs = new UPNV();

                        vs.uid = user.id;

                        users.Where(u => u.positiveVote == user.id).ToList().ForEach(u => vs.votes -= 1);
                        users.Where(u => u.negativeVote == user.id).ToList().ForEach(u => vs.votes += 1);

                        vts.Add(vs);

                        user.positiveVote = null;
                        user.negativeVote = null;
                        userService.UpdateUser(user);
                    }


                    vts = vts.OrderByDescending(e => e.votes).ToList();


                    if (vts[0].votes > 0)
                    {
                        var user = users.FirstOrDefault(u => u.id == vts[0].uid);
                        if (user != null)
                            user.status = UserStatus.dead;
                        userService.UpdateUser(user);

                        GameHub.PushAnounencment(hubContext, VoiceHelper.AnnouncementType.vote_killed, roomCode, user.id);
                        Thread.Sleep(5000);
                    }
                    else
                    {
                        ;
                    }

                    roomService.UpdateRoom(room);
                    
                }

                room.state = CurrentState.allSleeping;
                roomService.UpdateRoom(room);

                GameHub.PushAnounencment(hubContext, VoiceHelper.AnnouncementType.city_down, roomCode, null);
                Thread.Sleep(5000);
            }


            if (room.state == CurrentState.allSleeping)
            {
                if (room.slutEnabled)
                {
                    room.state = CurrentState.slut;
                    roomService.UpdateRoom(room);
                    GameHub.PushAnounencment(hubContext, VoiceHelper.AnnouncementType.slut_on, roomCode, null);
                }
                else
                {
                    room.state = CurrentState.mafia;
                    roomService.UpdateRoom(room);
                    GameHub.PushAnounencment(hubContext, VoiceHelper.AnnouncementType.mafia_on, roomCode, null);
                }

                return; 
            }

            if (room.state == CurrentState.slut)
            {


                room.state = CurrentState.allSleeping;
                roomService.UpdateRoom(room);
                GameHub.PushAnounencment(hubContext, VoiceHelper.AnnouncementType.slut_off, roomCode, null);
                Thread.Sleep(5000);

                room.state = CurrentState.mafia;
                roomService.UpdateRoom(room);
                GameHub.PushAnounencment(hubContext, VoiceHelper.AnnouncementType.mafia_on, roomCode, null);
               
                if (!cityJustWokeUp)
                    return;
            }

            if (room.state == CurrentState.mafia)
            {

                

                room.state = CurrentState.allSleeping;
                roomService.UpdateRoom(room);
                GameHub.PushAnounencment(hubContext, VoiceHelper.AnnouncementType.mafia_off, roomCode, null);
                Thread.Sleep(5000);


                if (room.sherifEnabled)
                {
                    room.state = CurrentState.sherif;
                    roomService.UpdateRoom(room);
                    GameHub.PushAnounencment(hubContext, VoiceHelper.AnnouncementType.sherif_on, roomCode, null);
                }
                else if (room.doctorEnabled)
                {
                    room.state = CurrentState.doctor;
                    roomService.UpdateRoom(room);
                    GameHub.PushAnounencment(hubContext, VoiceHelper.AnnouncementType.doctor_on, roomCode, null);
                }
                else
                {
                    room.state = CurrentState.day;
                    roomService.UpdateRoom(room);
                    GameHub.PushAnounencment(hubContext, VoiceHelper.AnnouncementType.city_up, roomCode, null);
                    Thread.Sleep(5000);
                    cityJustWokeUp = true;
                }

                if (!cityJustWokeUp)
                    return;
               
            }

            if (room.state == CurrentState.sherif)
            {
                room.state = CurrentState.allSleeping;
                roomService.UpdateRoom(room);
                GameHub.PushAnounencment(hubContext, VoiceHelper.AnnouncementType.sherif_off, roomCode, null);
                Thread.Sleep(5000);

                if (room.doctorEnabled)
                {
                    room.state = CurrentState.doctor;
                    roomService.UpdateRoom(room);
                    GameHub.PushAnounencment(hubContext, VoiceHelper.AnnouncementType.doctor_on, roomCode, null);
                }
                else
                {
                    room.state = CurrentState.day;
                    roomService.UpdateRoom(room);
                    GameHub.PushAnounencment(hubContext, VoiceHelper.AnnouncementType.city_up, roomCode, null);
                    Thread.Sleep(5000);
                    cityJustWokeUp = true;
                }

                if (!cityJustWokeUp)
                    return;
            }

            if (room.state == CurrentState.doctor)
            {

               

                room.state = CurrentState.allSleeping;
                roomService.UpdateRoom(room);
                GameHub.PushAnounencment(hubContext, VoiceHelper.AnnouncementType.doctor_off, roomCode, null);
                Thread.Sleep(5000);

                room.state = CurrentState.day;
                roomService.UpdateRoom(room);
                GameHub.PushAnounencment(hubContext, VoiceHelper.AnnouncementType.city_up, roomCode, null);
                Thread.Sleep(5000);
                cityJustWokeUp = true;

                if (!cityJustWokeUp)
                    return;

            }


            if (cityJustWokeUp)
            {
                cityJustWokeUp = false;

                string? slutVote = users.Where(u => u.role == UserRole.slut).Select(u => u.slutVote).FirstOrDefault();
                string? mafiaVote = users.Where(u => u.role == UserRole.mafia).Select(u => u.mafiaVote).FirstOrDefault();
                string? doctorVote = users.Where(u => u.role == UserRole.doctor).Select(u => u.doctorVote).FirstOrDefault();

                users.Where(u => u.role == UserRole.doctor).ToList().ForEach(u => { u.doctorVote = null; userService.UpdateUser(u); });
                users.Where(u => u.role == UserRole.mafia).ToList().ForEach(u => { u.mafiaVote = null; userService.UpdateUser(u); });
                users.Where(u => u.role == UserRole.mafia).ToList().ForEach(u => { u.slutVote = null; userService.UpdateUser(u); });

                if (room.slutEnabled)
                {
                    if (users.Where(u => u.id == slutVote).FirstOrDefault()?.role == UserRole.mafia)
                    {
                        mafiaVote = null;
                    }
                    else if (users.Where(u => u.id == slutVote).FirstOrDefault()?.role == UserRole.doctor)
                    {
                        doctorVote = null;
                    }
                }

                if (room.doctorEnabled)
                    if (doctorVote == mafiaVote)
                        mafiaVote = null;

                if (mafiaVote != null)
                {
                    var deadUser = users.Where(u => u.id == mafiaVote).FirstOrDefault();

                    if (deadUser != null)
                    {
                        deadUser.status = UserStatus.dead;
                        userService.UpdateUser(deadUser);
                        GameHub.PushAnounencment(hubContext, VoiceHelper.AnnouncementType.player_killed, roomCode, deadUser.id);
                    }
                }
            }
        }

        public void startGame(string roomCode)
        {
            var room = roomService.GetRoom(roomCode);
            var usersToUpdate = userService.GetUsersOfRoom(roomCode);


            var mafias = room.mafiaCount;
            var doctors = room.doctorEnabled ? 1 : 0;
            var sherifs = room.sherifEnabled ? 1 : 0;
            var sluts = room.slutEnabled ? 1 : 0;
            var civilians = usersToUpdate.Count - mafias - doctors - sherifs - sluts;

            if (civilians <= 0) return;

            List<UserRole> roles = new List<UserRole>();

            roles.AddRange(new List<UserRole>(Enumerable.Repeat(UserRole.mafia, mafias)));
            roles.AddRange(new List<UserRole>(Enumerable.Repeat(UserRole.doctor, doctors)));
            roles.AddRange(new List<UserRole>(Enumerable.Repeat(UserRole.sherif, sherifs)));
            roles.AddRange(new List<UserRole>(Enumerable.Repeat(UserRole.slut, sluts)));
            roles.AddRange(new List<UserRole>(Enumerable.Repeat(UserRole.peacfull, civilians)));

            while (roles.Count > 0)
            {
                var rand = new Random();
                int rIndex = rand.Next(0, roles.Count);

                var user = usersToUpdate[0];
                usersToUpdate.RemoveAt(0);

                user.role = roles[rIndex];
                roles.RemoveAt(rIndex);

                user.positiveVote = null;
                user.negativeVote = null;
                user.mafiaVote = null;
                user.slutVote = null;
                user.sherifVote = null;
                user.doctorVote = null;

                user.status = UserStatus.alive;

                userService.UpdateUser(user);
            }

            room.state = CurrentState.day;
            room.nightCount = 0;

            roomService.UpdateRoom(room);
        }
    }
}
