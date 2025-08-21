using _Mafia_API.Models;
using System.Collections.Concurrent;

namespace _Mafia_API.Repositories
{
    public class UserRepository
    {


        private static readonly ConcurrentDictionary<string, User> users = new();

        public static bool DeleteUser(string id)
        {
            return users.TryRemove(id, out _);
        }

        public static User NewUser()
        {
            var user = new User();
            users[user.Id] = user;
            return user;
        }

        public static User? GetUserById(string? id)
        {
            return id == null ? null : users.TryGetValue(id, out var user) ? user : null;
        }

        public static IEnumerable<User> GetAllUsers()
        {
            return users.Values;
        }


    }
}
