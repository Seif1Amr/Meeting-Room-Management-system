using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MeetingRoomManagement.Api.Data;
using MeetingRoomManagement.Api.Middleware;
using MeetingRoomManagement.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Database Configuration (EF Core SQLite)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=rooms.db";
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite(connectionString);
});

// 2. Dependency Injection
builder.Services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IBookingService, BookingService>();

// 3. Controllers & JSON options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// 4. CORS: Permissive for Angular dev server and Docker environments
const string corsPolicyName = "AllowAngularApp";
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicyName, policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// 5. Swagger / OpenAPI Configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Meeting Room Booking API",
        Version = "v1",
        Description = "Enterprise-grade REST API for Meeting Room Management with collision-free scheduling."
    });
});

var app = builder.Build();

// 6. Global Exception Handling Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// 7. Swagger UI available in both Dev and Prod for seamless evaluation
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Meeting Room Booking API v1");
    c.RoutePrefix = "swagger";
});

app.UseCors(corsPolicyName);

app.UseAuthorization();

app.MapControllers();

// 8. Auto-create & Seed Database on Startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbInitializer.InitializeAsync(context);
}

app.Run();
