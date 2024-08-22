
using _Mafia_API.Helpers;
using _Mafia_API.Hubs;
using _Mafia_API.Models;
using Microsoft.AspNetCore.SignalR;

namespace _Mafia_API.Services
{
    public class UserService
    {
        IHubContext<GameHub> hubContext;

        public static List<User> UserStore = new List<User>();

        public UserService(IHubContext<GameHub> hubContext)
        {
            this.hubContext = hubContext;
        }

        public List<User> GetUsers()
        {
            return UserStore;
        }

        public User? GetUser(string id)
        {
            var user = UserStore.Find(x => x.id == id);

            if (user != null && !File.Exists(Path.Combine("data", id)))
                if (user.nameConfirmed == true)
                    VoiceHelper.GenerateText(user.fullName, user.id);

            return user;
        }

        public void DeleteUser(string id)
        {
            var user = UserStore.Find(x => x.id == id);

            if (File.Exists(Path.Combine("data", id)))
                File.Delete(Path.Combine("data", id));

            if (user != null)
            {
                UserStore.RemoveAll(x => x.id == id);

                if (user.isGameMaster == true)
                {
                    GetUsersOfRoom(user.currentRoom).ForEach(x =>
                    {
                        x.currentRoom = null;
                        UpdateUser(x);
                    });
                }
                else
                    GameHub.PushRoomUpdate(hubContext, RoomService._GetRooms().Find(x => x.roomCode == user.currentRoom), GetUsersOfRoom(user.currentRoom));


            }
        }

        public void KickUser(string id)
        {
            var user = GetUser(id);

            if (user != null)
            {
                var userRoom = user.currentRoom;
                user.currentRoom = null;
                UpdateUser(user);

                GameHub.PushRoomUpdate(hubContext, RoomService._GetRooms().Find(x => x.roomCode == userRoom), GetUsersOfRoom(userRoom));

            }
        }

        public static User? st_GetUser(string id)
        {
            var user = UserStore.Find(x => x.id == id);

            if (user != null && !File.Exists(Path.Combine("data", id)))
                if (user.nameConfirmed == true)
                    VoiceHelper.GenerateText(user.fullName, user.id);

            return user;
        }

        public User? UpdateUser(User? user)
        {

            var original = GetUser(user.id);

            if ((original?.nameConfirmed == false && user?.nameConfirmed == true) || (original?.fullName != user?.fullName))
            {
                VoiceHelper.GenerateText(user.fullName, user.id);
            }


            if (user == null)
            {
                return null;
            }

            UserStore.RemoveAll(x => x.id == user.id);
            UserStore.Add(user);

            GameHub.PushUserUpdate(hubContext, user);
          
            if (user.currentRoom != null)
                GameHub.PushRoomUpdate(hubContext, RoomService._GetRooms().Find(x => x.roomCode == user.currentRoom), GetUsersOfRoom(user.currentRoom));

            return user;
        }

        public List<User>? GetUsersOfRoom(string roomCode)
        {
            var users = UserStore.FindAll(x => x.currentRoom == roomCode);

            return users;
        }

        public User? GetNewUser()
        {
            var user = new User();
            UserStore.Add(user);
            return user;
        }
    }
}
