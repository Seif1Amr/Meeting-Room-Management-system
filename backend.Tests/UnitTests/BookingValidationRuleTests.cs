using MeetingRoomManagement.Api.Domain.Exceptions;
using MeetingRoomManagement.Api.DTOs;
using MeetingRoomManagement.Api.Services;
using MeetingRoomManagement.Tests.TestHelpers;
using Xunit;

namespace MeetingRoomManagement.Tests.UnitTests;

public class BookingValidationRuleTests
{
    // Fixed simulated time: Friday, Nov 20, 2026 at 08:00 AM
    private readonly DateTime _simulatedNow = new(2026, 11, 20, 8, 0, 0);

    [Fact]
    public async Task CreateBooking_PastDate_ThrowsBookingValidationException()
    {
        // Simulated current time is 2026-11-20 08:00
        // Requesting 2026-11-19 10:00 (yesterday)
        using var context = DbContextHelper.CreateContext();
        var dateProvider = new TestDateTimeProvider(_simulatedNow);
        var service = new BookingService(context, dateProvider);

        var request = new CreateBookingRequest
        {
            RoomId = 1,
            Title = "Past Meeting",
            BookedBy = "Jane",
            StartTime = new DateTime(2026, 11, 19, 10, 0, 0),
            EndTime = new DateTime(2026, 11, 19, 11, 0, 0)
        };

        var ex = await Assert.ThrowsAsync<BookingValidationException>(() => service.CreateBookingAsync(request));
        Assert.Equal("PAST_BOOKING_NOT_ALLOWED", ex.Code);
    }

    [Fact]
    public async Task CreateBooking_PastTimeEarlierToday_ThrowsBookingValidationException()
    {
        // Simulated current time is 2026-11-20 14:00
        // Requesting 2026-11-20 10:00 (earlier today)
        var simulatedNowAfternoon = new DateTime(2026, 11, 20, 14, 0, 0);
        using var context = DbContextHelper.CreateContext();
        var dateProvider = new TestDateTimeProvider(simulatedNowAfternoon);
        var service = new BookingService(context, dateProvider);

        var request = new CreateBookingRequest
        {
            RoomId = 1,
            Title = "Morning Standup (Too Late)",
            BookedBy = "Jane",
            StartTime = new DateTime(2026, 11, 20, 10, 0, 0),
            EndTime = new DateTime(2026, 11, 20, 11, 0, 0)
        };

        var ex = await Assert.ThrowsAsync<BookingValidationException>(() => service.CreateBookingAsync(request));
        Assert.Equal("PAST_BOOKING_NOT_ALLOWED", ex.Code);
    }

    [Fact]
    public async Task CreateBooking_EndBeforeStart_ThrowsBookingValidationException()
    {
        using var context = DbContextHelper.CreateContext();
        var dateProvider = new TestDateTimeProvider(_simulatedNow);
        var service = new BookingService(context, dateProvider);

        var request = new CreateBookingRequest
        {
            RoomId = 1,
            Title = "Backwards Meeting",
            BookedBy = "Jane",
            StartTime = new DateTime(2026, 11, 20, 12, 0, 0),
            EndTime = new DateTime(2026, 11, 20, 11, 0, 0) // End before start
        };

        var ex = await Assert.ThrowsAsync<BookingValidationException>(() => service.CreateBookingAsync(request));
        Assert.Equal("INVALID_TIME_RANGE", ex.Code);
    }

    [Fact]
    public async Task CreateBooking_EndEqualToStart_ThrowsBookingValidationException()
    {
        using var context = DbContextHelper.CreateContext();
        var dateProvider = new TestDateTimeProvider(_simulatedNow);
        var service = new BookingService(context, dateProvider);

        var request = new CreateBookingRequest
        {
            RoomId = 1,
            Title = "Zero Duration Meeting",
            BookedBy = "Jane",
            StartTime = new DateTime(2026, 11, 20, 11, 0, 0),
            EndTime = new DateTime(2026, 11, 20, 11, 0, 0) // End == start
        };

        var ex = await Assert.ThrowsAsync<BookingValidationException>(() => service.CreateBookingAsync(request));
        Assert.Equal("INVALID_TIME_RANGE", ex.Code);
    }

    [Fact]
    public async Task CreateBooking_MultiDaySpanning_ThrowsBookingValidationException()
    {
        using var context = DbContextHelper.CreateContext();
        var dateProvider = new TestDateTimeProvider(_simulatedNow);
        var service = new BookingService(context, dateProvider);

        var request = new CreateBookingRequest
        {
            RoomId = 1,
            Title = "Overnight Hackathon",
            BookedBy = "Jane",
            StartTime = new DateTime(2026, 11, 20, 19, 0, 0),
            EndTime = new DateTime(2026, 11, 21, 9, 0, 0) // Spans midnight
        };

        var ex = await Assert.ThrowsAsync<BookingValidationException>(() => service.CreateBookingAsync(request));
        Assert.Equal("CROSS_DAY_NOT_ALLOWED", ex.Code);
    }

    [Fact]
    public async Task CreateBooking_BeforeWorkingHours_ThrowsBookingValidationException()
    {
        // 07:30 start is before 08:00
        using var context = DbContextHelper.CreateContext();
        var earlyProvider = new TestDateTimeProvider(new DateTime(2026, 11, 20, 6, 0, 0));
        var service = new BookingService(context, earlyProvider);

        var request = new CreateBookingRequest
        {
            RoomId = 1,
            Title = "Dawn Briefing",
            BookedBy = "Jane",
            StartTime = new DateTime(2026, 11, 20, 7, 30, 0),
            EndTime = new DateTime(2026, 11, 20, 9, 0, 0)
        };

        var ex = await Assert.ThrowsAsync<BookingValidationException>(() => service.CreateBookingAsync(request));
        Assert.Equal("OUTSIDE_WORKING_HOURS", ex.Code);
    }

    [Fact]
    public async Task CreateBooking_AfterWorkingHours_ThrowsBookingValidationException()
    {
        // 20:30 end is after 20:00
        using var context = DbContextHelper.CreateContext();
        var dateProvider = new TestDateTimeProvider(_simulatedNow);
        var service = new BookingService(context, dateProvider);

        var request = new CreateBookingRequest
        {
            RoomId = 1,
            Title = "Late Night Sync",
            BookedBy = "Jane",
            StartTime = new DateTime(2026, 11, 20, 19, 0, 0),
            EndTime = new DateTime(2026, 11, 20, 20, 30, 0)
        };

        var ex = await Assert.ThrowsAsync<BookingValidationException>(() => service.CreateBookingAsync(request));
        Assert.Equal("OUTSIDE_WORKING_HOURS", ex.Code);
    }

    [Fact]
    public async Task CreateBooking_ExactWorkingHourBoundaries_Succeeds()
    {
        using var context = DbContextHelper.CreateContext();
        var dateProvider = new TestDateTimeProvider(_simulatedNow);
        var service = new BookingService(context, dateProvider);

        // 1. Morning boundary: 08:00 to 09:00
        var morningRequest = new CreateBookingRequest
        {
            RoomId = 1,
            Title = "First Slot of Day",
            BookedBy = "Jane",
            StartTime = new DateTime(2026, 11, 20, 8, 0, 0),
            EndTime = new DateTime(2026, 11, 20, 9, 0, 0)
        };
        var morningResult = await service.CreateBookingAsync(morningRequest);
        Assert.NotNull(morningResult);

        // 2. Evening boundary: 19:00 to 20:00
        var eveningRequest = new CreateBookingRequest
        {
            RoomId = 1,
            Title = "Last Slot of Day",
            BookedBy = "Jane",
            StartTime = new DateTime(2026, 11, 20, 19, 0, 0),
            EndTime = new DateTime(2026, 11, 20, 20, 0, 0)
        };
        var eveningResult = await service.CreateBookingAsync(eveningRequest);
        Assert.NotNull(eveningResult);
    }

    [Fact]
    public async Task CreateBooking_ExceedsFourHoursDuration_ThrowsBookingValidationException()
    {
        // 09:00 to 13:01 is 4 hours and 1 minute (> 4 hours)
        using var context = DbContextHelper.CreateContext();
        var dateProvider = new TestDateTimeProvider(_simulatedNow);
        var service = new BookingService(context, dateProvider);

        var request = new CreateBookingRequest
        {
            RoomId = 1,
            Title = "Mega Marathon",
            BookedBy = "Jane",
            StartTime = new DateTime(2026, 11, 20, 9, 0, 0),
            EndTime = new DateTime(2026, 11, 20, 13, 1, 0)
        };

        var ex = await Assert.ThrowsAsync<BookingValidationException>(() => service.CreateBookingAsync(request));
        Assert.Equal("EXCEEDS_MAX_DURATION", ex.Code);
    }

    [Fact]
    public async Task CreateBooking_ExactlyFourHoursDuration_Succeeds()
    {
        // 08:00 to 12:00 is exactly 4 hours -> Allowed
        using var context = DbContextHelper.CreateContext();
        var dateProvider = new TestDateTimeProvider(_simulatedNow);
        var service = new BookingService(context, dateProvider);

        var request = new CreateBookingRequest
        {
            RoomId = 1,
            Title = "4-Hour Strategy Session",
            BookedBy = "Jane",
            StartTime = new DateTime(2026, 11, 20, 8, 0, 0),
            EndTime = new DateTime(2026, 11, 20, 12, 0, 0)
        };

        var result = await service.CreateBookingAsync(request);
        Assert.NotNull(result);
        Assert.Equal(TimeSpan.FromHours(4), result.EndTime - result.StartTime);
    }

    [Fact]
    public async Task CreateBooking_NonExistentRoom_ThrowsNotFoundException()
    {
        using var context = DbContextHelper.CreateContext();
        var dateProvider = new TestDateTimeProvider(_simulatedNow);
        var service = new BookingService(context, dateProvider);

        var request = new CreateBookingRequest
        {
            RoomId = 9999, // Non-existent room
            Title = "Lost Booking",
            BookedBy = "Jane",
            StartTime = new DateTime(2026, 11, 20, 10, 0, 0),
            EndTime = new DateTime(2026, 11, 20, 11, 0, 0)
        };

        var ex = await Assert.ThrowsAsync<NotFoundException>(() => service.CreateBookingAsync(request));
        Assert.Equal("ROOM_NOT_FOUND", ex.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateBooking_BlankTitle_ThrowsBookingValidationException(string emptyTitle)
    {
        using var context = DbContextHelper.CreateContext();
        var dateProvider = new TestDateTimeProvider(_simulatedNow);
        var service = new BookingService(context, dateProvider);

        var request = new CreateBookingRequest
        {
            RoomId = 1,
            Title = emptyTitle,
            BookedBy = "Jane",
            StartTime = new DateTime(2026, 11, 20, 10, 0, 0),
            EndTime = new DateTime(2026, 11, 20, 11, 0, 0)
        };

        var ex = await Assert.ThrowsAsync<BookingValidationException>(() => service.CreateBookingAsync(request));
        Assert.Equal("TITLE_REQUIRED", ex.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateBooking_BlankBookedBy_ThrowsBookingValidationException(string emptyBookedBy)
    {
        using var context = DbContextHelper.CreateContext();
        var dateProvider = new TestDateTimeProvider(_simulatedNow);
        var service = new BookingService(context, dateProvider);

        var request = new CreateBookingRequest
        {
            RoomId = 1,
            Title = "Valid Title",
            BookedBy = emptyBookedBy,
            StartTime = new DateTime(2026, 11, 20, 10, 0, 0),
            EndTime = new DateTime(2026, 11, 20, 11, 0, 0)
        };

        var ex = await Assert.ThrowsAsync<BookingValidationException>(() => service.CreateBookingAsync(request));
        Assert.Equal("BOOKED_BY_REQUIRED", ex.Code);
    }
}
