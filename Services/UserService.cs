
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

        public void RemoveUser(string id)
        {
            UserStore.RemoveAll(x => x.id == id);
        }

        public List<User> GetUsers()
        {
            return UserStore;
        }

        public User? GetUser(string id)
        {
            return UserStore.Find(x => x.id == id);
        }

        public User? UpdateUser(User? user)
        {


            if (user == null)
            {
                return null;
            }

            UserStore.RemoveAll(x => x.id == user.id);
            UserStore.Add(user);

            GameHub.PushUserUpdate(hubContext, user);

            return user;
        }

        public User? GetNewUser()
        {
            var user = new User();
            UserStore.Add(user);
            return user;
        }
    }
}
