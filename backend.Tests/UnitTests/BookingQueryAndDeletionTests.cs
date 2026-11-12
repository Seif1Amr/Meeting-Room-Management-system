using MeetingRoomManagement.Api.Domain.Entities;
using MeetingRoomManagement.Api.Domain.Exceptions;
using MeetingRoomManagement.Api.Services;
using MeetingRoomManagement.Tests.TestHelpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MeetingRoomManagement.Tests.UnitTests;

public class BookingQueryAndDeletionTests
{
    private readonly DateTime _simulatedNow = new(2026, 11, 20, 8, 0, 0);

    [Fact]
    public async Task GetBookingsByRoomAndDate_ReturnsOnlyMatchingRoomAndDate_OrderedChronologically()
    {
        using var context = DbContextHelper.CreateContext();
        var dateProvider = new TestDateTimeProvider(_simulatedNow);
        var service = new BookingService(context, dateProvider);

        var targetDate = new DateOnly(2026, 11, 20);

        // Add bookings: out of order, plus another room, plus another day
        context.Bookings.AddRange(
            // Target Room & Target Day: 14:00 - 15:00
            new Booking
            {
                RoomId = 1,
                Title = "Afternoon Meeting",
                BookedBy = "Bob",
                StartTime = new DateTime(2026, 11, 20, 14, 0, 0),
                EndTime = new DateTime(2026, 11, 20, 15, 0, 0),
                CreatedAt = _simulatedNow
            },
            // Target Room & Target Day: 09:00 - 10:00 (Should be first!)
            new Booking
            {
                RoomId = 1,
                Title = "Morning Meeting",
                BookedBy = "Alice",
                StartTime = new DateTime(2026, 11, 20, 9, 0, 0),
                EndTime = new DateTime(2026, 11, 20, 10, 0, 0),
                CreatedAt = _simulatedNow
            },
            // Different Room: Room 2
            new Booking
            {
                RoomId = 2,
                Title = "Room 2 Meeting",
                BookedBy = "Charlie",
                StartTime = new DateTime(2026, 11, 20, 10, 0, 0),
                EndTime = new DateTime(2026, 11, 20, 11, 0, 0),
                CreatedAt = _simulatedNow
            },
            // Different Day: 2026-11-21
            new Booking
            {
                RoomId = 1,
                Title = "Tomorrow Meeting",
                BookedBy = "Dave",
                StartTime = new DateTime(2026, 11, 21, 9, 0, 0),
                EndTime = new DateTime(2026, 11, 21, 10, 0, 0),
                CreatedAt = _simulatedNow
            }
        );
        await context.SaveChangesAsync();

        // Act
        var result = (await service.GetBookingsByRoomAndDateAsync(1, targetDate)).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Morning Meeting", result[0].Title);
        Assert.Equal(new DateTime(2026, 11, 20, 9, 0, 0), result[0].StartTime);
        Assert.Equal("Afternoon Meeting", result[1].Title);
        Assert.Equal(new DateTime(2026, 11, 20, 14, 0, 0), result[1].StartTime);
    }

    [Fact]
    public async Task GetBookingsByRoomAndDate_NonExistentRoom_ThrowsNotFoundException()
    {
        using var context = DbContextHelper.CreateContext();
        var dateProvider = new TestDateTimeProvider(_simulatedNow);
        var service = new BookingService(context, dateProvider);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.GetBookingsByRoomAndDateAsync(999, new DateOnly(2026, 11, 20)));
    }

    [Fact]
    public async Task DeleteBooking_ExistingBooking_RemovesFromDatabaseAndReturnsTrue()
    {
        using var context = DbContextHelper.CreateContext();
        var dateProvider = new TestDateTimeProvider(_simulatedNow);
        var service = new BookingService(context, dateProvider);

        var booking = new Booking
        {
            RoomId = 1,
            Title = "To Be Deleted",
            BookedBy = "Alice",
            StartTime = new DateTime(2026, 11, 20, 10, 0, 0),
            EndTime = new DateTime(2026, 11, 20, 11, 0, 0),
            CreatedAt = _simulatedNow
        };
        context.Bookings.Add(booking);
        await context.SaveChangesAsync();

        var success = await service.DeleteBookingAsync(booking.Id);
        Assert.True(success);

        var inDb = await context.Bookings.FirstOrDefaultAsync(b => b.Id == booking.Id);
        Assert.Null(inDb);
    }

    [Fact]
    public async Task DeleteBooking_NonExistentId_ReturnsFalse()
    {
        using var context = DbContextHelper.CreateContext();
        var dateProvider = new TestDateTimeProvider(_simulatedNow);
        var service = new BookingService(context, dateProvider);

        var success = await service.DeleteBookingAsync(9999);
        Assert.False(success);
    }
}
