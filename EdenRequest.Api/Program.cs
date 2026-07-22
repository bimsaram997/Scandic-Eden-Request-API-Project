using EdenRequest.Api.Data;
using EdenRequest.Api.Hubs;
using EdenRequest.Api.Repositories;
using EdenRequest.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Database configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));
// 1. CORS POLICY DEFINITION
builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularClientPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://localhost:4200", "https://eden-request-frontend-dev-0oip.onrender.com")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .SetPreflightMaxAge(TimeSpan.FromMinutes(10)); //  Tells Firefox to cache the approval for 10 minutes
    });
});

builder.Services.AddSignalR();

// Dependency Injection Scopes
builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.AddScoped<IItemService, ItemService>();
builder.Services.AddScoped<IRequestRepository, RequestRepository>();
builder.Services.AddScoped<IRequestService, RequestService>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IItemCategoryRepository, ItemCategoryRepository>();
builder.Services.AddScoped<IitemCategoryService, itemCategoryService>();
builder.Services.AddScoped<IExtraWorkRequestRepository, ExtraWorkRequestRepository>();
builder.Services.AddScoped<IExtraWorkRequestService, ExtraWorkRequestService>();
builder.Services.AddScoped<IExtraWorkItemRepository, ExtraWorkItemRepository>();
builder.Services.AddScoped<IExtraWorkItemService, ExtraWorkItemService>();

builder.Services.AddScoped<IReportsRepository, ReportsRepository>();
builder.Services.AddScoped<IReportsService, ReportsService>();


builder.Services.AddScoped<NotificationService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Eden Request API v1");

    // Optional: This makes Swagger the default page at the root URL (/)
    // If you prefer typing /swagger locally, you can leave this line out!
    c.RoutePrefix = string.Empty;
});
app.UseHttpsRedirection();

app.UseRouting(); // 1. Set up routing metadata first

// ===================================================================
// FIX: ACTIVATED CORS MIDDLEWARE HERE WITH YOUR EXACT POLICY NAME
// ===================================================================
app.UseCors("AngularClientPolicy");

app.UseAuthorization(); // 3. Authorize requests after CORS clears them

app.MapControllers();

// Map SignalR Hub
app.MapHub<NotificationHub>("/notificationHub");
Console.WriteLine($"\n ENVIRONMENT: {builder.Environment.EnvironmentName}");
Console.WriteLine($" ACTIVE VAPID PUBLIC KEY: {builder.Configuration["VapidDetails:PublicKey"]}\n");

app.Run();