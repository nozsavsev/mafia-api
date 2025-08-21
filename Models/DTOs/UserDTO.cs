using _Mafia_API.Models;
using System.Text.Json.Serialization;
using Macross.Json.Extensions;

namespace _Mafia_API.Models.DTOs
{
    public class UserDTO
    {
        public string Id { get; set; } = string.Empty;
        public string? currentRoomId { get; set; } = null;
        public RoomDTO? room { get; set; } = null;
        public string? fullName { get; set; } = null;
        
        public UserRole? role { get; set; } = UserRole.peaceful;
        public UserStatus status { get; set; } = UserStatus.alive;
        public string? vote { get; set; } = null;
        public bool voteConfirmed { get; set; } = false;
        public bool? isGameMaster { get; set; } = false;

        public static UserDTO FromUser(User user, bool skipRoom = false)
        {
            return new UserDTO
            {
                Id = user.Id,
                currentRoomId = user.currentRoomId,
                fullName = user.fullName,
                role = user.role,
                status = user.status,
                vote = user.vote,
                voteConfirmed = user.voteConfirmed,
                isGameMaster = user.isGameMaster,
                room = skipRoom ? null : RoomDTO.FromRoom(user.Room)

            };
        }
    }
}
