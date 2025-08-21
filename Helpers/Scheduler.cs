using _Mafia_API.Hubs;
using _Mafia_API.Models;
using _Mafia_API.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace _Mafia_API.Helpers
{
    public class Scheduler(IServiceProvider serviceProvider)
    {
        public void ScheduleAnnouncement(TimeSpan delay, Announcement announcement, Room room, User user)
        {
            // Create the scope here, before the Task.Run
            var scope = serviceProvider.CreateScope();

            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(delay);

                    var userService = scope.ServiceProvider.GetRequiredService<AnnouncementService>();
                    var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<GameHub>>();

                    await GameHub.PushAnnouncementAsync(announcement, room, user, hubContext);
                }
                finally
                {
                    // Dispose the scope when done
                    scope.Dispose();
                }
            });
        }





        public void Detached(Func<User?, Room?, GameService, Task> task, HubCallerContext context, TimeSpan? delay = null)
        {
            // Create the scope here, before the Task.Run
            var scope = serviceProvider.CreateScope();
            var userId = context.GetHttpContext()?.MafiaUser()?.Id;
            var roomId = context.GetHttpContext()?.MafiaRoom()?.Id;

            Task.Run(async () =>
            {
                try
                {
                    if (delay != null)
                        await Task.Delay(delay.Value);

                    var gameService = scope.ServiceProvider.GetRequiredService<GameService>();
                    var userService = scope.ServiceProvider.GetRequiredService<UserService>();
                    var roomService = scope.ServiceProvider.GetRequiredService<RoomService>();

                    var user = userService.GetUser(userId);
                    var room = roomService.GetRoom(roomId);

                    await task(user, room, gameService);

                }
                finally
                {
                    // Dispose the scope when done
                    scope.Dispose();
                }
            });
        }






    }
}
