using EdenRequest.Api.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WebPush;

namespace EdenRequest.Api.Services
{
    public class NotificationService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        // Permanent Public & Private key synchronization pairs
        

        

        public NotificationService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task SendNotificationToEmployeeAsync(int employeeId, string title, string body, string targetUrl)
        {
            var employee = await _context.Employees.FindAsync(employeeId);

            // Exit safely if the target worker has not registered device capabilities
            if (employee == null || string.IsNullOrEmpty(employee.PushEndpoint)) return;

            var subscription = new PushSubscription(
                employee.PushEndpoint,
                employee.PushP256DH,
                employee.PushAuth
            );

            // 🚀 THE LIVE PRODUCTION FIX: Extract keys safely out of environment variables
            var vapidSubject = _configuration["VapidDetails:Subject"] ?? "mailto:admin@edenapp.com";
            var publicVapidKey = _configuration["VapidDetails:PublicKey"];
            var privateVapidKey = _configuration["VapidDetails:PrivateKey"];

            // Safeguard: Ensure you have keys configured before attempting to invoke WebPushClient
            if (string.IsNullOrEmpty(publicVapidKey) || string.IsNullOrEmpty(privateVapidKey))
            {
                throw new InvalidOperationException("VAPID cryptographic keys are not configured in appsettings or cloud environment vars.");
            }

            var vapidDetails = new VapidDetails(vapidSubject, publicVapidKey, privateVapidKey);
            var webPushClient = new WebPushClient();

            // Establish standard browser JSON notifications block format
            var payload = JsonSerializer.Serialize(new
            {
                notification = new
                {
                    title = title,
                    body = body,
                    dir = "ltr",
                    lang = "en",
                    renotify = true,
                    tag = "request-alert", // Forces Chrome to treat it as a fresh alert group
                    data = new { url = targetUrl }
                }
            });

            try
            {
                await webPushClient.SendNotificationAsync(subscription, payload, vapidDetails);
            }
            catch (WebPushException ex)
            {
                // Clean dead subscription records from your SQL server if the device signature expired
                if (ex.StatusCode == System.Net.HttpStatusCode.Gone || ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    employee.PushEndpoint = null;
                    employee.PushP256DH = null;
                    employee.PushAuth = null;
                    await _context.SaveChangesAsync();
                }
            }
        }
    }
}
