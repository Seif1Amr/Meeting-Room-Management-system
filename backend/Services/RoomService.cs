using Microsoft.EntityFrameworkCore;
using MeetingRoomManagement.Api.Data;
using MeetingRoomManagement.Api.DTOs;
using MeetingRoomManagement.Api.Domain.Exceptions;

namespace MeetingRoomManagement.Api.Services;

public class RoomService : IRoomService
{
    private readonly AppDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;

    public RoomService(AppDbContext context, IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<IEnumerable<RoomDto>> GetAllRoomsAsync(CancellationToken ct = default)
    {
        return await _context.Rooms
            .AsNoTracking()
            .OrderBy(r => r.Floor)
            .ThenBy(r => r.Name)
            .Select(r => new RoomDto(r.Id, r.Name, r.Capacity, r.Floor))
            .ToListAsync(ct);
    }

    public async Task<RoomDto?> GetRoomByIdAsync(int id, CancellationToken ct = default)
    {
        var room = await _context.Rooms
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id, ct);

        return room == null ? null : new RoomDto(room.Id, room.Name, room.Capacity, room.Floor);
    }

    public async Task<IEnumerable<RoomDto>> GetAvailableRoomsAsync(
        DateOnly date,
        TimeOnly startTime,
        TimeOnly endTime,
        int? minCapacity,
        CancellationToken ct = default)
    {
        if (endTime <= startTime)
        {
            throw new BookingValidationException("End time must be strictly after start time.", "INVALID_TIME_RANGE");
        }

        if (startTime < new TimeOnly(8, 0) || endTime > new TimeOnly(20, 0))
        {
            throw new BookingValidationException("Requested slot must be within working hours (08:00 - 20:00).", "OUTSIDE_WORKING_HOURS");
        }

        if (endTime - startTime > TimeSpan.FromHours(4))
        {
            throw new BookingValidationException("Requested slot duration cannot exceed 4 hours.", "EXCEEDS_MAX_DURATION");
        }

        var slotStart = date.ToDateTime(startTime);
        var slotEnd = date.ToDateTime(endTime);

        var query = _context.Rooms
            .AsNoTracking()
            .Include(r => r.Bookings)
            .AsQueryable();

        if (minCapacity.HasValue && minCapacity.Value > 0)
        {
            query = query.Where(r => r.Capacity >= minCapacity.Value);
        }

        // Overlap rule: newStart < existingEnd && newEnd > existingStart
        var availableRooms = await query
            .Where(r => !r.Bookings.Any(b => slotStart < b.EndTime && slotEnd > b.StartTime))
            .OrderBy(r => r.Capacity)
            .ThenBy(r => r.Name)
            .Select(r => new RoomDto(r.Id, r.Name, r.Capacity, r.Floor))
            .ToListAsync(ct);

        return availableRooms;
    }
}
