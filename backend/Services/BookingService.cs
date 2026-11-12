using Microsoft.EntityFrameworkCore;
using MeetingRoomManagement.Api.Data;
using MeetingRoomManagement.Api.Domain.Entities;
using MeetingRoomManagement.Api.Domain.Exceptions;
using MeetingRoomManagement.Api.DTOs;

namespace MeetingRoomManagement.Api.Services;

public class BookingService : IBookingService
{
    private readonly AppDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;

    public BookingService(AppDbContext context, IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<IEnumerable<BookingDto>> GetBookingsByRoomAndDateAsync(int roomId, DateOnly date, CancellationToken ct = default)
    {
        var roomExists = await _context.Rooms.AnyAsync(r => r.Id == roomId, ct);
        if (!roomExists)
        {
            throw new NotFoundException($"Room with ID {roomId} was not found.", "ROOM_NOT_FOUND");
        }

        var dayStart = date.ToDateTime(TimeOnly.MinValue);
        var dayEnd = date.ToDateTime(TimeOnly.MaxValue);

        return await _context.Bookings
            .AsNoTracking()
            .Where(b => b.RoomId == roomId && b.StartTime >= dayStart && b.StartTime <= dayEnd)
            .OrderBy(b => b.StartTime)
            .Select(b => new BookingDto(
                b.Id,
                b.RoomId,
                b.Room.Name,
                b.Title,
                b.BookedBy,
                b.StartTime,
                b.EndTime,
                b.CreatedAt
            ))
            .ToListAsync(ct);
    }

    public async Task<BookingDto> CreateBookingAsync(CreateBookingRequest request, CancellationToken ct = default)
    {
        // 1. Basic sanitization & field checks
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new BookingValidationException("Booking title is required.", "TITLE_REQUIRED");
        }

        if (string.IsNullOrWhiteSpace(request.BookedBy))
        {
            throw new BookingValidationException("Booked by name/email is required.", "BOOKED_BY_REQUIRED");
        }

        // 2. Rule 2: Chronology & Future
        if (request.EndTime <= request.StartTime)
        {
            throw new BookingValidationException("End time must be strictly after start time.", "INVALID_TIME_RANGE");
        }

        if (request.StartTime < _dateTimeProvider.Now)
        {
            throw new BookingValidationException("Bookings cannot be created in the past.", "PAST_BOOKING_NOT_ALLOWED");
        }

        // 3. Rule 3: Working Hours & Same Day
        if (request.StartTime.Date != request.EndTime.Date)
        {
            throw new BookingValidationException("Bookings must start and end on the same calendar day.", "CROSS_DAY_NOT_ALLOWED");
        }

        var workStart = TimeSpan.FromHours(8);  // 08:00
        var workEnd = TimeSpan.FromHours(20);   // 20:00

        if (request.StartTime.TimeOfDay < workStart || request.EndTime.TimeOfDay > workEnd)
        {
            throw new BookingValidationException("Bookings must be scheduled strictly within working hours (08:00 to 20:00).", "OUTSIDE_WORKING_HOURS");
        }

        // 4. Rule 4: Max Duration
        var duration = request.EndTime - request.StartTime;
        if (duration > TimeSpan.FromHours(4))
        {
            throw new BookingValidationException("Booking duration cannot exceed 4 hours.", "EXCEEDS_MAX_DURATION");
        }

        // 5. Ensure Room Exists
        var room = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == request.RoomId, ct);
        if (room == null)
        {
            throw new NotFoundException($"Room with ID {request.RoomId} was not found.", "ROOM_NOT_FOUND");
        }

        // 6. Rule 1: Overlap Check
        // Overlap condition: newStart < existingEnd && newEnd > existingStart
        var hasConflict = await _context.Bookings
            .AsNoTracking()
            .AnyAsync(b => b.RoomId == request.RoomId &&
                           request.StartTime < b.EndTime &&
                           request.EndTime > b.StartTime, ct);

        if (hasConflict)
        {
            throw new BookingConflictException(
                "The requested time slot overlaps with an existing booking in this room.",
                "BOOKING_CONFLICT"
            );
        }

        var booking = new Booking
        {
            RoomId = request.RoomId,
            Title = request.Title.Trim(),
            BookedBy = request.BookedBy.Trim(),
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            CreatedAt = _dateTimeProvider.UtcNow
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync(ct);

        return new BookingDto(
            booking.Id,
            booking.RoomId,
            room.Name,
            booking.Title,
            booking.BookedBy,
            booking.StartTime,
            booking.EndTime,
            booking.CreatedAt
        );
    }

    public async Task<bool> DeleteBookingAsync(int id, CancellationToken ct = default)
    {
        var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == id, ct);
        if (booking == null)
        {
            return false;
        }

        _context.Bookings.Remove(booking);
        await _context.SaveChangesAsync(ct);
        return true;
    }
}
