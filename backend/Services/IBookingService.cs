using MeetingRoomManagement.Api.DTOs;

namespace MeetingRoomManagement.Api.Services;

public interface IBookingService
{
    Task<IEnumerable<BookingDto>> GetBookingsByRoomAndDateAsync(int roomId, DateOnly date, CancellationToken ct = default);
    Task<BookingDto> CreateBookingAsync(CreateBookingRequest request, CancellationToken ct = default);
    Task<bool> DeleteBookingAsync(int id, CancellationToken ct = default);
}
