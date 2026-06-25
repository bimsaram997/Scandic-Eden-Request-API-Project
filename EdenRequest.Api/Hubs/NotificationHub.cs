using Microsoft.AspNetCore.SignalR;

namespace EdenRequest.Api.Hubs
{
    public class NotificationHub: Hub
    {
        // Dynamic group assignment based on database Employee Id
        public async Task JoinEmployeeChannel(string employeeId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Employee_{employeeId}");
        }

        // Broad monitoring room for Team Leaders on duty
        public async Task JoinLeaderDashboard()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "ActiveLeadersDashboard");
        }
    }
}
