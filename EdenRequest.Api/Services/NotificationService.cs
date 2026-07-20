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

        public NotificationService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task SendNotificationToEmployeeAsync(int employeeId, string title, string body, string targetUrl)
        {
            var employee = await _context.Employees.FindAsync(employeeId);

            if (employee == null || string.IsNullOrEmpty(employee.PushEndpoint)) return;

            var subscription = new PushSubscription(
                employee.PushEndpoint,
                employee.PushP256DH,
                employee.PushAuth
            );

            // 🟢 Dynamics options read directly from your appsettings configuration layers
            var vapidSubject = _configuration["VapidDetails:Subject"] ?? "mailto:admin@edenapp.com";
            var publicVapidKey = "BM_zv_20Wct-5d_mzZQvOH61AN1laP6ZEIHZ9i7IB6eBPVhbl4U8KzFG_qTggrjfUMoc-5dPJ9d-12QeUQibmvE";
            var privateVapidKey = "pGUcYJFgd39O7jufYJjdlldcX5C3vf-6yPtkKJ0riHk";

            if (string.IsNullOrEmpty(publicVapidKey) || string.IsNullOrEmpty(privateVapidKey))
            {
                throw new InvalidOperationException("VAPID cryptographic keys are not configured in appsettings.json.");
            }

            var vapidDetails = new VapidDetails(vapidSubject, publicVapidKey, privateVapidKey);
            var webPushClient = new WebPushClient();

            var payload = JsonSerializer.Serialize(new
            {
                title = title,
                body = body,
                dir = "ltr",
                lang = "en",
                renotify = true,
                tag = "request-alert",
                data = new { url = targetUrl }
            });

            try
            {
                await webPushClient.SendNotificationAsync(subscription, payload, vapidDetails);
            }
            catch (WebPushException ex)
            {
                Console.WriteLine($"[WebPush Engine Exception] Code: {ex.StatusCode} | {ex.Message}");

                if (ex.StatusCode == System.Net.HttpStatusCode.Gone || ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    employee.PushEndpoint = null;
                    employee.PushP256DH = null;
                    employee.PushAuth = null;
                    await _context.SaveChangesAsync();
                }
                throw;
            }
        }
    }
}