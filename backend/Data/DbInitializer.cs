using MeetingRoomManagement.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MeetingRoomManagement.Api.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(AppDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        if (await context.Rooms.AnyAsync())
        {
            return; // DB has already been seeded
        }

        var rooms = new List<Room>
        {
            new() { Name = "Cairo Executive Suite", Capacity = 16, Floor = 3 },
            new() { Name = "Nile Boardroom", Capacity = 12, Floor = 2 },
            new() { Name = "Giza Innovation Pod", Capacity = 6, Floor = 1 },
            new() { Name = "Alexandria Focus Room", Capacity = 4, Floor = 2 },
            new() { Name = "Heliopolis Studio", Capacity = 8, Floor = 1 }
        };

        await context.Rooms.AddRangeAsync(rooms);
        await context.SaveChangesAsync();

        // Seed initial sample bookings for today and tomorrow to give rich immediate UX
        var today = DateTime.Today;
        var nowUtc = DateTime.UtcNow;

        var sampleBookings = new List<Booking>
        {
            // Room 1: Cairo Executive Suite
            new()
            {
                RoomId = rooms[0].Id,
                Title = "Quarterly Strategy Review",
                BookedBy = "Sarah Jenkins",
                StartTime = today.AddHours(9),
                EndTime = today.AddHours(11),
                CreatedAt = nowUtc
            },
            new()
            {
                RoomId = rooms[0].Id,
                Title = "Executive Committee Sync",
                BookedBy = "Tarek Mansour",
                StartTime = today.AddHours(14),
                EndTime = today.AddHours(15).AddMinutes(30),
                CreatedAt = nowUtc
            },

            // Room 2: Nile Boardroom
            new()
            {
                RoomId = rooms[1].Id,
                Title = "Product Roadmap H2",
                BookedBy = "Layla Omar",
                StartTime = today.AddHours(10),
                EndTime = today.AddHours(12),
                CreatedAt = nowUtc
            },
            new()
            {
                RoomId = rooms[1].Id,
                Title = "Investor Briefing",
                BookedBy = "Karim Zaki",
                StartTime = today.AddHours(16),
                EndTime = today.AddHours(17).AddMinutes(30),
                CreatedAt = nowUtc
            },

            // Room 3: Giza Innovation Pod
            new()
            {
                RoomId = rooms[2].Id,
                Title = "Design Sprint Standup",
                BookedBy = "Youssef Nabil",
                StartTime = today.AddHours(8).AddMinutes(30),
                EndTime = today.AddHours(10),
                CreatedAt = nowUtc
            },

            // Room 4: Alexandria Focus Room
            new()
            {
                RoomId = rooms[3].Id,
                Title = "Candidate Technical Interview",
                BookedBy = "Amira Fathy",
                StartTime = today.AddHours(13),
                EndTime = today.AddHours(14),
                CreatedAt = nowUtc
            }
        };

        await context.Bookings.AddRangeAsync(sampleBookings);
        await context.SaveChangesAsync();
    }
}
