using Microsoft.EntityFrameworkCore;
using MeetingRoomManagement.Api.Data;
using MeetingRoomManagement.Api.Domain.Entities;

namespace MeetingRoomManagement.Tests.TestHelpers;

public static class DbContextHelper
{
    public static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new AppDbContext(options);

        // Seed standard rooms
        context.Rooms.AddRange(
            new Room { Id = 1, Name = "Cairo Executive Suite", Capacity = 16, Floor = 3 },
            new Room { Id = 2, Name = "Nile Boardroom", Capacity = 12, Floor = 2 },
            new Room { Id = 3, Name = "Giza Innovation Pod", Capacity = 6, Floor = 1 },
            new Room { Id = 4, Name = "Alexandria Focus Room", Capacity = 4, Floor = 2 },
            new Room { Id = 5, Name = "Heliopolis Studio", Capacity = 8, Floor = 1 }
        );

        context.SaveChanges();
        return context;
    }
}
