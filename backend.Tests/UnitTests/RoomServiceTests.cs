using MeetingRoomManagement.Api.Domain.Entities;
using MeetingRoomManagement.Api.Domain.Exceptions;
using MeetingRoomManagement.Api.Services;
using MeetingRoomManagement.Tests.TestHelpers;
using Xunit;

namespace MeetingRoomManagement.Tests.UnitTests;

public class RoomServiceTests
{
    private readonly DateTime _simulatedNow = new(2026, 11, 20, 8, 0, 0);

    [Fact]
    public async Task GetAllRooms_ReturnsAllRoomsOrdered()
    {
        using var context = DbContextHelper.CreateContext();
        var dateProvider = new TestDateTimeProvider(_simulatedNow);
        var service = new RoomService(context, dateProvider);

        var rooms = (await service.GetAllRoomsAsync()).ToList();

        Assert.Equal(5, rooms.Count);
        Assert.Contains(rooms, r => r.Name == "Cairo Executive Suite" && r.Capacity == 16);
        Assert.Contains(rooms, r => r.Name == "Nile Boardroom" && r.Capacity == 12);
        Assert.Contains(rooms, r => r.Name == "Giza Innovation Pod" && r.Capacity == 6);
        Assert.Contains(rooms, r => r.Name == "Alexandria Focus Room" && r.Capacity == 4);
        Assert.Contains(rooms, r => r.Name == "Heliopolis Studio" && r.Capacity == 8);
    }

    [Fact]
    public async Task GetRoomById_ExistingRoom_ReturnsRoomDto()
    {
        using var context = DbContextHelper.CreateContext();
        var dateProvider = new TestDateTimeProvider(_simulatedNow);
        var service = new RoomService(context, dateProvider);

        var room = await service.GetRoomByIdAsync(1);

        Assert.NotNull(room);
        Assert.Equal("Cairo Executive Suite", room.Name);
        Assert.Equal(16, room.Capacity);
    }

    [Fact]
    public async Task GetRoomById_NonExistentRoom_ReturnsNull()
    {
        using var context = DbContextHelper.CreateContext();
        var dateProvider = new TestDateTimeProvider(_simulatedNow);
        var service = new RoomService(context, dateProvider);

        var room = await service.GetRoomByIdAsync(999);

        Assert.Null(room);
    }

    [Fact]
    public async Task GetAvailableRooms_FiltersOutBookedRoom_AndMatchesCapacity()
    {
        using var context = DbContextHelper.CreateContext();
        var dateProvider = new TestDateTimeProvider(_simulatedNow);
        var service = new RoomService(context, dateProvider);

        var date = new DateOnly(2026, 11, 20);

        // Room 1 (Cap 16) is booked 10:00 - 12:00
        context.Bookings.Add(new Booking
        {
            RoomId = 1,
            Title = "Executive Sync",
            BookedBy = "Alice",
            StartTime = new DateTime(2026, 11, 20, 10, 0, 0),
            EndTime = new DateTime(2026, 11, 20, 12, 0, 0),
            CreatedAt = _simulatedNow
        });
        await context.SaveChangesAsync();

        // Search 11:00 - 12:00 with minCapacity = 8
        // Room 1 has minCapacity 16, but is booked -> excluded!
        // Room 2 (Cap 12) is free and >= 8 -> included!
        // Room 5 (Cap 8) is free and >= 8 -> included!
        // Rooms 3 (Cap 6) and 4 (Cap 4) have cap < 8 -> excluded!
        var available = (await service.GetAvailableRoomsAsync(
            date,
            new TimeOnly(11, 0),
            new TimeOnly(12, 0),
            minCapacity: 8
        )).ToList();

        Assert.Equal(2, available.Count);
        Assert.DoesNotContain(available, r => r.Id == 1);
        Assert.Contains(available, r => r.Id == 2 && r.Name == "Nile Boardroom");
        Assert.Contains(available, r => r.Id == 5 && r.Name == "Heliopolis Studio");
    }

    [Fact]
    public async Task GetAvailableRooms_PastTime_ThrowsBookingValidationException()
    {
        // Simulated current time: 2026-11-20 15:00
        var afternoon = new DateTime(2026, 11, 20, 15, 0, 0);
        using var context = DbContextHelper.CreateContext();
        var dateProvider = new TestDateTimeProvider(afternoon);
        var service = new RoomService(context, dateProvider);

        // Searching for morning 09:00 - 10:00 earlier today
        await Assert.ThrowsAsync<BookingValidationException>(() =>
            service.GetAvailableRoomsAsync(
                new DateOnly(2026, 11, 20),
                new TimeOnly(9, 0),
                new TimeOnly(10, 0),
                null
            ));
    }

    [Fact]
    public async Task GetAvailableRooms_EndBeforeStart_ThrowsBookingValidationException()
    {
        using var context = DbContextHelper.CreateContext();
        var dateProvider = new TestDateTimeProvider(_simulatedNow);
        var service = new RoomService(context, dateProvider);

        await Assert.ThrowsAsync<BookingValidationException>(() =>
            service.GetAvailableRoomsAsync(
                new DateOnly(2026, 11, 20),
                new TimeOnly(12, 0),
                new TimeOnly(10, 0),
                null
            ));
    }

    [Fact]
    public async Task GetAvailableRooms_ExceedsFourHours_ThrowsBookingValidationException()
    {
        using var context = DbContextHelper.CreateContext();
        var dateProvider = new TestDateTimeProvider(_simulatedNow);
        var service = new RoomService(context, dateProvider);

        await Assert.ThrowsAsync<BookingValidationException>(() =>
            service.GetAvailableRoomsAsync(
                new DateOnly(2026, 11, 20),
                new TimeOnly(8, 0),
                new TimeOnly(13, 0), // 5 hours
                null
            ));
    }
}
