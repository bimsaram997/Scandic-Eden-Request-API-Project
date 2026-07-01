using Microsoft.AspNetCore.SignalR;

namespace EdenRequest.Api.Hubs
{
    public class NotificationHub: Hub
    {
        public async Task JoinUserByRole(string email, string role)
        {
            if (string.IsNullOrEmpty(email)) return;

            string cleanEmailGroup = $"User_{email.Replace("@", "_").Replace(".", "_")}";
            await Groups.AddToGroupAsync(Context.ConnectionId, cleanEmailGroup);

            if (role == "Leader" || role == "TeamLeader")
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "ActiveLeadersDashboard");
            }
        }
    }
}
