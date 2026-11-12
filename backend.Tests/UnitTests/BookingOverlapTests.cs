using MeetingRoomManagement.Api.Domain.Entities;
using MeetingRoomManagement.Api.Domain.Exceptions;
using MeetingRoomManagement.Api.DTOs;
using MeetingRoomManagement.Api.Services;
using MeetingRoomManagement.Tests.TestHelpers;
using Xunit;

namespace MeetingRoomManagement.Tests.UnitTests;

public class BookingOverlapTests
{
    private readonly DateTime _simulatedNow = new(2026, 11, 20, 8, 0, 0);

    [Fact]
    public async Task CreateBooking_ExactOverlap_ThrowsBookingConflictException()
    {
        // Arrange
        using var context = DbContextHelper.CreateContext();
        var dateProvider = new TestDateTimeProvider(_simulatedNow);
        var service = new BookingService(context, dateProvider);

        var existingBooking = new Booking
        {
            RoomId = 1,
            Title = "Existing Standup",
            BookedBy = "Alice",
            StartTime = new DateTime(2026, 11, 20, 10, 0, 0),
            EndTime = new DateTime(2026, 11, 20, 11, 0, 0),
            CreatedAt = _simulatedNow
        };
        context.Bookings.Add(existingBooking);
        await context.SaveChangesAsync();

        var request = new CreateBookingRequest
        {
            RoomId = 1,
            Title = "New Meeting",
            BookedBy = "Bob",
            StartTime = new DateTime(2026, 11, 20, 10, 0, 0),
            EndTime = new DateTime(2026, 11, 20, 11, 0, 0)
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BookingConflictException>(() => service.CreateBookingAsync(request));
        Assert.Equal("BOOKING_CONFLICT", ex.Code);
    }

    [Fact]
    public async Task CreateBooking_PartialOverlap_StartInsideExisting_ThrowsBookingConflictException()
    {
        // Existing: 10:00 - 12:00
        // New: 11:00 - 13:00
        using var context = DbContextHelper.CreateContext();
        var dateProvider = new TestDateTimeProvider(_simulatedNow);
        var service = new BookingService(context, dateProvider);

        context.Bookings.Add(new Booking
        {
            RoomId = 1,
            Title = "Workshop",
            BookedBy = "Alice",
            StartTime = new DateTime(2026, 11, 20, 10, 0, 0),
            EndTime = new DateTime(2026, 11, 20, 12, 0, 0),
            CreatedAt = _simulatedNow
        });
        await context.SaveChangesAsync();

        var request = new CreateBookingRequest
        {
            RoomId = 1,
            Title = "Planning",
            BookedBy = "Bob",
            StartTime = new DateTime(2026, 11, 20, 11, 0, 0),
            EndTime = new DateTime(2026, 11, 20, 13, 0, 0)
        };

        var ex = await Assert.ThrowsAsync<BookingConflictException>(() => service.CreateBookingAsync(request));
        Assert.Equal("BOOKING_CONFLICT", ex.Code);
    }

    [Fact]
    public async Task CreateBooking_PartialOverlap_EndInsideExisting_ThrowsBookingConflictException()
    {
        // Existing: 10:00 - 12:00
        // New: 09:00 - 11:00
        using var context = DbContextHelper.CreateContext();
        var dateProvider = new TestDateTimeProvider(_simulatedNow);
        var service = new BookingService(context, dateProvider);

        context.Bookings.Add(new Booking
        {
            RoomId = 1,
            Title = "Workshop",
            BookedBy = "Alice",
            StartTime = new DateTime(2026, 11, 20, 10, 0, 0),
            EndTime = new DateTime(2026, 11, 20, 12, 0, 0),
            CreatedAt = _simulatedNow
        });
        await context.SaveChangesAsync();

        var request = new CreateBookingRequest
        {
            RoomId = 1,
            Title = "Early Sync",
            BookedBy = "Bob",
            StartTime = new DateTime(2026, 11, 20, 9, 0, 0),
            EndTime = new DateTime(2026, 11, 20, 11, 0, 0)
        };

        var ex = await Assert.ThrowsAsync<BookingConflictException>(() => service.CreateBookingAsync(request));
        Assert.Equal("BOOKING_CONFLICT", ex.Code);
    }

    [Fact]
    public async Task CreateBooking_Enveloping_NewEncompassesExisting_ThrowsBookingConflictException()
    {
        // Existing: 10:00 - 11:00
        // New: 09:00 - 12:00
        using var context = DbContextHelper.CreateContext();
        var dateProvider = new TestDateTimeProvider(_simulatedNow);
        var service = new BookingService(context, dateProvider);

        context.Bookings.Add(new Booking
        {
            RoomId = 1,
            Title = "Quick Sync",
            BookedBy = "Alice",
            StartTime = new DateTime(2026, 11, 20, 10, 0, 0),
            EndTime = new DateTime(2026, 11, 20, 11, 0, 0),
            CreatedAt = _simulatedNow
        });
        await context.SaveChangesAsync();

        var request = new CreateBookingRequest
        {
            RoomId = 1,
            Title = "All Hands",
            BookedBy = "Bob",
            StartTime = new DateTime(2026, 11, 20, 9, 0, 0),
            EndTime = new DateTime(2026, 11, 20, 12, 0, 0)
        };

        var ex = await Assert.ThrowsAsync<BookingConflictException>(() => service.CreateBookingAsync(request));
        Assert.Equal("BOOKING_CONFLICT", ex.Code);
    }

    [Fact]
    public async Task CreateBooking_Enveloped_ExistingEncompassesNew_ThrowsBookingConflictException()
    {
        // Existing: 09:00 - 13:00
        // New: 10:00 - 11:00
        using var context = DbContextHelper.CreateContext();
        var dateProvider = new TestDateTimeProvider(_simulatedNow);
        var service = new BookingService(context, dateProvider);

        context.Bookings.Add(new Booking
        {
            RoomId = 1,
            Title = "Offsite",
            BookedBy = "Alice",
            StartTime = new DateTime(2026, 11, 20, 9, 0, 0),
            EndTime = new DateTime(2026, 11, 20, 13, 0, 0),
            CreatedAt = _simulatedNow
        });
        await context.SaveChangesAsync();

        var request = new CreateBookingRequest
        {
            RoomId = 1,
            Title = "Breakout",
            BookedBy = "Bob",
            StartTime = new DateTime(2026, 11, 20, 10, 0, 0),
            EndTime = new DateTime(2026, 11, 20, 11, 0, 0)
        };

        var ex = await Assert.ThrowsAsync<BookingConflictException>(() => service.CreateBookingAsync(request));
        Assert.Equal("BOOKING_CONFLICT", ex.Code);
    }

    [Fact]
    public async Task CreateBooking_BackToBack_ImmediatelyBefore_Succeeds()
    {
        // Existing: 10:00 - 11:00
        // New: 09:00 - 10:00 (ends exactly when existing starts)
        using var context = DbContextHelper.CreateContext();
        var dateProvider = new TestDateTimeProvider(_simulatedNow);
        var service = new BookingService(context, dateProvider);

        context.Bookings.Add(new Booking
        {
            RoomId = 1,
            Title = "Existing 10-11",
            BookedBy = "Alice",
            StartTime = new DateTime(2026, 11, 20, 10, 0, 0),
            EndTime = new DateTime(2026, 11, 20, 11, 0, 0),
            CreatedAt = _simulatedNow
        });
        await context.SaveChangesAsync();

        var request = new CreateBookingRequest
        {
            RoomId = 1,
            Title = "Morning Prep",
            BookedBy = "Bob",
            StartTime = new DateTime(2026, 11, 20, 9, 0, 0),
            EndTime = new DateTime(2026, 11, 20, 10, 0, 0)
        };

        var result = await service.CreateBookingAsync(request);
        Assert.NotNull(result);
        Assert.Equal(request.Title, result.Title);
        Assert.Equal(request.StartTime, result.StartTime);
        Assert.Equal(request.EndTime, result.EndTime);
    }

    [Fact]
    public async Task CreateBooking_BackToBack_ImmediatelyAfter_Succeeds()
    {
        // Existing: 10:00 - 11:00
        // New: 11:00 - 12:00 (starts exactly when existing ends)
        using var context = DbContextHelper.CreateContext();
        var dateProvider = new TestDateTimeProvider(_simulatedNow);
        var service = new BookingService(context, dateProvider);

        context.Bookings.Add(new Booking
        {
            RoomId = 1,
            Title = "Existing 10-11",
            BookedBy = "Alice",
            StartTime = new DateTime(2026, 11, 20, 10, 0, 0),
            EndTime = new DateTime(2026, 11, 20, 11, 0, 0),
            CreatedAt = _simulatedNow
        });
        await context.SaveChangesAsync();

        var request = new CreateBookingRequest
        {
            RoomId = 1,
            Title = "Afternoon Followup",
            BookedBy = "Charlie",
            StartTime = new DateTime(2026, 11, 20, 11, 0, 0),
            EndTime = new DateTime(2026, 11, 20, 12, 0, 0)
        };

        var result = await service.CreateBookingAsync(request);
        Assert.NotNull(result);
        Assert.Equal(request.Title, result.Title);
        Assert.Equal(request.StartTime, result.StartTime);
        Assert.Equal(request.EndTime, result.EndTime);
    }

    [Fact]
    public async Task CreateBooking_SameTimeSlotDifferentRoom_Succeeds()
    {
        // Room 1 has 10:00 - 11:00
        // Room 2 booking 10:00 - 11:00 should succeed
        using var context = DbContextHelper.CreateContext();
        var dateProvider = new TestDateTimeProvider(_simulatedNow);
        var service = new BookingService(context, dateProvider);

        context.Bookings.Add(new Booking
        {
            RoomId = 1,
            Title = "Room 1 Meeting",
            BookedBy = "Alice",
            StartTime = new DateTime(2026, 11, 20, 10, 0, 0),
            EndTime = new DateTime(2026, 11, 20, 11, 0, 0),
            CreatedAt = _simulatedNow
        });
        await context.SaveChangesAsync();

        var request = new CreateBookingRequest
        {
            RoomId = 2, // Different room
            Title = "Room 2 Meeting",
            BookedBy = "Bob",
            StartTime = new DateTime(2026, 11, 20, 10, 0, 0),
            EndTime = new DateTime(2026, 11, 20, 11, 0, 0)
        };

        var result = await service.CreateBookingAsync(request);
        Assert.NotNull(result);
        Assert.Equal(2, result.RoomId);
        Assert.Equal("Nile Boardroom", result.RoomName);
    }
}
