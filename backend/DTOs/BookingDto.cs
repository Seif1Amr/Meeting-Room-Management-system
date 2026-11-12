namespace MeetingRoomManagement.Api.DTOs;

public record BookingDto(
    int Id,
    int RoomId,
    string RoomName,
    string Title,
    string BookedBy,
    DateTime StartTime,
    DateTime EndTime,
    DateTime CreatedAt
);
