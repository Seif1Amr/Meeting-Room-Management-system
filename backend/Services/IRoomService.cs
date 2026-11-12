using MeetingRoomManagement.Api.DTOs;

namespace MeetingRoomManagement.Api.Services;

public interface IRoomService
{
    Task<IEnumerable<RoomDto>> GetAllRoomsAsync(CancellationToken ct = default);
    Task<RoomDto?> GetRoomByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<RoomDto>> GetAvailableRoomsAsync(DateOnly date, TimeOnly startTime, TimeOnly endTime, int? minCapacity, CancellationToken ct = default);
}
