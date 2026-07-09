using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace EdenRequest.Api.Hubs
{
    public class NotificationHub: Hub
    {
        // 🚀 A thread-safe lookup map: Email -> Set of Active Connection IDs
        public static readonly ConcurrentDictionary<string, HashSet<string>> ActiveUsers =
            new ConcurrentDictionary<string, HashSet<string>>();

        public async Task JoinUserByRole(string email, string role)
        {
            if (string.IsNullOrEmpty(email)) return;

            // 1. Add them to our online tracking dictionary
            ActiveUsers.AddOrUpdate(
                email.ToLower().Trim(), // Normalize to prevent casing mismatches
                new HashSet<string> { Context.ConnectionId },
                (key, existingSet) => {
                    lock (existingSet) { existingSet.Add(Context.ConnectionId); }
                    return existingSet;
                });

            // 2. Run your existing group placement rules
            string cleanEmailGroup = $"User_{email.Replace("@", "_").Replace(".", "_")}";
            await Groups.AddToGroupAsync(Context.ConnectionId, cleanEmailGroup);

            if (role == "Leader" || role == "TeamLeader")
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "ActiveLeadersDashboard");
            }
        }

        // 🧹 Automatically clean up tracking when a user disconnects
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // Scan our tracking dictionary to remove this specific ConnectionId
            foreach (var kvp in ActiveUsers)
            {
                var connections = kvp.Value;
                lock (connections)
                {
                    if (connections.Contains(Context.ConnectionId))
                    {
                        connections.Remove(Context.ConnectionId);

                        // If that was the user's only open tab, remove them from the online map entirely
                        if (connections.Count == 0)
                        {
                            ActiveUsers.TryRemove(kvp.Key, out _);
                        }
                        break;
                    }
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task LeaveUser(string email)
        {
            if (string.IsNullOrEmpty(email)) return;

            string cleanEmail = email.ToLower().Trim();

            if (ActiveUsers.TryGetValue(cleanEmail, out var connections))
            {
                lock (connections)
                {
                    connections.Remove(Context.ConnectionId);
                }

                if (connections.Count == 0)
                {
                    ActiveUsers.TryRemove(cleanEmail, out _);
                }
            }

            // Optional: Explicitly remove them from the dashboard group immediately
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "ActiveLeadersDashboard");

            Console.WriteLine($"🔴 SignalR tracking manually removed for logout: {cleanEmail}");
        }
    }
}
