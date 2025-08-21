using _Mafia_API.Models;
using System.Collections.Concurrent;

namespace _Mafia_API.Repositories
{
    public class RoomRepository
    {

        private static readonly ConcurrentDictionary<string, Room> rooms = new();

        public static bool DeleteRoom(string id)
        {
            return rooms.TryRemove(id, out var room);
        }
        public static Room NewRoom(string roomOwnerId)
        {
            var room = new Room
            {
                roomOwnerId = roomOwnerId
            };
            rooms[room.Id] = room;
            return room;
        }

        public static Room? GetRoomById(string? id)
        {

            return id == null ? null : rooms.TryGetValue(id, out var room) ? room : null;
        }

        public static Room? GetRoomByCode(string? code)
        {
            return code == null ? null : rooms.Values.FirstOrDefault(r => r.roomCode == code);
        }

        public static IEnumerable<Room> GetAllRooms()
        {
            return rooms.Values;
        }

    }
}
