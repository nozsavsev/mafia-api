using _Mafia_API.Hubs;
using _Mafia_API.Models;
using Microsoft.AspNetCore.SignalR;

namespace _Mafia_API.Services
{
    public class RoomService
    {
        private static List<Room> RoomStore = new List<Room>();
        IHubContext<GameHub> hubContext;
        UserService userService;

        public RoomService(IHubContext<GameHub> hubContext, UserService UserService)
        {
            this.hubContext = hubContext;
            this.userService = UserService;
        }

        public Room createNewRoom()
        {
            Room room = new Room();
            RoomStore.Add(room);
            return room;
        }

        public List<Room> GetRooms()
        {
            return RoomStore;
        }

        public Room? GetRoom(string? IdOrCode)
        {
            return RoomStore.Find(x => x?.Id == IdOrCode || x?.roomCode == IdOrCode);
        }

        public Room? UpdateRoom(Room Room)
        {
            var room = RoomStore.Find(x => x.Id == Room.Id);

            if (room != null)
            {
                RoomStore.RemoveAll(x => x.Id == Room.Id);
                RoomStore.Add(Room);
                var users = userService.GetUsersOfRoom(Room.roomCode);
                GameHub.PushRoomUpdate(hubContext, Room, users);
            }


            return room;
        }

        public static List<Room> _GetRooms()
        {
            return RoomStore;
        }


    }
}
